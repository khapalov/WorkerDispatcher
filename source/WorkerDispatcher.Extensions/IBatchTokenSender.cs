namespace WorkerDispatcher.Batch
{
    public interface IBatchTokenSender
    {
        void Send<TData>(TData data);

        void Flush<TData>();
    }
}
