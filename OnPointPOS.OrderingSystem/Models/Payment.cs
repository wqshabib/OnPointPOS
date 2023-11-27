using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class Payment : Result
    {
        public string PaymentId { get; set; }
        public string PaymentType { get; set; }
        public string CustomerId { get; set; }
        public string PhoneNo { get; set; }
        public decimal Amount { get; set; }
        public decimal VAT { get; set; }
        public string OrderId { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionNo { get; set; }
        public string OrderPrinter { get; set; }
    }



}