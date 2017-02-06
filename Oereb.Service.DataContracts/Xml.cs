using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Oereb.Service.DataContracts;

namespace Oereb.Service.DataContracts
{
    /// <summary>
    /// De- and serialization class for a xml
    /// </summary>
    /// <typeparam name="T">class of de- and serialization</typeparam>

    public static class Xml<T>
    {
        public static string SerializeToXmlString(T value)
        {
            if (value == null)
            {
                throw new Exception("serialize value is null");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        public static T DeserializeFromXmlString(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();

            using (StringReader textReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        public static void SerializeToFile(T value, string filename)
        {
            var xml = SerializeToXmlString(value);

            try
            {
                File.WriteAllText(filename, xml);
            }
            catch (Exception ex)
            {
                throw new Exception($"could not serialize to file {filename}", ex);
            }

        }

        public static T DeserializeFromFile(string filename)
        {
            try
            {  
                using (StreamReader streamReader = new StreamReader(filename))
                {
                    String value = streamReader.ReadToEnd();
                    return DeserializeFromXmlString(value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"could not deserialize from file {filename}", ex);
            }
        }
    }
}