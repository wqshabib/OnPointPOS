using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class SubContentGroup : Result
    {
        public string ContentSubContentGroupId { get; set; }
        public string Name { get; set; }
        public int Max { get; set; }
        public bool Mandatory { get; set; }
    }




}