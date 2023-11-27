using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data.Common
{
    public class FNSettingViewModel
    {
        public string Voucher { get; set; }
        public int DebitAccount { get; set; }
        public int CreditAccount { get; set; }
        public string Project { get; set; }

        //public string AccessToken { get; set; }
        //public string RefreshToken { get; set; }
        //public string Scope { get; set; }
        //public string ExpiresIn { get; set; }
        //public string TokenType { get; set; }
        //public string ConnectionString { get; set; }
    }


    public class FNResponse 
    {
        public Voucher Voucher { get; set; }
    }

    public class Voucher
    {
        public string url { get; set; }
        public object Comments { get; set; }
        public string CostCenter { get; set; }
        public string Description { get; set; }
        public string Project { get; set; }
        public string ReferenceNumber { get; set; }
        public string ReferenceType { get; set; }
        public string TransactionDate { get; set; }
        public int VoucherNumber { get; set; }
        public Voucherrow[] VoucherRows { get; set; }
        public int VoucherSeries { get; set; }
        public int Year { get; set; }
        public int ApprovalState { get; set; }
    }

    public class Voucherrow
    {
        public int Account { get; set; }
        public string CostCenter { get; set; }
        public int Credit { get; set; }
        public string Description { get; set; }
        public int Debit { get; set; }
        public string Project { get; set; }
        public bool Removed { get; set; }
        public string TransactionInformation { get; set; }
        public int Quantity { get; set; }
    }

}
