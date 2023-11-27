using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class ContentSubContent
    {
        public string ContentId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public bool Available { get; set; }
        public bool Included { get; set; }
        public string ContentSubContentGroupId { get; set; }
    }
}