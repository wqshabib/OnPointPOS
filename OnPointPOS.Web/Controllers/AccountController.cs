using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.Model;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using POSSUM.MasterData;
using POSSUM.Web.Models;
using System.Configuration;
using Microsoft.Owin;

namespace POSSUM.Web.Controllers
{
    [Authorize]
    public class AccountController : MyBaseController
    {

        private MasterApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(MasterApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public MasterApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<MasterApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        public ActionResult Index()
        {
            Log.WriteLog("Index...");

            var data = (from usr in UserManager.Users
                        select new RegisterViewModel
                        {
                            Id = usr.Id,
                            UserName = usr.UserName,
                            Email = usr.Email,
                            UserId = "'" + usr.Id + "'",
                            asc = "",
                            Company = usr.Company.Name

                        }).ToList();
            return View(data);

        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {

#if DEBUG
                                    ViewBag.ReturnUrl = "";

#else
            Log.WriteLog("Login... return url");

            ViewBag.ReturnUrl = "";
            var scheme = HttpContext.Request.Url.Scheme;
            var url = ConfigurationManager.AppSettings["BaseURL"];
            Log.WriteLog("Login url... url + scheme+ " + url + scheme);
            if (scheme != "https" && !string.IsNullOrEmpty(url))
                return Redirect(url);
#endif


            ViewBag.ReturnUrl = "";

            return View(new LoginViewModel());
        }
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            Log.WriteLog("Login...login ");

            if (ModelState.IsValid)
            {

                var _user = await UserManager.FindByEmailAsync(model.UserName);
                if (_user != null)
                    model.UserName = _user.UserName;
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        #region User Role
        public ActionResult RoleList()
        {

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(MasterDbContext.Create()));
            var data = (from role in RoleManager.Roles
                        select new RoleViewModel
                        {
                            Id = role.Id,
                            Name = role.Name,
                            asc = ""

                        }).ToList();
            return View(data);


            //  return View();
        }
        public ActionResult AddRole()
        {

            RoleViewModel model = new RoleViewModel();
            return PartialView(model);

        }
        [HttpPost]
        public ActionResult AddRole(RoleViewModel viewmodel)
        {
            string msg = "";
            try
            {

                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(MasterDbContext.Create()));
                IdentityResult roleResult;

                // Check to see if Role Exists, if not create it
                if (!RoleManager.RoleExists(viewmodel.Name))
                {
                    roleResult = RoleManager.Create(new IdentityRole(viewmodel.Name));

                    msg = "Success:Role created successflly";
                }
                else
                    msg = "Error:Already exisit with the same name";

            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult RemoveRole(string id)
        {
            string msg = "";
            try
            {

                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(MasterDbContext.Create()));
                IdentityRole roleResult = RoleManager.FindById(id);
                RoleManager.Delete(roleResult);

                msg = "Role deleted successfully";

            }
            catch (Exception ex)
            {

                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UserRole(string userId)
        {

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(MasterDbContext.Create()));
            var manager = UserManager;
            ViewBag.Id = userId;
            var roles = RoleManager.Roles.ToList();
            List<RoleViewModel> models = new List<RoleViewModel>();
            foreach (var role in roles)
            {
                var model = new RoleViewModel();
                model.Id = role.Id;
                model.Name = role.Name;
                model.UserId = userId;

                if (manager.IsInRole(userId, role.Name))
                {
                    model.IsSelected = true;
                }
                else
                {
                    if (role.Name.ToLower().Contains("till user"))
                        model.IsSelected = true;
                    else
                        model.IsSelected = false;
                }

                models.Add(model);
            }
            return View(models);
        }
        public ActionResult SaveRoles(List<RoleViewModel> roles)
        {

            var manager = UserManager;

            string msg = "";
            try
            {
                foreach (var role in roles)
                {

                    if (role.IsSelected)
                    {
                        if (!manager.IsInRole(role.UserId, role.Name))
                        {
                            manager.AddToRole(role.UserId, role.Name);
                        }
                    }
                    else
                    {
                        if (manager.IsInRole(role.UserId, role.Name))
                        {
                            manager.RemoveFromRole(role.UserId, role.Name);
                        }
                    }
                }

                msg = "Role(s) assigned successfully";
            }
            catch (Exception ex)
            {

                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        //

        [AllowAnonymous]
        public ActionResult Register(string id)
        {

            RegisterMasterUserViewModel model = new RegisterMasterUserViewModel();
            if (!string.IsNullOrEmpty(id))
                model = (from usr in UserManager.Users.Where(u => u.Id == id)
                         select new RegisterMasterUserViewModel
                         {
                             Id = usr.Id,
                             UserName = usr.UserName,
                             Email = usr.Email,
                             UserId = "'" + usr.Id + "'",
                             CompanyId = usr.CompanyId,
                             RoleId = default(Guid).ToString(),
                             asc = ""

                         }).FirstOrDefault();


            MasterDbContext db = new MasterDbContext();
            var manager = new UserManager<MasterApplicationUser>(
                new UserStore<MasterApplicationUser>(db));
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            var roles = RoleManager.Roles.ToList();


            List<RoleViewModel> roleModels = new List<RoleViewModel>();
            foreach (var role in roles)
            {
                var _role = new RoleViewModel();
                _role.Id = role.Id;
                _role.Name = role.Name;

                roleModels.Add(_role);
            }

            model.Roles = roleModels.OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });

            var companies = db.Company.Where(c => c.Active == true).ToList();
            model.Companies = companies.Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });
            model.RoleId = roleModels.First().Id.ToString();
            return PartialView(model);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterMasterUserViewModel model)
        {
            MasterDbContext db = new MasterDbContext();
            if (ModelState.IsValid)
            {

                var alreadyUser = UserManager.FindByName(model.UserName);
                if (alreadyUser != null)
                {
                    AddErrors(new IdentityResult(model.UserName + " Code already exists"));
                }
                else if (UserManager.FindByEmail(model.Email) != null)
                {
                    AddErrors(new IdentityResult(model.Email + " email already registered"));
                }
                else
                {

                    var user = new MasterApplicationUser() { Email = model.Email, UserName = model.UserName, LockoutEndDateUtc = DateTime.Now, PhoneNumber = model.PhoneNumber, CompanyId = model.CompanyId };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {

                        AssignRole(user.Id, model.RoleId);
                        var outlet = db.Outlet.FirstOrDefault(o => o.CompanyId == user.CompanyId);
                        if (outlet != null)
                        {
                            var outletUser = new MasterData.OutletUser
                            {
                                Id = Guid.NewGuid(),
                                UserId = user.Id,
                                OutletId = outlet.Id
                            };
                            db.OutletUser.Add(outletUser);
                            db.SaveChanges();
                        }
                        return RedirectToAction("Index", "Account");
                    }
                    else
                    {
                        AddErrors(result);


                    }
                }
            }



            var manager = new UserManager<MasterApplicationUser>(
                new UserStore<MasterApplicationUser>(MasterDbContext.Create()));
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(MasterDbContext.Create()));

            var roles = RoleManager.Roles.ToList();


            List<RoleViewModel> roleModels = new List<RoleViewModel>();
            foreach (var role in roles)
            {
                var _role = new RoleViewModel();
                _role.Id = role.Id;
                _role.Name = role.Name;

                roleModels.Add(_role);
            }


            model.Roles = roleModels.OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });

            if (model.RoleId == null)
                model.RoleId = roleModels.First().Id.ToString();

            var companies = db.Company.Where(c => c.Active == true).ToList();
            model.Companies = companies.Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });



            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveUser(RegisterViewModel model)
        {
            string msg = "";
            //  model.UserName = model.Email;
            if (ModelState.IsValid)
            {

                UserManager = new MasterApplicationUserManager(new UserStore<MasterApplicationUser>(MasterDbContext.Create()));

                var user = new MasterApplicationUser() { UserName = model.UserName, Email = model.Email, LockoutEndDateUtc = DateTime.Now };
                var result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {



                    msg = "User registered successfully";
                }
                else
                {
                    msg = result.Errors.First();// "User registered successfully";
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult doesUserNameExist(string UserName)
        {

            UserManager = new MasterApplicationUserManager(new UserStore<MasterApplicationUser>(MasterDbContext.Create()));

            var user = UserManager.FindByName(UserName);

            return Json(user == null);
        }

        public JsonResult IsUserExists(string UserName)
        {
            UserManager = new MasterApplicationUserManager(new UserStore<MasterApplicationUser>(MasterDbContext.Create()));
            var user = UserManager.FindByName(UserName);
            //check if any of the UserName matches the UserName specified in the Parameter using the ANY extension method.  
            return Json(user == null, JsonRequestBehavior.AllowGet);
        }




        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        public ActionResult EditUser(string Id)
        {
            MasterDbContext db = new MasterDbContext();
            var user = db.Users.FirstOrDefault(u => u.Id == Id);

            UserViewModel model = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CompanyId = user.CompanyId

            };


            var manager = new UserManager<MasterApplicationUser>(
                new UserStore<MasterApplicationUser>(db));
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            var roles = RoleManager.Roles.ToList();


            List<RoleViewModel> roleModels = new List<RoleViewModel>();


            foreach (var role in roles)
            {
                var _role = new RoleViewModel();
                _role.Id = role.Id;
                _role.Name = role.Name;
                _role.UserId = Id;

                if (manager.IsInRole(Id, role.Name))
                {
                    _role.IsSelected = true;
                }
                else
                {
                    if (role.Name.ToLower().Contains("till user"))
                        _role.IsSelected = true;
                    else
                        _role.IsSelected = false;
                }
                roleModels.Add(_role);
            }
            var companies = db.Company.ToList();
            model.Companies = companies.Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });
            model.Roles = roleModels.OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name, Selected = o.IsSelected });
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult EditUser(UserViewModel viewModel)
        {
            string msg = "";
            try
            {
                MasterDbContext db = new MasterDbContext();
                var user = db.Users.FirstOrDefault(u => u.Id == viewModel.Id);

                user.Email = viewModel.Email;
                user.UserName = viewModel.UserName;
                user.CompanyId = viewModel.CompanyId;
                user.Active = true;
                if (!string.IsNullOrEmpty(viewModel.Password))
                {
                    var manager = new UserManager<MasterApplicationUser>(
                        new UserStore<MasterApplicationUser>(db));
                    var usr = manager.FindById(viewModel.Id);
                    var store = new UserStore<MasterApplicationUser>(db);

                    var newPasswordHash = manager.PasswordHasher.HashPassword(viewModel.Password);
                    store.SetPasswordHashAsync(usr, newPasswordHash);


                }
                db.SaveChanges();

                AssignRole(user.Id, viewModel.RoleId);

                msg = "Success:User updated successfully.";

            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteUser(string id)
        {

            string msg = "";
            try
            {
                MasterDbContext db = new MasterDbContext();

                var user = db.Users.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    user.Active = false;
                    db.SaveChanges();
                }

                msg = "Success:User deleted successfully.";
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }


        private void AssignRole(string userId, string RoleId)
        {

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(MasterDbContext.Create()));
            string RoleName = RoleManager.FindById(RoleId).Name;

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

        [AllowAnonymous]
        public ActionResult ResetPasswordRequest()
        {
            return View(new ResetPasswordRequestViewModel());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPasswordRequest(ResetPasswordRequestViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Email))
            {
                var _user = UserManager.FindByEmail(model.Email);
                if (_user != null)
                {
                    string code = UserManager.GeneratePasswordResetToken(_user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = _user.Id, code = code }, protocol: Request.Url.Scheme);
                    UserManager.SendEmail(_user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
                else
                {
                    return View("ResetPasswordConfirmation");
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            return View(new ResetPasswordViewModel() { Code = code });
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordSuccess()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = UserManager.FindByEmail(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordSuccess", "Account");
            }
            
            var result = UserManager.ResetPassword(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordSuccess", "Account");
            }

            AddErrors(result);
            return View();
        }


        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.Email });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new MasterApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/LogOff
        // [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(MasterApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion



    }
}