using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NumbersServer
{
    public interface IMessageListener
    {
        bool ProcessMessage(string rawMessage);
    }

    public class MessageListener : IMessageListener
    {
        public MessageListener(IDuplicateNumberTracker duplicateNumberTracker, IReport report, INumbersLogger logger, CancellationTokenSource cancellationTokenSource)
        {
            _duplicateNumberTracker = duplicateNumberTracker;
            _report = report;
            _logger = logger;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public bool ProcessMessage(string rawMessage)
        {
            if (MessageIsInvalid(rawMessage))
                return false;

            if (rawMessage.StartsWith("terminate"))
            {
                TriggerTerminationSignal();
                return true; 
            }

            var number = Int32.Parse(rawMessage);
            ProcessNumber(number);

            return true;
        }

        private bool MessageIsInvalid(string rawMessage)
        {
            return rawMessage.Length != RequiredMessageLength || !MessageRegex.IsMatch(rawMessage) || !rawMessage.EndsWith(Environment.NewLine);
        }

        private void ProcessNumber(int number)
        {
            if (_duplicateNumberTracker.TrackIfNotDuplicate(number))
            {
                _report.ReceivedUniqueValue();
                _logger.AppendNumberToLog(number);
            }
            else
            {
                _report.ReceivedDuplicateValue();
            }
        }

        private void TriggerTerminationSignal()
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (Exception)
            {
                //The cancellationTokenSource was likely disposed of already because of a different shutdown mechanism
            }
        }

        private IDuplicateNumberTracker _duplicateNumberTracker;
        private IReport _report;
        private INumbersLogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private static Regex MessageRegex = new Regex(@"\d{9}|(terminate)", RegexOptions.Compiled);
        private readonly static int RequiredMessageLength = 9 + Environment.NewLine.Length;
    }
}
