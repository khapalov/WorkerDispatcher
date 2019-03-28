using System;

namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchBindingBuilder
    {
        IBatchBindingBuilder MaxCount(int maxCount);

        IBatchBindingBuilder TimeLimit(TimeSpan time);

        IBatchBindingBuilder Period(TimeSpan time);   
        
        void Bind<TData>(BatchFactoryDelegate<TData> factoryDelegate);
    }
}
