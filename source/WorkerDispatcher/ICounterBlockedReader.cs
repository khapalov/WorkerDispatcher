namespace WorkerDispatcher
{
    public interface ICounterBlockedReader
    {
        bool StopSignal { get; }
        int Count { get; }
    }
}
