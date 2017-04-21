using System.IO;
using System.Xml.Serialization;

namespace Common.Config
{
    public partial class Configuration
    {
        public static T FromFile<T>(string path)
        {
            T result;

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var reader = new StreamReader(path))
            {
                result = (T)serializer.Deserialize(reader);
            }

            return result;
        }
    }
}
