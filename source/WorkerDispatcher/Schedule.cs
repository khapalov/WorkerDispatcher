using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public enum ScheduleType
    {
        /// <summary>
        /// All worker executed as parallel
        /// </summary>
        Parallel,

        /// <summary>
        /// All work is done sequentially
        /// </summary>
        Sequenced,

    }
}
