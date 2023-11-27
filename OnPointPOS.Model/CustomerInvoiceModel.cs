using System;
using System.Collections.Generic;


namespace POSSUM.Model
{
    public class CustomerInvoiceModel
    {
        public CustomerInvoice Invoice { get; set; }
        public Customer Customer { get; set; }
        public List<OrderLine> OrderDetails { get; set; }
    }
    public class ReceiptItems
    {
        public Guid ContentGuid { get; set; }
        public bool Extra { get; set; }
        public bool Strip { get; set; }
        public string Text { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public int Quantity { get; set; }

        public string Direction
        {
            get { return (Extra == true ? "+" : "-"); }
        }
    }
}