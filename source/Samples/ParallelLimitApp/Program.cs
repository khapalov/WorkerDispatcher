using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace Samples
{
    class Program
    {
        static IDispatcherToken DisaptcherToken;

        static void Execute()
        {
            PostSomeWorker();
            PostSomeWorkerWithError();
            PostSomeWorkerToLong();

            var t = Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(1000);

                    Console.WriteLine($"queue count: {DisaptcherToken.QueueProcessCount}, current count: {DisaptcherToken.ProcessCount}");
                } while (true);
            });

            DisaptcherToken.WaitCompleted(120);
        }

        private static void PostSomeWorker()
        {
            for (int i = 0; i < 100; i++)
            {
                var save = i;

                DisaptcherToken.Post(new Worker(), save);
            }
        }

        private static void PostSomeWorkerWithError()
        {
            for (int i = 0; i < 100; i++)
            {
                var save = i;

                DisaptcherToken.Post(new WorkerWithError(), save);
            }
        }

        private static void PostSomeWorkerToLong()
        {
            for (int i = 0; i < 100; i++)
            {
                var save = i;

                DisaptcherToken.Post(new WorkerToLong(), save);
            }
        }

        static void Main(string[] args)
        {
            var factory = new ActionDispatcherFactory(new Workerhandler());

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            DisaptcherToken = factory.Start(new ActionDispatcherSettings
            {
                PrefetchCount = 10,
                Schedule = ScheduleType.ParallelLimit,
                Timeout = TimeSpan.FromSeconds(1.5)
            });

            Execute();

            stopwatch.Stop();

            Console.WriteLine($"STOP {stopwatch.Elapsed.TotalSeconds} queue count: {DisaptcherToken.QueueProcessCount}, current count: {DisaptcherToken.ProcessCount}");
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
            Console.WriteLine($"result: {result}, duration: {duration}");
        }
    }

}
