using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Workers
{
    internal class InternalWorkerProgress : IActionInvoker
    {
        private readonly IProgress<WorkerProgressData> _progress;

        private readonly int _index;

        private readonly IActionInvoker _actionInvoker;

        public InternalWorkerProgress(IActionInvoker actionInvoker, IProgress<WorkerProgressData> progress, int index)
        {
            _progress = progress;
            _index = index;
            _actionInvoker = actionInvoker;
        }

        public async Task<object> Invoke(CancellationToken token)
        {
            var stopwatch = new Stopwatch();

            var result = default(object);

            var workerData = _actionInvoker is IActionInvokerData data ? data.Data : default(object);

            try
            {
                stopwatch.Start();

                result = await _actionInvoker.Invoke(token);

                stopwatch.Stop();

                _progress.Report(new WorkerProgressData
                {
                    Duration = stopwatch.ElapsedMilliseconds,
                    Index = _index,
                    Result = result,
                    Data = workerData
                });
            }
            catch (TaskCanceledException)
            {
                stopwatch.Stop();

                _progress.Report(new WorkerProgressData
                {
                    Duration = stopwatch.ElapsedMilliseconds,
                    Index = _index,
                    IsError = true,
                    IsCancelled = true,
                    Data = workerData
                });

                throw;
            }
            catch (Exception)
            {
                stopwatch.Stop();

                _progress.Report(new WorkerProgressData
                {
                    Duration = stopwatch.ElapsedMilliseconds,
                    Index = _index,
                    IsError = true,
                    Data = workerData
                });

                throw;
            }

            return result;
        }
    }
}
