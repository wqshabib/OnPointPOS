using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class CustomerDiscountData
    {
        public List<CustomerCard> CustomerCards { get; set; }
        public List<DiscountGroup> DiscountGroups { get; set; }
        public List<CustomerDiscountGroup> CustomerDiscountGroups { get; set; }
    }
}
