using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using POSSUM.Integration;
using POSSUM.Model;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Concurrent;

namespace POSSUM.Integration
{
    public static class DefaultsIntegration
    {
        public static int BamboraLogMode = 1;
        #region Properties
        public static Terminal Terminal { get; set; }
        public static Outlet Outlet { get; set; }
        public static TillUser User { get; set; }
        public static string ClientId { get; set; }
        public static string ClientPassword { get; set; }
        public static string ClientUserId { get; set; }
        public static string ClientWebURL { get; set; }
        // public static ConcurrentDictionary<JournalActionCode, JournalAction> ActionList { get; set; }
        public static ConcurrentDictionary<SettingCode, string> SettingsList { get; set; }
        public static OrderEntryType OrderEntryType { get; set; }
        public static SaleType SaleType { get; set; }
        public static bool TipStatus { get; set; }
        public static bool Takeaway { get; set; }
        public static bool DirectCash { get; set; }
        public static bool DirectCard { get; set; }
        public static bool BONG { get; set; }
        public static bool ShowTableGrid { get; set; }
        public static bool ShowEmployeeLog { get; set; }
        public static bool LogoEnable { get; set; }
        public static bool ShowSwishButton { get; set; }
        public static bool ShowStudentCardButton { get; set; }
        public static bool DailyBongCounter { get; set; }


        public static bool IsClient { get; set; }
        public static RegionInfo RegionInfo { get; set; }
        public static int CategoryLines { get; set; }
        public static int ItemsLines { get; set; }
        public static RunningMode RunningMode { get; set; }
        public static ScreenResulution ScreenResulution { get; set; }
        public static string SlideShowURL { get; set; }
        public static string SlideShowURL2 { get; set; }
        public static string PrinterName { get; set; }
        public static List<IconStore> IconList { get; set; }
        public static List<PaymentType> PaymentTypes { get; set; }
        public static List<Product> Products { get; set; }
        public static List<ItemCategory> ItemCategories { get; set; }
        public static List<Printer> Printers { get; set; }
        public static List<string> PerformanceLog { get; set; }
        public static long ReceiptCounter { get; set; }
        public static bool ShowBongAlert { get; set; }
        public static bool ShowExternalOrder { get; set; }
        public static string M2MqttService { get; set; }
        public static string ExternalOrderAPI { get; set; }
        public static string ExternalAPIUSER { get; set; }
        public static string ExternalAPIPassword { get; set; }
        public static string APIUSER { get; set; }
        public static string APIPassword { get; set; }
        public static string FakturaReference { get; set; }
        public static bool IsDallasKey { get; set; }
        public static string DallasKey { get; set; }
        public static int RootCategoryId { get; set; }
        public static bool CustomerView { get; set; }
        public static int CustomerOrderInfo { get; set; }
        public static bool BongNormalFont { get; set; }
        public static bool TableNeededOnBong { get; set; }
        public static bool TableShowOnBong { get; set; }
        public static PriceMode PriceMode { get; set; }
        public static int NightStartHour { get; set; }
        public static int NightEndHour { get; set; }
        public static bool DualPriceMode { get; set; }
        public static bool BongByProduct { get; set; }
        public static bool OrderNoOnBong { get; set; }
        public static bool BongCounter { get; set; }
        public static bool MultiKitchen { get; set; }

        public static bool DeviceLog { get; set; }
        public static bool PricePolicy { get; set; }
        public static bool ShowPrice { get; set; }
        public static bool CreditNote { get; set; }
        public static bool BeamPayment { get; set; }
        public static bool OnlineCash { get; set; }
        public static string CurrencyName { get; set; }
        public static string PrinterIP { get; set; }
        public static string CustomerId { get; set; }
        public static bool HidePaymentButton { get; set; }

        public static bool EnableExternalNetworking { get; set; }

        public static bool DigitOnly { get; set; }

        public static string LUEMqttService { get; set; }
        public static string restaurant_group_id { get; set; }
        public static string restaurant_id { get; set; }

        public static bool DebugCleanCash { get; set; }
        public static bool ReplacePosIdSpecialChars { get; set; }
        public static bool DirectMobileCard { get; set; }
        public static bool EnableMqtt { get; set; }
        public static int InvoiceDueDays { get; set; }

        public static List<CGWarning> CGWarnings { get; set; }
      
        public static bool CASH_GUARD { get; set; }
        public static int CASH_GuardPort { get; set; }
        public static string MenuItemUrl { get; set; }
        public static string DiscountCode { get; set; }

        #endregion

        public static CurrentLanguage Language = (CurrentLanguage)Enum.Parse(typeof(CurrentLanguage), ConfigurationManager.AppSettings["Language"]);
        public static CashDrawerType CashDrawerType = (CashDrawerType)Enum.Parse(typeof(CashDrawerType), ConfigurationManager.AppSettings["CashDrawerType"]);
        public static Int16 CashDrawerHardwarePort = 0;// 0x48C;// Convert.ToInt16( ConfigurationManager.AppSettings["CashDrawerHardwarePort"]);
        public static PaymentDeviceType PaymentDeviceType = (PaymentDeviceType)Enum.Parse(typeof(PaymentDeviceType), ConfigurationManager.AppSettings["PaymentDeviceType"]);
        public static ControlUnitType ControlUnitType = (ControlUnitType)Enum.Parse(typeof(ControlUnitType), ConfigurationManager.AppSettings["ControlUnitType"]);
        public static string PaymentDevicConnectionString = ConfigurationManager.AppSettings["PaymentDevicConnectionString"];
        public static string ControlUnitConnectionString = ConfigurationManager.AppSettings["ControlUnitConnectionString"];
       
        public static ScaleType ScaleType = (ScaleType)Enum.Parse(typeof(ScaleType), ConfigurationManager.AppSettings["ScaleType"]);
        public static string SCALEPORT { get; set; }
        public static string LocalConnectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;
        public static string TerminalId = ConfigurationManager.AppSettings["TerminalId"];
        public static string SyncAPIUri = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
        public static AppProvider AppProvider = new AppProvider { AppTitle = "NIMPOS AB - Kassasystem", Name = "NIMPOS AB", Version = "1.0.0" };
        static string region = "SE";
        public static TerminalMode TerminalMode = TerminalMode.SingleOutlet;
        public static PosIdType PosIdType = PosIdType.UniqueId;
        //public static bool LocalSetting(string userName)
        //{
        //    try
        //    {


        //        TerminalMode terminalMode;
        //        Enum.TryParse(ConfigurationManager.AppSettings["TerminalMode"], true, out terminalMode);
        //        TerminalMode = terminalMode;
        //        if (terminalMode == TerminalMode.MultiOutlet)
        //        {

        //            //apply local setting from xml
        //            var localSettings = Utilities.ReadLocalSettings(userName);
        //            LocalConnectionString = localSettings.ConnectionString;
        //            TerminalId = localSettings.TerminalId;
        //            PaymentDevicConnectionString = localSettings.PaymentDevicConnectionString;
        //            SyncAPIUri = localSettings.SyncAPIUri;
        //            PaymentDeviceType = (PaymentDeviceType)Enum.Parse(typeof(PaymentDeviceType), localSettings.PaymentDeviceType);
        //            (App.Current as App).State = new PosState();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWriter.LogWrite(ex);
        //        return false;
        //    }
        //}
        //public static void Init()
        //{
        //    try
        //    {
        //        PerformanceLog = new List<string>();
        //        CGWarnings = new List<CGWarning>();
        //        using (var uof = PosState.GetInstance().CreateUnitOfWork())
        //        {
        //            //LOAD TERMINAL SETTINGS
        //            var terminalRepository = new Repository<Terminal, Guid>(uof.Session);
        //            var printerRepo = new Repository<Printer, int>(uof.Session);
        //            Terminal = terminalRepository.FirstOrDefault(t => t.Id == new Guid(TerminalId));
        //            if (Terminal == null)
        //            {
        //                return;
        //            }
        //            RootCategoryId = Defaults.Terminal.Category.Id;
        //            Outlet = Terminal.Outlet;
        //            if (string.IsNullOrEmpty(Outlet.FooterText))
        //                Outlet.FooterText = "  ";
        //            Printers = printerRepo.ToList();
        //            var printer = Printers.FirstOrDefault();
        //            if (printer != null)
        //                PrinterName = printer.PrinterName;
        //            else
        //                PrinterName = "Microsoft XPS Document Writer";
        //            var typeRepo = new Repository<PaymentType, int>(uof.Session);
        //            PaymentTypes = typeRepo.ToList();
        //            //LOAD SETTINGS
        //            SettingsList = new ConcurrentDictionary<SettingCode, string>();
        //            var settingRepository = new Repository<Setting, int>(uof.Session);
        //            //GLOBAL SETTINGS
        //            foreach (
        //                var item in
        //                    settingRepository.Where(
        //                        i =>
        //                            i.Type == (int)SettingType.TillSettings && i.TerminalId == null &&
        //                            i.OutletId == null).ToList())
        //            {
        //                if (SettingsList.ContainsKey(item.Code))
        //                    SettingsList[item.Code] = item.Value;
        //                else
        //                    SettingsList.TryAdd(item.Code, item.Value);
        //            }
        //            foreach (var item in settingRepository.Where(i => i.Type == (int)SettingType.TillSettings && i.OutletId == Terminal.Outlet.Id).ToList())
        //            {
        //                //OUTLET CAN OVERRIDE GLOBAL SETTINGS
        //                if (SettingsList.ContainsKey(item.Code))
        //                    SettingsList[item.Code] = item.Value;
        //                else
        //                    SettingsList.TryAdd(item.Code, item.Value);
        //            }
        //            foreach (var item in settingRepository.Where(i => i.Type == (int)SettingType.TillSettings && i.TerminalId == Terminal.Id).ToList())
        //            {
        //                //TERMINAL CAN OVERRIDE GLOBAL OR OUTLET SETTINGS
        //                if (SettingsList.ContainsKey(item.Code))
        //                    SettingsList[item.Code] = item.Value;
        //                else
        //                    SettingsList.TryAdd(item.Code, item.Value);
        //            }
        //            //Get ClientInfo
        //            var clientRepo = new Repository<Client, string>(uof.Session);
        //            var client = clientRepo.FirstOrDefault();
        //            if (client != null)
        //            {
        //                ClientId = client.clientId;
        //                ClientPassword = client.password;
        //                ClientUserId = client.ClientUserId;
        //                ClientWebURL = client.connectionString;
        //            }
        //            //Get Last Receipt Counter

        //            var receiptRepo = new Repository<Receipt, Guid>(uof.Session);
        //            try
        //            {
        //                ReceiptCounter = receiptRepo.Where(rcp=> rcp.TerminalId == Defaults.Terminal.Id).Max(rcp => rcp.ReceiptNumber);
        //            }
        //            catch (Exception ex)
        //            {
        //                ReceiptCounter = 0;
        //            }
        //            //LOAD JOURNAL ACTION LIST
        //            //ActionList = new ConcurrentDictionary<JournalActionCode, JournalAction>();
        //            //var jaRepository = new Repository<JournalAction, int>(uof.Session);
        //            //foreach (var item in jaRepository.ToList())
        //            //    ActionList.TryAdd(item.ActionCode, item);
        //            string AccountNumber = " ";
        //            string PaymentReceiver = " ";
        //            var BankAccount = settingRepository.FirstOrDefault(ot => ot.Code == SettingCode.AccountNumber);
        //            if (BankAccount != null)
        //                AccountNumber = BankAccount.Value;
        //            var BankPaymentReceiver = settingRepository.FirstOrDefault(ot => ot.Code == SettingCode.PaymentReceiver);
        //            if (BankPaymentReceiver != null)
        //                PaymentReceiver = BankPaymentReceiver.Value;

        //            var fakturaRefernces = settingRepository.FirstOrDefault(ot => ot.Code == SettingCode.FakturaReference);
        //            if (fakturaRefernces != null)
        //                FakturaReference = fakturaRefernces.Value;

        //            CompanyInfo = new CompanyInfo
        //            {
        //                Name = Terminal.Outlet.Name,// "NIMPOS",
        //                Logo = AppDomain.CurrentDomain.BaseDirectory + "logo.png",
        //                Address = string.IsNullOrEmpty(Terminal.Outlet.Address) ? " " : Terminal.Outlet.Address,
        //                Address1 = string.IsNullOrEmpty(Terminal.Outlet.Address1) ? " " : Terminal.Outlet.Address1,
        //                Phone = string.IsNullOrEmpty(Terminal.Outlet.Phone) ? " " : Terminal.Outlet.Phone,
        //                Email = string.IsNullOrEmpty(Terminal.Outlet.Email) ? " " : Terminal.Outlet.Email,
        //                URL = string.IsNullOrEmpty(Terminal.Outlet.WebUrl) ? " " : Terminal.Outlet.WebUrl,
        //                OrgNo = string.IsNullOrEmpty(Terminal.Outlet.OrgNo) ? " " : Terminal.Outlet.OrgNo,
        //                Footer = string.IsNullOrEmpty(Terminal.Outlet.FooterText) ? " " : Terminal.Outlet.FooterText,
        //                Header = string.IsNullOrEmpty(Terminal.Outlet.HeaderText) ? " " : Terminal.Outlet.HeaderText,
        //                ZipCode = string.IsNullOrEmpty(Terminal.Outlet.PostalCode) ? " " : Terminal.Outlet.PostalCode,
        //                TaxDescription = string.IsNullOrEmpty(Terminal.Outlet.TaxDescription) ? " " : Terminal.Outlet.TaxDescription,
        //                BankAccountNo = AccountNumber,
        //                PaymentReceiver = PaymentReceiver

        //            };
        //            OrderEntryType entryType;
        //            Enum.TryParse(SettingsList[SettingCode.OrderEntryType], true, out entryType);
        //            OrderEntryType = OrderEntryType.RecordAll;

        //            bool bresult;
        //            bool.TryParse(SettingsList[SettingCode.TipStatus], out bresult);
        //            TipStatus = bresult;

        //            SaleType saleType;
        //            Enum.TryParse(ConfigurationManager.AppSettings["SaleType"], true, out saleType);
        //            SaleType = saleType;

        //            RunningMode runningMode;
        //            Enum.TryParse(ConfigurationManager.AppSettings["RunningMode"], true, out runningMode);
        //            RunningMode = runningMode;

        //            PosIdType posIdType;
        //            Enum.TryParse(ConfigurationManager.AppSettings["PosIdType"], true, out posIdType);
        //            PosIdType = posIdType;

        //            int lines = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["CategoryLines"], out lines);
        //            CategoryLines = lines == 0 ? 1 : lines;
        //            int itemlines = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["ItemLines"], out itemlines);
        //            ItemsLines = lines == 0 ? 4 : itemlines;
        //            int takeaway = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["Takeaway"], out takeaway);
        //            Takeaway = takeaway == 1 ? true : false;

        //            int directCash = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DirectCash"], out directCash);
        //            DirectCash = directCash == 1 ? true : false;

        //            int directCard = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DirectCard"], out directCard);
        //            DirectCard = directCard == 1 ? true : false;

        //            int directMobileCard = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DirectMobileCard"], out directMobileCard);
        //            DirectMobileCard = directMobileCard == 1 ? true : false;

        //            int enableMqtt = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["EnableMqtt"], out enableMqtt);
        //            EnableMqtt = enableMqtt == 1 ? true : false;
                    


        //            int bong = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["BONG"], out bong);
        //            BONG = bong == 1 ? true : false;
        //            int debugCleanCash = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DebugCleanCash"], out debugCleanCash);
        //            DebugCleanCash = debugCleanCash == 1 ? true : false;

        //            int replacePosIdSpecialChars = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["ReplacePosIdSpecialChars"], out replacePosIdSpecialChars);
        //            ReplacePosIdSpecialChars = replacePosIdSpecialChars == 1 ? true : false;
                    

        //            int tableGrid = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["TableView"], out tableGrid);
        //            ShowTableGrid = tableGrid == 1 ? true : false;

        //            int logvisibility = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["EmployeeLog"], out logvisibility);
        //            ShowEmployeeLog = logvisibility == 1 ? true : false;

        //            int logoEnable = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["LogoEnable"], out logoEnable);
        //            LogoEnable = logoEnable == 1 ? true : false;

        //            int isDallasKey = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DallasKey"], out isDallasKey);
        //            IsDallasKey = isDallasKey == 1 ? true : false;

        //            int bongAlert = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["ShowBongAlert"], out bongAlert);
        //            ShowBongAlert = bongAlert == 1 ? true : false;

        //            int showExternal = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["ShowExternalOrder"], out showExternal);
        //            ShowExternalOrder = showExternal == 1 ? true : false;

        //            int customerView = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["CustomerView"], out customerView);
        //            CustomerView = customerView == 1 ? true : false;

        //            int dailyBongCounter = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DailyBongCounter"], out dailyBongCounter);
        //            DailyBongCounter = dailyBongCounter == 1 ? true : false;

        //            int customerOrderInfo = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["CustomerOrderInfo"], out customerOrderInfo);
        //            CustomerOrderInfo = customerOrderInfo;// == 1 ? true : false;


        //            int bongFont = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["BongNormalFont"], out bongFont);
        //            BongNormalFont = bongFont == 1 ? true : false;

        //            int tableNeededOnBong = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["TableNeededOnBong"], out tableNeededOnBong);
        //            TableNeededOnBong = tableNeededOnBong == 1 ? true : false;

        //            int tableShowOnBong = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["TableShowOnBong"], out tableShowOnBong);
        //            TableShowOnBong = tableShowOnBong == 1 ? true : false;


        //            int dualPriceMode = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DualPriceMode"], out dualPriceMode);
        //            DualPriceMode = dualPriceMode == 1 ? true : false;

        //            int nightHour = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["NightStartHour"], out nightHour);
        //            NightStartHour = nightHour;
        //            int nightendHour = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["NightEndHour"], out nightendHour);
        //            NightEndHour = nightendHour;

        //            int isClient = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["IsClient"], out isClient);
        //            IsClient = isClient == 1 ? true : false;

        //            int showPaymentButton = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["ShowSwishButton"], out showPaymentButton);
        //            ShowSwishButton = showPaymentButton == 1 ? true : false;

        //            int showPrice = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["ShowPrice"], out showPrice);
        //            ShowPrice = showPrice == 1 ? true : false;

        //            int showStudentCardButton = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["ShowStudentCardButton"], out showStudentCardButton);
        //            ShowStudentCardButton = showStudentCardButton == 1 ? true : false;

        //            int bongByProduct = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["BongByProduct"], out bongByProduct);
        //            BongByProduct = bongByProduct == 1 ? true : false;

        //            int bongCounter = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["BongCounter"], out bongCounter);
        //            BongCounter = bongCounter == 1 ? true : false;

        //            int multiKitchen = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["MultiKitchen"], out multiKitchen);
        //            MultiKitchen = multiKitchen == 1 ? true : false;

        //            int orderNoOnBong = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["OrderNoOnBong"], out orderNoOnBong);
        //            OrderNoOnBong = orderNoOnBong == 1 ? true : false;

        //            int deviceLog = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DeviceLog"], out deviceLog);
        //            DeviceLog = deviceLog == 1 ? true : false;

        //            int creditNote = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["CreditNote"], out creditNote);
        //            CreditNote = creditNote == 1 ? true : false;

        //            int beamPayment = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["BeamPayment"], out beamPayment);
        //            BeamPayment = beamPayment == 1 ? true : false;

        //            int hidePaymentButton = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["HidePaymentButton"], out hidePaymentButton);
        //            HidePaymentButton = hidePaymentButton == 1 ? true : false;

        //            int _onlineCash = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["OnlineCash"], out _onlineCash);
        //            OnlineCash = _onlineCash == 1 ? true : false;

        //            int pricePolicy = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["PricePolicy"], out pricePolicy);
        //            PricePolicy = pricePolicy == 1 ? true : false;

        //            int digitOnly = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["DigitOnly"], out digitOnly);
        //            DigitOnly = digitOnly == 1 ? true : false;

        //            int invoiceDueDays = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["InvoiceDueDays"], out invoiceDueDays);
        //            InvoiceDueDays = invoiceDueDays == 0 ? 21 : invoiceDueDays;

        //            int enableExternalNetworking = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["EnableExternalNetworking"], out enableExternalNetworking);
        //            EnableExternalNetworking = enableExternalNetworking == 1 ? true : false;

        //            CustomerId = ConfigurationManager.AppSettings["CustomerId"];

        //            SCALEPORT = ConfigurationManager.AppSettings["SCALEPORT"];
        //            SlideShowURL = ConfigurationManager.AppSettings["SlideShowURL"];
        //            SlideShowURL2 = ConfigurationManager.AppSettings["SlideShowURL2"];
        //            M2MqttService = Convert.ToString(ConfigurationManager.AppSettings["M2MqttService"]);
        //            ExternalOrderAPI = Convert.ToString(ConfigurationManager.AppSettings["ExternalOrderAPI"]);

        //            ExternalAPIUSER = Convert.ToString(ConfigurationManager.AppSettings["ExternalAPIUSER"]);
        //            ExternalAPIPassword = Convert.ToString(ConfigurationManager.AppSettings["ExternalAPIPassword"]);

        //            LUEMqttService = Convert.ToString(ConfigurationManager.AppSettings["LUEMqttService"]);
        //            restaurant_group_id = Convert.ToString(ConfigurationManager.AppSettings["restaurant_group_id"]);
        //            restaurant_id = Convert.ToString(ConfigurationManager.AppSettings["restaurant_id"]);

        //            APIUSER = Convert.ToString(ConfigurationManager.AppSettings["APIUSER"]);
        //            APIPassword = Convert.ToString(ConfigurationManager.AppSettings["APIPassword"]);
        //            var iconRepo = new Repository<IconStore, int>(uof.Session);
        //            IconList = iconRepo.ToList();
        //            // string hexString = ConfigurationManager.AppSettings["CashDrawerHardwarePort"];
        //            // Int16 num = Int16.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        //            Int16 port = 0;// 0x48C;
        //            Int16.TryParse(ConfigurationManager.AppSettings["CashDrawerHardwarePort"], out port);
        //            CashDrawerHardwarePort = port;

        //            int cg = 0;
        //            int.TryParse(ConfigurationManager.AppSettings["CASH_GUARD"], out cg);
        //            CASH_GUARD = cg == 1 ? true : false;

        //            Int16 cgport = 0;//1;
        //            Int16.TryParse(ConfigurationManager.AppSettings["CASH_GuardPort"], out cgport);
        //            CASH_GuardPort = cgport;

        //            MenuItemUrl = ConfigurationManager.AppSettings["MenuItem"];
        //            DiscountCode= ConfigurationManager.AppSettings["DiscountCode"];

        //        }
        //        //    ClientId = "prcenter_eproducts";
        //        //   ClientPassword = "550561";

        //        RegionInfo = new RegionInfo(region);
        //    }
        //    catch (Exception exp)
        //    {
        //        LogWriter.LogWrite(exp);
        //    }
        //}

        //public static void ResetOutlet()
        //{
        //    try
        //    {
        //        using (var uof = PosState.GetInstance().CreateUnitOfWork())
        //        {
        //            //LOAD TERMINAL SETTINGS
        //            var terminalRepository = new Repository<Terminal, Guid>(uof.Session);
        //            var printerRepo = new Repository<Printer, int>(uof.Session);
        //            Terminal = terminalRepository.FirstOrDefault(t => t.Id == new Guid(TerminalId));
        //            if (Terminal == null)
        //            {
        //                return;
        //            }
        //            RootCategoryId = Defaults.Terminal.Category.Id;
        //            Outlet = Terminal.Outlet;
        //            if (string.IsNullOrEmpty(Outlet.FooterText))
        //                Outlet.FooterText = "  ";
        //            Printers = printerRepo.ToList();
        //            var printer = Printers.FirstOrDefault();
        //            if (printer != null)
        //                PrinterName = printer.PrinterName;
        //            else
        //                PrinterName = "Microsoft XPS Document Writer";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWriter.LogException(ex);
        //    }
        //}
        public static string CultureString
        {
            get
            {
                switch (ConfigurationManager.AppSettings["Language"])
                {
                    case "1":
                        return "sv-SE";
                    case "2":
                        return "en-US";
                    case "3":
                        return "es-ES";
                    case "4":
                        return "ar-SA";

                    default:
                        return "sv-SE";
                }

            }
        }

        /// <summary>
        /// Get the system culture info, currently set as the CurrentUICulture
        /// </summary>
        //public static IFormatProvider UICultureInfo = System.Globalization.CultureInfo.CurrentUICulture;
        public static IFormatProvider UICultureInfo { get; set; }// = new CultureInfo(CultureString);
        // public static IFormatProvider UICultureInfo = new CultureInfo("en-US");
        public static IFormatProvider CurrencyCultureInfo
        {
            get; set;
        }
        public static IFormatProvider UICultureInfoWithoutCurrencySymbol
        {
            get
            {
                var setUICulture = (CultureInfo)UICultureInfo;
                var uiCultureWithoutCurrencySymbol = (CultureInfo)setUICulture.Clone();
                uiCultureWithoutCurrencySymbol.NumberFormat.CurrencySymbol = "";
                return uiCultureWithoutCurrencySymbol;
            }
        }


    }
    public enum RunningMode
    {
        Online = 1,
        Offline = 2
    }
    public enum TerminalMode
    {
        SingleOutlet = 0,
        MultiOutlet = 1
    }
    public enum CurrentLanguage
    {
        Swedish = 1,
        English = 2,
        Spanish = 3,
        Arabic = 4

    }
    public enum PosIdType
    {
        UniqueId = 0,
        TerminalNo = 1
    }
    public enum SaleType
    {
        Restaurant = 1,
        Retail = 2
    }
    public enum OrderEntryType
    {
        RecordAll = 1,
        Default = 2
    }
    public enum ScreenResulution
    {
        SR_1000X768 = 1,
        SR_1366X768 = 2,
        SR_OTHER = 3
    }

    public class CGWarning
    {
        public CGWarning(DateTime dt,string desc)
        {
            CreatedOn = dt;
            Description = desc;
        }
        public DateTime CreatedOn { get; set; }
        public string Description { get; set; }

        public string Text { get { return CreatedOn + ": " + Description + "\n"; } }
    }

}
