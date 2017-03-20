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
        ///<summary>
        /// XML Desirializer
        /// </summary>
        ///<returns>
        /// Object of desired type if XML is valid
        /// otherwise NULL
        /// </returns>   
        public static object ToObject(string xmlString, string ns = "Common.Schema.")
        {
            XmlDocument xd = new XmlDocument();
            try
            {
                xd.LoadXml(xmlString);
                XmlSerializer xs = new XmlSerializer(Type.GetType(ns + xd.LastChild.Name));
                using (var s = new StringReader(xmlString))
                {
                    return xs.Deserialize(s);
                }
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (XmlException)
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