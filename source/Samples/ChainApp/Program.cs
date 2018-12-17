using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace ChainApp
{
    class Program
    {
        static IDispatcherToken DisaptcherToken;

        static void Execute()
        {
            DisaptcherToken.Chain()
                .Post(new Worker(), 10)
                .Post(new Worker(), 20)
                .Post(new WorkerWithError(), 3)
                .Post(new WorkerToLong(), 30)
                .Run(new CompletedWorker());
        }

        static void Main(string[] args)
        {
            var factory = new ActionDispatcherFactory(new Workerhandler());

            DisaptcherToken = factory.Start(new ActionDispatcherSettings
            {
                Schedule = ScheduleType.Parallel,
                Timeout = TimeSpan.FromSeconds(1.5)
            });

            Execute();
            
            Console.ReadKey();

            DisaptcherToken.WaitCompleted(120);

        }
    }

    internal class Workerhandler : IWorkerHandler
    {
        public void HandleError(Exception ex, decimal duration, bool isCancel)
        {
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

    internal class Worker : IActionInvoker<int>
    {
        public async Task<object> Invoke(int data, CancellationToken token)
        {
            await Task.Delay(1000, token);

            return $"i={data}";
        }
    }

    internal class CompletedWorker : IActionInvoker<WorkerCompletedData>
    {
        public async Task<object> Invoke(WorkerCompletedData completed, CancellationToken token)
        {
            await Task.Yield();

            foreach (var data in completed.Results)
            {
                Console.WriteLine($"Data: {data.Data}, Duration:{data.Duration} Result: {data.Result?.ToString()}, IsError: {data.IsError}, IsCancelled: {data.IsCancelled}");
            }

            var totalDuration = completed.Results.Sum(p => p.Duration);

            return $"Ok, TotalDuration {totalDuration}";
        }
    }

    internal class WorkerWithError : IActionInvoker<int>
    {
        public async Task<object> Invoke(int data, CancellationToken token)
        {
            if ((data % 3) == 0)
            {
                throw new ArgumentException($"wrong data is {data}");
            }

            await Task.Delay(1000, token);

            return $"i={data}";
        }
    }

    internal class WorkerToLong : IActionInvoker<int>
    {
        public async Task<object> Invoke(int data, CancellationToken token)
        {
            if ((data % 3) == 0)
            {
                await Task.Delay(3000, token);
            }
            else
            {
                await Task.Delay(1000, token);
            }

            return $"i={data}";
        }
    }
}
