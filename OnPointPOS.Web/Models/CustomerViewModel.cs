using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class CustomerViewModel:Customer
    {
        public string Edit { get; set; }
        public string Delete { get; set; }
        public string Status { get; set; }
    }
}