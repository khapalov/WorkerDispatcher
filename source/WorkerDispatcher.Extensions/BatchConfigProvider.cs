using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerDispatcher.Batch
{
    internal class BatchConfigProvider
    {
        private readonly Dictionary<Type, BatchConfig> _configBatches;

        public BatchConfigProvider(Dictionary<Type, BatchConfig> configBatches)
        {
            _configBatches = configBatches;
        }

        public BatchConfig Get(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return _configBatches.TryGetValue(type, out BatchConfig batchConfig)
                ? batchConfig
                : throw new ArgumentException($"Unknown type {type}");
        }

        public BatchConfig Get(object data)
        {            
            var type = (Type)data;
            return Get(type);
        }

        public BatchConfig Get<TType>()
        {
            return Get(typeof(TType));
        }

        public IEnumerable<KeyValuePair<Type, BatchConfig>> GetAll()
        {
            return _configBatches;
        }

        public IEnumerable<KeyValuePair<Type, BatchConfig>> Find(Func<KeyValuePair<Type, BatchConfig>, bool> predicate)
        {
            return _configBatches.Where(predicate);
        }
    }
}
