using LUE;
using LUETemplateSite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.OrderingSystem.Controllers
{
    public class TemplateSiteController : Controller
    {
        // GET: /Home/
        public ActionResult Index(string id)
        {
            var mod = new SiteModel();
            mod.ShopType = "1";
            mod.PriceType = "1";
            var url = Request.Url.GetLeftPart(UriPartial.Authority);
            if (url.StartsWith("http://"))
            {
                url = url.Substring("http://".Length);
            }
#if (DEBUG)
            //LUE:
            //url = "jattegott.letuseat.se";
            //url = "masalakitchen.letuseat.se";
            //url = "cyrano.ewo.se";
            //url = "shop.sannegarden.com";
            //url = "ilp.letuseat.se";
            //url = "pizzeriapomerans.letuseat.se";
            //url = "hosphilippe.letuseat.se";
            //url = "ifk.ewo.se";
            //url = "ilp.lue.dev02.luqon.com";
            //url = "picoeat.letuseat.se";

            //url = "template3.lue.dev02.luqon.com";

            url = "template.letuseat.se";

            //url = "lammetochgrisen.letuseat.se";

            //MyClub:
            //url = "ifensektion.ewo.se";

            //menuid: 162664F9-E8F3-433F-A72F-382A56864005 (ContentCategoryGuid: Cyrano Vasa)
#endif
            var res = LUEApi.PublicUrl(url);
            var res2 = res.First;
            if (res2 != null && (Int32)res2.SelectToken("Status") == 0)
            {
                ViewBag.URL = url;
                mod.CompanyGuid = res2.SelectToken("CompanyId").ToString();
                mod.ShopConfig = new ShopConfig(LUEApi.ShopConfig(mod.CompanyGuid, mod.ShopType).First());
                if (mod.ShopConfig.MultipleMenus)
                {
                    //mod.Menuid = mod.ShopConfig.ContentTemplateId;
                    if (id == null && mod.Menuid == null)
                    {
                        return Redirect("/Home/CheckZipCode");
                    }
                    else if (id != null)
                    {
                        if (id == "delivery")
                        {
                            return Redirect("/Home/CheckZipCode");
                        }
                        if (mod.Menuid != id)
                        {
                            LUEApi.DeleteFromCustomerCart(mod.CustomerGuid, mod.CompanyGuid, mod.ShopConfig.Secret);
                        }
                        mod.Menuid = id;
                        mod.TakeawaySelected = true;
                    }
                }
                else
                {
                    mod.Menuid = mod.ShopConfig.ContentCategoryId;
                }
                //var al = mod.CompanyOrderPrinter;
                mod.MenuContent = new SimpleContent(mod.ShopConfig.Secret, mod.CompanyGuid, mod.Menuid + "/1", "540");





                mod.StandAlone = mod.ShopConfig.StandAlone;
                //Shahid > donot set important
                if (string.IsNullOrEmpty(mod.ShopConfig.CompanyBackgroundImagePath))
                    mod.BodyStyle = mod.ShopConfig.StandAlone ? "background: url(../img/pizza-big-bg.jpg) top center repeat-y fixed;" : string.Empty;
                else
                    mod.BodyStyle = mod.ShopConfig.StandAlone ? "background: url(" + mod.ShopConfig.CompanyBackgroundImagePath + ") top center repeat-y fixed;" : string.Empty;

                mod.StandAloneStyle = mod.ShopConfig.StandAlone ? string.Empty : "display:none;";
                ViewBag.CSS = mod.ShopConfig.CSS;
                mod.ShopTemplate = mod.ShopConfig.ShopTemplate;


                return View(mod);
            }
            return Redirect("/home/notfound");
        }

        public ActionResult CheckZipCode()
        {
            var mod = new SiteModel();
            return View(mod);
        }

        public ActionResult GetModalData()
        {
            var id = Request.Form.Get("itemid");
            var pricetype = Request.Form.Get("pricetype");
            var takeaway = Request.Form.Get("takeaway");
            if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrWhiteSpace(pricetype))
            {
                var savetake = takeaway != null;
                var mod = new SiteModel();
                if (savetake)
                {
                    mod.TakeawaySelected = savetake;
                }
                mod.PriceType = pricetype;
                var content = new CompanyContent(LUEApi.GetCompanyContent(mod.CompanyGuid, mod.ShopConfig.Secret, id, "400", pricetype).First);
                ViewBag.ID = id;
                ViewBag.Name = content.Name;
                ViewBag.Description = content.Description;
                ViewBag.Data = String.Concat(content.Variants, content.Images, content.SubContents);
                return PartialView("_modaldata", new SiteModel());
            }
            return Content("");
        }

        public ActionResult GetZipModal(string id)
        {
            ViewBag.ID = id;
            return PartialView("_zipmodal", new SiteModel());
        }

        public ActionResult OpenOrderModal()
        {
            var mod = new SiteModel();
            ViewBag.Cart = new CustomerCart(LUEApi.GetCustomerCart(mod.CustomerGuid, mod.CompanyGuid, mod.ShopConfig.Secret).First);
            return PartialView("_ordermodal", mod);
        }

        public ActionResult ReloadCart()
        {
            var mod = new SiteModel();
            if (mod.HasCartInternal)
            {
                ViewBag.Cart = mod.Cart;
                return PartialView("_cartdata", mod);
            }
            return PartialView("_cartempty");
        }

        public ActionResult HasCart()
        {
            var mod = new SiteModel();
            return Json(new { HasCart = mod.HasCart, HasCartInternal = mod.HasCartInternal }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchAdressByPostalGroup(string id, string text)
        {
            if (id == null)
            {
                id = text;
            }
            if (!String.IsNullOrWhiteSpace(id) && id.Length >= 3)
            {
                var mod = new SiteModel();
                HttpContent content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("Search", id),
                });
                var res = LUEApi.PostCompanyGroupPostal(mod.ShopConfig.Secret, mod.CompanyGuid, "10", content);
                var search = res.Select(r => new { Text = r.SelectToken("Street") + ", " + r.SelectToken("ZipCode") + ", " + r.SelectToken("City"), Value = r.SelectToken("PostalGroupGuid").ToString() });
                return Json(search, JsonRequestBehavior.AllowGet);
            }
            return Content("");
        }
        public ActionResult SearchAdress(string id, string text)
        {
            if (id == null)
            {
                id = text;
            }
            if (!String.IsNullOrWhiteSpace(id) && id.Length >= 3)
            {
                var mod = new SiteModel();
                HttpContent content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("Search", id),
                });
                var res = LUEApi.PostCompanyPostal(mod.ShopConfig.Secret, mod.CompanyGuid, "10", content);
                var search = res.Select(r => new { Text = r.SelectToken("Street") + " " + r.SelectToken("StreetNumber") + r.SelectToken("StreetNumberLetter") + ", " + r.SelectToken("ZipCode") + ", " + r.SelectToken("City"), Value = r.SelectToken("PostalId").ToString() });
                return Json(search, JsonRequestBehavior.AllowGet);
            }
            return Content("");
        }

        [HttpGet]
        public ActionResult GetDeliveryFeeByPostalGroup(string postalGroupGuid)
        {
            try
            {
                var mod = new SiteModel();
                var result = LUEApi.GetDeliveryFeeByPostalGroup(mod.ShopConfig.Secret, mod.OrderPrinterId, postalGroupGuid);
                var data = result.Select(r => new { DeliveryFee = r.SelectToken("DeliveryFee").ToString(), DeliveryMinimumAmount = r.SelectToken("DeliveryMinimumAmount").ToString() });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCompanyOrderPrinter(string postalid)
        {
            try
            {
                var mod = new SiteModel();
                // var ver = LUEApi.GetCompanyOrderPrinter(mod.CompanyGuid, mod.ShopConfig.Secret, postalid);               
                //var printerId = ver.First().SelectToken("ContentCategoryId");
                //var resul = LUEApi.GetZipCodes(mod.ShopConfig.Secret, printerId.ToString());
                //var res = resul.Select(c => new
                //{
                //    ZipCode = c["ZipCode"].ToString(),
                //    DeliveryFee = c["DeliveryFee"].ToString(),
                //    DeliveryMinimumAmount = c["DeliveryMinimumAmount"].ToString()

                //}).First();
                //return Json(res, JsonRequestBehavior.AllowGet);

                //Zahid: new implaementation against PostalGroupGuid
                var result = LUEApi.GetOrderPrinterPostal(mod.ShopConfig.Secret, mod.CompanyGuid, postalid);
                var res = result.Select(c => new
                {
                    PostalGroupGuid = c["PostalGroupGuid"].ToString(),
                    DeliveryFee = c["DeliveryFee"].ToString(),
                    DeliveryMinimumAmount = c["DeliveryMinimumAmount"].ToString()

                }).First();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CheckDiscountCode()
        {
            var code = Request.Form.Get("DiscountCode");
            if (!String.IsNullOrWhiteSpace(code))
            {
                var cleanpostal = code.Where(c => !Char.IsWhiteSpace(c)).Select(c => c.ToString()).Aggregate((a, b) => a + b);
                var mod = new SiteModel();
                JArray res = LUEApi.GetDiscount(mod.ShopConfig.Secret, mod.CompanyGuid, cleanpostal, "v2");
                //JArray res = LUEApi.GetCompanyOrderPrinter(mod.CompanyGuid, mod.ShopConfig.Secret, cleanpostal);
                if (res != null && res.Count > 0)
                {
                    decimal discount = 0;
                    decimal.TryParse(res.First.SelectToken("Discount").ToString(), out discount);
                    //return Content(res.First.ToString(), "application/json");
                    JObject obj = new JObject();
                    if (discount > 0)
                    {
                        obj.Add("Discount", res.First.SelectToken("Discount"));
                        obj.Add("DiscountCodeExists", "true");
                    }
                    else
                    {
                        obj.Add("Discount", "0");
                        obj.Add("DiscountCodeExists", "false");
                    }
                    JArray ret = new JArray(obj);
                    return Content(ret.First.ToString(), "application/json");
                }
                else
                {
                    JObject obj = new JObject();
                    obj.Add("Discount", "0");
                    obj.Add("DiscountCodeExists", "false");
                    JArray ret = new JArray(obj);
                    return Content(ret.First.ToString(), "application/json");
                }
                //    return Json(new { ZipCodeExists = false }, JsonRequestBehavior.AllowGet);
                //}
                //  return Json(new { ZipCodeExists = true }, JsonRequestBehavior.AllowGet);
            }
            return Content("");
        }
        public ActionResult AddToCart()
        {
            var mod = new SiteModel();
            String itemid = "";
            String variant = "";
            var included = new List<String>();
            var excluded = new List<String>();
            var special = new List<String>();
            var owing = new List<String>();
            foreach (string key in Request.Form.Keys)
            {
                var value = Request.Form.Get(key);
                var split = key.Split('_');
                switch (split[0])
                {
                    case "itemid":
                        itemid = value;
                        break;
                    case "variant":
                        variant = value;
                        break;
                    case "included":
                        included.Add(value);
                        break;
                    case "excluded":
                        excluded.Add(value);
                        break;
                    case "special":
                        special.Add(value);
                        break;
                    default:
                        break;
                }
            }
            if (!String.IsNullOrWhiteSpace(itemid))
            {
                var contentvalues = new List<KeyValuePair<string, string>>();
                contentvalues.Add(new KeyValuePair<string, string>("ContentId", itemid));
                contentvalues.Add(new KeyValuePair<string, string>("PriceType", mod.PriceType));
                if (HttpContext.Session["PostalId"] != null)
                {
                    contentvalues.Add(new KeyValuePair<string, string>("PostalId", HttpContext.Session["PostalId"].ToString()));
                }
                if (!String.IsNullOrWhiteSpace(variant))
                {
                    contentvalues.Add(new KeyValuePair<string, string>("ContentVariantId", variant));
                }
                if (included.Count > 0)
                {
                    contentvalues.Add(new KeyValuePair<string, string>("IncludedSubContentIds", String.Join(",", included)));
                }
                if (excluded.Count > 0)
                {
                    contentvalues.Add(new KeyValuePair<string, string>("SubContentIds", String.Join(",", excluded)));
                }
                if (special.Count > 0)
                {
                    contentvalues.Add(new KeyValuePair<string, string>("ContentSpecialIds", String.Join(",", special)));
                }
                HttpContent content = new FormUrlEncodedContent(contentvalues);
                var res = LUEApi.AddToCustomerCart(mod.CustomerGuid, mod.CompanyGuid, mod.ShopConfig.Secret, content);
                var custid = mod.PriceType.Equals("2") ? mod.CustomerGuid + "/" + HttpContext.Session["PostalId"].ToString() : mod.CustomerGuid;
                var cart = new CustomerCart(LUEApi.GetCustomerCart(custid, mod.CompanyGuid, mod.ShopConfig.Secret).First);
                var menupostal = mod.PriceType.Equals("2") ? HttpContext.Session["PostalId"].ToString() : mod.Menuid;
                var cops = new CustomerOrderPrinterStatus(mod.ShopConfig.Secret, mod.CompanyGuid, menupostal, mod.CustomerGuid);
                ViewBag.Cops = cops;
                ViewBag.Cart = cart;
                var mid = mod.PriceType.Equals("2") ? HttpContext.Session["PostalId"].ToString() : mod.Menuid;
                var avail = LUEApi.GetCustomerOrderPrinterAvailabilityForDelivery(mod.ShopConfig.Secret, mod.CompanyGuid, mid);
                var avail2 = avail.Select(s => new SelectListItem { Text = s.SelectToken("Key").ToString(), Value = s.SelectToken("Value").ToString() }).ToList();
                ViewBag.Avail = avail2;
                ViewBag.TakeAwayPayment = mod.ShopConfig.TakeAwayPayment;
                ViewBag.TakeAway = mod.ShopConfig.TakeAway;
                ViewBag.Delivery = mod.ShopConfig.Delivery;
                ViewBag.DeliveryPayment = mod.ShopConfig.DeliveryPayment;
            }
            return PartialView("_cartdata", mod);
        }
        public ActionResult DeleteFromCart()
        {
            var mod = new SiteModel();
            var id = Request.Form.Get("cartid");
            if (!String.IsNullOrWhiteSpace(id))
            {
                ViewBag.Cart= LUEApi.DeleteFromCustomerCart(id, mod.CompanyGuid, mod.ShopConfig.Secret);
                //ViewBag.Cart = new CustomerCart(LUEApi.GetCustomerCart(mod.CustomerGuid, mod.CompanyGuid, mod.ShopConfig.Secret).First);
                if (mod.HasOrderPrinter)
                {
                    var cops = new CustomerOrderPrinterStatus(mod.ShopConfig.Secret, mod.CompanyGuid, mod.Menuid, mod.CustomerGuid);
                    ViewBag.Cops = cops;
                }
                ViewBag.TakeAwayPayment = mod.ShopConfig.TakeAwayPayment;
                ViewBag.TakeAway = mod.ShopConfig.TakeAway;
                ViewBag.Delivery = mod.ShopConfig.Delivery;
                ViewBag.DeliveryPayment = mod.ShopConfig.DeliveryPayment;
            }
            return PartialView("_cartdata", mod);
        }
        public ActionResult MoveToPaymentChoice(string submit)
        {
            return Content("");
        }

        public ActionResult CreateOrder(string submit)
        {
            var deliverytype = Request.Form.Get("deliverytype");
            var doorcode = Request.Form.Get("doorcode");
            var floor = Request.Form.Get("floor");
            var apartmentnumber = Request.Form.Get("apartmentnumber");
            var deliverymessage = Request.Form.Get("other");
            String namekey = "", surnamekey = "", phonekey = "", opi = "";
            var mod = new SiteModel();
            var contentvalues = new List<KeyValuePair<string, string>>();
            string pay = "";
            switch (submit)
            {
                case "card":
                    pay = "1";
                    break;
                case "cash":
                    pay = "0";
                    break;
                default:
                    return Content("");
            }
            switch (deliverytype)
            {
                case "0":
                    namekey = "firstname2";
                    surnamekey = "surname2";
                    phonekey = "phone2";
                    break;
                case "1":
                    namekey = "firstname";
                    surnamekey = "surname";
                    phonekey = "phone";
                    break;
            }
            contentvalues.Add(new KeyValuePair<string, string>("FirstName", Request.Form.Get(namekey).ToString()));
            contentvalues.Add(new KeyValuePair<string, string>("LastName", Request.Form.Get(surnamekey).ToString()));
            contentvalues.Add(new KeyValuePair<string, string>("PhoneNo", Request.Form.Get(phonekey).ToString()));
            var resc = LUEApi.AddOrUpdateCompanyCustomer(mod.ShopConfig.Secret, mod.CompanyGuid, mod.CustomerGuid, new FormUrlEncodedContent(contentvalues));
            contentvalues = new List<KeyValuePair<string, string>>();
            contentvalues.Add(new KeyValuePair<string, string>("PaymentRequired", pay));
            var comment = Request.Form.Get("comment");
            var deliverydatetime = Request.Form.Get("deliverydate");
            if (deliverytype == "0")
            {
                comment = Request.Form.Get("comment");
                deliverydatetime = Request.Form.Get("deliverydate");
            }
            string adr = Request.Form.Get("deliveryadress");
            string rest = Request.Form.Get("restaurant");
            if (deliverytype.Equals("1") && !String.IsNullOrWhiteSpace(adr))
            {
                opi = adr;
            }
            else
            {
                if (String.IsNullOrEmpty(rest))
                {
                    opi = mod.Menuid;
                }
                else
                {
                    opi = rest;
                }
            }
            if (!String.IsNullOrWhiteSpace(deliverydatetime))
            {
                contentvalues.Add(new KeyValuePair<string, string>("DeliveryDateTime", deliverydatetime));
            }
            if (!String.IsNullOrWhiteSpace(comment))
            {
                contentvalues.Add(new KeyValuePair<string, string>("Text", comment));
            }
            if (!String.IsNullOrWhiteSpace(doorcode))
            {
                contentvalues.Add(new KeyValuePair<string, string>("DoorCode", doorcode));
            }
            if (!String.IsNullOrWhiteSpace(floor))
            {
                contentvalues.Add(new KeyValuePair<string, string>("Floor", floor));
            }
            if (!String.IsNullOrWhiteSpace(apartmentnumber))
            {
                contentvalues.Add(new KeyValuePair<string, string>("ApartmentNumber", apartmentnumber));
            }
            if (!String.IsNullOrWhiteSpace(deliverymessage))
            {
                contentvalues.Add(new KeyValuePair<string, string>("DeliveryMessage", deliverymessage));
            }
            if (opi != null)
            {
                HttpContent content = new FormUrlEncodedContent(contentvalues);
                var ret = LUEApi.PostCustomerOrder(mod.ShopConfig.Secret, mod.CompanyGuid, mod.CustomerGuid, opi, content);
                var status = ret.First.SelectToken("Status").ToString();
                if (status.Equals("0"))
                {
                    var baseurl = Request.Url.GetLeftPart(UriPartial.Authority);
                    var orderid = ret.First.SelectToken("OrderGuid").ToString();
                    var accepturl = "/Home/VerifyOrder/" + orderid;
                    mod.OrderGuid = orderid;
                    string PayUrl = ConfigurationManager.AppSettings["PayUrl"].ToString();
                    var rurl = PayUrl + orderid + "/?cancelUrl=" + baseurl + "/Home/Cancel" + "&acceptUrl=" + baseurl + accepturl;
                    return Json(new { url = rurl }, JsonRequestBehavior.AllowGet);
                }
                return Content("");
            }
            return Content("");
        }

        public ActionResult Cancel()
        {
            HttpContext.Session["Canceld"] = true;
            return View();
        }

        public ActionResult CancelPoll()
        {
            var mod = new SiteModel();
            var ver = new CustomerOrderStatus(LUEApi.GetCustomerOrderStatus(mod.ShopConfig.Secret, mod.CompanyGuid, mod.OrderGuid).First);
            Boolean cancel = HttpContext.Session["Canceld"] == null ? false : (Boolean)HttpContext.Session["Canceld"];
            HttpContext.Session["Canceld"] = null;
            return Json(new { closemodal = cancel, orderStatus = ver.OrderStatus }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PostalCheck()
        {
            var postal = Request.Form.Get("Postal");
            if (!String.IsNullOrWhiteSpace(postal))
            {
                var cleanpostal = postal.Where(c => !Char.IsWhiteSpace(c)).Select(c => c.ToString()).Aggregate((a, b) => a + b);
                var mod = new SiteModel();
                //JArray res = LUEApi.CheckPostal(mod.ShopConfig.Secret, mod.CompanyGuid, cleanpostal);
                JArray res = LUEApi.GetCompanyOrderPrinter(mod.CompanyGuid, mod.ShopConfig.Secret, cleanpostal);
                if (res != null)
                {
                    if (res.First.SelectToken("Status").ToString() == "0")
                    {
                        //if ((bool)res.First.SelectToken("ZipCodeExists"))
                        //{
                        mod.ZipCode = postal;
                        mod.Menuid = res.First.SelectToken("ContentCategoryId").ToString();
                        mod.TakeawaySelected = true;
                        //}
                        //return Content(res.First.ToString(), "application/json");
                        JObject obj = new JObject();
                        obj.Add("ZipCodeExists", "true");
                        JArray ret = new JArray(obj);
                        return Content(ret.First.ToString(), "application/json");
                    }
                    return Json(new { ZipCodeExists = false }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { ZipCodeExists = false }, JsonRequestBehavior.AllowGet);
            }
            return Content("");
        }

        public SiteModel OrderStatus(string id = null)
        {
            var mod = new SiteModel();
            var ver = new CustomerOrderStatus(LUEApi.GetCustomerOrderStatus(mod.ShopConfig.Secret, mod.CompanyGuid, mod.OrderGuid).First);
            if (id != null)
            {
                ver = new CustomerOrderStatus(LUEApi.GetCustomerOrderStatus(mod.ShopConfig.Secret, mod.CompanyGuid, id).First);
            }
            var raw = LUEApi.CustomerOrderRaw(mod.ShopConfig.Secret, mod.CompanyGuid, mod.CustomerGuid, mod.OrderGuid);

            mod.CustomerOrderRaw = raw;
            mod.DeliveryDateTime = (String)raw[0]["DeliveryDateTime"];
            mod.DailyOrderNo = (int)raw[0]["DailyOrderNo"];
            mod.TotalAmount = (decimal)raw[0]["TotalAmount"];
            mod.OrderPrinter = (String)raw[0]["OrderPrinter"];
            mod.Receipt = (String)raw[0]["Receipt"];

            mod.OrderStatus = ver.OrderStatus;

            if (!mod.VerificationMessages.ContainsKey(ver.OrderStatus))
            {
                mod.VerificationMessages.Add(ver.OrderStatus, ver.OrderStatusMessageShort);
            }

            ViewBag.Poll = !(ver.OrderStatus == 6 || ver.OrderStatus == 9 || ver.OrderStatus == 10);

            return mod;
        }

        public ActionResult VerifyOrder(string id)
        {
            var mod = OrderStatus(id);
            return View(mod);
        }

        public ActionResult CustomerOrderStatus()
        {
            var mod = OrderStatus();
            return PartialView("_verify", mod);
        }

        public ActionResult CustomerOrderStatusReset()
        {
            var mod = new SiteModel();
            mod.VerificationMessages = null;

            ViewBag.Poll = false;

            return View("VerifyOrder", mod);
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult Customer(string id)
        {
            var mod = new SiteModel();
            mod.VerificationMessages = null;
            var orders = new Orders().CustomerOrders(LUEApi.GetCustomerOrder(mod.ShopConfig.Secret, mod.CompanyGuid, id));
            ViewBag.Orders = orders;
            return View(mod);
        }

        public String GetCartQuantity()
        {
            var mod = new SiteModel();
            CustomerCart cart = new CustomerCart(LUEApi.GetCustomerCart(mod.CustomerGuid, mod.CompanyGuid, mod.ShopConfig.Secret).First);
            return cart.TotalQuantity;
        }

        public String GetCartTotalAmount()
        {
            var mod = new SiteModel();
            CustomerCart cart = new CustomerCart(LUEApi.GetCustomerCart(mod.CustomerGuid, mod.CompanyGuid, mod.ShopConfig.Secret).First);
            return (cart.TotalAmount - cart.CustomerPaymentFee).ToString();
        }

    }
}