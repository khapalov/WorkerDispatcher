using System;
using System.Threading.Tasks;
using WorkerDispatcher;
using WorkerDispatcher.Extensions.Batch;

namespace BulkApp
{
    class Program
    {

        static void Main(string[] args)
        {
            var handler = new Workerhandler();

            var factory = new ActionDispatcherFactory(handler);

            var dispatcher = factory.Start();

            var bathToken = dispatcher.Plugin.Batch(p =>
            {
                p.For<int>()
                    .MaxCount(15)
                    .Period(TimeSpan.FromSeconds(1.5))
                    .Bind(() =>
                    {
                        return new BatchDataWorkerInt();
                    });
                
                p.For<string>()
                    .MaxCount(15)
                    .Period(TimeSpan.FromSeconds(1))
                    .Bind(() => new BatchDataWorkerString());

            }).Start();

            Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < 60; i++)
                {
                    if ((i % 10) == 0)
                    {
                        await Task.Delay(200);
                    }

                    bathToken.Send(i);
                }
            });

            Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    if ((i % 10) == 0)
                    {
                        await Task.Delay(2000);
                    }

                    bathToken.Send($"hello {i}");
                }
            });

            Console.ReadKey();

            bathToken.Dispose();

            dispatcher.WaitCompleted();

            Console.WriteLine("stop all");
        }
    }
}
