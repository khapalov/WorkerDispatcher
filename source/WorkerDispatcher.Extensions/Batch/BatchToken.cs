using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchToken : IBatchToken
    {
        

        public BatchToken()
        {
        }

        public void Send<TData>(TData data)
        { }
    }
}
