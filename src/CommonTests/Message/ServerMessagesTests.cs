using Common.Message;
using Common.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonTests.Message
{
    [TestClass]
    public class ServerMessagesTests
    {
        [TestMethod]
        public void JoinGameTest()
        {
            JoinGame joinGame = new JoinGame();
            joinGame.gameName = "testgamename";
            joinGame.preferredRole = PlayerType.leader;
            joinGame.preferredTeam = TeamColour.red;

            string xml = XmlMessageConverter.ToXml(joinGame);
            JoinGame result = (JoinGame)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual("testgamename", result.gameName);
            Assert.AreEqual(PlayerType.leader, result.preferredRole);
            Assert.AreEqual(TeamColour.red, result.preferredTeam);
        }

        [TestMethod]
        public void RegisteredGamesTest()
        {
            RegisteredGames registeredGames = new RegisteredGames();
            GameInfo[] gameInfoTab = new GameInfo[3];
            gameInfoTab[0] = new GameInfo() { gameName = "testname" };
            registeredGames.GameInfo = gameInfoTab;


            string xml = XmlMessageConverter.ToXml(registeredGames);
            RegisteredGames result = (RegisteredGames)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual("testname", result.GameInfo[0].gameName);
        }


        [TestMethod]
        public void RegisterGamesTest()
        {
            RegisterGame registerGame = new RegisterGame();
            GameInfo gameInfo = new GameInfo();
            gameInfo = new GameInfo() { gameName = "testname" };
            registerGame.NewGameInfo = gameInfo;


            string xml = XmlMessageConverter.ToXml(registerGame);
            RegisterGame result = (RegisterGame)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual("testname", result.NewGameInfo.gameName);
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
