﻿using System;
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

namespace Server
{

    public class CommunicationServer
    {
        public IConnectionEndpoint ConnectionEndpoint;
        public IGamesContainer RegisteredGames;
        private Dictionary<ulong, Socket> clients;
        private List<ulong> freeIdList;

        public CommunicationServer(IConnectionEndpoint connectionEndpoint)
        {
            this.ConnectionEndpoint = connectionEndpoint;
            RegisteredGames = new GamesContainer();
            connectionEndpoint.OnConnect += OnClientConnect;
            connectionEndpoint.OnMessageRecieve += OnMessage;
            clients = new Dictionary<ulong, Socket>();
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

        public void OnRegisterGame(object sender, MessageRecieveEventArgs eventArgs)
                {
            RegisterGame request = (RegisterGame) XmlMessageConverter.ToObject(eventArgs.Message);

                if (request == null)
                    return;

            Game.Game g = new Game.Game(gameId: RegisteredGames.NextGameId(), name: request.NewGameInfo.gameName,
                bluePlayers: request.NewGameInfo.blueTeamPlayers,
                redPlayers: request.NewGameInfo.redTeamPlayers
                );

            RegisteredGames.RegisterGame(g);

            ConfirmGameRegistration gameRegistration = new ConfirmGameRegistration() {gameId = (ulong) g.Id};

            var response = XmlMessageConverter.ToXml(gameRegistration);
            ConnectionEndpoint.SendFromServer(eventArgs.Handler, response);
            }

        public void OnJoiningGame(object sender, MessageRecieveEventArgs eventArgs)
        {
            JoinGame request = (JoinGame)XmlMessageConverter.ToObject(eventArgs.Message);

            if (request == null)
                return;

            Game.IGame g = RegisteredGames.GetGameByName(request.gameName);

            request.playerId = IdForNewClient();
            clients.Add(request.playerId, eventArgs.Handler);

            var response = XmlMessageConverter.ToXml(request);
            ConnectionEndpoint.SendFromServer(g.GameMaster, response);
        }

        public void OnConfirmJoiningGame(object sender, MessageRecieveEventArgs eventArgs)
        {
            ConfirmJoiningGame request = (ConfirmJoiningGame)XmlMessageConverter.ToObject(eventArgs.Message);

            if (request == null)
                return;

            Game.IGame g = RegisteredGames.GetGameById((int)request.gameId);
            var response = XmlMessageConverter.ToXml(request);
            ConnectionEndpoint.SendFromServer(clients[request.playerId], response);
        }

        private  ulong IdForNewClient()
        {
            ulong id;

            if (freeIdList.Count == 0)
                id = (ulong)clients.Count;

           id = freeIdList[freeIdList.Count - 1];
            freeIdList.RemoveAt(freeIdList.Count - 1);

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
