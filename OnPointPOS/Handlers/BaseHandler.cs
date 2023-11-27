using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Handlers
{
    public class BaseHandler
    {
        private static string GetPaymentType(int p)
        {
            //var type = Defaults.PaymentTypes.FirstOrDefault(pt => pt.Id == p);
            switch (p)
            {
                case 0:
                    return UI.CheckOutOrder_Method_FreeCoupon; // "Free Coupon";
                case 1:
                    return UI.CheckOutOrder_Method_Cash; // "Kontant";
                case 2:
                    return UI.CheckOutOrder_Method_Account; //"Faktura";
                case 3:
                    return UI.CheckOutOrder_Method_DebitCard; //"Presentkort";
                case 4:
                    return UI.CheckOutOrder_Method_CreditCard; // "Kort";
                case 5:
                    return "Bankkort";
                case 6:
                    return "Check";
                case 7:
                    return UI.CheckOutOrder_Label_CashBack; //"Utbetalning";
                case 8:
                    return UI.Global_Return; //"Retur";
                case 9:
                    return UI.CheckOutOrder_Method_Mobile; //"Retur";
                case 10:
                    return "Swish";
                case 11:
                    return "Elevkort";
                case 12: return UI.CheckOutOrder_Method_CreditNote;
                case 13: return UI.CheckOutOrder_Method_Beam;
                case 14: return "AMEX";
            }
            return "Annan";
        }
    }
}
