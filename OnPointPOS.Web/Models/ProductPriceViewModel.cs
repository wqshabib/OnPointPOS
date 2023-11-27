using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class ProductPriceViewModel:ProductPrice
    {

        public string OutletName { get; set; }
        public IEnumerable<SelectListItem> Outlets { get; set; }
        public IEnumerable<SelectListItem> PriceModes { get; set; }
    }
}