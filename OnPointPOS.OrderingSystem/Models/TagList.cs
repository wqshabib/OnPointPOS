using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class TagList : Result
    {
        public string TagListId { get; set; }
        public string Name { get; set; }
        public List<Tag> Tags { get; set; }
    }

    public class Tag
    {
        public string TagListId { get; set; }
        public string TagId { get; set; }
        public string Name { get; set; }
    }
}