using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    // [Authorize(Roles = "Admin")]
    [Authorize]
    public class TerminalController : MyBaseController
    {
        private readonly IUnitOfWork uof;

        public TerminalController()
        {
            // uof = PosState.GetInstance().CreateUnitOfWork();
        }

        public ActionResult Index()
        {
            List<TerminalViewModel> terminals = new List<TerminalViewModel>();

            try
            {
                using (var db = GetConnection)

                {
                    terminals = (from t in db.Terminal
                                 where t.IsDeleted == false
                                 select new TerminalViewModel
                                 {
                                     Id = t.Id,
                                     Outlet = t.Outlet,
                                     OutletId = t.Outlet.Id,
                                     Description = t.Description,
                                     TerminalNo = t.TerminalNo,
                                     OutletName = t.Outlet.Name,
                                     UniqueIdentification = t.UniqueIdentification == null ? "" : t.UniqueIdentification,
                                     TerminalId = "'" + t.Id + "'",
                                     //  Category = t.Category,
                                     //   RootCategoryId = t.Category.Id,
                                     CategoryName = t.Category.Name

                                 }).ToList();
                }

                return View(terminals);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Create(string id)
        {
            TerminalViewModel model = new TerminalViewModel();

            using (var db = GetConnection)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    Guid guid = new Guid(id);

                    model = db.Terminal.Where(t => t.Id == guid).Select(t => new TerminalViewModel
                    {
                        Id = t.Id,
                        Outlet = t.Outlet,
                        OutletId = t.Outlet.Id,
                        Description = t.Description,
                        Category = t.Category,
                        //  RootCategoryId = t.Category.Id,
                        RootCategoryId = t.Category == null ? 0 : t.Category.Id,
                        HardwareAddress = t.HardwareAddress,
                        Status = t.Status,
                        TerminalNo = t.TerminalNo,
                        UniqueIdentification = t.UniqueIdentification,
                        TerminalType = t.TerminalType,
                        IsDeleted = t.IsDeleted,
                        AutoLogin = t.AutoLogin,
                    })
                    .FirstOrDefault();
                }

                if (model == null)
                    model = new TerminalViewModel();

                if (model.TerminalNo == 0)
                {
                    int maxTerminalNo = 1;
                    if (db.Terminal.Count() > 0)
                    {
                        maxTerminalNo = db.Terminal.Max(obj => obj.TerminalNo);
                        maxTerminalNo++;
                    }
                    model.TerminalNo = maxTerminalNo;
                }

                var outlets = db.Outlet.ToList();
                if (outlets == null)
                    outlets = new List<Outlet>();
                outlets.Add(new Outlet { Id = default(Guid), Name = "Select outlet" });

                model.Outlets = outlets.OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });

                var terminalType = new List<TerminalType>();
                terminalType.Add(new TerminalType { Id = default(Guid), Name = "Select terminal type" });
                model.TerminalTypes = terminalType.OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });

                var categories = db.Category.ToList();
                if (categories == null)
                    categories = new List<Category>();
                categories.Add(new Category { Id = 0, Name = "Select root category" });

                model.Categories = categories.Where(o => o.Parant == 0).OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });
                model.Statuses = Enum.GetValues(typeof(TerminalStatus)).Cast<TerminalStatus>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() });
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Create(TerminalViewModel viewModel)
        {
            string msg = "";
            try
            {
                bool isEdit = false;

                using (var uof = new UnitOfWork(GetConnection))
                {
                    Terminal terminal = new Terminal();

                    var terminalRepo = uof.TerminalRepository;
                    var outLetRepo = uof.OutletRepository;
                    var catRepo = uof.CategoryRepository;

                    viewModel.Outlet = outLetRepo.FirstOrDefault(o => o.Id == viewModel.OutletId);
                    viewModel.Category = catRepo.FirstOrDefault(o => o.Id == viewModel.RootCategoryId);

                    if (viewModel.Id != default(Guid))
                    {
                        terminal = terminalRepo.Single(t => t.Id == viewModel.Id);
                        isEdit = true;
                    }
                    else
                    {
                        terminal.Id = Guid.NewGuid();
                        viewModel.Id = terminal.Id;
                    }
                    terminal.Description = viewModel.Description;
                    terminal.Outlet = viewModel.Outlet;
                    terminal.IsDeleted = false;
                    terminal.Category = viewModel.Category;
                    terminal.TerminalNo = viewModel.TerminalNo;
                    terminal.TerminalType = viewModel.TerminalType;
                    terminal.Status = viewModel.Status;
                    terminal.HardwareAddress = viewModel.HardwareAddress;
                    terminal.UniqueIdentification = viewModel.UniqueIdentification;
                    terminal.AutoLogin = viewModel.AutoLogin;
                    terminal.Updated = DateTime.Now;

                    if (isEdit == false)
                    {
                        terminal.Created = DateTime.Now;

                        var cashDrawer = new CashDrawer
                        {
                            Id = Guid.NewGuid(),
                            TerminalId = terminal.Id,
                            Name = "Kassalåda " + terminal.TerminalNo,
                            Location = "Kassan"

                        };

                        var cashDrawRepo = uof.CashDrawerRepository;
                        cashDrawRepo.Add(cashDrawer);
                    }

                    terminalRepo.AddOrUpdate(terminal);

                    uof.Commit();

                }

                UpdateAdminTerminal(viewModel.Id, viewModel.UniqueIdentification, isEdit);

                msg = "Success" + ":" + Resource.Terminal + " " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        private void UpdateAdminTerminal(Guid terminalId, string code, bool isEdit)
        {
            try
            {
                using (MasterData.MasterDbContext masterDb = new MasterData.MasterDbContext())
                {
                    if (isEdit)
                    {
                        var terminal = masterDb.Terminal.FirstOrDefault(t => t.UniqueIdentification == code);
                        if (terminal != null)
                        {
                            if (terminal.Id == terminalId)
                            {
                                terminal.Customer = CurrentDBName;
                                masterDb.Entry(terminal).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                terminal = masterDb.Terminal.Remove(terminal);

                                var _terminal = new MasterData.AdminTerminal
                                {
                                    Id = terminalId,
                                    Customer = CurrentDBName,
                                    UniqueIdentification = code
                                };

                                masterDb.Terminal.Add(_terminal);
                            }
                        }
                        else
                        {
                            var terminal1 = masterDb.Terminal.FirstOrDefault(t => t.Id == terminalId);
                            if (terminal1 != null)
                            {
                                terminal1.UniqueIdentification = code;
                                terminal1.Customer = CurrentDBName;
                                masterDb.Entry(terminal1).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                var _terminal = new MasterData.AdminTerminal
                                {
                                    Id = terminalId,
                                    Customer = CurrentDBName,
                                    UniqueIdentification = code
                                };

                                masterDb.Terminal.Add(_terminal);
                            }
                        }
                    }
                    else
                    {
                        var terminal = masterDb.Terminal.FirstOrDefault(t => t.UniqueIdentification == code);
                        if (terminal != null)
                            terminal = masterDb.Terminal.Remove(terminal);

                        var _terminal = new MasterData.AdminTerminal
                        {
                            Id = terminalId,
                            Customer = CurrentDBName,
                            UniqueIdentification = code
                        };

                        masterDb.Terminal.Add(_terminal);
                    }

                    masterDb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }

        [HttpPost]
        public ActionResult DeleteTerminal(Guid id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var terminalRepo = uof.TerminalRepository;
                    var terminal = terminalRepo.Single(o => o.Id == id);
                    terminal.IsDeleted = true;
                    uof.Commit();
                }

                msg = Resource.Success + ":" + Resource.Terminal + " " + Resource.Deleted + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

    }
}