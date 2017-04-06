using System;
using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using Wrapper = Common.SchemaWrapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMaster.Log;

namespace GameMaster.Net
{
    
    public class GameMasterClient
    {
        public IConnection Connection { get; }

        //Contents of configuration file
        public Common.Config.GameMasterSettings Settings;

        public ILogger Logger { get; set; }

        //The two teams
        public Wrapper.Team TeamRed { get; set; }
        public Wrapper.Team TeamBlue { get; set; }
        public IEnumerable<Wrapper.Player> Players
        {
            get
            {
                return TeamRed.Players.Concat(TeamBlue.Players);
            }
        }

        public ulong Id { get; set; }

        public bool IsReady => TeamRed.IsFull && TeamBlue.IsFull;
        public GameBoard Board { get; set; }
        public IList<Wrapper.Piece> Pieces = new List<Wrapper.Piece>();


        public GameMasterClient(IConnection connection, Common.Config.GameMasterSettings settings)
        {
            this.Connection = connection;
            this.Settings = settings;
            Logger=new Logger();

            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;

            TeamRed = new Wrapper.Team(TeamColour.red, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));
            TeamBlue = new Wrapper.Team(TeamColour.blue, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));
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

            //ConsoleDebug.Message("New message from:" + socket.GetRemoteAddress() + "\n" + eventArgs.Message);

            BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this, socket);
        }

        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            //System.Console.WriteLine("New message sent to {0}", address.ToString());
            var socket = eventArgs.Handler as Socket;

        }

        //returns null if both teams are full
        public Wrapper.Team SelectTeamForPlayer(TeamColour preferredTeam)
        {
            var selectedTeam = preferredTeam == TeamColour.blue ? TeamBlue : TeamRed;
            var otherTeam = preferredTeam == TeamColour.blue ? TeamRed : TeamBlue;

            if (selectedTeam.IsFull)
                selectedTeam = otherTeam;

            //both teams are full
            if (selectedTeam.IsFull)
            {
                return null;
            }

            return selectedTeam;
        }

        public void PlaceNewPieces(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Pieces.Add(new Wrapper.Piece((ulong)i, PieceType.normal, DateTime.Now));
                ConsoleDebug.Good($"Placed new Piece, time: { DateTime.Now.ToLongTimeString()}");
            }
        }

        public async Task GeneratePieces()
        {
            while(true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(Settings.GameDefinition.PlacingNewPiecesFrequency));
                PlaceNewPieces(1);
            }
        }





    }//class
}//namespace
