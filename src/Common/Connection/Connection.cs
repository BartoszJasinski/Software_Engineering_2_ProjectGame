using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Connection.EventArg;

namespace Common.Connection
{
    public class Connection : IConnection
    {
        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);
        private static ManualResetEvent disconnectDone =
            new ManualResetEvent(false);

        private string ipString;
        private int port;

        public event EventHandler<ConnectEventArgs> OnConnection;
        public event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;
        public event EventHandler<MessageSendEventArgs> OnMessageSend;

        // The response from the remote device.
        private static String response = String.Empty;

        public Connection(string ipString, int port)
        {
            this.ipString = ipString;
            this.port = port;
        }

        public void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipString), port);

                // Create a TCP/IP socket.
                Socket client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                //// Send test data to the remote device.
                //Send(client, "Message" + (char)0x23);
                //sendDone.WaitOne();

                disconnectDone.WaitOne();

                // Release the socket.

                Console.WriteLine("Finished connection with {0}", client.GetRemoteAddress().ToString());
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                

              


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void StopClient()
        {
            disconnectDone.Set();
        }

        public void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                //Console.WriteLine("Socket connected to {0}",
                //    client.RemoteEndPoint.ToString());

                //inform about a connection
                OnConnection(this, new ConnectEventArgs(client));

                // Signal that the connection has been made.
                connectDone.Set();

                // Receive the response from the remote device.
                Receive(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the client socket 
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            try
            {
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    var content = state.sb.ToString();

                    //messages end with <ETB> (0x23)
                    if (content.IndexOf((char)0x23) > -1)
                    {
                        //inform that a new message was received
                        OnMessageRecieve(this, new MessageRecieveEventArgs(content, client));
                        state.sb.Clear();
                        receiveDone.Set();
                    }

                }
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (SocketException)
            {
                Console.WriteLine("Server {0} stopped working.", client.GetRemoteAddress().ToString());
                StopClient();
            }

        }

        public void Send(Socket client, string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }



        public void SendFromClient(Socket client, string data)
        {
            // Send test data to the remote device.
            Send(client, data + (char)0x23);
            sendDone.WaitOne();
        }

        public void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                //  Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                //inform that a message was sent
                OnMessageSend(this, new MessageSendEventArgs(client));

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



    }//class
}
