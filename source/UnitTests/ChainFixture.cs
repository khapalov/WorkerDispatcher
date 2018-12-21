using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher;

namespace UnitTests
{
    [TestFixture]
    public abstract class ChainFixture
    {
        protected Mock<IWorkerHandler> MockWorkerHandler = new Mock<IWorkerHandler>();
        protected Mock<IActionInvoker> MockActionInvoker = new Mock<IActionInvoker>();

        public ActionDispatcherFactory Factory;

        protected IDispatcherToken DispatcherToken;

        public ChainFixture()
        {
            Factory = new ActionDispatcherFactory(MockWorkerHandler.Object);
        }
    }

    public class if_run_chain_through_run : ChainFixture
    {
        Mock<IActionInvoker<WorkerCompletedData>> MockCompleted = new Mock<IActionInvoker<WorkerCompletedData>>();

        [SetUp]
        public void Initalize()
        {
            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(1000)
            });

            MockCompleted.Invocations.Clear();
            MockActionInvoker.Invocations.Clear();

            DispatcherToken.Chain()
                .Post(MockActionInvoker.Object)
                .Post(MockActionInvoker.Object)
                .Run(MockCompleted.Object);
        }

        [Test]
        public void should_be_invoke_completed()
        {
            //await when worker complete is execute
            Task.Delay(50).Wait();

            DispatcherToken.WaitCompleted();

            MockCompleted.Verify(p => p.Invoke(It.IsAny<WorkerCompletedData>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void should_be_invoke_chain_workers()
        {
            //await when worker complete is execute
            Task.Delay(50).Wait();

            DispatcherToken.WaitCompleted();

            MockActionInvoker.Verify(p => p.Invoke(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }

    public class if_run_chain_through_run_callback : ChainFixture
    {
        Mock<IActionInvoker<WorkerCompletedData>> MockCompleted = new Mock<IActionInvoker<WorkerCompletedData>>();

        Action<WorkerCompletedData> Callback;

        [SetUp]
        public void Initalize()
        {
            Callback = (WorkerCompletedData p) => MockCompleted.Object.Invoke(p, CancellationToken.None);

            MockCompleted.Setup(p => p.Invoke(It.IsAny<WorkerCompletedData>(), It.IsAny<CancellationToken>()));

            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(1000)
            });

            MockActionInvoker.Invocations.Clear();
            MockCompleted.Invocations.Clear();

            MockActionInvoker.Setup(p => p.Invoke(It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

            DispatcherToken.Chain()
                .Post(MockActionInvoker.Object)
                .Post(MockActionInvoker.Object)
                .Run(Callback);
        }

        [Test]
        public void should_be_invoke_callback()
        {
            DispatcherToken.WaitCompleted();

            MockCompleted.Verify(p => p.Invoke(It.IsAny<WorkerCompletedData>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void should_be_invoke_chain_workers()
        {
            DispatcherToken.WaitCompleted();

            MockActionInvoker.Verify(p => p.Invoke(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }

    public class if_run_chain_through_run_sync : ChainFixture
    {
        IWorkerChain Chain;

        [SetUp]
        public void Initalize()
        {
            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(1000)
            });

            MockActionInvoker.Invocations.Clear();

            Chain = DispatcherToken.Chain()
                .Post(MockActionInvoker.Object)
                .Post(MockActionInvoker.Object);
        }

        [Test]
        public void should_be_invoke_sync_success()
        {
            var result = Chain.RunSync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Results);
            Assert.IsTrue(result.Results.All(p => !p.IsError));

            DispatcherToken.WaitCompleted();
        }

        [Test]
        public void should_be_invoke_chain_workers()
        {
            var result = Chain.RunSync();

            DispatcherToken.WaitCompleted();

            MockActionInvoker.Verify(p => p.Invoke(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }

    public class if_run_chain_through_run_async : ChainFixture
    {
        IWorkerChain Chain;

        [SetUp]
        public void Initalize()
        {
            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(1000)
            });

            MockActionInvoker.Invocations.Clear();

            Chain = DispatcherToken.Chain()
                .Post(MockActionInvoker.Object)
                .Post(MockActionInvoker.Object);
        }

        [Test]
        public async Task should_be_invoke_async_success()
        {
            var result = await Chain.RunAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Results);
            Assert.IsTrue(result.Results.All(p => !p.IsError));

            DispatcherToken.WaitCompleted();
        }


        [Test]
        public async Task should_be_invoke_chain_workers()
        {
            var result = await Chain.RunAsync();

            DispatcherToken.WaitCompleted();

            MockActionInvoker.Verify(p => p.Invoke(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }

    public class if_run_chain_with_worker_error : ChainFixture
    {
        IWorkerChain Chain;
        Mock<IActionInvoker> MockActionInvokerError = new Mock<IActionInvoker>();

        [SetUp]
        public void Initalize()
        {
            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(1000)
            });

            MockActionInvoker.Invocations.Clear();

            MockActionInvokerError.Setup(p => p.Invoke(It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException("test"));

            Chain = DispatcherToken.Chain()
                .Post(MockActionInvoker.Object)
                .Post(MockActionInvoker.Object)
                .Post(MockActionInvokerError.Object);
        }

        [Test]
        public async Task should_be_invoke_async_item_error()
        {
            var result = await Chain.RunAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Results);
            Assert.NotNull(result.Results.Single(p => p.IsError));
            Assert.IsTrue(result.Results.Any(p => !p.IsError));

            DispatcherToken.WaitCompleted();
        }

        [Test]
        public void should_be_invoke_sync_item_error()
        {
            var result = Chain.RunSync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Results);
            Assert.NotNull(result.Results.Single(p => p.IsError));
            Assert.IsTrue(result.Results.Any(p => !p.IsError));

            DispatcherToken.WaitCompleted();
        }
    }

}
