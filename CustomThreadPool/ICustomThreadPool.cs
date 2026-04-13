namespace CustomThreadPool
{
    public interface ICustomThreadPool
    {
        public void EnqueueTask(Action task);
        public void Close();
    }
}
