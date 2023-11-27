using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSSUM.Model
{

    public class CheckOutModel
    {
        public CheckOutModel()
        {
            CurrencySymbol = Defaults.RegionInfo.ISOCurrencySymbol;
        }
        public decimal TotalBillAmount { get; set; }
        public decimal TipAmount { get; set; }
        public decimal TotalBalanceAmount { get; set; }
        public decimal PaidCashAmount { get; set; }
        public decimal PaidOthersAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal RoundedAmount { get; set; }
        public decimal CashBackAmount { get; set; }
        public decimal ReturnedAmount { get; set; }
        public decimal ReceivedPayments { get; set; }
        public string CurrencySymbol { get; set; }

    }
    public class TipModViewModel
    {
        public string tipMode { get; set; }
        public decimal tipModeAmount { get; set; }
    }
}
