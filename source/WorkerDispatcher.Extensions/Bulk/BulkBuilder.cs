using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Bulk
{
    internal class BulkQueueBuilder : IBulkQueueBuilder
    {
        private readonly IDictionary<Type, object> _dic = new Dictionary<Type, object>();

        public BulkQueueBuilder(IDictionary<Type, object> dic)
        {
            _dic = dic;
        }

        public void For<TData>()
        {
            _dic.Add(typeof(TData), new object());
        }
    }

    public interface IBulkQueueBuilder
    {
        void For<TData>();
    }
}
