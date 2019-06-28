using System;
using System.Linq;
using System.Threading.Tasks;
using WorkerDispatcher;
using WorkerDispatcher.Batch;

namespace BulkApp
{
    class Program
    {

        static void Main(string[] args)
        {
            var handler = new Workerhandler();

            var factory = new ActionDispatcherFactory();

            var dispatcher = factory.Start();

            var bathToken = dispatcher.Plugin.Batch(p =>
            {
                p.For<int>()
                    .MaxCount(15)
                    .Period(TimeSpan.FromSeconds(10))
                    .TriggerCount(5)
                    .FlushOnStop(false)
                    .Bind(() =>
                    {
                        return new BatchDataWorkerInt();
                    });

                p.For<string>()
                    .MaxCount(10)
                    .Period(TimeSpan.FromSeconds(1))
                    //.TriggerCount(5)
                    .FlushOnStop(true)
                    .Bind(data =>
                    {
                        var str = string.Join(", ", data.Select(s => s.ToString()));
                        Console.WriteLine(str);
                        return Task.CompletedTask;
                    });

            }).Start();

            //Task.Factory.StartNew(() =>
            //{
            //    for (var i = 0; i < 50; i++)
            //    {
            //        if ((i % 10) == 0)
            //        {
            //            //await Task.Delay(200);
            //        }

            //        bathToken.Send(i);
            //    }
            //});

            Task.Factory.StartNew(async () =>
            {
                for (var i = 1; i < 100; i++)
                {
                    if ((i % 2) == 0)
                    {
                        //await Task.Delay(100);
                    }

                    bathToken.Send($"hello {i}");
                }
            });           

            Console.ReadKey();

            bathToken.Stop();
            bathToken.Dispose();

            dispatcher.WaitCompleted();

            Console.WriteLine("stop all");
        }
    }
}
