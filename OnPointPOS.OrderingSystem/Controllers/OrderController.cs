using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class OrderController : ApiController
    {
        List<Order> _mOrders = new List<Order>();
        Order _mOrder = new Order();
        
        public OrderController()
        {
            _mOrders.Add(_mOrder);
        }

        // id = OrderPrinterGuid or ContentCategoryGuid
        public HttpResponseMessage Post(string token, string id, int take)
        {   
            if (!Request.Content.IsFormData())
            {
                _mOrder.Status = RestStatus.NotFormData;
                _mOrder.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(id) && take < 1)
            {
                _mOrder.Status = RestStatus.ParameterError;
                _mOrder.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mOrder.Status = RestStatus.AuthenticationFailed;
                _mOrder.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            DateTime dtStartDateTime = string.IsNullOrEmpty(dic["StartDateTime"]) ? Convert.ToDateTime("2000-01-01 00:00:00") : Convert.ToDateTime(dic["StartDateTime"]);
            DateTime dtEndDateTime = string.IsNullOrEmpty(dic["EndDateTime"]) ? DateTime.Now : Convert.ToDateTime(dic["EndDateTime"]);

            // Get Orders
            IQueryable<DB.tOrder> orders = null;
            IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(new Guid(id));
            if (orderPrinters.Count() > 1 || orderPrinters.Count() == 0)
            {
                orders = new DB.OrderRepository().SearchOrders(usertoken.CompanyGuid, Guid.Empty, take, dtStartDateTime, dtEndDateTime, string.Empty);
            }
            else
            {
                orders = new DB.OrderRepository().SearchOrders(usertoken.CompanyGuid, orderPrinters.FirstOrDefault().OrderPrinterGuid, take, dtStartDateTime, dtEndDateTime, string.Empty);
            }

            // Populate
            _mOrders.Clear();
            foreach (DB.tOrder order in orders)
            {
                var status = order.tOrderStatus.FirstOrDefault(i => i.OrderStatusTypeID == (int)DB.OrderStatusRepository.OrderStatus.TimeOut);
                if (status == null)
                    status = order.tOrderStatus.FirstOrDefault(i => i.OrderStatusTypeID == (int)DB.OrderStatusRepository.OrderStatus.Rejected);
                if (status == null)
                    status = order.tOrderStatus.FirstOrDefault(i => i.OrderStatusTypeID == (int)DB.OrderStatusRepository.OrderStatus.Completed);
                
                _mOrder = new Order();

                _mOrder.OrderGuid = order.OrderGuid.ToString();
                _mOrder.OrderId = order.OrderID;
                _mOrder.DailyOrderNo = order.DailyOrderNo;
                _mOrder.TimeStamp = order.TimeStamp;
                _mOrder.OrderPrinter = order.tOrderPrinter.Name;
                if (!string.IsNullOrEmpty(order.tCustomer.PhoneNo))
                    _mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
                else
                    _mOrder.CustomerFullName = string.Format("{0} {1}", order.tCustomer.FirstName, order.tCustomer.LastName);
                _mOrder.TotalAmount = new DB.OrderService().GetTotal(order);
                if (status!=null && status.OrderStatusTypeID != (int)DB.OrderStatusRepository.OrderStatus.Completed)
                {
                    _mOrder.TotalAmount = 0;
                }
                //NOT COMPLETED ORDERS
                _mOrder.StatusId = status != null ? status.OrderStatusTypeID : -3;

                _mOrder.TotalPaymentAmount = 0;
                if (order.PaymentRequired)
                {
                    DB.tPayment payment = new DB.PaymentRepository().GetByOrderGuid(order.OrderGuid);
                    if (payment != null && status != null)
                    {
                        if (status.OrderStatusTypeID == (int)DB.OrderStatusRepository.OrderStatus.Completed && !payment.Test)
                        {
                            _mOrder.TotalPaymentAmount = payment.Amount;
                        }
                    }
                }

                _mOrders.Add(_mOrder);
                
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
        }

        /// <summary>
        /// Get one or many orders
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// id = OrderId or CustomerId
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mOrder.Status = RestStatus.ParameterError;
                _mOrder.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrder));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mOrder.Status = RestStatus.AuthenticationFailed;
                _mOrder.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            IQueryable<DB.tOrder> orders = new DB.OrderRepository().GetOrder(new Guid(id));
            if (!orders.Any())
            {
                orders = new DB.OrderRepository().GetByCustomerGuid(new Guid(id));

                if (orders == null)
                {
                    _mOrder.Status = RestStatus.NotExisting;
                    _mOrder.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
                }
            }

            // Populate
            _mOrders.Clear();
            foreach (DB.tOrder order in orders)
            {
                _mOrder = new Order();

                _mOrder.OrderGuid = order.OrderGuid.ToString();
                _mOrder.OrderId = order.OrderID;
                _mOrder.DailyOrderNo = order.DailyOrderNo;
                _mOrder.TimeStamp = order.TimeStamp;
                _mOrder.OrderPrinter = order.tOrderPrinter.Name;
                if (!string.IsNullOrEmpty(order.tCustomer.PhoneNo))
                    _mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
                else
                    _mOrder.CustomerFullName = string.Format("{0} {1}", order.tCustomer.FirstName, order.tCustomer.LastName);
                _mOrder.TotalAmount = new DB.OrderService().GetTotal(order);

                _mOrder.StatusId = order.tOrderStatus.OrderByDescending(os => os.Sequence).FirstOrDefault().OrderStatusTypeID;

                _mOrder.Receipt = new DB.OrderService().GetOrderReceipt(order);

                //DB.tPayment payment = new DB.PaymentRepository().GetByOrderGuid(order.OrderGuid);
                //_mOrder.PaymentId = payment == null ? Guid.Empty.ToString() : payment.PaymentGuid.ToString();

                _mOrders.Add(_mOrder);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
        }







    }
}
