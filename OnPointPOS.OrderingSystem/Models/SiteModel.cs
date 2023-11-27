using System.Configuration;
using LUE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUETemplateSite.Models
{
    public class SiteModel
    {
        private String customer;
        private String menuid;

        private Newtonsoft.Json.Linq.JArray raw;
        private String deliveryDateTime;
        private int dailyOrderNo;
        private decimal totalAmount;
        private String orderPrinter;
        private String receipt;
        private int orderStatus;

        private string _bodyStyle;
        private string _standAloneStyle;
        private bool _standAlone = true;
        private int _shopTemplate = 1;
        
        private SimpleContent content;
        private List<CompanyInfo> companyinfo;

        private List<CompanyInfo> CompanyInfo
        {
            get
            {
                if (companyinfo == null)
                {
                    companyinfo = LUEApi.GetCompanyInfo(ShopConfig.Secret, ShopConfig.CompanyID).Select(s => JsonConvert.DeserializeObject<CompanyInfo>(s.ToString())).ToList();
                }
                return companyinfo;
            }
        }

        public CompanyInfo GetCompanyInfo
        {
            get
            {
                CompanyInfo companyInfo = CompanyInfo.FirstOrDefault(f => f.OrderPrinterId.Equals(SelectedPrinter.OrderPrinterId));
                if (companyInfo == null)
                {
                    companyInfo = new LUE.CompanyInfo();
                }
                return companyInfo;
            }
        }

        public ShopConfigOrderPrinter SelectedPrinter
        {
            get
            {
                if (HttpContext.Current.Session["OrderPrinter"] == null)
                {
                    if(ShopConfig.OrderPrinters != null)
                    {
                        HttpContext.Current.Session["OrderPrinter"] = ShopConfig.OrderPrinters.First();
                    }
                    else
                    {
                        ShopConfigOrderPrinter shopConfigOrderPrinter = new ShopConfigOrderPrinter();
                        shopConfigOrderPrinter.Name = string.Empty;
                        shopConfigOrderPrinter.OrderPrinterId = Guid.Empty.ToString();
                        HttpContext.Current.Session["OrderPrinter"] = shopConfigOrderPrinter;
                    }
                }
                return (ShopConfigOrderPrinter)HttpContext.Current.Session["OrderPrinter"];
            }
            set
            {
                HttpContext.Current.Session["OrderPrinter"] = value;
            }
        }

        public CompanyOrderPrinter CompanyOrderPrinter
        {
            get
            {
                return new CompanyOrderPrinter(LUEApi.CompanyOrderPrinterRaw(ShopConfig.Secret, CompanyGuid, OrderPrinterId).First);
            }
        }

        public String CompanyGuid
        {
            get
            {
                return HttpContext.Current.Session["CompanyGuid"].ToString();
            }
            set
            {
                HttpContext.Current.Session["CompanyGuid"] = value;
            }
        }

        public Boolean HasOrderPrinter
        {
            get
            {
                if (HttpContext.Current.Session["OrderPrinterId"] == null)
                {
                    return false;
                }
                return !HttpContext.Current.Session["OrderPrinterId"].ToString().Equals(Guid.Empty.ToString());
            }
        }

        public String OrderPrinterId
        {
            get
            {
                return SelectedPrinter.OrderPrinterId;
            }

        }

        public bool TakeawaySelected
        {
            get
            {
                if (HttpContext.Current.Session["TakeawaySelected"] == null)
                {
                    HttpContext.Current.Session["TakeawaySelected"] = false;
                }
                return (bool)HttpContext.Current.Session["TakeawaySelected"];
            }
            set
            {
                HttpContext.Current.Session["TakeawaySelected"] = value;
            }
        }

        public bool HasCart
        {
            get
            {
                if (TakeawaySelected)
                {
                    return true;
                }
                if (!ShopConfig.Delivery && !ShopConfig.DeliveryPayment)
                {
                    return true;
                }
                var cart = LUEApi.GetCustomerCart(CustomerGuid, CompanyGuid, ShopConfig.Secret);
                try
                {
                    return cart.First.SelectToken("Status").ToString().Equals("0");
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool HasCartInternal
        {
            get
            {
                var cart = LUEApi.GetCustomerCart(CustomerGuid, CompanyGuid, ShopConfig.Secret);
                try
                {
                    return cart.First.SelectToken("Status").ToString().Equals("0");
                }
                catch
                {
                    return false;
                }
            }
        }

        public CustomerCart Cart
        {
            get
            {
                return new CustomerCart(LUEApi.GetCustomerCart(CustomerGuid, CompanyGuid, ShopConfig.Secret).First);
            }
        }

        public String PriceType
        {
            get
            {
                return HttpContext.Current.Session["PriceType"].ToString();
            }
            set
            {
                HttpContext.Current.Session["PriceType"] = value;
            }
        }

        public SimpleContent MenuContent
        {
            set
            {
                content = value;
            }
            get
            {
                return content;
            }
        }

        public List<SelectListItem> DeliveryDates
        {
            get
            {
                return LUEApi.GetCustomerOrderPrinterAvailabilityForDelivery(ShopConfig.Secret, CompanyGuid, Menuid)
                                .Select(s => new SelectListItem { Text = s.SelectToken("Key").ToString(), Value = s.SelectToken("Value").ToString() }).ToList();
            }
        }
        public ShopConfig ShopConfig
        {
            get
            {
                return (ShopConfig)HttpContext.Current.Session["ShopConfig"];
            }
            set
            {
                HttpContext.Current.Session["ShopConfig"] = value;
            }
        }

        public String ShopType
        {
            get
            {
                return HttpContext.Current.Session["ShopType"].ToString();
            }
            set
            {
                HttpContext.Current.Session["ShopType"] = value;
            }
        }

        public String ZipCode
        {
            get
            {
                return HttpContext.Current.Session["ZipCode"].ToString();
            }
            set
            {
                HttpContext.Current.Session["ZipCode"] = value;
            }
        }

        public String CustomerGuid
        {
            get
            {
                if (customer == null)
                {
                    if (HttpContext.Current.Session["CustomerGuid"] != null)
                    {
                        customer = HttpContext.Current.Session["CustomerGuid"].ToString();
                    }
                    else
                    {
                        HttpContext.Current.Session["CustomerGuid"] = Guid.NewGuid();
                        customer = HttpContext.Current.Session["CustomerGuid"].ToString();

                    }
                }
                return customer;
            }
        }

        public String OrderFlow
        {
            get
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["OrderFlow"]))
                    return ConfigurationManager.AppSettings["OrderFlow"].ToString();
                return string.Empty;
            }
        }

        public int OrderStatus
        {
            get
            {
                return orderStatus;
            }
            set
            {
                orderStatus = value;
            }
        }

        public CustomerOrderPrinterStatus CustomerOrderPrinterStatus
        {
            get
            {
                if (!HasCartInternal)
                {
                    return new CustomerOrderPrinterStatus(ShopConfig.Secret, CompanyGuid, Menuid, "");
                }
                else
                {
                    return new CustomerOrderPrinterStatus(ShopConfig.Secret, CompanyGuid, Menuid, CustomerGuid);
                }
            }
        }

        public String Menuid
        {
            get
            {
                if (menuid == null)
                {
                    if (HttpContext.Current.Session["MenuId"] != null)
                    {
                        menuid = HttpContext.Current.Session["MenuId"].ToString();
                    }
                }
                return menuid;
            }
            set
            {
                menuid = value;
                HttpContext.Current.Session["MenuId"] = menuid;
            }
        }

        public Newtonsoft.Json.Linq.JArray CustomerOrderRaw
        {
            get
            {
                return raw;
            }
            set
            {
                raw = value;
            }
        }

        public String DeliveryDateTime
        {
            get
            {
                return deliveryDateTime;
            }
            set
            {
                deliveryDateTime = value;
            }
        }
        public int DailyOrderNo
        {
            get
            {
                return dailyOrderNo;
            }
            set
            {
                dailyOrderNo = value;
            }
        }
        public decimal TotalAmount
        {
            get
            {
                return totalAmount;
            }
            set
            {
                totalAmount = value;
            }
        }
        public String OrderPrinter
        {
            get
            {
                return orderPrinter;
            }
            set
            {
                orderPrinter = value;
            }
        }
        public String Receipt
        {
            get
            {
                return receipt;
            }
            set
            {
                receipt = value;
            }
        }

        public String BodyStyle
        {
            get
            {
                if (string.IsNullOrEmpty(ShopConfig.CompanyBackgroundImagePath))
                    BodyStyle = ShopConfig.StandAlone ? "background: url(../img/pizza-big-bg.jpg) top center repeat-y fixed;" : string.Empty;
                else
                    BodyStyle = ShopConfig.StandAlone ? "background: url(" + ShopConfig.CompanyBackgroundImagePath + ") top center repeat-y fixed;" : string.Empty;
                return _bodyStyle;
            }
            set
            {
                _bodyStyle = value;
            }
        }

        public string StandAloneStyle
        {
            get
            {
                return _standAloneStyle;
            }
            set
            {
                _standAloneStyle = value;
            }
        }

        public bool StandAlone
        {
            get
            {
                return _standAlone;
            }
            set
            {
                _standAlone = value;
            }
        }

        public int ShopTemplate
        {
            get
            {
                return _shopTemplate;
            }
            set
            {
                _shopTemplate = value;
            }
        }
        

        public String OrderGuid
        {
            get
            {
                return HttpContext.Current.Session["OrderGuid"].ToString();
            }
            set
            {
                HttpContext.Current.Session["OrderGuid"] = value;
            }
        }

        public Dictionary<Int32, String> VerificationMessages
        {
            get
            {
                if (HttpContext.Current.Session["VerificationMessages"] == null)
                {
                    HttpContext.Current.Session["VerificationMessages"] = new Dictionary<Int32, String>();
                }
                return (Dictionary<Int32, String>)HttpContext.Current.Session["VerificationMessages"];
            }
            set
            {
                HttpContext.Current.Session["VerificationMessages"] = value;
            }
        }

        public string TruncateString(String s, int maxlength = 40, string trunc = "...")
        {
            if (maxlength == -1)
            {
                return s;
            }
            if (s.Length + trunc.Length > maxlength)
            {
                return s.Substring(0, maxlength - trunc.Length) + trunc;
            }
            return s;
        }

        public String ReturnOnlyNumerals(String str)
        {
            //string test = new String(str.Where(Char.IsDigit).ToArray());
            return str == null ? string.Empty : new String(str.Where(Char.IsDigit).ToArray());
        }

        public String ColLayoutBasedOnNumSocialNetworks()
        {
            bool tw = GetCompanyInfo.TwitterUrl == "" ? false : true;
            bool fb = GetCompanyInfo.FacebookUrl == "" ? false : true;
            bool insta = GetCompanyInfo.InstagramUrl == "" ? false : true;

            if (tw && fb && insta)
            {
                return "col-xs-4 col-md-4 col-lg-4";
            }
            else if (tw && fb)
            {
                return "col-xs-6 col-md-6 col-lg-6";
            }
            else if (fb && insta)
            {
                return "col-xs-6 col-md-6 col-lg-6";
            }
            else if (insta && tw)
            {
                return "col-xs-6 col-md-6 col-lg-6";
            }
            else if (tw || fb || insta)
            {
                return "col-xs-12 col-md-12 col-lg-12";
            }
            else
            {
                return String.Empty;
            }
        }
    }
}