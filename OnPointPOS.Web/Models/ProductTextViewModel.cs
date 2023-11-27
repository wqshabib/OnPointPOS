using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class ProductTextViewModel:Product_Text
    {
        public string LanguageName { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }
    }
}