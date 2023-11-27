using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class ContentType : Result
    {
        public string ContentTypeId { get; set; }
        public string Name { get; set; }
    }

}