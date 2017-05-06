using Common.Message;
using Common.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CommonTests.Message
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void FieldTest()
        {
            GameBoard message = new GameBoard();

            message.goalsHeight = 5;
            message.tasksHeight = 3;
            message.width = 10;

            string xml = XmlMessageConverter.ToXml(message);
            GameBoard result = (GameBoard)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((uint)5, result.goalsHeight);
            Assert.AreEqual((uint)3, result.tasksHeight);
            Assert.AreEqual((uint)10, result.width);
        }

        [TestMethod]
        public void LocationTest()
        {
            Location message = new Location();

            message.x = 3;
            message.y = 4;

            string xml = XmlMessageConverter.ToXml(message);
            Location result = (Location)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((uint)3, result.x);
            Assert.AreEqual((uint)4, result.y);
        }

        [TestMethod]
        public void PieceTest()
        {
            Piece message = new Piece();

            message.id = 0;
            message.playerId = 3;
            message.playerIdSpecified = true;
            message.timestamp = new DateTime(2000, 1, 1);
            message.type = PieceType.normal;

            string xml = XmlMessageConverter.ToXml(message);
            Piece result = (Piece)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((ulong)0, result.id);
            Assert.AreEqual((ulong)3, result.playerId);
            Assert.AreEqual(true, result.playerIdSpecified);
            Assert.AreEqual(2000, result.timestamp.Year);
            Assert.AreEqual(PieceType.normal, result.type);
        }

        [TestMethod]
        public void GoalFieldTest()
        {
            GoalField message = new GoalField();

            message.type = GoalFieldType.goal;
            message.team = TeamColour.red;

            string xml = XmlMessageConverter.ToXml(message);
            GoalField  result = (GoalField)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual(GoalFieldType.goal, result.type);
            Assert.AreEqual(TeamColour.red, result.team);

        }

        [TestMethod]
        public void TaskFieldTest()
        {
            TaskField message = new TaskField();

            message.distanceToPiece = 5;
            message.pieceId = 1;
            message.pieceIdSpecified = true;

            string xml = XmlMessageConverter.ToXml(message);
            TaskField result = (TaskField)XmlMessageConverter.ToObject(xml);

            Assert.AreEqual((int)5, result.distanceToPiece);
            Assert.AreEqual((ulong)1, result.pieceId);
            Assert.AreEqual(true, result.pieceIdSpecified);

        }

    }
}
