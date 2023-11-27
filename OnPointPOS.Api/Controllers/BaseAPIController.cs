using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.MasterData;
using System.Data.SqlClient;
using System.Configuration;
using POSSUM.Data;

namespace POSSUM.Api.Controllers
{
    public class BaseAPIController : ApiController
    {
        public List<string> AllowedOutlets { get; set; }
        public string CurrentDBName { get; set; }
        public Guid CurrentCompanyId { get; set; }
        public Company Company { get; set; }

        public ApplicationDbContext GetConnection
        {
            get
            {
                string connectionString = "";
                using (MasterData.MasterDbContext db = new MasterData.MasterDbContext())
                {
                    string userID = User.Identity.GetUserId();
                    var user = db.Users.FirstOrDefault(u => u.Id == userID);

                    var outlets = db.Outlet.Where(o => o.CompanyId == user.CompanyId).ToList();
                    var outletUsers = db.OutletUser.ToList();

                    List<string> myoutlets = new List<string>();
                    foreach (var outlet in outlets)
                    {

                        if (outletUsers.FirstOrDefault(u => u.UserId == userID && u.OutletId == outlet.Id) != null)
                        {
                            myoutlets.Add(outlet.Id.ToString());
                        }
                    }
                    AllowedOutlets = myoutlets;
                    CurrentDBName = user.Company.DBName;
                    CurrentCompanyId = user.Company.Id;
                    connectionString = user.Company.ConnectionString;
                }
                return new ApplicationDbContext(connectionString);
            }
        }

        public string GetConnectionString()
        {
            try
            {
                string dbServerName = "";
                string dbName = "";
                string dbUser = "";
                string dbPassword = "";
                string userId = User.Identity.GetUserId();

                LogWriter.LogWrite("GetConnectionString calling " + userId);

                var manager = new ApplicationUserManager(new UserStore<MasterApplicationUser>(new MasterDbContext()));
                var user = manager.FindById(userId);
                dbServerName = user.Company.DBServer;
                dbName = user.Company.DBName;
                dbUser = user.Company.DBUser;
                dbPassword = user.Company.DBPassword;
                Company = user.Company;

                string connectionString = @"Data Source=.\sqlexpress;Initial Catalog=stageshop;User Id=sa;Password=sql2k12;";

                if (string.IsNullOrEmpty(dbServerName) || string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword))
                    connectionString = "";
                else
                    connectionString = "Data Source=" + dbServerName + ";Initial Catalog=" + dbName + ";UID=" + dbUser + ";Password=" + dbPassword + ";";

                return connectionString;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                return "";
            }
        }
    }
}
