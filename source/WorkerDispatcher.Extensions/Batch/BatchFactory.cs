using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchFactory : IBatchFactory
    {
        private readonly BatchQueueProvider _batchQueueProvider;
        private readonly IDispatcherTokenSender _sender;
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;

        public BatchFactory(BatchQueueProvider batchQueueProvider,
            IDispatcherTokenSender sender,
            IReadOnlyDictionary<Type, BatchConfig> config)
        {
            _batchQueueProvider = batchQueueProvider;
            _sender = sender;
            _config = config;
        }

        public IBatchToken Start()
        {
            BatchToken batchToken;
            
            var localQueue = CreateQueue();

            var batchDataType = typeof(BatchData<>);
            var actionInvokeType = typeof(IActionInvoker<>);
            
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                batchToken = new BatchToken(localQueue, cancellationTokenSource);

                var cancellationToken = cancellationTokenSource.Token;

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var type = _batchQueueProvider.WaitEvent(cancellationToken);

                            if (localQueue.TryGetValue(type, out ConcurrentQueue<object> q))
                            {
                                var batchGeneric = batchDataType.MakeGenericType(type);
                                var invokerGeneric = actionInvokeType.MakeGenericType(type);

                                var configQueue = _config[type];

                                //var worker = configQueue.Factory.DynamicInvoke();

                                var list = new List<object>();

                                for (int i = 0; i < configQueue.MaxCount; i++)
                                {
                                    if (!q.TryDequeue(out object res))
                                        break;

                                    list.Add(res);
                                }

                                var bacthDatas = Activator.CreateInstance(batchGeneric, new object[] { list.ToArray() });

                                var method = _sender.GetType().GetMethod("Post");

                                var genericMethod = method.MakeGenericMethod(type);

                                genericMethod.Invoke(_sender, new object[] { bacthDatas });

                                /*_sender.GetType().InvokeMember("Post",
                                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null,
                                    _sender, 
                                    new object[] { worker, bacthDatas });*/
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //_workerHandler.HandleFault(ex);
                    }
                });
            }

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
