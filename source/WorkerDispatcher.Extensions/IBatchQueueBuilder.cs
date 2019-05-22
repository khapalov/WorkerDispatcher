namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchQueueBuilder
    {        
        IBatchBindingBuilder<TData> For<TData>();
    }
}
