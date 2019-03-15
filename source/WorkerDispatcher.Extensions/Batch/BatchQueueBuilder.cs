using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchQueueBuilder : IBatchQueueBuilder
    {
        private readonly Dictionary<Type, BatchConfig> _config;

        public BatchQueueBuilder(Dictionary<Type, BatchConfig> config)
        {
            _config = config;
        }

        public IBatchBindingBuilder For<TData>()
        {
            var bathConfig = new BatchConfig();            

            var builder = new BatchBindingBuilder(bathConfig, _config);

            return builder;
        }
    }
}
