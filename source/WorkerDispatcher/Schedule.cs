using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public enum ScheduleType
    {
        /// <summary>
        /// all tasks are called in parallel
        /// </summary>
        Parallel,

        /// <summary>
        /// All tasks are called sequentially
        /// </summary>
        Sequenced,

        /// <summary>
        /// All tasks are called in parallel with the restriction on the number of threads
        /// </summary>
        ParallelLimit
    }
}
