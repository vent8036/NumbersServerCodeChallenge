using FluentAssertions;
using NumbersServer;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace NumbersServerTests
{
    [TestFixture]
    public class LoggerTests
    {
        [Test]
        public void ShouldWriteToFile()
        {
            string logFile = _currentDirectory + "testLog.txt";

            using (var logger = new Logger(logFile))
            {
                logger.AppendNumberToLog(1);
            }

            var text = File.ReadAllText(logFile);
            text.Should().Be($"1{Environment.NewLine}");
        }

        [Test]
        public void ShouldWriteMultipleLines()
        {
            string logFile = _currentDirectory + "testLog.txt";

            using (var logger = new Logger(logFile))
            {
                logger.AppendNumberToLog(1);
                logger.AppendNumberToLog(2);
            }

            var text = File.ReadAllText(logFile);
            text.Should().Be($"1{Environment.NewLine}2{Environment.NewLine}");
        }

        [Test]
        public void ShouldTruncateLog()
        {
            string logFile = _currentDirectory + "testLog.txt";

            using (var logger = new Logger(logFile))
            {
                logger.AppendNumberToLog(1);
            }

            using (var logger = new Logger(logFile))
            {
                logger.AppendNumberToLog(2);
            }

            var text = File.ReadAllText(logFile);
            text.Should().Be($"2{Environment.NewLine}");
        }
        
        [SetUp]
        public void SetUp()
        {
            _currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
        }

        private string _currentDirectory;
    }
}
