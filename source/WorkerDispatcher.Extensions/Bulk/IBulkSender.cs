namespace WorkerDispatcher.Extensions.Bulk
{
    public interface IBulkSender<in TData>
    {
        void Send(TData data);
    }
}
