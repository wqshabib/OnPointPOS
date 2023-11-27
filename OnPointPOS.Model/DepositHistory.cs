using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class DepositHistory : BaseEntity
    {
        [Key]
        public long Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TerminalId { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DepositType DepositType { get; set; }
        public string CustomerReceipt { get; set; }
        public string MerchantReceipt { get; set; }
    }

    public class DepositHistoryViewModel
    {
        public string DebitAmount { get; set; }
        public string CreditAmount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DepositType DepositType { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public Guid? OrderId { get; set; }
        public string CustomerReceipt { get; set; }
        public long Id { get; set; }
        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }
    }

    public enum DepositType
    {
        Debit=0,
        CreditViaCard=1,
        CreditViaCash=2,
        CreditViaReturnOrder=3
    }
}
