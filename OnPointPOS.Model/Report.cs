using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public enum ReportType
    {
        XReport,
        ZReport,
        PaymentPurchase,
        PaymentRefund,
        CustomerReceipt
    }

    public class Report : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime CreationDate { get; set; }
        public int ReportType { get; set; }
        public int ReportNumber { get; set; }
        public Guid TerminalId { get; set; }

        [ForeignKey("TerminalId")]
        public virtual Terminal Terminal { get; set; }
    }
}