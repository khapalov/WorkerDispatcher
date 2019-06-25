using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher.Batch.QueueEvent;

namespace WorkerDispatcher.Batch
{
    internal class BatchFactory : IBatchFactory
    {
        private readonly TimerQueueProvider _batchQueueProvider;
        private readonly IDispatcherPlugin _plugin;
        private readonly BatchConfigProvider _config;
        private readonly IQueueEvent _queueEvent;
        private readonly LocalQueueProvider _localQueue;
        private readonly Dictionary<Type, MethodInfo> _cacheMethod = new Dictionary<Type, MethodInfo>();
        private readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim(false);

        public BatchFactory(TimerQueueProvider batchQueueProvider,
            IDispatcherPlugin plugin,
            BatchConfigProvider config,
            IQueueEvent queueEvent,
            LocalQueueProvider queueManager)
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

            var batchToken = new BatchToken(_localQueue, _batchQueueProvider, _queueEvent, _manualResetEventSlim, _config);

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

        private void PostWorker(object typeObj, MethodInfo methodPost, int retreiveCount = 0)
        {
            var sender = _plugin.Sender;

            var type = (Type)typeObj;

            var arr = _localQueue.Dequeue(type, retreiveCount);

            if (arr == null || arr.Length == 0)
                return;

            var configQueue = _config.Get(type);

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
                var queueFlush = _config.GetAll().Where(p => p.Value.FlushOnStop);

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
