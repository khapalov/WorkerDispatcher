using System;
using System.Threading.Tasks;
using WorkerDispatcher;
using WorkerDispatcher.Extensions;
using WorkerDispatcher.Extensions.Batch;

namespace BulkApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var factory = new ActionDispatcherFactory();

            var dispatcher = factory.Start();

            var handler = new Workerhandler();

            var bathToken = dispatcher.Batch(p =>
            {
                p.For<int>()
                    .MaxCount(5)
                    .AwaitTimePeriod(TimeSpan.FromSeconds(5))
                    .Bind(() =>
                    {
                        return new BatchDataWorkerInt();
                    });

                p.For<string>()
                    .MaxCount(20)
                    .Bind(() =>
                    {
                        return new BatchDataWorkerString();
                    });

            }).Start();

            Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    if ((i % 10) == 0)
                    {
                        await Task.Delay(1000);
                    }

                    bathToken.Send(i);
                }
            });

            
            Console.ReadKey();


            //provider.Stop();

            dispatcher.WaitCompleted();

            Console.WriteLine("stop all");
        }
    }
}
