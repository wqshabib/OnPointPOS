using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Payment : BaseEntity
    {
        public Payment()
        {

        }
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public  Order Order { get; set; }
        [Column("PaymentType")]
        public int TypeId { get; set; }
        //  public virtual PaymentType PaymentType { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentRef { get; set; }
        public string CreditCardNo { get; set; }
        public decimal TipAmount { get; set; }
        public decimal CashCollected { get; set; }
        public decimal CashChange { get; set; }
        public int IsCashSaleDropped { get; set; }
        public int Direction { get; set; }
        public string ProductName { get; set; }
        public  decimal DeviceTotal { get; set; }
        public string PayerRef { get; set; }

        #region Not Mapped Properties
        public Payment(Payment payment)
        {
            Id = payment.Id;
            OrderId = payment.OrderId;
            TypeId = payment.TypeId;
            PaidAmount = payment.PaidAmount;
            ReturnAmount = payment.ReturnAmount;
            PaymentDate = payment.PaymentDate;
            PaymentRef = payment.PaymentRef;
            CreditCardNo = payment.CreditCardNo;
            TipAmount = payment.TipAmount;
            CashCollected = payment.CashCollected;
            CashChange = payment.CashChange;
            IsCashSaleDropped = payment.IsCashSaleDropped;
            Direction = payment.Direction;
            ProductName = payment.ProductName;
            DeviceTotal = payment.DeviceTotal;
            PayerRef = payment.PayerRef;
        }

        [NotMapped]
        public string TypeName { get; set; }
        
        #endregion
    }
}
