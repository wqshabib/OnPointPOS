using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.ViewModels
{
    public class OrderLineViewModel : OrderLine
    {
        public string ItemName { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal VAT { get; set; }

        public Guid OutletId { get; set; }
    }
}
