namespace WorkerDispatcher.Extensions.Batch
{
    public class BatchData<TData>
    {

        internal BatchData(TData[] datas)
        {
            Datas = datas;
        }

        public TData[] Datas { get; }
    }
}