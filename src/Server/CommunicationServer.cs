using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common;
using Common.Connection.EventArg;
using Common.Schema;
using Server.Connection;
using Common.Message;
using Server.Game;
using Common.Config;

namespace Server
{

    public class CommunicationServer
    {
        public IConnectionEndpoint ConnectionEndpoint;
        public IGamesContainer RegisteredGames;
        public Dictionary<ulong, Socket> Clients;
        private List<ulong> freeIdList;

        private CommunicationServerSettings settings;

        public CommunicationServer(IConnectionEndpoint connectionEndpoint, CommunicationServerSettings settings)
        {
            this.ConnectionEndpoint = connectionEndpoint;
            RegisteredGames = new GamesContainer();
            this.settings = settings;

            connectionEndpoint.OnConnect += OnClientConnect;
            connectionEndpoint.OnMessageRecieve += OnMessage;
            Clients = new Dictionary<ulong, Socket>();
            freeIdList = new List<ulong>();
        }

        public void Start()
        {
            ConnectionEndpoint.Listen();
        }

        private void OnClientConnect(object sender, ConnectEventArgs eventArgs)
        {

            var address = eventArgs.Handler.GetRemoteAddress();
            Console.WriteLine("New client connected with address {0}", address.ToString());
        }

        private void OnMessage(object sender, MessageRecieveEventArgs eventArgs)
        {

            var address = eventArgs.Handler.GetRemoteAddress();
            if (address != null)
                Console.WriteLine("New message from {0}: {1}", address, eventArgs.Message);
            BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this,
                eventArgs.Handler);

            //ConnectionEndpoint.SendFromServer(eventArgs.Handler, eventArgs.Message);

        }


        public  ulong IdForNewClient()
        {
            ulong id;

            if (freeIdList.Count == 0)
            {
                id = (ulong)Clients.Count;
            }
            else
            {
                id = freeIdList[freeIdList.Count - 1];
                freeIdList.RemoveAt(freeIdList.Count - 1);
            }
            return id;
        }
        private static string Serialize<T>(T gameRegistration)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            string response;
            using (TextWriter writer = new StringWriter())
            {
                ser.Serialize(writer, gameRegistration);
                response = writer.ToString();
            }
            return response;
        }
    }
}
