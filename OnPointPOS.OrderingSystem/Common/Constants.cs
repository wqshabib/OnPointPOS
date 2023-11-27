using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ML.Common
{
    public class Constants
    {
        public static int EWO_PARTNER_ID = 994;
        public static string EWO_PARTNER_PASSWORD = "x7r9k2g6"; 

        public static string MOBLINK_XML_CATCHER_URL = "http://admin.moblink.se/integration/XmlCatcher.aspx";

        public static string MOBLINK_OLD_LINK_URL = "http://www.moblink.se/m.aspx?l={0}";
        public static string MOBLINK_LINK_URL = "http://www.moblink.se/{0}";

        public static string MOBLINK_HOST_NAME = "www.moblink.se";

        public static string MOBLINK_INTERNAL_SECRET_KEY = "r!dJs%reiowxRsT8!Stg3789%gdsghiuweu";

        public static Guid MOBLINK_COMPANY_GUID = new Guid("12d0cd8f-aba3-440b-9287-bf11230bd11e");
        public static string MOBLINK_COMPANY_NAME = "moblink";

#if DEBUG
        public static string MOBLINK_STORAGE_PATH = @"c:\temp";
        public static string MOBLINK_IDE_HOST_NAME = "localhost:43924";

#else
        public static string MOBLINK_STORAGE_PATH = @"\\172.20.13.31\e$\Sites\moblink\MOBLINK_STORAGE";
        public static string MOBLINK_IDE_HOST_NAME = "localhost";
#endif

        public static string EWO_INVOICE_URL = "http://invoice.ewo.se";


    }
}
