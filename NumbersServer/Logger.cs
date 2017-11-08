using System;
using System.IO;
using System.Text;

namespace NumbersServer
{
    public interface INumbersLogger
    {
        void AppendNumberToLog(int number);
    }

    public class Logger : INumbersLogger, IDisposable
    {
        public Logger(string fileName)
        {
            //Create a new log file for writing, truncating the existing file if necessary, and allow other processes to read the file
            fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        public void AppendNumberToLog(int number)
        {
            lock (_lockObject)
            {
                var data = Encoding.ASCII.GetBytes($"{number}{Environment.NewLine}");
                fileStream.Write(data, 0, data.Length);
            }
        }

        public void Dispose()
        {
            if (fileStream != null)
            {
                fileStream.Dispose();
                fileStream = null;
            }
        }

        private FileStream fileStream;
        private readonly object _lockObject = new object();
    }
}
