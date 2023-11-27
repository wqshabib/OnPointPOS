using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Cms : Result
    {
        public string Name { get; set; }

        public List<CmsContent> CmsContents = new List<CmsContent>();
    }
}