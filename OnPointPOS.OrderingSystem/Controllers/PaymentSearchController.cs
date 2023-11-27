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
    public class PaymentSearchController : ApiController
    {
        List<Models.Payment> _mPayments = new List<Models.Payment>();
        Models.Payment _mPayment = new Models.Payment();

        public PaymentSearchController()
        {
            _mPayments.Add(_mPayment);
        }

        public HttpResponseMessage Post(string token, int take)
        {
            return Post(token, Guid.Empty.ToString(), take);
        }

        /// <summary>
        /// Search payments
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderPrinterId"></param>
        /// <param name="take"></param>
        /// <returns></returns>
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
            string strSearch = string.IsNullOrEmpty(dic["Search"]) ? string.Empty : dic["Search"];


            // Search Payments
            IQueryable<DB.tPayment> payments = null;
            bool bFound = false;
            if (ML.Common.Text.IsNumeric(strSearch))
            {
                // Try TransactionNo
                payments = new DB.PaymentRepository().GetPaymentByTransactionNo(usertoken.CompanyGuid, strSearch);
                bFound = payments.Any();
            
                // Try PhoneNo
                if (!bFound)
                {
                    payments = new DB.PaymentRepository().GetPaymentsByPhoneNo(usertoken.CompanyGuid, guidOrderPrinterGuid, take, dtStartDateTime, dtEndDateTime, decStartAmount, decEndAmount, strSearch);
                    bFound = payments.Any();
                }
            }
            if (!bFound)
            {
                payments = new DB.PaymentRepository().SearchPayments(usertoken.CompanyGuid, guidOrderPrinterGuid, take, dtStartDateTime, dtEndDateTime, decStartAmount, decEndAmount, strSearch);
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
