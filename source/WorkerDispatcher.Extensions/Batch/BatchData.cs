using System.Linq;

namespace WorkerDispatcher.Extensions.Batch
{
    public class BatchData<TData>
    {
        public BatchData(object[] datas)
        {
            Datas = datas.Cast<TData>().ToArray();
        }

        public TData[] Datas { get; }
    }
}