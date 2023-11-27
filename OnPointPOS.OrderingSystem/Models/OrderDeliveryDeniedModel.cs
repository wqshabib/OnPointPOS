using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class OrderDeliveryDeniedModel
    {
        public string token { get; set; }
        public string companyId { get; set; }
        public string orderId { get; set; }
        public string driverId { get; set; }
        public string reason { get; set; }
    }
}