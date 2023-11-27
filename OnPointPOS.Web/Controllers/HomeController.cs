using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace POSSUM.Web.Controllers
{
    [Authorize]
    // [Authorize(Roles = "admin")]
    public class HomeController : MyBaseController
    {
        private string connectionString;

        public ActionResult Index()
        {

            Log.WriteLog("home controller index is calling...");

            if (User.IsInRole("Super Admin"))
                return RedirectToAction("Index", "Admin");
            else
            {
                //MasterData.MasterDbContext db = new MasterData.MasterDbContext();
                //string userID = User.Identity.GetUserId();
                //var user = db.Users.FirstOrDefault(u => u.Id == userID);
                //MvcApplication.conString = user.Company.ConnectionString;
                //  MvcApplication.nHibernateHelper = null;
                //string url = HttpContext.Request.Url.Authority;
                // if (url != user.Company.AdminURL)
                //  return Redirect("http://" + user.Company.AdminURL);
                //else
                //  connectionString = GetConnectionString();
                return View(GetReportData());
            }
            //  return View(GetReportData());

        }


        public JsonResult SetProductPLU()
        {
            var msg = "";
            try
            {

                using (var db = GetConnection)
                {
                    var products = db.Product.Where(p => !string.IsNullOrEmpty(p.BarCode) && p.BarCode.Length >= 6 && string.IsNullOrEmpty(p.PLU)).ToList();
                    foreach (var product in products)
                    {
                        var plu = Convert.ToInt32(product.BarCode.Substring(2, 4));
                        product.PLU = plu.ToString();
                        db.Entry(product).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    msg = products.Count() + " Products Data Successfully Added!";
                }
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                msg = "Exception: " + e.ToString();
                return Json(msg, JsonRequestBehavior.AllowGet);

            }
        }







        //private IUnitOfWork GetConnection()
        //{
        //    string url = HttpContext.Request.Url.Authority;

        //    string connectionString = PosState.GetInstance().GetConnectionString(url);
        //   // var helper = new NHibernateHelper(connectionString);
        //    return new UnitOfWork(helper.SessionFactory);
        //}
        public ActionResult ApiTester()
        {
            return View();
        }
        private ReportViewModel GetReportData()
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

                model.Products = GetProducts(startDate, dtTo);

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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";



            return View();
        }

        public ActionResult GetTopProductsSoldByTimeSpan(string timeSpan)
        {
            ReportViewModel model = new ReportViewModel();

            if (timeSpan == "Day")
            {
                DateTime dateNow = DateTime.Now.Date;

                var startDate = dateNow.Year + "-" + dateNow.Month + "-" + dateNow.Day + " 00:00:00";
                var endDate = dateNow.Year + "-" + dateNow.Month + "-" + dateNow.Day + " 23:59:59";

                model.Products = GetProducts(startDate, endDate);
            }
            else if (timeSpan == "Week")
            {
                DateTime dateSub1Week = DateTime.Now.AddDays(-7).Date;
                DateTime dateNow = DateTime.Now.Date;

                var startDate = dateSub1Week.Year + "-" + dateSub1Week.Month + "-" + dateSub1Week.Day + " 00:00:00";
                var endDate = dateNow.Year + "-" + dateNow.Month + "-" + dateNow.Day + " 23:59:59";

                model.Products = GetProducts(startDate, endDate);
            }
            else if (timeSpan == "Month")
            {
                DateTime dateSub1Month = DateTime.Now.AddMonths(-1).Date;
                DateTime dateNow = DateTime.Now.Date;

                var startDate = dateSub1Month.Year + "-" + dateSub1Month.Month + "-" + dateSub1Month.Day + " 00:00:00";
                var endDate = dateNow.Year + "-" + dateNow.Month + "-" + dateNow.Day + " 23:59:59";

                model.Products = GetProducts(startDate, endDate);
            }
            else
            {
                DateTime dateSub1Year = DateTime.Now.AddYears(-1).Date;
                DateTime dateNow = DateTime.Now.Date;

                var startDate = dateSub1Year.Year + "-" + dateSub1Year.Month + "-" + dateSub1Year.Day + " 00:00:00";
                var endDate = dateNow.Year + "-" + dateNow.Month + "-" + dateNow.Day + " 23:59:59";

                model.Products = GetProducts(startDate, endDate);
            }

            return PartialView("TopProductsSold", model);
        }

        public List<ItemViewModel> GetProducts(string dtFrom, string dtTo)
        {
            var model = new ItemViewModel();
            model.Items = new List<ItemViewModel>();

            try
            {
                string query = @"SELECT      top 100 I.Id,I.Description,d.UnitPrice,I.Tax,I.Unit,SUM(d.Qty) as soldQty,Sum(ROUND((d.UnitPrice*d.Qty)-d.ItemDiscount,2)) as TotalSale,Sum(ROUND((d.UnitPrice*d.Qty/(1+d.TaxPercent/100))-d.ItemDiscount,2)) as NetSale,isnull(Sum(Round((d.UnitPrice*d.Qty/(1+d.TaxPercent/100))*(d.TaxPercent/100),2)),0) AS VATSum   from dbo.OrderDetail  as d
                     inner join dbo.Product as I on d.ItemId = I.Id
                     inner join dbo.OrderMaster as M  on M.Id = d.OrderId
            Where M.Status=13  and d.Active=1 AND d.Direction=1 AND  d.ItemType=0 AND (M.CreationDate between '" + dtFrom + "' AND '" + dtTo + "') ";
                if (!string.IsNullOrEmpty(UserOutlets))
                {
                    query += " AND M.OutletId in (" + UserOutlets + ")";
                }
                query += "      group by I.Id,I.Description,d.UnitPrice,I.Tax,I.Unit order by TotalSale desc";
                using (var db = GetConnection)
                {
                    using (var conn = db.Database.Connection)
                    {
                        conn.Open();
                        IDbCommand command = new SqlCommand(query);
                        command.Connection = conn;

                        IDataReader dr = command.ExecuteReader();

                        while (dr.Read())
                        {
                            model.Items.Add(new ItemViewModel
                            {
                                Description = Convert.ToString(dr["Description"]),
                                SoldQty = Convert.ToDecimal(dr["soldQty"]),
                                TotalSale = Convert.ToDecimal(dr["TotalSale"]),
                                NetSale = Convert.ToDecimal(dr["NetSale"]),
                                VatSum = Convert.ToDecimal(dr["VATSum"]),
                                Tax = Convert.ToDecimal(dr["Tax"]),
                                Unit = (ProductUnit)Enum.Parse(typeof(ProductUnit), Convert.ToString(Convert.ToInt16(dr["Unit"]))),
                                Price = Convert.ToDecimal(dr["UnitPrice"]),

                            });
                        }
                        dr.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }


            return model.Items;
        }


        public ActionResult GetTopProducts()
        {

            List<TopProducts> tProducts = new List<TopProducts>();

            using (var db = new ApplicationDbContext(connectionString))
            {

                var categories = db.Category.Where(c => c.CategoryLevel > 1).ToList();
                foreach (var cat in categories)
                {
                    TopProducts product = new TopProducts();
                    product.name = cat.Name;
                    List<decimal> values = new List<decimal>();
                    for (int i = 1; i <= 12; i++)
                    {
                        List<SaleDetail> sales = GetMonthlyReportData(cat.Id, i);
                        decimal monthTotal = 0;
                        foreach (var sale in sales)
                        {
                            monthTotal = monthTotal + sale.GrossTotal;

                        }
                        values.Add(monthTotal);
                    }
                    product.data = values.ToArray();
                    tProducts.Add(product);
                }
            }

            return Json(tProducts, JsonRequestBehavior.AllowGet);

        }
        private List<SaleDetail> GetMonthlyReportData(int categoryId, int month)
        {
            List<SaleDetail> catSales = new List<SaleDetail>();

            try
            {
                int year = DateTime.Now.Year;

                var dtFrom = new DateTime(year, month, 1);
                var dtTo = dtFrom.AddMonths(1).AddDays(-1);

                string query = @"SELECT        cast(OrderMaster.CreationDate as date) as SaleDay,DATEPART(HOUR,OrderMaster.CreationDate) as SaleHour,OrderMaster.CreationDate,  (OrderDetail.Qty* OrderDetail.UnitPrice) as GrossTotal, Category.Name
FROM            ItemCategory INNER JOIN
                         Item ON ItemCategory.ItemId = Item.Id INNER JOIN
                         Category ON ItemCategory.CategoryId = Category.Id INNER JOIN
                         OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId ON Item.Id = OrderDetail.ItemId
						Where OrderMaster.Status=13 AND OrderDetail.Direction=1 and OrderDetail.Active=1 AND  OrderDetail.ItemType=0 AND ItemCategory.CategoryId=" + categoryId + "  AND (OrderMaster.CreationDate between '" + dtFrom + "' AND '" + dtTo + "') ";
                //if (outletId != default(Guid))
                //    query = query + " AND OrderMaster.OutletId='" + outletId + "'";

                using (var db = new ApplicationDbContext(connectionString))
                {
                    using (var conn = db.Database.Connection)
                    {
                        conn.Open();
                        IDbCommand command = new SqlCommand(query);
                        command.Connection = conn;
                        //command.CommandType = CommandType.Text;
                        //command.CommandText = query;
                        //  uof.Session.Transaction.Enlist(command);

                        IDataReader dr = command.ExecuteReader();

                        while (dr.Read())
                        {
                            catSales.Add(new SaleDetail
                            {
                                GroupName = Convert.ToString(dr["Name"]),
                                GrossTotal = Convert.ToDecimal(dr["GrossTotal"]),
                                CreationDate = Convert.ToDateTime(dr["CreationDate"]),
                                SaleDay = Convert.ToDateTime(dr["SaleDay"]).Date,
                                SaleHour = Convert.ToInt16(dr["SaleHour"])

                            });
                        }
                        dr.Dispose();
                    }
                }


            }
            catch (Exception ex)
            {

                //  throw;
            }



            return catSales;
        }

        public class TopProducts
        {

            public TopProducts()
            {

            }
            public string name { get; set; }

            public decimal[] data { get; set; }
        }

        public class Data
        {
            public double Value { get; set; }
        }

        public ActionResult ChangeLanguage(string lang)
        {
            new SiteLanguages().SetLanguage(lang);
            // return RedirectToAction("Index", "Home");

            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}