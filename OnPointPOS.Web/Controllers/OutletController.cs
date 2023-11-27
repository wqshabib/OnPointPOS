using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    // [Authorize(Roles = "Admin")]
    [Authorize]
    public class OutletController : MyBaseController
    {

        public OutletController()
        {

        }
        public ActionResult Index()
        {
            using (var db = GetConnection)
            {
                var outlets = db.Outlet.Where(o => o.IsDeleted == false).Select(o => new OutletViewModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    Address1 = o.Address1,
                    City = o.City,
                    PostalCode = o.PostalCode,
                    OuletId = "'" + o.Id + "'"

                }).ToList();
                return View(outlets);
            }
        }
        public ActionResult Create(string id)
        {
            OutletViewModel outlet = new OutletViewModel();

            using (var db = GetConnection)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var guid = new Guid(id);

                    outlet = db.Outlet.Where(o => o.Id == guid).Select(o => new OutletViewModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        PostalCode = o.PostalCode,
                        OrgNo = o.OrgNo,
                        City = o.City,
                        Address1 = o.Address1,
                        Address2 = o.Address2,
                        Address3 = o.Address3,
                        BillPrinterId = o.BillPrinterId,
                        KitchenPrinterId = o.KitchenPrinterId,
                        IsDeleted = o.IsDeleted,
                        TaxDescription = o.TaxDescription,
                        HeaderText = o.HeaderText,
                        FooterText = o.FooterText,
                        Email = o.Email,
                        WebUrl = o.WebUrl,
                        Phone = o.Phone
                    }).FirstOrDefault();

                }
                var billPrinters = db.Printer.ToList();
                if (billPrinters == null)
                    billPrinters = new List<Printer>();
                billPrinters.Add(new Printer { Id = 0, PrinterName = "Select Printer" });
                outlet.BillPrinters = billPrinters.OrderBy(o => o.Id).Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PrinterName });
                outlet.KitchenPrinters = billPrinters.OrderBy(o => o.Id).Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PrinterName });
            }

            return PartialView(outlet);
        }
        [HttpPost]
        public ActionResult Create(OutletViewModel viewModel)
        {
            string msg = "";
            try
            {
                bool isEdit = false;
                if (ModelState.IsValid)
                {
                    using (var uof = new UnitOfWork(GetConnection))
                    {
                        var outletRepo = uof.OutletRepository;

                        Outlet outlet = new Outlet();

                        if (default(Guid) != viewModel.Id)
                        {
                            outlet = outletRepo.Single(o => o.Id == viewModel.Id);
                            isEdit = true;
                        }
                        else
                        {
                            outlet.Id = Guid.NewGuid();
                            viewModel.Id = outlet.Id;
                        }
                        outlet.Name = viewModel.Name;
                        outlet.City = viewModel.City;
                        outlet.PostalCode = viewModel.PostalCode;
                        outlet.Address1 = viewModel.Address1;
                        outlet.Address2 = viewModel.Address2;
                        outlet.Address3 = viewModel.Address3;
                        outlet.OrgNo = viewModel.OrgNo;
                        outlet.Phone = viewModel.Phone;
                        outlet.Email = viewModel.Email;
                        outlet.WebUrl = viewModel.WebUrl;
                        outlet.IsDeleted = false;
                        outlet.KitchenPrinterId = viewModel.KitchenPrinterId;
                        outlet.BillPrinterId = viewModel.BillPrinterId;
                        outlet.TaxDescription = string.IsNullOrEmpty(viewModel.TaxDescription) ? "SE" + viewModel.OrgNo + "01" : viewModel.TaxDescription;
                        outlet.HeaderText = string.IsNullOrEmpty(viewModel.HeaderText) ? viewModel.Name : viewModel.HeaderText;
                        outlet.FooterText = viewModel.FooterText;
                        outlet.Updated = DateTime.Now;
                        if (isEdit == false)
                        {
                            outlet.Created = DateTime.Now;
                        }
                        outletRepo.AddOrUpdate(outlet);
                        uof.Commit();
                    }
                    UpdateAdminOutlet(viewModel, isEdit);
                    msg = "Success" + ":" + Resource.Outlet + " " + Resource.saved + " " + Resource.successfully;
                }
                else
                {
                    string validationErrors = string.Join(",", ModelState.Values.Where(E => E.Errors.Count > 0)
                       .SelectMany(E => E.Errors)
                       .Select(E => E.ErrorMessage)
                       .ToArray());
                    msg = "Error:" + validationErrors;
                }
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        private void UpdateAdminOutlet(OutletViewModel viewModel, bool isEdit)
        {
            try
            {


                using (MasterData.MasterDbContext masterDb = new MasterData.MasterDbContext())
                {
                    if (isEdit)
                    {
                        var existing = masterDb.Outlet.FirstOrDefault(o => o.Id == viewModel.Id);
                        if (existing != null)
                        {
                            existing.Name = viewModel.Name;
                            masterDb.Entry(existing).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            var adminOutlet = new MasterData.AdminOutlet
                            {
                                Id = viewModel.Id,
                                Name = viewModel.Name,
                                CompanyId = CurrentCompanyId
                            };
                            masterDb.Outlet.Add(adminOutlet);
                        }
                    }
                    else
                    {
                        var adminOutlet = new MasterData.AdminOutlet
                        {
                            Id = viewModel.Id,
                            Name = viewModel.Name,
                            CompanyId = CurrentCompanyId
                        };
                        masterDb.Outlet.Add(adminOutlet);
                    }
                    masterDb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                
            }
        }
        [HttpPost]
        public ActionResult DeleteOutlet(Guid id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var outletRepo = uof.OutletRepository;

                    var outlet = outletRepo.Single(o => o.Id == id);
                    outlet.IsDeleted = true;
                    outletRepo.AddOrUpdate(outlet);
                    uof.Commit();
                }
                msg = Resource.Success + ":" + Resource.Outlet + " " + Resource.Deleted + " " + Resource.successfully;
            }
            catch (Exception ex)
            {

                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Setting(string id)
        {
            OutletViewModel model = new OutletViewModel();
            var guid = new Guid(id);
            using (var db = GetConnection)
            {
                model = db.Outlet.Where(o => o.Id == guid).Select(o => new OutletViewModel
                {
                    Id = o.Id,
                    Email = o.Email,
                    WebUrl = o.WebUrl,
                    OrgNo = o.OrgNo,
                    FooterText = o.FooterText,
                    HeaderText = o.HeaderText,
                    Phone = o.Phone,
                    TaxDescription = o.TaxDescription
                }).FirstOrDefault();
            }
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Setting(OutletViewModel viewModel)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var outletRepo = uof.OutletRepository;

                    var outlet = outletRepo.Single(o => o.Id == viewModel.Id);
                    outlet.Email = viewModel.Email;
                    outlet.WebUrl = viewModel.WebUrl;
                    outlet.OrgNo = viewModel.OrgNo;
                    outlet.FooterText = viewModel.FooterText;
                    outlet.HeaderText = viewModel.HeaderText;
                    outlet.Phone = viewModel.Phone;
                    outlet.TaxDescription = viewModel.TaxDescription;
                    outlet.Updated = DateTime.Now;
                    outletRepo.AddOrUpdate(outlet);
                    uof.Commit();
                }
                msg = Resource.Success + ":" + Resource.Settings + " " + Resource.Updated + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}