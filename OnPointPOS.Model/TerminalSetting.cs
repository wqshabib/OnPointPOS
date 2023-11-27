using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class TerminalSetting
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public TerminalSettingCode Code { get; set; }
        public string Value { get; set; }
        public Guid OutletId { get; set; }
        public Guid TerminalId { get; set; }
        public DateTime Updated { get; set; }
    }
    public enum TerminalSettingCode
    {
        //API URL
        SyncAPI = 0,
        APIUSER = 1,
        APIPassword = 2,      
        ShowExternalOrder = 3,
        ExternalOrderAPI = 4,
        ExternalAPIUSER = 5,
        ExternalAPIPassword = 6,       
        ExternalOrderMqttService = 7,
        Restaurant_group_id = 8,
        Restaurant_id = 9,
        EnableMqtt = 10,
        M2MqttService = 11,
        SlideShowURL = 12,
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
        ReplacePosIdSpecialChars = 27,
        //GUI Setting
      
        Language = 31,
        Currency = 32,
        SaleType = 33,
        CategoryLines = 34,
        ItemLines = 35,
        TableView = 36,
        Takeaway = 37,
        DirectCash = 38,
        DirectCard =39,  
        ShowSwishButton = 40,
        ShowStudentCardButton = 41,
        CustomerView = 42,
        CustomerOrderInfo = 43,
        CreditNote = 44,
        BeamPayment = 45,
        EmployeeLog = 46,
        DigitOnly = 47,
        //Print Setting
        LogoEnable = 51,
        ShowBongAlert = 52,
        BongNormalFont = 53,
        BONG =54,
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
        RunningMode=70,
        DeviceLog = 71,
        PricePolicy = 72,
        ShowPrice = 73,
        EnableExternalNetworking = 74,       
        CustomerId = 75,      
        HidePaymentButton = 76,
        InvoiceDueDays = 77,
        DiscountCode = 78,
        EnableCheckoutLog = 79
    }
}
