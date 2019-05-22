using System;
using System.Threading.Tasks;

namespace WorkerDispatcher.Batch
{
    public interface IBatchBindingBuilder<TData>
    {
        IBatchBindingBuilder<TData> MaxCount(int maxCount);

        IBatchBindingBuilder<TData> TimeLimit(TimeSpan time);

        IBatchBindingBuilder<TData> Period(TimeSpan time);

        IBatchBindingBuilder<TData> FlushOnStop(bool flush);

        IBatchBindingBuilder<TData> TriggerCount(int count);

        void Bind(BatchFactoryDelegate<TData> factoryDelegate);

        void Bind(Func<TData[], Task> action);
    }
}
