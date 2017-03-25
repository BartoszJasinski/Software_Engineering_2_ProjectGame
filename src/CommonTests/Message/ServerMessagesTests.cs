using Common.Message;
using Common.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTests.Message
{
    [TestClass]
    public class ServerMessagesTests
    {
        [TestMethod]
        public void JoinGameTest()
        {
            JoinGame joinGame = new JoinGame();
            joinGame.gameName = "testGameName";
            joinGame.preferedRole = PlayerType.leader;
            joinGame.preferedTeam = TeamColour.red;

            string xml = XmlMessageConverter.ToXml(joinGame);
            JoinGame result = (JoinGame)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual("testGameName", result.gameName);
            Assert.AreEqual(PlayerType.leader, result.preferedRole);
            Assert.AreEqual(TeamColour.red, result.preferedTeam);
        }

        [TestMethod]
        public void RegisteredGamesTest()
        {
            RegisteredGames registeredGames = new RegisteredGames();
            GameInfo[] gameInfoTab = new GameInfo[3];
            gameInfoTab[0] = new GameInfo() { name = "testName" };
            registeredGames.GameInfo = gameInfoTab;


            string xml = XmlMessageConverter.ToXml(registeredGames);
            RegisteredGames result = (RegisteredGames)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual("testName", result.GameInfo[0].name);
        }


        [TestMethod]
        public void RegisterGamesTest()
        {
            RegisterGame registerGame = new RegisterGame();
            GameInfo gameInfo = new GameInfo();
            gameInfo = new GameInfo() { name = "testName" };
            registerGame.NewGameInfo = gameInfo;


            string xml = XmlMessageConverter.ToXml(registerGame);
            RegisterGame result = (RegisterGame)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual("testName", result.NewGameInfo.name);
        }


        [TestMethod]
        public void ConfirmGameRegistrationTest()
        {
            ConfirmGameRegistration confirmGameRegistration = new ConfirmGameRegistration();

            
            confirmGameRegistration.gameId = 4;

            string xml = XmlMessageConverter.ToXml(confirmGameRegistration);
            ConfirmGameRegistration result = (ConfirmGameRegistration)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((ulong)4, result.gameId);
        }
    }
}
