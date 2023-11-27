using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;
using MQTTnet.Client;
using POSSUM.Data;
using POSSUM.Integration;
using POSSUM.Integration.ControlUnits.CleanCash;
using POSSUM.Model;

namespace POSSUM
{
    public static class Defaults
    {
        //public static CurrentLanguage Language =
        //    (CurrentLanguage)Enum.Parse(typeof(CurrentLanguage), ConfigurationManager.AppSettings["Language"]);

        //SettingRepository settingRepo = null;
     
        public static string TerminalId = ConfigurationManager.AppSettings["TerminalId"]; 
        public static string MQTTTopic = ConfigurationManager.AppSettings["MQTTTopic"];
        public static string ISMQTTInitialize = ConfigurationManager.AppSettings["ISMQTTInitialize"];
        public static string ISMQTTInitializeForTerminal = ConfigurationManager.AppSettings["ISMQTTInitializeForTerminal"]; 
        public static string POSMiniPrintForAccountCustomer = ConfigurationManager.AppSettings["POSMiniPrintForAccountCustomer"];
        public static string ISMQTTFORPOSMINI = ConfigurationManager.AppSettings["ISMQTTFORPOSMINI"];
        public static string ISMQTTFORONLINEORDER = ConfigurationManager.AppSettings["ISMQTTFORONLINEORDER"];

        public static CurrentLanguage Language =
            (CurrentLanguage)Enum.Parse(typeof(CurrentLanguage), GetDefaultLanguageFromSettings(TerminalId));




        public static string GetDefaultLanguageFromSettings(string TerminalId)
        {
            string langValue = "1";
            try
            {
                Setting setings = new SettingRepository().GetDefaultSettings(SettingCode.Language, TerminalId);
                if (setings != null)
                    langValue = setings.Value;
                return langValue.ToString();
            }
            catch (Exception exp)
            {
                return langValue;
            }
        }

        //public static CurrentLanguage Language =
        //   (CurrentLanguage)Enum.Parse(typeof(CurrentLanguage), Defaults.SettingsList[SettingCode.Language]);
        //public static string TerminalId = ConfigurationManager.AppSettings["TerminalId"];

        //public static CashDrawerType CashDrawerType =
        //    (CashDrawerType)Enum.Parse(typeof(CashDrawerType), ConfigurationManager.AppSettings["CashDrawerType"]);
        public static CashDrawerType CashDrawerType =
            (CashDrawerType)Enum.Parse(typeof(CashDrawerType), new SettingRepository().GetSettingsByCode(SettingCode.CashDrawerType,TerminalId));



        public static short CashDrawerHardwarePort;
        // 0x48C;// Convert.ToInt16( ConfigurationManager.AppSettings["CashDrawerHardwarePort"]);

        public static PaymentDeviceType PaymentDeviceType =
            (PaymentDeviceType)
            Enum.Parse(typeof(PaymentDeviceType), new SettingRepository().GetSettingsByCode(SettingCode.PaymentDeviceType, TerminalId));

        //public static ControlUnitType ControlUnitType =
        //    (ControlUnitType)Enum.Parse(typeof(ControlUnitType), ConfigurationManager.AppSettings["ControlUnitType"]);
        public static ControlUnitType ControlUnitType =
            (ControlUnitType)Enum.Parse(typeof(ControlUnitType), new SettingRepository().GetSettingsByCode(SettingCode.ControlUnitType, TerminalId));


        public static string PaymentDevicConnectionString = new SettingRepository().GetSettingsByCode(SettingCode.PaymentDevicConnectionString, TerminalId);
        //ConfigurationManager.AppSettings["PaymentDevicConnectionString"];

        public static string ControlUnitConnectionString = new SettingRepository().GetSettingsByCode(SettingCode.ControlUnitConnectionString, TerminalId);
        //ConfigurationManager.AppSettings["ControlUnitConnectionString"]; 

        public static ScaleType ScaleType =
            (ScaleType)Enum.Parse(typeof(ScaleType), new SettingRepository().GetSettingsByCode(SettingCode.ScaleType, TerminalId));

        public static string LocalConnectionString =
            ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;


        //public static string SyncAPIUri = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
        public static string SyncAPIUri = new SettingRepository().GetSettingsByCode(SettingCode.ClientSettingsProvider, TerminalId);

        private static readonly string region = "SE";
        public static AppProvider AppProvider = region == "PK" ? PKOfficeInfo() : SEOfficeInfo();

        private static AppProvider SEOfficeInfo()
        {
            return new AppProvider
            {
                AppTitle = "POSSUM AB - Kassasystem",
                Name = "POSSUM",
                Version = "Version 1.0.0",
                SupportInfo = "Support -031-7882400 (Vardagar 08:00 - 17:00)",
                Phone = "031-7882400",
                City = "Göteborg",
                Zip = "412 62",
                POBOX = "412 62",
                Address = "KULLEGATAN 10"
            };
        }
        private static AppProvider PKOfficeInfo()
        {
            return new AppProvider
            {
                AppTitle = "POSSUM PK - Cash System",
                Name = "POSSUM PK",
                Version = "Version 2.0.0",
                SupportInfo = "Support - 0332-790 53 63 (From 08:00 - 17:00)",
                Phone = "0332-790 53 63",
                City = "Islamabad",
                Zip = "44000",
                POBOX = "Box 1902",
                Address = "I-9/3 ST Park-3 Islamabad"
            };
        }


        public static TerminalMode TerminalMode = TerminalMode.SingleOutlet;

        /// <summary>
        /// Get the system culture info, currently set as the CurrentUICulture
        /// </summary>
        //public static IFormatProvider UICultureInfo = System.Globalization.CultureInfo.CurrentUICulture;
        public static IFormatProvider UICultureInfo { get; set; }

        // public static IFormatProvider UICultureInfo = new CultureInfo("en-US");
        public static IFormatProvider CurrencyCultureInfo = new CultureInfo(ConfigurationManager.AppSettings["Currency"]);
        public static string SCALEPORT { get; set; }

        public static string AccountNumber1 { get; set; }
        public static string PaymentReceiverName { get; set; }
        public static string InvoiceReference { get; set; }


        public static bool CustomerOrderInfo { get; set; }
        public static bool CustomerView { get; set; }
        public static bool BongNormalFont { get; set; }
        public static string FakturaReference { get; set; }
        public static string CultureString
        {
            get
            {
                switch (GetDefaultLanguageFromSettings(TerminalId))
                {
                    case "1":
                        return "sv-SE";
                    case "2":
                        return "en-US";
                    case "3":
                        return "es-ES";
                    case "4":
                        return "ar-SA";
                    case "5":
                        return "ur-PK";
                    default:
                        return "sv-SE";
                }
            }
        }
        public static string CurrencyName
        {
            get
            {
                switch (ConfigurationManager.AppSettings["Currency"])
                {
                    case "sv-SE":
                        return "SEK";
                    case "en":
                        return "USD";
                    case "es-ES":
                        return "MXN";
                    case "ar-SA":
                        return "AED";
                    case "ur-PK":
                        return "PKR";
                    default:
                        return "SEK";
                }
            }
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

        public static bool LocalSetting(string userName)
        {
            try
            {
                TerminalMode terminalMode;
                Enum.TryParse(ConfigurationManager.AppSettings["TerminalMode"], true, out terminalMode);
                TerminalMode = terminalMode;
                if (terminalMode == TerminalMode.MultiOutlet)
                {
                    Utilities utility = new Utilities();
                    //apply local setting from xml
                    var localSettings = utility.ReadLocalSettings(userName);
                    LocalConnectionString = localSettings.ConnectionString;
                    TerminalId = localSettings.TerminalId;
                    PaymentDevicConnectionString = localSettings.PaymentDevicConnectionString;
                    SyncAPIUri = localSettings.SyncAPIUri;
                    PaymentDeviceType =
                        (PaymentDeviceType)Enum.Parse(typeof(PaymentDeviceType), localSettings.PaymentDeviceType);
                    (Application.Current as App).State = new PosState();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        } 

        public static void Init()
        {
            try
            {

                Defaults.PerformanceLog = new PerformanceLog();
                using (var uof = new UnitOfWork(new ApplicationDbContext()))
                {
                    //LOAD TERMINAL SETTINGS
                    var guid = new Guid(TerminalId);
                    var _Terminal = uof.TerminalRepository.FirstOrDefault(t => t.Id == guid);

                    if (_Terminal == null)
                    {
                        return;
                    }
                    else
                    {
                        Terminal = new Terminal
                        {
                            Id = _Terminal.Id,
                            CashDrawer = _Terminal.CashDrawer,
                            Category = new Category { Id = _Terminal.Category.Id, Name = _Terminal.Category.Name, Parant = _Terminal.Category.Parant, CategoryLevel = _Terminal.Category.CategoryLevel },
                            Created = _Terminal.Created,
                            Description = _Terminal.Description,
                            HardwareAddress = _Terminal.HardwareAddress,
                            IsDeleted = _Terminal.IsDeleted,
                            Outlet = new Outlet { Id = _Terminal.Outlet.Id, Name = _Terminal.Outlet.Name, Address1 = _Terminal.Outlet.Address1, Address2 = _Terminal.Outlet.Address2, Address3 = _Terminal.Outlet.Address3, City = _Terminal.Outlet.City, PostalCode = _Terminal.Outlet.PostalCode, BillPrinterId = _Terminal.Outlet.BillPrinterId, KitchenPrinterId = _Terminal.Outlet.KitchenPrinterId, Email = _Terminal.Outlet.Email, Phone = _Terminal.Outlet.Phone, OrgNo = _Terminal.Outlet.OrgNo, WebUrl = _Terminal.Outlet.WebUrl, TaxDescription = _Terminal.Outlet.TaxDescription, HeaderText = _Terminal.Outlet.HeaderText, FooterText = _Terminal.Outlet.FooterText, Created = _Terminal.Outlet.Created, Updated = _Terminal.Outlet.Updated, WarehouseID = _Terminal.Outlet.WarehouseID },
                            RootCategoryId = _Terminal.Category.Id,
                            Status = _Terminal.Status,
                            TerminalNo = _Terminal.TerminalNo,
                            UniqueIdentification = _Terminal.UniqueIdentification,
                            Updated = _Terminal.Updated,
                            TerminalType = _Terminal.TerminalType,
                            CCUData = _Terminal.CCUData,
                            OutletId = _Terminal.Outlet.Id
                        };
                    }
                    RootCategoryId = Terminal.Category.Id;
                    Outlet = Terminal.Outlet;
                    Printers = uof.PrinterRepository.GetAll().ToList();
                    var printer = Printers.FirstOrDefault();
                    if (printer != null)
                        PrinterName = printer.PrinterName;
                    else
                        PrinterName = "Microsoft XPS Document Writer";

                    PaymentTypes = uof.PaymentTypeRepository.GetAll().ToList();

                    //INIT CLOUD CLEAN CASH
                    InitCleanCash(Outlet, Terminal);

                    //LOAD SETTINGS
                    SettingsList = new ConcurrentDictionary<SettingCode, string>();
                    var settings = uof.SettingRepository.GetAll().ToList();
                    settings = settings.Where(s => s.TerminalId == Terminal.Id).ToList();
                    //GLOBAL SETTINGS

                    foreach (var item in settings)
                    {
                        if (SettingsList.ContainsKey(item.Code))
                            SettingsList[item.Code] = item.Value;
                        else
                            SettingsList.TryAdd(item.Code, item.Value);
                    }
                    //foreach (var item in
                    //   settings.Where(
                    //            i => i.Type == (int)SettingType.TillSettings && i.TerminalId == null && i.OutletId == null)
                    //        .ToList())
                    //{
                    //    if (SettingsList.ContainsKey(item.Code))
                    //        SettingsList[item.Code] = item.Value;
                    //    else
                    //        SettingsList.TryAdd(item.Code, item.Value);
                    //}
                    //foreach (
                    //    var item in
                    //   settings.Where(
                    //        i => i.Type == (int)SettingType.TillSettings && i.OutletId == Terminal.Outlet.Id).ToList())
                    //{
                    //    //OUTLET CAN OVERRIDE GLOBAL SETTINGS
                    //    if (SettingsList.ContainsKey(item.Code))
                    //        SettingsList[item.Code] = item.Value;
                    //    else
                    //        SettingsList.TryAdd(item.Code, item.Value);
                    //}
                    //foreach (
                    //    var item in
                    //    settings.Where(
                    //        i => i.Type == (int)SettingType.TillSettings && i.TerminalId == Terminal.Id).ToList())
                    //{
                    //    //TERMINAL CAN OVERRIDE GLOBAL OR OUTLET SETTINGS
                    //    if (SettingsList.ContainsKey(item.Code))
                    //        SettingsList[item.Code] = item.Value;
                    //    else
                    //        SettingsList.TryAdd(item.Code, item.Value);
                    //}
                    //Get ClientInfo
                    var client = uof.ClientRepository.FirstOrDefault();

                    if (client != null)
                    {
                        ClientId = client.clientId;
                        ClientPassword = client.password;
                        ClientUserId = client.ClientUserId;
                        ClientWebURL = client.connectionString;
                    }
                    //Get Last Receipt Counter


                    try
                    {
                        var counter = uof.ReceiptRepository.AsQueryable().Max(rcp => rcp.ReceiptNumber);
                        ReceiptCounter = counter;
                    }
                    catch (Exception ex)
                    {
                        ReceiptCounter = 0;
                    }

                    string AccountNumber = " ";
                    string PaymentReceiver = " ";
                    var BankAccount = settings.FirstOrDefault(ot => ot.Code == SettingCode.AccountNumber);
                    if (BankAccount != null)

                        AccountNumber = BankAccount.Value;
                    var BankPaymentReceiver =
                        settings.FirstOrDefault(ot => ot.Code == SettingCode.PaymentReceiver);
                    if (BankPaymentReceiver != null)
                        PaymentReceiver = BankPaymentReceiver.Value;
                    var fakturaRefernces = settings.FirstOrDefault(ot => ot.Code == SettingCode.FakturaReference);
                    if (fakturaRefernces != null)
                        FakturaReference = string.IsNullOrEmpty(fakturaRefernces.Value) ? Outlet.Name : fakturaRefernces.Value;
                    else
                        FakturaReference = Outlet.Name;
                    CompanyInfo = new CompanyInfo
                    {
                        Name = Terminal.Outlet.Name, // "POSSUM",
                        Logo = AppDomain.CurrentDomain.BaseDirectory + "logo.png",
                        Address = string.IsNullOrEmpty(Terminal.Outlet.Address) ? " " : Terminal.Outlet.Address,
                        Address1 = string.IsNullOrEmpty(Terminal.Outlet.Address1) ? " " : Terminal.Outlet.Address1,
                        Phone = string.IsNullOrEmpty(Terminal.Outlet.Phone) ? " " : Terminal.Outlet.Phone,
                        Email = string.IsNullOrEmpty(Terminal.Outlet.Email) ? " " : Terminal.Outlet.Email,
                        URL = string.IsNullOrEmpty(Terminal.Outlet.WebUrl) ? " " : Terminal.Outlet.WebUrl,
                        OrgNo = string.IsNullOrEmpty(Terminal.Outlet.OrgNo) ? " " : Terminal.Outlet.OrgNo,
                        Footer = string.IsNullOrEmpty(Terminal.Outlet.FooterText) ? " " : Terminal.Outlet.FooterText,
                        Header = string.IsNullOrEmpty(Terminal.Outlet.HeaderText) ? " " : Terminal.Outlet.HeaderText,
                        ZipCode = string.IsNullOrEmpty(Terminal.Outlet.PostalCode) ? " " : Terminal.Outlet.PostalCode,
                        TaxDescription =
                             string.IsNullOrEmpty(Terminal.Outlet.TaxDescription) ? " " : Terminal.Outlet.TaxDescription,
                        BankAccountNo = AccountNumber,
                        PaymentReceiver = PaymentReceiver
                    };
                    //OrderEntryType entryType;
                    //Enum.TryParse(SettingsList[SettingCode.OrderEntryType], true, out entryType);
                    OrderEntryType = OrderEntryType.RecordAll;
                    //var recordSetting = settings.FirstOrDefault(ot => ot.Code == SettingCode.OrderEntryType);
                    //if (recordSetting != null)
                    //    OrderEntryType = recordSetting.Value == "1" ? OrderEntryType.RecordAll : OrderEntryType.Default;
                    bool bresult;
                    bool.TryParse(SettingsList[SettingCode.TipStatus], out bresult);
                    TipStatus = bresult;

                    bool AskForPrint;
                    bool.TryParse(Defaults.SettingsList[SettingCode.AskForPrintInvoice], out AskForPrint);
                    AskForPrintInvoice = bresult;


                    SaleType saleType;
                    Enum.TryParse(Defaults.SettingsList[SettingCode.SaleType], true, out saleType);
                    SaleType = saleType;

                    RunningMode runningMode;
                    Enum.TryParse(ConfigurationManager.AppSettings["RunningMode"], true, out runningMode);
                    RunningMode = runningMode;

                    int lines = 0;
                    //int.TryParse(ConfigurationManager.AppSettings["CategoryLines"], out lines);
                    int.TryParse(Defaults.SettingsList[SettingCode.CategoryLines], out lines);
                    CategoryLines = lines == 0 ? 1 : lines;

                    int itemlines = 0;
                    //int.TryParse(Defaults.SettingsList[SettingCode.ItemLines], out itemlines);
                    int.TryParse(Defaults.SettingsList[SettingCode.ItemLines], out itemlines);
                    ItemsLines = lines == 0 ? 4 : itemlines;

                    int takeaway = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.Takeaway], out takeaway);
                    //int.TryParse(ConfigurationManager.AppSettings["Takeaway"], out takeaway);
                    Takeaway = takeaway == 1 ? true : false;

                    int customerInfo = 0;
                    int.TryParse(ConfigurationManager.AppSettings["CustomerOrderInfo"], out customerInfo);
                    CustomerOrderInfo = customerInfo == 1 ? true : false;

                    int customerView = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.CustomerView], out customerView);
                    //int.TryParse(ConfigurationManager.AppSettings["CustomerView"], out customerView);
                    CustomerView = customerView == 1 ? true : false;


                    int directCash = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DirectCash], out directCash);
                    DirectCash = directCash == 1 ? true : false;

                    int directCard = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DirectCard], out directCard);
                    DirectCard = directCard == 1 ? true : false;

                    int bong = 0;
                    int.TryParse(ConfigurationManager.AppSettings["BONG"], out bong);
                    BONG = bong == 1 ? true : false;


                    int logvisibility = 0;
                    int.TryParse(ConfigurationManager.AppSettings["EmployeeLog"], out logvisibility);
                    ShowEmployeeLog = logvisibility == 1 ? true : false;

                    int logoEnable = 0;
                    int.TryParse(ConfigurationManager.AppSettings["LogoEnable"], out logoEnable);
                    LogoEnable = logoEnable == 1 ? true : false;

                    int isDallasKey = 0;
                    int.TryParse(ConfigurationManager.AppSettings["DallasKey"], out isDallasKey);
                    IsDallasKey = isDallasKey == 1 ? true : false;



                    int showExternal = 0;
                    int.TryParse(ConfigurationManager.AppSettings["ShowExternalOrder"], out showExternal);
                    ShowExternalOrder = showExternal == 1 ? true : false;


                    int bongFont = 0;
                    int.TryParse(ConfigurationManager.AppSettings["BongNormalFont"], out bongFont);
                    BongNormalFont = bongFont == 1 ? true : false;

                    int tableNeededOnBong = 0;
                    int.TryParse(ConfigurationManager.AppSettings["TableNeededOnBong"], out tableNeededOnBong);
                    TableNeededOnBong = tableNeededOnBong == 1 ? true : false;



                    int dualPriceMode = 0;
                    int.TryParse(ConfigurationManager.AppSettings["DualPriceMode"], out dualPriceMode);
                    DualPriceMode = dualPriceMode == 1 ? true : false;

                    int deviceLog = 0;
                    int.TryParse(ConfigurationManager.AppSettings["DeviceLog"], out deviceLog);
                    DeviceLog = deviceLog == 1 ? true : false;
                    int nightHour = 0;
                    int.TryParse(ConfigurationManager.AppSettings["NightStartHour"], out nightHour);
                    NightStartHour = nightHour;
                    int nightendHour = 0;
                    int.TryParse(ConfigurationManager.AppSettings["NightEndHour"], out nightendHour);
                    NightEndHour = nightendHour;

                    int isClient = 0;
                    int.TryParse(ConfigurationManager.AppSettings["IsClient"], out isClient);
                    IsClient = isClient == 1 ? true : false;

                    int showswishButton = 0;
                    int.TryParse(ConfigurationManager.AppSettings["ShowSwishButton"], out showswishButton);
                    ShowSwishButton = showswishButton == 1 ? true : false;
                    int showStudentCardButton = 0;
                    int.TryParse(ConfigurationManager.AppSettings["ShowStudentCardButton"], out showStudentCardButton);
                    ShowStudentCardButton = showStudentCardButton == 1 ? true : false;

                    int bongByProduct = 0;
                    int.TryParse(ConfigurationManager.AppSettings["BongByProduct"], out bongByProduct);
                    BongByProduct = bongByProduct == 1 ? true : false;

                    int orderNoOnBong = 0;
                    int.TryParse(ConfigurationManager.AppSettings["OrderNoOnBong"], out orderNoOnBong);
                    OrderNoOnBong = orderNoOnBong == 1 ? true : false;

                    int pricePolicy = 0;
                    int.TryParse(ConfigurationManager.AppSettings["PricePolicy"], out pricePolicy);
                    PricePolicy = pricePolicy == 1 ? true : false;

                    int bongCounter = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.BongCounter], out bongCounter);
                    //int.TryParse(ConfigurationManager.AppSettings["BongCounter"], out bongCounter);
                    BongCounter = bongCounter == 1 ? true : false;

                    int dailyBongCounter = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DailyBongCounter], out dailyBongCounter);
                    //int.TryParse(ConfigurationManager.AppSettings["DailyBongCounter"], out dailyBongCounter);
                    DailyBongCounter = dailyBongCounter == 1 ? true : false;

                    int multiKitchen = 0;
                    int.TryParse(ConfigurationManager.AppSettings["MultiKitchen"], out multiKitchen);
                    MultiKitchen = multiKitchen == 1 ? true : false;

                    int enableCheckoutLog = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.EnableCheckoutLog], out enableCheckoutLog);
                    EnableCheckoutLog = enableCheckoutLog == 1 ? true : false;

                    int creditNote = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.CreditNote], out creditNote);
                    CreditNote = creditNote == 1 ? true : false;

                    int elveCard = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.ElveCard], out elveCard);
                    //int.TryParse(ConfigurationManager.AppSettings["ElveCard"], out elveCard);
                    ElveCard = elveCard == 1 ? true : false;

                    int beamPayment = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.BeamPayment], out beamPayment);
                    BeamPayment = beamPayment == 1 ? true : false;

                    int digitOnly = 0;
                    int.TryParse(ConfigurationManager.AppSettings["DigitOnly"], out digitOnly);
                    DigitOnly = digitOnly == 1 ? true : false;

                    int enableExternalNetworking = 0;
                    int.TryParse(ConfigurationManager.AppSettings["EnableExternalNetworking"], out enableExternalNetworking);
                    EnableExternalNetworking = enableExternalNetworking == 1 ? true : false;

                    Guid _customerId = default(Guid);
                    Guid.TryParse(ConfigurationManager.AppSettings["CustomerId"], out _customerId);
                    CustomerId = _customerId.ToString();

                    // SCALEPORT = ConfigurationManager.AppSettings["SCALEPORT"];
                    SCALEPORT = Defaults.SettingsList[SettingCode.SCALEPORT];

                    AccountNumber1 = Defaults.SettingsList[SettingCode.AccountNumber];
                    PaymentReceiverName = Defaults.SettingsList[SettingCode.PaymentReceiver];
                    InvoiceReference = Defaults.SettingsList[SettingCode.FakturaReference];



                    M2MqttService = Convert.ToString(ConfigurationManager.AppSettings["M2MqttService"]);
                    ExternalOrderAPI = Convert.ToString(ConfigurationManager.AppSettings["ExternalOrderAPI"]);
                    ExternalAPIUSER = Convert.ToString(ConfigurationManager.AppSettings["ExternalAPIUSER"]);
                    ExternalAPIPassword = Convert.ToString(ConfigurationManager.AppSettings["ExternalAPIPassword"]);


                    LUEMqttService = Convert.ToString(ConfigurationManager.AppSettings["LUEMqttService"]);
                    restaurant_group_id = Convert.ToString(ConfigurationManager.AppSettings["restaurant_group_id"]);
                   // restaurant_id = Convert.ToString(ConfigurationManager.AppSettings["restaurant_id"]);


                    APIUSER = Defaults.SettingsList[SettingCode.APIUSER];
                    APIPassword = Defaults.SettingsList[SettingCode.APIPassword];

                    IconList = uof.IconStoreRepository.GetAll().ToList();
                    // string hexString = ConfigurationManager.AppSettings["CashDrawerHardwarePort"];
                    // Int16 num = Int16.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
                    //short port = 0; // 0x48C;
                    //short.TryParse(ConfigurationManager.AppSettings["CashDrawerHardwarePort"], out port);
                    //CashDrawerHardwarePort = port;

                    short port = 0;
                    short.TryParse(Defaults.SettingsList[SettingCode.CashDrawerHardwarePort], out port);
                    CashDrawerHardwarePort = port;

                    int cg = 0;
                    int.TryParse(ConfigurationManager.AppSettings["CASH_GUARD"], out cg);
                    CASH_GUARD = cg == 1 ? true : false;

                    Int16 cgport = 0;//1;
                    Int16.TryParse(ConfigurationManager.AppSettings["CASH_GuardPort"], out cgport);
                    CASH_GuardPort = cgport;

                    DiscountCode = Convert.ToString(ConfigurationManager.AppSettings["DiscountCode"]);
                    MenuPinCode = Convert.ToString(ConfigurationManager.AppSettings["MenuPinCode"]);
                    
                    ReturnCode = Convert.ToString(ConfigurationManager.AppSettings["ReturnCode"]);

                    /*****/
                    int posIdType = 0;
                    int.TryParse(ConfigurationManager.AppSettings["PosIdType"], out posIdType);
                    PosIdType = posIdType == 0 ? PosIdType.UniqueId : PosIdType.TerminalNo;

                    int debugCleanCash = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DebugCleanCash], out debugCleanCash);
                    DebugCleanCash = debugCleanCash == 1 ? true : false;

                    int replacePosIdSpecialChars = 0;
                    int.TryParse(ConfigurationManager.AppSettings["ReplacePosIdSpecialChars"], out replacePosIdSpecialChars);
                    ReplacePosIdSpecialChars = replacePosIdSpecialChars == 1 ? true : false;

                    //New entries of settings
                    int dirbong = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DirectBong], out dirbong);
                    DirectBONG = dirbong == 1 ? true : false;

                    int dirdeposit = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.Deposit], out dirdeposit);
                    Deposit = dirdeposit == 1 ? true : false;

                    int productBold = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.ProductBold], out productBold);
                    ProductBold = productBold == 1 ? true : false;

                    int catBold = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.CategoryBold], out catBold);
                    CategoryBold = catBold == 1 ? true : false;

                    int showPrice = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.ShowPrice], out showPrice);
                    //int.TryParse(ConfigurationManager.AppSettings["ShowPrice"], out showPrice);
                    ShowPrice = showPrice == 1 ? true : false;

                    int tableGrid = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.TableView], out tableGrid);
                    //int.TryParse(ConfigurationManager.AppSettings["TableView"], out tableGrid);
                    ShowTableGrid = tableGrid == 1 ? true : false;

                    int disableCreditCard = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DisableCreditCard], out disableCreditCard);
                    DisableCreditCard = disableCreditCard == 1 ? true : false;

                    int disableCashButton = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DisableCashButton], out disableCashButton);
                    DisableCashButton = disableCashButton == 1 ? true : false;

                    int disableSwishButton = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.DisableSwishButton], out disableSwishButton);
                    DisableSwishButton = disableSwishButton == 1 ? true : false;

                    int showCommentsButton = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.ShowComments], out showCommentsButton);
                    ShowComments = showCommentsButton == 1 ? true : false;
                    
                    int signatureOnReturnReceipt = 0;
                    int.TryParse(ConfigurationManager.AppSettings["SignatureOnReturnReceipt"], out signatureOnReturnReceipt);
                    SignatureOnReturnReceipt = signatureOnReturnReceipt == 1 ? true : false;

                    int tipStatus = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.TipStatus], out tipStatus);
                    TipStatus = tipStatus == 1 ? true : false;

                    int askForPrintInvoice = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.AskForPrintInvoice], out askForPrintInvoice);
                    AskForPrintInvoice = askForPrintInvoice == 1 ? true : false;

                    int bongAlert = 0;
                    int.TryParse(Defaults.SettingsList[SettingCode.ShowBongAlert], out bongAlert);
                    //int.TryParse(ConfigurationManager.AppSettings["ShowBongAlert"], out bongAlert);
                    ShowBongAlert = bongAlert == 1 ? true : false;

                   
                    SlideShowURL = Defaults.SettingsList[SettingCode.SlideShowURL];
                    //SlideShowURL = ConfigurationManager.AppSettings["SlideShowURL"];

                }
                //    ClientId = "prcenter_eproducts";
                //   ClientPassword = "550561";

                RegionInfo = new RegionInfo(region);

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
        }

        public static void InitCleanCash(Outlet Outlet, Terminal Terminal)
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Srv4Pos.PostCCU"]))
                CleanCashConfiguraiton.CleanCashBaseUrl = ConfigurationManager.AppSettings["Srv4Pos.PostCCU"];
            else
                CleanCashConfiguraiton.CleanCashBaseUrl = "";

            if (!string.IsNullOrEmpty(Outlet.TaxDescription))
                CleanCashConfiguraiton.TaxDescription = Outlet.TaxDescription;
            else
                CleanCashConfiguraiton.TaxDescription = "";

            if (!string.IsNullOrEmpty(Terminal.CCUData))
                CleanCashConfiguraiton.CCUData = Terminal.CCUData;
            else
                CleanCashConfiguraiton.CCUData = "";

            if (!string.IsNullOrEmpty(Terminal.UniqueIdentification))
                CleanCashConfiguraiton.UniqueIdentification = Terminal.UniqueIdentification;
            else
                CleanCashConfiguraiton.UniqueIdentification = "";
        }

        #region Properties

        public static CompanyInfo CompanyInfo { get; set; }
        public static Terminal Terminal { get; set; }
        public static Outlet Outlet { get; set; }
        public static OutletUser User { get; set; }
        public static string ClientId { get; set; }
        public static string ClientPassword { get; set; }
        public static string ClientUserId { get; set; }
        public static string ClientWebURL { get; set; }
        public static ConcurrentDictionary<SettingCode, string> SettingsList { get; set; }
        public static OrderEntryType OrderEntryType { get; set; }
        public static SaleType SaleType { get; set; }
        public static bool TipStatus { get; set; }
        public static bool Takeaway { get; set; }
        public static bool DirectCash { get; set; }
        public static bool DirectCard { get; set; }
        public static bool BONG { get; set; }
        public static bool IsOpenOrder { get; set; }
        public static bool ShowTableGrid { get; set; }
        public static bool DisableCreditCard { get; set; }
        public static bool DisableCashButton { get; set; }        
        public static bool DisableSwishButton { get; set; }
        public static bool ShowComments { get; set; }
        public static bool SignatureOnReturnReceipt { get; set; }
        
        public static bool ShowEmployeeLog { get; set; }
        public static bool LogoEnable { get; set; }
        public static bool ShowSwishButton { get; set; }
        public static bool ShowStudentCardButton { get; set; }
        public static bool BongByProduct { get; set; }
        public static bool OrderNoOnBong { get; set; }

        public static RegionInfo RegionInfo { get; set; }
        public static int CategoryLines { get; set; }
        public static int ItemsLines { get; set; }
        public static RunningMode RunningMode { get; set; }
        public static ScreenResulution ScreenResulution { get; set; }
        public static string SlideShowURL { get; set; }
        public static string PrinterName { get; set; }
        public static List<IconStore> IconList { get; set; }
        public static List<PaymentType> PaymentTypes { get; set; }
        public static List<Category> Categories { get; set; }
        public static List<Product> Products { get; set; }
        public static List<ItemCategory> ItemCategories { get; set; }
        public static List<Printer> Printers { get; set; }
        public static PerformanceLog PerformanceLog { get; set; }
        public static long ReceiptCounter { get; set; }
        public static bool ShowBongAlert { get; set; }

        public static bool AskForPrintInvoice { get; set; }
        public static bool ShowExternalOrder { get; set; }
        public static string M2MqttService { get; set; }
        public static string ExternalOrderAPI { get; set; }
        public static string ExternalAPIUSER { get; set; }
        public static string ExternalAPIPassword { get; set; }
        public static string APIUSER { get; set; }
        public static string APIPassword { get; set; }
        public static bool IsClient { get; set; }
        public static bool IsDallasKey { get; set; }
        public static string DallasKey { get; set; }
        public static int RootCategoryId { get; set; }
        public static bool TableNeededOnBong { get; set; }
        public static PriceMode PriceMode { get; set; }
        public static int NightStartHour { get; set; }
        public static int NightEndHour { get; set; }
        public static bool DualPriceMode { get; set; }
        public static bool DeviceLog { get; set; }
        public static bool PricePolicy { get; set; }

        public static bool BongCounter { get; set; }

        public static bool DailyBongCounter { get; set; }
        public static bool MultiKitchen { get; set; }


        public static bool ShowPrice { get; set; }
        public static bool CreditNote { get; set; }
        public static bool ElveCard { get; set; }
        public static bool BeamPayment { get; set; }
        public static bool DigitOnly { get; set; }
        public static string PrinterIP { get; set; }
        public static string CustomerId { get; set; }
        public static string LUEMqttService { get; set; }
        public static string restaurant_group_id { get; set; }
      //  public static string restaurant_id { get; set; }

        public static bool EnableExternalNetworking { get; set; }
        public static bool EnableCheckoutLog { get; set; }
        public static List<CGWarning> CGWarnings { get; set; }
        public static bool DebugCleanCash { get; set; }
        public static bool ReplacePosIdSpecialChars { get; set; }
        public static bool DirectMobileCard { get; set; }
        public static bool EnableMqtt { get; set; }
        public static int InvoiceDueDays { get; set; }
        public static PosIdType PosIdType = PosIdType.UniqueId;

        public static string DiscountCode { get; set; }
        public static string MenuPinCode { get; set; }
        public static string ReturnCode { get; set; }

        public static bool CASH_GUARD { get; set; }
        public static int CASH_GuardPort { get; set; }
        public static bool OnlineCash { get; set; }

        public static bool CategoryBold { get; set; }
        public static bool ProductBold { get; set; }
        public static bool DirectBONG { get; set; }
        public static bool Deposit { get; set; }
         


        #endregion
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
        //Spanish = 3,
        Arabic = 4,
        //URDU = 5
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
        SR_1366X768 = 2
    }
    public enum PosIdType
    {
        UniqueId = 0,
        TerminalNo = 1
    }
    public enum CleanCashType
    {
        DUMMY = 1,
        CLEAN_CASH = 2,
        CLEAN_CASH2 = 3
    }




    public class PerformanceLog : List<string>
    {
        public void AddEntry(string value)
        {
            Add($"{value} -> {DateTime.Now:yyyy-MM-dd_hh-mm-ss-fff}");
        }
    }

    public class CGWarning
    {
        public CGWarning(DateTime dt, string desc)
        {
            CreatedOn = dt;
            Description = desc;
        }
        public DateTime CreatedOn { get; set; }
        public string Description { get; set; }

        public string Text { get { return CreatedOn + ": " + Description + "\n"; } }
    }
}