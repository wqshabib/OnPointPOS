using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public enum SaleType
    {
        Restaurant = 1,
        Retail = 2
    }

    public enum CustomerType
    { 
        Deposit, NonDeposit, All
    }

    public enum PaymentTypesEnum
    {
        FreeCoupon = 0,
        PaidbyCash = 1,
        OnCredit = 2,
        PaidByGift = 3,
        PaidByCreditCard = 4,
        PaidByDebitCard = 5,
        PaidByCheque = 6,
        CashBack = 7,
        Returned = 8,
        MobileCard = 9,
        Swish = 10,
        ElveCard = 11,
        CreditNote = 12,
        Beam = 13,
        PaidByAMEXCard = 14,
        OnlineCash = 15
    }
}
