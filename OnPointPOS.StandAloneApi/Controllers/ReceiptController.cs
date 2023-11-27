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
    [System.Web.Http.RoutePrefix("mapi/Receipt")]
    public class ReceiptController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public ReceiptController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostReceipts")]
        public HttpResponseMessage PostReceipts(List<ReceiptApi> receipts)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<ReceiptApi> SuccessList = new List<ReceiptApi>();
                    List<ReceiptApi> ErrorList = new List<ReceiptApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var receipt in receipts)
                        {
                            try
                            {
                                var dbObject = db.Receipt.FirstOrDefault(obj => obj.ReceiptId == receipt.ReceiptId);
                                if (dbObject == null)
                                {
                                    dbObject = ReceiptApi.ConvertApiModelToModel(receipt);
                                    db.Receipt.Add(dbObject);
                                }
                                else
                                {
                                    dbObject = ReceiptApi.UpdateModel(dbObject, receipt);
                                    db.Entry(dbObject).State = EntityState.Modified;
                                }

                                db.SaveChanges();

                                SuccessList.Add(receipt);
                            }
                            catch(Exception ex)
                            {
                                responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                                ErrorList.Add(receipt);
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
