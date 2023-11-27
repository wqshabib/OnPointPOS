using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using POSSUM.Model;
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
    [System.Web.Http.RoutePrefix("mapi/Setting")]
    public class SettingController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public SettingController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpGet]
        [Route("GetSettings")]
        public HttpResponseMessage GetSettings([FromUri] DatesApi dates)
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

                    List<SettingApi> settings = new List<SettingApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        var dbSettings = db.Setting.Where(obj => obj.Updated > LAST_EXECUTED_DATETIME && obj.Updated <= EXECUTED_DATETIME && 
                                                    ((obj.OutletId == dates.OutletId && obj.TerminalId == Guid.Empty) || 
                                                    (obj.TerminalId == dates.TerminalId)))
                                                    .ToList();

                        if (dbSettings != null)
                            settings = dbSettings.Select(obj => SettingApi.ConvertModelToApiModel(obj)).ToList();

                        var dbPrinterLogoSetting = db.Setting.FirstOrDefault(obj => obj.Updated > LAST_EXECUTED_DATETIME && obj.Updated <= EXECUTED_DATETIME &&
                                                    obj.Code == SettingCode.PrinterLogo);

                        if (dbPrinterLogoSetting != null)
                            settings.Add(SettingApi.ConvertModelToApiModel(dbPrinterLogoSetting));
                    }

                    string json = JsonConvert.SerializeObject(settings);
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

        [System.Web.Http.HttpPost]
        [Route("PostSettings")]
        public HttpResponseMessage PostSettings(List<SettingApi> settings)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<SettingApi> SuccessList = new List<SettingApi>();
                    List<SettingApi> ErrorList = new List<SettingApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var setting in settings)
                        {
                            try
                            {
                                var dbObject = db.Setting.FirstOrDefault(obj => obj.Code == setting.Code && obj.OutletId == setting.OutletId && obj.TerminalId == setting.TerminalId);
                                if (dbObject == null)
                                {
                                    dbObject = SettingApi.ConvertApiModelToModel(setting);
                                    db.Setting.Add(dbObject);
                                }
                                else
                                {
                                    dbObject = SettingApi.UpdateModel(dbObject, setting);
                                    db.Entry(dbObject).State = EntityState.Modified;
                                }

                                db.SaveChanges();

                                SuccessList.Add(setting);
                            }
                            catch (Exception ex)
                            {
                                responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                                ErrorList.Add(setting);
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
