using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NumbersServer
{
    public interface IReport
    {
        void ReceivedUniqueValue();
        void ReceivedDuplicateValue();
    }

    public class Report : IReport
    {
        public Report()
        {
            _totalUnique = 0;
            _previousUnique = 0;
            _duplicatesSinceLastReport = 0;
        }

        public void ReceivedUniqueValue()
        {
            lock (_lockObject)
            {
                Interlocked.Increment(ref _totalUnique);
            }
        }

        public void ReceivedDuplicateValue()
        {
            lock (_lockObject)
            {
                Interlocked.Increment(ref _duplicatesSinceLastReport);
            }
        }

        public string GenerateReport()
        {
            lock (_lockObject)
            {
                var uniquesSinceLastReport = _totalUnique - _previousUnique;

                var report = $"Received {uniquesSinceLastReport} unique number(s), {_duplicatesSinceLastReport} duplicate(s). Unique total: {_totalUnique}";

                _previousUnique = _totalUnique;
                _duplicatesSinceLastReport = 0;

                return report;
            }
        }

        private int _totalUnique;
        private int _previousUnique;
        private int _duplicatesSinceLastReport;
        private readonly object _lockObject = new object();
    }
}
