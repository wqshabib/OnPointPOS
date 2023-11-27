using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using POSSUM.MasterData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace POSSUM.StandAloneApi.Controllers
{
    [System.Web.Http.RoutePrefix("mapi/Login")]
    public class LoginController : ApiController
    {
        public LoginController()
        {
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Index(LoginApi loginApi)
        {
            try
            {
                List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();

                data.Add(new KeyValuePair<string, string>("grant_type", loginApi.grant_type));
                data.Add(new KeyValuePair<string, string>("username", loginApi.username));
                data.Add(new KeyValuePair<string, string>("password", loginApi.password));

                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(data))
                    {
                        var baseUrl = ConfigurationManager.AppSettings["BaseUrl"]; //"https://api.possumsystem.com/standalone/"

                        content.Headers.Clear();
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        HttpResponseMessage response = httpClient.PostAsync(baseUrl + "miniToken", content).Result;
                        var message = response.Content.ReadAsStringAsync().Result;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            TokenApi tokenApi = JsonConvert.DeserializeObject<TokenApi>(message);
                            tokenApi.UserType = !string.IsNullOrEmpty(tokenApi.UserTypeString) ? Int32.Parse(tokenApi.UserTypeString) : 0;
                            
                            List<OutletApi> outlets = new List<OutletApi>();
                            var connectionString = "";

                            using (MasterDbContext mdb = new MasterDbContext())
                            {
                                var company = mdb.Company.Where(obj => obj.Id == tokenApi.CompanyId).FirstOrDefault();
                                if (company != null)
                                {
                                    tokenApi.CompanyName = string.IsNullOrEmpty(company.Name) ? "" : company.Name;
                                    connectionString = "Data Source=" + company.DBServer + ";Initial Catalog=" + company.DBName + ";UID=" + company.DBUser + ";Password=" + company.DBPassword + ";";
                                }

                                #if DEBUG
                                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                                #endif
                            }

                            if (!string.IsNullOrEmpty(connectionString))
                            {
                                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                                {
                                    var bindedTerminal = db.Terminal.Where(obj => !obj.IsDeleted && !string.IsNullOrEmpty(obj.HardwareAddress) && obj.HardwareAddress.ToString().Equals(loginApi.imei)).FirstOrDefault();
                                    if (bindedTerminal != null)
                                    {
                                        var dbOutlet = db.Outlet.Where(obj => !obj.IsDeleted && obj.Id == bindedTerminal.OutletId).FirstOrDefault();
                                        var dbCashDrawer = db.CashDrawer.Where(obj => obj.TerminalId == bindedTerminal.Id).FirstOrDefault();
                                        OutletApi outlet = OutletApi.ConvertModelToApiModel(dbOutlet);
                                        outlet.Terminals = new List<TerminalApi>();
                                        TerminalApi terminal = TerminalApi.ConvertModelToApiModel(bindedTerminal);
                                        if (dbCashDrawer != null)
                                            terminal.CashDrawer = CashDrawerApi.ConvertModelToApiModel(dbCashDrawer);
                                        outlet.Terminals.Add(terminal);
                                        outlets.Add(outlet);

                                        tokenApi.Status = 1;
                                    }
                                    else
                                    {
                                        var dbOutlets = db.Outlet.Where(obj => !obj.IsDeleted).ToList();
                                        foreach (var dbOutlet in dbOutlets)
                                        {
                                            OutletApi outlet = OutletApi.ConvertModelToApiModel(dbOutlet);
                                            outlet.Terminals = new List<TerminalApi>();
                                            var dbTerminals = db.Terminal.Where(obj => !obj.IsDeleted && obj.OutletId == dbOutlet.Id && string.IsNullOrEmpty(obj.HardwareAddress)).ToList();
                                            if (dbTerminals != null)
                                                outlet.Terminals = dbTerminals.Select(obj => TerminalApi.ConvertModelToApiModel(obj)).ToList();
                                            outlets.Add(outlet);
                                        }

                                        tokenApi.Status = 2;
                                    }
                                }
                            }

                            tokenApi.Outlets = outlets;

                            string json = JsonConvert.SerializeObject(tokenApi);
                            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                            return httpResponseMessage;
                        }
                        else
                        {
                            LogWriter.LogWrite("LoginController: Index: " + message);
                            return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.DeserializeObject<TokenApi>(message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return Request.CreateResponse(HttpStatusCode.BadGateway, ex.Message);
            }
        }
    }
}