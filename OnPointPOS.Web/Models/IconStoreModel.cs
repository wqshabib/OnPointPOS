using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class IconStoreModel:IconStore
    {
        public IEnumerable<SelectListItem> Types { get; set; }
        public string ImageUrl { get; set; }
    }
}