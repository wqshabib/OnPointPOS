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
    [System.Web.Http.RoutePrefix("mapi/PaymentType")]
    public class PaymentTypeController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public PaymentTypeController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpGet]
        [Route("GetPaymentTypes")]
        public HttpResponseMessage GetPaymentTypes([FromUri] DatesApi dates)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    DateTime LAST_EXECUTED_DATETIME = dates.From.AddMinutes(-5);
                    DateTime EXECUTED_DATETIME = dates.To;

                    List<PaymentTypeApi> paymentTypes = new List<PaymentTypeApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {                        
                        var dbPaymentTypes = db.PaymentType.Where(obj => obj.Updated > LAST_EXECUTED_DATETIME && obj.Updated <= EXECUTED_DATETIME).ToList();
                        if (dbPaymentTypes != null)
                            paymentTypes = dbPaymentTypes.Select(obj => PaymentTypeApi.ConvertModelToApiModel(obj)).ToList();
                    }

                    string json = JsonConvert.SerializeObject(paymentTypes);
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
