using System;
using System.IO;
using System.Text;
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
            //TODO when XmlValidation.Validate(xmlString) is uncommented few unit tests fail 
            XmlValidation.Instance.Validate(xmlString);
            xd.LoadXml(xmlString);
            try
            {
                XmlSerializer xs = new XmlSerializer(Type.GetType(ns + xd.LastChild.Name));
                using (var s = new StringReader(xmlString))
                {
                    return xs.Deserialize(s);
                }
            }
            catch(ArgumentNullException e)
            {
                throw new ArgumentException("The given message type does not exist.");
            }
        }

        public static string ToXml(object msg)
        {
            XmlSerializer xs = new XmlSerializer(msg.GetType());
            StringBuilder sb = new StringBuilder();
            using (StringWriter s = new Utf8StringWriter(sb))
            {
                xs.Serialize(s, msg);
            }
            return sb.ToString();
        }
    }
}