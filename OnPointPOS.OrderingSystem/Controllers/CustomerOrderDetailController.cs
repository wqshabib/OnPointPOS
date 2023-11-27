using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using ML.Common.Handlers.Serializers;
using ML.DB;
using ML.Rest2.Models;

namespace ML.Rest2.Controllers
{
    public class CustomerOrderDetailController : ApiController
    {

        public HttpResponseMessage Get(string secret, string companyId, string orderId)
        {
            List<Order> mOrders = new List<Order>();
            Order mOrder = new Order();

            mOrders.Add(mOrder);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(orderId))
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
            DB.tOrder order = new DB.OrderRepository().GetOrder(new Guid(orderId)).FirstOrDefault();
            if (order==null)
            {  
                mOrder.Status = RestStatus.NotExisting;
                mOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
            }
            // Populate
            mOrders.Clear();
            mOrder = new Order();

            mOrder.OrderGuid = order.OrderGuid.ToString();
            mOrder.OrderId = order.OrderID;
            mOrder.DailyOrderNo = order.DailyOrderNo;
            mOrder.TimeStamp = order.TimeStamp;
            if (order.tOrderPrinter != null)
                mOrder.OrderPrinter = order.tOrderPrinter.Name;
            if (order.tCustomer != null)
                mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
            mOrder.TotalAmount = new DB.OrderService().GetTotal(order);
            var invoice = order.tInvoice.OrderByDescending(i => i.TimeStamp).FirstOrDefault();
            if (invoice!=null)
            {
                mOrder.InvoiceGuid = invoice.InvoiceGuid.ToString();
                mOrder.LastRetryDateTime = invoice.LastRetryDateTime;
                mOrder.NextReminderDateTime = invoice.LastRetryDateTime.AddDays(invoice.Interval);
                var ocr = invoice.tOcr.OrderByDescending(i => i.TimeStamp).FirstOrDefault();
                if (ocr != null)
                    mOrder.Ocr = ocr.Ocr;
            }
            mOrder.OrderStatuses = (from oStatus in order.tOrderStatus select new CustomerOrderStatus(oStatus) ).ToList(); 
            mOrder.OrderPayments = (from payment in order.tPayment select new CustomerPayment(payment)).ToList();
            mOrders.Add(mOrder);
            

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrders));
        }

	}
}