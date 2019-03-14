using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchConfigBuilder : IBatchConfigBuilder
    {
        private readonly IDictionary<Type, object> _mapping;

        private TimeSpan _awaitTimePeriod = TimeSpan.FromSeconds(30);
        private int _maxCount = 10;
        private TimeSpan _timeLimit = TimeSpan.FromSeconds(30);

        public BatchConfigBuilder(IDictionary<Type, object> mapping)
        {
            _mapping = mapping;
        }

        public IBatchConfigBuilder AwaitTimePeriod(TimeSpan time)
        {
            _awaitTimePeriod = time;
            return this;
        }

        public void Bind<TData>(BatchDelegate<TData> factoryDelegate)
        {
            _mapping.Add(typeof(TData), factoryDelegate);
        }


        public IBatchConfigBuilder MaxCount(int maxCount)
        {
            _maxCount = maxCount;
            return this;
        }

        public IBatchConfigBuilder TimeLimit(TimeSpan time)
        {
            _timeLimit = time;
            return this;
        }             
    }
}
