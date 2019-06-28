namespace WorkerDispatcher.Batch
{
    public interface IBatchQueueBuilder
    {        
        IBatchBindingBuilder<TData> For<TData>();
    }
}
