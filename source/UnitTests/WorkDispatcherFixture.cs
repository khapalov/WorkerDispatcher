using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace UnitTests
{
    [TestFixture]
    public abstract class WorkDispatcherFixture
    {
        protected Mock<IWorkerHandler> MockWorkerHandler = new Mock<IWorkerHandler>();
        protected Mock<IActionInvoker> MockActionInvoker = new Mock<IActionInvoker>();

        public ActionDispatcherFactory Factory;        

        public WorkDispatcherFixture()
        {
            Factory = new ActionDispatcherFactory(MockWorkerHandler.Object);
        }
    }

    [TestFixture]
    public class post_some_action : WorkDispatcherFixture
    {
        protected IDispatcherToken DispatcherToken;

        [SetUp]
        public void Initalize()
        {
            MockActionInvoker.Reset();
            MockWorkerHandler.Reset();

            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {                
                Timeout = TimeSpan.FromSeconds(10)
            });

            DispatcherToken.Post(MockActionInvoker.Object);
            DispatcherToken.Post(MockActionInvoker.Object);
            DispatcherToken.Post(p => Task.Delay(100, p));

            DispatcherToken.Stop().Wait();
        }

        [Test]
        public void should_be_two_action_execute()
        {
            MockActionInvoker.Verify(p => p.Invoke(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }

    [TestFixture]
    public class worker_longer : WorkDispatcherFixture
    {
        protected IDispatcherToken DispatcherToken;

        [SetUp]
        public void Initalize()
        {
            MockActionInvoker.Reset();
            MockWorkerHandler.Reset();

            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(1)
            });
            
            DispatcherToken.Post(t => Task.Delay(10000, t));            
        }

        [Test]
        public void should_be_cancelled_work()
        {
            DispatcherToken.Stop().Wait();
            MockWorkerHandler.Verify(p => p.HandleError(null, It.IsAny<decimal>(), It.IsAny<bool>()), Times.Once);
        }
    }

    [TestFixture]
    public class many_action_with_exception : WorkDispatcherFixture
    {
        protected IDispatcherToken DispatcherToken;

        [SetUp]
        public void Initalize()
        {
            MockActionInvoker.Reset();
            MockWorkerHandler.Reset();

            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(10)
            });

            DispatcherToken.Post(MockActionInvoker.Object);
            DispatcherToken.Post(p => throw new ArgumentException());
            DispatcherToken.Post(p => throw new ArgumentException());
            DispatcherToken.Post(MockActionInvoker.Object);

            DispatcherToken.Stop().Wait();
        }

        [Test]
        public void should_be_two_action_error()
        {
            MockWorkerHandler.Verify(p => p.HandleError(It.IsAny<Exception>(), It.IsAny<decimal>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public void should_be_two_action_success()
        {
            MockActionInvoker.Verify(p => p.Invoke(It.IsAny<CancellationToken>()), Times.Exactly(2));
            MockWorkerHandler.Verify(p => p.HandleResult(It.IsAny<object>(), It.IsAny<decimal>()), Times.Exactly(2));
        }
    }   
}
