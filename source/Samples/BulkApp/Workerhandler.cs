using System;
using System.Collections.Generic;
using System.Text;
using WorkerDispatcher;

namespace BulkApp
{
    internal class Workerhandler : IWorkerHandler
    {
        public void HandleError(Exception ex, decimal duration, bool isCancel)
        {
            //Console.WriteLine($"error: {ex?.ToString()}, duration: {duration}, isCancelled: {isCancel}");
            Console.WriteLine($"error: {ex?.InnerException?.Message}, duration: {duration}, isCancelled: {isCancel}");
        }

        public void HandleFault(Exception ex)
        {
            Console.WriteLine($"fault: {ex?.ToString()}");
        }

        public void HandleResult(object result, decimal duration)
        {
            //Console.WriteLine($"result: {result}, duration: {duration}");
        }
    }
}
