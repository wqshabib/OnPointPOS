using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class OrderPrinter : Result
    {
        public string OrderPrinterId { get; set; }
        public List<OrderPrinterAvailability> OrderPrinterAvailability { get; set; }
    }

    [Serializable]
    public class OrderPrinterAvailability : Result
    {
        public string OrderPrinterAvailabilityId { get; set; }
        public string Date { get; set; }
        public string WeekDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int OrderPrinterAvailabilityType { get; set; }
        public string DeliveryStartTime { get; set; }
        public string DeliveryEndTime { get; set; }
    }

    [Serializable]
    public class CompanyOrderPrinterAvailabilityHtml : Result
    {
        public string OrderPrinterAvailabilityList { get; set; }
        public string OrderPrinterAvailabilityLunchList { get; set; }
        public string OrderPrinterAvailabilityALaCarteList { get; set; }
    }

    [Serializable]
    public class CustomerOrderPrinterStatus : Result
    {
        public int OrderPrinterStatus { get; set; }
        public string OrderPrinterStatusText { get; set; }
        public string OrderPrinterId { get; set; }
        public string OrderPrinterName { get; set; }
    }

    [Serializable]
    public class CustomerOrderPrinterAvailabilityForDelivery : Result
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }


    [Serializable]
    public class CompanyOrderPrinterAvailability : Result
    {
        public string OrderPrinterName { get; set; }
        public List<CompanyOrderPrinterAvailabilityLine> CompanyOrderPrinterAvailabilityLines { get; set; }

    }

    [Serializable]
    public class CompanyOrderPrinterAvailabilityLine
    {
        public string Line { get; set; }
    }






}