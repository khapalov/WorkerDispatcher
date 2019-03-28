using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchFactory : IBatchFactory
    {
        private readonly BatchQueueProvider _batchQueueProvider;
        private readonly IDispatcherPlugin _plugin;
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;

        public BatchFactory(BatchQueueProvider batchQueueProvider,
            IDispatcherPlugin plugin,
            IReadOnlyDictionary<Type, BatchConfig> config)
        {
            _batchQueueProvider = batchQueueProvider;
            _plugin = plugin;
            _config = config;
        }

        public IBatchToken Start()
        {
            var localQueue = CreateQueue();

            var actionInvokeType = typeof(IActionInvoker<>);

            var sender = _plugin.Sender;

            var methodPost = sender.GetType()
                .GetMethods()
                .Where(p => p.IsGenericMethod && p.Name == nameof(sender.Post))
                .Single(p => p.GetParameters().Length == 3);

            var batchToken = new BatchToken(localQueue, _batchQueueProvider);

            var cancellationToken = batchToken.CancellationToken;

            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var type = _batchQueueProvider.WaitEvent(cancellationToken);

                        if (type == null)
                            continue;

                        if (localQueue.TryGetValue(type, out ConcurrentQueue<object> q))
                        {
                            if (q.Any())
                            {
                                var invokerGeneric = actionInvokeType.MakeGenericType(type);

                                var configQueue = _config[type];

                                var worker = configQueue.Factory.DynamicInvoke();

                                var count = q.Count;

                                var len = count >= configQueue.MaxCount ? configQueue.MaxCount : count;

                                var arrType = type.MakeArrayType();

                                var arrObj = (Array)Activator.CreateInstance(arrType, len);

                                for (int i = 0; i < configQueue.MaxCount; i++)
                                {
                                    if (!q.TryDequeue(out object res))
                                        break;

                                    arrObj.SetValue(res, i);
                                }

                                var genericMethod = methodPost.MakeGenericMethod(arrType);

                                genericMethod.Invoke(sender, new object[] { worker, arrObj, configQueue.TimeLimit });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _plugin.LogFault(ex);
                    }
                }
            });

            _batchQueueProvider.StartTimers();

            return batchToken;
        }

        private ConcurrentDictionary<Type, ConcurrentQueue<object>> CreateQueue()
        {
            var queue = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();
            foreach (var c in _config)
            {
                if (!queue.TryAdd(c.Key, new ConcurrentQueue<object>()))
                {
                    throw new ArgumentException($"Key is exist {c.Key}");
                }
            }

            return queue;
        }
    }
}
