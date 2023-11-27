using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using POSSUM.Model;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.Data;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using static POSSUM.Api.Models.DibsOrderInformation;
using POSSUM.Api.Utility;
using POSSUM.Api.Models;


namespace POSSUM.Api.Controllers
{
	// [EnableCors(origins: "*", headers: "*", methods: "*")]
	// [Authorize]
	[System.Web.Http.RoutePrefix("api/User")]
	public class UserController : BaseAPIController
	{
		string connectionString = "";
		bool nonAhenticated = false;
		public UserController()
		{
			connectionString = GetConnectionString();
			if (string.IsNullOrEmpty(connectionString))
				nonAhenticated = true;
		}


		/// <summary>
		/// Get POSSUM SYSTEM users updated or created with in a date range
		/// </summary>
		/// <param name="dates"></param>
		/// <returns></returns>
		[Route("GetUsers")]
		public async Task<UserData> GetUsers([FromUri] Dates dates)
		{
			try
			{


				DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
				DateTime EXECUTED_DATETIME = dates.CurrentDate;
				List<OutletUser> liveTillUsers = new List<OutletUser>();
				List<Role> liveRoles = new List<Role>();
				List<User> liveUsers = new List<User>();
				List<UserRole> UserRoles = new List<UserRole>();
				using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
				{

					//var liveUsersRepo = new Repository<User, string>(uof.Session);
					//var liveTillUsersRepo = new Repository<TillUser, string>(uof.Session);

					//var liveRoleRepo = new Repository<Role, string>(_session);
					//liveUsers = (from usr in db.Users                                 
					//             where (usr.Updated > LAST_EXECUTED_DATETIME && usr.Updated <= EXECUTED_DATETIME)
					//             select usr).Select(u => new User
					//             {
					//                 Id = u.Id,
					//                 AccessFailedCount = u.AccessFailedCount,
					//                 Active = u.Active,
					//                 Created = u.Created,
					//                 Email = u.Email,
					//                 EmailConfirmed = u.EmailConfirmed,
					//                 LockoutEnabled = u.LockoutEnabled,
					//                 LockoutEndDateUtc=DateTime.Now,
					//                 Password = u.Password,
					//                 PasswordHash = u.PasswordHash,
					//                 PhoneNumber = u.PhoneNumber,
					//                 PhoneNumberConfirmed = u.PhoneNumberConfirmed,
					//                 SecurityStamp = u.SecurityStamp,
					//                 TrainingMode = u.TrainingMode,
					//                 TwoFactorEnabled = u.TwoFactorEnabled,
					//                 Updated = u.Updated,
					//                 UserName = u.UserName,
					//                 DallasKey=u.DallasKey

					//             }).ToList();

					//var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
					//if (liveUsers.Count > 0)
					//{

					//    liveRoles = RoleManager.Roles.Select(r => new Role
					//    {
					//        Id = r.Id,
					//        Name = r.Name
					//    }).ToList();
					//}

					liveTillUsers = db.OutletUser.Where(usr => usr.Updated > LAST_EXECUTED_DATETIME).ToList();
					//var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));


					//UserRoles =  (from u in manager.Users
					//                   from ur in u.Roles
					//                   join r in RoleManager.Roles on ur.RoleId equals r.Id
					//                   select new UserRole
					//                   {
					//                       UserId = u.Id,
					//                       UserName = u.UserName,
					//                       RoleName = r.Name,
					//                       RoleId = r.Id
					//                   }).ToList();


				}
				UserData userData = new UserData();
				userData.Users = liveUsers;
				userData.TillUsers = liveTillUsers;
				userData.Roles = liveRoles;
				userData.UserRoles = UserRoles;
				return userData;
			}
			catch (Exception ex)
			{
				Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
				UserData userData = new UserData();
				return userData;
			}
		}
		/// <summary>
		/// Post user added from NIMPOS side and Get users added on POS admin
		/// </summary>
		/// <param name="userModel"></param>
		/// <returns></returns>
		[Route("GetNewUsers")]
		public async Task<UserData> GetNewUsers([FromUri] UserModel userModel)
		{
			try
			{

				List<OutletUser> liveTillUsers = new List<OutletUser>();
				List<Role> liveRoles = new List<Role>();
				List<User> liveUsers = new List<User>();
				List<UserRole> UserRoles = new List<UserRole>();
				using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
				{
					var tillUser = new OutletUser
					{
						Id = userModel.Id,
						UserCode = userModel.UserCode,
						UserName = userModel.UserName,
						Email = userModel.Email,
						Password = userModel.Password,
						Active = true,
						DallasKey = userModel.DallasKey,
						OutletId = userModel.OutletId,
						TrainingMode = false,
						Updated = DateTime.Now
					};
					db.OutletUser.Add(tillUser);
					db.SaveChanges();
					/* var user = new ApplicationUser { UserName = userModel.Email, Email = userModel.Email, LockoutEndDateUtc = DateTime.Now, DallasKey = userModel.DallasKey, Created = DateTime.Now, Updated = DateTime.Now, Active = true };
					 var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));


					 var result = manager.CreateAsync(user, userModel.Password);
					 if (result.Result.Succeeded)
					 {
						 string id = user.Id;
						 var _user = db.Users.FirstOrDefault(u => u.Id == id);
						 if (_user != null)
						 {
							 _user.Password = CalculateSHA1(userModel.Password, Encoding.UTF8);
							 _user.TrainingMode = false;
							 _user.Active = true;

						 }
						 var _tillUser = new TillUser
						 {
							 Id = id,                            
							 OutletId = userModel.OutletId
						 };
						 db.TillUser.Add(_tillUser);
						 db.SaveChanges();
						 liveUsers.Add(new User
									  {
										  Id = _user.Id,
										  AccessFailedCount = _user.AccessFailedCount,
										  Active = _user.Active,
										  Created = _user.Created,
										  Email = _user.Email,
										  EmailConfirmed = _user.EmailConfirmed,
										  LockoutEnabled = _user.LockoutEnabled,
										  LockoutEndDateUtc = DateTime.Now,
										  Password = _user.Password,
										  PasswordHash = _user.PasswordHash,
										  PhoneNumber = _user.PhoneNumber,
										  PhoneNumberConfirmed = _user.PhoneNumberConfirmed,
										  SecurityStamp = _user.SecurityStamp,
										  TrainingMode = _user.TrainingMode,
										  TwoFactorEnabled = _user.TwoFactorEnabled,
										  Updated = _user.Updated,
										  UserName = _user.UserName,
										  DallasKey = _user.DallasKey

									  });


						 if (liveUsers.Count > 0)
						 {
							 liveTillUsers = db.TillUser.Where(tu=>tu.Id==id).ToList();                           
						 }


					 }

	 */

				}

				UserData userData = new UserData();
				userData.Users = liveUsers;
				userData.TillUsers = liveTillUsers;
				userData.Roles = liveRoles;
				userData.UserRoles = UserRoles;
				return userData;
			}
			catch (Exception ex)
			{
				Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
				UserData userData = new UserData();
				return userData;
			}
		}
		/// <summary>
		/// Post user created from NIMPOS systems
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("PostUser")]
		//   [ValidateAntiForgeryToken]
		public IHttpActionResult PostUser(UserModel model)
		{
			try
			{
				using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
				{
					var user = new ApplicationUser { UserName = model.Email, Email = model.Email, LockoutEndDateUtc = DateTime.Now, DallasKey = model.DallasKey, Created = DateTime.Now, Updated = DateTime.Now, Active = true };
					var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));


					var result = manager.CreateAsync(user, model.Password);
					if (result.Result.Succeeded)
					{
						string id = user.Id;
						var _user = db.Users.FirstOrDefault(u => u.Id == id);
						if (_user != null)
						{
							_user.Password = CalculateSHA1(model.Password, Encoding.UTF8);
							_user.TrainingMode = false;
							_user.Active = true;

						}
						var tillUser = new OutletUser
						{
							Id = id,
							OutletId = model.OutletId
						};
						db.OutletUser.Add(tillUser);
						db.SaveChanges();
						return StatusCode(HttpStatusCode.OK);
					}
					else
						return StatusCode(HttpStatusCode.PreconditionFailed);



				}
			}
			catch (Exception ex)
			{
				Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
				return StatusCode(HttpStatusCode.ExpectationFailed);
			}
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





		[HttpGet]
		[Route("Login/{phoneNo}")]
		public async Task<UsersLoginResponse> Login(string phoneNo)
		{
			var userLoginResponse = new UsersLoginResponse();
			try
			{
				LogWriter.LogWrite("GetConnectionString calling " + phoneNo);

				var phoneNoSender = ConfigurationManager.AppSettings["PhoneNoSender"];
				var companyId = ConfigurationManager.AppSettings["CompanyId"];
				var secrect = ConfigurationManager.AppSettings["Secrect"];

				SmsHelper smsHelper = new SmsHelper();
				using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))//var uof = PosState.GetInstance().CreateUnitOfWork()
				{
					var customerRepo = uof.CustomerRepository;
					var user = customerRepo.FirstOrDefault(c => c.Phone == phoneNo);
					var userPin = smsHelper.GenerateRandomCodeForMobile();
					if (user == null)
					{
						var message = "Din verifieringskod är " + userPin + "  Mvh Food Order";
						//var sendSms = smsHelper.SendSMSViaMoblink(phoneNo, message, DateTime.Now, companyId, secrect, phoneNoSender);
						//create new cutomer
						Customer customer = new Customer
						{
							Id = Guid.NewGuid(),
							Phone = phoneNo,
							Active = true,
							Created = DateTime.Now,
							PinCode = userPin,
							Name = "online customer",
						};
						customer.Reference = customer.Id.ToString();
						customerRepo.Add(customer);
						userLoginResponse = new UsersLoginResponse
						{
							Message = "User Created Successfully. Pin sent to user",
							PinCode = userPin,
							user_id = customer.Id.ToString()
						};
					}
					else
					{
						var pin = !string.IsNullOrEmpty(user.PinCode) ? user.PinCode : userPin;
						var message = "Din verifieringskod är " + pin + "  Mvh Food Order";
						//var sendSms = smsHelper.SendSMSViaMoblink(phoneNo, message, DateTime.Now, companyId, secrect, phoneNoSender);
						userLoginResponse = new UsersLoginResponse
						{
							Message = "User already Exist Sms sent to User with Pin",
							PinCode = user.PinCode
						};
					}

					uof.Commit();
					return userLoginResponse;

				}

			}
			catch (Exception e)
			{
				//Logger.Logexception(e);
				return new UsersLoginResponse();

			}

		}



		[HttpGet]
		[Route("SMSValidation/{phoneNo}/{pin}")]
		public UsersLoginResponse SMSValidation(string phoneNo, string pin)
		{
			var userLoginResponse = new UsersLoginResponse();
			try
			{
				using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))//var uof = PosState.GetInstance().CreateUnitOfWork()
				{
					var customerRepo = uof.CustomerRepository;
					var customer = customerRepo.FirstOrDefault(c => c.Phone == phoneNo && c.PinCode == pin);

					if (customer != null)
					{
						userLoginResponse = new UsersLoginResponse
						{
							Token = "",
							access_token = "",
							user_id = customer.Id.ToString(),
							Message = "Success",
							Customer = customer,
						};
					}
					else
					{
						return new UsersLoginResponse
						{
							Message = "Invalid Pin"
						};
					}
					return userLoginResponse;
				}

			}
			catch (Exception e)
			{
				//Logger.Logexception(e);
				return new UsersLoginResponse();

			}

		}

		[HttpPost]
		[Route("UpdateCustomer")]
		public UsersPutResponse UpdateCustomer([FromBody] Customer model)
		{
			LogWriter.LogWrite(new Exception("UpdateCustomer is calling not exception"));
			var userResponse = new UsersPutResponse();
			try
			{
				using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))//var uof = PosState.GetInstance().CreateUnitOfWork()
				{
					var customerRepo = uof.CustomerRepository;
					var user = customerRepo.FirstOrDefault(c => c.Id == model.Id);
					if (user != null)
					{
						user.Name = model.Name;
						user.ZipCode = model.ZipCode;
						user.OrgNo = model.OrgNo;
						user.Address1 = model.Address1;
						user.Address2 = model.Address2;
						user.FloorNo = model.FloorNo;
						user.PortCode = model.PortCode;
						user.CustomerNo = model.CustomerNo;
						user.City = model.City;
						user.ZipCode = model.ZipCode;
						user.Updated = DateTime.Now;
						user.Active = user.Active;
						user.Email = !string.IsNullOrEmpty(model.Email) ? model.Email : user.Email;
						customerRepo.AddOrUpdate(user);
						uof.Commit();
						userResponse.success = true;
					}
					else
					{
						userResponse.success = false;

					}
					return userResponse;
				}

			}
			catch (Exception e)
			{
				//Logger.Logexception(e);
				return new UsersPutResponse();

			}

		}



		[Route("GetTenantUserByCompanyId/{companyId}")]
		public async Task<TenantUser> GetTenantUserByCompanyId(Guid companyId)
		{
			try
			{
				MasterData.MasterDbContext db = new MasterData.MasterDbContext();
				var user = db.Users.FirstOrDefault(u => u.CompanyId == companyId);
				if (user != null)
				{
					return new TenantUser
					{
						UserName = user.UserName,
						Password = POSSUM.Data.StringCipher.encodeSTROnUrl(user.Password)
					};
				}
				else
				{
					return new TenantUser
					{
						UserName = "",
						Password = ""
					};
				}
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
				return new TenantUser
				{
					UserName = "",
					Password = ""
				};

			}
		}


	}


}



