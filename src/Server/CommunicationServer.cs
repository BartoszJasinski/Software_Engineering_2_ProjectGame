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
using Common.Connection.EventArg;
using Common.Schema;
using Server.Connection;
using Server.Game;

namespace Server
{
    public class CommunicationServer
    {
        private IConnectionEndpoint connectionEndpoint;
        private IGamesContainer registeredGames;
        public CommunicationServer(IConnectionEndpoint connectionEndpoint)
        {
            this.connectionEndpoint = connectionEndpoint;
            registeredGames = new GamesContainer();
            connectionEndpoint.OnConnect += OnClientConnect;
            connectionEndpoint.OnMessageRecieve += OnMessage;
        }

        public void Start()
        {
            connectionEndpoint.Listen();
        }

        private void OnClientConnect(object sender, ConnectEventArgs eventArgs)
        {
            //TODO extension method to get address from socket
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            Console.WriteLine("New client connected with address {0}", address.ToString());
        }

        private void OnMessage(object sender, MessageRecieveEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            Console.WriteLine("New message from {0}: {1}", address, eventArgs.Message);
            connectionEndpoint.SendFromServer(eventArgs.Handler, "Answer!");
        }

        private void OnRegisterGame(object sender, MessageRecieveEventArgs eventArgs)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RegisterGame));
            try
            {
                RegisterGame request;
                using (TextReader reader = new StringReader(eventArgs.Message))
                {
                    request = (RegisterGame)serializer.Deserialize(reader);
                }
                if (request == null)
                    return;

                Game.Game g = new Game.Game(gameId: registeredGames.NextGameId(), name: request.NewGameInfo.name,
                    bluePlayers: (int)request.NewGameInfo.blueTeamPlayers, redPlayers: (int)request.NewGameInfo.redTeamPlayers);

                registeredGames.RegisterGame(g);
                ConfirmGameRegistration gameRegistration = new ConfirmGameRegistration() { gameId = (ulong)g.Id };

                var response = Serialize(gameRegistration);
                connectionEndpoint.SendFromServer(eventArgs.Handler, response);
            }
            catch
            {
                // TODO add some logger
            }
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
