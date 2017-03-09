using Common.EventArg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Connection
{
    //TODO add send callback maybe
    public class ConnectionEndpoint : IConnectionEndpoint
    {
        public int Port { get; set; }

        private ManualResetEvent resetEvent;

        public event EventHandler<ConnectEventArgs> OnConnect;
        public event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;

        public ConnectionEndpoint(int port)
        {
            Port = port;
            resetEvent = new ManualResetEvent(false);
        }

        public void Listen()
        {
            //for empty string GetHostEntry returns hostEntry for local machine
            IPHostEntry iphostEntry = Dns.GetHostEntry(string.Empty);
            //Find local endpoint, the last is in lan, the first for internet
            IPAddress address = iphostEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).Last();
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

            string address = (handler.RemoteEndPoint as IPEndPoint).Address.ToString();
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

            int numberOfBytesRead = handler.EndReceive(asyncResult);

            if (numberOfBytesRead > 0)
            {
                state.StringBuilder.Append(Encoding.ASCII.GetString(state.buffer, 0, numberOfBytesRead));

                content = state.StringBuilder.ToString();
                //messages end with <ETB> (0x23)
                if (content.IndexOf((char)0x23) > -1)
                {
                    //inform that a new message was received
                    OnMessageRecieve(this, new MessageRecieveEventArgs(content, handler));
                }
                else
                {
                    //not everything was received
                    handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(readCallback), state);
                }
            }
        }
    }
}
