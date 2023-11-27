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

namespace ML.Rest2.Controllers
{
    public class CustomerInvoiceController : ApiController
    {   
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>        
        /// <returns></returns>
        public HttpResponseMessage Put(string secret, string companyId, string id)
        {
            var mInvoices = new List<Invoice>();
            var mInvoice = new Invoice();
            mInvoices.Add(mInvoice);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId))
            {
                mInvoice.Status = RestStatus.ParameterError;
                mInvoice.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                mInvoice.Status = RestStatus.AuthenticationFailed;
                mInvoice.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
            }
            Guid invoiceId;
            Guid.TryParse(id, out invoiceId);
            var invoiceRep = new DB.InvoiceRepository();
            var invoice = invoiceRep.GetInvoice(invoiceId);
            if (invoice == null || invoice.InvoiceGuid == Guid.Empty)
            {
                mInvoice.Status = RestStatus.NotExisting;
                mInvoice.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
            }
            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            if (!string.IsNullOrEmpty(dic["InvoiceTitle"]))
                invoice.Title = dic["InvoiceTitle"];
            if (!string.IsNullOrEmpty(dic["InvoiceText"]))
                invoice.Text = dic["InvoiceText"];
            if (!string.IsNullOrEmpty(dic["EmailText"]))
                invoice.EmailText = dic["EmailText"];
            if (!string.IsNullOrEmpty(dic["EmailSubject"]))
                invoice.EmailSubject = dic["EmailSubject"];
            if (!string.IsNullOrEmpty(dic["Reference"]))
                invoice.Reference = dic["Reference"];
            if (!string.IsNullOrEmpty(dic["ReminderFee"]))
            {
                decimal result;
                decimal.TryParse(dic["ReminderFee"], out result);
                invoice.ReminderFee = result;
            }
            if (invoice.tOrder != null)
            {
                if (!string.IsNullOrEmpty(dic["OrderText"]))
                    invoice.tOrder.Text = dic["OrderText"];
                if (invoice.tOrder.tOrderItem.Count == 1)
                {
                    var item = invoice.tOrder.tOrderItem.FirstOrDefault();
                    if (!string.IsNullOrEmpty(dic["Price"]))
                    {
                        decimal result;
                        decimal.TryParse(dic["Price"], out result);
                        invoice.tOrder.tOrderItem.FirstOrDefault().Price = result;
                    }
                    if (!string.IsNullOrEmpty(dic["ContentText"]))
                        invoice.tOrder.tOrderItem.FirstOrDefault().tContent.Name= dic["ContentText"];
                }
                if (invoice.tOrder.tCustomer != null)
                {
                    if (!string.IsNullOrEmpty(dic["FirstName"]))
                        invoice.tOrder.tCustomer.FirstName = dic["FirstName"];
                    if (!string.IsNullOrEmpty(dic["LastName"]))
                        invoice.tOrder.tCustomer.LastName = dic["LastName"];
                    if (!string.IsNullOrEmpty(dic["Address"]))
                        invoice.tOrder.tCustomer.Address = dic["Address"]; ;
                    if (!string.IsNullOrEmpty(dic["ZipCode"]))
                        invoice.tOrder.tCustomer.ZipCode = dic["ZipCode"]; ;
                    if (!string.IsNullOrEmpty(dic["City"]))
                        invoice.tOrder.tCustomer.City = dic["City"]; ;
                    if (invoice.tOrder.tCustomer.tCompany != null && !string.IsNullOrEmpty(dic["CompanyName"]))
                        invoice.tOrder.tCustomer.tCompany.Name= dic["CompanyName"];
                }
            }

            if (invoiceRep.Save() != DB.Repository.Status.Success)
            {
                mInvoice.Status = RestStatus.DataError;
                mInvoice.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
            }
            //RETURN UPDATED DATA
            mInvoice = ReturnInvoiceData(invoice);
            mInvoice.Status = RestStatus.Success;
            mInvoice.StatusText = "Success";
            mInvoices.Clear();
            mInvoices.Add(mInvoice);
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
        }

        public HttpResponseMessage Get(string secret, string companyId, string id)
        {
            var mInvoices = new List<Invoice>();
            var mInvoice = new Invoice();
            mInvoices.Add(mInvoice);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId))
            {
                mInvoice.Status = RestStatus.ParameterError;
                mInvoice.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                mInvoice.Status = RestStatus.AuthenticationFailed;
                mInvoice.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
            }
            Guid invoiceId;
            Guid.TryParse(id, out invoiceId);

            var invoice = new DB.InvoiceRepository().GetInvoice(invoiceId);
            if (invoice==null || invoice.InvoiceGuid == Guid.Empty)
            {
                mInvoice.Status = RestStatus.NotExisting;
                mInvoice.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
            }

            // Populate
            mInvoices.Clear();

            mInvoice = ReturnInvoiceData(invoice);
            mInvoices.Add(mInvoice);
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mInvoices));
        }

        Invoice ReturnInvoiceData(DB.tInvoice invoice)
        {
            var mInvoice = new Invoice()
            {
                InvoiceGuid = invoice.InvoiceGuid,
                EmailSubject = invoice.EmailSubject,
                EmailText = invoice.EmailText,
                OrderGuid = invoice.OrderGuid,
                Reference = invoice.Reference,
                ReminderFee = invoice.ReminderFee,
                InvoiceText = invoice.Text,
                InvoiceTitle = invoice.Title,
                Created = invoice.TimeStamp
            };
            if (invoice.tOcr.Count() > 0)
            {
                mInvoice.Ocr = invoice.tOcr.FirstOrDefault().Ocr;
            }
            if (invoice.Reminder)
            {
                if (invoice.Retries > 0)
                {
                    mInvoice.LastReminder = invoice.LastRetryDateTime;
                }
                if ( invoice.Retries < invoice.MaxRetries)
                {
                    mInvoice.NextReminder = invoice.LastRetryDateTime.AddDays(invoice.Interval);
                }
            }
            
            var payment = invoice.tOrder.tPayment.FirstOrDefault();
            if (payment != null)
            {
                var type = (ML.Payment.PaymentType.Type)Enum.Parse(typeof(ML.Payment.PaymentType.Type), payment.PaymentTypeID.ToString());
                mInvoice.PaymentMethod = type.ToString().ToLower();
            }

            if (invoice.tOrder != null)
            {
                mInvoice.OrderText = invoice.tOrder.Text;
                if (invoice.tOrder.tOrderItem.Count == 1)
                {
                    var item = invoice.tOrder.tOrderItem.FirstOrDefault();
                    mInvoice.ContentText = item.tContent.Name;
                    mInvoice.Price = item.Price;
                }
                if (invoice.tOrder.tCustomer != null)
                {
                    mInvoice.FirstName = invoice.tOrder.tCustomer.FirstName;
                    mInvoice.LastName = invoice.tOrder.tCustomer.LastName;
                    mInvoice.Address = invoice.tOrder.tCustomer.Address;
                    mInvoice.ZipCode = invoice.tOrder.tCustomer.ZipCode;
                    mInvoice.City = invoice.tOrder.tCustomer.City;
                    if (invoice.tOrder.tCustomer.tCompany != null)
                        mInvoice.CompanyName = invoice.tOrder.tCustomer.tCompany.Name;
                }
            }
            return mInvoice;
        }
    }
}