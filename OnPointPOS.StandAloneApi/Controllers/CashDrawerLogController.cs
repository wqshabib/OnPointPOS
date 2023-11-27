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
    [System.Web.Http.RoutePrefix("mapi/CashDrawerLog")]
    public class CashDrawerLogController : BaseAPIController
    {
        private string connectionString = "";
        bool isAhenticated = false;

        public CashDrawerLogController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAhenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostCashDrawerLogs")]
        public HttpResponseMessage PostCashDrawerLogs(List<CashDrawerLogApi> cashDrawerLogs)
        {
            if (isAhenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<CashDrawerLogApi> SuccessList = new List<CashDrawerLogApi>();
                    List<CashDrawerLogApi> ErrorList = new List<CashDrawerLogApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var cashDrawerLog in cashDrawerLogs)
                        {
                            try
                            {
                                var dbObject = db.CashDrawerLog.FirstOrDefault(obj => obj.Id == cashDrawerLog.Id);
                                if (dbObject == null)
                                {
                                    dbObject = CashDrawerLogApi.ConvertApiModelToModel(cashDrawerLog);
                                    db.CashDrawerLog.Add(dbObject);
                                }
                                else
                                {
                                    dbObject = CashDrawerLogApi.UpdateModel(dbObject, cashDrawerLog);
                                    db.Entry(dbObject).State = EntityState.Modified;
                                }

                                db.SaveChanges();

                                SuccessList.Add(cashDrawerLog);
                            }
                            catch(Exception ex)
                            {
                                responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                                ErrorList.Add(cashDrawerLog);
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
