using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class ContentVariant : Result
    {
        public string ContentVariantId { get; set; }
        public string ContentId { get; set; }
        public string ContentVariantTemplateId { get; set; }
        public decimal Price { get; set; }
        public string ContentVariantTemplateName { get; set; }
        public decimal DeliveryPrice { get; set; }
    }

    [Serializable]
    public class SimpleContentVariant
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }


}   