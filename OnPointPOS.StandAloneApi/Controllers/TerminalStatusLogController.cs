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
    [System.Web.Http.RoutePrefix("mapi/TerminalStatusLog")]
    public class TerminalStatusLogController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public TerminalStatusLogController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostTerminalStatusLog")]
        public HttpResponseMessage PostTerminalStatusLog(List<TerminalStatusLogApi> terminalStatusLogs)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<TerminalStatusLogApi> SuccessList = new List<TerminalStatusLogApi>();
                    List<TerminalStatusLogApi> ErrorList = new List<TerminalStatusLogApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var terminalStatusLog in terminalStatusLogs)
                        {
                            try
                            {
                                var dbObject = db.TerminalStatusLog.FirstOrDefault(obj => obj.Id == terminalStatusLog.Id);
                                if (dbObject == null)
                                {
                                    dbObject = TerminalStatusLogApi.ConvertApiModelToModel(terminalStatusLog);
                                    db.TerminalStatusLog.Add(dbObject);
                                }
                                else
                                {
                                    dbObject = TerminalStatusLogApi.UpdateModel(dbObject, terminalStatusLog);
                                    db.Entry(dbObject).State = EntityState.Modified;
                                }

                                db.SaveChanges();

                                SuccessList.Add(terminalStatusLog);
                            }
                            catch (Exception ex)
                            {
                                responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                                ErrorList.Add(terminalStatusLog);
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
