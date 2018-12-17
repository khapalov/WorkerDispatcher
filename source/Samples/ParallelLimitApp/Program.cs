using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace Samples
{
	class Program
	{        
		static IDispatcherToken DisaptcherToken;

        static int progressIndex = 0;

		static void Execute()
		{
            //PostSomeWorker(sender);
            //PostSomeWorkerWithError(sender);
            //PostSomeWorkerToLong(sender);            

            /*var t = Task.Run(async () =>
			{
				do
				{
					await Task.Delay(1000);

					Console.WriteLine($"queue count: {DisaptcherToken.QueueProcessCount}, current count: {DisaptcherToken.ProcessCount}");
				} while (true);
			});*/


            DisaptcherToken.Chain()
                .Post(new Worker(), 10)
                .Post(new Worker(), 20)
                .Post(new WorkerWithError(), 3)
                .Post(new WorkerToLong(), 30)
                .Run(new CompletedWorker());

            /*.Run((WorkerCompletedData completedData) =>
            {
                foreach (var data in completedData.Results)
                {
                    Console.WriteLine($"Data: {data.Data}, Duration:{data.Duration} Result: {data.Result?.ToString()}, IsError: {data.IsError}, IsCancelled: {data.IsCancelled}");
                }
            });*/

            //Task.Delay(2000).Wait();            
           
        }       

        public static void CompleteAll(int[] arr)
        {
            Console.WriteLine($"Competed {arr.Length}");
            var res = string.Join(", ", arr.Select((index, p) => $"{p}={index}"));
            Console.WriteLine($"Result: {res}");
        }

        private static void PostSomeWorker(IDispatcherTokenSender sender)
		{
			for (int i = 0; i < 100; i++)
			{
				var save = i;

                sender.Post(new Worker(), save);
			}
		}

		private static void PostSomeWorkerWithError(IDispatcherTokenSender sender)
		{
			for (int i = 0; i < 100; i++)
			{
				var save = i;

                sender.Post(new WorkerWithError(), save);
			}
		}

		private static void PostSomeWorkerToLong(IDispatcherTokenSender sender)
		{
			for (int i = 0; i < 100; i++)
			{
				var save = i;

                sender.Post(new WorkerToLong(), save);
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
            Console.ReadKey();

            DisaptcherToken.WaitCompleted(120);
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
