using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class Result
    {
        public int Status { get; set; }
        public string StatusText { get; set; }
    }
}