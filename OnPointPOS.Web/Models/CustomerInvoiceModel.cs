using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class CustomerInvoiceModel
    {
        public CustomerInvoice Invoice { get; set; }
        public Customer Customer { get; set; }
        public List<OrderLineViewModel> OrderDetails { get; set; }
    }
}