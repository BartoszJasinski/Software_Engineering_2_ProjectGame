using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Connection.EventArg;

namespace Server.Connection
{
  
    public class ConnectionEndpoint : IConnectionEndpoint
    {
        public int Port { get; set; }

        private ManualResetEvent resetEvent;

        public event EventHandler<ConnectEventArgs> OnConnect;
        public event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;
        public event EventHandler<ConnectEventArgs> OnDisconnected;


        public ConnectionEndpoint(int port)
        {
            Port = port;
            resetEvent = new ManualResetEvent(false);
        }

        //is needed to avoid virtualbox ips :(
        private IPAddress getPhysicalIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
                if (addr != null && !addr.Address.ToString().Equals("0.0.0.0"))
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void Listen()
        {
            //for empty string GetHostEntry returns hostEntry for local machine
            IPHostEntry iphostEntry = Dns.GetHostEntry(string.Empty);
            //Find local endpoint, the last is in lan, the first for internet
            var test = iphostEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork);
            IPAddress address = getPhysicalIp();
            IPEndPoint localEndPoint = new IPEndPoint(address, Port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    //event is not signaled
                    resetEvent.Reset();

                    Console.WriteLine("Listening on {0} / {1}", address.ToString(), Port);
                    listener.BeginAccept(new AsyncCallback(acceptCallback), listener);
                    //wait until a connecting is established before continnuing
                    resetEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void acceptCallback(IAsyncResult asyncResult)
        {
            //the main thread can continue (can accept new clients)
            resetEvent.Set();

            Socket listener = (Socket)asyncResult.AsyncState;
            Socket handler = listener.EndAccept(asyncResult);

            string address = handler.GetRemoteAddress().ToString();
            //inform that a new client connected
            OnConnect(this, new ConnectEventArgs(handler));

            StateObject state = new StateObject();
            state.Socket = handler;

            handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(readCallback), state);
        }

        private void readCallback(IAsyncResult asyncResult)
        {
            string content = string.Empty;

            StateObject state = (StateObject)asyncResult.AsyncState;
            Socket handler = state.Socket;

            try
            {
                int numberOfBytesRead = handler.EndReceive(asyncResult);

                if (numberOfBytesRead > 0)
                {
                    state.StringBuilder.Append(Encoding.ASCII.GetString(state.buffer, 0, numberOfBytesRead));

                    content = state.StringBuilder.ToString();
                    //messages end with <ETB> (0x23)
                    int etbIndex = content.IndexOf((char)23);
                    if (etbIndex > -1)
                    {
                        //inform that a new message was received
                        OnMessageRecieve(this, new MessageRecieveEventArgs(content.Substring(0, etbIndex), handler));
                        state.StringBuilder.Remove(0, etbIndex + 1);
                    }
                }

                handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(readCallback), state);
            }
            catch(SocketException e)
            {
                Console.WriteLine("Client {0} disconnected", handler.GetRemoteAddress().ToString());
                OnDisconnected(this, new ConnectEventArgs(handler));
            }
        }

        public void SendFromServer(Socket handler, string message)
        {
            send(handler, message + (char)23);
        }

        private void send(Socket handler, string message)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(message);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(sendCallback), handler);
        }

        private void sendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        
    }
}
