using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class Info : Result
    {
        public string Name { get; set; }
        public string OrderPrinterId { get; set; }
        public string ContentCategoryId { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string Presentation { get; set; }
        public string About { get; set; }
        public string PdfUrl { get; set; }
        public Guid CompanyGuid { get; set; }
        public Guid CorporateGuid { get; set; }

    }


}