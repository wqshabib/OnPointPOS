using DotNet.Highcharts;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace POSSUM.Web.Models
{
    public class ReportViewModel
    {
        public DailySale DailySale { get; set; }
        public MonthlySale MonthlySale { get; set; }
        public string SaleMonth { get; set; }
        public List<ItemViewModel> Products { get; set; }
        public Highcharts SalesByCategory { get; set; }
        public Highcharts SalesByProduct { get; set; }

    }
    public class DailySale
    {
        public string Description { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal NetTotal { get; set; }
        public List<OutletSale> OutletSale { get; set; }
    }
    public class MonthlySale
    {
        public string Description { get; set; }
        public string SaleMonth { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal NetTotal { get; set; }
        public List<OutletSale> OutletSale { get; set; }
    }
    public class OutletSale
    {
        public Guid OutletId { get; set; }
        public DateTime SaleDate { get; set; }
        public string OutletName { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal NetTotal { get; set; }
    }
    public class DailyCategorySale
    {
        public string Description { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal NetTotal { get; set; }
        public List<SaleDetail> Categories { get; set; }
        public List<SaleDetail> HourlySales { get; set; }
    }
    public class MonthlyCategorySale
    {
        public string Description { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal NetTotal { get; set; }
        public List<SaleDetail> Categories { get; set; }
        public List<SaleDetail> DailySales { get; set; }
        public List<SaleDetail> HourlySales { get; set; }
    }
    public class SaleDetail
    {
        public string GroupName { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal NetTotal { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime SaleDay { get; set; }
        public int SaleHour { get; set; }
        public string DataType { get; internal set; }
        public Guid OutletId { get; set; }
    }




    public class DashboardStats
    {
        public List<SaleTrendResult> GetAllSale { get; set; }
        public List<SaleTrendResult> LstDaySaleTrend { get; set; }
        public List<SaleTrendResult> LstWeekSaleTrend { get; set; }
        public List<SaleTrendResult> LstMonthSaleTrend { get; set; }
        public List<SaleTrendResult> LstYearSaleTrend { get; set; }


        public List<SaleTrendResult> LstDayCostTrend { get; set; }
        public List<SaleTrendResult> LstWeekCostTrend { get; set; }
        public List<SaleTrendResult> LstMonthCostTrend { get; set; }
        public List<SaleTrendResult> LstYearCostTrend { get; set; }


        public decimal Sales { get; set; }
        public decimal SalesOfWeek { get; set; }
        public decimal SalesOfMonth { get; set; }
        public decimal SalesOfYear { get; set; }

        public decimal SalesDifference { get; set; }
        public decimal SalesOfWeekDifference { get; set; }
        public decimal SalesOfMonthDifference { get; set; }
        public decimal SalesOfYearDifference { get; set; }



    }





    public class DashboardPaymentStats
    {
        public List<PaymentData> StatusLstDayCostTrend { get; set; }
        public List<PaymentData> StatusLstWeekCostTrend { get; set; }
        public List<PaymentData> StatusLstMonthCostTrend { get; set; }
        public List<PaymentData> StatusLstYearCostTrend { get; set; }
        public Highcharts SalesByCategory { get; set; }
        public Highcharts SalesByProducts { get; set; }
    }


    public class PaymentData
    {
        public PaymentData()
        {
            SaleTrendResult = new List<SaleTrendResult>();
        }
        public string PaymentType { get; set; }

        public List<Result> Result { get; set; }
        public List<SaleTrendResult> SaleTrendResult { get; set; }
    }



    public class Series
    {
        public string name { get; set; }
        public List<int> data { get; set; }
    }


    public class SaleTrendResult
    {
        public int SortOrder { get; set; }
        public string Key { get; set; }
        public decimal Value1 { get; set; }
        public decimal Value2 { get; set; }
        public int OrderCount { get; set; }
        public double AVO { get; set; }
        public string KeyName { get; set; }

    }

    public class Result
    {
        public DateTime DateTimeKey { get; set; }
        public Guid Id { get; set; }
        public string Key1 { get; set; }
        public int Key { get; set; }
        public int DayKey { get; set; }
        public int YearKey { get; set; }
        public int MonthKey { get; set; }
        public decimal Value { get; set; }
        public int Value1 { get; set; }
        public string KeyName { get; set; }

    }
    public class SalesData
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }

    }








}