using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils
{
    public class APIClient
    {

        private string _localConnectionString = "";
        public APIClient(string connectionString)
        {
            _localConnectionString = connectionString;
        }
        private IUnitOfWork CreateLocalUnitOfWork()
        {
            return new UnitOfWork(new ApplicationDbContext(_localConnectionString));
        }
        public List<OutletInfo> GetOutlets()
        {
            using (IUnitOfWork uof = CreateLocalUnitOfWork())
            {
                try
                {
                    var outletRepo = uof.OutletRepository;
                    return outletRepo.GetAll().Where(o => o.IsDeleted == false).Select(o => new OutletInfo
                    {
                        Id = o.Id,
                        Name = o.Name

                    }).ToList();
                }
                catch (Exception ex)
                {
                    return new List<OutletInfo>();
                }

            }
        }
        public ReportViewModel GetTodaySale()
        {
            ReportViewModel model = new ReportViewModel();
            try
            {
                List<SaleDetail> catSales = new List<SaleDetail>();

                DateTime date = DateTime.Now.Date;
                int today = date.Day;
                int year = date.Year;
                int month = date.Month;
                DateTime TodayEndDate = Convert.ToDateTime(year + "-" + month + "-" + today + "  11:59:00 PM");
                var dtFrom = new DateTime(year, month, 1);
                var lastDate = dtFrom.AddMonths(1).AddDays(-1).Day;
                if (today > lastDate)
                    today = lastDate;
                var dtTo = Convert.ToDateTime(year + "-" + month + "-" + today + "  11:59:00 PM");

                string query = @"SELECT   OrderMaster.InvoiceDate as SaleDate,  (OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount as GrossTotal,(OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100)-ItemDiscount) as NetTotal,Outlet.Id as OutletId, Outlet.Name as OutletName
FROM            OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId
						 INNER JOIN Outlet ON OrderMaster.OutletId = Outlet.Id
						Where OrderMaster.InvoiceGenerated=1  and OrderDetail.Active=1 AND  OrderDetail.ItemType=0  AND (OrderMaster.InvoiceDate between '" + dtFrom + "' AND '" + dtTo + "') ";

                var monthlySale = new List<OutletSale>();
                using (ApplicationDbContext db = new ApplicationDbContext(_localConnectionString))
                {
                    var conn = db.Database.Connection;
                    conn.Open();
                    IDbCommand command = new SqlCommand(query);
                    command.Connection = conn;

                    //command.CommandType = CommandType.Text;
                    //command.CommandText = query;


                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        monthlySale.Add(new OutletSale
                        {
                            OutletName = Convert.ToString(dr["OutletName"]),
                            GrossTotal = Convert.ToDecimal(dr["GrossTotal"]),
                            NetTotal = Convert.ToDecimal(dr["NetTotal"]),
                            SaleDate = Convert.ToDateTime(dr["SaleDate"]),
                            OutletId = Guid.Parse(dr["OutletId"].ToString())

                        });
                    }
                    dr.Dispose();

                }
                var dailySale = monthlySale.Where(o => o.SaleDate >= date && o.SaleDate <= TodayEndDate).ToList();
                DailySale dailySales = new DailySale();
                List<OutletSale> outletDailySales = new List<OutletSale>();
                var groups = dailySale.GroupBy(o => new { o.OutletId });
                foreach (var grp in groups)
                {
                    var d = grp.First();
                    var sum = grp.Sum(s => s.GrossTotal);
                    var netsum = grp.Sum(s => s.NetTotal);
                    var sale = new OutletSale
                    {
                        GrossTotal = sum,
                        NetTotal = netsum,
                        OutletName = d.OutletName
                    };
                    outletDailySales.Add(sale);
                }
                dailySales.OutletSale = outletDailySales;
                dailySales.GrandTotal = outletDailySales.Sum(s => s.GrossTotal);
                model.DailySale = dailySales;

                var monthaly = new MonthlySale();
                var monthlygroups = monthlySale.GroupBy(o => new { o.OutletId });
                List<OutletSale> outletmonthlySales = new List<OutletSale>();
                foreach (var grp in monthlygroups)
                {
                    var d = grp.First();
                    var sum = grp.Sum(s => s.GrossTotal);
                    var netsum = grp.Sum(s => s.NetTotal);
                    var sale = new OutletSale
                    {
                        GrossTotal = sum,
                        NetTotal = netsum,
                        OutletName = d.OutletName
                    };
                    outletmonthlySales.Add(sale);
                }
                monthaly.OutletSale = outletmonthlySales;
                monthaly.GrandTotal = outletmonthlySales.Sum(s => s.GrossTotal);
                monthaly.SaleMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(DateTime.Now.Month);
                model.MonthlySale = monthaly;


                // Get Products


            }
            catch (Exception ex)
            {


                model = new ReportViewModel();
                model.DailySale = new DailySale();
                model.MonthlySale = new MonthlySale();
                return model;
            }



            return model;


        }
        public ReportViewModel GetSaleByMonth(int month)
        {
            ReportViewModel model = new ReportViewModel();
            try
            {
                List<SaleDetail> catSales = new List<SaleDetail>();

                DateTime date = DateTime.Now.Date;
                int today = date.Day;
                int year = date.Year;
                var dtFrom = new DateTime(year, month, 1);
                var lastDate = dtFrom.AddMonths(1).AddDays(-1).Day;
                var dtTo = Convert.ToDateTime(year + "-" + month + "-" + lastDate + "  11:59:00 PM");
                if (today > lastDate)
                    today = lastDate;
                DateTime TodayEndDate = Convert.ToDateTime(year + "-" + month + "-" + today + "  11:59:00 PM");
                string query = @"SELECT   OrderMaster.InvoiceDate as SaleDate,  (OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount as GrossTotal,OrderMaster.OutletId,(OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100)-ItemDiscount) as NetTotal,OrderMaster.OutletId,(select Top 1 Name from Outlet Where id=OrderMaster.OutletId) as OutletName
FROM            OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId
						Where OrderMaster.InvoiceGenerated=1  and OrderDetail.Active=1 AND  OrderDetail.ItemType=0  AND (OrderMaster.InvoiceDate between '" + dtFrom + "' AND '" + dtTo + "') ";

                var monthlySale = new List<OutletSale>();
                using (ApplicationDbContext db = new ApplicationDbContext(_localConnectionString))
                {
                    var conn = db.Database.Connection;
                    conn.Open();

                    IDbCommand command = new SqlCommand(query);
                    command.Connection = conn;
                    //command.CommandType = CommandType.Text;
                    //command.CommandText = query;


                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        monthlySale.Add(new OutletSale
                        {
                            OutletName = Convert.ToString(dr["OutletName"]),
                            GrossTotal = Convert.ToDecimal(dr["GrossTotal"]),
                            NetTotal = Convert.ToDecimal(dr["NetTotal"]),
                            SaleDate = Convert.ToDateTime(dr["SaleDate"]),
                            OutletId = Guid.Parse(dr["OutletId"].ToString())

                        });
                    }
                    dr.Dispose();

                }
                var dailySale = monthlySale.Where(o => o.SaleDate >= date && o.SaleDate <= TodayEndDate).ToList();
                DailySale dailySales = new DailySale();
                List<OutletSale> outletDailySales = new List<OutletSale>();
                var groups = dailySale.GroupBy(o => new { o.OutletId });
                foreach (var grp in groups)
                {
                    var d = grp.First();
                    var sum = grp.Sum(s => s.GrossTotal);
                    var netsum = grp.Sum(s => s.NetTotal);
                    var sale = new OutletSale
                    {
                        GrossTotal = sum,
                        NetTotal = netsum,
                        OutletName = d.OutletName,
                        OutletId = d.OutletId,
                        SaleDate = d.SaleDate
                    };
                    outletDailySales.Add(sale);
                }
                dailySales.OutletSale = outletDailySales;
                dailySales.GrandTotal = outletDailySales.Sum(s => s.GrossTotal);
                model.DailySale = dailySales;

                var monthaly = new MonthlySale();
                var monthlygroups = monthlySale.GroupBy(o => new { o.OutletId });
                List<OutletSale> outletmonthlySales = new List<OutletSale>();
                foreach (var grp in monthlygroups)
                {
                    var d = grp.First();
                    var sum = grp.Sum(s => s.GrossTotal);
                    var netsum = grp.Sum(s => s.NetTotal);
                    var sale = new OutletSale
                    {
                        GrossTotal = sum,
                        NetTotal = netsum,
                        OutletName = d.OutletName,
                        OutletId = d.OutletId,
                        SaleDate = d.SaleDate
                    };
                    outletmonthlySales.Add(sale);
                }
                monthaly.OutletSale = outletmonthlySales;
                monthaly.GrandTotal = outletmonthlySales.Sum(s => s.GrossTotal);
                monthaly.SaleMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);// CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                model.MonthlySale = monthaly;


                // Get Products


            }
            catch (Exception ex)
            {


                model = new ReportViewModel();
                model.DailySale = new DailySale();
                model.MonthlySale = new MonthlySale();
                return model;
            }



            return model;
        }

        public MonthlyCategorySale GetCurrentMonthSale()
        {
            return GetMonthlyReportData(default(Guid), DateTime.Now.Year, DateTime.Now.Month);
        }

        public DailyCategorySale GetDailyReportData(Guid outletId, DateTime dt)
        {

            DailyCategorySale model = new DailyCategorySale();
            try
            {
                List<SaleDetail> catSales = new List<SaleDetail>();
                var dtFrom = dt.AddDays(-1);
                var dtTo = dt;
                string query = @"SELECT    cast(OrderMaster.CreationDate as date) as SaleDay,DATEPART(HOUR,OrderMaster.CreationDate) as SaleHour,     OrderMaster.CreationDate, (OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount as GrossTotal,(OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100)-ItemDiscount) as NetTotal, Category.Name
FROM            ItemCategory INNER JOIN
                         Item ON ItemCategory.ItemId = Item.Id INNER JOIN
                         Category ON ItemCategory.CategoryId = Category.Id INNER JOIN
                         OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId ON Item.Id = OrderDetail.ItemId
						Where OrderMaster.Status=13  and OrderDetail.Active=1 AND  OrderDetail.ItemType=0 AND (OrderMaster.CreationDate between '" + dtFrom + "' AND '" + dtTo + "') ";
                if (outletId != default(Guid))
                    query = query + " AND OrderMaster.OutletId='" + outletId + "'";

                using (ApplicationDbContext db = new ApplicationDbContext(_localConnectionString))
                {
                    var conn = db.Database.Connection;
                    conn.Open();

                    IDbCommand command = new SqlCommand(query);
                    command.Connection = conn;
                    //command.CommandType = CommandType.Text;
                    //command.CommandText = query;


                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        catSales.Add(new SaleDetail
                        {
                            GroupName = Convert.ToString(dr["Name"]),
                            GrossTotal = Convert.ToDecimal(dr["GrossTotal"]),
                            NetTotal = Convert.ToDecimal(dr["NetTotal"]),
                            CreationDate = Convert.ToDateTime(dr["CreationDate"]),
                            SaleDay = Convert.ToDateTime(dr["SaleDay"]),
                            SaleHour = Convert.ToInt16(dr["SaleHour"])

                        });
                    }
                    dr.Dispose();
                    conn.Close();
                }
                var groups = catSales.GroupBy(o => new { o.GroupName });
                List<SaleDetail> categorySales = new List<SaleDetail>();
                foreach (var grp in groups)
                {
                    var d = grp.First();
                    var sum = grp.Sum(s => s.GrossTotal);
                    var netsum = grp.Sum(s => s.NetTotal);
                    var sale = new SaleDetail
                    {
                        GrossTotal = Math.Round(sum, 2),
                        NetTotal = Math.Round(netsum, 2),
                        GroupName = d.GroupName
                    };
                    categorySales.Add(sale);
                }
                model.Categories = categorySales;
                model.GrandTotal = categorySales.Sum(s => s.GrossTotal);

                var hourlyGroups = catSales.GroupBy(o => new { o.SaleHour });
                List<SaleDetail> hourlySales = new List<SaleDetail>();
                hourlySales = GetHourlySale(outletId, dt);
                //foreach (var grp in hourlyGroups)
                //{
                //    var d = grp.First();
                //    var sum = grp.Sum(s => s.GrossTotal);
                //    var netsum = grp.Sum(s => s.NetTotal);
                //    var sale = new SaleDetail
                //    {
                //        GrossTotal = sum,
                //        NetTotal = netsum,
                //        GroupName = d.SaleHour + ".00",
                //        SaleHour = d.SaleHour
                //    };
                //    hourlySales.Add(sale);
                //}
                model.GrandTotal = catSales.Sum(g => g.GrossTotal);
                model.NetTotal = catSales.Sum(g => g.NetTotal);
                model.HourlySales = hourlySales.OrderBy(o => o.SaleHour).ToList();
                //model. = hourlySales.Sum(s => s.GrossTotal);
            }
            catch (Exception ex)
            {


                throw;
            }



            return model;
        }
        public List<SaleDetail> GetHourlySale(Guid outletId, DateTime date)
        {

            DateTime dt = Convert.ToDateTime(date.Year + "-" + date.Month + "-" + date.Day + "  11:59:00 PM");
            List<SaleDetail> hourlySales = new List<SaleDetail>();
            List<SaleDetail> outletSales = new List<SaleDetail>();
            var dtFrom = dt.AddDays(-1);
            var dtTo = dt;
            string query = @"SELECT    cast(OrderMaster.InvoiceDate as date) as SaleDay,DATEPART(HOUR,OrderMaster.InvoiceDate) as SaleHour,     OrderMaster.InvoiceDate,  (OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount as GrossTotal,(OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100)-ItemDiscount) as NetTotal
FROM          
                         OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId 
						Where OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1 AND  OrderDetail.ItemType=0 AND (OrderMaster.InvoiceDate between '" + dtFrom + "' AND '" + dtTo + "') ";
            if (outletId != default(Guid))
                query = query + " AND OrderMaster.OutletId='" + outletId + "'";

            using (ApplicationDbContext db = new ApplicationDbContext(_localConnectionString))
            {
                var conn = db.Database.Connection;
                conn.Open();

                IDbCommand command = new SqlCommand(query);
                command.Connection = conn;
                //command.CommandType = CommandType.Text;
                //command.CommandText = query;


                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    outletSales.Add(new SaleDetail
                    {
                        GroupName = Convert.ToString(dr["SaleHour"]),
                        GrossTotal = Convert.ToDecimal(dr["GrossTotal"]),
                        NetTotal = Convert.ToDecimal(dr["NetTotal"]),
                        CreationDate = Convert.ToDateTime(dr["InvoiceDate"]),
                        SaleDay = Convert.ToDateTime(dr["SaleDay"]),
                        SaleHour = Convert.ToInt16(dr["SaleHour"])

                    });
                }
                dr.Dispose();
                conn.Close();
            }

            var hourlyGroups = outletSales.GroupBy(o => new { o.SaleHour });

            foreach (var grp in hourlyGroups)
            {
                var d = grp.First();
                var sum = grp.Sum(s => s.GrossTotal);
                var netsum = grp.Sum(s => s.NetTotal);
                var sale = new SaleDetail
                {
                    GrossTotal = sum,
                    NetTotal = netsum,
                    GroupName = d.SaleHour + ".00",
                    SaleHour = d.SaleHour
                };
                hourlySales.Add(sale);
            }
            return hourlySales.OrderByDescending(o => o.SaleHour).ToList();
        }



        public MonthlyCategorySale GetMonthlyReportData(Guid outletId, int year, int month)
        {

            MonthlyCategorySale model = new MonthlyCategorySale();
            try
            {
                List<SaleDetail> monthSales = new List<SaleDetail>();

                var dtFrom = new DateTime(year, month, 1);
                var lastDate = dtFrom.AddMonths(1).AddDays(-1).Day;
                var dtTo = Convert.ToDateTime(year + "-" + month + "-" + lastDate + "  11:59:00 PM");
                List<Outlet> outlets = new List<Outlet>();

                using (ApplicationDbContext db = new ApplicationDbContext(_localConnectionString))
                {
                    var conn = db.Database.Connection;
                    conn.Open();

                    outlets = db.Outlet.ToList();
                    string query = "exec SP_MonthlyReport '" + dtFrom + "','" + dtTo + "'";
                    IDbCommand command = new SqlCommand(query);
                    command.Connection = conn;
                    //command.CommandType = CommandType.Text;
                    //command.CommandText = query;


                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        monthSales.Add(new SaleDetail
                        {
                            DataType = Convert.ToString(dr["DataType"]),
                            GroupName = Convert.ToString(dr["Name"]),
                            GrossTotal = Convert.ToDecimal(dr["GrossTotal"]),
                            NetTotal = Convert.ToDecimal(dr["NetTotal"]),
                            SaleDay = dr["SaleDay"] != DBNull.Value ? Convert.ToDateTime(dr["SaleDay"]).Date : DateTime.Now,
                            OutletId = dr["OutletId"] != DBNull.Value ? Guid.Parse(dr["OutletId"].ToString()) : default(Guid)


                        });
                    }
                    dr.Dispose();
                    conn.Close();
                }

                if (outletId == default(Guid))
                {
                    model.Categories = monthSales.Where(c => c.DataType == "CategorySale").ToList();
                    model.DailySales = monthSales.Where(d => d.DataType == "SaleDay").Select(d => new SaleDetail
                    {

                        GrossTotal = d.GrossTotal,
                        NetTotal = d.NetTotal,
                        SaleDay = d.SaleDay,
                        GroupName = d.SaleDay.ToShortDateString(),
                        OutletId = d.OutletId

                    }).OrderBy(o => o.SaleDay).ToList();
                    model.GrandTotal = model.DailySales.Sum(g => g.GrossTotal);
                    model.NetTotal = model.DailySales.Sum(g => g.NetTotal);
                }
                else
                {
                    model.Categories = monthSales.Where(c => c.DataType == "CategorySale" && c.OutletId == outletId).ToList();
                    model.DailySales = monthSales.Where(d => d.DataType == "SaleDay" && d.OutletId == outletId).Select(d => new SaleDetail
                    {

                        GrossTotal = d.GrossTotal,
                        NetTotal = d.NetTotal,
                        SaleDay = d.SaleDay,
                        GroupName = d.SaleDay.ToShortDateString(),
                        OutletId = d.OutletId

                    }).OrderBy(o => o.SaleDay).ToList();
                    model.GrandTotal = model.DailySales.Where(ol => ol.OutletId == outletId).Sum(g => g.GrossTotal);
                    model.NetTotal = model.DailySales.Where(ol => ol.OutletId == outletId).Sum(g => g.NetTotal);
                }
                List<OutletSale> outletDailySales = new List<OutletSale>();
                var groups = model.DailySales.GroupBy(o => new { o.OutletId });
                foreach (var grp in groups)
                {
                    var d = grp.First();
                    var sum = grp.Sum(s => s.GrossTotal);
                    var netsum = grp.Sum(s => s.NetTotal);
                    var sale = new OutletSale
                    {
                        GrossTotal = sum,
                        NetTotal = netsum,
                        OutletId = d.OutletId,
                        OutletName = outlets.FirstOrDefault(o => o.Id == d.OutletId).Name,
                        SaleDate = d.SaleDay
                    };
                    outletDailySales.Add(sale);
                }
                List<SaleDetail> hourlySales = new List<SaleDetail>();
                hourlySales = GetHourlySale(outletId, dtFrom);
                //foreach (var grp in hourlyGroups)
                //{
                //    var d = grp.First();
                //    var sum = grp.Sum(s => s.GrossTotal);
                //    var netsum = grp.Sum(s => s.NetTotal);
                //    var sale = new SaleDetail
                //    {
                //        GrossTotal = sum,
                //        NetTotal = netsum,
                //        GroupName = d.SaleHour + ".00",
                //        SaleHour = d.SaleHour
                //    };
                //    hourlySales.Add(sale);
                //}

                model.OutletSales = outletDailySales.OrderByDescending(o => o.SaleDate).ToList();
                model.HourlySales = hourlySales.OrderByDescending(o => o.SaleHour).ToList();
            }
            catch (Exception ex)
            {


                //  throw;
            }



            return model;
        }




    }
    public class ReportViewModel
    {
        public DailySale DailySale { get; set; }
        public MonthlySale MonthlySale { get; set; }


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
        public List<OutletSale> OutletSales { get; set; }
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

    public class OutletInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
