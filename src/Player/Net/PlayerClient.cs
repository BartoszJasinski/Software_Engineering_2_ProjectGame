using System;
using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.DebugUtils;
using Common.Message;
using Common.Config;
using Common.IO.Console;
using Common.Schema;
using Location = Common.Schema.Location;
using System.Collections.Generic;
using System.Linq;
using Player.Strategy;
using GoalFieldType = Common.Schema.GoalFieldType;
using TeamColour = Common.Schema.TeamColour;
using Wrapper = Common.SchemaWrapper;
using System.Threading.Tasks;
using System.Threading;

namespace Player.Net
{
    public class PlayerClient
    {
        private IGame game;
        private IConnection connection;
        private PlayerSettings settings;
        private Socket serverSocket;
        private ulong _gameId;
        private string _guid;
        private Common.Schema.TeamColour _team;
        private Common.Schema.Player[] _players;
        private IList<Piece> _pieces;
        private GameBoard _board;
        private Common.Schema.PlayerType _type;
        private Common.SchemaWrapper.Field[,] _fields;
        private Random random = new Random();
        private State currentState;
        private CancellationTokenSource keepAliveToken { get; } = new CancellationTokenSource();

        private const int NO_PIECE = -1;

       

        public PlayerClient(IConnection connection, PlayerSettings settings, AgentCommandLineOptions options)
        {
            this.connection = connection;
            this.settings = settings;
            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
            game = new Game(this, settings, options);
            currentState = game.BiuldDfa();
        }

        public void Connect()
        {
            connection.StartClient();
        }

        public void Disconnect()
        {
            keepAliveToken.Cancel();
            connection.StopClient();
        }


        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            var address = eventArgs.Handler.GetRemoteAddress();
            ConsoleDebug.Ordinary("Successful connection with address " + address.ToString());
            var socket = eventArgs.Handler as Socket;
            serverSocket = socket;
            string xmlMessage = XmlMessageConverter.ToXml(new GetGames());

            connection.SendFromClient(socket, xmlMessage);

            //start sending keep alive bytes
            startKeepAlive(socket);
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            if (eventArgs.Message.Length > 0) //the message is not the keepalive packet
            {
                ConsoleDebug.Message("New message from: " + socket.GetRemoteAddress() + "\n" + eventArgs.Message);
                game.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message));
            }
        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;
        }

        public void Play()
        {
            ConsoleDebug.Strategy(currentState.Name);
            BoardPrinter.Print(game.Fields);
            
            currentState = currentState.NextState();
            if (currentState.Action == null)
                Play();
            else
                currentState.Action();
        }



        


        //private void RegisterForNextGameAfterGameEnd()
        //{
        //    JoinGame joinGame = new JoinGame()
        //    {
        //        preferredTeam = options.PreferredTeam == "blue"
        //            ? Common.Schema.TeamColour.blue
        //            : Common.Schema.TeamColour.red,
        //        preferredRole = options.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
        //        gameName = options.GameName,
        //        playerIdSpecified = false
        //    };

        //    connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(joinGame));
        //}

      



       

        private async Task startKeepAlive(Socket server)
        {
            while (true)
            {
                if (keepAliveToken.Token.IsCancellationRequested)
                    break;
                await Task.Delay(TimeSpan.FromMilliseconds(settings.KeepAliveInterval));
                if (keepAliveToken.Token.IsCancellationRequested)
                    break;
                connection.SendFromClient(server, string.Empty);
            }
        }

        public void Send(string data)
        {
            connection.SendFromClient(serverSocket, data);
        }
    } //class
} //namespace