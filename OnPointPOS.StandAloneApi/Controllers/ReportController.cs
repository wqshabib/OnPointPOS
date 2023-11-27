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
    [System.Web.Http.RoutePrefix("mapi/Report")]
    public class ReportController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public ReportController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostReports")]
        public HttpResponseMessage PostReports(List<ReportApi> reports)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<ReportApi> SuccessList = new List<ReportApi>();
                    List<ReportApi> ErrorList = new List<ReportApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var report in reports)
                        {
                            try
                            {
                                var dbObject = db.Report.FirstOrDefault(obj => obj.Id == report.Id);
                                if (dbObject == null)
                                {
                                    dbObject = ReportApi.ConvertApiModelToModel(report);
                                    db.Report.Add(dbObject);
                                }
                                else
                                {
                                    dbObject = ReportApi.UpdateModel(dbObject, report);
                                    db.Entry(dbObject).State = EntityState.Modified;
                                }

                                db.SaveChanges();

                                SuccessList.Add(report);
                            }
                            catch(Exception ex)
                            {
                                responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                                ErrorList.Add(report);
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
