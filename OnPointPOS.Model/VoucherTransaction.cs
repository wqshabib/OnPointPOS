using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class VoucherTransaction : BaseEntity
    {
        public VoucherTransaction()
        {
            Canceled = false;
            TransactionDate = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Product Product { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ErsReference { get; set; }
        public bool Canceled { get; set; }
        [NotMapped]
        public string ItemName { get; set; }
        [NotMapped]
        public string SKU { get; set; }
    }
}