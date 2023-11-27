using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class FoodTableViewModel:FoodTable
    {
        public List<SelectListItem> Floors { get; set; }
    }
}