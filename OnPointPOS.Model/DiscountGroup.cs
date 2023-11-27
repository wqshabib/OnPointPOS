using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class DiscountGroup
    {
        [Key]
        public Guid DiscountId { get; set; }
        public string Title { get; set; }
        public decimal Discount { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
