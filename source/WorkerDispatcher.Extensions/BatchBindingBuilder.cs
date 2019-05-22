using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Batch
{
    internal class BatchBindingBuilder<TData> : IBatchBindingBuilder<TData>
    {
        private readonly BatchConfig _configToType;
        private readonly Dictionary<Type, BatchConfig> _config;

        public BatchBindingBuilder(BatchConfig batchConfig, Dictionary<Type, BatchConfig> config)
        {
            _configToType = batchConfig;
            _config = config;
        }

        public IBatchBindingBuilder<TData> Period(TimeSpan time)
        {
            _configToType.AwaitTimePeriod = time;
            return this;
        }

        public IBatchBindingBuilder<TData> MaxCount(int maxCount)
        {
            _configToType.MaxCount = maxCount;
            return this;
        }

        public IBatchBindingBuilder<TData> TimeLimit(TimeSpan time)
        {
            _configToType.TimeLimit = time;
            return this;
        }

        public IBatchBindingBuilder<TData> FlushOnStop(bool flush)
        {
            _configToType.FlushOnStop = flush;
            return this;
        }

        public void Bind(BatchFactoryDelegate<TData> factoryDelegate)
        {
            _configToType.Factory = factoryDelegate;
            _config.Add(typeof(TData), _configToType);
        }

        public void Bind(Func<TData[], Task> action)
        {
            BatchFactoryDelegate<TData> fn = () => 
            {
                var worker = new BatchWorkerInternal<TData>(action);
                return worker;
            };

            _configToType.Factory = fn;
            _config.Add(typeof(TData), _configToType);
        }
    }

    internal class BatchWorkerInternal<TData> : IBatchActionInvoker<TData>
    {
        private readonly Func<TData[], Task> _action;

        public BatchWorkerInternal(Func<TData[], Task> action)
        {
            _action = action;
        }


        public async Task<object> Invoke(TData[] data, CancellationToken token)
        {
            await _action(data);

            return null;
        }
    }
}
