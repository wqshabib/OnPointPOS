using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class SaleModel
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Terminal { get; set; }
        public string UserName { get; set; }
		public string Outlet { get; set; }
        public List<CategorySaleDetail> CategorySaleDetail { get; set; }
        public List<PaymentDetail> PaymentDetails { get; set; }
    }
    public class CategorySaleDetail
    {
        public string CategoryName { get; set; }
        public decimal NetSale { get; set; }
        public decimal TotalSale { get; set; }
        public decimal TAXPercentage { get; set; }
        public decimal VAT { get; set; }
        public decimal Percentage25Total { get; internal set; }
        public decimal Percentage25NetTotal { get; internal set; }
        public decimal Percentage25VAT { get; internal set; }
        public decimal Percentage12Total { get; internal set; }
        public decimal Percentage12NetTotal { get; internal set; }
        public decimal Percentage12VAT { get; internal set; }
        public decimal Percentage6Total { get; internal set; }
        public decimal Percentage6NetTotal { get; internal set; }
        public decimal Percentage6VAT { get; internal set; }
        public decimal Percentage0Total { get; internal set; }
        public decimal Percentage0NetTotal { get; internal set; }
        public decimal Percentage0VAT { get; internal set; }
        
        public decimal Qty { get; internal set; }
        public decimal UnitPrice { get; internal set; }
    }
    public class PaymentDetail
    {
        public int PaymentType { get; set; }
        public string PaymentRef { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RoundAmount { get; set; }
        public int PaymentCount { get; set; }
    }

    public class VATDetail
    {
        public DateTime InvoiceDate { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal Net { get; set; }
        public decimal VatSum { get; set; }
        public decimal ExcpectedVat { get; set; }
    }
}