using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class DeliveryFeeModel:Result
    {
        public Guid OrderPrinterGuid { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DeliveryMinimumAmount { get; set; }
        
    }
}