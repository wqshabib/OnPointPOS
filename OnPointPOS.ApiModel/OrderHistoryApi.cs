using POSSUM.Model;
using System;
using System.Collections.Generic;

namespace POSSUM.ApiModel
{
    public class OrderHistoryApi
    {
        public Guid Id { get; set; }
        public int TableId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal OrderTotal { get; set; }
        public string OrderNoOfDay { get; set; }
        public OrderStatus Status { get; set; }
        public int PaymentStatus { get; set; }
        public int Updated { get; set; }
        public string UserId { get; set; }
        public decimal TaxPercent { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int InvoiceGenerated { get; set; }
        public decimal TipAmount { get; set; }
        public string Comments { get; set; }
        public int ShiftNo { get; set; }
        public int ShiftOrderNo { get; set; }
        public int ShiftClosed { get; set; }
        public int TipAmountType { get; set; }
        public int ZPrinted { get; set; }
        public string CheckOutUserId { get; set; }
        public string OrderComments { get; set; }
        public OrderType Type { get; set; }
        public Guid? CustomerInvoiceId { get; set; }
        public string Bong { get; set; }
        public Guid TerminalId { get; set; }
        public Guid OutletId { get; set; }
        public bool TrainingMode { get; set; }
        public decimal RoundedAmount { get; set; }
        public string DailyBong { get; set; }
        public string ExternalID { get; set; }
        public bool IsVismaInvoiceGenerated { get; set; }
        public int? OrderIntID { get; set; }
        public bool IsInInvoice { get; set; }
        public int OrderSource { get; set; }
        public int WC_ID { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal DiscountTax { get; set; }
        public bool IsOnlineOrder { get; set; }
        public string FnResponse { get; set; }
        public List<OrderLineApi> OrderLines { get; set; }
        public List<PaymentApi> Payments { get; set; }
        public List<ReceiptApi> Receipts { get; set; }

        public static OrderHistoryApi ConvertModelToApiModel(Order order)
        {
            return new OrderHistoryApi
            {
                Id = order.Id,
                TableId = order.TableId,
                CustomerId = order.CustomerId,
                CreationDate = order.CreationDate,
                OrderTotal = order.OrderTotal,
                OrderNoOfDay = order.OrderNoOfDay,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Updated = order.Updated,
                UserId = order.UserId,
                TaxPercent = order.TaxPercent,
                InvoiceNumber = order.InvoiceNumber,
                InvoiceDate = order.InvoiceDate,
                InvoiceGenerated = order.InvoiceGenerated,
                TipAmount = order.TipAmount,
                Comments = order.Comments,
                ShiftNo = order.ShiftNo,
                ShiftOrderNo = order.ShiftOrderNo,
                ShiftClosed = order.ShiftClosed,
                TipAmountType = order.TipAmountType,
                ZPrinted = order.ZPrinted,
                CheckOutUserId = order.CheckOutUserId,
                OrderComments = order.OrderComments,
                Type = order.Type,
                CustomerInvoiceId = order.CustomerInvoiceId,
                Bong = order.Bong,
                TerminalId = order.TerminalId,
                OutletId = order.OutletId,
                TrainingMode = order.TrainingMode,
                RoundedAmount = order.RoundedAmount,
                DailyBong = order.DailyBong,
                ExternalID = order.ExternalID,
                IsVismaInvoiceGenerated = order.IsVismaInvoiceGenerated,
                OrderIntID = order.OrderIntID,
                IsInInvoice = order.IsInInvoice,
                OrderSource = order.OrderSource,
                WC_ID = order.WC_ID,
                DiscountTotal = order.DiscountTotal,
                DiscountTax = order.DiscountTax,
                IsOnlineOrder = order.IsOnlineOrder,
                FnResponse = order.FnResponse
            };
        }
    }
}
