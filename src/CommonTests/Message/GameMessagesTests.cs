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
    public class GameMessagesTests
    {
       

        [TestMethod]
        public void AuthorizeKnowledgeExchangeTest()
        {
            AuthorizeKnowledgeExchange message = new AuthorizeKnowledgeExchange();

            message.withPlayerId = 4;

            string xml = XmlMessageConverter.ToXml(message);
            AuthorizeKnowledgeExchange result = (AuthorizeKnowledgeExchange)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((ulong)4, result.withPlayerId);
        }

        [TestMethod]
        public void MoveTest()
        {
            Move message = new Move();

            message.direction = MoveType.down;

            message.directionSpecified = true;

            string xml = XmlMessageConverter.ToXml(message);
            Move result = (Move)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual(MoveType.down, result.direction);
            Assert.AreEqual(true, result.directionSpecified);
        }

        [TestMethod]
        public void GameFinishedTest()
        {
            GameFinished message =  new GameFinished();

            message.gameId = 0;

            string xml = XmlMessageConverter.ToXml(message);
            GameFinished result = (GameFinished)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((ulong)0, result.gameId);

        }

        [TestMethod]
        public void GameInfoTest()
        {
            GameInfo message = new GameInfo();

            message.blueTeamPlayers = 3;
            message.redTeamPlayers = 3;
            message.name = "testName";

            string xml = XmlMessageConverter.ToXml(message);
            GameInfo result = (GameInfo)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((ulong)3,result.redTeamPlayers);
            Assert.AreEqual((ulong)3, result.blueTeamPlayers);
            Assert.AreEqual("testName", result.name);

        }
    }
}
