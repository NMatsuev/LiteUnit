using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using CustomThreadPool.Model;
using TestsRunner.Helpers;
using TestsRunner.Models;
using TestsRunner.Models.Enums;
using TestsRunner.Models.Filters;
using TestsRunner.Services;
using TestsRunner.ViewModels.Abstractions;

namespace TestsRunner.ViewModels.Implementations
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private TreeViewItemBase _selectedItem;
        private int _totalTests;
        private int _passedTests;
        private int _failedTests;
        private int _skippedTests;
        private int _selectedCount;
        private int _minParallelism = 1;
        private int _maxParallelism = 1; //По умолчанию 1 (без параллелизма)
        private bool _isParallelExecutionEnabled;
        private TimeSpan _totalExecutionTime;
        private bool _isRunning;

        private ThreadPoolStatsModel _threadPoolStats;
        private ObservableCollection<ThreadPoolErrorModel> _threadPoolErrors;
        private ThreadPoolErrorModel _selectedThreadPoolError;
        private CustomThreadPool.CustomThreadPool _currentThreadPool;

        public FiltersViewModel FiltersViewModel { get; } = new();

        #endregion

        #region Properties
        public ObservableCollection<AssemblyViewModel> TestAssemblies { get; }

        public ObservableCollection<AssemblyViewModel> FilteredTestAssemblies => GetFilteredTestAssemblies();

        public TreeViewItemBase SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    UpdateSelectedCount();
                }
            }
        }

        public int TotalTests
        {
            get => _totalTests;
            private set => SetProperty(ref _totalTests, value);
        }

        public int PassedTests
        {
            get => _passedTests;
            private set => SetProperty(ref _passedTests, value);
        }

        public int FailedTests
        {
            get => _failedTests;
            private set => SetProperty(ref _failedTests, value);
        }

        public int SkippedTests
        {
            get => _skippedTests;
            private set => SetProperty(ref _skippedTests, value);
        }

        public int SelectedCount
        {
            get => _selectedCount;
            private set => SetProperty(ref _selectedCount, value);
        }

        public string SelectedCountText => $"Выбрано: {SelectedCount} {GetTestWord(SelectedCount)}";

        public int MaxParallelism
        {
            get => _maxParallelism;
            set
            {
                if (SetProperty(ref _maxParallelism, Math.Max(1, value))) //Минимум 1
                {
                    UpdateCanExecuteCommands();
                }
            }
        }

        public int MinParallelism
        {
            get => _minParallelism;
            set
            {
                if (SetProperty(ref _minParallelism, Math.Max(1, value))) //Минимум 1
                {
                    UpdateCanExecuteCommands();
                }
            }
        }

        public bool IsParallelExecutionEnabled
        {
            get => _isParallelExecutionEnabled;
            set
            {
                if (SetProperty(ref _isParallelExecutionEnabled, value))
                {
                    UpdateCanExecuteCommands();
                }
            }
        }

        public TimeSpan TotalExecutionTime
        {
            get => _totalExecutionTime;
            private set => SetProperty(ref _totalExecutionTime, value);
        }

        public string TotalExecutionTimeDisplay
        {
            get
            {
                if (_totalExecutionTime.TotalMilliseconds < 1000)
                    return $"{_totalExecutionTime.TotalMilliseconds:F0} мс";
                else if (_totalExecutionTime.TotalSeconds < 60)
                    return $"{_totalExecutionTime.TotalSeconds:F2} с";
                else
                    return $"{_totalExecutionTime.TotalMinutes:F2} мин";
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        // Новые свойства для пула потоков
        public ThreadPoolStatsModel ThreadPoolStats
        {
            get => _threadPoolStats;
            private set
            {
                SetProperty(ref _threadPoolStats, value);
            }
        }

        public ObservableCollection<ThreadPoolErrorModel> ThreadPoolErrors
        {
            get => _threadPoolErrors;
            set => SetProperty(ref _threadPoolErrors, value);
        }

        public ThreadPoolErrorModel SelectedThreadPoolError
        {
            get => _selectedThreadPoolError;
            set
            {
                if (SetProperty(ref _selectedThreadPoolError, value) && value != null)
                {
                    ShowErrorDetails(value);
                }
            }
        }

        public ObservableCollection<AssemblyViewModel> CurrentTestAssemblies
        {
            get { return FiltersViewModel.IsFiltersEnabled ? FilteredTestAssemblies : TestAssemblies; }
        }

        #endregion

        #region Commands
        public RelayCommand LoadAssemblyCommand { get; }
        public RelayCommand DeleteAssemblyCommand { get; }
        public RelayCommand RunSelectedTestsCommand { get; }
        public RelayCommand IncreaseMinParallelismCommand { get; }
        public RelayCommand DecreaseMinParallelismCommand { get; }
        public RelayCommand IncreaseMaxParallelismCommand { get; }
        public RelayCommand DecreaseMaxParallelismCommand { get; }
        public RelayCommand ToggleParallelExecutionCommand { get; }
        public RelayCommand ClearThreadPoolErrorsCommand { get; }


        #endregion

        #region Constructors

        public MainViewModel() : this(new DialogService()) { }

        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            TestAssemblies = new ObservableCollection<AssemblyViewModel>();
            TestAssemblies.CollectionChanged += TestAssemblies_CollectionChanged;
            ThreadPoolErrors = new ObservableCollection<ThreadPoolErrorModel>();

            // Подписываемся на изменения
            FiltersViewModel.PropertyChanged += FiltersViewModel_PropertyChanged;

            // Подписываемся на изменения фильтров (SelectedValue)
            foreach (var filter in FiltersViewModel.Filters)
            {
                filter.PropertyChanged += Filter_PropertyChanged;
            }

            //Инициализация команд
            LoadAssemblyCommand = new RelayCommand(async () => {
                await LoadAssemblyAsync();
                FiltersViewModel.UpdateFiltersPossibleValues(TestAssemblies.Select(ta => ta.Assembly));
                OnPropertyChanged(nameof(CurrentTestAssemblies));
            }
            );
            DeleteAssemblyCommand = new RelayCommand(() => { 
                DeleteAssembly(); 
                FiltersViewModel.UpdateFiltersPossibleValues(TestAssemblies.Select(ta => ta.Assembly));
                OnPropertyChanged(nameof(CurrentTestAssemblies));
            }, () => SelectedItem is AssemblyViewModel);
            RunSelectedTestsCommand = new RelayCommand(async () => await RunSelectedTestsAsync(), () => SelectedItem != null);
            IncreaseMinParallelismCommand = new RelayCommand(() => MinParallelism = Math.Min(MaxParallelism, MinParallelism + 1));
            DecreaseMinParallelismCommand = new RelayCommand(() => MinParallelism = Math.Max(1, MinParallelism - 1));
            IncreaseMaxParallelismCommand = new RelayCommand(() => MaxParallelism++);
            DecreaseMaxParallelismCommand = new RelayCommand(() => MaxParallelism = Math.Max(MinParallelism, MaxParallelism - 1));
            ToggleParallelExecutionCommand = new RelayCommand(() => IsParallelExecutionEnabled = !IsParallelExecutionEnabled);
            ClearThreadPoolErrorsCommand = new RelayCommand(ClearThreadPoolErrors);
        }

        private void FiltersViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FiltersViewModel.IsFiltersEnabled))
            {
                OnPropertyChanged(nameof(CurrentTestAssemblies));

                if (FiltersViewModel.IsFiltersEnabled)
                {
                    foreach (var testAssemblyModel in TestAssemblies)
                        foreach (var testClassModel in testAssemblyModel.Assembly.Classes)
                        {
                            testClassModel.IsRunnable = true;
                            foreach (var testMethodModel in testClassModel.Methods)
                                testMethodModel.IsRunnable = true;
                        }
                }
            }
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterBase.SelectedValue))
            {
                OnPropertyChanged(nameof(CurrentTestAssemblies));
            }
        }

        #endregion

        #region Thread Pool Management

        private void OnThreadPoolStatsUpdated(object sender, ThreadPoolStatsModel stats)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ThreadPoolStats = stats;
            });
        }

        private void OnThreadPoolErrorOccurred(object sender, ThreadPoolErrorModel error)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ThreadPoolErrors.Add(error);
            });
        }

        private void ClearThreadPoolErrors()
        {
            ThreadPoolErrors.Clear();
        }

        private void ShowErrorDetails(ThreadPoolErrorModel error)
        {
            _dialogService.ShowMessage(
                $"Ошибка: {error.Message}\n\nВремя: {error.Timestamp:HH:mm:ss}\nПоток: {error.ThreadName}",
                "Детали ошибки пула потоков",
                MessageBoxImage.Error);
        }

        #endregion

        #region Load/delete assembly
        private async Task LoadAssemblyAsync()
        {
            var filePath = _dialogService.OpenFileDialog("Assembly files (*.dll;*.exe)|*.dll;*.exe", "Выберите тестовую сборку");
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                var assemblyModel = await Task.Run(() => TestLoaderService.LoadAssembly(filePath));
                var assemblyVM = new AssemblyViewModel(assemblyModel);

                foreach (var classModel in assemblyModel.Classes)
                {
                    AddClassToTree(assemblyVM, classModel);
                }

                TestAssemblies.Add(assemblyVM);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки сборки: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void AddClassToTree(TreeViewItemBase parentVM, TestClassModel classModel)
        {
            var classVM = new ClassViewModel(classModel);

            foreach (var methodModel in classModel.Methods)
            {
                classVM.AddChild(new MethodViewModel(methodModel));
            }

            foreach (var nestedClass in classModel.NestedClasses)
            {
                AddClassToTree(classVM, nestedClass);
            }

            parentVM.AddChild(classVM);
        }

        private void DeleteAssembly()
        {
            if (SelectedItem is AssemblyViewModel assemblyVM)
            {
                TestAssemblies.Remove(TestAssemblies.FirstOrDefault(el => el.Assembly == assemblyVM.Assembly));
                FilteredTestAssemblies.Remove(FilteredTestAssemblies.FirstOrDefault(el => el.Assembly == assemblyVM.Assembly));
                SelectedItem = null;
            }
        }

        #endregion

        #region Run

        private async Task RunSelectedTestsAsync()
        {
            if (SelectedItem == null)
            {
                _dialogService.ShowMessage("Выберите тест для запуска", "Информация", MessageBoxImage.Information);
                return;
            }

            try
            {
                //Определяем тип выбранного элемента
                switch (SelectedItem)
                {
                    case AssemblyViewModel assemblyVM:
                        await RunTestsInternalAsync(assemblyVM, RunAssemblyTestsAsync);
                        break;

                    case ClassViewModel classVM:
                        await RunTestsInternalAsync(classVM, RunClassTestsAsync);
                        break;

                    case MethodViewModel methodVM:
                        await RunTestsInternalAsync(methodVM, RunSingleTestAsync);
                        break;
                    default:
                        _dialogService.ShowMessage("Выбран неподдерживаемый тип элемента", "Ошибка", MessageBoxImage.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка выполнения тестов: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        //Обобщенный метод для запуска тестов с учетом параллелизма
        private async Task RunTestsInternalAsync<T>(T item, Func<T, Task> runTestsFunc) where T : TreeViewItemBase
        {
            IsRunning = true;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (IsParallelExecutionEnabled && MaxParallelism > 1)
                {
                    await RunTestsParallelAsync(item);
                }
                else
                {
                    await RunTestsSequentialAsync(item, runTestsFunc);
                }
            }
            finally
            {
                stopwatch.Stop();
                TotalExecutionTime = stopwatch.Elapsed;
                IsRunning = false;
                _currentThreadPool = null;

                // Обновляем отображение времени в UI
                OnPropertyChanged(nameof(TotalExecutionTimeDisplay));
            }
        }

        private async Task RunTestsSequentialAsync<T>(T item, Func<T, Task> runTestsFunc) where T : TreeViewItemBase
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SetRunningStatus(item);
                    UpdateParentStatuses(item);
                    UpdateOverallStatistics();
                });

                await runTestsFunc(item);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdateFromModel(item);
                    UpdateParentStatuses(item);
                    UpdateOverallStatistics();
                });
            });
        }

        private async Task RunTestsParallelAsync<T>(T item) where T : TreeViewItemBase
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SetRunningStatus(item);
                    UpdateParentStatuses(item);
                    UpdateOverallStatistics();
                });

                //Собираем все тесты для параллельного запуска
                var testMethods = new List<(MethodViewModel method, ClassViewModel classVM)>();
                CollectTestMethodsForParallel(item, testMethods);

                testMethods = testMethods.Where(t => t.method.Method.IsRunnable == true).ToList();

                if (testMethods.Count == 0)
                    return;

                _currentThreadPool = new CustomThreadPool.CustomThreadPool(
                                        minThreads: MinParallelism,
                                        maxThreads: MaxParallelism);

                _currentThreadPool.StatsUpdated += OnThreadPoolStatsUpdated;
                _currentThreadPool.ErrorOccurred += OnThreadPoolErrorOccurred;

                using (_currentThreadPool)
                {
                    int remainingTasks = testMethods.Count;
                    var completionEvent = new ManualResetEvent(false);
                    object lockObj = new();
                    Thread.Sleep(5000);
                    int counter = 0;
                    foreach (var (method, classVM) in testMethods)
                    {
                        if (counter > 40)
                            Thread.Sleep(2000);
                        _currentThreadPool.EnqueueTask(async () =>
                        {
                            try
                            {
                                if (method.IsParameterized)
                                {
                                    foreach (var testCase in method.Children.OfType<TestCaseViewModel>())
                                    {
                                        await RunSingleTestCaseAsync(testCase, method, classVM);
                                    }
                                }
                                else
                                {
                                    await RunSingleTestMethodAsync(method, classVM);
                                }
                            }
                            finally
                            {
                                lock (lockObj)
                                {
                                    remainingTasks--;
                                    if (remainingTasks == 0)
                                    {
                                        completionEvent.Set();
                                    }
                                }
                            }
                        });
                        counter++;
                    }

                    _currentThreadPool.EnqueueTask(() =>
                    { throw new Exception("Проверка ошибки"); });

                    // Ожидаем завершения
                    completionEvent.WaitOne();
                    completionEvent.Dispose();
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdateFromModel(item);
                    UpdateParentStatuses(item);
                    UpdateOverallStatistics();
                    ClearThreadPoolStatistics();
                });
            });
        }

        private ObservableCollection<AssemblyViewModel> GetFilteredTestAssemblies()
        {
            ObservableCollection<AssemblyViewModel> assemblyViewModels = new ObservableCollection<AssemblyViewModel>();
            foreach(var assemblyVM in TestAssemblies)
            {
                var filtAssembly = new AssemblyViewModel(assemblyVM.Assembly);

                foreach (var classM in filtAssembly.Assembly.Classes)
                {
                    classM.IsRunnable = false;

                    foreach (var item in classM.Methods)
                        item.IsRunnable = false;

                    var filtClass = new ClassViewModel(classM);
                    
                    var filtMathods = filtClass.Class.Methods.Where(m => FiltersViewModel.Filters.All(fb => fb.Filter(m)));

                    foreach (var method in filtMathods)
                    {
                        method.IsRunnable = true;
                        filtClass.AddChild(new MethodViewModel(method));
                    }

                    if (filtClass.Children.Any())
                    {
                        filtClass.Class.IsRunnable = true;
                        filtAssembly.AddChild(filtClass);
                    }
                }

                if (filtAssembly.Children.Any())
                {
                    assemblyViewModels.Add(filtAssembly);
                }
            }
            return assemblyViewModels;
        }

        private void CollectTestMethodsForParallel(TreeViewItemBase item, List<(MethodViewModel, ClassViewModel)> methods)
        {
            switch (item)
            {
                case MethodViewModel methodVM:
                    var parentClass = FindParentClass(methodVM);
                    if (parentClass != null)
                        methods.Add((methodVM, parentClass));
                    break;

                case ClassViewModel classVM:
                    foreach (var child in classVM.Children.OfType<MethodViewModel>())
                    {
                        methods.Add((child, classVM));
                    }
                    foreach (var nestedClass in classVM.Children.OfType<ClassViewModel>())
                    {
                        CollectTestMethodsForParallel(nestedClass, methods);
                    }
                    break;

                case AssemblyViewModel assemblyVM:
                    foreach (var child in assemblyVM.Children)
                    {
                        CollectTestMethodsForParallel(child, methods);
                    }
                    break;
            }
        }

        private async Task RunSingleTestMethodAsync(MethodViewModel methodVM, ClassViewModel classVM)
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    methodVM.Status = TestStatus.Running;
                    UpdateClassStatus(classVM);
                });

                await TestRunnerService.RunTestAsync(methodVM.Method, classVM.Class);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    methodVM.UpdateFromModel();
                    UpdateClassStatus(classVM);
                });
            });
        }

        private async Task RunSingleTestCaseAsync(TestCaseViewModel testCaseVM, MethodViewModel methodVM, ClassViewModel classVM)
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    testCaseVM.Status = TestStatus.Running;
                });

                await TestRunnerService.RunTestCaseAsync(
                    testCaseVM.TestCase,
                    methodVM.Method.MethodInfo,
                    classVM.Class);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    testCaseVM.UpdateFromModel();
                });
            });
        }

        //Вспомогательные методы для запуска конкретных тестов
        private async Task RunAssemblyTestsAsync(AssemblyViewModel assemblyVM)
        {
            await TestRunnerService.RunAssemblyTestsAsync(assemblyVM.Assembly);
        }

        private async Task RunClassTestsAsync(ClassViewModel classVM)
        {
            await TestRunnerService.RunClassTestsAsync(classVM.Class);
        }

        private async Task RunSingleTestAsync(MethodViewModel methodVM)
        {
            var classVM = FindParentClass(methodVM);
            if (classVM == null)
            {
                _dialogService.ShowMessage("Не удалось найти родительский класс для метода", "Ошибка", MessageBoxImage.Error);
                return;
            }

            if (methodVM.IsParameterized)
            {
                await RunParameterizedMethodAsync(methodVM, classVM);
            }
            else
            {
                await RunOrdinaryMethodAsync(methodVM, classVM);
            }
        }

        private async Task RunParameterizedMethodAsync(MethodViewModel methodVM, ClassViewModel classVM)
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    methodVM.Status = TestStatus.Running;
                    foreach (var testCaseVM in methodVM.Children.OfType<TestCaseViewModel>())
                    {
                        testCaseVM.Status = TestStatus.Running;
                    }
                    UpdateClassStatus(classVM);
                });

                foreach (var testCaseVM in methodVM.Children.OfType<TestCaseViewModel>())
                {
                    await TestRunnerService.RunTestCaseAsync(
                        testCaseVM.TestCase,
                        methodVM.Method.MethodInfo,
                        classVM.Class);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var testCaseVM in methodVM.Children.OfType<TestCaseViewModel>())
                    {
                        testCaseVM.UpdateFromModel();
                    }
                    methodVM.UpdateFromModel();
                    UpdateClassStatus(classVM);
                    if (classVM.Parent is AssemblyViewModel assemblyVM)
                    {
                        UpdateAssemblyStatus(assemblyVM);
                    }
                    UpdateOverallStatistics();
                });
            });
        }

        private async Task RunOrdinaryMethodAsync(MethodViewModel methodVM, ClassViewModel classVM)
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    methodVM.Status = TestStatus.Running;
                    UpdateClassStatus(classVM);
                });

                await TestRunnerService.RunTestAsync(methodVM.Method, classVM.Class);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    methodVM.UpdateFromModel();
                    UpdateClassStatus(classVM);
                    if (classVM.Parent is AssemblyViewModel assemblyVM)
                    {
                        UpdateAssemblyStatus(assemblyVM);
                    }
                    UpdateOverallStatistics();
                });
            });
        }

        #endregion Run

        #region UI Update Methods

        private void SetRunningStatus(TreeViewItemBase item)
        {
            switch (item)
            {
                case TestCaseViewModel testCaseVM:
                    {
                        if (testCaseVM.Parent is MethodViewModel method && method.Method.IsRunnable)
                        {
                            testCaseVM.Status = TestStatus.Running;
                        }
                        break;
                    }

                case MethodViewModel methodVM:
                    {
                        if (methodVM.Method.IsRunnable)
                        {
                            if (methodVM.IsParameterized)
                            {
                                methodVM.Status = TestStatus.Running;
                                foreach (var child in methodVM.Children.OfType<TestCaseViewModel>())
                                {
                                    child.Status = TestStatus.Running;
                                }
                            }
                            else
                            {
                                methodVM.Status = TestStatus.Running;
                            }
                        }
                        break;
                    }
                        

                case ClassViewModel classVM:
                    {
                        if (classVM.Class.IsRunnable)
                        {
                            foreach (var method in classVM.Children.OfType<MethodViewModel>())
                            {
                                SetRunningStatus(method);
                            }
                        }
                        break;
                    }

                case AssemblyViewModel assemblyVM:
                    foreach (var method in GetAllMethods(assemblyVM))
                    {
                        if (method.Method.IsRunnable)
                            method.Status = TestStatus.Running;
                    }
                    break;
            }
        }

        private void UpdateFromModel(TreeViewItemBase item)
        {
            switch (item)
            {
                case TestCaseViewModel testCaseVM:
                    testCaseVM.UpdateFromModel();
                    break;

                case MethodViewModel methodVM:
                    methodVM.UpdateFromModel();
                    break;

                case ClassViewModel classVM:
                    foreach (var method in classVM.Children.OfType<MethodViewModel>())
                    {
                        method.UpdateFromModel();
                    }
                    break;

                case AssemblyViewModel assemblyVM:
                    foreach (var method in GetAllMethods(assemblyVM))
                    {
                        method.UpdateFromModel();
                    }
                    break;
            }
        }

        private void UpdateParentStatuses(TreeViewItemBase item)
        {
            switch (item)
            {
                case MethodViewModel methodVM:
                    var parentClass = FindParentClass(methodVM);
                    if (parentClass != null)
                    {
                        UpdateClassStatus(parentClass);
                        if (parentClass.Parent is AssemblyViewModel assemblyVM)
                        {
                            UpdateAssemblyStatus(assemblyVM);
                        }
                    }
                    break;

                case ClassViewModel classVM:
                    UpdateClassStatus(classVM);
                    if (classVM.Parent is AssemblyViewModel parentAssembly)
                    {
                        UpdateAssemblyStatus(parentAssembly);
                    }
                    break;

                case AssemblyViewModel assemblyVM:
                    foreach (var classVM in assemblyVM.Children.OfType<ClassViewModel>())
                        UpdateClassStatus(classVM);
                    UpdateAssemblyStatus(assemblyVM);
                    break;
            }
        }

        private ClassViewModel FindParentClass(TreeViewItemBase item)
        {
            var current = item.Parent;
            while (current != null)
            {
                if (current is ClassViewModel classVM)
                    return classVM;
                current = current.Parent;
            }
            return null;
        }

        private void UpdateClassStatus(ClassViewModel classVM)
        {
            var methods = classVM.Children.OfType<MethodViewModel>().ToList();

            if (methods.All(m => m.Status == TestStatus.Passed))
                classVM.Status = TestStatus.Passed;
            else if (methods.Any(m => m.Status == TestStatus.Failed))
                classVM.Status = TestStatus.Failed;
            else if (methods.Any(m => m.Status == TestStatus.Running))
                classVM.Status = TestStatus.Running;
            else
                classVM.Status = TestStatus.None;
        }

        private void UpdateAssemblyStatus(AssemblyViewModel assemblyVM)
        {
            var classes = assemblyVM.Children.OfType<ClassViewModel>().ToList();

            if (classes.All(c => c.Status == TestStatus.Passed))
                assemblyVM.Status = TestStatus.Passed;
            else if (classes.Any(c => c.Status == TestStatus.Failed))
                assemblyVM.Status = TestStatus.Failed;
            else if (classes.Any(c => c.Status == TestStatus.Running))
                assemblyVM.Status = TestStatus.Running;
            else
                assemblyVM.Status = TestStatus.None;
        }

        private void UpdateOverallStatistics()
        {
            var allMethods = CurrentTestAssemblies
                .OfType<AssemblyViewModel>()
                .SelectMany(a => GetAllMethods(a))
                .ToList();

            TotalTests = allMethods.Count;
            PassedTests = allMethods.Count(m => m.Status == TestStatus.Passed);
            FailedTests = allMethods.Count(m => m.Status == TestStatus.Failed);
            SkippedTests = allMethods.Count(m => m.Status == TestStatus.None || m.Status == TestStatus.Running);
        }

        private void ClearThreadPoolStatistics() 
        {
            ThreadPoolStats = new ThreadPoolStatsModel
            {
                TotalThreads = 0,
                QueuedTasks = 0
            };
        }

        public static IEnumerable<MethodViewModel> GetAllMethods(TreeViewItemBase item)
        {
            switch (item)
            {
                case MethodViewModel method:
                    yield return method;
                    break;

                case ClassViewModel classVM:
                    foreach (var child in classVM.Children)
                    {
                        if (child is MethodViewModel methodChild)
                            yield return methodChild;
                        else if (child is ClassViewModel nestedClass)
                            foreach (var m in GetAllMethods(nestedClass))
                                yield return m;
                    }
                    break;

                case AssemblyViewModel assembly:
                    foreach (var child in assembly.Children)
                        foreach (var m in GetAllMethods(child))
                            yield return m;
                    break;
            }
        }

        private void UpdateSelectedCount()
        {
            if (SelectedItem == null)
            {
                SelectedCount = 0;
                OnPropertyChanged(nameof(SelectedCountText));
                return;
            }

            var methods = new List<MethodViewModel>();

            void Collect(TreeViewItemBase current)
            {
                switch (current)
                {
                    case MethodViewModel methodItem:
                        methods.Add(methodItem);
                        break;
                    case ClassViewModel classItem:
                    case AssemblyViewModel assemblyItem:
                        foreach (var child in current.Children)
                            Collect(child);
                        break;
                }
            }

            Collect(SelectedItem);
            SelectedCount = methods.Count;
            OnPropertyChanged(nameof(SelectedCountText));
        }

        private void UpdateCanExecuteCommands()
        {
            DeleteAssemblyCommand.RaiseCanExecuteChanged();
            RunSelectedTestsCommand.RaiseCanExecuteChanged();
        }

        private void TestAssemblies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateOverallStatistics();
        }

        private static string GetTestWord(int count)
        {
            if (count % 10 == 1 && count % 100 != 11) return "тест";
            if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20)) return "теста";
            return "тестов";
        }

        #endregion
    }
}