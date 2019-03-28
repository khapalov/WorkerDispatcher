using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchBindingBuilder : IBatchBindingBuilder
    {
        private readonly BatchConfig _configToType;
        private readonly Dictionary<Type, BatchConfig> _config;

        public BatchBindingBuilder(BatchConfig batchConfig, Dictionary<Type, BatchConfig> config)
        {
            _configToType = batchConfig;
            _config = config;
        }

        public IBatchBindingBuilder Period(TimeSpan time)
        {
            _configToType.AwaitTimePeriod = time;
            return this;
        }

        public IBatchBindingBuilder MaxCount(int maxCount)
        {
            _configToType.MaxCount = maxCount;
            return this;
        }

        public IBatchBindingBuilder TimeLimit(TimeSpan time)
        {
            _configToType.TimeLimit = time;
            return this;
        }

        public void Bind<TData>(BatchFactoryDelegate<TData> factoryDelegate)
        {
            _configToType.Factory = factoryDelegate;
            _config.Add(typeof(TData), _configToType);
        }
    }
}
