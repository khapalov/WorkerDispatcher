using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class LocalQueueBuilder
    {
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;

        public LocalQueueBuilder(IReadOnlyDictionary<Type, BatchConfig> config)
        {
            _config = config;
        }

        public LocalQueueManager Build()
        {
            var queue = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();
            foreach (var c in _config)
            {
                if (!queue.TryAdd(c.Key, new ConcurrentQueue<object>()))
                {
                    throw new ArgumentException($"Key is exist {c.Key}");
                }
            }

            return new LocalQueueManager(queue, _config);
        }
    }
}
