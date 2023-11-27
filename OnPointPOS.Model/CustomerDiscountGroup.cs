using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
   public class CustomerDiscountGroup
    {
        public Guid Id { get; set; }
        public Guid DiscountId { get; set; }
        public Guid  CustomerId { get; set; }
        public bool Deleted { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        
    }
}
