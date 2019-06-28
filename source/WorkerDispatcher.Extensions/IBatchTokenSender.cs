namespace WorkerDispatcher.Batch
{
    public interface IBatchTokenSender
    {
        int Send<TData>(TData data);

        void Flush<TData>();
    }
}
