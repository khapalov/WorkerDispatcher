using System;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace Samples
{
	class Program
	{
		static IDispatcherToken DisaptcherToken;

		static async Task MainAsync(string[] args)
		{
			for (int i = 0; i < 20; i++)
			{
				var a = i;

				DisaptcherToken.Post(async ct => { await Task.Delay(3000, ct); Console.WriteLine(a); });
			}

			var t = Task.Run(async () =>
			{
				do
				{
					await Task.Delay(1000);

					Console.WriteLine($"queue count: {DisaptcherToken.QueueProcessCount}, current count: {DisaptcherToken.ProcessCount}");
				} while (true);
			});

			await Task.Delay(1000);

			DisaptcherToken.WaitComplete(50);
		}

		static void Main(string[] args)
		{
			var factory = new ActionDispatcherFactory();

			DisaptcherToken = factory.Start(new ActionDispatcherSettings
			{
				PrefetchCount = 5,
				Schedule = ScheduleType.Parallel,
				Timeout = TimeSpan.FromSeconds(60)
			});

			MainAsync(null).Wait();

			Console.WriteLine("stop");
		}
	}
}
