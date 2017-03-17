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
            try
            {
                XmlSerializer xs = new XmlSerializer(Type.GetType(ns + xd.LastChild.Name));
                using (var s=new StringReader(xmlString))
                {
                    return xs.Deserialize(s);
                }
                
                
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public static string ToXml(object msg)
        {
            XmlSerializer xs = new XmlSerializer(msg.GetType());
            StringBuilder sb = new StringBuilder();
            using (var s = new StringWriter(sb))
            {
                xs.Serialize(s, msg);
            }
            return sb.ToString();
        }
    }
}
