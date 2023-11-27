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
    public class OrderSearchController : ApiController
    {
        List<Order> _mOrders = new List<Order>();
        Order _mOrder = new Order();

        public OrderSearchController()
        {
            _mOrders.Add(_mOrder);
        }

        public HttpResponseMessage Post(string token, int take)
        {
            return Post(token, Guid.Empty.ToString(), take);
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
            string strSearch = string.IsNullOrEmpty(dic["Search"]) ? string.Empty : dic["Search"];

            // Search orders
            IQueryable<DB.tOrder> orders = null;
            bool bFound = false;
            if (ML.Common.Text.IsNumeric(strSearch))
            {
                // Try OrderId
                orders = new DB.OrderRepository().GetByCompanyGuidAndOrderId(usertoken.CompanyGuid, Convert.ToInt32(strSearch));
                bFound = orders.Any();

                // Try Payment TransactionNo
                if (!bFound)
                {
                    orders = new DB.OrderRepository().GetPaymentByTransactionNo(usertoken.CompanyGuid, strSearch);
                    bFound = orders.Any();
                }
            }

            // Try search
            if (!bFound)
            {
                IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(new Guid(id));
                if (orderPrinters.Count() > 1 || orderPrinters.Count() == 0)
                {
                    orders = new DB.OrderRepository().SearchOrders(usertoken.CompanyGuid, Guid.Empty, take, dtStartDateTime, dtEndDateTime, strSearch);
                }
                else
                {
                    orders = new DB.OrderRepository().SearchOrders(usertoken.CompanyGuid, orderPrinters.FirstOrDefault().OrderPrinterGuid, take, dtStartDateTime, dtEndDateTime, strSearch);
                }

                //new ML.Email.Email().SendDebug("OrderSearchController.Post 1", "orders: " + orders.Count().ToString() + " dtStartDateTime: " + dtStartDateTime.ToString() + " dtEndDateTime: " + dtEndDateTime.ToString() + " strSearch: " + strSearch + " id: " + id.ToString() + " take: " + take.ToString());
            }

            // Populate
            _mOrders.Clear();
            foreach (DB.tOrder order in orders)
            {
                var status = order.tOrderStatus.FirstOrDefault( i => i.OrderStatusTypeID == (int)DB.OrderStatusRepository.OrderStatus.TimeOut);
                if(status ==null)
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




    }
}
