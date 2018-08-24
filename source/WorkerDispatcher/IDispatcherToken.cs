using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IDispatcherToken : IDispatcherTokenSender
    {		
		Task Stop(int delaySeconds = 30);
    }
}