using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Setting : BaseEntity
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public SettingType Type { get; set; }
        public SettingCode Code { get; set; }
        public string Value { get; set; }
        public Guid TerminalId { get; set; }
        public Guid OutletId { get; set; }
        public int Sort { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }

    public enum SettingCode
    {

        //API URL
        SyncAPIURL = 0,
        APIUSER = 1,
        APIPassword = 2,
        ShowExternalOrder = 3,
        ExternalOrderAPI = 4,
        ExternalAPIUSER = 5,
        ExternalAPIPassword = 6,
        ExternalOrderMqttService = 7,
        Restaurant_group_id = 8,
        // Restaurant_id = 9,
        AskForPrintInvoice = 9,
        EnableMqtt = 10,
        M2MqttService = 11,
        Last_Executed = 12,
        //Hardware
        ControlUnitType = 15,
        ControlUnitConnectionString = 16,
        CashDrawerType = 17,
        CashDrawerHardwarePort = 18,
        CASH_GUARD = 19,
        CASH_GuardPort = 20,
        PaymentDeviceType = 21,
        PaymentDevicConnectionString = 22,
        ScaleType = 23,
        SCALEPORT = 24,
        PosIdType = 25,
        DebugCleanCash = 26,
        //ReplacePosIdSpecialChars = 27,
        ShowComments = 27,
        //Terminal GUI Setting
        Language = 31,
        Currency = 32,
        CurrencySymbol = 33,
        SaleType = 34,
        CategoryLines = 35,
        ItemLines = 36,
        TableView = 37,
        Takeaway = 38,
        DirectCash = 39,
        DirectCard = 40,
        ShowSwishButton = 41,
        ShowStudentCardButton = 42,
        CustomerView = 43,
        CustomerOrderInfo = 44,
        CreditNote = 45,
        BeamPayment = 46,
        EmployeeLog = 47,
        DigitOnly = 48,
        ShowPrice = 49,
        HidePaymentButton = 50,
        //Print Setting
        LogoEnable = 51,
        ShowBongAlert = 52,
        BongNormalFont = 53,
        BONG = 54,
        BongByProduct = 55,
        TableNeededOnBong = 56,
        DailyBongCounter = 57,
        BongCounter = 58,
        MultiKitchen = 59,
        OrderNoOnBong = 60,
        //Misc Setting
        IsClient = 65,
        DallasKey = 66,
        NightStartHour = 67,
        NightEndHour = 68,
        DualPriceMode = 69,
        RunningMode = 70,
        DeviceLog = 71,
        PricePolicy = 72,
        EnableExternalNetworking = 73,
        CustomerId = 74,
        SlideShowURL = 75,
        InvoiceDueDays = 76,
        DiscountCode = 77,
        EnableCheckoutLog = 78,
        OrderEntryType = 79,
        AccountNumber = 80,
        PaymentReceiver = 81,
        FakturaReference = 82,
        //New Settings/Configuration Fields into Settings table
        CategoryBold = 84,
        ProductBold = 85,
        DirectBong = 86,
        ClientSettingsProvider = 87,
        TerminalId = 88,
        ElveCard = 89,
        DisableCreditCard = 90,
        TipStatus = 91,
        DisableCashButton = 92,
        Deposit = 93,
        ShowCashierLogin = 190,
        ShowDirectCheckout = 191,
        ShowCustomerCheckout = 192,
        MinStockLevel = 193,
        PrinterLogo = 205,
        BackupDB = 210,
        ForcedSyncFrom = 211,
        ForcedSyncTo = 212,
        SoftPayStatus = 230,
        SoftPayId = 231,
        SoftPayMerchant = 232,
        SoftIntegratorCredentials = 233,
        AutoCloseTerminal = 234,
        AutoCloseTerminalTime = 235,
        DisableSwishButton = 236
    }

    public enum SettingType
    {
        MiscSettings = 0,
        ExternalSettings = 1,
        HardwareSettings = 2,
        TerminalSettings = 3,
        PrintSettings = 4,
        AdminSettings = 5,
        BoldSettings = 6
    }
}
