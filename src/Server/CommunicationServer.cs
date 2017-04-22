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

namespace Server
{

    public class CommunicationServer
    {
        public IConnectionEndpoint ConnectionEndpoint;
        public IGamesContainer RegisteredGames;
        public Dictionary<ulong, Socket> Clients;
        private List<ulong> freeIdList;
        public IDictionary<ulong, ICollection<ulong>> GameIdPlayerIdDictionary;

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
            GameIdPlayerIdDictionary = new Dictionary<ulong, ICollection<ulong>>();

        }

        private void OnDisconnect(object sender, ConnectEventArgs e)
        {
            RegisteredGames.RemoveGameMastersGames(e.Handler);

            
            // if it was a player, tell gm about it
            try
            {
                ulong playerId = Clients.First(pair => pair.Value == e.Handler).Key;
                ConsoleDebug.Message($"ID: {playerId}");
                ICollection<ulong> gameIds = GameIdPlayerIdDictionary
                    .Where(pair => pair.Value.Contains(playerId))
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
                    .Keys;

                ICollection<Socket> gameMasters = new List<Socket>(gameIds.Count);
                foreach (var gameId in gameIds)
                {
                    gameMasters.Add(RegisteredGames.GetGameById((int)gameId).GameMaster);
                    ConsoleDebug.Message($"Informing game master of game {gameId} about disconnected player, id: {playerId}");

                    // remove player id occurrence
                    GameIdPlayerIdDictionary[gameId].Remove(playerId);
                }

                foreach (var gameMaster in gameMasters)
                {
                    InformGameMasterAboutDisconnectedPlayer(gameMaster, playerId);
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
                BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this,
                    eventArgs.Handler);
            }
            catch(Exception e)
            {
                ConsoleDebug.Error(e.Message);
            }

            //ConnectionEndpoint.SendFromServer(eventArgs.Handler, eventArgs.Message);

        }


        public  ulong IdForNewClient()
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
