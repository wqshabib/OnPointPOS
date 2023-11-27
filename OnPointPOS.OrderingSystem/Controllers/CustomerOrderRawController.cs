using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class CustomerOrderRawController : ApiController
    {
        public HttpResponseMessage Get(string secret, string companyId, string customerId)
        {
            List<Order> mOrders = new List<Order>();
            Order mOrder = new Order();

            mOrders.Add(mOrder);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId))
            {
                mOrder.Status = RestStatus.ParameterError;
                mOrder.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                mOrder.Status = RestStatus.AuthenticationFailed;
                mOrder.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            // Lookup customer
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), customerId);
            if (customer == null)
            {
                mOrder.Status = RestStatus.NotExisting;
                mOrder.StatusText = "NotExisting";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            IQueryable<DB.tOrder> orders = new DB.OrderRepository().GetByCustomerGuid(customer.CustomerGuid);
            if (!orders.Any())
            {
                mOrder.Status = RestStatus.NotExisting;
                mOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            // Populate
            mOrders.Clear();
            foreach (DB.tOrder order in orders)
            {
                mOrder = new Order();

                mOrder.OrderGuid = order.OrderGuid.ToString();
                mOrder.OrderId = order.OrderID;
                mOrder.DailyOrderNo = order.DailyOrderNo;
                mOrder.TimeStamp = order.TimeStamp;
                mOrder.OrderPrinter = order.tOrderPrinter.Name;
                mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
                mOrder.TotalAmount = new DB.OrderService().GetTotal(order);

                //mOrder.Receipt = new DB.OrderService().GetOrderReceipt(order);

                mOrders.Add(mOrder);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        }

        public HttpResponseMessage Get(string secret, string companyId, string customerId, string orderId)
        {
            List<Order> mOrders = new List<Order>();
            Order mOrder = new Order();

            mOrders.Add(mOrder);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId))
            {
                mOrder.Status = RestStatus.ParameterError;
                mOrder.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                mOrder.Status = RestStatus.AuthenticationFailed;
                mOrder.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            // Lookup customer
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), customerId);
            if (customer == null)
            {
                mOrder.Status = RestStatus.NotExisting;
                mOrder.StatusText = "NotExisting";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            DB.tOrder order = new DB.OrderRepository().GetByOrderGuid(new Guid(orderId));
            if (order == null)
            {
                mOrder.Status = RestStatus.NotExisting;
                mOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }

            // Populate
            mOrder.OrderGuid = order.OrderGuid.ToString();
            mOrder.OrderId = order.OrderID;
            mOrder.DailyOrderNo = order.DailyOrderNo;
            mOrder.TimeStamp = order.TimeStamp;
            mOrder.OrderPrinter = order.tOrderPrinter.Name;
            mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
            mOrder.TotalAmount = new DB.OrderService().GetTotal(order);

            //mOrder.Receipt = new DB.OrderService().GetOrderReceiptText(order);
            mOrder.Receipt = new DB.OrderService().GetOrderReceipt(order);

            //HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            //htmlDoc.LoadHtml(mOrder.Receipt);

            DateTime dtDeliveryDateTime = order.DeliveryDateTime;

            var orderStatusMessageExtracted = new DB.OrderStatusMessageRepository().GetCurrentOrderStatusByCustomerGuid(customer.CustomerGuid);
            if (orderStatusMessageExtracted != null)
            {
                if(ML.Common.DateTimeHelper.IsTime(orderStatusMessageExtracted.Message))
                {
                    dtDeliveryDateTime = Convert.ToDateTime(string.Concat(dtDeliveryDateTime.Date.ToShortDateString(), " ", orderStatusMessageExtracted.Message));
                }
            }

            if (dtDeliveryDateTime.Date > DateTime.Now.Date)
            {
                mOrder.DeliveryDateTime = string.Concat(dtDeliveryDateTime.ToShortDateString(), " ", dtDeliveryDateTime.ToShortTimeString());
            }
            else
            {
                mOrder.DeliveryDateTime = dtDeliveryDateTime.ToShortTimeString();
            }

            mOrders.Add(mOrder);

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        }

    }




}
