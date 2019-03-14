using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Bulk
{
    public class BulkData<TData>
    {
        internal BulkData(TData[] datas)
        {
            Datas = datas;
        }

        public TData[] Datas { get; }
    }
}
