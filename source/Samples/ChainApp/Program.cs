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
            var completedChain = CreateCheain(1, 10);
            completedChain.Run(new CompletedWorker());

            var callbackChain = CreateCheain(11, 10);
            callbackChain.Run(async p =>
            {
                await Task.Yield();

                var cpl = await new CompletedWorker().Invoke(p, CancellationToken.None);
            });

            var syncChaing = CreateCheain(21, 10);
            var res = syncChaing.RunSync();

            foreach (var data in res.Results)
            {
                Console.WriteLine($"Data: {data.Data}, Duration:{data.Duration} Result: {data.Result?.ToString()}, IsError: {data.IsError}, IsCancelled: {data.IsCancelled}");
            }
        }

        private static IWorkerChain CreateCheain(int start, int count)
        {
            var chain = DisaptcherToken.Chain();

            var len = start + count;
            for (var i = start; i < len; i++)
            {
                chain.Post(new WorkerToLong(), i);
            }

            return chain;
        }

        static void Main(string[] args)
        {
            var factory = new ActionDispatcherFactory(new Workerhandler());

            DisaptcherToken = factory.Start(new ActionDispatcherSettings
            {
                Schedule = ScheduleType.Parallel,
                Timeout = TimeSpan.FromSeconds(5)
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
