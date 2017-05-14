using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Serialization;
using Common;
using Common.Connection.EventArg;
using Server.Connection;
using Common.Message;
using Server.Game;
using Common.Config;
using Common.DebugUtils;
using Common.Schema;

namespace Server
{

    public class CommunicationServer
    {
        public IConnectionEndpoint ConnectionEndpoint;
        public IGamesContainer RegisteredGames;
        public Dictionary<ulong, Socket> Clients;
        private List<ulong> freeIdList;
        public IList<string> startedGames;

        private CommunicationServerSettings settings;

        public CommunicationServer(IConnectionEndpoint connectionEndpoint, CommunicationServerSettings settings)
        {
            this.ConnectionEndpoint = connectionEndpoint;
            RegisteredGames = new GamesContainer();
            this.settings = settings;

            connectionEndpoint.OnConnect += OnClientConnect;
            connectionEndpoint.OnMessageRecieve += OnMessage;
            connectionEndpoint.OnDisconnected += OnDisconnect;
            Clients = new Dictionary<ulong, Socket>();
            freeIdList = new List<ulong>();
            startedGames = new List<string>();

        }

        private void OnDisconnect(object sender, ConnectEventArgs e)
        {
            RegisteredGames.RemoveGameMastersGames(e.Handler);

            
            // if it was a player, tell gm about it
            try
            {
                ulong playerId = Clients.First(pair => pair.Value == e.Handler).Key;
                ConsoleDebug.Message($"ID: {playerId}");



                IEnumerable<IGame> abandonedGames = RegisteredGames
                    .Where(game => game.Players.Any(player => player.Id == playerId));
                foreach (var game in abandonedGames)
                {
                    ConsoleDebug.Message($"Informing game master of game {game.Id} about disconnected player, id: {playerId}");
                    InformGameMasterAboutDisconnectedPlayer(game.GameMaster, playerId);

                    // remove player id occurrence
                    game.Players.Remove(game.Players.First(player => playerId == player.Id));
                }
            }
            catch (Exception exception)
            {
                // ignore: it was not a player
                Console.WriteLine(exception);
            }

        }

        private void InformGameMasterAboutDisconnectedPlayer(Socket gameMasterSocket, ulong playerId)
        {
            var msg = new PlayerDisconnected { playerId = playerId };

            ConnectionEndpoint.SendFromServer(gameMasterSocket, XmlMessageConverter.ToXml(msg));
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

            try
            {
                if (eventArgs.Message.Length == 0) //ETB keep alive packet
                {
                    //send keep alive back
                    ConnectionEndpoint.SendFromServer(eventArgs.Handler, String.Empty);
                }
                else
                {
                    BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this,
                        eventArgs.Handler);
                }
            }
            catch(Exception e)
            {
                ConsoleDebug.Error(e.Message);
            }

            //ConnectionEndpoint.SendFromServer(eventArgs.Handler, eventArgs.Message);

        }


        public ulong IdForNewClient()
        {
            ulong id;

            if (freeIdList.Count == 0)
            {
                id = (ulong)Clients.Count;
                Console.WriteLine(id);
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
