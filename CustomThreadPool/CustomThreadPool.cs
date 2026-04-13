using System.Collections.Concurrent;
using CustomThreadPool.Model;

namespace CustomThreadPool
{
    public class CustomThreadPool : ICustomThreadPool, IDisposable
    {
        private readonly int _minThreads;
        private readonly int _maxThreads;
        private readonly int _queueTimeoutThresholdMs;
        private readonly TimeSpan _threadIdleTimeout;

        private readonly Queue<Action> _tasks;
        private readonly List<Thread> _threads;
        private readonly object _syncLock = new object();

        private bool _isDisposed;
        private int _activeThreads;
        private int _waitingThreads;
        private DateTime _lastTaskEnqueuedTime;

        private readonly Dictionary<Thread, DateTime> _threadLastActivity;
        private readonly ConcurrentQueue<Exception> _errorLog;
        private Timer _healthCheckTimer;
        private Thread _monitorThread;

        public CustomThreadPool(
            int minThreads = 2,
            int maxThreads = 10,
            int queueTimeoutThresholdMs = 5000,
            int threadIdleTimeoutSeconds = 30)
        {
            if (minThreads < 1) throw new ArgumentException("Min threads must be at least 1");
            if (maxThreads < minThreads) throw new ArgumentException("Max threads must be >= min threads");
            if (queueTimeoutThresholdMs <= 0) throw new ArgumentException("Queue timeout threshold must be positive");

            _minThreads = minThreads;
            _maxThreads = maxThreads;
            _queueTimeoutThresholdMs = queueTimeoutThresholdMs;
            _threadIdleTimeout = TimeSpan.FromSeconds(threadIdleTimeoutSeconds);

            _tasks = new Queue<Action>();
            _threads = new List<Thread>();
            _activeThreads = 0;
            _waitingThreads = 0;
            _lastTaskEnqueuedTime = DateTime.UtcNow;
            _threadLastActivity = new Dictionary<Thread, DateTime>();
            _errorLog = new ConcurrentQueue<Exception>();

            for (int i = 0; i < _minThreads; i++)
            {
                CreateAndStartThread();
            }

            StartMonitoring();

            StartHealthCheck();
        }

        public void EnqueueTask(Action task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (_isDisposed) throw new ObjectDisposedException(nameof(CustomThreadPool));

            lock (_syncLock)
            {
                _tasks.Enqueue(task);
                _lastTaskEnqueuedTime = DateTime.UtcNow;

                TryScaleUp();

                Monitor.Pulse(_syncLock);
            }
        }

        public void Close()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            _healthCheckTimer?.Dispose();

            lock (_syncLock)
            {
                for (int i = 0; i < _threads.Count; i++)
                {
                    _tasks.Enqueue(null);
                }
                Monitor.PulseAll(_syncLock);
            }

            foreach (Thread thread in _threads.ToList())
            {
                if (thread.IsAlive)
                    thread.Join(TimeSpan.FromSeconds(5));
            }

            _threads.Clear();
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        public ThreadPoolStatsModel GetStats()
        {
            lock (_syncLock)
            {
                return new ThreadPoolStatsModel
                {
                    TotalThreads = _threads.Count,
                    ActiveThreads = _activeThreads,
                    WaitingThreads = _waitingThreads,
                    QueuedTasks = _tasks.Count,
                    MinThreads = _minThreads,
                    MaxThreads = _maxThreads
                };
            }
        }

        public IReadOnlyCollection<Exception> GetErrors()
        {
            return _errorLog.ToList().AsReadOnly();
        }

        private void CreateAndStartThread()
        {
            var thread = new Thread(DoThreadWork)
            {
                IsBackground = true,
                Name = $"ThreadPool-{_threads.Count + 1}"
            };

            _threads.Add(thread);
            _threadLastActivity[thread] = DateTime.UtcNow;
            thread.Start();
        }

        private void TryScaleUp()
        {
            int queuedTasks = _tasks.Count;
            int currentThreads = _threads.Count;
            int busyThreads = _activeThreads;

            bool shouldScaleUp = false;

            // Условия для масштабирования:
            // 1. Очередь превышает количество активных потоков в 2 раза
            if (queuedTasks > busyThreads * 2 && currentThreads < _maxThreads)
            {
                shouldScaleUp = true;
            }
            // 2. В очереди есть задачи, но нет активных потоков
            else if (queuedTasks > 0 && busyThreads == 0 && currentThreads < _maxThreads)
            {
                shouldScaleUp = true;
            }
            // 3. Очередь превышает максимальное количество потоков
            else if (queuedTasks > currentThreads && currentThreads < _maxThreads)
            {
                shouldScaleUp = true;
            }

            if (shouldScaleUp)
            {
                CreateAndStartThread();
            }
        }

        private void TryScaleDown()
        {
            lock (_syncLock)
            {
                int currentThreads = _threads.Count;

                // Не уменьшаем ниже минимального порога
                if (currentThreads <= _minThreads)
                    return;

                // Проверяем, сколько потоков простаивают
                int idleThreads = _waitingThreads;

                // Если более 50% потоков простаивают, удаляем лишние
                if (idleThreads > currentThreads / 2 && currentThreads > _minThreads)
                {
                    int threadsToRemove = Math.Min(
                        idleThreads / 2,
                        currentThreads - _minThreads
                    );

                    for (int i = 0; i < threadsToRemove; i++)
                    {
                        _tasks.Enqueue(null);
                    }
                    Monitor.PulseAll(_syncLock);
                }
            }
        }

        private void DoThreadWork()
        {
            while (!_isDisposed)
            {
                Action task = null;
                bool shouldExit = false;

                lock (_syncLock)
                {
                    _threadLastActivity[Thread.CurrentThread] = DateTime.UtcNow;

                    if (_tasks.Count == 0)
                    {
                        _waitingThreads++;

                        shouldExit = !Monitor.Wait(_syncLock, _threadIdleTimeout);

                        _waitingThreads--;

                        if (shouldExit && _threads.Count > _minThreads)
                            break;

                        if (_tasks.Count == 0) continue;
                    }

                    task = _tasks.Dequeue();
                    _threadLastActivity[Thread.CurrentThread] = DateTime.UtcNow;
                }

                if (task != null)
                {
                    Interlocked.Increment(ref _activeThreads);

                    try
                    {
                        task.Invoke();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, Thread.CurrentThread);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeThreads);
                    }
                }
            }

            // Выход из потока
            RemoveDeadThread(Thread.CurrentThread);
        }

        private void RemoveDeadThread(Thread thread)
        {
            lock (_syncLock)
            {
                if (_threads.Contains(thread))
                {
                    _threads.Remove(thread);
                    _threadLastActivity.Remove(thread);
                    Console.WriteLine($"[ThreadPool] Thread {thread.Name} terminated. Remaining threads: {_threads.Count}");
                }
            }

            EnsureMinThreads();
        }

        private void EnsureMinThreads()
        {
            lock (_syncLock)
            {
                while (_threads.Count < _minThreads && !_isDisposed)
                {
                    CreateAndStartThread();
                    Console.WriteLine($"[ThreadPool] Creating replacement thread. Total threads: {_threads.Count}");
                }
            }
        }

        private void StartHealthCheck()
        {
            _healthCheckTimer = new Timer(_ =>
            {
                if (_isDisposed) return;

                List<Thread> hangedThreads = new List<Thread>();

                lock (_syncLock)
                {
                    var now = DateTime.UtcNow;
                    hangedThreads = _threads
                        .Where(t => t.IsAlive &&
                               now - _threadLastActivity.GetValueOrDefault(t, now) > TimeSpan.FromMinutes(2))
                        .ToList();

                    foreach (var thread in hangedThreads)
                    {
                        try
                        {
                            thread.Interrupt();
                            Console.WriteLine($"[HealthCheck] Interrupted hanged thread: {thread.Name}");
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, thread);
                        }

                        LogError(new TimeoutException($"Thread {thread.Name} hanged for more than 2 minutes"), thread);

                        if (_threads.Contains(thread))
                        {
                            _threads.Remove(thread);
                            _threadLastActivity.Remove(thread);
                        }

                        CreateAndStartThread();
                        Console.WriteLine($"[HealthCheck] Replaced hanged thread {thread.Name} with new thread");
                    }
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        private void LogError(Exception ex, Thread thread)
        {
            var error = new Exception($"Thread {thread?.Name ?? "Unknown"}: {ex.Message}", ex);
            _errorLog.Enqueue(error);
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {error}");
        }

        private void StartMonitoring()
        {
            _monitorThread = new Thread(() =>
            {
                while (!_isDisposed)
                {
                    Thread.Sleep(1000);

                    if (_isDisposed) break;

                    lock (_syncLock)
                    {
                        if (_tasks.Count > 0)
                        {
                            TimeSpan queueWaitTime = DateTime.UtcNow - _lastTaskEnqueuedTime;

                            if (queueWaitTime.TotalMilliseconds > _queueTimeoutThresholdMs &&
                                _threads.Count < _maxThreads)
                            {
                                int neededThreads = Math.Min(
                                    _maxThreads - _threads.Count,
                                    Math.Min(_tasks.Count, _maxThreads - _threads.Count)
                                );

                                for (int i = 0; i < neededThreads; i++)
                                {
                                    CreateAndStartThread();
                                    Console.WriteLine($"[Monitor] Scaling up due to queue timeout. Total threads: {_threads.Count}");
                                }
                            }
                        }

                        TryScaleDown();
                    }
                }
            })
            {
                IsBackground = true,
                Name = "ThreadPoolMonitor"
            };

            _monitorThread.Start();
        }


        public Task EnqueueTaskAsync(Func<Task> asyncTask)
        {
            var tcs = new TaskCompletionSource<bool>();

            EnqueueTask(() =>
            {
                try
                {
                    asyncTask().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            tcs.SetException(t.Exception ?? new Exception("Task failed"));
                        else if (t.IsCanceled)
                            tcs.SetCanceled();
                        else
                            tcs.SetResult(true);
                    });
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<T> EnqueueTaskAsync<T>(Func<Task<T>> asyncTask)
        {
            var tcs = new TaskCompletionSource<T>();

            EnqueueTask(() =>
            {
                try
                {
                    asyncTask().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            tcs.SetException(t.Exception ?? new Exception("Task failed"));
                        else if (t.IsCanceled)
                            tcs.SetCanceled();
                        else
                            tcs.SetResult(t.Result);
                    });
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}