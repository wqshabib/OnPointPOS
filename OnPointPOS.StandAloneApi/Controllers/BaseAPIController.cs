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

namespace POSSUM.StandAloneApi.Controllers
{
    public class BaseAPIController : ApiController
    {
        public string UserId { get; set; }
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
                    UserId = User.Identity.GetUserId();
                    var user = db.Users.FirstOrDefault(u => u.Id == UserId);

                    var outlets = db.Outlet.Where(o => o.CompanyId == user.CompanyId).ToList();
                    var outletUsers = db.OutletUser.ToList();

                    List<string> myoutlets = new List<string>();
                    foreach (var outlet in outlets)
                    {

                        if (outletUsers.FirstOrDefault(u => u.UserId == UserId && u.OutletId == outlet.Id) != null)
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

                UserId = User.Identity.GetUserId();

                LogWriter.LogWrite("GetConnectionString calling " + UserId);

                var manager = new ApplicationUserManager(new UserStore<MasterApplicationUser>(new MasterDbContext()));
                var user = manager.FindById(UserId);
                dbServerName = user.Company.DBServer;
                dbName = user.Company.DBName;
                dbUser = user.Company.DBUser;
                dbPassword = user.Company.DBPassword;
                Company = user.Company;

                string connectionString = "";
                if (!string.IsNullOrEmpty(dbServerName) && !string.IsNullOrEmpty(dbName))
                    connectionString = "Data Source=" + dbServerName + ";Initial Catalog=" + dbName + ";UID=" + (dbUser == null ? "" : dbUser) + ";Password=" + (dbPassword == null ? "" : dbPassword) + ";";
                    
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
