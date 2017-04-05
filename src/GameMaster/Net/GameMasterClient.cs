using System;
using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using GameMaster.Logic;
using System.Collections.Generic;
using System.Linq;

namespace GameMaster.Net
{
    
    public class GameMasterClient
    {
        public IConnection Connection { get; }

        //Contents of configuration file
        public Common.Config.GameMasterSettings Settings;

        //The two teams
        public Team TeamRed{ get; set; }
        public Team TeamBlue { get; set; }
        public IEnumerable<Logic.Player> Players
        {
            get
            {
                return TeamRed.Players.Concat(TeamBlue.Players);
            }
        }

        public ulong Id { get; set; }

        public bool IsReady => TeamRed.IsFull && TeamBlue.IsFull;
        public GameBoard Board { get; set; }


        public GameMasterClient(IConnection connection, Common.Config.GameMasterSettings settings)
        {
            this.Connection = connection;
            this.Settings = settings;

            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;

            TeamRed = new Team(TeamColour.red, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));
            TeamBlue = new Team(TeamColour.blue, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));
        }

        public void Connect()
        {
            Connection.StartClient();
        }

        public void Disconnect()
        {
            Connection.StopClient();
        }


        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            var address = eventArgs.Handler.GetRemoteAddress();
            ConsoleDebug.Ordinary("Successful connection with address " + address.ToString());
            var socket = eventArgs.Handler as Socket;

            //at the beginning both teams have same number of open player slots
            ulong noOfPlayersPerTeam = ulong.Parse(Settings.GameDefinition.NumberOfPlayersPerTeam);

            RegisterGame registerGameMessage = new RegisterGame()
            {
                NewGameInfo = new GameInfo()
                {
                    gameName = Settings.GameDefinition.GameName,
                    blueTeamPlayers = noOfPlayersPerTeam,
                    redTeamPlayers = noOfPlayersPerTeam
                }
            };


            string registerGameString = XmlMessageConverter.ToXml(registerGameMessage);
            Connection.SendFromClient(socket, registerGameString);
            
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            ConsoleDebug.Message("New message from:" + socket.GetRemoteAddress() + "\n" + eventArgs.Message);

            BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this, socket);
            
            string xmlMessage = XmlMessageConverter.ToXml(XmlMessageGenerator.GetXmlMessage());

           // connection.SendFromClient(socket, xmlMessage);


        }

        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
            var socket = eventArgs.Handler as Socket;

        }





    }//class
}//namespace
