namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchTokenSender
    {
        void Send<TData>(TData data);

        void Flush<TData>();
    }
}
