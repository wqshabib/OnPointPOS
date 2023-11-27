using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POSSUM.Api.Models;
using POSSUM.MasterData;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using POSSUM.Data;
using POSSUM.Model;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using System.Net.Mail;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace POSSUM.Api.Controllers
{
    [RoutePrefix("api/Admin")]
    public class AdminController : ApiController
    {
        /// <summary>
        /// Register POS admin User and create databse for new customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(Company model)
        {
            try
            {
                AdminOutlet adminoutlet = null;
                AdminTerminal adminterminal = null;
                using (MasterDbContext db = new MasterDbContext())
                {
                    var company = db.Company.FirstOrDefault(c => c.Name == model.Name);

                    if (company == null)
                    {
                        var DBServer = ConfigurationManager.AppSettings["DBServer"];
                        var DBServerUser = ConfigurationManager.AppSettings["UID"];
                        var DBServerPassword = ConfigurationManager.AppSettings["Password"];
                        var manager = new ApplicationUserManager(new UserStore<MasterApplicationUser>(db));
                        var user = manager.FindByName(model.UserName);
                        if (user == null)
                        {
                            model.Id = Guid.NewGuid();
                            model.DBName = string.IsNullOrEmpty(model.DBName) ? model.Name.Replace(" ", "") : model.DBName.Replace(" ", "");
                            model.DBServer = DBServer;
                            model.DBUser = DBServerUser;
                            model.DBPassword = DBServerPassword;
                            db.Company.Add(model);

                            var newuser = new MasterApplicationUser() { Email = model.UserEmail, UserName = model.UserName, LockoutEndDateUtc = DateTime.Now, CompanyId = model.Id };
                            var result = await manager.CreateAsync(newuser, model.UserPassword);
                            if (result.Succeeded)
                            {

                                AssignRole(newuser.Id, manager, db);
                                adminoutlet = new AdminOutlet
                                {
                                    Id = Guid.NewGuid(),
                                    Name = model.Name,
                                    CompanyId = model.Id

                                };
                                db.Outlet.Add(adminoutlet);
                                adminterminal = new AdminTerminal
                                {
                                    Id = Guid.NewGuid(),
                                    UniqueIdentification = GenerateUniqueCode(db)

                                };

                                db.Terminal.Add(adminterminal);
                                var useroutlet = new MasterData.OutletUser
                                {
                                    Id = Guid.NewGuid(),
                                    OutletId = adminoutlet.Id,
                                    UserId = newuser.Id

                                };
                                db.OutletUser.Add(useroutlet);
                                db.SaveChanges();



                            }
                        }
                        else
                            return Ok(ApiResult(0, "User already exist"));

                    }
                    else
                    {
                        return Ok(ApiResult(0, "Company already exist"));
                    }
                }

                //Create Database
                try
                {

                    string script = @"USE master;  
                            GO 
                            CREATE DATABASE [" + model.DBName + "]; ";

                    string masterConnection = @"Data Source=" + model.DBServer + ";Initial Catalog=master;User Id=" + model.DBUser + ";Password=" + model.DBPassword;
                    SqlConnection conn = new SqlConnection(masterConnection);

                    Server server = new Server(new ServerConnection(conn));
                    server.ConnectionContext.ExecuteNonQuery(script);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return Ok(ApiResult(0, ex.Message));
                }



                try
                {

                    SqlConnection conn = new SqlConnection(model.ConnectionString);

                    Server server = new Server(new ServerConnection(conn));

                    string filePath = System.Web.HttpContext.Current.Server.MapPath("~/SQL/NIMPOSDBScript.sql");
                    string script = File.ReadAllText(filePath);
                    server.ConnectionContext.ExecuteNonQuery(script);

                    filePath = System.Web.HttpContext.Current.Server.MapPath("~/SQL/IndexinginPOS.sql");
                    script = File.ReadAllText(filePath);
                    server.ConnectionContext.ExecuteNonQuery(script);

                    string sql = @"update Outlet set Id='" + adminoutlet.Id + "',Name='" + adminoutlet.Name + "',HeaderText='" + adminoutlet.Name + "',Created='" + DateTime.Now + "',Updated='" + DateTime.Now

+ "' update Terminal set Id='" + adminterminal.Id + "', OutletId='" + adminoutlet.Id + "',UniqueIdentification='" + adminterminal.UniqueIdentification + "',Created='" + DateTime.Now + "',Updated='" + DateTime.Now

+ "' update CashDrawer set TerminalId='" + adminterminal.Id

+ "' update Setting set TerminalId='" + adminterminal.Id + "',OutletId='" + adminoutlet.Id + "' where TerminalId is not null and OutletId is not null";
                    server.ConnectionContext.ExecuteNonQuery(sql);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Ok(ApiResult(0, e.Message));
                }
                string message = @"";
                SendEmail("NIMPOS Registration", message, model.UserEmail, model.UserName, model.UserPassword);
                return Ok(ApiResult(1, "Registered Success fully"));
            }
            catch (Exception e)
            {
                return Ok(ApiResult(0, e.Message));
            }
        }

        protected internal virtual JsonTextActionResult JsonText(string jsonText)
        {
            return new JsonTextActionResult(Request, jsonText);
        }
        private void AssignRole(string userId, ApplicationUserManager UserManager, MasterDbContext db)
        {

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            string RoleName = RoleManager.FindByName("Admin").Name;

            var roles = RoleManager.Roles.ToList();

            // delete all previous role
            foreach (var role in roles)
            {
                UserManager.RemoveFromRole(userId, role.Name);
            }


            if (!UserManager.IsInRole(userId, RoleName))
            {
                UserManager.AddToRole(userId, RoleName);
            }



        }

        public string GenerateUniqueCode(MasterDbContext db)
        {

            var terminasls = db.Terminal.ToList();
            string UniqueCode = "";
            bool done = true;
            while (done)
            {
                string uniquecode = "SENIM-" + RandomString(3) + GenerateRandomNo();
                var exists = terminasls.FirstOrDefault(t => t.UniqueIdentification == uniquecode);
                if (exists == null)
                {
                    UniqueCode = uniquecode;
                    done = false;

                }
            }

            return UniqueCode;
        }
        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }
        public void SendEmail(string subject, string message, string receiver, string userName, string password)
        {
            try
            {
                /*var smtp = "smtp.office365.com";
                var mailAddressFrom = ConfigurationManager.AppSettings["MailAddressFrom"];
                var mailAddressPassword = ConfigurationManager.AppSettings["EmailPassword"];*/
                var cc = ConfigurationManager.AppSettings["MailAddressCC"];

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<div style='padding: 10px 30px 0'>");
                stringBuilder.AppendLine("<p style ='margin: 0 0 10px; padding: 0; font - size:16px; color:#111111'> Hello, " + userName + "</ p >");
                stringBuilder.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111' > Welcome to POSSUM SYSTEM! We're so happy you're here.</ p >");
                stringBuilder.AppendLine("<div style = 'width: 100 %; height: 1px; background:#cfcfcf; margin-top:25px; max-height:1px' ></ div >");
                stringBuilder.AppendLine("<hr/>");
                stringBuilder.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111' > Your login credentials</ p >");
                stringBuilder.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111; font-weight:bold' > User Name=" + userName + "</ p >");
                stringBuilder.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111; font-weight:bold' > Password=" + password + "</ p >");
                stringBuilder.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111; margin-bottom:10px;'> You may login to NIMPOS Admin by clicking this link:</ p > ");
                stringBuilder.AppendLine("<div><a href = 'https://portal.nimpos.com' target = '_blank' rel = 'noopener noreferrer' style = 'font-size:16px; color:#005297; text-decoration:none' > https://portal.nimpos.com </a></div>");
                stringBuilder.AppendLine("</div>");

                new SMTPClient().SendMail(receiver, cc, subject, message, stringBuilder.ToString());
            }
            catch (Exception e)
            {
                //
            }


        }
        private JToken ApiResult(int status, string message)
        {
            // string json = JsonConvert.SerializeObject(new Result("SUCCESS", "Your registration done successfully"));
            return JToken.FromObject(new Result(status, "Your registration done successfully"));
        }

        public class Result
        {
            public Result(int status, string message)
            {
                Status = status;
                Message = message;
            }

            public int Status { get; set; }
            public string Message { get; set; }
        }

        public class JsonTextActionResult : IHttpActionResult
        {
            public HttpRequestMessage Request { get; }

            public string JsonText { get; }

            public JsonTextActionResult(HttpRequestMessage request, string jsonText)
            {
                Request = request;
                JsonText = jsonText;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(Execute());
            }

            public HttpResponseMessage Execute()
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonText, Encoding.UTF8, "application/json");

                return response;
            }
        }
    }
}
