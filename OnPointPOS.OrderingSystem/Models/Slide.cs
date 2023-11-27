using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Slide : Result
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }

    }
}