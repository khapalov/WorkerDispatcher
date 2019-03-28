using System;
using System.Diagnostics;

namespace WorkerDispatcher
{
    public class DefaultWorkerHandler : IWorkerHandler
    {
        public void HandleError(Exception ex, decimal duration, bool isCancel)
        {
            Debug.WriteLineIf(isCancel, String.Format("Worker cancelled, Duration {0}", duration));
            Debug.WriteLineIf(!isCancel, String.Format("Error: {0}, Duration: {1}", ex, duration));
        }

        public void HandleFault(Exception ex)
        {
            Debug.Write(ex);
        }

        public void HandleResult(object result, decimal duration)
        {            
            Debug.WriteLine(result);
        }
    }
}
