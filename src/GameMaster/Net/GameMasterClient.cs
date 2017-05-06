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
using System.Threading;

namespace GameMaster.Net
{

    public class GameMasterClient
    {
        public IConnection Connection { get; }

        //Contents of configuration file
        public Common.Config.GameMasterSettings Settings;

        public ILogger Logger { get; set; }

        public CancellationTokenSource CancelToken { get; } = new CancellationTokenSource();
        private CancellationTokenSource keepAliveToken { get; } = new CancellationTokenSource();

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

        public ulong gameId { get; set; }

        public object BoardLock { get; set; } = new object();

        public bool IsReady => TeamRed.IsFull && TeamBlue.IsFull;
        public Wrapper.AddressableBoard Board { get; set; }
        public IList<Wrapper.Piece> Pieces = new List<Wrapper.Piece>();//TODO pieces are not added to this collection

        private Random rng = new Random();


        public GameMasterClient(IConnection connection, Common.Config.GameMasterSettings settings, ILogger logger)
        {
            this.Connection = connection;
            this.Settings = settings;
            Logger = logger;

            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;

            TeamRed = new Wrapper.Team(TeamColour.red, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));
            TeamBlue = new Wrapper.Team(TeamColour.blue, uint.Parse(settings.GameDefinition.NumberOfPlayersPerTeam));

            //var boardGenerator = new RandomGoalBoardGenerator(uint.Parse(Settings.GameDefinition.BoardWidth),
            //    uint.Parse(Settings.GameDefinition.TaskAreaLength),
            //    uint.Parse(Settings.GameDefinition.GoalAreaLength),
            //    123);
            var boardGenerator = new SimpleBoardGenerator(uint.Parse(Settings.GameDefinition.BoardWidth),
                uint.Parse(Settings.GameDefinition.TaskAreaLength),
                uint.Parse(Settings.GameDefinition.GoalAreaLength),
                Settings.GameDefinition.Goals);
            Board = boardGenerator.CreateBoard();
        }

        public void Connect()
        {
            Connection.StartClient();
        }

        public void Disconnect()
        {
            keepAliveToken.Cancel();
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

            //start sending keepalive bytes
            startKeepAlive(socket);

        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            if (eventArgs.Message.Length > 0) //the message is not the keepalive packet
            {
                ConsoleDebug.Message("New message from:" + socket.GetRemoteAddress() + "\n" + eventArgs.Message);
                BoardPrinter.PrintAlternative(Board);

                BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message), this, socket);
            }
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

        public void PlaceNewPiece(Common.SchemaWrapper.TaskField field)
        {

            var pieceType = rng.NextDouble() < Settings.GameDefinition.ShamProbability ? PieceType.sham : PieceType.normal;
            lock (BoardLock)
            {
                var newPiece = new Wrapper.Piece((ulong)Pieces.Count, pieceType, DateTime.Now);
                newPiece.Id = pieceid++;
                if (field == null)
                {
                    ConsoleDebug.Warning("There are no empty places for a new Piece!");
                    return;   //TODO BUSYWAITING HERE probably
                }
                //remove old piece
                if (field.PieceId != null)
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

        public bool IsPlayerInGoalArea(Wrapper.Player p)
        {
            if (p.Team.Color == TeamColour.blue && p.Y < Board.GoalsHeight)
                return true;
            return p.Team.Color == TeamColour.red && p.Y >= Board.Height - Board.GoalsHeight;
        }

        public async Task GeneratePieces()
        {
            while (true)
            {
                if (CancelToken.Token.IsCancellationRequested)
                    break;
                await Task.Delay(TimeSpan.FromMilliseconds(Settings.GameDefinition.PlacingNewPiecesFrequency));
                if (CancelToken.Token.IsCancellationRequested)
                    break;
                PlaceNewPiece(Board.GetRandomEmptyFieldInTaskArea());
            }
        }

        public async Task startKeepAlive(Socket server)
        {
            while(true)
            {
                if (keepAliveToken.Token.IsCancellationRequested)
                    break;
                await Task.Delay(TimeSpan.FromMilliseconds(Settings.KeepAliveInterval));
                if (keepAliveToken.Token.IsCancellationRequested)
                    break;
                Connection.SendFromClient(server, string.Empty);
            }
        }


    }//class
}//namespace
