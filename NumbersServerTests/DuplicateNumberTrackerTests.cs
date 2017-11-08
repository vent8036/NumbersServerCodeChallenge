using FluentAssertions;
using NumbersServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersServerTests
{
    [TestFixture]
    public class DuplicateNumberTrackerTests
    {
        [Test]
        public void NumberIsADuplicate()
        {
            WhenTrackingNumber(999);
            ThenIsTracked.Should().BeTrue();
        }

        [Test]
        public void NumberIsNotADuplicate()
        {
            WhenTrackingNumber(3);
            ThenIsTracked.Should().BeTrue();
            WhenTrackingNumber(3);
            ThenIsTracked.Should().BeFalse();
        }

        [SetUp]
        public void SetUp()
        {
            _duplicateNumberTracker = new DuplicateNumberTracker(1000);
        }

        private void WhenTrackingNumber(int number)
        {
            ThenIsTracked = _duplicateNumberTracker.TrackIfNotDuplicate(number);
        }

        protected bool ThenIsTracked { get; set; }

        private DuplicateNumberTracker _duplicateNumberTracker;
    }
}
