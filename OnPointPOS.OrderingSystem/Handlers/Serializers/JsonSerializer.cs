using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ML.Common.Handlers.Serializers
{
    public class JsonSerializer : ISerializer
    {
        public Stream Serialize(object dto)
        {
            var sw = new StringWriter();
            var tw = new JsonTextWriter(sw);
            var ser = new Newtonsoft.Json.JsonSerializer();
            ser.Serialize(tw, dto);

            tw.Close();
            sw.Close();
            ser = null;

            return new MemoryStream(new UTF8Encoding().GetBytes(sw.ToString()));
        }
    }
}