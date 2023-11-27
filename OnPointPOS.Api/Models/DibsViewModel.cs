using POSSUM.MasterData;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace POSSUM.Api.Models
{
    public class DibsViewModel
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string OrderIntId { get; set; }
        public string OrderGuid { get; set; }
        public string UserId { get; set; }
        public string CustomerID { get; set; }
        public string Mac { get; set; }
        public string Merchant { get; set; }
        public string AcceptReturnUrl { get; set; }
        public string Test { get; set; }
        public string CancelReturnUrl { get; set; }
        public string CallbackUrl { get; set; }
        public string Language { get; set; }
        public string LanguageShort { get; set; }
        public string AcceptUrl { get; set; }
        public string MD5key { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Receipt { get; set; }
        public string Cancel { get; set; }
        public string Action { get; set; }
        public string CompanyImagePath { get; set; }
        public string Display { get; set; }
        public string CompanyName { get; set; }




    }



    public class ReceiptViewModel
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }


    public static class DibsOrderInformation
    {
        public static decimal GetTotal(List<ReceiptViewModel> lstReceipt, decimal deliveryFee, decimal customerPaymentFee, decimal serviceGift = 0)
        {
            return deliveryFee + serviceGift;
        }
        public static List<ReceiptViewModel> PrepareReceipt()
        {
            return new List<ReceiptViewModel>() { new ReceiptViewModel() { Name = "POSSUM", Amount = 100 } };
        }

        public static string GetOrderReceipt(Order order, List<ReceiptViewModel> lstReceipt)
        {
            StringBuilder sb = new StringBuilder();
            var vat = order.VatAmount;
            var netAmount = order.NetAmount;
            var grossAmount = order.GrossAmount;
            // Header
            sb.Append("<table class=\"table\" style=\"width:100%; background:#fff; font-size: 14px; font-family:Helvetica,sans-serif; \">");
            sb.Append("<thead style=\"font-weight: normal!important;\">");

            // Order
            sb.Append("<tr style=\"font-weight: lighter!important;\">");
            sb.Append("<th style=\"font-weight: normal;text-align: left;\">");
            sb.Append(string.Format("Ordernummer: <b>{0}</b>", order.OrderIntID.ToString())); 

            sb.Append("</th>");
            sb.Append("<th style=\"font-weight: normal;text-align: right;\">");

            sb.Append(string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd hh:mm")));

            sb.Append("</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");


            // Items
            foreach (var item in order.OrderLines.Where(o => o.Active == 1 && o.ItemType != ItemType.Ingredient))
            {
                var price = Convert.ToDecimal(item.UnitPrice) * Convert.ToInt32(item.Quantity);
                // Main item
                sb.Append("<tr><td style=\"text-align:left;padding-left: 10px;display: block;\">");
                sb.Append(item.Product.Description.ToUpper());
                sb.Append("</td><td style=\"text-align:right;\">");
                sb.Append(FormatAmount(price, "SEK")); // RULE ???
                sb.Append("</td></tr>");
                foreach (var subItem in order.OrderLines.Where(o => o.GroupKey == item.Id && o.Active == 1 && o.ItemType == ItemType.Ingredient))
                {
                    var subItemprice = Convert.ToDecimal(subItem.UnitPrice) * Convert.ToInt32(subItem.Quantity);
                    // Main item
                    sb.Append("<tr><td style=\"text-align:left;padding-left: 20px;display: block;\">");
                    sb.Append(subItem.Product.Description.ToUpper());
                    sb.Append("</td><td style=\"text-align:right;\">");
                    sb.Append(FormatAmount(subItemprice, "SEK")); // RULE ???
                    sb.Append("</td></tr>");
                }
            }



            // Totals
            sb.Append("<tr><td style=\"text-align:left;\">Totalt (" + "SEK" + "):</td><td style=\"text-align:right;font-weight: bold;\">");
            sb.Append(FormatAmount(Convert.ToDecimal(order.OrderTotal), "SEK")); // Total
            sb.Append("</td></tr>");
            sb.Append("</tbody></table>");

            var message = "";
            if (!string.IsNullOrEmpty(message))
            {
                sb.Append("<table class=\"table\" style=\"width:100%; background:#fff; font-size: 14px;\">");
                sb.Append("<thead style=\"font-weight: normal!important;\">");
                sb.Append("<tr><td style=\"text-align:left;margin-left: 10px;\">");
                sb.Append("Meddelande:");
                sb.Append("</td></tr>");
                sb.Append("<tr><td style=\"text-align:left;margin-left: 10px;\">");
                sb.Append(message.Replace("\r\n", "<br/>"));
                sb.Append("</td></tr>");
                sb.Append("</tbody></table>");
            }
            sb.Append("<table class=\"table\" style=\"width:100%; background:#fff; font-size: 14px;\">");
            sb.Append("<thead style=\"font-weight: normal!important;\">");


            sb.Append("</tbody></table>");

            return sb.ToString();
        }

        public static string GetOrderReceiptForEmail(Order order, string strTransactionNo, string cardName)
        {
            StringBuilder sb = new StringBuilder();
            var vat = order.VatAmount;
            var netAmount = order.NetAmount;
            var grossAmount = order.GrossAmount;

            // Header

            sb.Append("<table class=\"table\" style=\"background:#fff; font-size: 14px; font-family:Helvetica,sans-serif; \">");
            sb.Append("<thead style=\"font-weight: normal!important;\">");

            // Items
            foreach (var item in order.OrderLines.Where(o => o.Active == 1 && o.ItemType != ItemType.Ingredient))
            {
                var price = Convert.ToDecimal(item.UnitPrice) * Convert.ToInt32(item.Quantity);
                // Main item
                sb.Append("<tr><td style=\"text-align:left;padding-left: 10px;display: block;\">");
                sb.Append(item.Product.Description.ToUpper());
                sb.Append("</td><td style=\"text-align:right;\">");
                sb.Append(FormatAmount(price, "SEK")); // RULE ???
                sb.Append("</td></tr>");
                foreach (var subItem in order.OrderLines.Where(o => o.GroupKey == item.Id && o.Active == 1 && o.ItemType == ItemType.Ingredient))
                {
                    var subItemprice = Convert.ToDecimal(subItem.UnitPrice) * Convert.ToInt32(subItem.Quantity);
                    // Main item
                    sb.Append("<tr><td style=\"text-align:left;padding-left: 20px;display: block;\">");
                    sb.Append(subItem.Product.Description.ToUpper());
                    sb.Append("</td><td style=\"text-align:right;\">");
                    sb.Append(FormatAmount(subItemprice, "SEK")); // RULE ???
                    sb.Append("</td></tr>");
                }
            }



            sb.Append("</tbody></table>");


            var totalamount = FormatAmount(Convert.ToDecimal(order.NetAmount), "SEK");

            var template = GetTemplate(vat, netAmount, grossAmount);

            var res = template.Replace("{OrderDetails}", sb.ToString()).Replace("{TotalAmount}", totalamount).Replace("{phoneNo}", order.Customer.Phone).Replace("{dateTime}", DateTime.Now.ToString("yyyy-MM-dd hh:mm")).Replace("{CardName}", cardName).Replace("{Email}", order.Customer.Email).Replace("{phoneNo}", order.Customer.Phone);

            return res.ToString();
        }


        public static string GetOrderReceiptPDF(Order order, List<ReceiptViewModel> lstReceipt)
        {
            // TODO Combine items

            StringBuilder sb = new StringBuilder();
            var vat = Math.Round(order.VatAmount, 2);
            var netAmount = Math.Round(order.NetAmount, 2);
            var grossAmount = Math.Round(order.GrossAmount, 2);

            var serviceGift = Convert.ToInt32(10);
            //if (new OrderService().IsSpecialCustomer(order.CustomerGuid.ToString()))
            //{
            //    serviceGift = 0;
            //}

            var header = string.Format("{0} - Betalning", "POSSUM");
            var imgBaseUrl = ConfigurationManager.AppSettings["PayBaseUrl"];
            //sb.Append("<br ><img src ='" + imgBaseUrl + "/img/IR_logo_cyk.png' style='width:100%' alt ='EWO Easy Way Out AB'> ");                
            sb.Append("<br><br><br><br><br>");
            sb.Append("<h3>" + header + "</h3>");
            sb.Append("<hr>");
            // Header
            sb.Append("<table class=\"table\" style=\"width:100%; background:#fff; font-size: 23px; font-family:Helvetica,sans-serif; \">");
            sb.Append("<thead style=\"font-weight: normal!important;\">");

            // Order
            sb.Append("<tr style=\"font-weight: lighter!important;\">");
            sb.Append("<th style=\"font-weight: normal;text-align: left;\">");
            sb.Append(string.Format("Ordernummer: <b>{0}</b>", order.OrderIntID.ToString()));
            sb.Append("</th>");
            sb.Append("<th style=\"font-weight: normal;text-align: right;\">");
            sb.Append(string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd hh:mm")));
            sb.Append("</th>");
            sb.Append("</tr>");

            sb.Append("</thead>");

            // Phoneno
            //if (!string.IsNullOrEmpty(order.Customer.Phone))
            //{
            //    sb.Append("<tr><td style=\"text-align:left;\" colspan=\"2\">Tele: ");
            //    sb.Append(order.Customer.Phone);
            //    sb.Append("</td></tr>");
            //}
            // Items
            foreach (var item in order.OrderLines.Where(o => o.Active == 1 && o.ItemType != ItemType.Ingredient))
            {
                var price = Convert.ToDecimal(item.UnitPrice) * Convert.ToInt32(item.Quantity);
                // Main item
                sb.Append("<tr><td style=\"text-align:left;padding-left: 10px;display: block;\">");
                sb.Append(item.Product.Description.ToUpper());
                sb.Append("</td><td style=\"text-align:right;\">");
                sb.Append(FormatAmount(price, "SEK")); // RULE ???
                sb.Append("</td></tr>");
                foreach (var subItem in order.OrderLines.Where(o => o.GroupKey == item.Id && o.Active == 1 && o.ItemType == ItemType.Ingredient))
                {
                    var subItemprice = Convert.ToDecimal(subItem.UnitPrice) * Convert.ToInt32(subItem.Quantity);
                    // Main item
                    sb.Append("<tr><td style=\"text-align:left;padding-left: 20px;display: block;\">");
                    sb.Append(subItem.Product.Description.ToUpper());
                    sb.Append("</td><td style=\"text-align:right;\">");
                    sb.Append(FormatAmount(subItemprice, "SEK")); // RULE ???
                    sb.Append("</td></tr>");
                }
            }

            //vat net amount and gross amount
            sb.Append("<br/><br/><br/><br/>");
            sb.Append("<tr><td style=\"text-align:left;display: block;\">Net");
            sb.Append("</td><td style=\"text-align:right;\">");
            sb.Append(netAmount);
            sb.Append("</td></tr>");

            sb.Append("<br/><br/>");
            sb.Append("<tr><td style=\"text-align:left;display: block;\">Vat");
            sb.Append("</td><td style=\"text-align:right;\">");
            sb.Append(vat);
            sb.Append("</td></tr>");

            sb.Append("<br/><br/>");
            sb.Append("<tr><td style=\"text-align:left;display: block;\">Total");
            sb.Append("</td><td style=\"text-align:right;\">");
            sb.Append(Math.Round(order.OrderTotal, 2));
            sb.Append("<br/><br/>");
            sb.Append("</td></tr>");


            // Totals
            sb.Append("<tr><td style=\"text-align:left;\">Totalt (" + "SEK" + "):</td><td style=\"text-align:right;font-weight: bold;\">");
            sb.Append(FormatAmount(Convert.ToDecimal(Math.Round(order.OrderTotal, 2)), "SEK")); // Total
            sb.Append("</td></tr>");
            sb.Append("</tbody></table>");
            sb.Append("<br><br><br>");
            var message = "My Message";
            // Append Message
            if (!string.IsNullOrEmpty(message))
            {

                sb.Append("<table class=\"table\" style=\"width:100%; background:#fff; font-size: 23px;\">");
                sb.Append("<thead style=\"font-weight: normal!important;\">");
                sb.Append("<tr><td style=\"text-align:left;margin-left: 10px;\">");
                sb.Append("Meddelande:");
                sb.Append("<hr>");
                sb.Append("</td></tr>");
                sb.Append("<tr><td style=\"text-align:left;margin-left: 10px;\">");
                sb.Append(message.Replace("\r\n", "<br/>"));
                sb.Append("</td></tr>");
                sb.Append("</tbody></table>");
            }

            // Adress
            sb.Append("<table class=\"table\" style=\"width:100%; background:#fff; font-size: 23px;\">");
            sb.Append("<thead style=\"font-weight: normal!important;\">");


            sb.Append("</tbody></table>");

            return sb.ToString();
        }


        public static string GetTemplate(decimal vat, decimal netAmount, decimal grossAmount)
        {

            var template = @"<html><body><table><tr><td>" +
                          "<div style=\"font-family:Arial;font-size:13px\"> " +
                            "<b style=\"font -size:25px\">Betalningsbekräftelse från POSSUM</b><br /><br /><br /> " +
                             "Tack för din beställning gjord den {dateTime}<br /><br /> " +
                             "Betalningsbekräftelsen fungerar som kvitto för din beställning.<br /><br /><br /> " +
                             " <b style=\"font -size:18px\">Din beställning:</b><br /><br /><br /> " +

                              "{OrderDetails}" +
                              "<br /><br />" +
                              "<b>Net: </b>" + netAmount + "<br /><br /> " +
                              "<b>Vat: </b>" + vat + ":-<br /><br /> " +
                              "<b>Total: </b>" + grossAmount + "<br /><br /> " +
                              "<b>Summa betalt: </b>{TotalAmount}:-<br /><br /><br /> " +
                             " <b>Betalning: </b>{CardName}<br /><br /> " +
                              "<b>Tel: </b>{phoneNo}<br /><br /> " +
                             " <b>E-post: </b><a href=\"mailto:{Email}\">{Email}</a><br /><br /> " +
                             " Om du har frågor eller funderingar är du välkommen att kontakta oss på." +
                          " </div></td></tr></table></body></html>";
            return template;

        }

        public static string FormatAmount(decimal amount, string currencyCode)
        {
            if (currencyCode == "USD")
                return amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US")); ;
            if (currencyCode == "EUR")
                return amount.ToString("C", CultureInfo.CreateSpecificCulture("fr-FR"));
            if (currencyCode == "SEK")
                return amount.ToString("C", CultureInfo.CreateSpecificCulture("sv-SE"));
            return string.Format("{0:C}", amount);
        }


        public class UsersPutResponse
        {
            public bool success { get; set; }
        }

        public class UsersLoginResponse
        {
            public string user_id { get; set; }
            public string access_token { get; set; }
            public string Message { get; set; }
            public string PinCode { get; set; }
            public object Token { get; set; }
            public Customer Customer { get; set; }
        }




    }

}