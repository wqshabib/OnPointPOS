using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Api.Models
{
    public class CompanyInfo
    {
        public Guid Id { get; set; }
        public string  Name { get; set; }
        public string DBInfo { get; set; }
        public bool Active { get; set; }

    }
}