using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Common.DebugUtils;

namespace Common.Message
{
    public class XmlValidation
    {
        static XmlValidation instance;
        XmlReaderSettings settings;

        private XmlValidation()
        {
            settings = new XmlReaderSettings();
            settings.Schemas.Add("https://se2.mini.pw.edu.pl/17-results/", "TheProjectGameCommunication.xsd");
            settings.ValidationType = ValidationType.Schema;
        }

        public static XmlValidation Instance
        {
            get
            {
                if (instance == null)
                    instance = new XmlValidation();
                return instance;
            }
        }

        public void Validate(string message)
        {
            XmlReader reader = XmlReader.Create(new StringReader(message), settings);
            XmlDocument document = new XmlDocument();
            document.Load(reader);

            ValidationEventHandler eventHandler =
                new ValidationEventHandler(ValidationEventHandler);
            document.Validate(eventHandler);

        }


        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
           
            ConsoleDebug.Error("\n ERROR IN VALIDATION\n ");

            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Error: {0}", e.Message);
                    throw new  XmlException();
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    throw new XmlException();
                    break;
            }

         
         
        }
    }
}
