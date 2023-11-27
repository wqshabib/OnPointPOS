using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using POSSUM.Data;
using System.Diagnostics;
using POSSUM.Api.Models;
using POSSUM.ApiModel;
using Newtonsoft.Json;
using System.Text;

namespace POSSUM.Api.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    [System.Web.Http.RoutePrefix("api/Setting")]
    public class SettingController : BaseAPIController
    {

        string connectionString = "";
        bool nonAhenticated = false;

        public SettingController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
		[HttpPost]
        [Route("PostSetting")]
        public IHttpActionResult PostSetting(Setting setting)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    var _setting = db.Setting.FirstOrDefault(u => u.Id == setting.Id);
                    if (_setting == null)
                    {
                        db.Setting.Add(setting);
                    }
                    else
                    {
                        _setting.Code = setting.Code;
                        _setting.Description = setting.Description;
                        _setting.OutletId = setting.OutletId;
                        _setting.Sort = setting.Sort;
                        _setting.TerminalId = setting.TerminalId;
                        _setting.Value = setting.Value;
                        _setting.Updated = setting.Updated;
                        _setting.Type = setting.Type;
                    }

                    db.SaveChanges();
                }

                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("GetInvoiceCounters")]
        public async Task<List<InvoiceCounter>> GetInvoiceCounters()
        {
            using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
            {
                return db.InvoiceCounter.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostInvoiceCounter")]
        public IHttpActionResult PostInvoiceCounter(InvoiceCounter model)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var lastNo = Convert.ToInt64(model.LastNo);
                    var obj = db.InvoiceCounter.FirstOrDefault(a => a.InvoiceType == model.InvoiceType);
                    if (obj!=null && Convert.ToInt64(obj.LastNo) < lastNo)
                    {
                        obj.LastNo = Convert.ToString(lastNo);
                        db.SaveChanges();
                    }
                }

                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }


        /// <summary>
        /// Get general settings for NIMPOS
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        [Route("GetSettings")]
        public async Task<SettingData> GetSettings([FromUri] Dates dates)
        {
            DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
            DateTime EXECUTED_DATETIME = dates.CurrentDate;
            Guid terminalId = dates.TerminalId;

            try
            {
                SettingData settingData = new SettingData();


                List<TempSetting> liveSettings = new List<TempSetting>();
                List<TempPrinter> livePrinters = new List<TempPrinter>();
                List<TempCampaign> liveCampaigns = new List<TempCampaign>();
                List<ZReportSetting> liveZReportSetting = new List<ZReportSetting>();
                List<Client> liveClients = new List<Client>();
                Guid outletId = default(Guid);
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {

                    //var liveTerminalRepo = new Repository<Terminal, Guid>(uof.Session);
                    //var outletRepo = new Repository<Outlet, Guid>(uof.Session);
                    //var liveSettingRepo = new Repository<Setting, int>(uof.Session);
                    //var livecategoryRepo = new Repository<Category, int>(uof.Session);
                    //var cashDrawerRepo = new Repository<CashDrawer, Guid>(uof.Session);
                    var clientterminal = db.Terminal.FirstOrDefault(i => i.Id == terminalId);
                    if (clientterminal != null)
                    {
                        outletId = clientterminal.OutletId;

                        var outlet = db.Outlet.FirstOrDefault(ol => ol.Id == outletId && ol.Updated > LAST_EXECUTED_DATETIME && ol.Updated <= EXECUTED_DATETIME);
                        if (outlet != null)
                        {
                            var _Outlet = new TempOutlet
                            {
                                Id = outlet.Id,
                                Address1 = outlet.Address1,
                                Address2 = outlet.Address2,
                                Address3 = outlet.Address3,
                                BillPrinterId = outlet.BillPrinterId,
                                City = outlet.City,
                                Email = outlet.Email,
                                FooterText = outlet.FooterText,
                                HeaderText = outlet.HeaderText,
                                IsDeleted = outlet.IsDeleted,
                                Name = outlet.Name,
                                KitchenPrinterId = outlet.KitchenPrinterId,
                                OrgNo = outlet.OrgNo,
                                Phone = outlet.Phone,
                                PostalCode = outlet.PostalCode,
                                TaxDescription = outlet.TaxDescription,
                                WebUrl = outlet.WebUrl,
                                Created = outlet.Created,
                                Updated = outlet.Updated,
                                WarehouseID = outlet.WarehouseID
                            };
                            settingData.Outlet = _Outlet;

                        }
                    }
                    var t = db.Terminal.Where(tr => tr.Id == terminalId && tr.Updated > LAST_EXECUTED_DATETIME && tr.Updated <= EXECUTED_DATETIME).FirstOrDefault();//.Select(t => new Terminal
                    if (t != null)
                    {

                        var drawers = db.CashDrawer.Where(cs => cs.TerminalId == terminalId).Select(c => new TempCashDrawer
                        {
                            Id = c.Id,
                            ConnectionString = c.ConnectionString,
                            Location = c.Location,
                            Name = c.Name,
                            TerminalId = c.TerminalId,
                            UserId = c.UserId,


                        }).ToList();
                        var category = db.Category.FirstOrDefault(c => c.Id == t.Category.Id);
                        var terminal = new TempTerminal
                        {

                            Id = t.Id,
                            CashDrawer = drawers,
                            CategoryId = t.Category.Id,
                            Description = t.Description,
                            HardwareAddress = t.HardwareAddress,
                            IsDeleted = t.IsDeleted,
                            OutletId = t.Outlet.Id,
                            Status = POSSUM.Model.Terminal.TerminalStatus.CLOSED,
                            TerminalNo = t.TerminalNo,
                            TerminalType = t.TerminalType,
                            UniqueIdentification = t.UniqueIdentification,
                            Created = t.Created,
                            Updated = t.Updated

                        };

                        settingData.Terminal = terminal;
                    }




                    livePrinters = db.Printer.Where(s => s.Updated > LAST_EXECUTED_DATETIME && s.Updated <= EXECUTED_DATETIME).Select(s => new TempPrinter
                    {
                        Id = s.Id,
                        LocationName = s.LocationName,
                        PrinterName = s.PrinterName,
                        Updated = s.Updated

                    }).ToList();
                    liveCampaigns = db.Campaign.Where(s => s.Updated > LAST_EXECUTED_DATETIME && s.Updated <= EXECUTED_DATETIME).Select(s => new TempCampaign
                    {
                        Id = s.Id,
                        BuyLimit = s.BuyLimit,
                        FreeOffer = s.FreeOffer,
                        Description = s.Description,
                        Updated = s.Updated

                    }).ToList();
                    liveSettings = db.Setting.Where(s => s.TerminalId == terminalId && (s.Updated > LAST_EXECUTED_DATETIME && s.Updated <= EXECUTED_DATETIME)).Select(s => new TempSetting
                    {
                        Id = s.Id,
                        Created = s.Created,
                        Description = s.Description,
                        OutletId = s.OutletId,
                        Sort = s.Sort,
                        TerminalId = s.TerminalId,
                        Type = s.Type,
                        Updated = s.Updated,
                        Value = s.Value,
                        Code = s.Code
                    }).ToList();
                    //  liveSettings = db.Database.SqlQuery<TempSetting>("select *from Setting Where TerminalId='" + terminalId + "' AND Updated between '" + LAST_EXECUTED_DATETIME + "' AND '" + EXECUTED_DATETIME + "'").ToList();//
                    liveZReportSetting = db.ZReportSetting.Where(tr => tr.Updated > LAST_EXECUTED_DATETIME && tr.Updated <= EXECUTED_DATETIME).ToList();
                    liveClients = db.Client.Where(tr => tr.Updated > LAST_EXECUTED_DATETIME && tr.Updated <= EXECUTED_DATETIME).ToList();

                }


                settingData.Settings = liveSettings;
                settingData.Printers = livePrinters;
                settingData.Campaigns = liveCampaigns;
                settingData.ZReportSettings = liveZReportSetting;
                settingData.Clients = liveClients;
                //if (outletId != default(Guid))
                //{
                //	MasterData.MasterDbContext _db = new MasterData.MasterDbContext();
                //	var messages = _db.CustomerInbox.Where(m => m.OutletId == outletId && (m.CreatedOn > LAST_EXECUTED_DATETIME && m.CreatedOn <= EXECUTED_DATETIME)).ToList();
                //	if (messages != null && messages.Count > 0)
                //		settingData.InboxMessages = messages.Select(m => new Inbox { Id = m.Id, Description = m.Description, CreatedOn = m.CreatedOn }).ToList();
                //}
                return settingData;
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));

                return new SettingData();
            }
        }

        [System.Web.Http.HttpGet]
        [Route("GetSettingsV2")]
        public HttpResponseMessage GetSettingsV2([FromUri] DatesApi dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.From.AddMinutes(-5);
                DateTime EXECUTED_DATETIME = dates.To;

                List<SettingApi> settings = new List<SettingApi>();

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var dbSettings = db.Setting.Where(obj => obj.Updated > LAST_EXECUTED_DATETIME && obj.Updated <= EXECUTED_DATETIME && obj.OutletId == dates.OutletId).ToList();
                    if (dbSettings != null)
                        settings = dbSettings.Select(obj => SettingApi.ConvertModelToApiModel(obj)).ToList();
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
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// Post Exception generated in NIMPOS systems
        /// </summary>
        /// <param name="exceptionLog"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("PostException")]
        public IHttpActionResult PostException(POSSUM.Model.ExceptionLog exceptionLog)
        {
            try
            {

                //using (var uof = _helper.CreateUnitOfWork())
                //{
                //    var exceptionLogRepo = new Repository<ExceptionLog, long>(uof.Session);

                //    exceptionLogRepo.Add(exceptionLog);

                //    uof.Commit();
                //}
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {

                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// Post Payment device Log
        /// </summary>
        /// <param name="employeeLog"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostEmployeeLog")]
        public IHttpActionResult PostEmployeeLog(POSSUM.Model.EmployeeLog employeeLog)
        {
            try
            {

                using (var db = new ApplicationDbContext(connectionString))
                {
                    var emp = db.Employee.FirstOrDefault(e => e.Id == employeeLog.EmployeeId);
                    if (emp == null)
                    {
                        db.Employee.Add(employeeLog.Employee);
                    }
                    else
                        employeeLog.Employee = null;
                    db.EmployeeLog.Add(employeeLog);
                    //
                    db.SaveChanges();
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        [HttpPost]
        [Route("PostDeviceLog")]
        public IHttpActionResult PostDeviceLog(POSSUM.Model.PaymentDeviceLog deviceLog)
        {
            try
            {
                using (var db = new ApplicationDbContext(connectionString))
                {
                    db.PaymentDeviceLog.Add(deviceLog);

                    db.SaveChanges();
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }


        /// <summary>
        /// Get updated employees in a date range
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>

        [Route("GetEmployees")]
        public async Task<List<Employee>> GetEmployees([FromUri] Dates dates)
        {
            DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
            DateTime EXECUTED_DATETIME = dates.CurrentDate;
            Guid terminalId = dates.TerminalId;

            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    return db.Employee.Where(emp => emp.Updated > LAST_EXECUTED_DATETIME && emp.Updated <= EXECUTED_DATETIME).ToList();
                }

            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));

                return new List<Employee>();
            }
        }
        /// <summary>
        /// Post Employeee added from NIMPOS system
        /// </summary>
        /// <param name="employee">Employee Model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostEmployee")]
        public IHttpActionResult PostEmployee(POSSUM.Model.Employee employee)
        {
            try
            {

                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var empRepo = uof.EmployeeRepository;


                    empRepo.AddOrUpdate(employee);

                    uof.Commit();
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {

                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// Get All Floors
        /// </summary>
        /// <returns></returns>
        [Route("GetFloors")]
        public async Task<List<Floor>> GetFloors()
        {
            
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    return db.Floor.ToList().Select(s => new Floor { Id = s.Id, Name = s.Name }).ToList();
                }

            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));

                return new List<Floor>();
            }
        }
        /// <summary>
        /// Get All Tables
        /// </summary>
        /// <returns></returns>
        [Route("GetTables")]
        public async Task<List<FoodTable>> GetTables()
        {


            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    return db.FoodTable.ToList().Select(f => new FoodTable { Id = f.Id, Name = f.Name, FloorId = f.FloorId }).ToList();
                }

            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));

                return new List<FoodTable>();
            }
        }
        /// <summary>
        /// Get Payment Types
        /// </summary>
        /// <returns></returns>
        [Route("GetPaymentTypes")]
        public async Task<List<PaymentType>> GetPaymentTypes()
        {


            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    return db.PaymentType.ToList();
                }

            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));

                return new List<PaymentType>();
            }
        }
        /// <summary>
        /// Get Outlets for customers
        /// </summary>
        /// <returns></returns>
        [Route("GetAllOutlets")]
        public async Task<List<CustomerOutlet>> GetAllOutlets()
        {
            List<CustomerOutlet> customerOutlets = new List<CustomerOutlet>();

            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var outlets = db.Outlet.ToList();
                    var terminals = db.Terminal.ToList();
                    foreach (var outlet in outlets)
                    {
                        customerOutlets.Add(new CustomerOutlet
                        {
                            Id = outlet.Id,
                            Name = outlet.Name,
                            Terminals = terminals.Where(t => t.OutletId == outlet.Id).Select(t => new CustomerTerminal
                            {
                                Id = t.Id,
                                OutletId = t.OutletId,
                                Name = t.UniqueIdentification
                            }).ToList()
                        });
                    }
                    return customerOutlets;
                }

            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));

                return customerOutlets;
            }
        }
    }
}
