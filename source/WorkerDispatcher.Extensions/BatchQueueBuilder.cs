using System;
using System.Collections.Generic;

namespace WorkerDispatcher.Batch
{
    internal class BatchQueueBuilder : IBatchQueueBuilder
    {
        private readonly Dictionary<Type, BatchConfig> _config;

        public BatchQueueBuilder(Dictionary<Type, BatchConfig> config)
        {
            _config = config;
        }

        public IBatchBindingBuilder<TData> For<TData>()
        {
            var bathConfig = new BatchConfig();            

            var builder = new BatchBindingBuilder<TData>(bathConfig, _config);

            return builder;
        }
    }
}
