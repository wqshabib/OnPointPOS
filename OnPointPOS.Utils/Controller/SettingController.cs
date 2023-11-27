using Newtonsoft.Json;
using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
    public class SettingController
    {

        TempTerminal liveTerminal = new TempTerminal();
        List<TempSetting> liveSettings = new List<TempSetting>();
        TempOutlet liveOutlet = new TempOutlet();
        List<TempPrinter> livePrinters = new List<TempPrinter>();
        List<TempCampaign> liveCampaigns = new List<TempCampaign>();
        List<ZReportSetting> liveReportSettings = new List<ZReportSetting>();
        List<Inbox> liveMessages = new List<Inbox>();
        string connectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;
        ApplicationDbContext db; 
        public SettingController(string connectionString, ApplicationDbContext dbCOntext)
        {
            this.db = dbCOntext;
            this.connectionString = connectionString;
        }

        public SettingController(SettingData settingData, ApplicationDbContext dbCOntext)
        {
            this.db = dbCOntext;
            this.liveTerminal = settingData.Terminal;
            this.liveSettings = settingData.Settings;
            this.liveOutlet = settingData.Outlet;
            this.livePrinters = settingData.Printers;
            this.liveCampaigns = settingData.Campaigns;
            this.liveReportSettings = settingData.ZReportSettings;
            this.liveMessages = settingData.InboxMessages;
        }
        public bool UpdateSettings()
        {
            try
            {
                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var localOutletRepo = uof.OutletRepository;
                    var localTerminalRepo = uof.TerminalRepository;
                    var localSettingRepo = uof.SettingRepository;
                    var localPrinterRepo = uof.PrinterRepository;
                    var localCampaignRepo = uof.CampaignRepository;
                    var localCashDrawerRepo = uof.CashDrawerRepository;
                    if (liveTerminal != null)
                    {
                        Log.WriteLog("liveTerminal = " + JsonConvert.SerializeObject(liveTerminal));

                        if (liveTerminal.Id != default(Guid))
                        {
                            var _terminal = localTerminalRepo.FirstOrDefault(t => t.Id == liveTerminal.Id);
                            if (_terminal == null)
                            {
                                _terminal = new Terminal
                                {
                                    Id = liveTerminal.Id,
                                    OutletId = liveTerminal.OutletId,
                                    Description = liveTerminal.Description,
                                    RootCategoryId = liveTerminal.CategoryId,
                                    HardwareAddress = liveTerminal.HardwareAddress,
                                    Status = liveTerminal.Status,
                                    TerminalNo = liveTerminal.TerminalNo,
                                    UniqueIdentification = liveTerminal.UniqueIdentification,
                                    TerminalType = liveTerminal.TerminalType,
                                    IsDeleted = liveTerminal.IsDeleted,
                                    Created = liveTerminal.Created,
                                    Updated = liveTerminal.Updated
                                };
                                localTerminalRepo.Add(_terminal);
                            }
                            else
                            {
                                _terminal.OutletId = liveTerminal.OutletId;
                                _terminal.Description = liveTerminal.Description;
                                _terminal.RootCategoryId = liveTerminal.CategoryId;
                                _terminal.HardwareAddress = liveTerminal.HardwareAddress;
                                // _terminal.Status = liveTerminal.Status;//no need to update status of terminal in local db
                                _terminal.TerminalNo = liveTerminal.TerminalNo;
                                _terminal.UniqueIdentification = liveTerminal.UniqueIdentification;
                                _terminal.TerminalType = liveTerminal.TerminalType;
                                _terminal.IsDeleted = liveTerminal.IsDeleted;
                                _terminal.Created = liveTerminal.Created;
                                _terminal.Updated = liveTerminal.Updated;

                                localTerminalRepo.AddOrUpdate(_terminal);
                            }

                            var cashDrawer = liveTerminal.CashDrawer.FirstOrDefault();
                            if (cashDrawer != null)
                            {
                                var _cashDrawer = new CashDrawer
                                {
                                    Id = cashDrawer.Id,
                                    Name = cashDrawer.Name,
                                    TerminalId = cashDrawer.TerminalId,
                                    ConnectionString = cashDrawer.ConnectionString,
                                    Location = cashDrawer.ConnectionString,
                                    UserId = cashDrawer.UserId
                                };
                                localCashDrawerRepo.AddOrUpdate(_cashDrawer);
                            }

                        }
                    }
                    if (liveOutlet != null)
                    {
                        Log.WriteLog("liveOutlet = " + JsonConvert.SerializeObject(liveOutlet));

                        var _outlet = new Outlet
                        {
                            Id = liveOutlet.Id,
                            Name = liveOutlet.Name,
                            City = liveOutlet.City,
                            PostalCode = liveOutlet.PostalCode,
                            Address1 = liveOutlet.Address1,
                            Address2 = liveOutlet.Address2,
                            Address3 = liveOutlet.Address3,
                            IsDeleted = liveOutlet.IsDeleted,
                            KitchenPrinterId = liveOutlet.KitchenPrinterId,
                            BillPrinterId = liveOutlet.BillPrinterId,
                            Email = liveOutlet.Email,
                            WebUrl = liveOutlet.WebUrl,
                            OrgNo = liveOutlet.OrgNo,
                            FooterText = liveOutlet.FooterText,
                            HeaderText = liveOutlet.HeaderText,
                            Phone = liveOutlet.Phone,
                            TaxDescription = liveOutlet.TaxDescription,
                            Created = liveOutlet.Created,
                            Updated = liveOutlet.Updated,
                            WarehouseID = liveOutlet.WarehouseID

                        };
                        localOutletRepo.AddOrUpdate(_outlet);


                    }

                    if (liveSettings != null && liveSettings.Count > 0)
                    {
                        Log.WriteLog("liveSettings = " + JsonConvert.SerializeObject(liveSettings));

                        foreach (var setting in liveSettings)
                        {

                            var _setting = new Setting
                            {
                                Id = setting.Id,
                                Description = setting.Description,
                                Code = setting.Code,//(SettingCode)Enum.Parse(typeof(SettingCode), Convert.ToString(setting.Code)),
                                OutletId = setting.OutletId,
                                Sort = setting.Sort,
                                TerminalId = setting.TerminalId,
                                Type = setting.Type,
                                Value = setting.Value,
                                Created = setting.Created,
                                Updated = setting.Updated
                            };
                            localSettingRepo.AddOrUpdate(_setting);

                        }
                    }
                    if (livePrinters != null && livePrinters.Count > 0)
                    {
                        Log.WriteLog("livePrinters = " + JsonConvert.SerializeObject(livePrinters));

                        foreach (var printer in livePrinters)
                        {
                            var _printer = new Printer
                            {
                                Id = printer.Id,
                                LocationName = printer.LocationName,
                                PrinterName = printer.PrinterName,
                                Updated = printer.Updated
                            };
                            localPrinterRepo.AddOrUpdate(_printer);

                        }
                    }
                    if (liveCampaigns != null && liveCampaigns.Count > 0)
                    {
                        //foreach (var campaign in liveCampaigns)
                        //{
                        //    var _campaign = new Campaign
                        //    {
                        //        Id = campaign.Id,
                        //        BuyLimit = campaign.BuyLimit,
                        //        FreeOffer = campaign.FreeOffer,
                        //        Description = campaign.Description,
                        //        Updated = campaign.Updated,
                        //        DiscountPercentage = campaign.
                        //    };

                        //    localCampaignRepo.AddOrUpdate(_campaign);

                        //}
                    }
                    if (liveReportSettings != null && liveReportSettings.Count > 0)
                    {
                        Log.WriteLog("liveReportSettings = " + JsonConvert.SerializeObject(liveReportSettings));

                        var localzReportRepo = uof.ZReportSettingRepository;
                        foreach (var reportSettings in liveReportSettings)
                        {
                            var _reprotsettings = new ZReportSetting
                            {
                                Id = reportSettings.Id,
                                ReportTag = reportSettings.ReportTag,
                                Updated = reportSettings.Updated,
                                Visiblity = reportSettings.Visiblity

                            };

                            localzReportRepo.AddOrUpdate(_reprotsettings);

                        }
                    }
                    if (liveMessages != null && liveMessages.Count > 0)
                    {
                        Log.WriteLog("liveMessages = " + JsonConvert.SerializeObject(liveMessages));
                        var locaInboxRepo = uof.InboxRepository;
                        foreach (var liveMessage in liveMessages)
                        {
                            locaInboxRepo.AddOrUpdate(liveMessage);

                        }
                    }
                    uof.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;

            }
        }


        public bool UploadSettings(DateTime dt, string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                var lst = db.Setting.Where(a => a.Updated >= dt).ToList();
                Log.WriteLog("Found " + lst.Count + " Settings to be uploaded.");
                foreach (var item in lst)
                {
                    Log.WriteLog("Uploading Setting with Id = " + item.Id + ",Description = " + item.Description + ", baseUrl=" + baseUrl + " , apiUser = " + apiUser + ", apiPassword = " + apiPassword);
                    UploadSetting(item, baseUrl, apiUser, apiPassword);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }
        }

        private void UploadSetting(Setting setting, string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                client.PostSetting(setting);
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
            }
        }
    }
}
