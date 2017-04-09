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
using GameMaster.Logic.Board;

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

        public object BoardLock { get; set; } = new object();

        public bool IsReady => TeamRed.IsFull && TeamBlue.IsFull;
        public Wrapper.AddressableBoard Board { get; set; }
        public IList<Wrapper.Piece> Pieces = new List<Wrapper.Piece>();//TODO pieces are not added to this collection

        private Random rng = new Random();


        public GameMasterClient(IConnection connection, Common.Config.GameMasterSettings settings)
        {
            this.Connection = connection;
            this.Settings = settings;
            Logger = new Logger();

            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;

            TeamRed = new Wrapper.Team(TeamColour.red, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));
            TeamBlue = new Wrapper.Team(TeamColour.blue, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));

            var boardGenerator = new RandomGoalBoardGenerator(uint.Parse(Settings.GameDefinition.BoardWidth),
                uint.Parse(Settings.GameDefinition.TaskAreaLength),
                uint.Parse(Settings.GameDefinition.GoalAreaLength),
                123);
            Board = boardGenerator.CreateBoard();
        }

        public void Connect()
        {
            Connection.StartClient();
        }

        public void Disconnect()
        {
            Logger.Dispose();
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
            BoardPrinter.Print(Board);

            BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this, socket);
        }

        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
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

        private static ulong pieceid = 0;

        public void PlaceNewPieces(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var pieceType = rng.NextDouble() < Settings.GameDefinition.ShamProbability ? PieceType.sham : PieceType.normal;
                lock(BoardLock)
                {
                    var newPiece = new Wrapper.Piece((ulong)Pieces.Count, pieceType, DateTime.Now);
                    newPiece.Id = pieceid++;
                    var field = Board.GetRandomEmptyFieldInTaskArea();
                    if (field == null)
                    {
                        ConsoleDebug.Warning("There are no empty places for a new Piece!");
                        continue;   //TODO BUSYWAITING HERE probably
                    }
                    //remove old piece
                    if(field.PieceId != null)
                    {
                        var oldPiece = Pieces.Where(p => p.Id == field.PieceId.Value).Single();
                        Pieces.Remove(oldPiece);
                    }
                    field.PieceId = newPiece.Id;
                    newPiece.Location = new Location() { x = field.X, y = field.Y };
                    Pieces.Add(newPiece);
                    Board.UpdateDistanceToPiece(Pieces);
                    ConsoleDebug.Good($"Placed new Piece at: ({ field.X }, {field.Y})");
                }
                //BoardPrinter.Print(Board);
            }
        }

        public bool IsPlayerInGoalArea(Wrapper.Player p)
        {
            if (p.Team.Color == TeamColour.blue && p.Y < Board.GoalsHeight)
                return true;
            return p.Team.Color == TeamColour.red && p.Y >= Board.Height - Board.GoalsHeight;
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
