using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class CmsContent
    {
        public string Name { get; set; }
        public string Html { get; set; }
        public int SortOrder { get; set; }
        public string PageId { get; set; }
    }
}