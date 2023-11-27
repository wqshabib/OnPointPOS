using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using System.Security;

namespace ML.Common
{
    public class XmlHelper
    {
        public static bool IsXml(string strXml)
        {
            return IsXml(strXml, string.Empty);
        }
        public static string CleanXml(string strXml)
        {
            var xml = strXml.Replace("\r\n","<br/>");
            xml = xml.Replace("\n", "<br/>");
            xml = xml.Replace("\r", "");
            xml = xml.Replace("\a", "");
            return xml;
        }

        public static bool IsXml(string strXml, string strRoot)
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(strXml);

                if (!string.IsNullOrEmpty(strRoot))
                {
                    if (xml.SelectSingleNode(strRoot) != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch { } 
            {
                return false;
            }
        }

        public static string XmlSerializeObject<T>(object obj)
        {
            //MemoryStream ms = new MemoryStream();
            //XmlSerializer xs = new XmlSerializer(typeof(T));
            //XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.UTF8);
            //xs.Serialize(xtw, obj); ms = (MemoryStream)xtw.BaseStream;
            //return StringHelper.UTF8ByteArrayToString(ms.ToArray());

            //XmlSerializer ser = new XmlSerializer(obj.GetType());
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            //ser.Serialize(writer, obj);
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(sb.ToString());
            //return doc.OuterXml;

            // WORKING for one object
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T XmlDeserializeObject<T>(string strXml)
        {
            //XmlSerializer xs = new XmlSerializer(typeof(T));
            //MemoryStream ms = new MemoryStream(StringHelper.StringToUTF8ByteArray(xml));
            //XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.UTF8);
            //return (T)xs.Deserialize(ms);

            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(xml);
            //XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            //XmlSerializer ser = new XmlSerializer(typeof(T));
            //object obj = ser.Deserialize(reader);
            //return (T)obj;

            using (StringReader reader = new StringReader(strXml))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            } 
        }

        public static byte[] ObjectToByteArray<T>(object obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, obj);
            return memoryStream.ToArray();
        }

        public static T ByteArrayToObject<T>(byte[] byteArray)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return (T)binaryFormatter.Deserialize(memoryStream);
        }

        public static string UpdateXml(string strXml, string strTag, string strData)
        {
            return UpdateXml(strXml, strTag, strData, true);
        }

        public static string UpdateXml(string strXml, string strTag, string strData, bool bWrapCDATA)
        {
            if (strXml == string.Empty)
            {
                return string.Empty;
            }

            if (strData != "0" && strData != "1")
            {
                if (bWrapCDATA)
                {
                    strData = ML.Common.XmlHelper.WrapCDATA(strData);
                }
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strXml);

            XmlNode main = xml.ChildNodes[0];
            if (main.SelectSingleNode(strTag) == null)
            {
                XmlNode node = xml.CreateNode(XmlNodeType.Element, strTag, null);
                //node.InnerXml = string.Format("<![CDATA[{0}]]>", strData);
                node.InnerXml = strData;
                main.AppendChild(node);
            }
            else
            {
                main.SelectSingleNode(strTag).InnerXml = strData;
            }

            return xml.InnerXml;
        }

        public static string UpdateXml(string strXml, string strTag, string strSubTag, string strData)
        {
            return UpdateXml(strXml, strTag, strSubTag, strData, true);
        }

        public static string UpdateXml(string strXml, string strTag, string strSubTag, string strData, bool bWrapCDATA)
        {
            if (strXml == string.Empty)
            {
                return string.Empty;
            }

            if (bWrapCDATA)
            {
                strData = ML.Common.XmlHelper.WrapCDATA(strData);
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strXml);

            XmlNode main = xml.ChildNodes[0];
            if (main.SelectSingleNode(strTag) == null)
            {
                XmlNode node = xml.CreateNode(XmlNodeType.Element, strTag, null);
                main.AppendChild(node);
            }

            if (main.SelectSingleNode(strTag).SelectSingleNode(strSubTag) == null)
            {
                XmlNode nodeSub = xml.CreateNode(XmlNodeType.Element, strSubTag, null);
                nodeSub.InnerXml = strData;
                XmlNode node = main.SelectSingleNode(strTag);
                node.AppendChild(nodeSub);
            }
            else
            {
                main.SelectSingleNode(strTag).SelectSingleNode(strSubTag).InnerXml = strData;
            }

            return xml.InnerXml;
        }

        public static string RemoveXmlNode(string strXml, string strTag)
        {
            if (strXml == string.Empty)
            {
                return string.Empty;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strXml);

            XmlNode main = xml.ChildNodes[0];
            if (main.SelectSingleNode(strTag) != null)
            {
                main.RemoveChild(main.SelectSingleNode(strTag));
            }

            return xml.InnerXml;
        }

        public static string ClearXmlIfEmpty(string strXml)
        {
            if (strXml == string.Empty)
            {
                return string.Empty;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strXml);

            bool bFound = false;
            foreach (XmlNode node1 in xml.ChildNodes)
            {
                if (!string.IsNullOrEmpty(node1.InnerXml))
                {
                    foreach (XmlNode node2 in node1)
                    {
                        if (!string.IsNullOrEmpty(node2.InnerXml))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }
            }
            if (!bFound)
            {
                return string.Empty;
            }

            return strXml;
        }

        public static string WrapCDATA(string strXmlTagData)
        {
            if (strXmlTagData.IndexOf("<![CDATA") > -1 && strXmlTagData.IndexOf("]]>") > -1)
            {
                return strXmlTagData;
            }

            return string.Format("<![CDATA[{0}]]>", strXmlTagData);
        }

        public static string UnwrapCDATA(string strXmlTagData)
        {
            strXmlTagData = strXmlTagData.Replace("<![CDATA[", string.Empty);
            strXmlTagData = strXmlTagData.Replace("]]>", string.Empty);

            return strXmlTagData;
        }

        public static string AddRootTags(string strXml)
        {
            return string.Format("<root>{0}</root>", strXml);
        }

        public static string RemoveRootXmlNode(string strXml)
        {
            if (strXml == string.Empty)
            {
                return string.Empty;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strXml);

            return xml.ChildNodes[0].InnerXml;
        }

        public static string GetNodeAttributeValue(string strXml, string strXmlTag)
        {
            if (strXml == string.Empty)
            {
                return string.Empty;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strXml);

            if (xml.ChildNodes[0].SelectSingleNode(strXmlTag).Attributes.Count == 0)
            {
                return string.Empty;
            }

            return xml.ChildNodes[0].SelectSingleNode(strXmlTag).Attributes[0].Value;
        }

        public static bool NodeExists(string strXml, string strXmlTag)
        {
            if (strXml == string.Empty)
            {
                return false;
            }

            //try
            //{
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(strXml);

                if (xml.ChildNodes[0].SelectSingleNode(strXmlTag) == null)
                {
                    return false;
                }
            //}
            //catch(Exception ex)
            //{
            //    new Email.Email().SendDebug("ML.Common.XmlHelper.NodeExists", string.Format("strXml={0}, strXmlTag={1}, ex.Message={2}, ex.StackTrace={3}", strXml, strXmlTag, ex.Message, ex.StackTrace));
            //}

            return true;    
        }


        public static List<ExpandoObject> ConvertXmlToObjects(XmlDocument xml)
        {
            List<ExpandoObject> expandoObjects = new List<ExpandoObject>();

            int intSortOrder = 0;

            foreach(XmlNode node in xml.FirstChild)
            {
                dynamic expandoObject = new ExpandoObject();

                expandoObjects.Add(expandoObject);
                expandoObject.name = node.Name;
                expandoObject.sortorder = intSortOrder++;

                if(node.Attributes.Count == 0)
                {
                    expandoObject.type = "text";
                    expandoObject.label = node.Name;
                }
                else
                {
                    foreach(XmlAttribute attribute in node.Attributes)
                    {
                        if (ML.Common.Text.IsNumeric(attribute.Value))
                        {
                            int intValue = Convert.ToInt32(attribute.Value);
                            (expandoObject as IDictionary<string, dynamic>)[attribute.Name] = intValue;
                        }
                        else
                        {
                            (expandoObject as IDictionary<string, dynamic>)[attribute.Name] = attribute.Value;
                        }
                    }

                    if(node.Attributes["type"] == null)
                    {
                        (expandoObject as IDictionary<string, dynamic>)["type"] = "text";
                    }
                    if(node.Attributes["label"] == null)
                    {
                        (expandoObject as IDictionary<string, dynamic>)["label"] = node.Name;
                    }
                }
            }

            return expandoObjects;
        }

        public static List<ExpandoObject> ConvertXmlToObjects(XmlDocument xml, bool bProcessValues) //, Guid guidCompanyGuid, int intImageWidth)
        {
            List<ExpandoObject> expandoObjects = new List<ExpandoObject>();

            int intSortOrder = 0;

            foreach (XmlNode node in xml.FirstChild)
            {
                dynamic expandoObject = new ExpandoObject();

                expandoObjects.Add(expandoObject);
                expandoObject.name = node.Name;
                expandoObject.sortorder = intSortOrder++;

                if (node.Attributes.Count == 0)
                {
                    expandoObject.type = "text";
                    expandoObject.label = node.Name;
                }
                else
                {
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (ML.Common.Text.IsNumeric(attribute.Value))
                        {
                            int intValue = Convert.ToInt32(attribute.Value);
                            (expandoObject as IDictionary<string, dynamic>)[attribute.Name] = intValue;
                        }
                        else
                        {
                            (expandoObject as IDictionary<string, dynamic>)[attribute.Name] = attribute.Value;
                        }
                    }

                    if (node.Attributes["type"] == null)
                    {
                        (expandoObject as IDictionary<string, dynamic>)["type"] = "text";
                    }
                    if (node.Attributes["label"] == null)
                    {
                        (expandoObject as IDictionary<string, dynamic>)["label"] = node.Name;
                    }
                }

                // Add Values
                if (expandoObject.type == "image")
                {
                   // (expandoObject as IDictionary<string, dynamic>)["value"] = node.InnerText;
                }
                else
                {
                    (expandoObject as IDictionary<string, dynamic>)["value"] = node.InnerText;
                }
            }

            return expandoObjects;
        }


        public static List<string> ExtractCustomList(XmlDocument xml, int intList)
        {
            List<string> lst = new List<string>();
            foreach(XmlNode node in xml.FirstChild)
            {
                if(node.Attributes.GetNamedItem("list") != null)
                {
                    if (node.Attributes.GetNamedItem("list").Value.IndexOf(intList.ToString()) > -1)
                    {
                        lst.Add(node.Name);
                    }
                }
            }

            return lst;
        }



    }
}
