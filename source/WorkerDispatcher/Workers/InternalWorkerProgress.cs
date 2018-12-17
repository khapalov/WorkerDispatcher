using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Workers
{
    internal class InternalWorkerProgress : IActionInvoker
    {
        public IProgress<WorkerProgressData> Progress { get; }

        internal int Index { get; }

        private readonly IActionInvoker _actionInvoker;

        public InternalWorkerProgress(IActionInvoker actionInvoker, IProgress<WorkerProgressData> progress, int index)
        {
            Progress = progress;
            Index = index;
            _actionInvoker = actionInvoker;
        }

        public async Task<object> Invoke(CancellationToken token)
        {
            var stopwatch = new Stopwatch();

            var result = default(object);

            try
            {
                stopwatch.Start();

                result = await _actionInvoker.Invoke(token);

                stopwatch.Stop();

                Progress.Report(new WorkerProgressData
                {
                    Duration = stopwatch.ElapsedMilliseconds,
                    Index = Index,
                    Result = result
                });
            }
            catch (OperationCanceledException)
            {
                Progress.Report(new WorkerProgressData
                {
                    Duration = stopwatch.ElapsedMilliseconds,
                    Index = Index,
                    IsError = true,
                    IsCancelled = true
                });

                throw;
            }
            catch (Exception)
            {
                stopwatch.Stop();

                Progress.Report(new WorkerProgressData
                {
                    Duration = stopwatch.ElapsedMilliseconds,
                    Index = Index,
                    IsError = true                    
                });

                throw;
            }

            return result;
        }
    }
}
