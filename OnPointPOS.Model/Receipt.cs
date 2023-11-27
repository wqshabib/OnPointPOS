using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Receipt : BaseEntity
    {
        public Receipt()
        {

        }
        public Guid ReceiptId { get; set; }
        public Guid TerminalId { get; set; }
        public int TerminalNo { get; set; }
        public long ReceiptNumber { get; set; }
        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public  Order Order { get; set; }
        public Byte ReceiptCopies { get; set; }

        public decimal GrossAmount { get; set; }
        public decimal VatAmount { get; set; }
        public string VatDetail { get; set; }
        public DateTime PrintDate { get; set; }
        public string ControlUnitName { get; set; }
        public string ControlUnitCode { get; set; }
        public string CustomerPaymentReceipt { get; set; }
        public string MerchantPaymentReceipt { get; set; }
        public bool IsSignature { get; set; }

        #region Not Mapped Properties
        public Receipt(Receipt receipt)
        {
            ReceiptId = receipt.ReceiptId;
            TerminalId = receipt.TerminalId;
            TerminalNo = receipt.TerminalNo;
            ReceiptNumber = receipt.ReceiptNumber;
            OrderId = receipt.OrderId;
            ReceiptCopies = receipt.ReceiptCopies;
            GrossAmount = receipt.GrossAmount;
            VatAmount = receipt.VatAmount;
            VatDetail = receipt.VatDetail;
            PrintDate = receipt.PrintDate;
            ControlUnitName = receipt.ControlUnitName;
            ControlUnitCode = receipt.ControlUnitCode;
            CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;
            MerchantPaymentReceipt = receipt.MerchantPaymentReceipt;
            IsSignature = receipt.IsSignature;
        }
        [NotMapped]
        public List<VAT> VatDetails { get; set; }

        [NotMapped]
        public decimal NegativeAmount { get; set; }

        #endregion Not Mapped Properties
    }

    public class VAT
    {
        public VAT(decimal vat, decimal vatAmount)
        {
            VATPercent = vat;
            VATTotal = vatAmount;
        }

        public decimal VATPercent { get; set; }

        public decimal VATTotal { get; set; }

        public static decimal NetFromGross(decimal unitPriceWithVat, decimal vatPercent)
        {
            return unitPriceWithVat * (1 - ((vatPercent / 100) / (1 + (vatPercent / 100))));
        }

        public static decimal GrossFromNet(decimal unitPriceWithoutVat, decimal vatPercent)
        {
            return unitPriceWithoutVat * vatPercent / 100;
        }
    }
}