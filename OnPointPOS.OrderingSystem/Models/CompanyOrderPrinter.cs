using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class CompanyOrderPrinter : Result
    {
        public string OrderPrinterId { get; set; }
        public string Name { get; set; }
        public string ContentTemplateId { get; set; }
        public string ContentCategoryId { get; set; }
    }



}