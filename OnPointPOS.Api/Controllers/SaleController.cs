using POSSUM.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.Api.Models;
using POSSUM.MasterData;

namespace POSSUM.Api.Controllers
{
    // [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    [System.Web.Http.RoutePrefix("api/Sale")]
    public class SaleController : ApiController
    {
        private string connectionString = "";
        bool nonAhenticated = false;
        public SaleController()
        {
            string DataServer = ConfigurationManager.AppSettings["DBServer"];
            string UserName = ConfigurationManager.AppSettings["UID"];
            string Password = ConfigurationManager.AppSettings["Password"];
            string dbName = GetDBName();
            if (string.IsNullOrEmpty(dbName))
                nonAhenticated = true;
            connectionString = "Data Source =" + DataServer + ";Initial Catalog=" + dbName + ";UID=" + UserName + ";Password=" + Password + ";";
        }
        /// <summary>
        /// URL: api/SaleGetOutlets
        /// </summary>
        /// <returns>Return the List of Outlets</returns>
        [Route("GetOutlets")]
        public async Task<List<OutletInfo>> GetOutlets()
        {
            if (nonAhenticated)
                return null;
            List<OutletInfo> lstOutlets = new List<OutletInfo>();
            lstOutlets.Add(new OutletInfo { Id = default(Guid), Name = "All restauranger" });
            APIClient client = new APIClient(connectionString);
            var data = client.GetOutlets();
            if (data.Count > 0)
                lstOutlets.AddRange(data);
            return lstOutlets;
        }
        /// <summary>
        /// Get current Day sale
        /// URL: api/GetTodaySale
        /// </summary>
        /// <returns>Return Report View model</returns>
        [Route("GetTodaySale")]
        public async Task<ReportViewModel> GetTodaySale()
        {
            if (nonAhenticated)
                return null;
            APIClient client = new APIClient(connectionString);
            var data = client.GetTodaySale();
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        [Route("GetSaleByMonth")]
        public async Task<ReportViewModel> GetSaleByMonth(int month)
        {
            if (nonAhenticated)
                return null;
            APIClient client = new APIClient(connectionString);
            var data = client.GetSaleByMonth(month);
            return data;
        }
        /// <summary>
        /// Get Curretn Month sale
        /// </summary>
        /// <returns>return MonthlyCategorySale</returns>
        [Route("GetCurrentMonthSale")]
        public async Task<MonthlyCategorySale> GetCurrentMonthSale()
        {
            if (nonAhenticated)
                return null;
            APIClient client = new APIClient(connectionString);
            var data = client.GetCurrentMonthSale();
            return data;
        }
        /// <summary>
        /// Get the given month sale by outlet
        /// </summary>
        /// <param name="outletId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Route("GetMonthalySale")]
        public async Task<MonthlyCategorySale> GetMonthalySale(Guid outletId, int year, int month)
        {
            if (nonAhenticated)
                return null;
            APIClient client = new APIClient(connectionString);
            var data = client.GetMonthlyReportData(outletId, year, month);
            return data;
        }
        [Route("GetDailySale")]
        public async Task<DailyCategorySale> GetDailySale(Guid outletId, DateTime dt)
        {
            if (nonAhenticated)
                return null;
            APIClient client = new APIClient(connectionString);
            var data = client.GetDailyReportData(outletId, dt);
            return data;
        }
        /// <summary>
        /// Get Sale by hours
        /// </summary>
        /// <param name="outletId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        [Route("GetHourlySale")]
        public async Task<List<SaleDetail>> GetHourlySale(Guid outletId, int year, int month, int day)
        {
            if (nonAhenticated)
                return null;
            APIClient client = new APIClient(connectionString);
            DateTime date = Convert.ToDateTime(year + "-" + month + "-" + day);
            var data = client.GetHourlySale(outletId, date);
            return data;
        }
        #region helper
        private string GetDBName()
        {
            try
            {
                string dbName = "";
                string userId = User.Identity.GetUserId();
                var manager = new ApplicationUserManager(new UserStore<MasterApplicationUser>(new MasterDbContext()));
                var user = manager.FindById(userId);
                dbName = user.Company.DBName;
                return dbName;
            }
            catch (Exception)
            {

                return "";
            }
        }
        #endregion
    }
}
