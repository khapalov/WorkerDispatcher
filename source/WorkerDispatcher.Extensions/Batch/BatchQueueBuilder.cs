using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchQueueBuilder : IBatchQueueBuilder
    {
        private readonly Dictionary<Type, object> _mapping = new Dictionary<Type, object>();
        private readonly IBatchConfigBuilder _configBuilder;

        public BatchQueueBuilder()
        {
            _configBuilder = new BatchConfigBuilder(_mapping);
        }

        public IBatchConfigBuilder For<TData>()
        {
            
            //TODO fix
            _mapping.Add(typeof(TData), new object());

            return _configBuilder;
        }

        //internal BatchQueueProvider Build()
        //{            
        //    return new BatchQueueProvider(_configBuilder, _queueEvent);
        //}
    }
}
