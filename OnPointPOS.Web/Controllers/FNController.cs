using FortnoxApiSDK.Models.Authorization;
using FortnoxNET.Communication;
using FortnoxNET.Communication.Project;
using FortnoxNET.Communication.VoucherSeries;
using FortnoxNET.Services;
using Newtonsoft.Json;
using POSSUM.Data.Common;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    [System.Web.Mvc.AllowAnonymous]
    public class FNController : Controller
    {
        private string ClientId = ConfigurationManager.AppSettings["ClientId"];
        private string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        private string ReturnURL = ConfigurationManager.AppSettings["ReturnURL"];

        public ActionResult Index()
        {
            ViewBag.UserName = "";
            return View("Index");
        }

        public async Task<ActionResult> activation(string code, string state, string response_type)
        {
            ViewBag.Message = "";
            ViewBag.HasError = false;
            var ProjectList = new List<KeyValueViewModel>();
            var VouchersList = new List<KeyValueViewModel>();
            var AccountsList = new List<KeyValueViewModel>();
            if (!string.IsNullOrEmpty(code))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                try
                {
                    var oAuthToken = await AuthorizationService.GetAccessTokenAsync(code, ClientId, ClientSecret, ReturnURL);
                    var AccessToken = oAuthToken.AccessToken;

                    WriteAccessToken(oAuthToken);
                    //Get Profile
                    //try
                    //{
                    //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    //    var result = await CompanyInformationService.GetCompanyInformationAsync(new FortnoxApiRequest(AccessToken, ClientSecret));
                    //    ViewBag.Address = result.Address;
                    //    ViewBag.City = result.City;
                    //    ViewBag.CompanyName = result.CompanyName;
                    //    ViewBag.OrganizationNumber = result.OrganizationNumber;
                    //    ViewBag.ZipCode = result.ZipCode;
                    //}
                    //catch (Exception e)
                    //{
                    //    throw new Exception("Something went wrong while getting profile.", e);
                    //}

                    //Get ProjectList
                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var result = await ProjectService.GetProjectsAsync(new ProjectListRequest(AccessToken, ClientSecret));
                        if (result.Data != null && result.Data.Count() > 0)
                        {
                            foreach (var item in result.Data)
                            {
                                ProjectList.Add(new KeyValueViewModel()
                                {
                                    Value = item.ProjectNumber,
                                    Text = item.ProjectNumber
                                });
                            }
                        }
                        else
                        {
                            var fortnoxApiRequest = new FortnoxApiRequest(AccessToken, ClientSecret);
                            var createProjects = await ProjectService.CreateProjectAsync(fortnoxApiRequest, new FortnoxNET.Models.Project.Project()
                            {
                                Comments = "POSSUM",
                                //ContactPerson = "POSSUM",
                                //Description = "POSSUM Integration",
                                EndDate = DateTime.Now.AddYears(5),
                                //ProjectLeader = "POSSUM",
                                ProjectNumber = "1",
                                StartDate = DateTime.Now.AddDays(-3),
                                Status = FortnoxNET.Models.Project.ProjectStatus.ONGOING,
                                //Url = "https://admin.possumsystem.com/"
                            });

                            var result1 = await ProjectService.GetProjectsAsync(new ProjectListRequest(AccessToken, ClientSecret));
                            if (result1.Data != null && result1.Data.Count() > 0)
                            {
                                foreach (var item in result.Data)
                                {
                                    ProjectList.Add(new KeyValueViewModel()
                                    {
                                        Value = item.ProjectNumber,
                                        Text = item.ProjectNumber
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message += "Error: Something went wrong while getting projects list. " + e.Message + "<br />";
                        ViewBag.HasError = true;
                    }

                    //Get Accounts
                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var result = await AccountService.GetAccountsAsync(new AccountListRequest(AccessToken, ClientSecret));
                        if (result.Data != null && result.Data.Count() > 0)
                        {
                            foreach (var item in result.Data)
                            {
                                AccountsList.Add(new KeyValueViewModel()
                                {
                                    Value = Convert.ToString(item.Number),
                                    Text = Convert.ToString(item.Number)
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message += "Error: Something went wrong while getting accounts list. " + e.Message + "<br />";
                        ViewBag.HasError = true;
                    }

                    //Get Voucher Series
                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var result = await VoucherSeriesService.GetVoucherSeriesAsync(new VoucherSeriesListRequest(AccessToken, ClientSecret));
                        if (result.Data != null && result.Data.Count() > 0)
                        {
                            foreach (var item in result.Data)
                            {
                                VouchersList.Add(new KeyValueViewModel()
                                {
                                    Value = Convert.ToString(item.Code),
                                    Text = Convert.ToString(item.Code)
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message += "Error: Something went wrong while getting Vouchers list. " + e.Message + "<br />";
                        ViewBag.HasError = true;
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Message += "Error: Something went wrong while getting token. " + e.Message + "<br />";
                    ViewBag.HasError = true;
                }

                if (string.IsNullOrEmpty(ViewBag.Message))
                {
                    ViewBag.Message = "Success: Login completed, please choose project, account and voucher series and press Save button. Sync will auto start in background.";
                }

                ViewBag.Code = code;
                ViewBag.response_type = response_type;
                ViewBag.State = state;
            }
            else
            {
                var lastMessageFile = ConfigurationManager.AppSettings["FNLastMessage"];
                if (System.IO.File.Exists(lastMessageFile))
                {
                    var txt = System.IO.File.ReadAllText(lastMessageFile);
                    ViewBag.Message = txt;
                    ViewBag.HasError = txt.StartsWith("Error:");
                }
            }

            ViewBag.ProjectList = ProjectList;
            ViewBag.VouchersList = VouchersList;
            ViewBag.AccountsList = AccountsList;

            return View();
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<JsonResult> SaveSetting(FNSettingViewModel model)
        {
            try
            {
                var settingFile = ConfigurationManager.AppSettings["FNSetting"];
                System.IO.File.WriteAllText(settingFile, JsonConvert.SerializeObject(model));
                var lastMessageFile = ConfigurationManager.AppSettings["FNLastMessage"];
                System.IO.File.WriteAllText(lastMessageFile, @"Setting saved successfully, it will sync vouchers in background.");
                return Json("Setting Saved.");
            }
            catch (Exception ex)
            {
                return Json("Something went wrong, please try again. " +ex.Message);
            }
        }

        private void WriteAccessToken(OAuthToken oAuthToken)
        {
            var tokenFilePath = ConfigurationManager.AppSettings["FNTokenFile"];
            System.IO.File.WriteAllText(tokenFilePath, JsonConvert.SerializeObject(oAuthToken));
        }

        public async Task<JsonResult> GetAuthToken(string authorization)
        {
            StringBuilder myStringBuilder = new StringBuilder("Get GetAuthToken is Calling!");
            myStringBuilder.AppendLine();

            if (!string.IsNullOrEmpty(authorization))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                var oAuthToken = new OAuthToken();
                try
                {
                    oAuthToken = await AuthorizationService.GetAccessTokenAsync(authorization, ClientId, ClientSecret, ReturnURL);
                    myStringBuilder.Append("oAuthToken Success Result:   " + JsonConvert.SerializeObject(oAuthToken));
                    myStringBuilder.AppendLine();
                }
                catch (Exception e)
                {
                    myStringBuilder.Append("oAuthToken Exception Result:   " + e.ToString());
                    myStringBuilder.AppendLine();
                }

                return Json(myStringBuilder.ToString(), JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Authorization code is empty", JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> GetRefreshToken(string refreshToken)
        {
            StringBuilder myStringBuilder = new StringBuilder("Get RefreshToken is Calling!");
            myStringBuilder.AppendLine();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshTokn = new OAuthToken();
                try
                {
                    refreshTokn = await AuthorizationService.RefreshTokenAsync(ClientId, ClientSecret, refreshToken);
                    myStringBuilder.Append("Refresh Token Success Result:   " + JsonConvert.SerializeObject(refreshTokn));
                    myStringBuilder.AppendLine();
                }
                catch (Exception e)
                {
                    myStringBuilder.Append("Refresh Token Exception Result:   " + e.ToString());
                    myStringBuilder.AppendLine();
                }

                return Json(myStringBuilder.ToString(), JsonRequestBehavior.AllowGet);

            }
            else
                return Json("RefreshToken code is empty", JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpPost]
        public async Task<JsonResult> GetCompanyInfo(OAuthToken authToken)
        {
            StringBuilder myStringBuilder = new StringBuilder("Get GetCompanyInfo is Calling!");
            myStringBuilder.AppendLine();
            if (!string.IsNullOrEmpty(authToken.AccessToken))
            {
                var ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    var result = await CompanyInformationService.GetCompanyInformationAsync(new FortnoxApiRequest(authToken.AccessToken, ClientSecret));

                    myStringBuilder.Append("Company info Success Result:   " + JsonConvert.SerializeObject(result));
                    myStringBuilder.AppendLine();
                }
                catch (Exception e)
                {
                    myStringBuilder.Append("Refresh Token Exception Result:   " + e.ToString());
                    myStringBuilder.AppendLine();
                }

                return Json(myStringBuilder.ToString(), JsonRequestBehavior.AllowGet);

            }
            else
                return Json("AuthToken code is empty", JsonRequestBehavior.AllowGet);
        }






        public async Task<string> GetToken(string authCode)
        {
            try
            {
                StringBuilder myStringBuilder = new StringBuilder("Get Token is Calling!");
                myStringBuilder.AppendLine();
                var AuthorizationCode = authCode;
                var ClientId = ConfigurationManager.AppSettings["ClientId"];
                var ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                var ReturnURL = ConfigurationManager.AppSettings["ReturnURL"];
                var FNTokenFile = ConfigurationManager.AppSettings["FNTokenFile"];

#if DEBUG
                FNTokenFile = @"E:\token.txt";
#endif
                var strTokenResult = System.IO.File.ReadAllText(FNTokenFile);

                if (!string.IsNullOrEmpty(strTokenResult))
                {
                    myStringBuilder.Append("strTokenResult is not null:");
                    myStringBuilder.AppendLine();

                    myStringBuilder.Append("strTokenResult: " + strTokenResult);
                    myStringBuilder.AppendLine();

                    return myStringBuilder.ToString();
                }
                else
                {
                    myStringBuilder.Append("strTokenResult is null");
                    myStringBuilder.AppendLine();

                    myStringBuilder.Append("oAuthToken Calling: ");
                    myStringBuilder.AppendLine();

                    var oAuthToken = await AuthorizationService.GetAccessTokenAsync(AuthorizationCode, ClientId, ClientSecret, ReturnURL);
                    myStringBuilder.Append("oAuthToken Success Result: " + oAuthToken);
                    myStringBuilder.AppendLine();

                    myStringBuilder.Append("Refresh Token now");
                    myStringBuilder.AppendLine();

                    var refreshToken = await AuthorizationService.RefreshTokenAsync(ClientId, ClientSecret, oAuthToken.RefreshToken);
                    myStringBuilder.Append("Refresh Token Success Result: " + refreshToken);
                    myStringBuilder.AppendLine();

                    System.IO.File.WriteAllText(FNTokenFile, JsonConvert.SerializeObject(refreshToken));
                    myStringBuilder.Append("Refresh Token Saved in File");
                    myStringBuilder.AppendLine();

                    myStringBuilder.Append(System.IO.File.ReadAllText(FNTokenFile));


                    return myStringBuilder.ToString();
                }
            }
            catch (Exception e)
            {

                return "Get Token Exception: " + e.ToString();
            }
        }
    }
}
