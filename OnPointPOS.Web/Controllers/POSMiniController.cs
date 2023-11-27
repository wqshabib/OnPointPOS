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

namespace POSSUM.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class POSMiniController : MyBaseController
    {

        public POSMiniController()
        {
            //  uof = PosState.GetInstance().CreateUnitOfWork();
        }
        public ActionResult Index()
        {
            return View(GetReportData());
        }

        public ActionResult Reportgenerator()
        {
            return View();
        }

        public ActionResult ExportFile()
        {
            return View();
        }
        public ActionResult CustomerInvoice()
        {
            return View();
        }

        public PartialViewResult _partialPendingInvoices(Guid customerId, string dtFrom, string dtTo)
        {
            CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
            DateTime startDate = Convert.ToDateTime(dtFrom).Date;
            DateTime endDate = Convert.ToDateTime(dtTo);
            var model = handler.CustomerIsInIncoiceOrderSearch(customerId, startDate, endDate, GetConnection);
            return PartialView(model);
        }
        public ActionResult GenerateCustomerInvoice(string ids, Guid customerId, string remanrks)
        {
            string msg = "";
            try
            {
                ids = ids.TrimEnd(',');
                List<string> orderGuid = ids.Split(',').ToList();//.Select(string).ToList();
                CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
                handler.GenerateCustomerIsInInvoice(orderGuid, customerId, remanrks, GetConnection);
                msg = "Success: Ready for Invoice successfully";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInvoicedCustomers()
        {

            using (var db = GetConnection)
            {
                List<CustomerViewModel> customers = new List<CustomerViewModel>();
                customers.Add(new CustomerViewModel { Id = default(Guid), Name = Resource.Select });

                var data = (from cust in db.Customer
                            join ord in db.OrderMaster on cust.Id equals ord.CustomerId
                            where cust.HasDeposit == false
                            select new CustomerViewModel
                            {
                                Id = cust.Id,
                                Name = cust.Name
                            }).Distinct().ToList();
                if (data.Count > 0)
                    customers.AddRange(data);
                return Json(customers, JsonRequestBehavior.AllowGet);

            }


        }



        public FileResult DownloadFile(int reportSource, string _dtFrom, string _dtTo)
        {
            try
            {
                StringBuilder b = new StringBuilder();
                string terminalcode = "";
                string exportDate = "";
                //Terminal terminal = new Terminal();
                //Outlet outlet = new Outlet();
                List<AccountingData> data = new List<AccountingData>();
                using (ApplicationDbContext db = GetConnection)
                {
                    //Guid terminalId = Guid.Parse(_terminalId);
                    DateTime dateFrom = Convert.ToDateTime(_dtFrom).Date;
                    DateTime dateTo = Convert.ToDateTime(_dtTo + "  11:59:00 PM");

                    //terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                    //outlet.Id = terminal.Outlet.Id;
                    //outlet.Name = terminal.Outlet.Name;
                    //outlet.OrgNo = terminal.Outlet.OrgNo;
                    //terminalcode = terminal.UniqueIdentification;
                    exportDate = dateFrom.Year.ToString() + dateFrom.Month.ToString() + dateFrom.Day.ToString() + "_" + dateTo.Year.ToString() + dateTo.Month.ToString() + dateTo.Day.ToString();

                    //string query = "exec SP_ExportAccounting '" + terminalId + "', '" + dateFrom + "','" + dateTo + "'";
                    string query = "exec SP_ExportAccountingPOSMini " + reportSource + ", '" + dateFrom + "','" + dateTo + "'";
                    data = db.Database.SqlQuery<AccountingData>(query).ToList();
                }

                string month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
                string day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
                var generatedOn = DateTime.Now.Year.ToString() + month + day;

                string newline = Environment.NewLine;
                b.Append("#FLAGGA 0");
                b.Append(newline);
                b.Append("#FORMAT PC8");
                b.Append(newline);
                b.Append("#SIETYP 4");
                b.Append(newline);
                b.Append("#PROGRAM \"POSSUM SE\" 1.0");
                b.Append(newline);
                b.Append("#GEN " + generatedOn);
                b.Append(newline);
                //b.Append("#FNAMN \"" + outlet.Name + "\"");
                //b.Append(newline);
                //if (!string.IsNullOrEmpty(outlet.OrgNo))
                //{
                //    b.Append("#ORGNR " + outlet.OrgNo);
                //    b.Append(newline);
                //}

                b.Append("#VALUTA SEK");
                b.Append(newline);

                List<string> dataTypes = new List<string>();
                var groups = data.GroupBy(o => new { o.SaleDay });
                foreach (var grp in groups)
                {
                    foreach (var dr in grp.OrderBy(o => o.SortOrder))
                    {
                        if (!dataTypes.Contains(dr.DataType) && dr.DataType != "TipAmount")
                        {
                            string row = "#KONTO " + dr.DataType + " \"" + dr.DataTypeText + "\"";
                            b.Append(row);
                            b.Append(newline);
                            dataTypes.Add(dr.DataType);
                        }
                    }
                }

                foreach (var grp in groups)
                {
                    var d = grp.First();
                    b.Append("#VER " + "\"" + "\"" + " " + "\"" + d.DataType + "\"" + " " + d.SaleDay.Replace("-", "") + "" + " " + "\"" + terminalcode + "" + " Sales\"" + "");
                    b.Append(newline);
                    b.Append("{");
                    b.Append(newline);
                    var payments = grp.Where(p => p.SortOrder < 2).Sum(p => p.Amount);
                    var rounded = grp.Where(p => p.SortOrder == 2).Sum(p => p.Amount);
                    var netsale = grp.Where(p => p.SortOrder > 2 && p.SortOrder < 50).Sum(p => p.Amount);
                    var returnPayment = grp.Where(p => p.SortOrder == 50).Sum(p => p.Amount);
                    var returnSale = grp.Where(p => p.SortOrder > 50).Sum(p => p.Amount);

                    var res = payments - returnSale - returnPayment + rounded + netsale;
                    if (res != 0)
                        rounded = rounded - res;
                    foreach (var dr in grp.OrderBy(o => o.SortOrder))
                    {
                        if (dr.SortOrder == 2)
                        {
                            string row = "#TRANS " + dr.DataType + " {} " + rounded.ToString().Replace(',', '.') + " " + dr.SaleDay.Replace("-", "") + " \"" + dr.DataTypeText + "\"";
                            b.Append(row);
                        }
                        else
                        {
                            {
                                if (dr.DataType == "2820")
                                {
                                    string row = "#TRANS " + dr.DataType + " {} -" + dr.DataValue + " " + dr.SaleDay.Replace("-", "") + " \"" + dr.DataTypeText + "\"";
                                    b.Append(row);
                                }
                                else
                                {
                                    string row = "#TRANS " + dr.DataType + " {} " + dr.DataValue + " " + dr.SaleDay.Replace("-", "") + " \"" + dr.DataTypeText + "\"";
                                    b.Append(row);
                                }
                            }
                        }
                        b.Append(newline);
                    }

                    b.Append("}");
                    b.Append(newline);
                }


                Encoding iso = Encoding.GetEncoding("ISO-8859-1");//For western language uni code
                var byteArray = iso.GetBytes(b.ToString()); // Encoding.ASCII.GetBytes(b.ToString());
                var stream = new MemoryStream(byteArray);
                string fileName = terminalcode + "_" + exportDate;
                return File(stream, "text/plain", fileName + ".si");
            }
            catch (Exception ex)
            {
                return null;
            }
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
                    //var orders = db.OrderMaster.Include("OrderLines").Where(ord => ord.Status == OrderStatus.Completed && (ord.CreationDate >= dtFrom && ord.CreationDate <= dtTo && ord.Outlet != null)).ToList();

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
                        Name = t.UniqueIdentification
                    }).ToList();
                else
                    terminals = db.Terminal.Where(c => c.IsDeleted == false).Select(t => new TerminalData
                    {
                        Id = t.Id,
                        Name = t.UniqueIdentification
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

        public ActionResult PrintDetailReport(int reportSource, string dtFrom, string dtTo)
        {
            try
            {
                DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
                DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");
                var info = ReportGenerator.PrintDetailReportByDateRangeForPOSMini(reportSource, dateFrom, dateTo, User.Identity.Name, GetConnection);
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
            IEnumerable<ItemViewModel> filteredItems = allItems.Where(s => s.TotalSale > 0);

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

        private List<ItemViewModel> GetCategorySaleData(DateTime dateFrom, DateTime dateTo, string catId, Guid outletId)
        {

            var model = new ItemViewModel();
            string query = @"SELECT   dbo.Fn_CategoryIdByItem(Product.Id) as CatId , dbo.Fn_CategoryByItem(Product.Id) as CategoryName,Product.Description,Sum(OrderDetail.Qty*OrderDetail.Direction) as soldQty,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent , Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) as TotalSale,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NETSale,isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) as VATSum
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

                    query = query + " GROUP BY   dbo.Fn_CategoryIdByItem(Product.Id), dbo.Fn_CategoryByItem(Product.Id),Product.Description,OrderDetail.UnitPrice,Product.Unit,OrderDetail.TaxPercent";
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
    }



    //public class ReportInfo
    //{
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    public string Address1 { get; set; }
    //    public string City { get; set; }
    //    public string Address { get; set; }
    //    public string Phone { get; set; }
    //    public string Email { get; set; }
    //    public string URL { get; set; }
    //    public string Logo { get; set; }
    //    public string OrgNo { get; set; }
    //    public string Footer { get; set; }
    //    public string Header { get; set; }
    //    public string TaxDescription { get; set; }
    //    public List<ReportRow> ReportRows { get; set; }
    //    public string PostalCode { get; internal set; }
    //}
}