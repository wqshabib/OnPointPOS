using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public static class UtilityConstants
    {
        public static List<int> ListCashBack = new List<int>()
        {
            (int)PaymentTypesEnum.PaidbyCash,
            (int)PaymentTypesEnum.PaidByCreditCard,
            (int)PaymentTypesEnum.MobileCard,
            (int)PaymentTypesEnum.ElveCard,
        };
    }
}
