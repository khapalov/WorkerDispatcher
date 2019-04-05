using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchFactory : IBatchFactory
    {
        private readonly BatchQueueProvider _batchQueueProvider;
        private readonly IDispatcherPlugin _plugin;
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;
        private readonly QueueEvent<Type> _queueEvent;

        public BatchFactory(BatchQueueProvider batchQueueProvider,
            IDispatcherPlugin plugin,
            IReadOnlyDictionary<Type, BatchConfig> config,
            QueueEvent<Type> queueEvent)
        {
            _batchQueueProvider = batchQueueProvider;
            _plugin = plugin;
            _config = config;
            _queueEvent = queueEvent;
        }

        public IBatchToken Start()
        {
            var localQueue = new LocalQueueBuilder(_config).Build();

            var actionInvokeType = typeof(IActionInvoker<>);

            var sender = _plugin.Sender;

            var methodPost = sender.GetType()
                .GetMethods()
                .Where(p => p.IsGenericMethod && p.Name == nameof(sender.Post))
                .Single(p => p.GetParameters().Length == 3);

            var batchToken = new BatchToken(localQueue, _batchQueueProvider, _queueEvent);

            var cancellationToken = batchToken.CancellationToken;

            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var type = _queueEvent.WaitEvent(cancellationToken);

                        if (type == null)
                            continue;

                        var arr = localQueue.Dequeue(type);

                        if (arr == null || arr.Length == 0)
                            continue;

                        var configQueue = _config[type];

                        var worker = configQueue.Factory.DynamicInvoke();

                        var genericMethod = methodPost.MakeGenericMethod(arr.GetType());

                        genericMethod.Invoke(sender, new object[] { worker, arr, configQueue.TimeLimit });
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
    }
}
