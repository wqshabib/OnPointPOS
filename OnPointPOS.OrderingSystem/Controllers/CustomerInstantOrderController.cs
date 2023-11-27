using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace ML.Rest2.Controllers
{
    public class CustomerInstantOrderController : ApiController
    {
        public HttpResponseMessage Post(string secret, string companyId, string customerId)
        {
            List<CustomerOrder> mCustomerOrders = new List<CustomerOrder>();
            CustomerOrder mCustomerOrder = new CustomerOrder();

            mCustomerOrders.Add(mCustomerOrder);

            if (!Request.Content.IsFormData())
            {
                mCustomerOrder.Status = RestStatus.NotFormData;
                mCustomerOrder.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId))
            {
                mCustomerOrder.Status = RestStatus.ParameterError;
                mCustomerOrder.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
            }
            Guid guidCompanyGuid = new Guid(companyId);

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                mCustomerOrder.Status = RestStatus.AuthenticationFailed;
                mCustomerOrder.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
            }
            
            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            string strExternalTrackingNo = string.IsNullOrEmpty(dic["TrackingId"]) ? string.Empty : dic["TrackingId"];
            string strExternalMessage = string.IsNullOrEmpty(dic["TrackingMessage"]) ? string.Empty : dic["TrackingMessage"];

            string strFirstName = string.IsNullOrEmpty(dic["FirstName"]) ? string.Empty : dic["FirstName"];
            string strLastName = string.IsNullOrEmpty(dic["LastName"]) ? string.Empty : dic["LastName"];
            string strEmail = string.IsNullOrEmpty(dic["Email"]) ? string.Empty : dic["Email"];
            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strAddress = string.IsNullOrEmpty(dic["Address"]) ? string.Empty : dic["Address"];
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            string strCity = string.IsNullOrEmpty(dic["City"]) ? string.Empty : dic["City"];
            string strCurrency = string.IsNullOrEmpty(dic["Currency"]) ? string.Empty : dic["Currency"];
            // "Channel" / product

            string strOrderText = string.IsNullOrEmpty(dic["OrderText"]) ? string.Empty : dic["OrderText"];

            // Invoice
            
            string strInvoiceTitle = string.IsNullOrEmpty(dic["InvoiceTitle"]) ? string.Empty : dic["InvoiceTitle"];
            string strInvoiceText = string.IsNullOrEmpty(dic["InvoiceText"]) ? string.Empty : dic["InvoiceText"];
            decimal decReminderFee = 0;
            if(!string.IsNullOrEmpty(dic["ReminderFee"]))
                decimal.TryParse(dic["ReminderFee"], out decReminderFee);

            int intMaxRetries = 0;
            if(!string.IsNullOrEmpty(dic["MaxRetries"]))
                int.TryParse(dic["MaxRetries"], out intMaxRetries);
            int intInterval = 0;
            if(!string.IsNullOrEmpty(dic["Interval"]))
                int.TryParse(dic["Interval"], out intInterval); // ie 30 dagar

            DateTime dtStartDateTime = DateTime.Now;
            if (!string.IsNullOrEmpty(dic["StartDateTime"]))
            {
                DateTime.TryParse(dic["StartDateTime"], out dtStartDateTime);
                if (dtStartDateTime == DateTime.MinValue)
                    dtStartDateTime = DateTime.Now;
            }
            string strEmailText = string.IsNullOrEmpty(dic["EmailText"]) ? string.Empty : dic["EmailText"];

            bool bInvoice = !string.IsNullOrEmpty(dic["Invoice"]) && dic["Invoice"] == "1";
            bool bDibs = !string.IsNullOrEmpty(dic["Dibs"]) && dic["Dibs"] == "1" ;
            bool bOcr = !string.IsNullOrEmpty(dic["Ocr"])&& dic["Ocr"] == "1";
            bool bSendEmailAlert = !string.IsNullOrEmpty(dic["SendEmailAlert"]) && dic["SendEmailAlert"] == "1";
            bool bReminder = !string.IsNullOrEmpty(dic["Reminder"]) && dic["Reminder"] == "1";
            string strEmailSubject = string.IsNullOrEmpty(dic["EmailSubject"]) ? string.Empty : dic["EmailSubject"];
            string strAcceptUrl = string.IsNullOrEmpty(dic["AcceptUrl"]) ? string.Empty : dic["AcceptUrl"];
            string strCancelUrl = string.IsNullOrEmpty(dic["CancelUrl"]) ? string.Empty : dic["CancelUrl"];

            string strReference = string.IsNullOrEmpty(dic["Reference"]) ? string.Empty : dic["Reference"];

            var items = new List<OrderItem>();
            if (!string.IsNullOrEmpty(dic["orderitems"]))
            {
                try
                {
                    var json = dic["orderitems"];
                    json = json.Replace(@"\", "#BACKSLASH#");
                    items = JsonConvert.DeserializeObject<List<OrderItem>>(json);
                    foreach (var item in items)
                    {
                        item.ContentName = item.ContentName.Replace("#BACKSLASH#", @"\");
                        item.ContentDescription = item.ContentDescription.Replace("#BACKSLASH#", @"\");
                    }
                }
                catch (Exception ex)
                {
                    mCustomerOrder.Status = RestStatus.ParameterError;
                    mCustomerOrder.StatusText = "Parameter Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
                }
            }
            else
            {
                items.Add(new OrderItem()
                {
                    ContentName = string.IsNullOrEmpty(dic["ContentName"]) ? string.Empty : dic["ContentName"],
                    ContentDescription = string.IsNullOrEmpty(dic["ContentDescription"]) ? string.Empty : dic["ContentDescription"],
                    Price = string.IsNullOrEmpty(dic["Price"]) ? 0 : Convert.ToDecimal(dic["Price"]),
                    Vat = string.IsNullOrEmpty(dic["VAT"]) ? 25 : Convert.ToDecimal(dic["VAT"])
                });
            }

            // Validate
            strPhoneNo = ML.Common.SmsHelper.CleanPhoneNumber(strPhoneNo);

            // Lookup customer
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);
            if (customer == null)
            {
                // Create new customer
                var status = new DB.CustomerService().AddUpdateCustomer(
                    guidCompanyGuid
                    , strPhoneNo
                    , Customer.Enums.CustomerType.Api
                    , strFirstName
                    , strLastName
                    , customerId
                    , string.Empty
                    , string.Empty
                    , strEmail
                    , strAddress
                    , strZipCode
                    , strCity
                    );
                if (status == DB.Repository.Status.Exception)
                {
                    mCustomerOrder.Status = RestStatus.DataError;
                    mCustomerOrder.StatusText = "Exception have occured";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
                }
                // Reget in context
                customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);

                if (customer == null)
                {
                    mCustomerOrder.Status = RestStatus.NotExisting;
                    mCustomerOrder.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
                }
            }
            else
            {
                // Update customer
                var status = new DB.CustomerService().AddUpdateCustomer(
                    guidCompanyGuid
                    , strPhoneNo
                    , Customer.Enums.CustomerType.Api
                    , strFirstName
                    , strLastName
                    , customerId
                    , string.Empty
                    , string.Empty
                    , strEmail
                    , strAddress
                    , strZipCode
                    , strCity
                    );
                if (status==DB.Repository.Status.Exception)
                {
                    mCustomerOrder.Status = RestStatus.DataError;
                    mCustomerOrder.StatusText = "Exception have occured";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
                }
            }

            // Get Default ContentTemplate
            IQueryable<DB.tContentTemplate> contentTemplates = new DB.ContentTemplateRepository().GetByCompanyGuid(guidCompanyGuid);
            if(!contentTemplates.Any())
            {
                mCustomerOrder.Status = RestStatus.NotExisting;
                mCustomerOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
            }
            
            XmlDocument xml = new XmlDocument();
            xml.LoadXml("<xml></xml>");
            DB.CartService cartService = new DB.CartService();
            // Empty Cart
            cartService.EmptyCart(customer.CustomerGuid);
            foreach (var item in items)
            {
                // Create Content
                Guid guidContentGuid = Guid.NewGuid();
                DB.Repository.Status contentStatus = new DB.ContentService().SaveContent(
                    contentTemplates.FirstOrDefault().ContentTemplateGuid
                    , Guid.Empty
                    , item.ContentName??""
                    , string.Empty
                    , item.ContentDescription??""
                    , true
                    , Convert.ToDateTime("2099-01-01")
                    , xml
                    , string.Empty
                    , true
                    , 0
                    , item.Price??0
                    , item.Vat??0
                    , guidContentGuid
                    );
                if (contentStatus != DB.Repository.Status.Success)
                {
                    mCustomerOrder.Status = RestStatus.GenericError;
                    mCustomerOrder.StatusText = "Generic Error 1";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
                }
                // Add to Cart
                int intCartRc = cartService.AddToCart(customer.CustomerGuid, guidContentGuid, Guid.Empty, 1, Guid.Empty, DB.ContentRepository.PriceType.Regular);
                if (intCartRc == -1)
                {
                    mCustomerOrder.Status = RestStatus.GenericError;
                    mCustomerOrder.StatusText = "Generic Error 2";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
                }
            }
            
            // Create Order
            DB.OrderService orderService = new DB.OrderService();
            DB.OrderStatusRepository.OrderStatus orderStatus = orderService.CreateOrder(customer.CustomerGuid, customer.CompanyGuid, Guid.Empty, strOrderText, true, Guid.Empty, true, strExternalTrackingNo, strExternalMessage, DB.OrderStatusRepository.ClientType.Api, strAcceptUrl, strCancelUrl, strCurrency);
            if (orderStatus != DB.OrderStatusRepository.OrderStatus.Created)
            {
                mCustomerOrder.Status = RestStatus.GenericError;
                mCustomerOrder.StatusText = "Generic Error 3";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
            }
            mCustomerOrder.OrderGuid = orderService.OrderGuid.ToString();

            try
            {
                mCustomerOrder.Test = customer.tCompany.tShopConfig.FirstOrDefault().TestMode;
            }
            catch
            {
                mCustomerOrder.Test = true;
            }

            // Create Invoice
            if(bInvoice)
            {
                if (new DB.InvoiceService().CreateInvoice(orderService.OrderGuid, strInvoiceTitle, strInvoiceText, decReminderFee, intMaxRetries, intInterval, dtStartDateTime, strEmailText, bDibs, bOcr, bSendEmailAlert, strEmailSubject, bReminder, strReference) != DB.Repository.Status.Success)
                {
                    mCustomerOrder.Status = RestStatus.GenericError;
                    mCustomerOrder.StatusText = "Generic Error 4";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
                }
            }

            // Success
            mCustomerOrder.Status = RestStatus.Success;
            mCustomerOrder.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        }

        public class OrderItem
        {
            public  decimal? Vat { get; set; }
            public decimal? Price { get; set; }
            public string ContentName { get; set; }
            public string ContentDescription { get; set; }
        }
    }
}
