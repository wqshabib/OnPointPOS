using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.Integration
{
    public static class Utility
    {
        public static CultureInfo UICultureInfo = new CultureInfo(ConfigurationManager.AppSettings["Currency"]);

        public static string CultureString
        {
            get
            {
                switch (ConfigurationManager.AppSettings["Language"])
                {
                    case "1":
                        return "sv-SE";
                    case "2":
                        return "en-US";
                    case "3":
                        return "es-ES";
                    case "4":
                        return "ar-SA";
                    case "5":
                        return "ur-PK";
                    default:
                        return "sv-SE";
                }
            }
        }
        public static bool DebugCleanCash = ConfigurationManager.AppSettings["DebugCleanCash"] != null && ConfigurationManager.AppSettings["DebugCleanCash"] == "1" ? true : false;

        public static bool ReplacePosIdSpecialChars = ConfigurationManager.AppSettings["ReplacePosIdSpecialChars"] != null && ConfigurationManager.AppSettings["ReplacePosIdSpecialChars"] == "1" ? true : false;
        public static IFormatProvider UICultureInfoWithoutCurrencySymbol
        {
            get
            {
                var setUICulture = (CultureInfo)UICultureInfo;
                var uiCultureWithoutCurrencySymbol = (CultureInfo)setUICulture.Clone();
                uiCultureWithoutCurrencySymbol.NumberFormat.CurrencySymbol = "";
                return uiCultureWithoutCurrencySymbol;
            }
        }
    }
}
