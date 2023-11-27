using POSSUM.Model;
using System;
using System.Collections.Generic;

namespace POSSUM.ApiModel
{
    public class SwishOrderApi
    {
        public OrderApi Order { get; set; }
        public decimal SwishAmount { get; set; }
        public string PhoneNo { get; set; }
        public int isCustomer { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }
}
