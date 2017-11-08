using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NumbersServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var programCancellationTokenSource = new CancellationTokenSource())
            using (var messageListenerCancellationTokenSource = new CancellationTokenSource())
            using (var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(programCancellationTokenSource.Token, messageListenerCancellationTokenSource.Token))
            using (var logger = new Logger(LogFileName))
            {
                var duplicateNumberTracker = new DuplicateNumberTracker(PossibleNumberCount);
                var report = new Report();
                var messageListener = new MessageListener(duplicateNumberTracker, report, logger, messageListenerCancellationTokenSource);
                var linkedCancellationToken = linkedCancellationTokenSource.Token;
                var socketServer = new SocketServer(messageListener, linkedCancellationToken);

                Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, cancelEventArgs) => {
                    programCancellationTokenSource.Cancel();
                    cancelEventArgs.Cancel = true; //Allow graceful shutdown
                });

                StartReportThread(report, linkedCancellationToken);

                socketServer.CreateAndListenToSocket(PortNumber);
            }
        }

        private static void StartReportThread(Report report, CancellationToken cancellationToken)
        {
            var reportThread = new Thread(() => PrintReport(report, cancellationToken));
            reportThread.IsBackground = true; //Allow the thread to terminate when the program exits
            reportThread.Start();
        }

        private static void PrintReport(Report report, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine(report.GenerateReport());
                Thread.Sleep(ReportFrequencyInMilliseconds);
            }
        }

        private const int PossibleNumberCount = 1000000000;
        private const string LogFileName = "numbers.log";
        private const int PortNumber = 4000;
        private const int ReportFrequencyInMilliseconds = 10000; //10s
    }
}
