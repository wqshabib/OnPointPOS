using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class JournalViewModel : Journal
    {
        public string CreatedDateString { get; set; }
    }
}