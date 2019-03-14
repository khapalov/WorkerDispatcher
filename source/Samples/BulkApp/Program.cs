using System;
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
                    .MaxCount(10)
                    .TimeLimit(TimeSpan.FromSeconds(10))
                    .Bind(() =>
                    {
                        return new BatchDataWorkerInt();
                    });

                //p.For<int>(d => d.MaxCount(10).TimeLimit(TimeSpan.FromSeconds(10)))
                //    .Bind(() =>
                //    {
                //        return new BatchDataWorkerInt();
                //    });

                p.For<string>()
                    .MaxCount(20)
                    .Bind(() =>
                    {
                        return new BatchDataWorkerString();
                    });

            }).Start();


            //bathToken.Send(10);
            //bathToken.Send(20);
            //bathToken.Send(30);

            //var data = bulkSender.Flush();
            //var provider = new ScheduleTimerProvider();
            //var schedule = new ScheduleTimer(TimeSpan.FromSeconds(1));
            //var schedule2 = new ScheduleTimer(TimeSpan.FromSeconds(2));

            //provider.Add(schedule);
            //provider.Add(schedule2);            

            //provider.Start();

            Console.ReadKey();
            //provider.Stop();

            dispatcher.WaitCompleted();

            Console.WriteLine("stop all");
        }
    }
}
