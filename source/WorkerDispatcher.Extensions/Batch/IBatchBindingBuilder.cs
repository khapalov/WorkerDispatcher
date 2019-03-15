using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchBindingBuilder
    {
        IBatchBindingBuilder MaxCount(int maxCount);

        IBatchBindingBuilder TimeLimit(TimeSpan time);

        IBatchBindingBuilder AwaitTimePeriod(TimeSpan time);   
        
        void Bind<TData>(BatchDelegate<TData> factoryDelegate);
    }
}
