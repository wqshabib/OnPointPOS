using iTextSharp.text.pdf.qrcode;
using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using POSSUM.MasterData;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using static POSSUM.Model.Terminal;

namespace POSSUM.StandAloneApi.Controllers
{
    [Authorize]
    [System.Web.Http.RoutePrefix("mapi/Terminal")]
    public class TerminalController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public TerminalController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostTerminal")]
        public HttpResponseMessage PostTerminal(TerminalApi terminalApi)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        var dbTerminals = db.Terminal.ToList();
                        var dbTerminal = dbTerminals.Where(obj => !obj.IsDeleted && obj.Id == terminalApi.Id && string.IsNullOrEmpty(obj.HardwareAddress)).FirstOrDefault();
                        var maxTerminalNo = dbTerminals.Max(obj => obj.TerminalNo);
                        var dbCashDrawer = db.CashDrawer.Where(obj => obj.TerminalId == terminalApi.Id).FirstOrDefault();
                        string CCUData = null;
                        bool isCreateNew = false;

                        if (dbTerminal == null)
                        {

                            string uniqueCode = GenerateUniqueIdentifier(dbTerminals);

                            dbTerminal = new Terminal();
                            dbTerminal.Id = terminalApi.Id;
                            dbTerminal.OutletId = terminalApi.OutletId;
                            dbTerminal.TerminalNo = maxTerminalNo + 1;
                            dbTerminal.TerminalType = Guid.Empty;
                            dbTerminal.UniqueIdentification = uniqueCode;
                            dbTerminal.HardwareAddress = terminalApi.HardwareAddress;
                            dbTerminal.Description = terminalApi.Description;
                            dbTerminal.Status = TerminalStatus.OPEN;
                            dbTerminal.RootCategoryId = 1;
                            dbTerminal.IsDeleted = false;
                            dbTerminal.CCUData = CCUData;
                            dbTerminal.AutoLogin = false;
                            dbTerminal.Created = DateTime.Now;
                            dbTerminal.Updated = DateTime.Now;

                            isCreateNew = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(dbTerminal.UniqueIdentification))
                            {
                                string uniqueCode = GenerateUniqueIdentifier(dbTerminals);
                                dbTerminal.UniqueIdentification = uniqueCode;
                            }

                            dbTerminal.Description = terminalApi.Description;
                            dbTerminal.HardwareAddress = terminalApi.HardwareAddress;
                            dbTerminal.CCUData = dbTerminal.CCUData;
                            dbTerminal.Updated = DateTime.Now;
                        }

                        terminalApi.UniqueIdentification = dbTerminal.UniqueIdentification;

                        if (string.IsNullOrEmpty(dbTerminal.CCUData))
                        {
                            var dbOutlet = db.Outlet.Where(obj => obj.Id == dbTerminal.OutletId).FirstOrDefault();

                            HttpResponseMessage message = GetCCUDataAsync(terminalApi, dbOutlet);

                            if (message == null)
                            {
                                LogWriter.LogWrite("TerminalControllermessage: CCUDataResponse: Error");
                                return Request.CreateResponse(HttpStatusCode.BadRequest);
                            }
                            else if (message.IsSuccessStatusCode)
                            {
                                CCUData = message.Content.ReadAsStringAsync().Result;

                                LogWriter.LogWrite("TerminalControllermessage: CCUDataResponse: " + CCUData);

                                dbTerminal.CCUData = StringCipher.encodeSTROnUrl(CCUData);

                                if (isCreateNew)
                                {
                                    db.Terminal.Add(dbTerminal);
                                    CreateTerminalInMasterDB(dbTerminal);
                                } 
                                else
                                {
                                    db.Entry(dbTerminal).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                var result = message.Content.ReadAsStringAsync().Result;

                                LogWriter.LogWrite("TerminalControllermessage: CCUDataResponse: " + result);

                                var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                                errorHttpResponseMessage.Content = new StringContent(result, Encoding.UTF8, "application/json");

                                return errorHttpResponseMessage;
                            }
                        }

                        TerminalApi terminal = TerminalApi.ConvertModelToApiModel(dbTerminal);

                        if (dbCashDrawer == null)
                        {
                            CashDrawer cashDrawer = new CashDrawer();
                            cashDrawer.Id = Guid.NewGuid();
                            cashDrawer.Name = terminalApi.Description;
                            cashDrawer.Location = terminalApi.Description + " " + terminalApi.TerminalNo;
                            cashDrawer.UserId = UserId;
                            cashDrawer.TerminalId = dbTerminal.Id;
                            cashDrawer.ConnectionString = null;

                            db.CashDrawer.Add(cashDrawer);

                            terminal.CashDrawer = CashDrawerApi.ConvertModelToApiModel(cashDrawer);
                        }
                        else
                        {
                            terminal.CashDrawer = CashDrawerApi.ConvertModelToApiModel(dbCashDrawer);
                        }

                        db.SaveChanges();

                        string json = JsonConvert.SerializeObject(terminal);
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        return httpResponseMessage;
                    }
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

        public void CreateTerminalInMasterDB(Terminal terminal)
        {
            try
            {
                using (MasterDbContext dbContext = new MasterDbContext())
                {
                    var dbObject = dbContext.Terminal.FirstOrDefault(t => t.Id == terminal.Id);

                    if (dbObject == null)
                    {
                        var _terminal = new MasterData.AdminTerminal
                        {
                            Id = terminal.Id,
                            Customer = CurrentDBName,
                            UniqueIdentification = terminal.UniqueIdentification
                        };

                        dbContext.Terminal.Add(_terminal);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
            }
        }

        public string GenerateUniqueIdentifier(List<Terminal> terminals)
        {
            string uniqueCode = "";
            bool done = true;

            while (done)
            {
                string tempUniqueCode = "SESUM-" + RandomString(3) + GenerateRandomNo();

                var exists = terminals.FirstOrDefault(t => t.UniqueIdentification == tempUniqueCode);

                if (exists == null)
                {
                    uniqueCode = tempUniqueCode;
                    done = false;
                }
            }

            return uniqueCode;
        }

        public int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public HttpResponseMessage GetCCUDataAsync(TerminalApi terminalApi, Outlet dbOutlet)
        {
            try
            {
                DateTime utcDateTime = DateTime.UtcNow;
                DateTime utcAddedYearsDateTime = utcDateTime.AddYears(1);
                DateTime tempDateTime = new DateTime(1970, 1, 1);

                DateTime validFromDateOnly = utcDateTime.Date;
                DateTime validToDateOnly = utcAddedYearsDateTime.Date;

                long ValidFrom = validFromDateOnly.Subtract(tempDateTime).Ticks / TimeSpan.TicksPerMillisecond;
                long ValidTo = validToDateOnly.Subtract(tempDateTime).Ticks / TimeSpan.TicksPerMillisecond;

                string[] splitTerminalId = terminalApi.Id.ToString().Split('-');
                string cashRegisterName = splitTerminalId[4];

                if (!string.IsNullOrEmpty(terminalApi.UniqueIdentification))
                {
                    string[] uniqueIdentificationArray = terminalApi.UniqueIdentification.ToString().Split('-');
                    cashRegisterName = uniqueIdentificationArray[0] + uniqueIdentificationArray[1];
                }

                ContactInfo ContactInfo = new ContactInfo();
                ContactInfo.name = Company.Name;
                ContactInfo.phone = "";
                ContactInfo.email = "info@ewo.se";

                ControlUnitGeolocation ControlUnitGeolocation = new ControlUnitGeolocation();
                ControlUnitGeolocation.address = Company.Address;
                ControlUnitGeolocation.city = Company.City;
                ControlUnitGeolocation.postalCode = Company.PostalCode;
                ControlUnitGeolocation.companyName = Company.Name;

                RegistrationGeolocation RegistrationGeolocation = new RegistrationGeolocation();
                RegistrationGeolocation.address = Company.Address;
                RegistrationGeolocation.city = Company.City;
                RegistrationGeolocation.postalCode = Company.PostalCode;
                RegistrationGeolocation.companyName = Company.Name;

                InstallationCreationInfo InstallationCreationInfo = new InstallationCreationInfo();
                InstallationCreationInfo.deviceId = splitTerminalId[4];
                InstallationCreationInfo.buildInfo = new BuildInfo();

                CCUDataRequest CCUDataRequest = new CCUDataRequest();
                CCUDataRequest.country = ConfigurationManager.AppSettings["Country"];
                CCUDataRequest.corporateId = dbOutlet.OrgNo;
                CCUDataRequest.cashRegisterName = cashRegisterName;
                CCUDataRequest.validFrom = ValidFrom;
                CCUDataRequest.validTo = ValidTo;
                CCUDataRequest.features = new string[] { "CONTROL_UNIT", "SKATTERVERKET" };
                CCUDataRequest.comment = "";
                CCUDataRequest.contactInfo = ContactInfo;
                CCUDataRequest.controlUnitSerial = ConfigurationManager.AppSettings["ControlUnitSerial"];
                CCUDataRequest.controlUnitLocation = ConfigurationManager.AppSettings["ControlUnitLocation"];
                CCUDataRequest.controlUnitGeolocation = ControlUnitGeolocation;
                CCUDataRequest.registrationGeolocation = RegistrationGeolocation;
                CCUDataRequest.applicationPackage = ConfigurationManager.AppSettings["ApplicationPackage"];
                CCUDataRequest.productionNumber = null;
                CCUDataRequest.installationCreationInfo = InstallationCreationInfo;
                CCUDataRequest.connectionCode = null;
                CCUDataRequest.applicationNameAndVersion = terminalApi.ApplicationNameAndVersion;

                using (var httpClient = new HttpClient())
                {
                    var srv4posUrl = ConfigurationManager.AppSettings["Serv4PosUrl"];

                    var basicAuthUserNamePssword = string.Format("{0}:{1}", ConfigurationManager.AppSettings["Serv4PosUserName"], ConfigurationManager.AppSettings["Serv4PosPassword"]);
                    var encodedString = Convert.ToBase64String(Encoding.ASCII.GetBytes(basicAuthUserNamePssword));

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedString);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    string json = JsonConvert.SerializeObject(CCUDataRequest);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    return httpClient.PostAsync(srv4posUrl, stringContent).Result;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return null;
            }
        }
    }
}
