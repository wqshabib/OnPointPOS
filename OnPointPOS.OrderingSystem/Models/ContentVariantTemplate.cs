using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class ContentVariantTemplate : Result
    {
        public string ContentVariantTemplateId { get; set; }
        public string ContentTemplateId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
    }


}