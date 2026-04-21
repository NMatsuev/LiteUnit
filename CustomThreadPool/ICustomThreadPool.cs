using CustomThreadPool.Model;

namespace CustomThreadPool
{
    public interface ICustomThreadPool
    {
        public void EnqueueTask(Func<Task> task);
        public void Close();
        public ThreadPoolStatsModel GetStats();
        public IReadOnlyCollection<ThreadPoolErrorModel> GetErrors();
    }
}
