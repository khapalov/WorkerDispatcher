using System;
using System.Threading.Tasks;

namespace WorkerDispatcher.Batch
{
    public interface IBatchBindingBuilder<TData>
    {
        /// <summary>
        /// Max batch count 
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        IBatchBindingBuilder<TData> MaxCount(int maxCount);

        /// <summary>
        /// Time limit batch worker
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        IBatchBindingBuilder<TData> TimeLimit(TimeSpan time);

        /// <summary>
        /// Pediod collect batch
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        IBatchBindingBuilder<TData> Period(TimeSpan time);

        /// <summary>
        /// if ture, after call stop, collect batch, all queued data sending to worker (ignoring max count)
        /// </summary>
        /// <param name="flush"></param>
        /// <returns></returns>
        IBatchBindingBuilder<TData> FlushOnStop(bool flush);

        /// <summary>
        /// If queued data >= count, sending to worker
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        IBatchBindingBuilder<TData> TriggerCount(int count);

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="factoryDelegate"></param>
        void Bind(BatchFactoryDelegate<TData> factoryDelegate);

        void Bind(Func<TData[], Task> action);
    }
}
