using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.ViewModels
{
    public class CustomerInvoiceModel
    {
        public CustomerInvoice Invoice { get; set; }
        public Customer Customer { get; set; }
        public List<OrderLineViewModel> OrderDetails { get; set; }
    }
}
