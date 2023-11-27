using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class PricePolicy:BaseEntity
    {
        public virtual int Id { get; set; }
        public virtual int BuyLimit { get; set; }
        public virtual decimal DiscountAmount { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime Updated { get; set; }
    }
}
