using System.Web.UI;
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

namespace ML.Rest2.Controllers
{
    public class CustomerOrderStatusController : ApiController
    { }
    //    public class CustomerOrderStatusController : ApiController
    //{
    //    public HttpResponseMessage Get(string secret, string companyId, string orderId)
    //    {
    //        List<CustomerOrderStatus> mOrderStatusList = new List<CustomerOrderStatus>();
    //        CustomerOrderStatus mOrderStatus = new CustomerOrderStatus();

    //        mOrderStatusList.Add(mOrderStatus);

    //        if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(orderId))
    //        {
    //            mOrderStatus.Status = RestStatus.ParameterError;
    //            mOrderStatus.StatusText = "Parameter Error";
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }

    //        if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
    //        {
    //            mOrderStatus.Status = RestStatus.AuthenticationFailed;
    //            mOrderStatus.StatusText = "Authentication Failed";
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }

    //        Guid guidCompanyGuid = new Guid(companyId);
    //        Guid guidOrderGuid = new Guid(orderId);

    //        DB.tOrder order = new DB.OrderRepository().GetByOrderGuid(guidOrderGuid);
    //        if(order == null)
    //        {
    //            mOrderStatus.Status = RestStatus.NotExisting;
    //            mOrderStatus.StatusText = "Not Existing";
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }

    //        if(order.tCustomer.CompanyGuid != guidCompanyGuid)
    //        {
    //            mOrderStatus.Status = RestStatus.NotExisting;
    //            mOrderStatus.StatusText = "Not Existing";
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }

    //        DB.OrderStatusRepository.OrderStatus orderStatus = DB.OrderStatusService.GetCurrentOrderStatus(order.CustomerGuid);
    //        if (orderStatus == DB.OrderStatusRepository.OrderStatus.None
    //            || orderStatus == DB.OrderStatusRepository.OrderStatus.Creating
    //            || orderStatus == DB.OrderStatusRepository.OrderStatus.Created
    //            || orderStatus == DB.OrderStatusRepository.OrderStatus.Processing
    //            || orderStatus == DB.OrderStatusRepository.OrderStatus.Processed
    //            || orderStatus == DB.OrderStatusRepository.OrderStatus.Completing)
    //        {
    //            if(order.tOrderPrinter.OrderPrinterGuid!=Guid.Empty && DateTime.Now > order.TimeStamp.AddSeconds(order.tOrderPrinter.TransactionTimeOut))
    //            {
    //                new DB.OrderService().SetOrderStatusForCustomer(order.CustomerGuid, DB.OrderStatusRepository.OrderStatus.TimeOut, DB.OrderStatusRepository.ClientType.Api);
    //                orderStatus = DB.OrderStatusRepository.OrderStatus.TimeOut;
    //            }
    //        }

    //        //SHHAID > New Statuses that are not supported via current implementation
    //        //TODO: Current function need to be re-written
    //        if (orderStatus == DB.OrderStatusRepository.OrderStatus.DriverAcknowledged || orderStatus == DB.OrderStatusRepository.OrderStatus.ShopAcknowledged)
    //            orderStatus = DB.OrderStatusRepository.OrderStatus.Completed;
    //        // Populate
    //        mOrderStatus.OrderStatus = orderStatus;
    //        mOrderStatus.OrderStatusText = orderStatus.ToString();

    //        //mOrderStatus.OrderStatusMessage = DB.OrderStatusMessageService.GetCurrentOrderStatusMessageByOrderGuid(order.OrderGuid);
    //        mOrderStatus.OrderStatusMessage = DB.OrderStatusMessageService.GetCurrentOrderStatusMessageByCustomerGuid(order.CustomerGuid);
    //        mOrderStatus.OrderStatusMessageShort = DB.OrderStatusMessageService.GetCurrentOrderStatusMessageShortByCustomerGuid(order.CustomerGuid);

    //        mOrderStatus.Status = RestStatus.Success;
    //        mOrderStatus.StatusText = "Success";
    //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //    }

    //    public HttpResponseMessage Post(string secret, string companyId, string orderId, int status)
    //    {
    //        List<CustomerOrderStatus> mOrderStatusList = new List<CustomerOrderStatus>();
    //        var mOrderStatus = new CustomerOrderStatus();
    //        if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(orderId))
    //        {
    //            mOrderStatus.Status = RestStatus.ParameterError;
    //            mOrderStatus.StatusText = "Parameter Error";
    //            mOrderStatusList.Add(mOrderStatus);
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }

    //        if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
    //        {
    //            mOrderStatus.Status = RestStatus.AuthenticationFailed;
    //            mOrderStatus.StatusText = "Authentication Failed";
    //            mOrderStatusList.Add(mOrderStatus);
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }

    //        Guid id;
    //        Guid.TryParse(orderId, out id);
    //        if (id == Guid.Empty)
    //        {
    //            mOrderStatus.Status = RestStatus.DataError;
    //            mOrderStatus.StatusText = "Invalid Order Id";
    //            mOrderStatusList.Add(mOrderStatus);
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }   
            
    //       var orderRepository = new DB.OrderRepository();
    //        DB.tOrder order = orderRepository.GetByOrderGuid(id);
    //        if (order == null)
    //        {
    //            mOrderStatus.Status = RestStatus.DataError;
    //            mOrderStatus.StatusText = "Order doesNot Exist";
    //            mOrderStatusList.Add(mOrderStatus);
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }

    //        if (DB.OrderStatusService.HasOrderStatus(id, (DB.OrderStatusRepository.OrderStatus)status))
    //        {
    //            mOrderStatus.Status = RestStatus.AlreadyExists;
    //            mOrderStatus.StatusText = "Order status already exisits";
    //            mOrderStatusList.Add(mOrderStatus);
    //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //        }


    //        DB.OrderStatusService.AddOrderStatus(order, (DB.OrderStatusRepository.OrderStatus)status, DB.OrderStatusRepository.ClientType.Api);
    //        if (status == (int)DB.OrderStatusRepository.OrderStatus.OrderCancelled || status == (int)DB.OrderStatusRepository.OrderStatus.Completed)
    //        {
    //            if (order.tInvoice.Count > 0)
    //            {
    //                foreach (var invoice in order.tInvoice.ToList())
    //                {
    //                    invoice.Completed = true;
    //                }
    //                orderRepository.Save();
    //            }
    //        }
    //        mOrderStatus.Status = RestStatus.Success;
    //        mOrderStatus.StatusText = "Status updated";
    //        mOrderStatusList.Add(mOrderStatus);
    //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mOrderStatusList));
    //    }
    //}
}
