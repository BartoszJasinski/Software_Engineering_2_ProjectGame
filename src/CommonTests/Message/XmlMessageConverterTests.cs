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
        public void ConvertToObjectTest()
        {
            
        }
    }
}
