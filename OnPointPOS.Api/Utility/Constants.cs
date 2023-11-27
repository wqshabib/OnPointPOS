using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace POSSUM.Api.Utility
{
    public static class Constants
    {
        public const string HMAC_KEY = "24537d536b386e2c73454d582e6526432630577d24783078246f71347c2d7e545d336c4824726a714969747b2a636b74216d707261405b7e3a7d58772b2b6573";
        public const string MERCHANT = "90193914";
        public static bool bTest = string.IsNullOrEmpty(ConfigurationManager.AppSettings["bTest"]) ? false : Convert.ToBoolean(ConfigurationManager.AppSettings["bTest"]);
        public static string serviceGift = ConfigurationManager.AppSettings["Serviceavgift"];
        public static string Action = "https://payment.architrade.com/paymentweb/start.action";

        public static string OrderUniqueId = ConfigurationManager.AppSettings["OrderUniqueId"];
         
        public static string CompanyName { get; set; }

        public static string PayBaseUrl
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["PayBaseUrl"]) ? "http://pay.moblink.se" : ConfigurationManager.AppSettings["PayBaseUrl"]; }
        }


        public static string Title
        {
            get
            {
                if (bTest)
                    return string.Format("{0} - Betalning <span style='color:#f00;' class='msgAppMode'>(OBS! TEST)</span>", CompanyName);
                else
                    return string.Format("{0} - Betalning", CompanyName);
            }
        }



    }

}