using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using POSSUM.Data;
using System.Configuration;
using POSSUM.MasterData;
using POSSUM.Web.Models;
using POSSUM.Utils;

namespace POSSUM.Web.Controllers
{
    public class MyBaseController : Controller
    {
        // Here I have created this for execute each time any controller (inherit this) load 
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            Log.WriteLog("BeginExecuteCore is calling...");

            string lang = null;
            HttpCookie langCookie = Request.Cookies["culture"];

            if (langCookie != null)
            {
                try
                {
                    lang = langCookie.Value;
                }
                catch
                {
                    lang = "sv";
                }

            }
            else
            {
                try
                {


                    var userLanguage = Request.UserLanguages;

                    // var userLang = userLanguage != null ? userLanguage[0] : "";

                    // as we need default language as swedish 
                    var userLang = userLanguage != null ? userLanguage[1] : "";
                    if (userLang != "")
                    {
                        lang = userLang;
                    }
                    else
                    {
                        lang = SiteLanguages.GetDefaultLanguage();
                    }
                }
                catch
                {

                    lang = SiteLanguages.GetDefaultLanguage();
                }
            }
            Log.WriteLog("lang... " + lang);
            new SiteLanguages().SetLanguage(lang);
            return base.BeginExecuteCore(callback, state);
        }
        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    //ONLY EXCLUDE LOGIN ACTION
        //    if ((filterContext.ActionDescriptor).ActionName != "Login" && ((filterContext.ActionDescriptor).ControllerDescriptor).ControllerName != "Account")
        //    {
        //        if (Session["UserRole"] == null)
        //            filterContext.Result = new RedirectResult("/Account/Login");
        //    }
        //}

        public string CurrentUserEmail
        {
            get
            {
                using (MasterData.MasterDbContext db = new MasterData.MasterDbContext())
                {
                    string userID = User.Identity.GetUserId();
                    var user = db.Users.FirstOrDefault(u => u.Id == userID);
                    return user.Email;
                }
            }
            set
            {
                using (MasterData.MasterDbContext db = new MasterData.MasterDbContext())
                {
                    string userID = User.Identity.GetUserId();
                    var user = db.Users.FirstOrDefault(u => u.Id == userID);
                    user.Email = value;
                    db.SaveChanges();
                }
            }
        }

public ApplicationDbContext GetConnection
        {
            get
            {
                Log.WriteLog("GetConnection... timestamp. " + DateTime.Now);

                //int multitenant = 0;
                //int.TryParse(ConfigurationManager.AppSettings["Multitenant"], out multitenant);
                string connectionString = "";
                using (MasterData.MasterDbContext db = new MasterData.MasterDbContext())
                {
                    string userID = User.Identity.GetUserId();
                    var user = db.Users.FirstOrDefault(u => u.Id == userID);

                    var outlets = db.Outlet.Where(o => o.CompanyId == user.CompanyId).ToList();
                    var outletUsers = db.OutletUser.ToList();
                    List<AdminUserOutletModel> models = new List<AdminUserOutletModel>();
                    string outletIds = string.Empty;
                    List<string> myoutlets = new List<string>();
                    AllOutlets = new List<string>();
                    foreach (var outlet in outlets)
                    {

                        if (outletUsers.FirstOrDefault(u => u.UserId == userID && u.OutletId == outlet.Id) != null)
                        {
                            if (outletIds.Length > 0)
                                outletIds = outletIds + ",";

                            outletIds += "'" + outlet.Id.ToString() + "'";
                            //var model = new AdminUserOutletModel();
                            //model.UserId = user.Id;
                            //model.OutletId = outlet.Id;
                            //model.Name = outlet.Name;
                            //model.IsSelected = true;
                            //models.Add(model);
                            myoutlets.Add(outlet.Id.ToString());
                        }
                        AllOutlets.Add(outlet.Id.ToString());
                    }
                    AllowedOutlets = myoutlets;
                    UserOutlets = outletIds;
                    CurrentDBName = user.Company.DBName;
                    CurrentCompanyId = user.Company.Id;
                    connectionString = user.Company.ConnectionString;
                }
                return new ApplicationDbContext(connectionString);
            }
        }
        public string UserOutlets { get; set; }
        public List<string> AllOutlets { get; set; }
        public List<string> AllowedOutlets { get; set; }
        public string CurrentDBName { get; set; }
        public Guid CurrentCompanyId { get; set; }
        public string GetConnectionString()
        {

            string connectionString = "";
            using (MasterData.MasterDbContext db = new MasterData.MasterDbContext())
            {
                string userID = User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(u => u.Id == userID);
                connectionString = user.Company.ConnectionString;
            }
            return connectionString;
        }
    }
}