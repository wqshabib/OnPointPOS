using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Web.Http;



namespace ML.Common.Handlers.Serializers
{
    public class Serializer
    {
        //private readonly Dictionary<string, Handlers.Serializers.ISerializer> _serializers;
        protected static Dictionary<string, Handlers.Serializers.ISerializer> _serializers;

        //protected Serializer()
        //{
        //    _serializers = new Dictionary<string, ISerializer> { { "xml", new XmlSerializer() }, { "plist", new PlistSerializer() }, { "json", new JsonSerializer() } };
        //}

        public static Stream Serialize(object dto)
        {
            return Serialize("json", dto);
        }

        public static Stream Serialize(string format, object dto)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "json";
            }

            _serializers = new Dictionary<string, ISerializer> { { "xml", new XmlSerializer() }, { "plist", new PlistSerializer() }, { "json", new JsonSerializer() } };
            ISerializer serializer = null;
            return _serializers.TryGetValue(format, out serializer) ? serializer.Serialize(dto) : null;
        }




    }
}
