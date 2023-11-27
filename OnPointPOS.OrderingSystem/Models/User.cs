using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class User : Result
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string CompanyName { get; set; }
        //public Guid CompanyGuid { get; set; }
        //public string Secret { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public int[] ApplicationIDs { get; set; }
        public Guid DefaultContentTemplateId { get; set; }
        public Guid DefaultContentCategoryId { get; set; }
        public string Token { get; set; }
        public string CompanyImagePath { get; set; }
        public ShopConfig ShopConfig { get; set; }
        public int RoleID { get; set; }
        public string Secret { get; set; }
    }

    [Serializable]
    public class ShopConfig
    {
        public bool SubContentGroup { get; set; }
        public bool ContentVariant { get; set; }
        public bool FreeSubContent { get; set; }


        public bool TakeAway { get; set; }
        public bool TakeAwayPayment { get; set; }
        public bool Delivery { get; set; }
        public bool DeliveryPayment { get; set; }
        public bool OrderPrinter { get; set; }
        public int ShopRegistrationType { get; set; }
        public int ShopTemplate { get; set; }
        public string ContentTemplateId { get; set; }
        public string ContentCategoryId { get; set; }
        public bool MultipleMenus { get; set; }
        public bool UseOpenNotRedeemable { get; set; }
        public List<Company> Companies { get; set; }
        public bool PunchTicket { get; set; }
        public string AppStoreUrl { get; set; }
        public string GooglePlayUrl { get; set; }
        public bool Cms { get; set; }
    }

    public class Company
    {
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public bool Selected { get; set; }
    }





}