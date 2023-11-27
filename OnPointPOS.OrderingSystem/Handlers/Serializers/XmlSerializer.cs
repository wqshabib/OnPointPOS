using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace ML.Common.Handlers.Serializers
{
    public class XmlSerializer : ISerializer
    {
        public Stream Serialize(object dto)
        {
            var sw = new StringWriter();
            var tw = new XmlTextWriter(sw);
            var ser = new System.Xml.Serialization.XmlSerializer(dto.GetType());
            ser.Serialize(tw, dto);

            tw.Close();
            sw.Close();
            ser = null;

            return new MemoryStream(new UTF8Encoding().GetBytes(sw.ToString()));
        }
    }
}