using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ML.Customer
{
    public class Enums
    {
        public enum CustomerStatus : int
        {
            Active = 1
            , Passive = 2
            , Removed = 3
        }

        public enum CustomerType : int
        {
            None = 0
            , Sms = 1
            , Import = 2
            , Manual = 3
            , Integration = 4
            , Identification = 5
            , Anonymous = 6
            , Cookie = 7
            , PremiumSms = 8
            , Iphone = 9
            , Android = 10
            , WindowsPhone = 11
            , Ipad = 12
            , IpadMini = 13
            , Moffer = 14
            , Booking = 15
            , Api = 16
        }

        public enum Mode : int
        {
            New = 1
            , Edit = 2
            , Remove = 3
            , Search = 4
        }





    }


}
