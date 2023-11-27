using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Invoice : Result
    {
        public System.Guid InvoiceGuid { get; set; }
        public System.Guid OrderGuid { get; set; }
        public string CompanyName { get; set; }
        public string InvoiceTitle { get; set; }
        public string InvoiceText { get; set; }
        public string OrderText { get; set; }
        public string ContentText { get; set; }
        public decimal Price { get; set; }
        public decimal ReminderFee { get; set; }
        public string EmailSubject { get; set; }
        public string EmailText { get; set; }
        public string Reference { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Ocr { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastReminder { get; set; }
        public DateTime NextReminder { get; set; }
    }

    public class CustomerInvoice : Result
    {   
        public string StatusMessage { get; set; }
    }
}