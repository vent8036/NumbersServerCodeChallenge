using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NumbersServer
{
    // Much of this socket code was taken from the MSDN website
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder(BufferSize);
    }

    public class SocketServer
    {
        public SocketServer(IMessageListener messageListener, CancellationToken cancellationToken)
        {
            _messageListener = messageListener;
            _cancellationToken = cancellationToken;
        }

        public void CreateAndListenToSocket(int portNumber)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, portNumber);
            
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            listener.Bind(localEndPoint);
            listener.Listen(0 /*No backlog to make 5 connection max easier to validate*/); 

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _connectionSemaphore.Wait(_cancellationToken);
                }
                catch (Exception)
                {
                    //Cancellation was requested so just exit
                    return;
                }
                
                // Start an asynchronous socket to listen for connections.  
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }

            ReleaseSocket(listener, false);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                if (_cancellationToken.IsCancellationRequested)
                {
                    ReleaseSocket(handler);
                    return;
                }

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                _connectionSemaphore.Release();
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;
                
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                if (_cancellationToken.IsCancellationRequested)
                {
                    ReleaseSocket(handler);
                }
                
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead <= 0)
                {
                    ReleaseSocket(handler);

                    return;
                }

                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                content = state.sb.ToString();
                if (content.Length >= ExpectedMessageSize || content.Contains(Environment.NewLine))
                {
                    var charactersProcessed = ProcessReceivedData(content);

                    if (charactersProcessed <= 0)
                    {
                        ReleaseSocket(handler);

                        return;
                    }

                    state.sb.Remove(0, charactersProcessed);
                }

                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                _connectionSemaphore.Release();
            }
        }

        //If the return value is >0 then we successfully processed a message, otherwise the message was invalid
        private int ProcessReceivedData(string receivedData)
        {
            int charactersProcessed = 0;

            while (receivedData.Length >= (ExpectedMessageSize + charactersProcessed))
            {
                var rawMessage = receivedData.Substring(charactersProcessed, ExpectedMessageSize);
                bool processedMessage = _messageListener.ProcessMessage(rawMessage);
                
                if (!processedMessage || _cancellationToken.IsCancellationRequested)
                {
                    return -1; 
                }

                charactersProcessed += ExpectedMessageSize;
            }

            return charactersProcessed;
        }

        private void ReleaseSocket(Socket socket, bool isWorkerSocket = true)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception)
            {
                //Just swallow any errors when shutting down
            }
            finally
            {
                if (isWorkerSocket)
                    _connectionSemaphore.Release();
            }
        }
        
        private static readonly int ExpectedMessageSize = 9 + Environment.NewLine.Length; 
        private const int MaxConnections = 5;
        private SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(MaxConnections, MaxConnections); //Used to limit the number of simultaneous connections
        private readonly IMessageListener _messageListener;
        private readonly CancellationToken _cancellationToken;
    }
}
