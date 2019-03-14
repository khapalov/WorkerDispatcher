namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchToken
    {
        void Send<TData>(TData data);
    }
}