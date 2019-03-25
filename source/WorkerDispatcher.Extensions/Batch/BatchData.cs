using System.Linq;

namespace WorkerDispatcher.Extensions.Batch
{
    public class BatchData<TData>
    {

        /*public BatchData(TData[] datas)
        {
            Datas = datas;
        }*/

        public BatchData(object[] datas)
        {
            Datas = datas.Cast<TData>().ToArray();
        }

        public TData[] Datas { get; }
    }
}