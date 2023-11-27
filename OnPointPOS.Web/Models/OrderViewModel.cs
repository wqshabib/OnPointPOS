using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class OrderViewModel : Order
    {
        public string CustomerName { get; set; }
        public string PaymentRef { get; set; }
        public string Remarks { get; set; }
        public decimal NetTotal { get; set; }
        public decimal StripeFee { get; set; }
        public string OrderStatusFromStatus
        {
            get
            {
                return Enum.GetName(typeof(OrderStatus), Status);
            }
        }
        public decimal SumAmt { get {
                return Math.Round(OrderTotal,2);    
            }
        }
    }

    public class OrderLineViewModel : OrderLine
    {
        public string ItemName { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal VAT { get; set; }

        public Guid OutletId { get; set; }
    }

    public class ReadyInvoiceModel
    {
        public bool IsMultiCustomerOrders { get; set; }
        public ReadyInvoiceModel()
        {
            OrderViewModel = new List<OrderViewModel>();
        }
        public string CustomerName { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal InvoiceTotal { get; set; }
        public List<OrderViewModel> OrderViewModel { get; set; }
    }
}