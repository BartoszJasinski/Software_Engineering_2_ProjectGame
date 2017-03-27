using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Schema;
using Common.Message;

namespace CommonTests.Message
{
    [TestClass]
    public class XmlMessageConverterTests
    {
        private string testXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<SomeMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />";

        [TestMethod]
        public void ConvertToXmlTest()
        {
            Move msg = new Move();
            msg.direction = MoveType.down;
            string xmlString = XmlMessageConverter.ToXml(msg);
            Console.WriteLine(xmlString);
        }


        [TestMethod]
        public void CorrectMoveMessageTest()
        {
            string xml =
                $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                $"<Move xmlns=\"https://se2.mini.pw.edu.pl/17-results/\"\r\ngameId=\"1\"\r\n" +
                $"playerGuid=\"c094cab7-da7b-457f-89e5-a5c51756035f\"\r\n" +
                $"direction=\"up\"/>\r";
            var obj = XmlMessageConverter.ToObject(xml);
            Assert.IsTrue(obj is Move);
            Assert.IsFalse(obj is Discover);
            Move m = obj as Move;
            Assert.AreEqual(m.direction, MoveType.up);
            Assert.AreEqual(m.directionSpecified, true);
            Assert.AreEqual(m.playerGuid, "c094cab7-da7b-457f-89e5-a5c51756035f");
            Assert.AreEqual(m.gameId, (ulong)1);
        }


        [TestMethod]
        public void WrongMoveMessageTest()
        {
            string xmlNoGameId = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
                                    "<ConfirmGameRegistration\r\n" +
                                    "xmlns=\"https://se2.mini.pw.edu.pl/17-results/\"\r\n" +
                                    "/>";
            var obj = XmlMessageConverter.ToObject(xmlNoGameId);
            Assert.IsTrue(obj is ConfirmGameRegistration);
        }

        [TestMethod]
        public void NotExistigMessageType()
        {
            string xml= "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
                                    "<ConfirmasdasdGameRegistration\r\n" +
                                    "xmlns=\"http://theprojectgame.mini.pw.edu.pl/\"\r\n" +
                                    "/>";
            var obj = XmlMessageConverter.ToObject(xml);
            Assert.IsNull(obj);
        }

        [TestMethod]
        public void BullshitParseTest()
        {
            string xml = "sadasdsad";
            var obj = XmlMessageConverter.ToObject(xml);
            Assert.IsNull(obj);
        }

        [TestMethod]
        public void BrokenXmlTest()
        {
            string xmlNoGameId = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
                        "<ConfirmGameRegistration\r\n" +
                        "xmlns=\"http://theprojectgame.mini.pw.edu.pl/\"\r\n" +
                        "/";
            var obj = XmlMessageConverter.ToObject(xmlNoGameId);
            Assert.IsNull(obj);
        }
    }
}
