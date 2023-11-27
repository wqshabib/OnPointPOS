using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class OrderSCMController : ApiController
    {
        List<Order> _mOrders = new List<Order>();
        Order _mOrder = new Order();

        private enum SCMStatus : int
        {
            None = 0
            , Picked = 1
        }

        public OrderSCMController()
        {
            _mOrders.Add(_mOrder);
        }

        // id = OrderPrinterGuid or ContentCategoryGuid
        public HttpResponseMessage Post(string token, string id, int scmStatusId, int take)
        {
            if (!Request.Content.IsFormData())
            {
                _mOrder.Status = RestStatus.NotFormData;
                _mOrder.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(id) && take < 1 && !ML.Common.Text.IsNumeric(scmStatusId))
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
            DB.OrderRepository.SCMStatus scmStatus = (DB.OrderRepository.SCMStatus)Enum.Parse(typeof(DB.OrderRepository.SCMStatus), scmStatusId.ToString());

            IQueryable<DB.tOrder> orders = null;
            IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(new Guid(id));
            if (orderPrinters.Count() > 1 || orderPrinters.Count() == 0)
            {
                orders = new DB.OrderRepository().GetCompletedOrders(usertoken.CompanyGuid, take, dtStartDateTime, dtEndDateTime, scmStatus);
            }
            else
            {
                orders = new DB.OrderRepository().GetCompletedOrders(usertoken.CompanyGuid, orderPrinters.FirstOrDefault().OrderPrinterGuid, take, dtStartDateTime, dtEndDateTime, scmStatus);
            }

            // Set sortorder
            if(scmStatus == DB.OrderRepository.SCMStatus.None)
            {
                // Do nothing...
            }
            else if (scmStatus == DB.OrderRepository.SCMStatus.Picked)
            {
                orders = orders.OrderByDescending(o => o.TimeStamp);
            }
            // Cont...

            // Populate
            _mOrders.Clear();
            foreach (DB.tOrder order in orders)
            {
                _mOrder = new Order();

                _mOrder.OrderGuid = order.OrderGuid.ToString();
                _mOrder.OrderId = order.OrderID;
                _mOrder.DailyOrderNo = order.DailyOrderNo;
                _mOrder.TimeStamp = order.TimeStamp;
                //_mOrder.OrderPrinter = order.tOrderPrinter.Name; 
                if(!string.IsNullOrEmpty(order.tCustomer.PhoneNo))
                    _mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo); 
                else
                    _mOrder.CustomerFullName = string.Format("{0} {1}", order.tCustomer.FirstName, order.tCustomer.LastName);

                _mOrder.TotalAmount = new DB.OrderService().GetTotal(order);

                //_mOrder.Receipt = new DB.OrderService().GetOrderReceipt(order);

                //DB.tPayment payment = new DB.PaymentRepository().GetByOrderGuid(order.OrderGuid);
                //_mOrder.PaymentId = payment == null ? Guid.Empty.ToString() : payment.PaymentGuid.ToString();

                _mOrder.SCMStatusId = Convert.ToInt32(scmStatusId);
                Models.Customer mCustomer = new Models.Customer();
                mCustomer.CustomerId = order.tCustomer.AdditionalCustomerNo;
                mCustomer.FirstName = order.tCustomer.FirstName;
                mCustomer.LastName = order.tCustomer.LastName;
                mCustomer.Email = order.tCustomer.Email;
                mCustomer.PhoneNo = order.tCustomer.PhoneNo;
                mCustomer.Address = order.tCustomer.Address == null ? string.Empty : order.tCustomer.Address;
                mCustomer.ZipCode = order.tCustomer.ZipCode == null ? string.Empty : order.tCustomer.ZipCode;
                mCustomer.City = order.tCustomer.City == null ? string.Empty : order.tCustomer.City;
                _mOrder.Customer = mCustomer;

                _mOrders.Add(_mOrder);
            }

            // Success
            _mOrder.Status = RestStatus.Success;
            _mOrder.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
        }

        public HttpResponseMessage Get(string token, string orderGuid, int scmStatusId)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(orderGuid) && !ML.Common.Text.IsNumeric(scmStatusId))
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

            // Get Order
            DB.OrderRepository orderRepository = new DB.OrderRepository();
            DB.tOrder order = orderRepository.GetByOrderGuid(new Guid(orderGuid));
            if (order == null)
            {
                _mOrder.Status = RestStatus.NotExisting;
                _mOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            // Ensure order belongs to company
            if(order.tCustomer.CompanyGuid != usertoken.CompanyGuid)
            {
                _mOrder.Status = RestStatus.NotExisting;
                _mOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            // Update SCM status
            try
            {
                order.OrderSCMStatusID = scmStatusId;
                if(orderRepository.Save() != DB.Repository.Status.Success)
                {
                    _mOrder.Status = RestStatus.GenericError;
                    _mOrder.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
                }
            }
            catch
            {
                _mOrder.Status = RestStatus.DataError;
                _mOrder.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            // Try Send CustomerOrder Picked email
            if (order.OrderSCMStatusID == (int)SCMStatus.Picked)
            {
                SendCustomerOrderPickedEmail(order);
            }

            // Populate
            _mOrder.OrderGuid = order.OrderGuid.ToString();
            _mOrder.OrderId = order.OrderID;
            _mOrder.DailyOrderNo = order.DailyOrderNo;
            _mOrder.TimeStamp = order.TimeStamp;
            //_mOrder.OrderPrinter = order.tOrderPrinter.Name;
            _mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
            _mOrder.TotalAmount = new DB.OrderService().GetTotal(order);

            //_mOrder.Receipt = new DB.OrderService().GetOrderReceipt(order);

            //DB.tPayment payment = new DB.PaymentRepository().GetByOrderGuid(order.OrderGuid);
            //_mOrder.PaymentId = payment == null ? Guid.Empty.ToString() : payment.PaymentGuid.ToString();

            _mOrder.SCMStatusId = order.OrderSCMStatusID;

            // Attach Customer
            Models.Customer mCustomer = new Models.Customer();
            mCustomer.CustomerId = order.tCustomer.AdditionalCustomerNo;
            mCustomer.FirstName = order.tCustomer.FirstName;
            mCustomer.LastName = order.tCustomer.LastName;
            mCustomer.Email = order.tCustomer.Email;
            mCustomer.PhoneNo = order.tCustomer.PhoneNo;
            mCustomer.Address = order.tCustomer.Address == null ? string.Empty : order.tCustomer.Address;
            mCustomer.ZipCode = order.tCustomer.ZipCode == null ? string.Empty : order.tCustomer.ZipCode;
            mCustomer.City = order.tCustomer.City == null ? string.Empty : order.tCustomer.City;
            _mOrder.Customer = mCustomer;

            _mOrder.SCMOrderList = new DB.OrderService().GetSCMOrderList(order);

            // Success
            _mOrder.Status = RestStatus.Success;
            _mOrder.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
        }

        public HttpResponseMessage Get(string token, string orderGuid)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(orderGuid) )
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

            // Get Order
            DB.OrderRepository orderRepository = new DB.OrderRepository();
            DB.tOrder order = orderRepository.GetByOrderGuid(new Guid(orderGuid));
            if (order == null)
            {
                _mOrder.Status = RestStatus.NotExisting;
                _mOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            // Ensure order belongs to company
            if (order.tCustomer.CompanyGuid != usertoken.CompanyGuid)
            {
                _mOrder.Status = RestStatus.NotExisting;
                _mOrder.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
            }

            // Populate
            _mOrder.OrderGuid = order.OrderGuid.ToString();
            _mOrder.OrderId = order.OrderID;
            _mOrder.DailyOrderNo = order.DailyOrderNo;
            _mOrder.TimeStamp = order.TimeStamp;
            //_mOrder.OrderPrinter = order.tOrderPrinter.Name;
            _mOrder.CustomerFullName = string.Format("{0} {1} ({2})", order.tCustomer.FirstName, order.tCustomer.LastName, order.tCustomer.PhoneNo);
            _mOrder.TotalAmount = new DB.OrderService().GetTotal(order);

            //_mOrder.Receipt = new DB.OrderService().GetOrderReceipt(order);

            //DB.tPayment payment = new DB.PaymentRepository().GetByOrderGuid(order.OrderGuid);
            //_mOrder.PaymentId = payment == null ? Guid.Empty.ToString() : payment.PaymentGuid.ToString();

            _mOrder.SCMStatusId = _mOrder.SCMStatusId;

            // Attach Customer
            Models.Customer mCustomer = new Models.Customer();
            mCustomer.CustomerId = order.tCustomer.AdditionalCustomerNo;
            mCustomer.FirstName = order.tCustomer.FirstName;
            mCustomer.LastName = order.tCustomer.LastName;
            mCustomer.Email = order.tCustomer.Email;
            mCustomer.PhoneNo = order.tCustomer.PhoneNo;
            mCustomer.Address = order.tCustomer.Address == null ? string.Empty : order.tCustomer.Address;
            mCustomer.ZipCode = order.tCustomer.ZipCode == null ? string.Empty : order.tCustomer.ZipCode;
            mCustomer.City = order.tCustomer.City == null ? string.Empty : order.tCustomer.City;
            _mOrder.Customer = mCustomer;

            _mOrder.SCMOrderList = new DB.OrderService().GetSCMOrderList(order);

            // Success
            _mOrder.Status = RestStatus.Success;
            _mOrder.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrders));
        }

        public void SendCustomerOrderPickedEmail(DB.tOrder order)
        {
            // Handle email
            DB.tShopConfig shopConfig = new DB.ShopConfigRepository().GetByCompanyGuid(order.tCustomer.CompanyGuid);
            if (shopConfig != null)
            {
                // Send customer order picked email
                if(shopConfig.OrderCustomerEmail)
                {
                    if(!string.IsNullOrEmpty(order.tCustomer.Email))
                    {
                        StringBuilder sbBody = new StringBuilder();
                        sbBody.Append(string.Format("Hej {0}!<br><br>", order.tCustomer.FirstName));
                        sbBody.Append("Din beställning är nu klar för leverans!<br><br>");
                        sbBody.Append("<div style='width:300px'>");
                        sbBody.Append(new DB.OrderService().GetOrderReceipt(order));
                        sbBody.Append("</div>");
                        sbBody.Append("<br><br>");
                        sbBody.Append(string.Format("Mvh {0}", order.tCustomer.tCompany.Name));

                        new ML.Email.Email().Send(
                            "noreply@ewopayments.se"
                            , string.Empty
                            , order.tCustomer.Email
                            , string.Format("{0} {1}", order.tCustomer.FirstName, order.tCustomer.LastName)
                            , string.Format("Beställning klar för leverans: {0} (Orderid:{1})", order.tCustomer.tCompany.Name, order.OrderID.ToString())
                            , sbBody.ToString()
                            , true
                            , System.Net.Mail.MailPriority.Normal
                            , false
                            );
                    }
                }
            }
        }





    }



}
