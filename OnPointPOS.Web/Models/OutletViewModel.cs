using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class OutletViewModel:Outlet
    {
        public IEnumerable<SelectListItem> BillPrinters { get; set; } 
        public IEnumerable<SelectListItem> KitchenPrinters { get; set; }

        public string OuletId { get; set; }
    }
   
}