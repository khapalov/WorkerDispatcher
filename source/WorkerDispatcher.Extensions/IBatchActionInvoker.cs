namespace WorkerDispatcher.Batch
{
    public interface IBatchActionInvoker<TData> : IActionInvoker<TData[]>
    {
    }
}
