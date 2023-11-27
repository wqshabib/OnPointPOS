using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace POSSUM.Sockets
{
    public static class SerializeUtility
    {
        public static string XmlSerialize<T>(T sourceValue) where T : class
        {
            // If source is empty, throw Exception
            if (sourceValue == null)
                throw new NullReferenceException("sourceValue is required");

            // Define encoding
            var encoding = Encoding.Default;

            // Declare the resultant variable
            string targetValue;

            // Using MemoryStream for In-Process conversion
            using (var memoryStream = new MemoryStream())
            {
                // Declare Stream with required Encoding
                using (var streamWriter = new StreamWriter(memoryStream, encoding))
                {
                    // Declare Xml Serializer with source value Type (serializing type)
                    var xmlSerializer = new XmlSerializer(sourceValue.GetType());

                    // Perform Serialization of the source value and write to Stream
                    xmlSerializer.Serialize(streamWriter, sourceValue);

                    // Grab the serialized string
                    targetValue = encoding.GetString(memoryStream.ToArray());
                }
            }

            // Return the resultant value;
            return targetValue;
        }

        public static bool TryDeserialize<T>(string sourceValue, out T o) where T : class
        {
            try
            {
                o = XmlDeserialize<T>(sourceValue);
                return true;
            }
            catch (Exception)
            {
                o = null;

                return false;
            }
        }


        public static T XmlDeserialize<T>(string sourceValue) where T : class
        {
            // If source is empty, throw Exception
            if (string.IsNullOrWhiteSpace(sourceValue))
            {
                throw new NullReferenceException("sourceValue is required");
            }

            // Define encoding
            var encoding = Encoding.UTF8;

            // Declare the resultant variable
            T targetValue;

            // Declare Xml Serializer with target value Type (serialized type)
            var xmlSerializer = new XmlSerializer(typeof(T));

            // Get the source value to bytes with required Encoding
            var sourceBytes = encoding.GetBytes(sourceValue);

            // Using MemoryStream for In-Process conversion
            using (var memoryStream = new MemoryStream(sourceBytes))
            {
                // Read stream into XML-based reader
                using (var xmlTextReader = new XmlTextReader(memoryStream))
                {
                    // Perform Deserialization from the stream and Convert to target type
                    targetValue = xmlSerializer.Deserialize(xmlTextReader) as T;
                }
            }

            // Return the resultant value;
            return targetValue;
        }

        public static string GetXMLFromObject(object o)
        {
            var sw = new StringWriter();
            XmlTextWriter tw = null;
            try
            {
                var serializer = new XmlSerializer(o.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, o);
            }
            catch (Exception)
            {
                //Handle Exception Code
            }
            finally
            {
                sw.Close();
                if (tw != null)
                {
                    tw.Close();
                }
            }
            return sw.ToString();
        }

        public static Object ObjectToXML(string xml, Type objectType)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            Object obj = null;
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(objectType);
                xmlReader = new XmlTextReader(strReader);
                obj = serializer.Deserialize(xmlReader);
            }
            catch (Exception exp)
            {
                //Handle Exception Code
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }
    }
}