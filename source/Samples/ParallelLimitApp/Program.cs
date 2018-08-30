using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace Samples
{
	class Program
	{
		static IDispatcherToken DisaptcherToken;

		static async Task MainAsync(string[] args)
		{
			for (int i = 0; i < 1000; i++)
			{
				DisaptcherToken.Post(async ct => { await Task.Delay(1500, ct); });
			}

			var t = Task.Run(async () =>
			{
				do
				{
					await Task.Delay(1000);

					Console.WriteLine($"queue count: {DisaptcherToken.QueueProcessCount}, current count: {DisaptcherToken.ProcessCount}");
				} while (true);
			});

			await Task.Delay(2000);

			DisaptcherToken.WaitComplete(120);
		}

		static void Main(string[] args)
		{
			var factory = new ActionDispatcherFactory();

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			DisaptcherToken = factory.Start(new ActionDispatcherSettings
			{
				PrefetchCount = 100,
				Schedule = ScheduleType.ParallelLimit,
				Timeout = TimeSpan.FromSeconds(60)
			});

			MainAsync(null).Wait();

			stopwatch.Stop();

			Console.WriteLine($"STOP {stopwatch.Elapsed.TotalSeconds} queue count: {DisaptcherToken.QueueProcessCount}, current count: {DisaptcherToken.ProcessCount}");			
		}
	}
}
