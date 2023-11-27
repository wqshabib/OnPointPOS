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
    public class IconStoreController : MyBaseController
    {


        public IconStoreController()
        {
        }
        // GET: IconStore
        public ActionResult Index()
        {
            List<IconStoreModel> result = new List<IconStoreModel>();
            using (var db = GetConnection)
            {
                var data = db.IconStore.ToList();

                foreach (var icon in data)
                {
                    result.Add(new IconStoreModel
                    {
                        Id = icon.Id,
                        Type = icon.Type,
                        Photo = icon.Photo,
                        Title = icon.Title,
                        ImageUrl = FromByteToImageSrc(icon.Photo)
                    });
                }
            }
            return View(result);
        }
        private string FromByteToImageSrc(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            var imgSrc = String.Format("data:image/gif;base64,{0}", base64);
            return imgSrc;
        }
        // GET: IconStore/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: IconStore/Create
        public ActionResult Create()
        {
            IconStoreModel model = new IconStoreModel();

            model.Types = FillIconType();
            return View(model);
        }
        private IEnumerable<SelectListItem> FillIconType()
        {
            IEnumerable<IconType> iconTypes = Enum.GetValues(typeof(IconType))
                                                       .Cast<IconType>();

            var types = from enumValue in iconTypes
                        select new SelectListItem
                        {
                            Text = enumValue.ToString(),
                            Value = ((int)enumValue).ToString(),

                        };
            return types;
        }
        // POST: IconStore/Create
        [HttpPost]
        public ActionResult Create(IconStoreModel viewModel, HttpPostedFileBase file)
        {
            try
            {
                if (file != null)
                {
                    using (var uof = new UnitOfWork(GetConnection))
                    {
                        var iconRepo = uof.IconStoreRepository;
                        string ImageName = System.IO.Path.GetFileName(file.FileName);

                        int lastId = 0;
                        try
                        {
                            lastId = iconRepo.GetAll().Max(i => i.Id);
                        }
                        catch
                        {

                           
                        }
                        
                        //save new record in database
                        IconStore newRecord = new IconStore();
                        newRecord.Id = lastId + 1;
                        newRecord.Title = viewModel.Title;
                        newRecord.Type = viewModel.Type;
                        newRecord.Updated = DateTime.Now;
                        newRecord.Photo = new byte[file.ContentLength];
                        file.InputStream.Read(newRecord.Photo, 0, file.ContentLength);

                        iconRepo.Add(newRecord);
                        uof.Commit();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Select Image";
                    viewModel.Types = FillIconType();

                }
                return View(viewModel);

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                viewModel.Types = FillIconType();
                return View(viewModel);
            }
        }

        // GET: IconStore/Edit/5
        public ActionResult Edit(int id)
        {
            IconStoreModel model = new IconStoreModel();
            using (var db = GetConnection)
            {
                var icon = db.IconStore.FirstOrDefault(i => i.Id == id);

                if (icon != null)
                {
                    model = new IconStoreModel
                    {
                        Id = icon.Id,
                        Type = icon.Type,
                        Photo = icon.Photo,
                        Title = icon.Title,
                        ImageUrl = FromByteToImageSrc(icon.Photo)
                    };
                }
            }
            model.Types = FillIconType();
            return View(model);
        }

        // POST: IconStore/Edit/5
        [HttpPost]
        public ActionResult Edit(IconStoreModel viewModel, HttpPostedFileBase file)
        {
            try
            {
                if (file != null)
                {
                    using (var uof = new UnitOfWork(GetConnection))
                    {
                        var iconRepo = uof.IconStoreRepository;
                        string ImageName = System.IO.Path.GetFileName(file.FileName);


                        IconStore editRecord = iconRepo.Single(i => i.Id == viewModel.Id);
                        editRecord.Title = viewModel.Title;
                        editRecord.Type = viewModel.Type;
                        editRecord.Updated = DateTime.Now;
                        editRecord.Photo = new byte[file.ContentLength];
                        file.InputStream.Read(editRecord.Photo, 0, file.ContentLength);
                        iconRepo.AddOrUpdate(editRecord);
                        uof.Commit();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Select Image";
                    viewModel.Types = FillIconType();
                    return View(viewModel);
                }

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                viewModel.Types = FillIconType();
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var iconRepo = uof.IconStoreRepository;
                    var icon = iconRepo.Single(p => p.Id == id);
                    iconRepo.Delete(icon);
                    uof.Commit();
                }
                msg = "Deleted successfully";

                //  return RedirectToAction("Index");


            }

            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}
