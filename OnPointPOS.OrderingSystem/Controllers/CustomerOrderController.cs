using ML.Common.Handlers.Serializers;
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
using Models;
using static Models.Order;

namespace ML.Rest2.Controllers
{
    public class CustomerOrderController : ApiController

    {
        //List<CustomerOrder> _mCustomerOrders = new List<CustomerOrder>();
        //CustomerOrder _mCustomerOrder = new CustomerOrder();

        //public CustomerOrderController()
        //{
        //    _mCustomerOrders.Add(_mCustomerOrder);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="customerId"></param>
        /// <param name="id"></param>
        /// id = contentTemplateGuid or contentCategoryId or postalId or orderPrinterId
        /// <returns></returns>
        public HttpResponseMessage Post(string secret, string companyId, string customerId, string id)
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

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mCustomerOrder.Status = RestStatus.ParameterError;
                mCustomerOrder.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
            }
            Guid guidId = new Guid(id);



            // OrderPrinter based on PostalGuid ?
            Guid guidPostalGuid = Guid.Empty;
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            Guid guidPostalGroupGuid = Guid.Empty;
            if (!string.IsNullOrEmpty(dic["guidPostalGroupGuid"]))
                Guid.TryParse(dic["guidPostalGroupGuid"], out guidPostalGroupGuid);
            
            

            // Prepare


            string strExternalTrackingNo = string.IsNullOrEmpty(dic["TrackingId"]) ? string.Empty : dic["TrackingId"];
            string strExternalMessage = string.IsNullOrEmpty(dic["TrackingMessage"]) ? string.Empty : dic["TrackingMessage"];

            string strText = string.IsNullOrEmpty(dic["Text"]) ? string.Empty : dic["Text"];
            bool bPaymentRequired = string.IsNullOrEmpty(dic["PaymentRequired"]) ? false : dic["PaymentRequired"] == "1" ? true : false;

            string strDoorCode = string.IsNullOrEmpty(dic["DoorCode"]) ? string.Empty : dic["DoorCode"];
            int intFloor = string.IsNullOrEmpty(dic["Floor"]) ? 0 : Convert.ToInt32(dic["Floor"]);
            int intApartmentNumber = string.IsNullOrEmpty(dic["ApartmentNumber"]) ? 0 : Convert.ToInt32(dic["ApartmentNumber"]);
            string strDeliveryMessage = string.IsNullOrEmpty(dic["DeliveryMessage"]) ? string.Empty : dic["DeliveryMessage"];
            string strStreetnumber = string.IsNullOrEmpty(dic["StreetNumber"]) ? string.Empty : dic["StreetNumber"];
            DateTime dtDeliveryDateTime = string.IsNullOrEmpty(dic["DeliveryDateTime"]) ? DateTime.Now : Convert.ToDateTime(dic["DeliveryDateTime"]);
            string strAcceptUrl = string.IsNullOrEmpty(dic["AcceptUrl"]) ? string.Empty : dic["AcceptUrl"];
            string strCancelUrl = string.IsNullOrEmpty(dic["CancelUrl"]) ? string.Empty : dic["CancelUrl"];
            string strCurrency = string.IsNullOrEmpty(dic["Currency "]) ? string.Empty : dic["Currency "];

            // Create Order
            //DB.OrderService orderService = new DB.OrderService();
            //DB.OrderStatusRepository.OrderStatus orderStatus = orderService.CreateOrder(customer.CustomerGuid, customer.CompanyGuid, guidOrderPrinterGuid, strText, bPaymentRequired, guidPostalGuid, guidPostalGroupGuid, strDoorCode, intFloor, intApartmentNumber, strStreetnumber, strDeliveryMessage, dtDeliveryDateTime, false, strExternalTrackingNo, strExternalMessage, DB.OrderStatusRepository.ClientType.Api, strAcceptUrl, strCancelUrl, strCurrency);
           

            mCustomerOrder.OrderGuid = "96364eb4-989f-41e5-b7be-00839bfc118b";
            mCustomerOrder.Test = false;

            // string strDibsUrl = string.Format("http://pay.moblink.se/dibs/initialize/{0}/{1}/{2}/?cancelUrl={3}&acceptUrl={4}", _customer.tCompany.Password, _customer.CompanyGuid.ToString(), orderService.OrderGuid.ToString(), strCancelUrl, strAcceptUrl);
            // string strDibsUrl = string.Format("http://pay.moblink.se/dibs/initialize/{0}/{1}/{2}/?cancelUrl={3}&acceptUrl={4}", [secret], [companyId], [OrderId]), strCancelUrl, strAcceptUrl);


            // Success
            mCustomerOrder.Status = RestStatus.Success;
            mCustomerOrder.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        }




        //public HttpResponseMessage GetOld(string secret, string companyId, string customerId)
        //{
        //    List<Order> mOrders = new List<Order>();
        //    Order mOrder = new Order();

        //    mOrders.Add(mOrder);

        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId))
        //    {
        //        mOrder.Status = RestStatus.ParameterError;
        //        mOrder.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        mOrder.Status = RestStatus.AuthenticationFailed;
        //        mOrder.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        //    }

        //    // Lookup customer
        //    DB.CustomerRepository customerRepository = new DB.CustomerRepository();
        //    DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), customerId);
        //    if (customer == null)
        //    {
        //        mOrder.Status = RestStatus.NotExisting;
        //        mOrder.StatusText = "NotExisting";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        //    }

        //    IQueryable<DB.tOrder> orders = new DB.OrderRepository().GetByCustomerGuid(customer.CustomerGuid);
        //    if (!orders.Any())
        //    {
        //        mOrder.Status = RestStatus.NotExisting;
        //        mOrder.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        //    }

        //    // Populate
        //    mOrders.Clear();
        //    foreach (DB.tOrder order in orders)
        //    {
        //        mOrder = new Order();

        //        mOrder.OrderGuid = order.OrderGuid.ToString();
        //        mOrder.OrderId = order.OrderID;
        //        mOrder.DailyOrderNo = order.DailyOrderNo;
        //        mOrder.TimeStamp = order.TimeStamp;
        //        mOrder.OrderPrinter = order.tOrderPrinter.Name;
        //        mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
        //        mOrder.TotalAmount = new DB.OrderService().GetTotal(order);

        //        mOrder.Receipt = new DB.OrderService().GetOrderReceipt(order);

        //        mOrders.Add(mOrder);
        //    }

        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        //}

         
         
        //public HttpResponseMessage PostOld(string secret, string companyId, string customerId, string id)
        //{
        //    List<CustomerOrder> mCustomerOrders = new List<CustomerOrder>();
        //    CustomerOrder mCustomerOrder = new CustomerOrder();

        //    mCustomerOrders.Add(mCustomerOrder);

        //    if (!Request.Content.IsFormData())
        //    {
        //        mCustomerOrder.Status = RestStatus.NotFormData;
        //        mCustomerOrder.StatusText = "Not FormData";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //    }

        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId) && !ML.Common.Text.IsGuidNotEmpty(id))
        //    {
        //        mCustomerOrder.Status = RestStatus.ParameterError;
        //        mCustomerOrder.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //    }
        //    Guid guidId = new Guid(id);

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        mCustomerOrder.Status = RestStatus.AuthenticationFailed;
        //        mCustomerOrder.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //    }

        //    // Lookup customer
        //    DB.CustomerRepository customerRepository = new DB.CustomerRepository();
        //    DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), customerId);
        //    if (customer == null)
        //    {
        //        mCustomerOrder.Status = RestStatus.NotExisting;
        //        mCustomerOrder.StatusText = "NotExisting";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //    }

        //    DB.tOrderPrinter orderPrinter = null;

        //    // OrderPrinter based on PostalGuid ?
        //    Guid guidPostalGuid = Guid.Empty;
        //    DB.tPostal postal = new DB.PostalRepository().GetPostal(guidId);

        //    System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
        //    Guid guidPostalGroupGuid = Guid.Empty;
        //    if (!string.IsNullOrEmpty(dic["guidPostalGroupGuid"]))
        //        Guid.TryParse(dic["guidPostalGroupGuid"], out guidPostalGroupGuid);
        //    if (guidPostalGroupGuid != Guid.Empty)
        //    {
        //        Guid printerGuid = Guid.Empty;
        //        if (!string.IsNullOrEmpty(dic["OrderPrinterGuid"]))
        //            Guid.TryParse(dic["OrderPrinterGuid"], out printerGuid);
        //        orderPrinter = new DB.OrderPrinterRepository().GetByOrderPrinterGuid(printerGuid);
        //    }
        //    else if (postal != null)
        //    {
        //        guidPostalGuid = postal.PostalGuid;

        //        DB.tOrderPrinterZipCode orderPrinterZipCode = new DB.OrderPrinterZipCodeRepository().GetByZipCodeAndCompanyGuid(postal.ZipCode, new Guid(companyId));
        //        if (orderPrinterZipCode == null)
        //        {
        //            mCustomerOrder.Status = RestStatus.NotExisting;
        //            mCustomerOrder.StatusText = "NotExisting";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //        }

        //        orderPrinter = new DB.OrderPrinterRepository().GetByOrderPrinterGuid(orderPrinterZipCode.OrderPrinterGuid);
        //        if (orderPrinter == null)
        //        {
        //            mCustomerOrder.Status = RestStatus.NotExisting;
        //            mCustomerOrder.StatusText = "NotExisting";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //        }
        //    }
        //    else
        //    {
        //        // OrderPrinter based on Menu ?
        //        orderPrinter = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidId).FirstOrDefault();
        //        if (orderPrinter == null)
        //        {
        //            // OrderPrinter based on default Menu ?
        //            DB.tContentCategory contentCategory = new DB.ContentCategoryRepository().GetByContentTemplateGuidAndContentParentCategoryGuid(guidId, Guid.Empty).FirstOrDefault();
        //            if (contentCategory != null)
        //            {
        //                orderPrinter = new DB.OrderPrinterRepository().GetByContentCategoryGuid(contentCategory.ContentCategoryGuid).FirstOrDefault();
        //            }
        //        }
        //    }

        //    // OrderPrinter based on OrderPrinterGuid
        //    if (orderPrinter == null)
        //    {
        //        orderPrinter = new DB.OrderPrinterRepository().GetByOrderPrinterGuid(guidId);
        //    }

        //    // Ensure OrderPrinter (if due) belongs to Company
        //    Guid guidOrderPrinterGuid = Guid.Empty;
        //    if (orderPrinter != null)
        //    {
        //        if (orderPrinter.OrderPrinterGuid != Guid.Empty)
        //        {
        //            if (customer.CompanyGuid != orderPrinter.CompanyGuid)
        //            {
        //                mCustomerOrder.Status = RestStatus.NotExisting;
        //                mCustomerOrder.StatusText = "NotExisting";
        //                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //            }
        //            guidOrderPrinterGuid = orderPrinter.OrderPrinterGuid;
        //        }
        //    }

        //    // Prepare


        //    string strExternalTrackingNo = string.IsNullOrEmpty(dic["TrackingId"]) ? string.Empty : dic["TrackingId"];
        //    string strExternalMessage = string.IsNullOrEmpty(dic["TrackingMessage"]) ? string.Empty : dic["TrackingMessage"];

        //    string strText = string.IsNullOrEmpty(dic["Text"]) ? string.Empty : dic["Text"];
        //    bool bPaymentRequired = string.IsNullOrEmpty(dic["PaymentRequired"]) ? false : dic["PaymentRequired"] == "1" ? true : false;

        //    string strDoorCode = string.IsNullOrEmpty(dic["DoorCode"]) ? string.Empty : dic["DoorCode"];
        //    int intFloor = string.IsNullOrEmpty(dic["Floor"]) ? 0 : Convert.ToInt32(dic["Floor"]);
        //    int intApartmentNumber = string.IsNullOrEmpty(dic["ApartmentNumber"]) ? 0 : Convert.ToInt32(dic["ApartmentNumber"]);
        //    string strDeliveryMessage = string.IsNullOrEmpty(dic["DeliveryMessage"]) ? string.Empty : dic["DeliveryMessage"];
        //    string strStreetnumber = string.IsNullOrEmpty(dic["StreetNumber"]) ? string.Empty : dic["StreetNumber"];
        //    DateTime dtDeliveryDateTime = string.IsNullOrEmpty(dic["DeliveryDateTime"]) ? DateTime.Now : Convert.ToDateTime(dic["DeliveryDateTime"]);
        //    string strAcceptUrl = string.IsNullOrEmpty(dic["AcceptUrl"]) ? string.Empty : dic["AcceptUrl"];
        //    string strCancelUrl = string.IsNullOrEmpty(dic["CancelUrl"]) ? string.Empty : dic["CancelUrl"];
        //    string strCurrency = string.IsNullOrEmpty(dic["Currency "]) ? string.Empty : dic["Currency "];

        //    // Create Order
        //    DB.OrderService orderService = new DB.OrderService();
        //    DB.OrderStatusRepository.OrderStatus orderStatus = orderService.CreateOrder(customer.CustomerGuid, customer.CompanyGuid, guidOrderPrinterGuid, strText, bPaymentRequired, guidPostalGuid, guidPostalGroupGuid, strDoorCode, intFloor, intApartmentNumber, strStreetnumber, strDeliveryMessage, dtDeliveryDateTime, false, strExternalTrackingNo, strExternalMessage, DB.OrderStatusRepository.ClientType.Api, strAcceptUrl, strCancelUrl, strCurrency);
        //    if (orderStatus != DB.OrderStatusRepository.OrderStatus.Created)
        //    {
        //        mCustomerOrder.Status = RestStatus.GenericError;
        //        mCustomerOrder.StatusText = "Generic Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //    }

        //    mCustomerOrder.OrderGuid = orderService.OrderGuid.ToString();

        //    // string strDibsUrl = string.Format("http://pay.moblink.se/dibs/initialize/{0}/{1}/{2}/?cancelUrl={3}&acceptUrl={4}", _customer.tCompany.Password, _customer.CompanyGuid.ToString(), orderService.OrderGuid.ToString(), strCancelUrl, strAcceptUrl);
        //    // string strDibsUrl = string.Format("http://pay.moblink.se/dibs/initialize/{0}/{1}/{2}/?cancelUrl={3}&acceptUrl={4}", [secret], [companyId], [OrderId]), strCancelUrl, strAcceptUrl);


        //    // Success
        //    mCustomerOrder.Status = RestStatus.Success;
        //    mCustomerOrder.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCustomerOrders));
        //}

    }
}
