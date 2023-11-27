using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUETemplateSite.Models
{
    public class CheckoutModel : SiteModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telephone { get; set; }
        public string DoorCode { get; set; }
        public string AppartmentNo { get; set; }
        public string Floor { get; set; }
        public string DeliveryMessage { get; set; }
        public string StreetNo { get; set; }
        public string DeliveryType { get; set; }
        public string PayType { get; set; }
    }
}