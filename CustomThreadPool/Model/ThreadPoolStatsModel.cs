namespace CustomThreadPool.Model
{
    public class ThreadPoolStatsModel
    {
        public int TotalThreads { get; set; }
        public int QueuedTasks { get; set; }
        public int MinThreads { get; set; }
        public int MaxThreads { get; set; }

        public override string ToString()
        {
            return $"Threads: {TotalThreads}" +
                   $"Queue: {QueuedTasks}, Limits: [{MinThreads}-{MaxThreads}]";
        }
    }
}
