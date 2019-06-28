using System;

namespace WorkerDispatcher
{
    public class WorkerProgressData
    {
        public bool IsError { get; set; }

        public bool IsCancelled { get; set; }

        internal int Index { get; set; }

        public long Duration { get; set; }

        public object Result { get; set; }

        public object Data { get; set; }

        public Exception Error { get; set; }
    }
}
