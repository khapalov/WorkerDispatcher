namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchFactory
    {
        IBatchToken Start();
    }
}