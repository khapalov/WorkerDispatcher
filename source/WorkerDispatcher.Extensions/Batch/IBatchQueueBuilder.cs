namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchQueueBuilder
    {        
        IBatchBindingBuilder For<TData>();
    }
}
