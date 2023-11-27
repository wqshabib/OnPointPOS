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
    [System.Web.Http.RoutePrefix("mapi/CashDrawer")]
    public class CashDrawerController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public CashDrawerController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpGet]
        [Route("GetCashDrawers")]
        public HttpResponseMessage GetCashDrawers([FromUri] DatesApi dates)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    List<CashDrawerApi> cashDrawers = new List<CashDrawerApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {                        
                        var dbCashDrawers = db.CashDrawer.Where(obj => obj.TerminalId == dates.TerminalId).ToList();
                        if (dbCashDrawers != null)
                            cashDrawers = dbCashDrawers.Select(obj => CashDrawerApi.ConvertModelToApiModel(obj)).ToList();
                    }

                    string json = JsonConvert.SerializeObject(cashDrawers);
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
