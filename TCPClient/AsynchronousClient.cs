using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPClient
{
    public class AsynchronousClient
    {
        private Socket _client;
        private Thread _keepAlive;

        private int _connectionRetryCount;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);

        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);

        public static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.  
        private string _response = String.Empty;

        public string Response => _response;

        public void StartClient(string address, int port)
        {
            // Connect to a remote device.  
            try
            {
                _connectionRetryCount = 0;
                // Establish the remote endpoint for the socket.  
                if (!IPAddress.TryParse(address, out var ipAddress))
                {
                    throw new ArgumentException("IP Address is invalid");
                }

                IPEndPoint remoteEp = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                _client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _client.BeginConnect(remoteEp,
                    new AsyncCallback(ConnectCallback), _client);
                connectDone.WaitOne();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket) ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();

                _keepAlive = new Thread(KeepAlive);
                _keepAlive.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ++_connectionRetryCount;
                if (_connectionRetryCount == 5)
                {
                    throw;
                }
            }
        }

        private void Receive()
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = _client;

                // Begin receiving the data from the remote device.  
                _client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject) ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        _response = state.sb.ToString();
                    }

                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            _client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), _client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void KeepAlive()
        {
            while (true)
            {
                Send("{AZ001127");
                Receive();
                receiveDone.WaitOne();
                Console.WriteLine(Response);
                Thread.Sleep(500);
            }
        }
    }
}