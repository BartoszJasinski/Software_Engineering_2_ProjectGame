using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Common.Schema.Game;

namespace Common.Xml
{
    public class XmlOperations
    {
        public static string Serialize()
        {
            GameFinished subReq = new GameFinished();
            subReq.gameId = 123;

            XmlSerializer xsSubmit = new XmlSerializer(typeof(GameFinished));
            var sww = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sww);
            xsSubmit.Serialize(writer, subReq);

            return sww.ToString();

        }
    }
}