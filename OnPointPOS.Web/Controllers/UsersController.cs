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
using POSSUM.Web.Models;
using POSSUM.Data;
using POSSUM.Model;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;



namespace POSSUM.Web.Controllers
{
	[Authorize]
	public class UsersController : MyBaseController
	{
		//public AccountController()
		//    : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(PosState.GetInstance().CreateIdentityUnitOfWork().Session)))
		//{
		//}

		//public AccountController(UserManager<ApplicationUser> userManager)
		//{
		//    UserManager = userManager;
		//}

		public UserManager<ApplicationUser> UserManager { get; private set; }
		public ActionResult Index()
		{
			//  var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(GetConnection));
			var db = GetConnection;
			var data =  db.OutletUser.ToList().Select(usr=> new OutletUserViewModel
                        {
                            Id = usr.Id,
                            UserName = usr.UserName,
                            Email = usr.Email,
                            UserCode = usr.UserCode,
                             Active=usr.Active,
                              TrainingMode=usr.TrainingMode,
                            asc = ""

                        }).ToList();

            return View("TillUser", data);

		}


		//
		// GET: /Account/Login
		[AllowAnonymous]
		public ActionResult Login(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		#region User Role
		public ActionResult RoleList()
		{

			var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(GetConnection));
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
				using (var db = GetConnection)
				{
					var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
					IdentityResult roleResult;

					// Check to see if Role Exists, if not create it
					if (!RoleManager.RoleExists(viewmodel.Name))
					{
						roleResult = RoleManager.Create(new IdentityRole(viewmodel.Name));
						db.SaveChanges();
						msg = "Success:Role created successflly";
					}
					else
						msg = "Error:Already exisit with the same name";
				}

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
				using (var db = GetConnection)
				{
					var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
					IdentityRole roleResult = RoleManager.FindById(id);
					RoleManager.Delete(roleResult);
					db.SaveChanges();
				}
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
			using (var db = GetConnection)
			{
				var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
				var manager = new UserManager<ApplicationUser>(
					  new UserStore<ApplicationUser>(db));
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
		}
		public ActionResult SaveRoles(List<RoleViewModel> roles)
		{
			string msg = "";
			try
			{
				using (var db = GetConnection)
				{
					var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

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
					db.SaveChanges();
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

		#region User Till
		public ActionResult TillUser()
		{
			List<OutletUserViewModel> models = new List<OutletUserViewModel>();
			using (var db = GetConnection)
			{


                //var outlets = db.Outlet.ToList();

                models = db.OutletUser.ToList().Select(usr => new OutletUserViewModel
                {
                    Id = usr.Id,
                    UserName = usr.UserName,
                    Email = usr.Email,
                    UserCode = usr.UserCode,
                    Active = usr.Active,
                    TrainingMode = usr.TrainingMode,
                    asc = ""

                }).ToList();

            }
			return View(models);
		}

        public ActionResult CreateTillUser()
        {
            var model = new OutletUserViewModel();

            using (var db = GetConnection)
            {
                var outlets = db.Outlet.Where(obj => !obj.IsDeleted).ToList();
                if (outlets == null)
                    outlets = new List<Outlet>();
                outlets.Insert(0, new Outlet { Id = default(Guid), Name = "Select outlet" });
                model.Outlets = outlets.Select(ol => new SelectListItem { Value = ol.Id.ToString(), Text = ol.Name }).ToList();
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult CreateTillUser(OutletUser viewModel)
        {

            string msg = "";

            try
            {
                if (!string.IsNullOrEmpty(viewModel.Password))
                {
                    if (viewModel.Password != viewModel.ConfirmPassword)
                    {
                        msg = "Error:Password not confirmed";
                        return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
                    }
                }

                using (var uof = new UnitOfWork(GetConnection))
                {
                    var tillUserRepo = uof.TillUserRepository;

                    var user = new OutletUser();

                    if (string.IsNullOrEmpty(viewModel.Id))
                    {
                        viewModel.Id = Guid.NewGuid().ToString();
                        user = viewModel;
                        if (!string.IsNullOrEmpty(viewModel.Password))
                            user.Password = CalculateSHA1(viewModel.Password, Encoding.UTF8);
                    }
                    else
                    {
                        user = tillUserRepo.FirstOrDefault(u => u.Id == viewModel.Id);
                        user.UserName = viewModel.UserName;
                        user.TrainingMode = viewModel.TrainingMode;
                        if (!string.IsNullOrEmpty(viewModel.Password))
                            user.Password = CalculateSHA1(viewModel.Password, Encoding.UTF8);
                    }

                    user.Updated = DateTime.Now;
                    tillUserRepo.AddOrUpdate(user);

                    uof.Commit();
                }

                msg = "Success: User saved successfully";
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditTillUser(string id)
		{
			var model = new OutletUserViewModel();

			using (var db = GetConnection)
			{
				if (!string.IsNullOrEmpty(id))
				{
					var user = db.OutletUser.FirstOrDefault(u => u.Id == id);
					if (user != null)
					{
						model.Id = user.Id;
						model.UserName = user.UserName;
						model.Email = user.Email;
						model.UserCode = user.UserCode;
						model.Active = user.Active;
						model.TrainingMode = user.TrainingMode;
						model.OutletId = user.OutletId;
					}

				}

                var outlets = db.Outlet.Where(obj => !obj.IsDeleted).ToList();
                if (outlets == null)
                    outlets = new List<Outlet>();
                outlets.Insert(0, new Outlet { Id = default(Guid), Name = "Select outlet" });
                model.Outlets = outlets.Select(ol => new SelectListItem { Value = ol.Id.ToString(), Text = ol.Name }).ToList();
            }

			return PartialView(model);
		}

		[HttpPost]
		public ActionResult EditTillUser(OutletUser viewModel)
		{

			string msg = "";

			try
			{
				if (!string.IsNullOrEmpty(viewModel.Password))
				{
					if (viewModel.Password != viewModel.ConfirmPassword)
					{
						msg = "Error:Password not confirmed";
						return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
					}
				}

				using (var uof = new UnitOfWork(GetConnection))
				{
					var tillUserRepo = uof.TillUserRepository;

					var user = new OutletUser();

					if (string.IsNullOrEmpty(viewModel.Id))
					{
						viewModel.Id = Guid.NewGuid().ToString();
						user = viewModel;
						if (!string.IsNullOrEmpty(viewModel.Password))
							user.Password = CalculateSHA1(viewModel.Password, Encoding.UTF8);
					}
					else
					{
						user = tillUserRepo.FirstOrDefault(u => u.Id == viewModel.Id);
						user.UserName = viewModel.UserName;
						user.TrainingMode = viewModel.TrainingMode;
						if (!string.IsNullOrEmpty(viewModel.Password))
							user.Password = CalculateSHA1(viewModel.Password, Encoding.UTF8);
                        user.OutletId = viewModel.OutletId;
                    }

					user.Updated = DateTime.Now;
					tillUserRepo.AddOrUpdate(user);

					uof.Commit();
				}

				msg = "Success: User saved successfully";
			}
			catch (Exception ex)
			{

				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetPOSUsers()
		{

			List<OutletUser> tillUsers = new List<OutletUser>();
			var db = GetConnection;
			tillUsers = db.OutletUser.Where(u=>u.TrainingMode==false).ToList();

			return Json(tillUsers, JsonRequestBehavior.AllowGet);
		}
		#endregion
		//
		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{

				UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(GetConnection));
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

		[AllowAnonymous]
		public ActionResult Register(string id)
		{
			using (var db = GetConnection)
			{
				UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

				RegisterViewModel model = new RegisterViewModel();
				if (!string.IsNullOrEmpty(id))
					model = (from usr in UserManager.Users.Where(u => u.Id == id)
							 select new RegisterViewModel
							 {
								 Id = usr.Id,
								 UserName = usr.UserName,
								 Email = usr.Email,
								 UserId = "'" + usr.Id + "'",
								 RoleId = default(Guid).ToString(),
								 asc = ""

							 }).FirstOrDefault();



				var manager = new UserManager<ApplicationUser>(
					new UserStore<ApplicationUser>(db));
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

				model.RoleId = roleModels.First().Id.ToString();
				return PartialView(model);
			}
		}

		//
		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (var db = GetConnection)
				{
					UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
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
						var user = new ApplicationUser() { Email = model.Email, UserName = model.UserName, LockoutEndDateUtc = DateTime.Now, PhoneNumber = model.PhoneNumber, Created = DateTime.Now, Active = true };
						user.Password = CalculateSHA1(model.Password, Encoding.UTF8);
						user.TrainingMode = model.TrainingMode;

						var result = await UserManager.CreateAsync(user, model.Password);
						if (result.Succeeded)
						{
							db.SaveChanges();

							AssignRole(user.Id, model.RoleId);
							return RedirectToAction("Index", "Users");
						}
						else
						{
							AddErrors(result);


						}
					}
				}
			}


			using (var db = GetConnection)
			{
				var manager = new UserManager<ApplicationUser>(
					new UserStore<ApplicationUser>(db));
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

				if (model.RoleId == null)
					model.RoleId = roleModels.First().Id.ToString();

			}

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
				using (var db = GetConnection)
				{
					UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

					var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email, LockoutEndDateUtc = DateTime.Now };
					user.Password = CalculateSHA1(model.Password, Encoding.UTF8);
					user.TrainingMode = model.TrainingMode;
					var result = UserManager.Create(user, model.Password);
					if (result.Succeeded)
					{
						db.SaveChanges();


						msg = "User registered successfully";
					}
					else
					{
						msg = result.Errors.First();// "User registered successfully";
						AddErrors(result);
					}
				}
			}

			// If we got this far, something failed, redisplay form
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult doesUserNameExist(string UserName)
		{

			UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(GetConnection));

			var user = UserManager.FindByName(UserName);

			return Json(user == null);
		}

		public JsonResult IsUserExists(string UserName)
		{

			UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(GetConnection));
			var user = UserManager.FindByName(UserName);
			//check if any of the UserName matches the UserName specified in the Parameter using the ANY extension method.  
			return Json(user == null, JsonRequestBehavior.AllowGet);
		}
		private static string CalculateSHA1(string text, Encoding enc)
		{
			try
			{
				byte[] buffer = enc.GetBytes(text);
				var cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
				return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
			}
			catch (Exception exp)
			{

				return string.Empty;
			}
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
			var db = GetConnection;
			var manager = new UserManager<ApplicationUser>(
			  new UserStore<ApplicationUser>(db));
			var user = manager.FindById(Id);

			UserViewModel model = new UserViewModel
			{
				Id = user.Id,
				UserName = user.UserName,
				Password = user.Password,
				TrainingMode = user.TrainingMode,
				Email = user.Email,
				ConfirmPassword = user.Password,
				PhoneNumber = user.PhoneNumber,


			};


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


			model.Roles = roleModels.OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name, Selected = o.IsSelected });
			return PartialView(model);
		}

		[HttpPost]
		public ActionResult EditUser(UserViewModel viewModel)
		{
			string msg = "";
			try
			{

				var db = GetConnection;
				var manager = new UserManager<ApplicationUser>(
				  new UserStore<ApplicationUser>(db));
				var user = manager.FindById(viewModel.Id);

				user.Email = viewModel.Email;
				user.TrainingMode = viewModel.TrainingMode;
				user.PhoneNumber = viewModel.PhoneNumber;
				user.UserName = viewModel.UserName;
				user.Active = true;
				manager.Update(user);
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
				var db = GetConnection;
				//var manager = new UserManager<ApplicationUser>(
				//  new UserStore<ApplicationUser>(db));

				var user = db.OutletUser.FirstOrDefault(u => u.Id == id);
				if (user != null)
				{
					user.Active = false;
					db.SaveChanges();
				}

				msg = "Success:User deactivated successfully.";
			}
			catch (Exception ex)
			{
				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}


		private void AssignRole(string userId, string RoleId)
		{
			var db = GetConnection;
			var manager = new UserManager<ApplicationUser>(
				new UserStore<ApplicationUser>(db));
			var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
			string RoleName = RoleManager.FindById(RoleId).Name;

			var roles = RoleManager.Roles.ToList();

			// delete all previous role
			foreach (var role in roles)
			{
				manager.RemoveFromRole(userId, role.Name);
			}


			if (!manager.IsInRole(userId, RoleName))
			{
				manager.AddToRole(userId, RoleName);
			}



			db.SaveChanges();
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
			return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Users"), User.Identity.GetUserId());
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
				var user = new ApplicationUser() { UserName = model.UserName };
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
			return RedirectToAction("Login", "Users");
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



		#region User Sale
		public ActionResult SaleByUser()
		{
			return View();
		}
		
		#endregion

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

		private async Task SignInAsync(ApplicationUser user, bool isPersistent)
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