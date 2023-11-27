using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class PaymentApi
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int TypeId { get; set; }
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
        public decimal DeviceTotal { get; set; }
        public string PayerRef { get; set; }

        public static PaymentApi ConvertModelToApiModel(Payment payment)
        {
            return new PaymentApi
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                TypeId = payment.TypeId,
                PaidAmount = payment.PaidAmount,
                ReturnAmount = payment.ReturnAmount,
                PaymentDate = payment.PaymentDate,
                PaymentRef = payment.PaymentRef,
                CreditCardNo = payment.CreditCardNo,
                TipAmount = payment.TipAmount,
                CashCollected = payment.CashCollected,
                CashChange = payment.CashChange,
                IsCashSaleDropped = payment.IsCashSaleDropped,
                Direction = payment.Direction,
                ProductName = payment.ProductName,
                DeviceTotal = payment.DeviceTotal,
                PayerRef = payment.PayerRef
            };
        }

        public static Payment ConvertApiModelToModel(PaymentApi payment)
        {
            return new Payment
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                TypeId = payment.TypeId,
                PaidAmount = payment.PaidAmount,
                ReturnAmount = payment.ReturnAmount,
                PaymentDate = payment.PaymentDate,
                PaymentRef = payment.PaymentRef,
                CreditCardNo = payment.CreditCardNo,
                TipAmount = payment.TipAmount,
                CashCollected = payment.CashCollected,
                CashChange = payment.CashChange,
                IsCashSaleDropped = payment.IsCashSaleDropped,
                Direction = payment.Direction,
                ProductName = payment.ProductName,
                DeviceTotal = payment.DeviceTotal,
                PayerRef = payment.PayerRef
            };
        }

        public static Payment UpdateModel(Payment dbObject, PaymentApi payment)
        {
            dbObject.OrderId = payment.OrderId;
            dbObject.TypeId = payment.TypeId;
            dbObject.PaidAmount = payment.PaidAmount;
            dbObject.ReturnAmount = payment.ReturnAmount;
            dbObject.PaymentDate = payment.PaymentDate;
            dbObject.PaymentRef = payment.PaymentRef;
            dbObject.CreditCardNo = payment.CreditCardNo;
            dbObject.TipAmount = payment.TipAmount;
            dbObject.CashCollected = payment.CashCollected;
            dbObject.CashChange = payment.CashChange;
            dbObject.IsCashSaleDropped = payment.IsCashSaleDropped;
            dbObject.Direction = payment.Direction;
            dbObject.ProductName = payment.ProductName;
            dbObject.DeviceTotal = payment.DeviceTotal;
            dbObject.PayerRef = payment.PayerRef;

            return dbObject;
        }
    }
}
