using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchFactory : IBatchFactory
    {
        private readonly BatchQueueProvider _batchQueueProvider;
        private readonly IDispatcherPlugin _plugin;
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;
        private readonly QueueEvent<Type> _queueEvent;
        private readonly LocalQueueManager _localQueue;
        private readonly Dictionary<Type, MethodInfo> _cacheMethod = new Dictionary<Type, MethodInfo>();
        private readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim(false);

        public BatchFactory(BatchQueueProvider batchQueueProvider,
            IDispatcherPlugin plugin,
            IReadOnlyDictionary<Type, BatchConfig> config,
            QueueEvent<Type> queueEvent,
            LocalQueueManager queueManager)
        {
            _batchQueueProvider = batchQueueProvider;
            _plugin = plugin;
            _config = config;
            _queueEvent = queueEvent;
            _localQueue = queueManager;
        }

        public IBatchToken Start()
        {
            var sender = _plugin.Sender;

            var methodPost = sender.GetType()
                .GetMethods()
                .Where(p => p.IsGenericMethod && p.Name == nameof(sender.Post))
                .Single(p => p.GetParameters().Length == 3);

            var batchToken = new BatchToken(_localQueue, _batchQueueProvider, _queueEvent, _manualResetEventSlim);

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

                        PostWorker(type, methodPost);
                        
                    }
                    catch (Exception ex)
                    {
                        _plugin.LogFault(ex);
                    }
                }

                Flushing(methodPost);
            });

            _batchQueueProvider.StartTimers();

            return batchToken;
        }

        private void PostWorker(Type type, MethodInfo methodPost, int retreiveCount = 0)
        {
            var sender = _plugin.Sender;

            var arr = _localQueue.Dequeue(type, retreiveCount);

            if (arr == null || arr.Length == 0)
                return;

            var configQueue = _config[type];

            var worker = configQueue.Factory.DynamicInvoke();

            if (!_cacheMethod.TryGetValue(type, out MethodInfo genericMethod))
            {
                genericMethod = methodPost.MakeGenericMethod(arr.GetType());

                _cacheMethod.Add(type, genericMethod);
            }

            genericMethod.Invoke(sender, new object[] { worker, arr, configQueue.TimeLimit });
        }

        private void Flushing(MethodInfo methodPost)
        {
            try
            {
                var queueFlush = _config.Where(p => p.Value.FlushOnStop);

                foreach (var f in queueFlush)
                {
                    PostWorker(f.Key, methodPost, -1);
                }
            }
            catch(Exception ex)
            {
                _plugin.LogFault(ex);
            }
            finally
            {
                _manualResetEventSlim.Set();
            }

            _manualResetEventSlim.Dispose();
        }
    }
}
