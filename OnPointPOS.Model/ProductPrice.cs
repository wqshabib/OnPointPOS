using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class ProductPrice:BaseEntity
    {
        public virtual int Id { get; set; }
        public virtual Guid ItemId { get; set; }
        
        public virtual decimal PurchasePrice { get; set; }
        public virtual decimal Price { get; set; }
        public virtual Guid OutletId { get; set; }
        public virtual PriceMode PriceMode { get; set; }
        public virtual DateTime Updated { get; set; }
    }
    public enum PriceMode
    {
        Day = 0,
        Night = 1
    }
}
