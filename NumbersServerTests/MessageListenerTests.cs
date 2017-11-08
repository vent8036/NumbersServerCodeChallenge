using FluentAssertions;
using NumbersServer;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NumbersServerTests
{
    [TestFixture]
    public class MessageListenerTests
    {
        [TestCase("")]
        [TestCase("a")]
        [TestCase("0123456789")]
        [TestCase("123456789")]
        [TestCase("terminate")]
        [TestCase("Terminate\r\n")] //Hard-coded newline to simplify testing and focus on other things
        public void ShouldReturnFalseWithBadMessages(string givenMessage)
        {
            WhenProcessingMessage(givenMessage);
            ThenResult.Should().BeFalse();
        }

        [TestCase("012345678\r\n")]
        [TestCase("000000000\r\n")]
        [TestCase("123456789\r\n")]
        [TestCase("terminate\r\n")]
        public void ShouldReturnTrueForValidMessages(string givenMessage)
        {
            WhenProcessingMessage(givenMessage);
            ThenResult.Should().BeTrue();
        }

        [TestCase("012345678\r\n", 12345678)]
        [TestCase("000000000\r\n", 0)]
        [TestCase("123456789\r\n", 123456789)]
        public void ShouldProcessUniqueNumber(string givenMessage, int givenNumber)
        {
            GivenNumberIsNotTrackedYet(givenNumber);
            WhenProcessingMessage(givenMessage);
            ThenProcessedAUniqueNumber(givenNumber);
        }

        [TestCase("012345678\r\n", 12345678)]
        [TestCase("000000000\r\n", 0)]
        [TestCase("123456789\r\n", 123456789)]
        public void ShouldProcessDuplicateNumber(string givenMessage, int givenNumber)
        {
            WhenProcessingMessage(givenMessage);
            ThenProcessedADuplicateNumber(givenNumber);
        }

        [Test]
        public void ShouldTriggerCancellation()
        {
            WhenProcessingMessage("terminate\r\n");
            ThenCancellationIsTriggered();
        }

        [Test]
        public void ShouldGracefullyHandleDisposedCancellationTokenSource()
        {
            GivenCancellationTokenSourceWasDisposed();
            WhenProcessingMessage("terminate\r\n");
            ThenCancellationIsNotTriggered();
        }

        [SetUp]
        public void SetUp()
        {
            _duplicateNumberTracker = MockRepository.GenerateStub<IDuplicateNumberTracker>();
            _report = MockRepository.GenerateStub<IReport>();
            _logger = MockRepository.GenerateStub<INumbersLogger>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _messageListener = new MessageListener(_duplicateNumberTracker, _report, _logger, _cancellationTokenSource);
        }

        [TearDown]
        public void TearDown()
        {
            _cancellationTokenSource?.Dispose();
        }

        private void GivenNumberIsNotTrackedYet(int number)
        {
            _duplicateNumberTracker.Stub(x => x.TrackIfNotDuplicate(number)).Return(true);
        }

        private void GivenCancellationTokenSourceWasDisposed()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        private void WhenProcessingMessage(string message)
        {
            ThenResult = _messageListener.ProcessMessage(message);
        }

        private void ThenProcessedAUniqueNumber(int number)
        {
            _report.AssertWasCalled(x => x.ReceivedUniqueValue());
            _report.AssertWasNotCalled(x => x.ReceivedDuplicateValue());
            _logger.AssertWasCalled(x => x.AppendNumberToLog(number));
        }

        private void ThenProcessedADuplicateNumber(int number)
        {
            _report.AssertWasNotCalled(x => x.ReceivedUniqueValue());
            _report.AssertWasCalled(x => x.ReceivedDuplicateValue());
            _logger.AssertWasNotCalled(x => x.AppendNumberToLog(number));
        }

        private void ThenCancellationIsTriggered()
        {
            _cancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        private void ThenCancellationIsNotTriggered()
        {
            _cancellationToken.IsCancellationRequested.Should().BeFalse();
        }

        protected bool ThenResult { get; set; }

        private IDuplicateNumberTracker _duplicateNumberTracker;
        private IReport _report;
        private INumbersLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private MessageListener _messageListener;
    }
}
