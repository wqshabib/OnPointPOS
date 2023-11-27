using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ML.Common.Handlers.Serializers
{
    public class PlistSerializer : ISerializer
    {
        public Stream Serialize(object dto)
        {
            var plist = Plist.PlistDocument.CreateDocument(dto);
            Stream result = new MemoryStream(new UTF8Encoding().GetBytes(plist));
            return result;

        }
    }
}