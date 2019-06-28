using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Batch
{
    internal class LocalQueueBuilder
    {
        private readonly BatchConfigProvider _config;

        public LocalQueueBuilder(BatchConfigProvider config)
        {
            _config = config;
        }

        public LocalQueueProvider Build()
        {
            var queue = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();
            foreach (var c in _config.GetAll())
            {
                if (!queue.TryAdd(c.Key, new ConcurrentQueue<object>()))
                {
                    throw new ArgumentException($"Key is exist {c.Key}");
                }
            }

            return new LocalQueueProvider(queue, _config);
        }
    }
}
