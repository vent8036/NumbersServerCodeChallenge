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
    public class ReportTests
    {
        [Test]
        public void ShouldReportAllTotals()
        {
            const int uniqueValues = 1;
            const int duplicateValues = 2;

            GivenUniqueValues(uniqueValues);
            GivenDuplicateValues(duplicateValues);

            WhenGeneratingReport();

            ThenReportShouldContain(uniqueValues, duplicateValues, uniqueValues);
        }

        [Test]
        public void ShouldResetTotalsAfterReportingTotals()
        {
            const int initialUniqueValues = 3;
            const int initialDuplicateValues = 3;
            const int uniqueValuesAfterReport = 2;
            const int duplicateValuesAfterReport = 2;

            GivenUniqueValues(initialUniqueValues);
            GivenDuplicateValues(initialDuplicateValues);
            WhenGeneratingReport();
            GivenUniqueValues(uniqueValuesAfterReport);
            GivenDuplicateValues(duplicateValuesAfterReport);

            WhenGeneratingReport();

            ThenReportShouldContain(uniqueValuesAfterReport, duplicateValuesAfterReport, uniqueValuesAfterReport + initialUniqueValues);
        }

        [SetUp]
        public void SetUp()
        {
            _report = new Report();
        }

        private void GivenUniqueValues(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _report.ReceivedUniqueValue();
            }
        }

        private void GivenDuplicateValues(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _report.ReceivedDuplicateValue();
            }
        }

        private void WhenGeneratingReport()
        {
            _lastReport = _report.GenerateReport();
        }

        private void ThenReportShouldContain(int uniquesSinceLastReport, int duplicatesSinceLastReport, int totalUniques)
        {
            _lastReport.Should().Be($"Received {uniquesSinceLastReport} unique number(s), {duplicatesSinceLastReport} duplicate(s). Unique total: {totalUniques}");
        }

        private Report _report;
        private string _lastReport;
    }
}
