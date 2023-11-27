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
    public class PaymentController : ApiController
    {
        List<Models.Payment> _mPayments = new List<Models.Payment>();
        Models.Payment _mPayment = new Models.Payment();

        public PaymentController()
        {
            _mPayments.Add(_mPayment);
        }

        public HttpResponseMessage Post(string token, string orderPrinterId, int take)
        {
            if (!Request.Content.IsFormData())
            {
                _mPayment.Status = RestStatus.NotFormData;
                _mPayment.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(orderPrinterId) && take < 1)
            {
                _mPayment.Status = RestStatus.ParameterError;
                _mPayment.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mPayment.Status = RestStatus.AuthenticationFailed;
                _mPayment.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
            }

            // Prepare
            Guid guidOrderPrinterGuid = new Guid(orderPrinterId);
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            DateTime dtStartDateTime = string.IsNullOrEmpty(dic["StartDateTime"]) ? Convert.ToDateTime("2000-01-01 00:00:00") : Convert.ToDateTime(dic["StartDateTime"]);
            DateTime dtEndDateTime = string.IsNullOrEmpty(dic["EndDateTime"]) ? DateTime.Now : Convert.ToDateTime(dic["EndDateTime"]);
            decimal decStartAmount = string.IsNullOrEmpty(dic["StartAmount"]) ? 0 : Convert.ToDecimal(dic["StartAmount"]);
            decimal decEndAmount = string.IsNullOrEmpty(dic["EndAmount"]) ? 1000000000 : Convert.ToDecimal(dic["EndAmount"]);

            // Get Payments
            IQueryable<DB.tPayment> payments = new DB.PaymentRepository().GetPayments(usertoken.CompanyGuid, guidOrderPrinterGuid, take, dtStartDateTime, dtEndDateTime, decStartAmount, decEndAmount);

            // Populate
            _mPayments.Clear();
            foreach (DB.tPayment payment in payments)
            {
                _mPayment = new Models.Payment();
                _mPayment.PaymentId = payment.PaymentGuid.ToString();
                _mPayment.PaymentType = payment.tPaymentType.Name;
                _mPayment.CustomerId = payment.CustomerGuid.ToString();
                _mPayment.PhoneNo = payment.PhoneNo;
                _mPayment.Amount = payment.Amount;
                _mPayment.VAT = payment.VAT;
                _mPayment.OrderId = payment.OrderGuid.ToString();
                _mPayment.PaymentStatus = payment.tPaymentStatus.Description;
                _mPayment.TransactionNo = payment.TransactionNo;

                DB.tOrder order = new DB.OrderRepository().GetByOrderGuid(payment.OrderGuid);
                _mPayment.OrderPrinter = order == null ? string.Empty : order.tOrderPrinter.Name;

                _mPayments.Add(_mPayment);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
        }

        /// <summary>
        /// Get one or many payments
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// id = PaymentId or CustomerId
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mPayment.Status = RestStatus.ParameterError;
                _mPayment.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mPayment.Status = RestStatus.AuthenticationFailed;
                _mPayment.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
            }

            IQueryable<DB.tPayment> payments = new DB.PaymentRepository().GetPayment(new Guid(id));
            if (!payments.Any())
            {
                payments = new DB.PaymentRepository().GetByCustomerGuid(new Guid(id));

                if (payments == null)
                {
                    _mPayment.Status = RestStatus.NotExisting;
                    _mPayment.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
                }
            }

            // Populate
            _mPayments.Clear();
            foreach (DB.tPayment payment in payments)
            {
                _mPayment = new Models.Payment();
                _mPayment.PaymentId = payment.PaymentGuid.ToString();
                _mPayment.PaymentType = payment.tPaymentType.Name;
                _mPayment.CustomerId = payment.CustomerGuid.ToString();
                _mPayment.PhoneNo = payment.PhoneNo;
                _mPayment.Amount = payment.Amount;
                _mPayment.VAT = payment.VAT;
                _mPayment.OrderId = payment.OrderGuid.ToString();
                _mPayment.PaymentStatus = payment.tPaymentStatus.Description;
                _mPayment.TransactionNo = payment.TransactionNo;

                DB.tOrder order = new DB.OrderRepository().GetByOrderGuid(payment.OrderGuid);
                _mPayment.OrderPrinter = order == null ? string.Empty : order.tOrderPrinter.Name;

                _mPayments.Add(_mPayment);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPayments));
        }


    }
}
