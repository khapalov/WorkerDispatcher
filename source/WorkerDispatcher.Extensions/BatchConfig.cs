using System;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchConfig
    {
        public BatchConfig()
        {
            AwaitTimePeriod = TimeSpan.FromSeconds(30);
            MaxCount = 10;
            TimeLimit = TimeSpan.FromSeconds(30);
            FlushOnStop = true;
        }

        public TimeSpan AwaitTimePeriod { get; internal set; }

        public int MaxCount { get; internal set; }

        public TimeSpan TimeLimit { get; internal set; }

        public Delegate Factory { get; internal set; }

        public bool FlushOnStop { get; internal set; }
    }
}
