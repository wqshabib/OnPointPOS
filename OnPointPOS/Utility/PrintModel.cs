using System;
using System.Collections.Generic;
using POSSUM.Model;
using POSSUM.Views.PrintOrder;

namespace POSSUM
{
    public class PrintModel
    {
        public string ReceiptNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Cashier { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal NetPayable
        {
            get
            {
                return GrandTotal + Tip;
            }
        }
        public decimal Tip { get; set; }
        public decimal RoundedAmount { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VAT { get; set; }
        public decimal InvoiceVAT { get; set; }
        public decimal InvoicePercentage { get; set; }
        public decimal InvoiceFee { get; set; }
        public int IntegerPart { get; set; }
        public decimal FractionalPart { get; set; }
        public decimal CollectedCash { get; set; }
        public decimal CollectCard { get; set; }
        public decimal CashBack { get; set; }
        public decimal TipAmount { get; set; }
        public string ControlUnitName { get; set; }
        public string ControlUnitCode { get; set; }
        public string CustomerPaymentReceipt { get; set; }
        public string MarchantPaymentReceipt { get; internal set; }
        public Order OrderMaster { get; set; }
        public List<OrderLine> Items { get; set; }
        public List<Payment> Payments { get; set; }
        public List<VATModel> VatDetails { get; set; }
        public List<VAT> VATAmounts { get; set; }
        public string TaxDesc { get; set; }
        public string Footer { get; set; }
        public string Comment { get; set; }
        public CompanyInfo CompanyInfo { get; set; }

        public string Customer { get; set; }

        public string AccountNumber { get; set; }
        public string PaymentReceiver { get; set; }

        public string ReferenceNo { get; set; }
        public string FakturaDate { get; set; }
        public string Header { get; internal set; }
        public string Address { get; internal set; }
        public bool HasTable => OrderMaster.TableId > 0;
        public bool HasControlUnit => !string.IsNullOrEmpty(ControlUnitName);
        public string Bong { get; set; }
        public decimal NewBalance { get; set; }
        public decimal OldBalance { get; set; }
        public bool HasBalanceValue { get; set; }
        public List<DiscountsViewModel> Discounts { get; set; }
        public bool HasDiscounts { get; set; }
    }

    public class VATModel : VAT
    {
        public VATModel(decimal vat, decimal vatAmount) : base(vat, vatAmount)
        {
        }

        public decimal NetAmount { get; set; }
        public decimal Total { get; set; }

        public VAT GetVatAmounts()
        {
            return this;
        }
    }
}