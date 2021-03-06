﻿using System;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher.Batch;

namespace BulkApp
{
    internal class BatchDataWorkerInt : IBatchActionInvoker<int>
    {
        public Task<object> Invoke(int[] data, CancellationToken token)
        {
            foreach(var d in data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine("============================");
            return Task.FromResult((object)data.Length.ToString());
        }
    }

    internal class BatchDataWorkerString : IBatchActionInvoker<string>
    {
        public Task<object> Invoke(string[] data, CancellationToken token)
        {            
            foreach (var d in data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine("============================");
            return Task.FromResult(new object());
        }
    }
}
