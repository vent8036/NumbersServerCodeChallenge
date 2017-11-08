using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersServer
{
    public interface IDuplicateNumberTracker
    {
        bool TrackIfNotDuplicate(int numberToTrack);
    }

    public class DuplicateNumberTracker : IDuplicateNumberTracker
    {
        public DuplicateNumberTracker(int totalPossibleNumbers)
        {
            _receivedNumbers = new BitArray(totalPossibleNumbers);
        }

        public bool TrackIfNotDuplicate(int numberToTrack)
        {
            lock (_lockObject)
            {
                var numberIsDuplicate = _receivedNumbers.Get(numberToTrack);

                if (!numberIsDuplicate)
                {
                    _receivedNumbers.Set(numberToTrack, true);
                }

                return !numberIsDuplicate;
            }
        }

        private BitArray _receivedNumbers;
        private readonly object _lockObject = new object();
    }
}
