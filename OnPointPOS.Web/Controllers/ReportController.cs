using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Net;
using System.Diagnostics;
using POSSUM.Data;
using POSSUM.Web.Models;
using POSSUM.Model;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using Microsoft.Reporting.WebForms;
using System.Configuration;
using DotNet.Highcharts;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Enums;
using Newtonsoft.Json;

namespace POSSUM.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class ReportController : MyBaseController
    {
        public ReportController()
        {
            //  uof = PosState.GetInstance().CreateUnitOfWork();
        }
        public ActionResult Index()
        {
            var data = GetReportData1();
            data.SalesByCategory = GetSalesDateByCategory(DateTime.Now.AddDays(-7));
            data.SalesByProduct = GetSalesDateByProduct(DateTime.Now.AddDays(-7));
            return View(data);
        }

        public ActionResult Reportgenerator()
        {
            return View();
        }

        private ReportViewModel GetReportData1()
        {

            ReportViewModel model = new ReportViewModel();
            try
            {
                List<SaleDetail> catSales = new List<SaleDetail>();

                DateTime date = DateTime.Now.Date;
                int today = date.Day;
                int year = date.Year;
                int month = date.Month;
                var dtFrom = new DateTime(year, month, 1);
                var lastDate = dtFrom.AddMonths(1).AddDays(-1).Day;
                var dtTo = year + "-" + month + "-" + lastDate + "  11:59:00 PM";
                DateTime TodayEndDate = date.AddDays(1).Date; //Convert.ToDateTime(year + "-" + month + "-" + today + "  11:59:00 PM");
                string startDate = year + "-" + month + "-" + 1 + "  00:00:00 AM";
                string query = @"SELECT  cast( OrderMaster.InvoiceDate as date) as SaleDate,  sum(((OrderDetail.Direction* OrderDetail.Qty)* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum(((OrderDetail.UnitPrice*(OrderDetail.Qty*OrderDetail.Direction))-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NetTotal,OrderMaster.OutletId,(select top 1 Name from outlet where Id= OrderMaster.OutletId) as OutletName
                           FROM    OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId						
						Where OrderDetail.Active=1 AND  OrderDetail.ItemType<>1 AND (OrderMaster.InvoiceDate between  '" + startDate + "' AND '" + dtTo + "')" +
                        " Group by cast(OrderMaster.InvoiceDate as date),OrderMaster.OutletId ";

                var monthlySale = new List<OutletSale>();

                using (var db = GetConnection)
                {
                    //var selectedoutles = UserOutlets;
                    if (AllowedOutlets != null && AllowedOutlets.Count > 0)
                    {
                        monthlySale = db.Database.SqlQuery<OutletSale>(query)
                            .Where(s => AllowedOutlets.Contains(s.OutletId.ToString())).ToList();
                    }
                    else
                    {
                        var pt = db.PaymentType.ToString();
                        monthlySale = db.Database.SqlQuery<OutletSale>(query).ToList();
                    }
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
                monthaly.SaleMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                model.MonthlySale = monthaly;
                // Get Products
                model.SaleMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);

            }
            catch (Exception ex)
            {


                model = new ReportViewModel();
                model.DailySale = new DailySale();

                // model.OutletSale=new 
                model.MonthlySale = new MonthlySale();
                model.Products = new List<ItemViewModel>();

            }



            return model;
        }


        private ReportViewModel GetReportData()
        {
            try
            {
                using (var db = GetConnection)
                {

                    ReportViewModel model = new ReportViewModel();
                    DateTime date = DateTime.Now.Date;
                    int today = date.Day;
                    int year = date.Year;
                    int month = date.Month;
                    var dtFrom = new DateTime(year, month, 1);
                    var lastDate = dtFrom.AddMonths(1).AddDays(-1).Day;
                    var dtTo = year + "-" + month + "-" + lastDate + "  11:59:00 PM";
                    DateTime TodayEndDate = Convert.ToDateTime(year + "-" + month + "-" + today + "  11:59:00 PM");
                    string startDate = year + "-" + month + "-" + today + "  00:00:00 AM";
                    // var orders = db.OrderMaster.Include("OrderLines").Where(ord => ord.Status == OrderStatus.Completed && (ord.CreationDate >= dtFrom && ord.CreationDate <= dtTo && ord.Outlet != null)).ToList();

                    var monthlySale = new List<OutletSale>();
                    string query = "exec SP_DashBoardSale '" + startDate + "','" + dtTo + "'";
                    monthlySale = db.Database.SqlQuery<OutletSale>(query).ToList();
                    //foreach (var ord in orders)
                    //{
                    //    ord.OrderLines = ord.OrderLines.Where(s => s.Active == 1 && s.ItemType==ItemType.Individual && s.OrderId == ord.Id).ToList();
                    //    monthlySale.Add(new OutletSale
                    //    {
                    //        SaleDate = ord.CreationDate,
                    //        GrossTotal = ord.OrderLines.Sum(sl => (sl.GrossAmountDiscounted())),
                    //        NetTotal = ord.OrderLines.Sum(sl => sl.NetAmount()),
                    //        OutletId = ord.Outlet.Id,
                    //        OutletName = ord.Outlet.Id == default(Guid) ? " " : ord.Outlet.Name
                    //    });
                    //}
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
                    monthaly.SaleMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                    model.MonthlySale = monthaly;



                    return model;
                };
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public ActionResult Daily()
        {

            DailyCategorySale model = GetDailyReportData(default(Guid), DateTime.Now);
            return View(model);
        }
        public ActionResult DailyByCategory(Guid outletId, string date)
        {
            DailyCategorySale model = GetDailyReportData(outletId, Convert.ToDateTime(date + "  11:59:00 PM"));
            return PartialView(model);
        }
        public ActionResult HourlySale(Guid outletId, DateTime date)
        {
            try
            {
                var hourlySales = GetHourlySale(outletId, date);
                return PartialView(hourlySales.OrderBy(o => o.SaleHour).ToList());
            }
            catch (Exception ex)
            {


                throw;
            }

        }
        private List<SaleDetail> GetHourlySale(Guid outletId, DateTime date)
        {

            string dt = date.Year + "-" + date.Month + "-" + date.Day + "  11:59:00 PM";
            List<SaleDetail> hourlySales = new List<SaleDetail>();
            List<SaleDetail> outletSales = new List<SaleDetail>();
            var dtFrom = date.Year + "-" + date.Month + "-" + date.Day + "  00:00:00 AM";

            var dtTo = dt;


            string query = "exec [dbo].[SP_HourlySale] '" + outletId + "','" + dtFrom + "','" + dtTo + "'";

            using (var db = GetConnection)
            {
                outletSales = db.Database.SqlQuery<SaleDetail>(query).ToList();

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
            return hourlySales;
        }
        private DailyCategorySale GetDailyReportData(Guid outletId, DateTime dt)
        {

            DailyCategorySale model = new DailyCategorySale();
            try
            {
                List<SaleDetail> catSales = new List<SaleDetail>();
                var dtTo = dt.Year + "-" + dt.Month + "-" + dt.Day + "  11:59:00 PM";
                string dtFrom = dt.Year + "-" + dt.Month + "-" + dt.Day + "  00:00:00 AM";
                //              

                string query = "exec [dbo].[SP_DailySale] '" + outletId + "','" + dtFrom + "','" + dtTo + "'";
                using (var db = GetConnection)
                {
                    if (AllowedOutlets != null && AllowedOutlets.Count > 0)
                        catSales = db.Database.SqlQuery<SaleDetail>(query).Where(o => AllowedOutlets.Contains(o.OutletId.ToString())).ToList();
                    else
                        catSales = db.Database.SqlQuery<SaleDetail>(query).ToList();

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

                model.HourlySales = hourlySales.OrderBy(o => o.SaleHour).ToList();

            }
            catch (Exception ex)
            {


                throw;
            }



            return model;
        }
        public ActionResult Monthly()
        {
            MonthlyCategorySale model = GetMonthlyReportData(default(Guid), DateTime.Now.Year, DateTime.Now.Month);
            return View(model);
        }
        public ActionResult MonthlyDetail(Guid outletId, int year, int month)
        {
            MonthlyCategorySale model = GetMonthlyReportData(outletId, year, month);
            return PartialView(model);
        }
        private MonthlyCategorySale GetMonthlyReportData(Guid outletId, int year, int month)
        {

            MonthlyCategorySale model = new MonthlyCategorySale();
            try
            {
                List<SaleDetail> monthSales = new List<SaleDetail>();

                var dtFrom = new DateTime(year, month, 1);
                var lastDate = dtFrom.AddMonths(1).AddDays(-1).Day;
                var dtTo = year + "-" + month + "-" + lastDate + "  11:59:00 PM";
                string startDate = year + "-" + month + "-" + 1 + "  00:00:00 AM";

                using (var db = GetConnection)
                {
                    string query = "exec SP_MonthlyReport '" + startDate + "','" + dtTo + "'";
                    //var data=  db.Database.SqlQuery<SaleDetail>(query).ToList();
                    IDbCommand command = new SqlCommand(query);
                    using (var conn = db.Database.Connection)
                    {
                        conn.Open();
                        command.Connection = conn;


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
                    }
                }
                if (outletId == default(Guid))
                {
                    if (AllowedOutlets != null && AllowedOutlets.Count > 0)
                        monthSales = monthSales.Where(c => AllowedOutlets.Contains(c.OutletId.ToString())).ToList();
                    model.Categories = monthSales.Where(c => c.DataType == "CategorySale").ToList();
                    model.DailySales = monthSales.Where(d => d.DataType == "SaleDay").Select(d => new SaleDetail
                    {

                        GrossTotal = d.GrossTotal,
                        NetTotal = d.NetTotal,
                        SaleDay = d.SaleDay,
                        GroupName = d.SaleDay.ToString("yyyy-MM-dd"),
                        OutletId = d.OutletId

                    }).OrderBy(o => o.SaleDay).ToList();
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
                }
                List<SaleDetail> hourlySales = new List<SaleDetail>();
                hourlySales = GetHourlySale(outletId, dtFrom);

                model.GrandTotal = monthSales.Sum(g => g.GrossTotal);
                model.HourlySales = hourlySales.OrderBy(o => o.SaleHour).ToList();
            }
            catch (Exception ex)
            {


                throw;
            }



            return model;
        }

        public JsonResult FillOutlets()
        {
            using (var db = GetConnection)
            {
                List<Outlet> outlets = new List<Outlet>();
                if (AllowedOutlets != null && AllowedOutlets.Count > 0)
                    outlets = db.Outlet.Where(c => c.IsDeleted == false && AllowedOutlets.Contains(c.Id.ToString())).ToList();
                else
                    outlets = db.Outlet.Where(c => c.IsDeleted == false).ToList();

                outlets.Add(new Outlet { Id = default(Guid), Name = "All" });
                return Json(outlets.OrderBy(o => o.Id).ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult FillTerminal()
        {
            using (var db = GetConnection)
            {
                List<TerminalData> terminals = new List<TerminalData>();
                if (AllowedOutlets != null && AllowedOutlets.Count > 0)
                    terminals = db.Terminal.Where(c => c.IsDeleted == false && AllowedOutlets.Contains(c.OutletId.ToString())).Select(t => new TerminalData
                    {
                        Id = t.Id,
                        Name = t.UniqueIdentification + " (Terminal: " + t.TerminalNo + ")" + (!string.IsNullOrEmpty(t.Description) ? " " + t.Description : "")
                    }).ToList();
                else
                    terminals = db.Terminal.Where(c => c.IsDeleted == false).Select(t => new TerminalData
                    {
                        Id = t.Id,
                        Name = t.UniqueIdentification + " (Terminal: " + t.TerminalNo + ")" + (!string.IsNullOrEmpty(t.Description) ? " " + t.Description : "")
                    }).ToList();
                terminals.Add(new TerminalData { Id = default(Guid), Name = Resource.Select });
                return Json(terminals.OrderBy(o => o.Id).ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult FillYears()
        {

            var outlets = new List<Outlet>();
            for (int i = 2015; i <= DateTime.Now.Year; i++)
            {
                outlets.Add(new Outlet { Id = default(Guid), Name = i.ToString() });
            }
            return Json(outlets.OrderBy(o => o.Id).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ZReport()
        {
            using (var db = GetConnection)
            {

                var terminalStatuses = db.TerminalStatusLog.Where(o => o.ReportId != null && o.ReportId != default(Guid)).OrderByDescending(o => o.ActivityDate).ToList();
                return View(terminalStatuses);
            }
        }
        public ActionResult ZReportList(Guid terminalId, int year, int month)
        {

            var dtFrom = new DateTime(year, month, 1).Date;
            var lastDay = dtFrom.AddMonths(1).AddDays(-1).Day;
            var dtTo = new DateTime(year, month, lastDay, 23, 59, 59);


            using (var db = GetConnection)
            {
                var terminalStatuses = db.TerminalStatusLog.Where(o => o.ReportId != null && o.ReportId != default(Guid) && o.Terminal.Id == terminalId && o.ActivityDate >= dtFrom && o.ActivityDate <= dtTo).OrderByDescending(o => o.ActivityDate).ToList();
                return PartialView(terminalStatuses);
            }
        }
        public ActionResult PrintZReport(Guid reportId)
        {

            var info = ReportGenerator.GetReport(reportId, 1, User.Identity.Name, GetConnection);

            return PartialView(info);
        }

        public ActionResult PrintDetailReport(string terminalId, string dtFrom, string dtTo)
        {
            try
            {


                DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
                DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");
                var info = ReportGenerator.PrintDetailReportByDateRange(terminalId, dateFrom, dateTo, User.Identity.Name, GetConnection);
                return PartialView(info);
            }
            catch (Exception ex)
            {

                return PartialView(new ReportInfo());
            }
        }
        public void PrintPDFReport(string terminalId, string dtFrom, string dtTo)
        {
            try
            {


                DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
                DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");
                var b = ReportGenerator.GetPDFReport(terminalId, dateFrom, dateTo, User.Identity.Name, GetConnection);
                //  var byteArray = Encoding.ASCII.GetBytes(b.ToString());
                //  var stream = new MemoryStream(byteArray);

                MemoryStream ms = Converter.HtmlToStream(b, "NIM POS");
                string exportDate = dateFrom.Year.ToString() + dateFrom.Month.ToString() + dateFrom.Day.ToString() + "_" + dateTo.Year.ToString() + dateTo.Month.ToString() + dateTo.Day.ToString();

                string fileName = exportDate;

                string strFileName = string.Format("DetailReport_{0}.pdf", exportDate);
                //  return File(ms, "application/pdf", strFileName);
                if (ms.Length > 0)
                {
                    // return File(ms, "application/pdf", strFileName);
                    //  Open Pdf in browser
                    Response.Clear();
                    Response.AddHeader("Content-Type", "application/pdf");
                    Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", strFileName));

                    Byte[] byteArray = ms.ToArray();
                    Response.BinaryWrite(byteArray);

                    try
                    {
                        Response.End();
                    }
                    catch { }
                }
                //  return File(stream, "application/pdf", fileName + ".pdf");
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public ActionResult SaleByCategory()
        {
            return View();
        }

        public ActionResult LowStockReport()
        {
            return View();
        }

        public ActionResult PrintCategorySale(int catId, string dtFrom, string dtTo)
        {
            try
            {
                DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
                DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");
                var info = ReportGenerator.PrintSaleByCategory(dateFrom, dateTo, catId, User.Identity.Name, GetConnection);
                return PartialView("PrintCategorySale", info);
            }
            catch (Exception ex)
            {
                var info = new ReportInfo
                {
                    Description = ex.Message
                };
                info.ReportRows = new List<ReportRow>();
                return PartialView("PrintCategorySale", info);
            }
        }
        public SaleModel GetCategorySale()
        {
            return new SaleModel();
        }
        public ActionResult GenerateAndEmailZReport(string dtFrom, string dtTo, string terminalId)
        {
            string msg = "";
            try
            {


                ReportViewer MyReportViewer = new ReportViewer();
                SaleDataProvider provider = new SaleDataProvider();

                var userName = User.Identity.Name;
                var reportInfo = provider.GetZReport(dtFrom, dtTo, terminalId, userName, GetConnection);
                var rows = reportInfo.ReportRows;
                MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("ReportData", rows));
                MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ZReport.rdlc");
                StringBuilder b = new StringBuilder();
                string orgNo = string.Format("{0,-10}{1,-32}", Resource.OrgNo + ":", reportInfo.OrgNo);
                b.AppendLine(orgNo);
                string address = reportInfo.Address1;
                if (reportInfo.Address1.Length > 32)
                {
                    string adrs = address.Substring(0, 32);

                    b.AppendLine(string.Format("{0,-10}{1,-32}", Resource.Address, adrs));
                    address = "";
                    int counter = 1;
                    foreach (var s in reportInfo.Address1.Skip(32))
                    {

                        if (counter % 20 == 0)
                        {

                            adrs = string.Format("{0,-10}{1,-32}", "", address);
                            b.AppendLine(adrs);
                            address = "";
                        }
                        address = address + s;
                        counter++;
                    }
                    if (!string.IsNullOrEmpty(address))
                    {
                        adrs = string.Format("{0,-10}{1,-32}", " ", address);
                        b.AppendLine(adrs);
                    }
                }
                else
                {
                    string addres = string.Format("{0,-9}{1,-32}", Resource.Report_Address, address);
                    b.AppendLine(addres);
                }
                string city = string.Format("{0,-15}{1,-27}", " ", reportInfo.PostalCode + " " + reportInfo.City);
                b.AppendLine(city);

                string phon = string.Format("{0,-10}{1,-32}", Resource.Phone + ":", reportInfo.Phone);
                b.AppendLine(phon);

                ReportParameter rpmheader = new ReportParameter("header", reportInfo.Header);
                ReportParameter rpmfooter = new ReportParameter("footer", reportInfo.Footer);
                ReportParameter rpminfo = new ReportParameter("companyInfo", b.ToString());

                MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpmheader, rpmfooter, rpminfo });

                MyReportViewer.LocalReport.EnableExternalImages = true;
                MyReportViewer.LocalReport.Refresh();

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                byte[] bytes = MyReportViewer.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);
                string filepath = Server.MapPath("~/Reports/ZReport.pdf");
                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                var mailfiles = new List<NIMPOSMailSettings.MailFile>();
                var memstream = new MemoryStream();
                using (FileStream fs = System.IO.File.OpenRead(filepath))
                {
                    fs.CopyTo(memstream);
                }
                memstream.Position = 0;
                mailfiles.Add(new NIMPOSMailSettings.MailFile(memstream, "ZReport.pdf", "application/pdf"));
                var dateFrom = Convert.ToDateTime(dtFrom).Date;

                var dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");
                string subject = "Z Report från: " + dateFrom + " till:" + dateTo;
                string comment = "Z Report";
                string body = "<span>" + comment + "</span><br />";
                body += "<span>från: " + dateFrom + "</span><br />";
                body += "<span>till: " + dateTo + "</span><br />";

                List<string> emails = new List<string>();
                emails.Add(reportInfo.Email);
                NIMPOSMailSettings.SendMail(subject, body, emails, mailfiles);
                msg = "Email sent successfully";
            }
            catch (Exception ex)
            {
                msg = ex.Message;

            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        #region Category Sale


        public ActionResult AjaxHandler(jQueryDataTableParamModel param, string catId, string dtFrom, string dtTo, Guid outletId)
        {
            dtFrom = string.IsNullOrEmpty(dtFrom) ? DateTime.Now.ToString("yyyy-MM-dd") : dtFrom;
            dtTo = string.IsNullOrEmpty(dtTo) ? DateTime.Now.ToString("yyyy-MM-dd") : dtTo;

            DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
            DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");

            var allItems = GetCategorySaleData(dateFrom, dateTo, catId, outletId);
            IEnumerable<ItemViewModel> filteredItems = allItems;//.Where(s => s.TotalSale > 0);

            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<ItemViewModel, string> orderingFunction = (c => sortColumnIndex == 0 ? c.CategoryName :
                                                                sortColumnIndex == 1 ? c.Description :
                                                                sortColumnIndex == 2 ? c.SoldQty.ToString() :
                                                                sortColumnIndex == 3 ? c.TotalSale.ToString() : c.NetSale.ToString());

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            if (sortDirection == "asc")
                filteredItems = filteredItems.OrderBy(orderingFunction);
            else
                filteredItems = filteredItems.OrderByDescending(orderingFunction);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredItems = filteredItems.Where(c => c.CategoryName.ToLower().Contains(param.sSearch) || c.Description.ToLower().Contains(param.sSearch));
            }
            var displayedCustomers = filteredItems;
            if (param.iDisplayLength != -1)
                displayedCustomers = filteredItems.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedCustomers
                         select new[] { c.CategoryName, c.Description, c.Price.ToString(), c.SoldQty.ToString(), c.TotalSale.ToString(), c.NetSale.ToString(), c.Tax.ToString(), c.VatSum.ToString() };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allItems.Count(),
                iTotalDisplayRecords = filteredItems.Count(),
                aaData = result
            },
                JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public ActionResult LowStockReportAjax(jQueryDataTableParamModel param, string catId)
        {
            var allItems = GetLowStockReport(catId);
            IEnumerable<ItemViewModel> filteredItems = allItems;//.Where(s => s.TotalSale > 0);

            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<ItemViewModel, string> orderingFunction = (c => sortColumnIndex == 0 ? c.CategoryName :
                                                                sortColumnIndex == 1 ? c.Description :
                                                                sortColumnIndex == 2 ? c.BarCode :
                                                                sortColumnIndex == 3 ? c.StockQuantity.ToString() :
                                                                c.MinStockLevel.ToString());

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            if (sortDirection == "asc")
                filteredItems = filteredItems.OrderBy(orderingFunction);
            else
                filteredItems = filteredItems.OrderByDescending(orderingFunction);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredItems = filteredItems.Where(c => c.CategoryName.ToLower().Contains(param.sSearch) || c.Description.ToLower().Contains(param.sSearch));
            }

            var displayedCustomers = filteredItems;
            if (param.iDisplayLength != -1)
                displayedCustomers = filteredItems.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedCustomers
                         select new[]
                         {
                             c.CategoryName, 
                             c.Description, 
                             c.BarCode, 
                             c.StockQuantity.ToString(), 
                             c.MinStockLevel.ToString(),
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allItems.Count(),
                iTotalDisplayRecords = filteredItems.Count(),
                aaData = result
            },
                JsonRequestBehavior.AllowGet);
        }


        public ActionResult ProfitReport()
        {
            return View();
        }

        public ActionResult AjaxHandlerCopy(jQueryDataTableParamModel param, string catId, string dtFrom, string dtTo, Guid outletId)
        {
            dtFrom = string.IsNullOrEmpty(dtFrom) ? DateTime.Now.ToString("yyyy-MM-dd") : dtFrom;
            dtTo = string.IsNullOrEmpty(dtTo) ? DateTime.Now.ToString("yyyy-MM-dd") : dtTo;

            DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
            DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");

            var allItems = GetCategorySaleDataProfit(dateFrom, dateTo, catId, outletId);
            IEnumerable<ItemViewModel> filteredItems = allItems.Where(s => s.TotalSale > 0);

            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            /* Func<ItemViewModel, string> orderingFunction = (c => sortColumnIndex == 0 ? c.CategoryName :
                                                                 sortColumnIndex == 1 ? c.Description :
                                                                 sortColumnIndex == 2 ? c.SoldQty.ToString() :
                                                                 sortColumnIndex == 3 ? c.TotalSale.ToString() : c.NetSale.ToString());
             */

            Func<ItemViewModel, string> orderingFunction = (c => sortColumnIndex == 0 ? c.Description :
                                                               sortColumnIndex == 1 ? c.Price.ToString() :
                                                               sortColumnIndex == 2 ? c.PurchasePrice.ToString() :
                                                               sortColumnIndex == 3 ? c.SoldQty.ToString() :
                                                               sortColumnIndex == 4 ? c.TotalSale.ToString() : c.NetSale.ToString());


            var sortDirection = Request["sSortDir_0"]; // asc or desc
            if (sortDirection == "asc")
                filteredItems = filteredItems.OrderBy(orderingFunction);
            else
                filteredItems = filteredItems.OrderByDescending(orderingFunction);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredItems = filteredItems.Where(c => c.CategoryName.ToLower().Contains(param.sSearch) || c.Description.ToLower().Contains(param.sSearch));
            }
            var displayedCustomers = filteredItems;
            if (param.iDisplayLength != -1)
                displayedCustomers = filteredItems.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedCustomers
                         select new[] { c.Description, c.Price.ToString(), c.PriceExclMoms.ToString(), c.PurchasePrice.ToString(), c.PurchasePriceExcMoms.ToString(), c.SoldQty.ToString(), c.TotalSale.ToString(), c.NetSale.ToString(), c.NetSaleExlMoms.ToString(), c.ProfitPercentage.ToString() };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allItems.Count(),
                iTotalDisplayRecords = filteredItems.Count(),
                aaData = result
            },
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Removed where clause and also changed group by
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="catId"></param>
        /// <param name="outletId"></param>
        /// <returns></returns>
        private List<ItemViewModel> GetCategorySaleData_Graphs(DateTime dateFrom, DateTime dateTo, string catId, Guid outletId)
        {

            var model = new ItemViewModel();
            string query = @"SELECT   dbo.Fn_CategoryIdByItem(Product.Id) as CatId , dbo.Fn_CategoryByItem(Product.Id) as CategoryName,Product.Description,Sum(OrderDetail.Qty*OrderDetail.Direction) as soldQty,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent , Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) as TotalSale,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NETSale,isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) as VATSum,OrderDetail.PurchasePrice
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on OrderDetail.ItemId=Product.Id
			
				WHERE   OrderMaster.TrainingMode=0 AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  (OrderMaster.InvoiceDate  BETWEEN '" + dateFrom + "' AND '" + dateTo + "' ) ";

            string innerQury = @"WITH tblChild AS
                        (
                            SELECT Id
                                FROM Category WHERE Parant = " + catId + " UNION ALL  SELECT Category.Id FROM Category  JOIN tblChild  ON Category.Parant = tblChild.Id )  SELECT Id FROM tblChild";

            var result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                using (var conn = db.Database.Connection)
                {
                    conn.Open();

                    if (outletId != default(Guid))
                    {
                        query = query + " AND OrderMaster.OutletId='" + outletId + "'";
                    }

                    query = query + " GROUP BY Product.Id, Product.Description,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent,OrderDetail.PurchasePrice";
                    IDbCommand command = new SqlCommand(query);
                    command.Connection = conn;
                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new ItemViewModel
                        {
                            CategoryName = Convert.ToString(dr["CategoryName"]),
                            Description = Convert.ToString(dr["Description"]),
                            SoldQty = Math.Round(Convert.ToDecimal(dr["soldQty"]), 0),
                            TotalSale = Math.Round(Convert.ToDecimal(dr["TotalSale"]), 2),
                            NetSale = Math.Round(Convert.ToDecimal(dr["NetSale"]), 2),
                            VatSum = Math.Round(Convert.ToDecimal(dr["VATSum"]), 2),
                            Tax = Convert.ToDecimal(dr["TaxPercent"]),
                            Unit = (ProductUnit)Enum.Parse(typeof(ProductUnit), Convert.ToString(Convert.ToInt16(dr["Unit"]))),
                            Price = Convert.ToDecimal(dr["UnitPrice"]),
                            PurchasePrice = Convert.ToDecimal(dr["PurchasePrice"]),
                            CategoryId = Convert.ToInt32(dr["CatId"]),

                        });
                    }
                    dr.Dispose();
                }
            }

            var itemTest = result.Where(a => a.Description == "Barnbiljett 0-4").ToList();

            return result;
        }


        private List<ItemViewModel> GetCategorySaleData(DateTime dateFrom, DateTime dateTo, string catId, Guid outletId)
        {

            var model = new ItemViewModel();
            string query = @"SELECT   dbo.Fn_CategoryIdByItem(Product.Id) as CatId , dbo.Fn_CategoryByItem(Product.Id) as CategoryName,Product.Description,Sum(OrderDetail.Qty*OrderDetail.Direction) as soldQty,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent , Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) as TotalSale,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NETSale,isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) as VATSum,OrderDetail.PurchasePrice
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on OrderDetail.ItemId=Product.Id
			
				WHERE   OrderMaster.TrainingMode=0 AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  (OrderMaster.InvoiceDate  BETWEEN '" + dateFrom + "' AND '" + dateTo + "' ) ";

            string innerQury = @"WITH tblChild AS
                        (
                            SELECT Id
                                FROM Category WHERE Parant = " + catId + " UNION ALL  SELECT Category.Id FROM Category  JOIN tblChild  ON Category.Parant = tblChild.Id )  SELECT Id FROM tblChild";



            var result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                using (var conn = db.Database.Connection)
                {
                    conn.Open();

                    string catIds = "";
                    if (!string.IsNullOrEmpty(catId) && (catId != "0" || catId != "1"))
                    {
                        using (IDbCommand cmd = new SqlCommand(innerQury))
                        {
                            cmd.Connection = conn;
                            IDataReader dr1 = cmd.ExecuteReader();
                            catIds = catId;
                            while (dr1.Read())
                            {
                                if (!string.IsNullOrEmpty(catIds))
                                    catIds = catIds + ",";
                                catIds = catIds + Convert.ToString(dr1["Id"]);
                            }
                            dr1.Dispose();
                        }
                    }
                    if (!string.IsNullOrEmpty(catIds))
                    {
                        query = query + " AND dbo.Fn_CategoryIdByItem(Product.Id) in (" + catIds + ")";
                    }

                    if (outletId != default(Guid))
                    {
                        query = query + " AND OrderMaster.OutletId='" + outletId + "'";
                    }

                    query = query + " GROUP BY   dbo.Fn_CategoryIdByItem(Product.Id), dbo.Fn_CategoryByItem(Product.Id),Product.Description,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent,OrderDetail.PurchasePrice";
                    IDbCommand command = new SqlCommand(query);


                    command.Connection = conn;


                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new ItemViewModel
                        {
                            CategoryName = Convert.ToString(dr["CategoryName"]),
                            Description = Convert.ToString(dr["Description"]),
                            SoldQty = Math.Round(Convert.ToDecimal(dr["soldQty"]), 0),
                            TotalSale = Math.Round(Convert.ToDecimal(dr["TotalSale"]), 2),
                            NetSale = Math.Round(Convert.ToDecimal(dr["NetSale"]), 2),
                            VatSum = Math.Round(Convert.ToDecimal(dr["VATSum"]), 2),
                            Tax = Convert.ToDecimal(dr["TaxPercent"]),
                            Unit = (ProductUnit)Enum.Parse(typeof(ProductUnit), Convert.ToString(Convert.ToInt16(dr["Unit"]))),
                            Price = Convert.ToDecimal(dr["UnitPrice"]),
                            PurchasePrice = Convert.ToDecimal(dr["PurchasePrice"]),
                            CategoryId = Convert.ToInt32(dr["CatId"]),

                        });
                    }
                    dr.Dispose();
                }
            }

            var itemTest = result.Where(a => a.Description == "Barnbiljett 0-4").ToList();

            return result;
        }

        private List<ItemViewModel> GetLowStockReport(string catId)
        {
            var model = new ItemViewModel();
            string query = @"SELECT   
            dbo.Fn_CategoryIdByItem(Product.Id) as CatId , 
            dbo.Fn_CategoryByItem(Product.Id) as CategoryName,
            Product.Description,
            Product.BarCode,
            Product.StockQuantity, Product.MinStockLevel
	        FROM Product
			WHERE Product.Deleted = 0 AND StockQuantity <= MinStockLevel ";

            string innerQury = @"WITH tblChild AS (
                            SELECT Id
                                FROM Category WHERE Parant = " + catId + " UNION ALL  SELECT Category.Id FROM Category  JOIN tblChild  ON Category.Parant = tblChild.Id )  SELECT Id FROM tblChild";
            
            var result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                using (var conn = db.Database.Connection)
                {
                    conn.Open();

                    string catIds = "";
                    if (!string.IsNullOrEmpty(catId) && (catId != "0" || catId != "1"))
                    {
                        using (IDbCommand cmd = new SqlCommand(innerQury))
                        {
                            cmd.Connection = conn;
                            IDataReader dr1 = cmd.ExecuteReader();
                            catIds = catId;
                            while (dr1.Read())
                            {
                                if (!string.IsNullOrEmpty(catIds))
                                    catIds = catIds + ",";
                                catIds = catIds + Convert.ToString(dr1["Id"]);
                            }
                            dr1.Dispose();
                        }
                    }
                    if (!string.IsNullOrEmpty(catIds))
                    {
                        query = query + " AND dbo.Fn_CategoryIdByItem(Product.Id) in (" + catIds + ")";
                    }

                    query = query + " GROUP BY dbo.Fn_CategoryIdByItem(Product.Id), dbo.Fn_CategoryByItem(Product.Id),Product.Description,Product.BarCode,Product.StockQuantity,Product.MinStockLevel";
                    IDbCommand command = new SqlCommand(query);
                    command.Connection = conn;
                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new ItemViewModel
                        {
                            CategoryName = Convert.ToString(dr["CategoryName"]),
                            Description = Convert.ToString(dr["Description"]),
                            BarCode = (dr["BarCode"] == null || dr["BarCode"] == DBNull.Value) ? "" : !string.IsNullOrEmpty(Convert.ToString(dr["BarCode"])) ? Convert.ToString(dr["BarCode"]) : "" ,
                            StockQuantity = Math.Round(Convert.ToDecimal(dr["StockQuantity"]), 0),
                            MinStockLevel = (dr["MinStockLevel"] == null || dr["MinStockLevel"] == DBNull.Value) ? 0 : Math.Round(Convert.ToDecimal(dr["MinStockLevel"]), 2),
                            CategoryId = Convert.ToInt32(dr["CatId"])
                        });
                    }
                    dr.Dispose();
                }
            }

            var itemTest = result.Where(a => a.Description == "Barnbiljett 0-4").ToList();

            return result;
        }
        private List<ItemViewModel> GetCategorySaleDataProfit(DateTime dateFrom, DateTime dateTo, string catId, Guid outletId)
        {

            var model = new ItemViewModel();
            string query = @"SELECT   dbo.Fn_CategoryIdByItem(Product.Id) as CatId , dbo.Fn_CategoryByItem(Product.Id) as CategoryName,Product.Description,Sum(OrderDetail.Qty*OrderDetail.Direction) as soldQty,OrderDetail.UnitPrice,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as PriceExcludeMom,Product.Unit,OrderDetail.TaxPercent , Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) as TotalSale,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(Product.PurchasePrice*OrderDetail.Qty))) as NETSale,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(Product.PurchasePrice*OrderDetail.Qty))/(1+OrderDetail.TaxPercent/100)) as NetSaleExlMoms,isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) as VATSum,Product.PurchasePrice
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on OrderDetail.ItemId=Product.Id
			
				WHERE   OrderMaster.TrainingMode=0 AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  (OrderMaster.InvoiceDate  BETWEEN '" + dateFrom + "' AND '" + dateTo + "' ) ";

            string innerQury = @"WITH tblChild AS
                        (
                            SELECT Id
                                FROM Category WHERE Parant = " + catId + " UNION ALL  SELECT Category.Id FROM Category  JOIN tblChild  ON Category.Parant = tblChild.Id )  SELECT Id FROM tblChild";



            var result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                using (var conn = db.Database.Connection)
                {
                    conn.Open();

                    string catIds = "";
                    if (!string.IsNullOrEmpty(catId) && (catId != "0" || catId != "1"))
                    {
                        using (IDbCommand cmd = new SqlCommand(innerQury))
                        {
                            cmd.Connection = conn;
                            IDataReader dr1 = cmd.ExecuteReader();
                            catIds = catId;
                            while (dr1.Read())
                            {
                                if (!string.IsNullOrEmpty(catIds))
                                    catIds = catIds + ",";
                                catIds = catIds + Convert.ToString(dr1["Id"]);
                            }
                            dr1.Dispose();
                        }
                    }
                    if (!string.IsNullOrEmpty(catIds))
                    {
                        query = query + " AND dbo.Fn_CategoryIdByItem(Product.Id) in (" + catIds + ")";
                    }

                    if (outletId != default(Guid))
                    {
                        query = query + " AND OrderMaster.OutletId='" + outletId + "'";
                    }

                    query = query + " GROUP BY   dbo.Fn_CategoryIdByItem(Product.Id), dbo.Fn_CategoryByItem(Product.Id),Product.Description,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent,Product.PurchasePrice";
                    IDbCommand command = new SqlCommand(query);


                    command.Connection = conn;


                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new ItemViewModel
                        {
                            CategoryName = Convert.ToString(dr["CategoryName"]),
                            Description = Convert.ToString(dr["Description"]),
                            SoldQty = Math.Round(Convert.ToDecimal(dr["soldQty"]), 0),
                            TotalSale = Math.Round(Convert.ToDecimal(dr["TotalSale"]), 2),
                            NetSale = Math.Round(Convert.ToDecimal(dr["NetSale"]), 2),
                            VatSum = Math.Round(Convert.ToDecimal(dr["VATSum"]), 2),
                            Tax = Convert.ToDecimal(dr["TaxPercent"]),
                            Unit = (ProductUnit)Enum.Parse(typeof(ProductUnit), Convert.ToString(Convert.ToInt16(dr["Unit"]))),
                            Price = Convert.ToDecimal(dr["UnitPrice"]),
                            PurchasePrice = Convert.ToDecimal(dr["PurchasePrice"]),
                            PriceExcludeMom = Math.Round(Convert.ToDecimal(dr["PriceExcludeMom"]), 2),
                            NetSaleExlMoms = Math.Round(Convert.ToDecimal(dr["NetSaleExlMoms"]), 2)
                        });
                    }

                    dr.Dispose();
                }
            }
            return result;
        }

        #endregion

        #region User Sale
        public ActionResult SaleByUser()
        {
            return View();
        }
        public ActionResult AjaxUserSale(jQueryDataTableParamModel param, string id, string dtFrom, string dtTo)
        {
            dtFrom = string.IsNullOrEmpty(dtFrom) ? DateTime.Now.ToString("yyyy-MM-dd") : dtFrom;
            dtTo = string.IsNullOrEmpty(dtTo) ? DateTime.Now.ToString("yyyy-MM-dd") : dtTo;

            DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
            DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");
            Guid userId;
            Guid.TryParse(id, out userId);
            var allItems = GetUserSaleData(dateFrom, dateTo, userId);
            IEnumerable<ItemViewModel> filteredItems = allItems.Where(s => s.TotalSale > 0);

            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<ItemViewModel, string> orderingFunction = (c => sortColumnIndex == 0 ? c.Description :
                                                                sortColumnIndex == 1 ? c.SoldQty.ToString() :
                                                                sortColumnIndex == 2 ? c.TotalSale.ToString() : c.NetSale.ToString());

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            if (sortDirection == "asc")
                filteredItems = filteredItems.OrderBy(orderingFunction);
            else
                filteredItems = filteredItems.OrderByDescending(orderingFunction);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredItems = filteredItems.Where(c => c.Description.ToLower().Contains(param.sSearch));
            }
            var displayedCustomers = filteredItems;
            if (param.iDisplayLength != -1)
                displayedCustomers = filteredItems.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedCustomers
                         select new[] { c.Description, c.Price.ToString(), c.SoldQty.ToString(), c.TotalSale.ToString(), c.NetSale.ToString(), c.Tax.ToString(), c.VatSum.ToString() };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allItems.Count(),
                iTotalDisplayRecords = filteredItems.Count(),
                aaData = result
            },
                JsonRequestBehavior.AllowGet);
        }
        private List<ItemViewModel> GetUserSaleData(DateTime dateFrom, DateTime dateTo, Guid userId)
        {

            var model = new ItemViewModel();
            string query = @"SELECT  Product.Description,Sum(OrderDetail.Qty*OrderDetail.Direction) as soldQty,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent , Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) as TotalSale,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NETSale,isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) as VATSum
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on OrderDetail.ItemId=Product.Id
				WHERE   OrderMaster.TrainingMode=0 AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  (OrderMaster.InvoiceDate  BETWEEN '" + dateFrom + "' AND '" + dateTo + "' ) ";

            //if (userId != default(Guid))
            //{
            query = query + " AND [dbo].OrderMaster.UserId='" + userId + "'";
            //}

            query = query + " GROUP BY Product.Description,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent";
            var result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                using (var conn = db.Database.Connection)
                {
                    IDbCommand command = new SqlCommand(query);

                    conn.Open();
                    command.Connection = conn;

                    IDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new ItemViewModel
                        {

                            Description = Convert.ToString(dr["Description"]),
                            SoldQty = Convert.ToDecimal(dr["soldQty"]),
                            TotalSale = Math.Round(Convert.ToDecimal(dr["TotalSale"]), 2),
                            NetSale = Math.Round(Convert.ToDecimal(dr["NetSale"]), 2),
                            VatSum = Math.Round(Convert.ToDecimal(dr["VATSum"]), 2),
                            Tax = Convert.ToDecimal(dr["TaxPercent"]),
                            Unit = (ProductUnit)Enum.Parse(typeof(ProductUnit), Convert.ToString(Convert.ToInt16(dr["Unit"]))),
                            Price = Convert.ToDecimal(dr["UnitPrice"]),

                        });
                    }
                    dr.Dispose();
                }
            }
            return result;
        }

        #endregion



        #region  Sales Graph 

        public ActionResult LoadDashboardData(string data)
        {

            try
            {
                return Json(new { SaleData = GetData() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Message = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LoadDashboardDataForPayments(string data)
        {
            try
            {
                DashboardPaymentStats dashboard = new DashboardPaymentStats();
                dashboard.StatusLstDayCostTrend = GetAllPaymentData(DateTime.Now, "day"); // GetDayCostTrend(currentDate);
                dashboard.StatusLstWeekCostTrend = GetAllPaymentData(DateTime.Now.AddDays(-7), "week"); // GetWeekCostTrend(currentDate);
                dashboard.StatusLstMonthCostTrend = GetAllPaymentData(DateTime.Now.AddMonths(-1), "month"); // GetMonthCostTrend(currentDate);
                dashboard.StatusLstYearCostTrend = GetAllPaymentData(DateTime.Now.AddYears(-1), "year"); // GetYearCostTrend(currentDate);
                //dashboard.SalesByCategory = GetSalesDateByCategory(DateTime.Now);

                return Json(new { SaleData = dashboard }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Message = "" }, JsonRequestBehavior.AllowGet);
            }
        }






        public DashboardStats GetData()
        {

            var getData = GetReportDataNew();
            DateTime currentDate = DateTime.Now.Date;
            DateTime TodayEndDate = currentDate.AddDays(1).Date;


            //var dtTo = Convert.ToDateTime(currentDate.Year + "-" + currentDate.Month + "-" + currentDate.Day + "  11:59:00 PM");
            var dtTo = DateTime.Now;
            var lastday = (from data in getData.Where(a => a.SaleDate >= currentDate && a.SaleDate <= dtTo)
                           group data by new { data.SaleDate.Hour } into groupedData
                           select new Result()
                           {
                               Key = groupedData.Key.Hour,
                               Value = groupedData.Sum(a => a.GrossTotal)
                           }).ToList();

            var LstWeek = (from data in getData.Where(a => a.SaleDate.Date >= currentDate.AddDays(-7).Date && a.SaleDate.Date.Date <= currentDate)
                           group data by new { data.SaleDate.Day } into groupedData
                           select new Result()
                           {
                               Key = groupedData.Key.Day,
                               Value = groupedData.Sum(a => a.GrossTotal)
                           }).ToList();

            var LastWeekOrders = (from data in getData.Where(a => a.SaleDate.Date >= currentDate.AddDays(-7).Date && a.SaleDate.Date.Date <= currentDate)
                                  group data by new { data.SaleDate.Day } into groupedData
                                  select new Result()
                                  {
                                      Key = groupedData.Key.Day,
                                      Value1 = groupedData.Count()
                                  }).ToList();

            var LstMonth = (from data in getData.Where(a => a.SaleDate.Date >= currentDate.AddMonths(-1).Date && a.SaleDate.Date.Date <= currentDate)
                            group data by new { data.SaleDate.Day, data.SaleDate.Month, data.SaleDate.Year } into groupedData
                            select new Result()
                            {
                                Key = groupedData.Key.Day,
                                DayKey = groupedData.Key.Day,
                                MonthKey = groupedData.Key.Month,
                                Value = groupedData.Sum(a => a.GrossTotal)
                            }).ToList();

            var LstLastYear = (from data in getData.Where(a => a.SaleDate.Date >= currentDate.AddYears(-1).Date && a.SaleDate.Date.Date <= currentDate)
                               group data by new { data.SaleDate.Month, data.SaleDate.Year } into groupedData
                               select new Result()
                               {
                                   Key = groupedData.Key.Month,
                                   DateTimeKey = groupedData.FirstOrDefault().SaleDate,
                                   YearKey = groupedData.Key.Year,
                                   Value = groupedData.Sum(a => a.GrossTotal)
                               }).ToList();

            DashboardStats dashboard = new DashboardStats();

            dashboard.SalesOfWeekDifference = 500;
            dashboard.SalesDifference = 500;


            dashboard.Sales = Math.Round(getData.Where(a => a.SaleDate >= currentDate && a.SaleDate <= TodayEndDate).Sum(a => a.GrossTotal), 2);
            dashboard.SalesOfWeek = Math.Round(getData.Where(a => a.SaleDate.Date >= currentDate.AddDays(-7).Date && a.SaleDate.Date <= dtTo).Sum(a => a.GrossTotal), 2);
            dashboard.SalesOfMonth = Math.Round(getData.Where(a => a.SaleDate.Date >= currentDate.AddMonths(-1).Date && a.SaleDate.Date <= dtTo).Sum(a => a.GrossTotal), 2);
            dashboard.SalesOfYear = Math.Round(getData.Where(a => a.SaleDate.Date >= currentDate.AddYears(-1).Date && a.SaleDate.Date <= dtTo).Sum(a => a.GrossTotal), 2);



            dashboard.LstDaySaleTrend = GetDaySaleTrend(dtTo, lastday);
            dashboard.LstWeekSaleTrend = GetWeekSaleTrend(currentDate, LstWeek, LastWeekOrders);
            dashboard.LstMonthSaleTrend = GetMonthSaleTrend(currentDate, LstMonth);
            dashboard.LstYearSaleTrend = GetYearSaleTrend(currentDate, LstLastYear);
            dashboard.LstYearSaleTrend = dashboard.LstYearSaleTrend.OrderByDescending(a => a.SortOrder).ToList();


            return dashboard;
        }



        private List<OutletSale> GetReportDataNew()
        {

            List<OutletSale> model = new List<OutletSale>();
            try
            {
                DateTime date = DateTime.Now.Date;
                int today = date.Day;
                int year = date.Year;
                int month = date.Month;
                var dtFrom = new DateTime(year, month, 1);
                var lastDate = dtFrom.AddMonths(1).AddDays(-1).Day;
                var dtTo = year + "-" + month + "-" + lastDate + "  11:59:00 PM";
                DateTime TodayEndDate = date.AddDays(1).Date; //Convert.ToDateTime(year + "-" + month + "-" + today + "  11:59:00 PM");
                string startDate = date.AddYears(-1).Year + "-" + month + "-" + 1 + "  00:00:00 AM";
                string query = @"SELECT  cast( OrderMaster.InvoiceDate as datetime) as SaleDate,  sum(((OrderDetail.Direction* OrderDetail.Qty)* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum(((OrderDetail.UnitPrice*(OrderDetail.Qty*OrderDetail.Direction))-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NetTotal,OrderMaster.OutletId,(select top 1 Name from outlet where Id= OrderMaster.OutletId) as OutletName
                           FROM    OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId						
						Where OrderDetail.Active=1 AND  OrderDetail.ItemType<>1 AND (OrderMaster.InvoiceDate between  '" + startDate + "' AND '" + dtTo + "')" +
                        " Group by cast(OrderMaster.InvoiceDate as datetime),OrderMaster.OutletId ";

                var monthlySale = new List<OutletSale>();

                using (var db = GetConnection)
                {
                    //var selectedoutles = UserOutlets;
                    if (AllowedOutlets != null && AllowedOutlets.Count > 0)
                    {
                        monthlySale = db.Database.SqlQuery<OutletSale>(query)
                            .Where(s => AllowedOutlets.Contains(s.OutletId.ToString())).ToList();
                    }
                    else
                    {
                        var pt = db.PaymentType.ToString();
                        monthlySale = db.Database.SqlQuery<OutletSale>(query).ToList();
                    }
                }

                return monthlySale;



            }
            catch (Exception ex)
            {

                return model;

            }

        }





        private List<SaleTrendResult> GetYearSaleTrend(DateTime now, List<Result> lstLastYear)
        {
            List<SaleTrendResult> saleYearTrendResult = new List<SaleTrendResult>();
            var currentMonth = now.Month;
            var lastYearMonth = now.AddYears(-1).Month + 1;
            var sortOrder = 0;
            var index = lastYearMonth;
            for (int j = 0; j < 12; j++)
            {
                if (index > 12)
                {
                    index = index - 12;
                }

                var month = new DateTime(now.Year, index, 1).ToString("MMMM");

                if (index > now.Month)
                {
                    if (lstLastYear.Any(a => a.Key == index && a.YearKey == (now.Year - 1)))
                    {
                        saleYearTrendResult.Add(new SaleTrendResult
                        {
                            SortOrder = ++sortOrder,
                            Key = month,
                            Value1 = Math.Round(lstLastYear.FirstOrDefault(a => a.Key == index && a.YearKey == (now.Year - 1)).Value, 2),
                        });
                    }
                    else
                    {
                        saleYearTrendResult.Add(new SaleTrendResult
                        {
                            SortOrder = ++sortOrder,
                            Key = month,
                            Value1 = 0,
                        });
                    }
                }
                else
                {
                    if (lstLastYear.Any(a => a.Key == index && a.YearKey == (now.Year)))
                    {
                        saleYearTrendResult.Add(new SaleTrendResult
                        {
                            SortOrder = ++sortOrder,
                            Key = month,
                            Value1 = Math.Round(lstLastYear.FirstOrDefault(a => a.Key == index && a.YearKey == (now.Year)).Value, 2),
                        });
                    }
                    else
                    {
                        saleYearTrendResult.Add(new SaleTrendResult
                        {
                            SortOrder = ++sortOrder,
                            Key = month,
                            Value1 = 0,
                        });
                    }
                }

                index++;
            }

            return saleYearTrendResult;
        }

        private List<SaleTrendResult> GetMonthSaleTrend(DateTime now, List<Result> lstMonth)
        {
            List<SaleTrendResult> saleMonthTrendResult = new List<SaleTrendResult>();
            for (int i = 0; i < 30; i++)
            {
                var date = now.Date.AddDays(i * (-1)).Date;

                if (lstMonth.Any(a => a.DayKey == date.Day && a.MonthKey == date.Month))
                {
                    saleMonthTrendResult.Add(new SaleTrendResult
                    {
                        Key = date.ToString("dd/MM/yyyy"),
                        Value1 = Math.Round(lstMonth.FirstOrDefault(a => a.DayKey == date.Day && a.MonthKey == date.Month).Value, 2),
                    });
                }

                else
                {
                    saleMonthTrendResult.Add(new SaleTrendResult
                    {
                        Key = date.ToString("dd/MM/yyyy"),
                        Value1 = 0,
                    });
                }

            }

            return saleMonthTrendResult;
        }

        private List<SaleTrendResult> GetWeekSaleTrend(DateTime now, List<Result> lstWeek, List<Result> lastWeekOrders)
        {

            List<SaleTrendResult> saleWeekTrendResult = new List<SaleTrendResult>();

            for (int i = 0; i < 7; i++)
            {
                var date = now.Date.AddDays(i * (-1)).Date;

                if (lstWeek.Any(a => a.Key == date.Day))
                {
                    saleWeekTrendResult.Add(new SaleTrendResult
                    {
                        Key = date.ToString("dd/MM/yyyy"),
                        Value1 = Math.Round(lstWeek.FirstOrDefault(a => a.Key == date.Day).Value, 2),

                        OrderCount = lastWeekOrders.FirstOrDefault(a => a.Key == date.Day).Value1,

                        AVO = lastWeekOrders.FirstOrDefault(a => a.Key == date.Day).Value == 0 ? 0 : Math.Round(Convert.ToDouble(lstWeek.FirstOrDefault(a => a.Key == date.Day).Value) / Convert.ToDouble(lastWeekOrders.FirstOrDefault(a => a.Key == date.Day).Value), 2)
                    });
                }
                else
                {
                    saleWeekTrendResult.Add(new SaleTrendResult
                    {
                        Key = date.ToString("dd/MM/yyyy"),
                        Value1 = 0,
                        OrderCount = 0,
                        AVO = 0
                    });
                }
            }

            return saleWeekTrendResult;
        }

        private List<SaleTrendResult> GetDaySaleTrend(DateTime now, List<Result> lstDay)
        {
            List<SaleTrendResult> saleDayTrendResult = new List<SaleTrendResult>();

            var sortOrder = 0;
            for (int i = 0; i < 7; i++)
            {
                var hour = now.AddHours(i * (-1)).Hour;

                if (lstDay.Any(a => a.Key == hour))
                {
                    saleDayTrendResult.Add(new SaleTrendResult
                    {
                        Key = hour + ":00",
                        Value1 = Math.Round(lstDay.FirstOrDefault(a => a.Key == hour).Value, 2),
                        SortOrder = sortOrder
                    });
                }
                else
                {
                    saleDayTrendResult.Add(new SaleTrendResult
                    {
                        Key = hour + ":00",
                        Value1 = 0,
                        SortOrder = sortOrder
                    });
                }
                sortOrder++;
            }
            return saleDayTrendResult.OrderBy(s => s.SortOrder).ToList();
        }



        public List<PaymentData> GetAllPaymentData(DateTime timestamp, string day)
        {
            using (var db = GetConnection)
            {
                if (day == "day")
                {
                    var result = (from payment in db.Payment
                                  join pt in db.PaymentType on payment.TypeId equals pt.Id
                                  where payment.PaymentDate >= timestamp.Date
                                  group payment by pt.Name into groupedData
                                  select new PaymentData
                                  {
                                      PaymentType = groupedData.Key,
                                      Result = (from innerData in groupedData
                                                group innerData by innerData.PaymentDate.Hour into innerGroupData
                                                select new Result()
                                                {
                                                    Key = innerGroupData.Key,
                                                    Value = innerGroupData.Sum(a => a.PaidAmount),
                                                }).ToList(),
                                  }).ToList();
                    return GetDailyPayment(result);
                }

                if (day == "week")
                {
                    var result = (from payment in db.Payment
                                  join pt in db.PaymentType on payment.TypeId equals pt.Id
                                  where payment.PaymentDate >= timestamp
                                  group payment by pt.Name into groupedData
                                  select new PaymentData
                                  {
                                      PaymentType = groupedData.Key,
                                      Result = (from innerData in groupedData
                                                group innerData by innerData.PaymentDate.Day into innerGroupData
                                                select new Result()
                                                {
                                                    Key = innerGroupData.Key,
                                                    Value = innerGroupData.Sum(a => a.PaidAmount),
                                                }).ToList(),
                                  }).ToList();


                    return GetWeeklyPayment(result);
                }

                if (day == "month")
                {
                    var result = (from payment in db.Payment
                                  join pt in db.PaymentType on payment.TypeId equals pt.Id
                                  where payment.PaymentDate >= timestamp
                                  group payment by pt.Name into groupedData
                                  select new PaymentData
                                  {
                                      PaymentType = groupedData.Key,
                                      Result = (from innerData in groupedData
                                                group innerData by new { innerData.PaymentDate.Day, innerData.PaymentDate.Month, innerData.PaymentDate.Year } into innerGroupData
                                                select new Result()
                                                {
                                                    Key = innerGroupData.Key.Day,
                                                    DayKey = innerGroupData.Key.Day,
                                                    MonthKey = innerGroupData.Key.Month,
                                                    Value = innerGroupData.Sum(a => a.PaidAmount),
                                                }).ToList(),
                                  }).ToList();
                    return GetMonthlyPayment(result);
                }

                if (day == "year")
                {
                    var result = (from payment in db.Payment
                                  join pt in db.PaymentType on payment.TypeId equals pt.Id
                                  where payment.PaymentDate >= timestamp
                                  group payment by pt.Name into groupedData
                                  select new PaymentData
                                  {
                                      PaymentType = groupedData.Key,
                                      Result = (from innerData in groupedData
                                                group innerData by new { innerData.PaymentDate.Month, innerData.PaymentDate.Year } into innerGroupData
                                                select new Result()
                                                {
                                                    Key = innerGroupData.Key.Month,
                                                    YearKey = innerGroupData.Key.Year,
                                                    Value = innerGroupData.Sum(a => a.PaidAmount),
                                                }).ToList(),
                                  }).ToList();
                    return GetYearlyPayment(result);
                }

                else return new List<PaymentData>();
            }

        }
        public List<PaymentData> GetDailyPayment(List<PaymentData> result)
        {
            List<PaymentData> paymentData = new List<PaymentData>();
            foreach (var item in result)
            {
                List<SaleTrendResult> lstresult = new List<SaleTrendResult>();

                for (int i = 0; i < 7; i++)
                {
                    var hour = DateTime.Now.AddHours(i * (-1)).Hour;

                    if (item.Result.Any(a => a.Key == hour))
                    {
                        lstresult.Add(new SaleTrendResult
                        {
                            Key = hour + ":00",
                            Value1 = Math.Round(item.Result.FirstOrDefault(a => a.Key == hour).Value, 2),
                            KeyName = item.Result.FirstOrDefault().KeyName
                        });
                    }
                    else
                    {
                        lstresult.Add(new SaleTrendResult
                        {
                            Key = hour + ":00",
                            Value1 = 0,
                            KeyName = ""

                        });
                    }
                }

                if (item.SaleTrendResult != null && !lstresult.Any(x => item.SaleTrendResult.Any(y => y == x)))
                {
                    item.SaleTrendResult = lstresult;
                    paymentData.Add(item);
                }
            }

            return paymentData;
        }
        public List<PaymentData> GetWeeklyPayment(List<PaymentData> result)
        {
            List<PaymentData> paymentData = new List<PaymentData>();
            foreach (var item in result)
            {
                List<SaleTrendResult> lstresult = new List<SaleTrendResult>();

                for (int i = 0; i < 7; i++)
                {
                    var date = DateTime.Now.Date.AddDays(i * (-1)).Date;

                    if (item.Result.Any(a => a.Key == date.Day))
                    {
                        lstresult.Add(new SaleTrendResult
                        {
                            Key = date.ToString("dd/MM/yyyy"),
                            Value1 = Math.Round(item.Result.FirstOrDefault(a => a.Key == date.Day).Value, 2),
                        });
                    }
                    else
                    {
                        lstresult.Add(new SaleTrendResult
                        {
                            Key = date.ToString("dd/MM/yyyy"),
                            Value1 = 0,
                        });
                    }
                }

                if (item.SaleTrendResult != null && !lstresult.Any(x => item.SaleTrendResult.Any(y => y == x)))
                {
                    item.SaleTrendResult = lstresult;
                    paymentData.Add(item);
                }
            }

            return paymentData;
        }
        public List<PaymentData> GetMonthlyPayment(List<PaymentData> result)
        {
            List<PaymentData> paymentData = new List<PaymentData>();
            foreach (var item in result)
            {
                List<SaleTrendResult> lstresult = new List<SaleTrendResult>();

                for (int i = 0; i < 30; i++)
                {
                    var date = DateTime.Now.AddDays(i * (-1)).Date;

                    if (item.Result.Any(a => a.DayKey == date.Day && a.MonthKey == date.Month))
                    {
                        lstresult.Add(new SaleTrendResult
                        {
                            Key = date.ToString("dd/MM/yyyy"),
                            Value1 = Math.Round(item.Result.FirstOrDefault(a => a.DayKey == date.Day && a.MonthKey == date.Month).Value, 2),
                            KeyName = item.Result.FirstOrDefault().KeyName

                        });
                    }

                    else
                    {
                        lstresult.Add(new SaleTrendResult
                        {
                            Key = date.ToString("dd/MM/yyyy"),
                            Value1 = 0,
                            KeyName = ""

                        });
                    }

                }

                if (item.SaleTrendResult != null && !lstresult.Any(x => item.SaleTrendResult.Any(y => y == x)))
                {
                    item.SaleTrendResult = lstresult;
                    paymentData.Add(item);
                }
            }

            return paymentData;
        }
        public List<PaymentData> GetYearlyPayment(List<PaymentData> result)
        {
            List<PaymentData> paymentData = new List<PaymentData>();
            foreach (var item in result)
            {
                List<SaleTrendResult> lstresult = new List<SaleTrendResult>();
                var now = DateTime.Now;

                var currentMonth = now.Month;
                var lastYearMonth = now.AddYears(-1).Month + 1;
                var sortOrder = 0;
                var index = lastYearMonth;
                for (int j = 0; j < 12; j++)
                {
                    if (index > 12)
                    {
                        index = index - 12;
                    }

                    var month = new DateTime(now.Year, index, 1).ToString("MMMM");

                    if (index > now.Month)
                    {
                        if (item.Result.Any(a => a.Key == index && a.YearKey == (now.Year - 1)))
                        {
                            lstresult.Add(new SaleTrendResult
                            {
                                SortOrder = ++sortOrder,
                                Key = month,
                                Value1 = Math.Round(item.Result.FirstOrDefault(a => a.Key == index && a.YearKey == (now.Year - 1)).Value, 2),
                                KeyName = item.Result.FirstOrDefault().KeyName
                            });
                        }
                        else
                        {
                            lstresult.Add(new SaleTrendResult
                            {
                                SortOrder = ++sortOrder,
                                Key = month,
                                Value1 = 0,
                                KeyName = ""
                            });
                        }
                    }
                    else
                    {
                        if (item.Result.Any(a => a.Key == index && a.YearKey == (now.Year)))
                        {
                            lstresult.Add(new SaleTrendResult
                            {
                                SortOrder = ++sortOrder,
                                Key = month,
                                Value1 = Math.Round(item.Result.FirstOrDefault(a => a.Key == index && a.YearKey == (now.Year)).Value, 2),
                                KeyName = item.Result.FirstOrDefault().KeyName

                            });
                        }
                        else
                        {
                            lstresult.Add(new SaleTrendResult
                            {
                                SortOrder = ++sortOrder,
                                Key = month,
                                Value1 = 0,
                                KeyName = ""

                            });
                        }
                    }

                    index++;
                }

                if (item.SaleTrendResult != null && !lstresult.Any(x => item.SaleTrendResult.Any(y => y == x)))
                {
                    item.SaleTrendResult = lstresult.OrderByDescending(s => s.SortOrder).ToList();
                    paymentData.Add(item);
                }
            }

            return paymentData;
        }

        #endregion


        #region Pie Charts by sales data product and category 


        public ActionResult GetSalesDateByCategoryTimeSapn(string timeSpan)
        {
            ReportViewModel model = new ReportViewModel();
            if (timeSpan == "Day")
                model.SalesByCategory = GetSalesDateByCategory(DateTime.Now.Date);
            else if (timeSpan == "Week")
                model.SalesByCategory = GetSalesDateByCategory(DateTime.Now.AddDays(-7));
            else if (timeSpan == "Month")
                model.SalesByCategory = GetSalesDateByCategory(DateTime.Now.AddMonths(-1));
            else if (timeSpan == "Year")
                model.SalesByCategory = GetSalesDateByCategory(DateTime.Now.AddYears(-1));
            return PartialView("_graphSaleByCategory", model);
        }
        private Highcharts GetSalesDateByCategory(DateTime timeStamp)
        {
            try
            {
                //for pie chart
                var resultData = GetCategorySaleData_Graphs(timeStamp, DateTime.Now, "0", Guid.Empty);
                var groupData = resultData.GroupBy(g => g.CategoryId).Select(g => new
                {
                    Id = g.Key,
                    CategoryName = g.FirstOrDefault().CategoryName,
                    TotalSale = g.Sum(x => x.TotalSale),
                }).ToList();


                var browers = new List<object[]>();
                foreach (var data in groupData)
                {
                    browers.Add(new object[] { data.CategoryName, data.TotalSale });
                }
                Highcharts pieChart = new Highcharts("pieChart")
                   .SetTitle(new Title { Text = "  " })
                   .SetCredits(new Credits { Enabled = false })
                   .SetSeries(new DotNet.Highcharts.Options.Series
                   {
                       Type = ChartTypes.Pie,
                       Name = Resource.TotalSale,
                       Data = new DotNet.Highcharts.Helpers.Data(browers.ToArray())
                   });

                return pieChart;
            }
            catch (Exception ex)
            {
                return new Highcharts("pieChart");
            }


        }

        public ActionResult GetSalesDateByProductTimeSapn(string timeSpan)
        {
            ReportViewModel model = new ReportViewModel();
            if (timeSpan == "Day")
                model.SalesByProduct = GetSalesDateByProduct(DateTime.Now.Date);
            else if (timeSpan == "Week")
                model.SalesByProduct = GetSalesDateByProduct(DateTime.Now.AddDays(-7));
            else if (timeSpan == "Month")
                model.SalesByProduct = GetSalesDateByProduct(DateTime.Now.AddMonths(-1));
            else if (timeSpan == "Year")
                model.SalesByProduct = GetSalesDateByProduct(DateTime.Now.AddYears(-1));
            return PartialView("_graphSaleByProduct", model);
        }
        private Highcharts GetSalesDateByProduct(DateTime timeStamp)
        {
            try
            {
                //for pie chart
                var resultData = GetCategorySaleData_Graphs(timeStamp, DateTime.Now, "0", Guid.Empty);
                var groupData = resultData.GroupBy(g => g.Description).Select(g => new
                {
                    Id = g.Key,
                    ProductName = g.FirstOrDefault().Description,
                    TotalSale = g.Sum(x => x.TotalSale),
                }).ToList();


                var browers = new List<object[]>();
                foreach (var data in groupData)
                {
                    browers.Add(new object[] { data.ProductName.Replace("'", "\\'"), data.TotalSale });
                }
                Highcharts pieChart = new Highcharts("pieChart1")
                   .SetTitle(new Title { Text = "  " })
                   .SetCredits(new Credits { Enabled = false })
                   .SetSeries(new DotNet.Highcharts.Options.Series
                   {
                       Type = ChartTypes.Pie,
                       Name = Resource.TotalSale,
                       Data = new DotNet.Highcharts.Helpers.Data(browers.ToArray())
                   });

                return pieChart;
            }
            catch (Exception)
            {
                return new Highcharts("pieChart");
            }


        }


        #endregion

        public ActionResult XMLOrders()
        {
            var pdfFilesPath = ConfigurationManager.AppSettings["XMLOrdersFilesPath"];
            //string[] filePaths = Directory.GetFiles(Server.MapPath("~/Files/XMLOrders/PDF/"));
            string[] filePaths = Directory.GetFiles(pdfFilesPath).OrderByDescending(f => new FileInfo(f).CreationTime).ToArray();

            //Copy File names to Model collection.
            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {
                files.Add(new FileModel { FileName = Path.GetFileName(filePath) });
            }

            return View(files);
        }

        public FileResult DownloadFile(string fileName)
        {
            var pdfFilesPath = ConfigurationManager.AppSettings["XMLOrdersFilesPath"];

            //Build the File Path.
            //string path = Server.MapPath(pdfFilesPath) + fileName;

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(pdfFilesPath + fileName);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

    }
}