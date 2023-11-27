using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class AllSettingsViewModel
    {
        public string AccountNumber { get; set; }
        public string PaymentReceiverName { get; set; }
        public string InvoiceReference { get; set; }
        public string Email { get; set; }
        public string MobileLogoUrl { get; set; }
        public HttpPostedFileBase File { get; set; }
    }
}