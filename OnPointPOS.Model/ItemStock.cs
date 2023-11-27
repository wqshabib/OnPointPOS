using System;
using System.ComponentModel.DataAnnotations;

namespace POSSUM.Model
{
    public class ItemStock
    {
        [Key]
        public int Id { get; set; }
        public virtual Guid ItemId { get; set; }        
        public decimal Quantity { get; set; }
        public string BatchNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime Updated { get; set; }

    }
}