using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace POSSUM.StandAloneApi.Controllers
{
    [Authorize]
    [System.Web.Http.RoutePrefix("mapi/Payment")]
    public class PaymentController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public PaymentController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostPayments")]
        public HttpResponseMessage PostPayments(List<PaymentApi> payments)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                string objJson = JsonConvert.SerializeObject(payments);
                LogWriter.LogWrite("PostPayments: CallBack: Json: " + objJson);

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<PaymentApi> SuccessList = new List<PaymentApi>();
                    List<PaymentApi> ErrorList = new List<PaymentApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var payment in payments)
                        {
                            try
                            {
                                var dbOrder = db.OrderMaster.FirstOrDefault(obj => obj.Id == payment.OrderId);
                                if (dbOrder != null)
                                {
                                    var dbObject = db.Payment.FirstOrDefault(obj => obj.Id == payment.Id);
                                    if (dbObject == null)
                                    {
                                        dbObject = PaymentApi.ConvertApiModelToModel(payment);
                                        db.Payment.Add(dbObject);
                                    }
                                    else
                                    {
                                        dbObject = PaymentApi.UpdateModel(dbObject, payment);
                                        db.Entry(dbObject).State = EntityState.Modified;
                                    }

                                    LogWriter.LogWrite("PaymentController: PostPayments: OrderId: " + dbObject.OrderId + ", PaymentId: " + dbObject.Id);

                                    db.SaveChanges();

                                    LogWriter.LogWrite("PaymentController: PostPayments: Done: OrderId: " + dbObject.OrderId + ", PaymentId: " + dbObject.Id);

                                    SuccessList.Add(payment);
                                }
                                else
                                {
                                    LogWriter.LogWrite("PaymentController: PostPayments: Order Not Found: OrderId: " + payment.OrderId + ", PaymentId: " + payment.Id);
                                    ErrorList.Add(payment);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogWriter.LogWrite("PaymentController: PostPayments: Exception: OrderId: " + payment.OrderId + ", PaymentId: " + payment.Id);

                                try { responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString(); } catch (Exception e) { }
                                ErrorList.Add(payment);
                                LogWriter.LogWrite(ex);
                            }
                        }
                    }

                    responseApi.SuccessList = SuccessList;
                    responseApi.ErrorList = ErrorList;

                    string json = JsonConvert.SerializeObject(responseApi);
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    return httpResponseMessage;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                    return Request.CreateResponse(HttpStatusCode.BadGateway, ex.Message);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
