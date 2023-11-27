using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class SettingViewModel : Setting
    {
        public IEnumerable<SelectListItem> Types { get; set; }
        public IEnumerable<SelectListItem> Outlets { get; set; }
        public IEnumerable<SelectListItem> Terminals { get; set; } 
        public IEnumerable<SelectListItem> Codes { get; set; }

    }
   
   
}