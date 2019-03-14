namespace WorkerDispatcher.Extensions.Batch
{
    public delegate IActionInvoker<BatchData<TData>> BatchDelegate<TData>();
}
