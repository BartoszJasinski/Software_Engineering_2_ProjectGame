using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Message
{
    /// <summary>
    /// Class with static methods to convert message objects from/to XML string/message objects from given namespace
    /// </summary>
    public class XmlMessageConverter
    {
        public static object ToObject(string xmlString, string ns = "Common.Schema.")
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xmlString);
            XmlSerializer xs = new XmlSerializer(Type.GetType(ns + xd.LastChild.Name));
            return xs.Deserialize(new StringReader(xmlString));
        }

        public static string ToXml(object msg)
        {
            XmlSerializer xs = new XmlSerializer(msg.GetType());
            StringBuilder sb = new StringBuilder();
            xs.Serialize(new StringWriter(sb), msg);
            return sb.ToString();
        }
    }
}
