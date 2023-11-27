using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class ReceiptApi
    {
        public Guid ReceiptId { get; set; }
        public Guid TerminalId { get; set; }
        public int TerminalNo { get; set; }
        public long ReceiptNumber { get; set; }
        public Guid OrderId { get; set; }
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

        public static ReceiptApi ConvertModelToApiModel(Receipt receipt)
        {
            return new ReceiptApi
            {
                ReceiptId = receipt.ReceiptId,
                TerminalId = receipt.TerminalId,
                TerminalNo = receipt.TerminalNo,
                ReceiptNumber = receipt.ReceiptNumber,
                OrderId = receipt.OrderId,
                ReceiptCopies = receipt.ReceiptCopies,
                GrossAmount = receipt.GrossAmount,
                VatAmount = receipt.VatAmount,
                VatDetail = receipt.VatDetail,
                PrintDate = receipt.PrintDate,
                ControlUnitName = receipt.ControlUnitName,
                ControlUnitCode = receipt.ControlUnitCode,
                CustomerPaymentReceipt = receipt.CustomerPaymentReceipt,
                MerchantPaymentReceipt = receipt.MerchantPaymentReceipt,
                IsSignature = receipt.IsSignature
            };
        }

        public static Receipt ConvertApiModelToModel(ReceiptApi receipt)
        {
            return new Receipt
            {
                ReceiptId = receipt.ReceiptId,
                TerminalId = receipt.TerminalId,
                TerminalNo = receipt.TerminalNo,
                ReceiptNumber = receipt.ReceiptNumber,
                OrderId = receipt.OrderId,
                ReceiptCopies = receipt.ReceiptCopies,
                GrossAmount = receipt.GrossAmount,
                VatAmount = receipt.VatAmount,
                VatDetail = receipt.VatDetail,
                PrintDate = receipt.PrintDate,
                ControlUnitName = receipt.ControlUnitName,
                ControlUnitCode = receipt.ControlUnitCode,
                CustomerPaymentReceipt = receipt.CustomerPaymentReceipt,
                MerchantPaymentReceipt = receipt.MerchantPaymentReceipt,
                IsSignature = receipt.IsSignature
            };
        }

        public static Receipt UpdateModel(Receipt dbObject, ReceiptApi receipt)
        {
            dbObject.TerminalId = receipt.TerminalId;
            dbObject.TerminalNo = receipt.TerminalNo;
            dbObject.ReceiptNumber = receipt.ReceiptNumber;
            dbObject.OrderId = receipt.OrderId;
            dbObject.ReceiptCopies = receipt.ReceiptCopies;
            dbObject.GrossAmount = receipt.GrossAmount;
            dbObject.VatAmount = receipt.VatAmount;
            dbObject.VatDetail = receipt.VatDetail;
            dbObject.PrintDate = receipt.PrintDate;
            dbObject.ControlUnitName = receipt.ControlUnitName;
            dbObject.ControlUnitCode = receipt.ControlUnitCode;
            dbObject.CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;
            dbObject.MerchantPaymentReceipt = receipt.MerchantPaymentReceipt;
            dbObject.IsSignature = receipt.IsSignature;

            return dbObject;
        }
    }
}
