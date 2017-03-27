using System;
using System.Net.Sockets;
using System.Threading;
using Common.Connection.EventArg;
using Common.Message;
using Common.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;
using Server.Connection;
using Server.Game;
using Game = Server.Game.Game;

namespace ServerTests
{
    [TestClass]
    public class GamesTests
    {
        GamesContainer gc = new GamesContainer();
        CommunicationServer MockServer;
        [TestInitialize]
        public void InitTest()
        {
            gc.RegisterGame(new Game());
            gc.RegisterGame(new Game(name:"g1", gameId:1, bluePlayers:2, redPlayers:2));
            MockServer = new CommunicationServer(new MockEndpoint());
        }

        [TestMethod]
        public void BehavTest()
        {
            //var m = new MockEndpoint();
            //CommunicationServer sv = new CommunicationServer(m);

            ((MockEndpoint)MockServer.ConnectionEndpoint).Receive(XmlMessageConverter.ToXml(new RegisterGame(){NewGameInfo = new GameInfo()
            {
                blueTeamPlayers = 4, name = "dasd", redTeamPlayers = 2
            }}));


        }
        
        [TestMethod]
        public void RegisterGameTest()
        {
            gc.RegisterGame(new Game(12, "asdsa"));
            Assert.AreEqual(gc.Count, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(NameExistsException))]
        public void RegisterGameWithExistingNameTest()
        {
            gc.RegisterGame(new Game(gameId:4123));
        }

        [TestMethod]
        [ExpectedException(typeof(IdExistsException))]
        public void RegisterGameWithExistingIdTest()
        {
            gc.RegisterGame(new Game(gameId: 1, name:"adasdasd"));
        }

        [TestMethod]
        public void RemoveGameTest()
        {
            gc.RemoveGame(gc.GetGameById(0));
            Assert.AreEqual(gc.Count, 1);
        }


        [TestMethod]
        [ExpectedException(typeof(GameNotFoundException))]
        public void RemoveGameFailTest()
        {
            gc.RemoveGame(new Game(gameId: 1, name: "adasdsaddasd"));
        }


    }

    class MockEndpoint : IConnectionEndpoint
    {
        public int Port { get; set; }
        public void Listen()
        {
            throw new NotImplementedException();
        }

        public void SendFromServer(Socket handler, string message)
        {
            
        }

        public void Receive(string message)
        {
            OnMessageRecieve(null, new MessageRecieveEventArgs(message, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)));
        }

        public event EventHandler<ConnectEventArgs> OnConnect;
        public event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;
    }
}
