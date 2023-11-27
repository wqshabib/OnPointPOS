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
    [System.Web.Http.RoutePrefix("mapi/Journal")]
    public class JournalController : BaseAPIController
    {
        private string connectionString = "";
        bool isAhenticated = false;

        public JournalController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAhenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostJournals")]
        public HttpResponseMessage PostJournals(List<JournalApi> journals)
        {
            if (isAhenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                string objJson = JsonConvert.SerializeObject(journals);
                LogWriter.LogWrite("JournalController: PostJournals: CallBack: Json: " + objJson);

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<JournalApi> SuccessList = new List<JournalApi>();
                    List<JournalApi> ErrorList = new List<JournalApi>();

                    ApplicationDbContext db = new ApplicationDbContext(connectionString);

                    foreach (var journal in journals)
                    {
                        try
                        {
                            var dbObject = JournalApi.ConvertApiModelToModel(journal);
                            db.Journal.Add(dbObject);

                            db.SaveChanges();

                            SuccessList.Add(journal);
                        }
                        catch (Exception ex)
                        {
                            responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                            ErrorList.Add(journal);
                            LogWriter.LogWrite(ex);

                            try { db = new ApplicationDbContext(connectionString); } catch (Exception e) { }
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
