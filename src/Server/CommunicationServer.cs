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

namespace Server
{

    public class CommunicationServer
    {
        public IConnectionEndpoint ConnectionEndpoint;
        public IGamesContainer RegisteredGames;

        public CommunicationServer(IConnectionEndpoint connectionEndpoint)
        {
            this.ConnectionEndpoint = connectionEndpoint;
            RegisteredGames = new GamesContainer();
            connectionEndpoint.OnConnect += OnClientConnect;
            connectionEndpoint.OnMessageRecieve += OnMessage;
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
            BehaviorChooser.React((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this,
                eventArgs.Handler);
            //ConnectionEndpoint.SendFromServer(eventArgs.Handler, "Answer!");
            
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

        //private static string Serialize<T>(T gameRegistration)
        //{
        //    XmlSerializer ser = new XmlSerializer(typeof(T));
        //    string response;
        //    using (TextWriter writer = new StringWriter())
        //    {
        //        ser.Serialize(writer, gameRegistration);
        //        response = writer.ToString();
        //    }
        //    return response;
        //}
    }
}
