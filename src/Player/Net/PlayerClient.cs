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
        private IMessageHandler messageHandler;
        private IConnection connection;
        private AgentCommandLineOptions options;
        private PlayerSettings settings;
        private Socket serverSocket;
        private State currentState;
        private CancellationTokenSource keepAliveToken { get; } = new CancellationTokenSource();

        public AgentCommandLineOptions Options
        {
            get
            {
                return options;
            }

            set
            {
                options = value;
            }
        }

        public PlayerSettings Settings
        {
            get
            {
                return settings;
            }

            set
            {
                settings = value;
            }
        }

        private const int NO_PIECE = -1;

        

       

        public PlayerClient(IConnection connection, PlayerSettings settings, AgentCommandLineOptions options, IMessageHandler messageHandler)
        {
            this.connection = connection;
            this.Settings = settings;
            this.Options = options;
            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
            this.messageHandler = messageHandler;
            messageHandler.Player = this;
            currentState = messageHandler.BiuldDfa();
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
                messageHandler.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message));
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
            messageHandler.PrintBoard();
            
            currentState = currentState.NextState();
            if (currentState.Action == null)
                Play();
            else
                currentState.Action();
        }






        private void RegisterForNextGameAfterGameEnd()
        {
            JoinGame joinGame = new JoinGame()
            {
                preferredTeam = Options.PreferredTeam == "blue"
                    ? Common.Schema.TeamColour.blue
                    : Common.Schema.TeamColour.red,
                preferredRole = Options.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                gameName = Options.GameName,
                playerIdSpecified = false
            };

            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(joinGame));
        }







        private async Task startKeepAlive(Socket server)
        {
            while (true)
            {
                if (keepAliveToken.Token.IsCancellationRequested)
                    break;
                await Task.Delay(TimeSpan.FromMilliseconds(Settings.KeepAliveInterval));
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