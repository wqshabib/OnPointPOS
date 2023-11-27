using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class PendingOrderModel
    {
        public string OrderGuid { get; set; }
        public string CustomerGuid { get; set; }
        public int OrderNo { get; set; }
        public string Text { get; set; }
        public string OrderPrinterGuid { get; set; }
        public string TimeStamp { get; set; }
        public int DailyOrderNo { get; set; }
        public int OrderID { get; set; }
        public bool PaymentRequired { get; set; }
        public string PostalGuid { get; set; }
        public int OrderSCMStatusID { get; set; }
        public decimal DeliveryFee { get; set; }
        public string DeliveryDateTime { get; set; }
        public string PickupDateTime { get; set; }
        public decimal CustomerPaymentFee { get; set; }
        public bool? Notification { get; set; }
        public string DiscountGuid { get; set; }
        public string ExternalTrackingNo { get; set; }
        public string ExternalMessage { get; set; }
        public string ResturantName { get; set; }
        public string CustomerName { get; set; }
        public decimal? Total { get; set; }
        public int? ItemsCount { get; set; }
        public bool showCategory { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? VATPercentage { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
        public RestaurantDetails RestaurantDetails { get; set; }
        public List<OrderItem> OrderItem { get; set; }
        public List<int> OrderStatuses { get; set; }

        public bool IsDeniable { get; set; }
        public bool MergedApp { get; set; }
        public string ColorCode { get; set; }

        public int Type { get; set; }




    }

    public class CustomerDetails
    {
        public string DeliveryMessage { get; set; }
        public int ApartmentNumber { get; set; }
        public int Floor { get; set; }
        public string DoorCode { get; set; }
        public string Address { get; set; }
        public string StreetDistance { get; set; }
        public string StreetNumberLetter { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string LatLong { get; set; }
        public string PhoneNo { get; set; }

    }

    public class RestaurantDetails
    {
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
    }


    public class OrderItem
    {
        public string Title { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public string ContentVariant { get; set; }
        public List<ReceiptItems> ReceiptItems { get; set; }

        public decimal GrossTotal { get { return Price * Quantity; } }


        public virtual decimal NetAmount()
        {
            return GrossTotal / (1 + (VAT / 100));
        }

        public virtual decimal VatAmount()
        {
            return GrossTotal - NetAmount();
        }
    }


   
}
