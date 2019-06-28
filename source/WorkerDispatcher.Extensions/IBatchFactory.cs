namespace WorkerDispatcher.Batch
{
    public interface IBatchFactory
    {
        IBatchToken Start();
    }
}