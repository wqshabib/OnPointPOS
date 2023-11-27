using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace POSSUM.Model
{
    public enum OrderType
    {
        Standard = 0,
        TakeAway = 1,
        TableOrder = 2,
        TableTakeAwayOrder = 3,
        TakeAwayReturn = 4,
        Return = 5,
        Cancel = 6
    }

    [Table("OrderMaster")]
    public partial class Order : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public int TableId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal OrderTotal { get; set; }
        [MaxLength(20)]
        public string OrderNoOfDay { get; set; }
        public OrderStatus Status { get; set; }
        public int PaymentStatus { get; set; }
        public int Updated { get; set; }
        public string UserId { get; set; }
        public decimal TaxPercent { get; set; }
        [MaxLength(20)]
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int InvoiceGenerated { get; set; }
        public decimal TipAmount { get; set; }
        public bool IsInInvoice { get; set; }
        public int OrderSource { get; set; }
        public string Comments { get; set; }
        public int ShiftNo { get; set; }
        public int ShiftOrderNo { get; set; }
        public int ShiftClosed { get; set; }
        public int TipAmountType { get; set; }
        public int ZPrinted { get; set; }
        [MaxLength(40)]
        public string CheckOutUserId { get; set; }
        public string OrderComments { get; set; }
        public OrderType Type { get; set; }
        public Guid? CustomerInvoiceId { get; set; }
        public string Bong { get; set; }
        public string DailyBong { get; set; }
        public Guid TerminalId { get; set; }
        public Guid OutletId { get; set; }
        [ForeignKey("OutletId")]
        public Outlet Outlet { get; set; }
        public bool TrainingMode { get; set; }
        public decimal RoundedAmount { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; }
        public IList<Receipt> Receipts { get; set; }
        public IList<Payment> Payments { get; set; }
        //[MaxLength(10)]
        public string ExternalID { get; set; }
        public string FnResponse { get; set; }
        [NotMapped]
        public string FnId { get; set; }

        public bool IsVismaInvoiceGenerated { get; set; }
        public int WC_ID { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal DiscountTax { get; set; }
        public int? OrderIntID { get; set; }
        public bool IsOnlineOrder { get; set; }
        [NotMapped]
        public Customer Customr { get; set; }
        [NotMapped]
        public UserOrder UserOrder { get; set; }
        [NotMapped]
        public DateTime? DeliveryDate { get; set; }
        [NotMapped]
        public bool IsPickup { get; set; }
        [NotMapped]
        public bool DailyBongCounter { get; set; }
        [NotMapped]
        public bool IsForAdult
        {
            get
            {
                var adultText = ConfigurationManager.AppSettings["AdultText"];
                return OrderDirection != -1 && OrderLines != null && OrderLines.FirstOrDefault(a => a.Product != null && a.Product.Description != null && a.Product.Description.Contains(adultText)) != null;
            }
        }

        public static Order New()
        {
            return OrderFactory.CreateEmtpy();
        }

        [NotMapped]
        public bool CheckoutButtonVisibilty { get { return this.Status == OrderStatus.Rejected ? false : true; } }
    }

    public class OrderFactory
    {
        public static Order CreateEmtpy()
        {
            return new Order { OrderLines = new List<OrderLine>() };
        }
    }

    public partial class Order
    {
        public Order GetFrom()
        {
            return new Order
            {
                Id = this.Id,
                InvoiceNumber = this.InvoiceNumber,
                OrderComments = this.OrderComments,
                InvoiceDate = this.InvoiceDate,
                CreationDate = this.CreationDate,
                InvoiceGenerated = this.InvoiceGenerated,
                Bong = this.Bong,
                DailyBong = this.DailyBong,
                CheckOutUserId = this.CheckOutUserId,
                Comments = this.Comments,
                CustomerId = this.CustomerId,
                CustomerInvoiceId = this.CustomerInvoiceId,
                OutletId = this.OutletId,
                TerminalId = this.TerminalId,
                PaymentStatus = this.PaymentStatus,
                OrderNoOfDay = this.OrderNoOfDay,
                ShiftNo = this.ShiftNo,
                RoundedAmount = 0, //this.ShiftNo,
                Status = this.Status,
                OrderTotal = this.OrderTotal,
                Updated = this.Updated,
                ZPrinted = this.ZPrinted,
                TaxPercent = this.TaxPercent,
                Type = this.Type,
                ShiftClosed = this.ShiftClosed,
                ShiftOrderNo = this.ShiftOrderNo,
                TableId = this.TableId,
                UserId = this.UserId,
                TrainingMode = this.TrainingMode,
                TipAmount = this.TipAmount,
                TipAmountType = this.TipAmountType,
            };
        }

        public Order GetPosMiniFrom()
        {
            return new Order
            {
                Id = this.Id,
                InvoiceNumber = this.InvoiceNumber,
                OrderComments = this.OrderComments,
                InvoiceDate = this.InvoiceDate,
                CreationDate = this.CreationDate,
                InvoiceGenerated = this.InvoiceGenerated,
                Bong = this.Bong,
                DailyBong = this.DailyBong,
                CheckOutUserId = this.CheckOutUserId,
                Comments = this.Comments,
                CustomerId = this.CustomerId,
                CustomerInvoiceId = this.CustomerInvoiceId,
                OutletId = this.OutletId,
                TerminalId = this.TerminalId,
                PaymentStatus = this.PaymentStatus,
                OrderNoOfDay = this.OrderNoOfDay,
                ShiftNo = this.ShiftNo,
                RoundedAmount = 0, //this.ShiftNo,
                Status = this.Status,
                OrderTotal = this.OrderTotal,
                Updated = this.Updated,
                ZPrinted = this.ZPrinted,
                TaxPercent = this.TaxPercent,
                Type = this.Type,
                ShiftClosed = this.ShiftClosed,
                ShiftOrderNo = this.ShiftOrderNo,
                TableId = this.TableId,
                UserId = this.UserId,
                TrainingMode = this.TrainingMode,
                TipAmount = this.TipAmount,
                TipAmountType = this.TipAmountType,
                OrderLines = this.OrderLines
            };
        }

        [NotMapped]
        public string TableName { get; set; }
        [NotMapped]
        public string CustomerName { get; set; }
        [NotMapped]
        public virtual string CreatedBy { get; set; }
        [NotMapped]
        public virtual bool IsSelected { get; set; }
        [NotMapped]
        public decimal BalanceAmount { get; set; }
        [NotMapped]
        public FoodTable SelectedTable { get; set; }
        [NotMapped]
        public Receipt Receipt { get; set; }
        [NotMapped]
        public Customer Customer { get; set; }
        [NotMapped]
        public string SelecttedImagePath => IsSelected ? "/POSSUM;component/images/selecteditem.png" : "/POSSUM;component/images/unselecteditem.png";
        [NotMapped]
        public string TaxPercentString => TaxPercent.ToString(CultureInfo.InvariantCulture);
        [NotMapped]
        public virtual string SumAmtString => OrderTotal.ToString(CultureInfo.InvariantCulture);
        [NotMapped]
        public int OrderDirection => Type == OrderType.Return ? -1 : 1;
        [NotMapped]
        public string TipAmountString => TipAmount.ToString(CultureInfo.InvariantCulture);
        [NotMapped]
        public string PaidStatus
        {
            get
            {
                switch (PaymentStatus)
                {
                    case 0:
                        return "Delbetald";

                    case 1:
                        return "Betald";

                    case 2:
                        return "Ej betald";

                    default:
                        return "Okänd";
                }
            }
        }
        [NotMapped]
        public string OrderNumber => Id.ToString();
        [NotMapped]
        public string ReceiptNumber => string.IsNullOrEmpty(InvoiceNumber) ? "0" : InvoiceNumber;
        public OrderType OrderType => Type;
        [NotMapped]
        public decimal GrossAmount => OrderLines != null ? GetGrossAmount(OrderLines) : 0;
        [NotMapped]
        public decimal NetAmount => OrderLines != null ? GetNetAmount(OrderLines) : 0;
        [NotMapped]
        public decimal VatAmount => OrderLines != null ? GetVatAmount(OrderLines) : 0;
        [NotMapped]
        public decimal PartialPaidAmount { get; set; }
        [NotMapped]
        public bool SelectEnable { get { return (IsSelected == true && Status == OrderStatus.New) ? false : true; } }
        [NotMapped]
        public bool ShowReceiptButton { get { return InvoiceGenerated != 1 && PaymentStatus == 1 ? true : false; } }
        [NotMapped]
        public bool ShowPaymentButton { get { return PaymentStatus != 1 ? true : false; } }
        [NotMapped]
        public bool ShowPerformaButton { get { return InvoiceGenerated == 1 ? true : false; } }

        private decimal GetGrossAmount(ICollection<OrderLine> lst)
        {
            decimal OrderTotal = 0;

            foreach (var item in lst)
            {
                if (item.IsValid)
                {
                    OrderTotal += item.GrossAmountDiscounted();

                    if (item.IngredientItems != null)
                        OrderTotal += item.IngredientItems.Where(m => m.IngredientMode == "+").Sum(s => s.GrossAmountDiscounted());
                }
            }

            return OrderTotal;
        }

        private decimal GetVatAmount(ICollection<OrderLine> lst)
        {
            decimal tax = 0;

            foreach (var item in lst)
            {
                if (item.IsValid)
                {
                    if (item.Product != null && item.Product.ItemType == ItemType.Individual)
                        tax += item.VatAmount();

                    if (item.IngredientItems != null)
                        tax += item.IngredientItems.Where(m => m.IngredientMode == "+").Sum(s => s.VatAmount());
                }
            }

            foreach (var detail in lst.Where(p => p.Product != null && p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
            {
                if (detail.ItemDetails != null)
                    tax += detail.ItemDetails.Sum(od => od.VatAmount());
            }

            return tax;

        }

        private decimal GetNetAmount(ICollection<OrderLine> lst)
        {

            decimal OrderTotal = 0;
            decimal tax = 0;
            decimal discount = 0;

            foreach (var item in lst)
            {
                if (item.IsValid)
                {
                    if (item.Product != null && item.Product.ItemType == ItemType.Individual)
                        tax += item.VatAmount();
                    OrderTotal += item.GrossAmountDiscounted(); //MasterOrder.Type==OrderType.Return?item.ReturnGrossAmountDiscounted():
                    discount += item.ItemDiscount;
                    if (item.IngredientItems != null)
                    {
                        tax += item.IngredientItems.Where(m => m.IngredientMode == "+").Sum(s => s.VatAmount());
                        discount += item.IngredientItems.Where(m => m.IngredientMode == "+").Sum(s => s.ItemDiscount);
                        OrderTotal += item.IngredientItems.Where(m => m.IngredientMode == "+").Sum(s => s.GrossAmountDiscounted()); //MasterOrder.Type == OrderType.Return ?ingredient.ReturnGrossAmountDiscounted():
                    }
                }
            }

            foreach (var detail in lst.Where(p => p.Product != null && p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
            {
                if (detail.ItemDetails != null)
                    tax += detail.ItemDetails.Sum(s => s.VatAmount());
            }

            decimal nettotal = OrderTotal - tax;
            return nettotal;
        }

        public void AddLine(Product product, decimal qty, int direction)
        {
            if (OrderLines != null)
                OrderLines = new List<OrderLine>();
            OrderLines.Add(new OrderLine
            {
                Id = Guid.NewGuid(),
                ItemComments = "",
                TaxPercent = product.Tax,
                Quantity = qty,
                UnitPrice = product.Price,
                DiscountedUnitPrice = product.Price,
                PurchasePrice = product.PurchasePrice,
                Active = 1,
                ItemStatus = (int)OrderStatus.New,
                Direction = direction,
                ItemId = product.Id,
                Product = product

            });
        }

        [NotMapped]
        public virtual OrderStatus OrderStatusFromType
        {
            get
            {
                switch (Type)
                {
                    case OrderType.Return:
                        return OrderStatus.ReturnOrder;
                    case OrderType.TakeAwayReturn:
                        return OrderStatus.ReturnOrder;
                    case OrderType.Cancel:
                        return OrderStatus.OrderCancelled;
                    default:
                        return OrderStatus.AssignedKitchenBar;
                }
            }
        }

    }
}