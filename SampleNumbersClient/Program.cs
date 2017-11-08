using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleNumbersClient
{
    //This code is not robust at all, and was used to help with testing and debugging
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, cancelEventArgs) =>
                {
                    cancellationTokenSource.Cancel();
                    cancelEventArgs.Cancel = true;
                });

                StartSendingData(cancellationTokenSource.Token);
            }
        }

        public static void StartSendingData(CancellationToken cancellationToken)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 4000);

            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sender.Connect(remoteEP);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sender.Close();
                return;
            }

            Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

            var randomNumberGenerator = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                var number = randomNumberGenerator.Next(0, 1000000000);

                if (!SendMessage(sender, number.ToString("D9")))
                    break;
            }

            SendMessage(sender, "terminate");
 
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public static bool SendMessage(Socket socket, string message)
        {
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(message + Environment.NewLine);
                socket.Send(msg);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}
