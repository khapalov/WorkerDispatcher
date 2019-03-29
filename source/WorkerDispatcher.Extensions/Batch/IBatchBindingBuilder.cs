using System;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchBindingBuilder<TData>
    {
        IBatchBindingBuilder<TData> MaxCount(int maxCount);

        IBatchBindingBuilder<TData> TimeLimit(TimeSpan time);

        IBatchBindingBuilder<TData> Period(TimeSpan time);   
        
        void Bind(BatchFactoryDelegate<TData> factoryDelegate);

        void Bind(Func<TData[], Task> action);
    }
}
