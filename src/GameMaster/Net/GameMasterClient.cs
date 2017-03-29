using System;
using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.Message;
using Common.Schema;
using GameMaster.Logic;

namespace GameMaster.Net
{
    
    public class GameMasterClient
    {
        private IConnection connection;


        public GameMasterClient(IConnection connection)
        {
            this.connection = connection;
            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
        }

        public void Connect()
        {
            connection.StartClient();
        }

        public void Disconnect()
        {
            connection.StopClient();
        }


        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            var address = eventArgs.Handler.GetRemoteAddress();
            System.Console.WriteLine("Successful connection with address {0}", address.ToString());
            var socket = eventArgs.Handler as Socket;

            ////TEST game registration
            GameInfo gameInfo = new GameInfo();
            gameInfo.gameName = "testgame";
            gameInfo.blueTeamPlayers = 42;
            gameInfo.redTeamPlayers = 24;
            RegisterGame registerGame = new RegisterGame();
            registerGame.NewGameInfo = gameInfo;
            ////TEST


            string registerGameMessage = XmlMessageConverter.ToXml(registerGame);
            connection.SendFromClient(socket, registerGameMessage);
            
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            System.Console.WriteLine("New message from: {0} \n {1}",socket.GetRemoteAddress(),eventArgs.Message);

            BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message));
            
            string xmlMessage = XmlMessageConverter.ToXml(XmlMessageGenerator.GetXmlMessage());

          //  connection.SendFromClient(socket, xmlMessage);


        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            //System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;

        }





    }//class
}//namespace
