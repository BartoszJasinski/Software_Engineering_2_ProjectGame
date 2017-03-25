using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Game;

namespace ServerTests
{
    [TestClass]
    public class GamesTests
    {
        GamesContainer gc = new GamesContainer();

        [TestInitialize]
        public void InitTest()
        {
            gc.RegisterGame(new Game());
            gc.RegisterGame(new Game(name:"g1", gameId:1, bluePlayers:2, redPlayers:2));
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
}
