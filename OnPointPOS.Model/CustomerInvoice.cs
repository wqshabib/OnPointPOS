using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class CustomerInvoice : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public long InvoiceNumber { get; set; }
        public Guid CustomerId { get; set; }
        [MaxLength(250)]
        public string Remarks { get; set; }
        public decimal InvoiceTotal { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Synced { get; set; }
        public int PaymentStatus { get; set; }

        public DateTime? PaidDate { get; set; }

        public DateTime? DueDate { get; set; }
        public Guid OutletId { get; set; }
        public Guid TerminalId { get; set; }
        [NotMapped]
        public virtual List<Guid> OrdersGuid { get; set; }
        [NotMapped]
        public virtual string PaidStatus
        {
            get
            {
                switch (PaymentStatus)
                {
                    case 0:
                        return "Ej betald";

                    case 1:
                        return "Betald";
                    case 2:
                        return "Delbetald";
                    case 3:
                        return "på Credit";
                    default:
                        return "Okänd";
                }
            }
        }
    }
}