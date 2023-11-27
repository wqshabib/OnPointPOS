using System;

namespace POSSUM.Model
{
    public class JournalAction:BaseEntity
    {
        public  int Id { get; set; }
        public  string Type { get; set; }
        public  string ActionCode { get; set; }
        public  string Description { get; set; }
        public string Description2 { get; set; }
    }
    public enum JournalActionCode
    {
        NewOrderEntry = 0,
        OrderTypeNew = 1,
        OrderTypeTakeAway = 2,
        OrderTypeReturnTakeAway = 3,
        OrderTypeReturn = 4,
        OrderCancelled = 5,
        OrderComment = 6,
        SwitchedToItemView = 7,
        SwitchedToTableView = 8,
        ItemAdded = 11,
        ReturnItemAdded = 12,
        ItemDeleted = 13,
        ItemAddedWithCustomPrice = 14,
        ItemDiscountAdded = 15,
        ItemPriceChanged = 16,
        ItemPriceReverted = 17,
        ItemMoved=18,
        OrderTableSelected = 20,
        OpenOrderSelected = 21,
        DirectCashPaymentStarted = 22,
        DirectCardPaymentStarted = 23,
        PaymentScreenNavigation = 30,
        PaymentScreenCancelled = 31,
        PaymentTerminalWindowOpen = 32,
        PaymentTerminalWindowCancel = 33,
        PaymentTerminalWindowClosed = 34,
        PaymentDeviceTotal = 35,
        ReceiptGenerating = 197,
        ReceiptGenerated = 198,
        ReceiptFail = 199,
        ReceiptPrinted = 200,
        ReceiptCopyPrinted = 201,
        ReceiptPrintedForReturnOrder = 202,
        ReceiptPrintedViaTrainingMode = 203,
        ReceiptCopyPrintedViaTrainingMode = 204,
        ReceiptPrintedForReturnOrderViaTrainingMode = 205,
        ReceiptKitchen = 210,
        PrintPerforma = 220,
        ReceiptViewed = 230,
        ReceiptViewedViaTrainingMode = 231,
        OrderCashPayment = 300,
        OrderCreditcardPayment = 301,
        OrderReturnCashPayment = 302,
        OrderSwishPayment = 303,
        OrderAccountPayment = 304,
        OrderCouponPayment = 305,
        OrderMobileTerminalPayment = 306,
        ReportXViewed = 400,
        ReportZViewed = 403,
        ReportXPrinted = 401,
        ReportZPrinted = 402,
        LoggedIn = 500,
        LoggedInFailure = 502,
        LoggedOut = 501,
        OpenCashDrawer = 600,
        TerminalClosed = 700,
        TerminalOpened = 701,
        DirectSwishPaymentStarted = 702,
    }
    public enum JournalActionType
    {
        Order,
        Report,
        Payment,
        CashDrawer,
        Invoice,
        Authentication,
        Terminal
    }
}
