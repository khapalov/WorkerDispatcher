using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerDispatcher.Batch
{
    internal class LocalQueueProvider
    {
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _queue;
        private readonly BatchConfigProvider _config;

        public LocalQueueProvider(ConcurrentDictionary<Type, ConcurrentQueue<object>> queue, BatchConfigProvider config)
        {
            _queue = queue;
            _config = config;
        }

        public int Enqueue<TData>(TData data)
        {
            if (_queue.TryGetValue(typeof(TData), out ConcurrentQueue<object> q))
            {                
                q.Enqueue(data);
            }
            else
            {
                throw new ArgumentException($"No registered type {typeof(TData)}");
            }

            return q.Count;
        }

        public bool HasQueued<TData>()
        {
            return CheckExistQueueData(typeof(TData));
        }

        public bool HasQueued(Type eventType)
        {
            return CheckExistQueueData(eventType);
        }

        private bool CheckExistQueueData(Type eventType)
        {
            if (_queue.TryGetValue(eventType, out ConcurrentQueue<object> queue))
            {
                return queue.Count > 0;
            }

            return false;
        }

        public Array Dequeue(Type type, int retreiveCount = 0)
        {
            Array resultArr = default(Array);

            if (_queue.TryGetValue(type, out ConcurrentQueue<object> queue))
            {
                if (queue.Any())
                {
                    var count = queue.Count;

                    var configQueue = _config.Get(type);

                    var maxCount = retreiveCount > 0 ? retreiveCount : configQueue.MaxCount;

                    var len = retreiveCount < 0 ? count :
                              count >= maxCount ? maxCount : count;

                    var arrType = type.MakeArrayType();

                    resultArr = (Array)Activator.CreateInstance(arrType, len);

                    for (int i = 0; i < len; i++)
                    {
                        if (!queue.TryDequeue(out object res))
                            break;

                        resultArr.SetValue(res, i);
                    }
                }
            }

            return resultArr;
        }
    }
}
