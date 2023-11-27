using System;

namespace POSSUM.Model
{
    public class InvoiceCounter:BaseEntity
    {
        public virtual int Id { get; set; }
        public virtual string LastNo { get; set; }
        public virtual InvoiceType InvoiceType { get; set; }
    }

    public enum InvoiceType
    {
        Standard = 0,
        Customer = 1
    }
}
