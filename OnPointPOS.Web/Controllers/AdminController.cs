using POSSUM.MasterData;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
	[Authorize(Roles = "Super Admin")]
	public class AdminController : Controller
	{
		// GET: Admin
		MasterDbContext db = new MasterDbContext();

		#region Company CRUD
		public ActionResult Index()
		{
			var companies = db.Company.Select(c => new CompanyViewModel
			{
				Id = c.Id,
				Name = c.Name,
				Address = c.Address,
				City = c.City,
				PostalCode = c.PostalCode,
				Country = c.Country
			}).ToList();
			return View(companies);
		}
		public ActionResult Create(string id)
		{
			var model = new Company();
			Guid guid = default(Guid);
			Guid.TryParse(id, out guid);
			var company = db.Company.FirstOrDefault(c => c.Id == guid);
			if (company != null)
				model = company;
            else
            {
                model.DBServer = ConfigurationManager.AppSettings["DBServer"];
                model.DBPassword = ConfigurationManager.AppSettings["Password"];
                model.DBUser = ConfigurationManager.AppSettings["UID"];
            }
			return PartialView(model);
		}
		[HttpPost]
		public ActionResult Create(Company company)
		{
			string msg = "";
			try
			{
				var _company = db.Company.FirstOrDefault(c => c.Id == company.Id);
				if (_company == null)
				{
					company.Id = Guid.NewGuid();
					db.Company.Add(company);
				}
				else
				{
					_company.Name = company.Name;
					_company.Address = company.Address;
					_company.City = company.City;
					_company.Country = company.Country;
					_company.PostalCode = company.PostalCode;
					_company.AdminURL = company.AdminURL;
					_company.DBName = company.DBName;
					_company.DBServer = company.DBServer;
					db.Entry(_company).State = System.Data.Entity.EntityState.Modified;
				}
				db.SaveChanges();
				msg = "Success:" + Resource.Company + " " + Resource.saved + " " + Resource.successfully;
			}
			catch (Exception ex)
			{
				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult DeleteCampany(string id)
		{
			string msg = "";
			try
			{
				Guid guid = default(Guid);
				Guid.TryParse(id, out guid);
				var company = db.Company.FirstOrDefault(c => c.Id == guid);
				if (company != null)
				{
					company.Active = false;
				}
				db.SaveChanges();
				msg = "Success:" + Resource.Company + " " + Resource.Deleted + " " + Resource.successfully;
			}
			catch (Exception ex)
			{

				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}
		#endregion
		#region Contact CRUD
		public ActionResult Contacts(string id)
		{
			Guid companyguid = default(Guid);
			Guid.TryParse(id, out companyguid);
			ViewBag.CompanyId = companyguid;
			var contacts = db.Contact.Where(c => c.CompanyId == companyguid).Select(c => new ContactViewModel
			{
				Id = c.Id,
				FirstName = c.FirstName,
				LastName = c.LastName,
				Email = c.Email,
				Phone = c.Phone,
				Username = c.Username,
				Password = c.Password
			}).ToList();
			return View(contacts);
		}
		public ActionResult CreateContact(string id, string companyId)
		{
			var model = new ContactViewModel();
			Guid guid = default(Guid);
			Guid.TryParse(id, out guid);
			Guid companyguid = default(Guid);
			Guid.TryParse(companyId, out companyguid);
			model.CompanyId = companyguid;
			var contact = db.Contact.FirstOrDefault(c => c.Id == guid);
			if (contact != null)
			{
				model.Id = contact.Id;
				model.CompanyId = contact.CompanyId;
				model.FirstName = contact.FirstName;
				model.LastName = contact.LastName;
				model.Email = contact.Email;
				model.Phone = contact.Phone;
				model.Username = contact.Username;
				model.Password = contact.Password;
				model.Notes = contact.Notes;
			}
			return PartialView(model);
		}
		[HttpPost]
		public ActionResult CreateContact(ContactViewModel contact)
		{
			string msg = "";
			try
			{
				var _cotact = db.Contact.FirstOrDefault(c => c.Id == contact.Id);
				if (_cotact == null)
				{
					_cotact = new Contact();
					_cotact.Id = Guid.NewGuid();
					_cotact.CompanyId = contact.CompanyId;
					_cotact.FirstName = contact.FirstName;
					_cotact.LastName = contact.LastName;
					_cotact.Email = contact.Email;
					_cotact.Phone = contact.Phone;
					_cotact.Username = contact.Username;
					_cotact.Password = contact.Password;
					_cotact.Notes = contact.Notes;
					db.Contact.Add(_cotact);
				}
				else
				{
					_cotact.FirstName = contact.FirstName;
					_cotact.LastName = contact.LastName;
					_cotact.Email = contact.Email;
					_cotact.Phone = contact.Phone;
					_cotact.Username = contact.Username;
					_cotact.Password = contact.Password;
					_cotact.Notes = contact.Notes;

					db.Entry(_cotact).State = System.Data.Entity.EntityState.Modified;
				}
				db.SaveChanges();
				msg = "Success:" + Resource.Contact + " " + Resource.saved + " " + Resource.successfully;
			}
			catch (Exception ex)
			{
				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult DeleteContact(string id)
		{
			string msg = "";
			try
			{
				Guid guid = default(Guid);
				Guid.TryParse(id, out guid);
				var contact = db.Contact.FirstOrDefault(c => c.Id == guid);
				if (contact != null)
				{
					db.Contact.Remove(contact);
				}
				db.SaveChanges();
				msg = "Success:" + Resource.Contact + " " + Resource.Deleted + " " + Resource.successfully;
			}
			catch (Exception ex)
			{

				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}
		#endregion

		#region Contract CRUD
		public ActionResult Contracts(string id)
		{
			Guid companyguid = default(Guid);
			Guid.TryParse(id, out companyguid);
			ViewBag.CompanyId = companyguid;
			var contracts = db.Contract.Where(c => c.CompanyId == companyguid).Select(c => new ContractViewModel
			{
				Id = c.Id,
				StartDate = c.StartDate,
				EndDate = c.EndDate,
				Status = c.Status,
				Description = c.Description,
				Actual_Deployment_Date = c.Actual_Deployment_Date,
				Expected_Deployment_Date = c.Expected_Deployment_Date,
				MonthlyPrice = c.MonthlyPrice,
				Notes = c.Notes,
				POSVersion = c.POSVersion
			}).ToList();
			return View(contracts);
		}
		public ActionResult CreateContract(string id, string companyId)
		{
			var model = new ContractViewModel();
			Guid guid = default(Guid);
			Guid.TryParse(id, out guid);
			Guid companyguid = default(Guid);
			Guid.TryParse(companyId, out companyguid);
			model.CompanyId = companyguid;
			var contract = db.Contract.FirstOrDefault(c => c.Id == guid);
			if (contract != null)
			{
				model.Id = contract.Id;
				model.CompanyId = contract.CompanyId;
				model.Description = contract.Description;
				model.Status = contract.Status;
				model.StartDate = contract.StartDate;
				model.EndDate = contract.EndDate;
				model.Actual_Deployment_Date = contract.Actual_Deployment_Date;
				model.Expected_Deployment_Date = contract.Expected_Deployment_Date;
				model.POSVersion = contract.POSVersion;
				model.MonthlyPrice = contract.MonthlyPrice;
				model.Notes = contract.Notes;
			}
			return PartialView(model);
		}
		[HttpPost]
		public ActionResult CreateContract(ContractViewModel contract)
		{
			string msg = "";
			try
			{
				var _cotract = db.Contract.FirstOrDefault(c => c.Id == contract.Id);
				if (_cotract == null)
				{
					_cotract = new Contract();
					_cotract.Id = Guid.NewGuid();
					_cotract.CompanyId = contract.CompanyId;
					_cotract.Description = contract.Description;
					_cotract.Status = contract.Status;
					_cotract.StartDate = contract.StartDate;
					_cotract.EndDate = contract.EndDate;
					_cotract.Actual_Deployment_Date = contract.Actual_Deployment_Date;
					_cotract.Expected_Deployment_Date = contract.Expected_Deployment_Date;
					_cotract.POSVersion = contract.POSVersion;
					_cotract.MonthlyPrice = contract.MonthlyPrice;
					_cotract.Notes = contract.Notes;
					db.Contract.Add(_cotract);
				}
				else
				{
					_cotract.Description = contract.Description;
					_cotract.Status = contract.Status;
					_cotract.StartDate = contract.StartDate;
					_cotract.EndDate = contract.EndDate;
					_cotract.Actual_Deployment_Date = contract.Actual_Deployment_Date;
					_cotract.Expected_Deployment_Date = contract.Expected_Deployment_Date;
					_cotract.POSVersion = contract.POSVersion;
					_cotract.MonthlyPrice = contract.MonthlyPrice;
					_cotract.Notes = contract.Notes;

					db.Entry(_cotract).State = System.Data.Entity.EntityState.Modified;
				}
				db.SaveChanges();
				msg = "Success:" + Resource.Contract + " " + Resource.saved + " " + Resource.successfully;
			}
			catch (Exception ex)
			{
				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult DeleteContract(string id)
		{
			string msg = "";
			try
			{
				Guid guid = default(Guid);
				Guid.TryParse(id, out guid);
				var contract = db.Contract.FirstOrDefault(c => c.Id == guid);
				if (contract != null)
				{
					db.Contract.Remove(contract);
				}
				db.SaveChanges();
				msg = "Success:" + Resource.Contract + " " + Resource.Deleted + " " + Resource.successfully;
			}
			catch (Exception ex)
			{

				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}


		#endregion


		public ActionResult Outlets()
		{
			var outlets = db.Outlet.ToList();
			return View(outlets);
		}
		

		public ActionResult TerminalCodes()
		{
			var terminals = db.Terminal.ToList();
			return View(terminals);
		}

		public ActionResult UserOutlet(string userId)
		{
			ViewBag.Id = userId;
			var user = db.Users.FirstOrDefault(u => u.Id == userId);
			var outlets = db.Outlet.Where(o => o.CompanyId == user.CompanyId).ToList();
			var outletUsers = db.OutletUser.ToList();
			List<AdminUserOutletModel> models = new List<AdminUserOutletModel>();
			foreach (var outlet in outlets)
			{
				var model = new AdminUserOutletModel();
				model.UserId = user.Id;
				model.OutletId = outlet.Id;
				model.Name = outlet.Name;
				if (outletUsers.FirstOrDefault(u => u.UserId == userId && u.OutletId == outlet.Id) != null)
					model.IsSelected = true;
				models.Add(model);
			}

			return View(models);
		}
		public ActionResult SaveUserOutlet(List<AdminUserOutletModel> oultes)
		{
			string msg = "";
			try
			{
				var userId = oultes.First().UserId;
				foreach (var outlet in oultes)
				{

					if (outlet.IsSelected)
					{
						var useroutlet = db.OutletUser.FirstOrDefault(o => o.OutletId == outlet.OutletId && o.UserId == userId);
						if (useroutlet == null)
						{
							useroutlet = new OutletUser
							{
								Id = Guid.NewGuid(),
								UserId = userId,
								OutletId = outlet.OutletId
							};
							db.OutletUser.Add(useroutlet);
						}
					}
					else
					{
						var useroutlet = db.OutletUser.FirstOrDefault(o => o.OutletId == outlet.OutletId && o.UserId == userId);
						if (useroutlet != null)
						{

							db.OutletUser.Remove(useroutlet);
						}
					}
				}
				db.SaveChanges();

				msg = "Success:Outlet(s) assigned successfully";
			}
			catch (Exception ex)
			{

				msg = "Error:" + ex.Message;
			}
			return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
		}

		[AllowAnonymous]
		public ActionResult GenerateUniqueIdentifier()
		{
			var terminasls = db.Terminal.ToList();
			string UniqueCode = "";
			bool done = true;
			while (done)
			{
				string uniquecode = "SESUM-" + RandomString(3) + GenerateRandomNo();
				var exists = terminasls.FirstOrDefault(t => t.UniqueIdentification == uniquecode);
				if (exists == null)
				{
					UniqueCode = uniquecode;
					done = false;
					var terminal = new AdminTerminal
					{
						Id = Guid.NewGuid(),
						UniqueIdentification = UniqueCode

					};
					db.Terminal.Add(terminal);
					db.SaveChanges();
				}
			}
			return Json(new { UniqueIdentification = UniqueCode }, JsonRequestBehavior.AllowGet);

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
	}
}