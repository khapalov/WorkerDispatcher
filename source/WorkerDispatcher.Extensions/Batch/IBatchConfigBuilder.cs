using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchConfigBuilder
    {
        IBatchConfigBuilder MaxCount(int maxCount);

        IBatchConfigBuilder TimeLimit(TimeSpan time);

        IBatchConfigBuilder AwaitTimePeriod(TimeSpan time);   
        
        void Bind<TData>(BatchDelegate<TData> factoryDelegate);
    }
}
