using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Config
    {
        [Serializable]
        public class ShopConfig : Result
        {
            public string CompanyId { get; set; }
            public string Secret { get; set; }
            public string ContentTemplateId { get; set; }
            public string ContentCategoryId { get; set; }
            public bool MultipleMenus { get; set; }
            public string CompanyName { get; set; }
            public string CompanyImagePath { get; set; }
            public string CompanyBackgroundImagePath { get; set; }

            // Corresponds to tShopConfig
            public bool TakeAway { get; set; }
            public string CorporateGuid { get; set; }
            public bool TakeAwayPayment { get; set; }
            public bool Delivery { get; set; }
            public bool DeliveryPayment { get; set; }
            public bool OrderPrinter { get; set; }
            public int ShopRegistrationType { get; set; }
            public int ShopTemplate { get; set; }
            public int ShopListType { get; set; }
            public bool Standalone { get; set; }
            public string Css { get; set; }
            public bool DeliveryZipCodePreCheck { get; set; }
            public bool OrderCustomerEmail { get; set; }
            public string AppStoreUrl { get; set; }
            public string GooglePlayUrl { get; set; }
            public int ShopType { get; set; }
            public Cms Cms  { get; set; }
            public List<Slide> Slides { get; set; }

            // Processed
            public bool Menu { get; set; }

            public List<OrderPrinter> OrderPrinters { get; set; }
        }

        public class OrderPrinter
        {
            public string OrderPrinterId { get; set; }
            public string Name { get; set; }
        }





    }
}