namespace CustomThreadPool.Model
{
    public class ThreadPoolStatsModel
    {
        public int TotalThreads { get; set; }
        public int ActiveThreads { get; set; }
        public int WaitingThreads { get; set; }
        public int QueuedTasks { get; set; }
        public int MinThreads { get; set; }
        public int MaxThreads { get; set; }

        public override string ToString()
        {
            return $"Threads: {TotalThreads} (Active: {ActiveThreads}, Waiting: {WaitingThreads}), " +
                   $"Queue: {QueuedTasks}, Limits: [{MinThreads}-{MaxThreads}]";
        }
    }
}
