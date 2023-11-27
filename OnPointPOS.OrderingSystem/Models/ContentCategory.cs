using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class ContentCategory : Result
    {
        public string ContentCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Identifier { get; set; }
        public int SortOrder { get; set; }
        public int OrderPrinterAvailabilityTypeID { get; set; }
        public bool Orderable { get; set; }

        public List<ContentCategoryCustom> ContentCategoryCustom { get; set; }
    }

    [Serializable]
    public class ContentCategoryCustom
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    [Serializable]
    public class SimpleContentCategory : Result
    {
        public string ContentCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }




}
