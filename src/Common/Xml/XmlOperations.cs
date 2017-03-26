using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Common.Schema;

namespace Common.Xml
{
    //PROBABLY useless class because it is doubling XmlMessageConverter functionality
    public class XmlOperations
    {
        public static string Serialize()
        {
            //GameFinished subReq = new GameFinished();
            //subReq.gameId = 123;

            //XmlSerializer xsSubmit = new XmlSerializer(typeof(GameFinished));
            //var sww = new StringWriter();
            //XmlWriter writer = XmlWriter.Create(sww);
            //xsSubmit.Serialize(writer, subReq);

            //return sww.ToString();
            return null;

        }

        public static void Deserialize(string xml)
        {
//            XmlSerializer serializer = new XmlSerializer(typeof(GameFinished));
//            TextReader xmlReader = new StringReader(xml);
////            StreamReader xmlReader = new StreamReader();
//            GameFinished gameFinished = (GameFinished)serializer.Deserialize(xmlReader);

//            Console.WriteLine("Pimpuś " + gameFinished.gameId);
        }
    }
}