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
    //  [Authorize(Roles = "Admin")]
    [Authorize]
    public class StoreSettingController : MyBaseController
    {

        public StoreSettingController()
        {

        }
        public ActionResult Index()
        {

            using (var db = GetConnection)
            {
                var settings = db.Setting.Select(s => new SettingViewModel
                {
                    Id = s.Id,
                    Description = s.Description,
                    Code = s.Code,
                    Value = s.Value,
                    OutletId = s.OutletId,
                    TerminalId = s.TerminalId,
                    Sort = s.Sort,
                    Type = s.Type
                }).ToList();
                return View(settings);
            }

        }
        public ActionResult ReportSettings()
        {
            using (var db = GetConnection)
            {
                var settings = db.ZReportSetting.ToList();
                return View(settings);
            }
        }
        public ActionResult SetReportSetting(List<ZReportSetting> settings)
        {
            string msg = "";
            try
            {
                using (var db = GetConnection)
                {

                    foreach (var setting in settings)
                    {
                        var _setting = db.ZReportSetting.FirstOrDefault(s => s.Id == setting.Id);
                        _setting.Visiblity = setting.Visiblity;
                        _setting.Updated = DateTime.Now;
                        db.Entry(_setting).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                msg = "Success:" + Resource.Settings + "  " + Resource.Updated + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Languages()
        {
            using (var db = GetConnection)
            {
                var languages = db.Language.ToList();
                return View(languages);
            }
        }
        public ActionResult SetDefaultLanguage(int id)
        {
            string msg = "";
            try
            {
                using (var db = GetConnection)
                {
                    var languages = db.Language.ToList();
                    foreach (var lang in languages)
                    {
                        if (lang.Id == id)
                            lang.IsDefault = true;
                        else
                            lang.IsDefault = false;
                        lang.Updated = DateTime.Now;
                        db.Entry(lang).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                msg = "Success:" + Resource.Language + "  " + Resource.Updated + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create(int? id)
        {
            SettingViewModel model = new SettingViewModel();

            using (var db = GetConnection)
            {
                if (id != null)
                {
                    int settingId = Convert.ToInt32(id);
                    model = db.Setting.Where(s => s.Id == settingId).Select(s => new SettingViewModel
                    {
                        Id = s.Id,
                       
                        Description = s.Description,
                        Code = s.Code,
                        Value = s.Value,
                        OutletId = s.OutletId == null ? default(Guid) : s.OutletId,
                        TerminalId = s.TerminalId == null ? default(Guid) : s.TerminalId,
                        Sort = s.Sort,
                        Type = s.Type
                    }).FirstOrDefault();
                }
                if (model == null)
                    model = new SettingViewModel();

                model.Types = Enum.GetValues(typeof(SettingType)).Cast<SettingType>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();

                var outlets = db.Outlet.ToList();
                if (outlets == null)
                    outlets = new List<Outlet>();
                outlets.Add(new Outlet { Id = default(Guid), Name = "Select outlet" });
                model.Outlets = outlets.OrderBy(o => o.Id).Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();

                var terminals = new List<Terminal>();
                if (model.OutletId != default(Guid))
                    terminals = db.Terminal.Where(t => t.Outlet.Id == model.OutletId).ToList();

                terminals.Add(new Terminal { Id = default(Guid), Description = "Select Terminal" });
                model.Terminals = terminals.OrderBy(o => o.Id).Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Description }).ToList();

                model.Codes = Enum.GetValues(typeof(SettingCode)).Cast<SettingCode>().Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() }).ToList();
            }

            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Create(SettingViewModel viewModel)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var settingRepo = uof.SettingRepository;
                    var setting = new Setting();
                    bool isEdit = false;
                    if (viewModel.Id > 0)
                    {
                        setting = settingRepo.Single(s => s.Id == viewModel.Id);
                        isEdit = true;
                        setting.Updated = DateTime.Now;

                    }
                    else
                    {
                        int maxId = (int)settingRepo.Max(s => s.Id);
                        viewModel.Id = maxId + 1;


                    }

                    setting.Id = viewModel.Id;
                    setting.Description = viewModel.Description;
                    setting.Code = viewModel.Code;
                    setting.Value = viewModel.Value;
                    setting.Sort = viewModel.Sort;
                    setting.OutletId = viewModel.OutletId;
                    setting.TerminalId = viewModel.TerminalId;
                    setting.Type = viewModel.Type;
                    setting.Updated = DateTime.Now;
                    if (isEdit == false)
                    {
                        setting.Created = DateTime.Now;

                    }

                    settingRepo.AddOrUpdate(setting);
                    uof.Commit();
                }
                msg = "Success" + ":" + Resource.Settings + " " + Resource.Save + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Success + ":" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteSetting(int id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var settingRepo = uof.SettingRepository;
                    var setting = settingRepo.Single(o => o.Id == id);
                    settingRepo.Delete(setting);
                    uof.Commit();
                }
                //  msg = "Success:Setting deleted successfully";

                msg = Resource.Success + ":" + Resource.Settings + " " + Resource.Deleted + " " + Resource.successfully;

            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FillTerminal(Guid id)
        {
            using (var db = GetConnection)
            {
                var terminals = db.Terminal.Where(t => t.Outlet.Id == id).Select(t => new TerminalData
                {
                    Id = t.Id,
                    Name = t.UniqueIdentification
                }).ToList();
                if (terminals == null)
                    terminals = new List<TerminalData>();
                terminals.Add(new TerminalData { Id = default(Guid), Name = Resource.Select + " " + Resource.Terminal });

                return Json(terminals.OrderBy(t => t.Id).ToList(), JsonRequestBehavior.AllowGet);
            }
        }

    }
}