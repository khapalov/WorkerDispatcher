using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace ChainApp
{
    class Program
    {
        static IDispatcherToken DisaptcherToken;

        static async Task Execute(string sel, int count)
        {
            await Task.Yield();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;            
            
            switch (sel)
            {
                case "complete":
                    {
                        //Chain with run CompleteWorker on completed
                        var completedChain = CreateChain(1, count);
                        completedChain.Run(new CompletedWorker());

                        break;
                    }
                case "callback":
                    {
                        //Chain with call callback on completed
                        var callbackChain = CreateChain(1, count);
                        callbackChain.Run(p =>
                        {
                            //await Task.Yield();

                            var cpl = new CompletedWorker().Invoke(p, CancellationToken.None);
                            cpl.Wait();
                        });

                        break;
                    }
                case "async":
                    {
                        //Chain with async task
                        var asyncChaing = CreateChain(1, count);
                        await asyncChaing.RunAsync().ContinueWith(t =>
                        {
                            if (!t.IsFaulted)
                            {
                                PrintResult(t.Result);
                            }
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);

                        break;
                    }
                case "sync":
                    {
                        //Chain run synchronous and get result on completed
                        var stopwatch = Stopwatch.StartNew();
                        var syncChaing = CreateChain(1, count);
                        var res = syncChaing.RunSync();
                        stopwatch.Stop();
                        Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}");
                        PrintResult(res);

                        break;
                    }
                case "pages":
                    {
                        DisaptcherToken.Post(new FindPagesWorker(DisaptcherToken, new string[] {
                            "https://raw.githubusercontent.com/NemoTravel/nemo.travel.geodata/master/airlines.json",
                            "https://raw.githubusercontent.com/NemoTravel/nemo.travel.geodata/master/countries.json",
                            "https://raw.githubusercontent.com/NemoTravel/nemo.travel.geodata/master/airports.json"
                        }));

                        break;
                    }
            }
        }

        static void Main(string[] args)
        {
            var factory = new ActionDispatcherFactory(new Workerhandler());

            DisaptcherToken = factory.Start(new ActionDispatcherSettings
            {
                Schedule = ScheduleType.Parallel,
                Timeout = TimeSpan.FromSeconds(2)
            });

            var select = args.Length > 0 ? args[0] : default(string) ?? "complete";

            Execute(select, args.Length > 1 ? int.Parse(args[1]) : 100).Wait();

            if (select == "pages")
                Console.ReadKey();

            Console.WriteLine("await stop");
            DisaptcherToken.WaitCompleted(120);
            DisaptcherToken.Dispose();
            Console.WriteLine("stopped success");
            //Console.ReadKey();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("process exit");
        }

        private static void PrintResult(WorkerCompletedData res)
        {
            foreach (var data in res.Results)
            {
                Console.WriteLine($"Data: {data.Data}, Duration:{data.Duration} Result: {data.Result?.ToString()}, IsError: {data.IsError}, IsCancelled: {data.IsCancelled}");
            }

            var maxDuration = res.Results.Max(p => p.Duration);

            Console.WriteLine($"Max duration: {maxDuration}");
        }

        private static IWorkerChain CreateChain(int start, int count)
        {
            var chain = DisaptcherToken.Chain();

            var len = start + count;
            for (var i = start; i < len; i++)
            {
                chain.Post(new WorkerToLong(), i);
            }

            return chain;
        }

        private static IWorkerChain CreateChainInline(int start, int count)
        {
            var chain = DisaptcherToken.Chain();

            var len = start + count;
            for (var i = start; i < len; i++)
            {
                var s = i;
                chain.Post(async ct => { await Task.Delay(s * 500); Console.WriteLine($"inline data i={s}"); });
            }

            return chain;
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

            return $"Ok, TotalCount { completed.Results.Length}";
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
            if ((data % 2) == 0)
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

    internal class FindPagesWorker : IActionInvoker
    {
        private readonly IDispatcherTokenSender _sender;
        private readonly string[] _data;

        public FindPagesWorker(IDispatcherTokenSender sender, string[] data)
        {
            _sender = sender;
            _data = data;
        }

        public async Task<object> Invoke(CancellationToken token)
        {
            await Task.Yield();

            var chain = _sender.Chain();
            foreach (var item in _data)
            {
                chain.Post(new DownloadWorker(), item);
            }

            var result = await chain.RunAsync();

            foreach (var item in result.Results)
            {
                Console.WriteLine($"Url: {item.Data}, Duration: {item.Duration}, \r\nResult:{item.Result?.ToString().Substring(0, 400)}");
            }         

            return $"Count: {result.Results?.Length}";
        }

        internal class DownloadWorker : IActionInvoker<string>
        {
            public async Task<object> Invoke(string url, CancellationToken token)
            {
                using (var http = new HttpClient())
                {
                    var content = await http.GetAsync(url, token);

                    return await content.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
