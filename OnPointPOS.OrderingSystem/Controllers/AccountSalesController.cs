using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class AccountSalesController : ApiController
    {
        public HttpResponseMessage Get(string token)
        {
            return Get(token, Guid.Empty.ToString());
        }

        public HttpResponseMessage Get(string token, string id)
        {
            List<dynamic> mAccountSalesList = new List<dynamic>();
            dynamic mAccountSales = new ExpandoObject();
            mAccountSalesList.Add(mAccountSales);

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(id))
            {
                mAccountSales.Status = RestStatus.ParameterError;
                mAccountSales.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mAccountSalesList));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                mAccountSales.Status = RestStatus.AuthenticationFailed;
                mAccountSales.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mAccountSalesList));
            }

            // Retrieve list of available AccountSales
            IQueryable<DB.tPayout> payouts = new DB.PayoutRepository().GetPayouts(usertoken.CompanyGuid);

            // Prepare
            Guid guidId = new Guid(id);
            List<Payout> customerPayouts = new List<Payout>();
            // Check if OrderPrinter exists
            if (guidId != Guid.Empty)
            {
                IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetManyByOrderPrinterGuid(guidId);
                if (!orderPrinters.Any())
                {
                    orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidId);
                }
                if (orderPrinters.Any())
                {
                    // Extract relevent periods based on Printer for a Company
                    foreach (DB.tPayout payout in payouts)
                    {
                        foreach (DB.tPayoutDetail payoutDetail in payout.tPayoutDetail.Where(i=>i.tPayment.CompanyGuid==usertoken.CompanyGuid))
                        {
                            if (orderPrinters.FirstOrDefault().OrderPrinterGuid != Guid.Empty)
                            {
                                DB.tOrder order = new DB.OrderRepository().GetByOrderGuid(payoutDetail.tPayment.OrderGuid);
                                if (order != null)
                                {
                                    if (order.OrderPrinterGuid == orderPrinters.FirstOrDefault().OrderPrinterGuid)
                                    {
                                        customerPayouts.Add(new Payout() { PayoutGuid= payout.PayoutGuid, Title= payout.Title, PayoutTypeID=payout.PayoutTypeID, PeriodStartDateTime=payout.PeriodStartDateTime, PeriodEndDateTime=payout.PeriodEndDateTime});
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Extract relevent periods based on Company
                foreach (DB.tPayout payout in payouts)
                {
                    //foreach (DB.tPayoutDetail payoutDetail in payout.tPayoutDetail.Where(i=>i.tPayment.CompanyGuid == usertoken.CompanyGuid))
                    {
                       customerPayouts.Add(new Payout() { PayoutGuid = payout.PayoutGuid, Title = payout.Title, PayoutTypeID = payout.PayoutTypeID, PeriodStartDateTime = payout.PeriodStartDateTime, PeriodEndDateTime = payout.PeriodEndDateTime });
                        //break;
                    }
                }
            }

            if(customerPayouts.Count == 0)
            {
                mAccountSales.Status = RestStatus.NotExisting;
                mAccountSales.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mAccountSalesList));
            }

            mAccountSales.Payouts = customerPayouts.OrderByDescending(i=>i.PeriodStartDateTime);

            // Success
            mAccountSales.Status = RestStatus.Success;
            mAccountSales.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mAccountSalesList));
        }

        public HttpResponseMessage Get(string token, string payoutGuid, string orderPrinterGuid)
        {
            if (string.IsNullOrEmpty(token) && ML.Common.Text.IsGuid(payoutGuid)  && ML.Common.Text.IsGuid(orderPrinterGuid))
            {
                //mAccountSales.Status = RestStatus.ParameterError;
                //mAccountSales.StatusText = "Parameter Error";
                //return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mAccountSalesList));
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);

            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                //mAccountSales.Status = RestStatus.AuthenticationFailed;
                //mAccountSales.StatusText = "Authentication Failed";
                //return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mAccountSalesList));
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            
            DB.tPayout payout = new DB.PayoutRepository().GetPayout(Guid.Parse(payoutGuid));
            if(payout == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            // Try lookup printer
            Guid guidOrderPrinterGuid = Guid.Empty;
            IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetManyByOrderPrinterGuid(new Guid(orderPrinterGuid));
            if (!orderPrinters.Any())
            {
                orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(new Guid(orderPrinterGuid));
            }

            if (orderPrinters.Any())
            {
                guidOrderPrinterGuid = orderPrinters.FirstOrDefault().OrderPrinterGuid;
            }

            // Get AccountSales
            MemoryStream ms = new DB.PayoutService().CreateAccountSales(usertoken.CompanyGuid, payout.PayoutGuid, DB.PayoutService.PayoutType.Customer, guidOrderPrinterGuid);

            string strCompanyName = new DB.CompanyRepository().GetByCompanyGuid(usertoken.CompanyGuid).Name;

            string strFileName = string.Empty;
            if (guidOrderPrinterGuid != Guid.Empty)
            {
                DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByOrderPrinterGuid(guidOrderPrinterGuid);
                if (orderPrinter == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }
                strFileName = string.Format("Avräkningsnota_{0}_{1}.pdf", orderPrinter.Name.Replace(" ", "_"), ML.Common.Text.DateTimeToFileFormat(payout.PeriodStartDateTime));
            }
            else
            {
                strFileName = string.Format("Avräkningsnota_{0}_{1}.pdf", strCompanyName.Replace(" ", "_"), ML.Common.Text.DateTimeToFileFormat(payout.PeriodStartDateTime));
            }


            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent);

            if (ms.Length > 0)
            {
                // Prepare Pdf for browser
                //Response.Clear();
                //Response.AddHeader("Content-Type", "application/pdf");
                //Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", strFileName));

                //Byte[] byteArray = ms.ToArray();
                //Response.BinaryWrite(byteArray);

                //try
                //{
                //    Response.End();
                //}
                //catch { }

                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                httpResponseMessage.StatusCode = HttpStatusCode.OK;

                //httpResponseMessage.Content = new StreamContent(ms);
                byte[] fileBytes = ms.ToArray();
                httpResponseMessage.Content = new ByteArrayContent(fileBytes);

                //httpResponseMessage.Content.Headers.ContentType.MediaType = "application/pdf";
                //httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                //httpResponseMessage.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", strFileName));
                //httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(string.Format("attachment; filename={0}", strFileName));
                //httpResponseMessage.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", strFileName));
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = strFileName;

                //byte[] fileBytes = System.IO.File.ReadAllBytes(pdfLocation);
                //response.Content = new ByteArrayContent(fileBytes);
                //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                //response.Content.Headers.ContentDisposition.FileName = fileName;
                //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }

            return httpResponseMessage;
        }


        
    }
}
