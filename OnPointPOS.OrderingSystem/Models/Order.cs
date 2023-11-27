
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Order : Result
    {
        //public class Order : Result
        //{
        //    public string OrderGuid { get; set; }
        //    public int OrderId { get; set; }
        //    //public int OrderNo { get; set; }
        //    public int DailyOrderNo { get; set; }
        //    public DateTime TimeStamp { get; set; }
        //    public string OrderPrinter { get; set; }
        //    public string CustomerFullName { get; set; }
        //    public decimal TotalAmount { get; set; }
        //    public int StatusId { get; set; }
        //    public decimal TotalPaymentAmount { get; set; }

        //    public string Receipt { get; set; }
        //    public string PaymentId { get; set; }

        //    public int SCMStatusId { get; set; }
        //    public Customer Customer { get; set; }
        //    //public List<SCMOrderItem> SCMOrderItems = new List<SCMOrderItem>();
        //    public string SCMOrderList { get; set; }

        //    public string DeliveryDateTime { get; set; }

        //    public string InvoiceGuid { get; set; }
        //    public DateTime LastRetryDateTime { get; set; }
        //    public DateTime NextReminderDateTime { get; set; }
        //    public string Ocr { get; set; }
        //    public List<CustomerOrderStatus> OrderStatuses { get; set; }
        //    public List<CustomerPayment> OrderPayments { get; set; }
        //}

        public class CustomerOrder : Result
        {
            public string OrderGuid { get; set; }
            public bool Test { get; set; }
        }
        //public class CustomerPayment
        //{
        //    public CustomerPayment(tPayment payment)
        //    {
        //        this.PaymentGuid = payment.PaymentGuid;
        //        this.CompanyGuid = payment.CompanyGuid;
        //        this.PaymentStatus = (PaymentRepository.PaymentStatus)Enum.Parse(typeof(PaymentRepository.PaymentStatus), payment.PaymentStatusID.ToString());
        //        this.PaymentType = (ML.Payment.PaymentType.Type) Enum.Parse(typeof(ML.Payment.PaymentType.Type), payment.PaymentTypeID.ToString());
        //        this.CustomerGuid = payment.CustomerGuid;
        //        this.PhoneNo = payment.PhoneNo;
        //        this.Amount = payment.Amount;
        //        this.VAT = payment.VAT;
        //        this.OrderGuid = payment.OrderGuid;
        //        this.PaymentOrderNo = payment.PaymentOrderNo;
        //        this.StatusText = payment.StatusText;
        //        this.Description = payment.Description;
        //        this.Log = payment.Log;
        //        this.TimeStamp = payment.TimeStamp;
        //        this.Notified = payment.Notified;
        //        this.ExternalTrackingNo = payment.ExternalTrackingNo;
        //        this.ExternalMessage = payment.ExternalMessage;
        //        this.Rollbacked = payment.Rollbacked;
        //        this.TransactionNo = payment.TransactionNo;
        //        this.Test = payment.Test;
        //        this.Notification = payment.Notification;
        //    }
        //    public System.Guid PaymentGuid { get; set; }
        //    public System.Guid CompanyGuid { get; set; }
        //    public System.Guid CustomerGuid { get; set; }
        //    public string PhoneNo { get; set; }
        //    public decimal Amount { get; set; }
        //    public decimal VAT { get; set; }
        //    public System.Guid OrderGuid { get; set; }
        //    public string PaymentOrderNo { get; set; }
        //    public PaymentRepository.PaymentStatus PaymentStatus { get; set; }
        //    public ML.Payment.PaymentType.Type PaymentType { get; set; }
        //    public string StatusText { get; set; }
        //    public string Description { get; set; }
        //    public string Log { get; set; }
        //    public System.DateTime TimeStamp { get; set; }
        //    public bool Notified { get; set; }
        //    public string ExternalTrackingNo { get; set; }
        //    public string ExternalMessage { get; set; }
        //    public bool Rollbacked { get; set; }
        //    public string TransactionNo { get; set; }
        //    public bool Test { get; set; }
        //    public Nullable<bool> Notification { get; set; }
        //}


        //public class OrderStat : Result
        //{
        //    public OrderStat()
        //    {
        //    }

        //    public OrderStat(string date, int count)
        //    {
        //        Date = date;
        //        Count = count;
        //    }

        //    public string Date { get; set; }
        //    public int Count { get; set; }
        //}


        //public class CustomerOrderStatus : Result
        //{
        //    public CustomerOrderStatus()
        //    {
        //    }
        //    public CustomerOrderStatus(tOrderStatus status)
        //    {
        //        this.OrderStatus = (OrderStatusRepository.OrderStatus)Enum.Parse(typeof(OrderStatusRepository.OrderStatus), status.OrderStatusTypeID.ToString());
        //        this.OrderStatusMessage = status.Message;
        //        this.OrderStatusText = status.ReasonText;
        //        this.TimeStamp = status.TimeStamp;
        //        this.Sequence = status.Sequence;
        //    }
        //    public OrderStatusRepository.OrderStatus OrderStatus { get; set; }        
        //    public string OrderStatusText { get; set; }
        //    public string OrderStatusMessage { get; set; }
        //    public string OrderStatusMessageShort { get; set; }
        //    public System.DateTime TimeStamp { get; set; }
        //    public int Sequence { get; set; }


        //}




        //public class PendingOrder : Result
        //{
        //    public string OrderGuid { get; set; }
        //    public string CustomerGuid { get; set; }
        //    public int OrderNo { get; set; }
        //    public string Text { get; set; }
        //    public string OrderPrinterGuid { get; set; }
        //    public string TimeStamp { get; set; }
        //    public int DailyOrderNo { get; set; }
        //    public int OrderID { get; set; }
        //    public bool PaymentRequired { get; set; }
        //    public string PostalGuid { get; set; }
        //    public int OrderSCMStatusID { get; set; }
        //    public decimal DeliveryFee { get; set; }
        //    public string DeliveryDateTime { get; set; }
        //    public string PickupDateTime { get; set; }
        //    public decimal CustomerPaymentFee { get; set; }
        //    public bool? Notification { get; set; }
        //    public string DiscountGuid { get; set; }
        //    public string ExternalTrackingNo { get; set; }
        //    public string ExternalMessage { get; set; }
        //    public string ResturantName { get; set; }
        //    public string CustomerName { get; set; }
        //    public decimal? Total { get; set; }
        //    public int? ItemsCount { get; set; }
        //    public bool showCategory { get; set; }
        //    public decimal? VATAmount { get; set; }
        //    public decimal? VATPercentage { get; set; }
        //    public CustomerDetails CustomerDetails { get; set; }
        //    public RestaurantDetails RestaurantDetails { get; set; }
        //    public List<OrderItem> OrderItem { get; set; }
        //    public List<int> OrderStatuses{ get; set; }

        //    public bool IsDeniable { get; set; }
        //    public bool MergedApp { get; set; }
        //    public string ColorCode { get; set; }

        //    public OrderType Type { get; set; }

        //    public PendingOrder(tOrder order)
        //    {   
        //        this.OrderGuid = order.OrderGuid.ToString();
        //        this.CustomerGuid = order.CustomerGuid.ToString();
        //        this.OrderNo = order.OrderNo;
        //        this.Text = order.Text;
        //        this.OrderPrinterGuid = order.OrderPrinterGuid.ToString();

        //        this.TimeStamp = order.TimeStamp.ToString("yyyy-MM-dd HH:mm"); ;
        //        this.DailyOrderNo = order.DailyOrderNo;
        //        this.OrderID = order.OrderID;
        //        this.PaymentRequired = order.PaymentRequired;
        //        this.PostalGuid = order.PostalGuid.ToString();

        //        this.OrderSCMStatusID = order.OrderSCMStatusID;
        //        this.DeliveryFee = order.DeliveryFee;
        //        this.DeliveryDateTime = order.DeliveryDateTime.ToString("yyyy-MM-dd HH:mm"); 
        //        this.PickupDateTime = order.PickupDateTime.HasValue ? order.PickupDateTime.Value.ToString("yyyy-MM-dd HH:mm") : order.DeliveryDateTime.ToString("yyyy-MM-dd HH:mm"); 
        //        this.CustomerPaymentFee = order.CustomerPaymentFee;
        //        this.Notification = order.Notification;

        //        this.DiscountGuid = order.DiscountGuid.ToString();
        //        this.ExternalTrackingNo = order.ExternalTrackingNo;
        //        this.ExternalMessage = order.ExternalMessage;
        //        this.CustomerName = order.tCustomer.FirstName + " " + order.tCustomer.LastName;
        //        this.ResturantName = order.tOrderPrinter.Name;

        //        this.ItemsCount = order.tOrderItem.Count(o => o.Price > 0);
        //        this.Total = order.tOrderItem.Sum(o => (o.Price * o.Quantity));
        //        this.showCategory = order.tOrderPrinter.ShowCategory;
        //        OrderType orderType;
        //        Enum.TryParse(order.Type.ToString(), true, out orderType);
        //        this.Type = orderType;
        //        //this.Type = (order.PostalGuid != Guid.Empty) ? OrderType.DeliveryOrder : OrderType.PickupOrder;
        //        this.OrderStatuses = order.tOrderStatus.Select(i => i.OrderStatusTypeID).ToList();
        //        //Shop Printer
        //        if (this.OrderStatuses.Contains((int)OrderStatusRepository.OrderStatus.TimeOut) || this.OrderStatuses.Contains((int)OrderStatusRepository.OrderStatus.Rejected))
        //            this.ColorCode = "#FF0000";
        //        else if (this.OrderStatuses.Contains((int)OrderStatusRepository.OrderStatus.ReadyToPickup))
        //            this.ColorCode = "#000000";
        //        else
        //            this.ColorCode = "#23FF48";
        //        var customerDetails = new CustomerDetails();
        //        if (order.PostalGuid != Guid.Empty || (order.PostalGroupGuid != null && order.PostalGroupGuid!=Guid.Empty))
        //        {
        //            DB.tPostal postal = new DB.PostalRepository().GetPostalAddress(order.PostalGuid, order.PostalGroupGuid);
        //            var delivery = order.tOrderDelivery.FirstOrDefault();
        //            if (order.PostalGroupGuid != Guid.Empty && order.PostalGroupGuid != null)
        //            {
        //                customerDetails.Address = postal.Street;
        //                if(delivery!=null)
        //                    customerDetails.Address += " " + delivery.StreetNumber;
        //            }   
        //            else
        //                customerDetails.Address = postal.Street + " " + postal.StreetNumber;
        //            customerDetails.ZipCode = postal.ZipCode;
        //            customerDetails.City = postal.City;

        //            customerDetails.StreetDistance = !string.IsNullOrEmpty(postal.StreetDistance) ? postal.StreetDistance : string.Empty;
        //            customerDetails.LatLong = (!string.IsNullOrEmpty(postal.LatLong))?postal.LatLong:string.Empty;

        //            if (delivery != null)
        //            {
        //                customerDetails.DoorCode = (!string.IsNullOrEmpty(delivery.DoorCode)) ? delivery.DoorCode : string.Empty;
        //                customerDetails.DeliveryMessage = (!string.IsNullOrEmpty(delivery.DeliveryMessage)) ? delivery.DeliveryMessage : string.Empty;
        //                if (delivery.Floor>0)
        //                    customerDetails.Floor = delivery.Floor;
        //                if (delivery.ApartmentNumber > 0)
        //                    customerDetails.ApartmentNumber = delivery.ApartmentNumber;
        //            }
        //        }
        //        customerDetails.PhoneNo = (!string.IsNullOrEmpty(order.tCustomer.PhoneNo)) ? order.tCustomer.PhoneNo : string.Empty;

        //        var resAddress = new RestaurantDetails
        //        {
        //            City = order.tOrderPrinter.City,
        //            ZipCode = order.tOrderPrinter.ZipCode,
        //            Address = order.tOrderPrinter.Address
        //        };
        //        this.CustomerDetails = customerDetails;
        //        this.RestaurantDetails = resAddress;
        //        //NEED TO SET DINAMIC VALUES UNDER DRIVIFY.GetOrderDetail
        //        this.IsDeniable = false;
        //    }

        //    public void SetDrivifyColorCode()
        //    {
        //        if (this.OrderStatuses.Contains((int)OrderStatusRepository.OrderStatus.TimeOut) || this.OrderStatuses.Contains((int)OrderStatusRepository.OrderStatus.Rejected))
        //            this.ColorCode = "#FF0000";
        //        else if (this.OrderStatuses.Contains((int)OrderStatusRepository.OrderStatus.Delivered))
        //            this.ColorCode = "#000000";
        //        else
        //            this.ColorCode = "#23FF48";
        //    }
        //}

        //public class CustomerDetails
        //{
        //    public string DeliveryMessage { get; set; }
        //    public int ApartmentNumber { get; set; }
        //    public int Floor { get; set; }
        //    public string DoorCode { get; set; }
        //    public string Address { get; set; }
        //    public string StreetDistance { get; set; }
        //    public string StreetNumberLetter { get; set; }
        //    public int ZipCode { get; set; }
        //    public string City { get; set; }
        //    public string LatLong { get; set; }
        //    public string PhoneNo { get; set; }

        //}

        //public class RestaurantDetails
        //{
        //    public string Address { get; set; }
        //    public string ZipCode { get; set; }
        //    public string City { get; set; }
        //}


        //public class OrderItem
        //{
        //    public string Title { get; set; }      
        //    public int Quantity { get; set; }
        //    public string Category { get; set; }
        //    public decimal Price { get; set; }
        //    public decimal VAT { get; set; }
        //    public string ContentVariant { get; set; }
        //    public List<ReceiptItems> ReceiptItems { get; set; }
        //}


        //public class ReceiptItems
        //{
        //    public Guid ContentGuid { get; set; }
        //    public bool Extra { get; set; }
        //    public bool Strip { get; set; }
        //    public string Text { get; set; }
        //    public decimal Price { get; set; }
        //    public decimal VAT { get; set; }
        //    public int Quantity { get; set; }       

        //}

    }

}