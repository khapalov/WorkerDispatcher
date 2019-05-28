using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WorkerDispatcher;
using WorkerDispatcher.Batch;

namespace UnitTests
{
    [TestFixture]
    public abstract class BatchFixture
    {
        protected Mock<IWorkerHandler> MockWorkerHandler = new Mock<IWorkerHandler>();
        protected Mock<IActionInvoker> MockActionInvoker = new Mock<IActionInvoker>();
        protected Mock<IBatchActionInvoker<SomeBatchData>> BatchActionInvoker = new Mock<IBatchActionInvoker<SomeBatchData>>();

        public ActionDispatcherFactory Factory;

        protected IDispatcherToken DispatcherToken;

        public BatchFixture()
        {
            MockActionInvoker.Setup(p => p.Invoke(It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid().ToString());
            Factory = new ActionDispatcherFactory(MockWorkerHandler.Object);

            BatchActionInvoker.Setup(p => p.Invoke(It.IsAny<SomeBatchData[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        }
    }

    public class if_count_batch_execute : BatchFixture
    {
        private IBatchToken _batchToken;
        private int _maxCount = 10;
             
        [SetUp]
        public void Initalize()
        {
            DispatcherToken = Factory.Start(new ActionDispatcherSettings
            {
                Timeout = TimeSpan.FromSeconds(1000)
            });

            _batchToken = DispatcherToken.Plugin.Batch(p =>
            {
                p.For<SomeBatchData>()
                    .MaxCount(_maxCount)
                    .TriggerCount(1)
                    .Bind(() => BatchActionInvoker.Object);
            }).Start();
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(10)]
        public void should_be_invoke_batch_count_success(int count)
        {
            BatchActionInvoker.Invocations.Clear();

            Enumerable.Range(0, count).ToList().ForEach(p =>
            {
                var data = new SomeBatchData();
                _batchToken.Send(data);
            });

            _batchToken.Stop();

            DispatcherToken.WaitCompleted();

            BatchActionInvoker.Verify(p => p.Invoke(It.Is<SomeBatchData[]>(x => x.Length == count), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void execute_batch_count_exceeded()
        {
            var exceeded = 2;
            var count = _maxCount * exceeded;
                
            BatchActionInvoker.Invocations.Clear();

            Enumerable.Range(0, count).ToList().ForEach(p =>
            {
                var data = new SomeBatchData();
                _batchToken.Send(data);
            });

            _batchToken.Stop();

            DispatcherToken.WaitCompleted();

            BatchActionInvoker.Verify(p => p.Invoke(It.Is<SomeBatchData[]>(x => x.Length == _maxCount), It.IsAny<CancellationToken>()), Times.Exactly(exceeded));
        }
    }

    public class SomeBatchData
    {
        public string Name { get; set; }
    }
}
