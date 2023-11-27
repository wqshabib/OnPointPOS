using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Net;
using System.Diagnostics;
using POSSUM.Web.Models;
using POSSUM.Data;
using POSSUM.Model;
using System.Data;
using System.Text;
using System.Reflection;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MvcJqGrid;

namespace POSSUM.Web.Controllers
{
    [Authorize]
    public class CategoryController : MyBaseController
    {
        private int TOTAL_ROWS = 0;
        public CategoryController()
        {

        }
        public ActionResult Index()
        {
            ViewBag.Statuses = new[] { Resource.Active, Resource.Inactive };
            ViewBag.DeleteStatus = new[] { Resource.Deleted };
            return View();
        }

        #region JQ search

        public ActionResult GeCategoryByFilter(GridSettings gridSettings)
        {
            using (var db = GetConnection)
            {

                var data = from itm in db.Category

                           select new CategoryViewModel
                           {
                               Id = itm.Id,
                               Name = itm.Name,
                               Parant = itm.Parant,
                               CategoryLevel = itm.CategoryLevel,
                               Active = itm.Active,
                               Deleted = itm.Deleted

                           };
                var categories = GetOrderedCategories(data, gridSettings.SortColumn, gridSettings.SortOrder);

                if (gridSettings.IsSearch)
                {
                    categories = gridSettings.Where.rules.Aggregate(categories, FilterCategories);

                }
                else
                {
                    categories = categories.Where(itm => itm.Deleted == false);
                }
                var total = 0;
                if (categories != null)
                    total = categories.Count();
                var _categories = categories.OrderBy(o => o.Id).Skip((gridSettings.PageIndex - 1) * gridSettings.PageSize).Take(gridSettings.PageSize).ToList();


                var d = new
                {
                    total = total / gridSettings.PageSize + 1,
                    page = gridSettings.PageIndex,
                    records = total,
                    rows = (
                        from c in _categories
                        select new
                        {
                            id = c.Id,
                            cell = new string[]
                        {
                        c.Id.ToString(),
                        c.Name,
                        c.CategoryLevel.ToString(),
                        c.ActiveStatus,
                        c.Deleted.ToString()
                        }
                        }).ToArray()
                };

                return Json(d, JsonRequestBehavior.AllowGet);
            }
        }

        private static IQueryable<CategoryViewModel> FilterCategories(IQueryable<CategoryViewModel> items, MvcJqGrid.Rule rule)
        {
            switch (rule.field)
            {
                case "Id":
                    return items.Where(c => c.Id == Convert.ToInt32(rule.data));

                case "Name":
                    return items.Where(c => c.Name.Contains(rule.data) && c.Deleted == false);

                case "ActiveStatus":
                    {
                        bool status = rule.data == Resource.Active ? true : false;
                        return items.Where(c => c.Active == status && c.Deleted == false);

                    }
                case "Deleted":
                    {
                        bool deleted = rule.data == Resource.Deleted ? true : false;
                        return items.Where(c => c.Deleted == deleted);

                    }
                default:
                    return items.Where(c => c.Deleted == false);
            }
        }

        private static IQueryable<CategoryViewModel> GetOrderedCategories(IQueryable<CategoryViewModel> items, string sortColumn, string sortOrder)
        {
            switch (sortColumn)
            {
                case "Id":
                    return (sortOrder == "desc") ? items.OrderByDescending(c => Convert.ToInt32(c.Id))
                        : items.OrderBy(c => Convert.ToInt32(c.Id));

                case "Name":
                    return (sortOrder == "desc") ? items.OrderByDescending(c => c.Name) : items.OrderBy(c => c.Name);
                default:
                    return items;
            }
        }

        #endregion

        public ActionResult GetSearchByName(string searchToken)
        {

            var model = new CategoryViewModel();
            using (var db = GetConnection)
            {

                if (!string.IsNullOrEmpty(searchToken))
                {
                    List<CategoryViewModel> result = db.Category.Where(c => c.Name.ToLower().Contains(searchToken.ToLower())).Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Parant = (int)c.Parant,
                        Name = c.Name,
                        CategoryLevel = c.CategoryLevel,
                        Active = c.Active

                    }).ToList();

                    model.Children = result;
                    return PartialView("PartialCategoryList", model.Children);
                }
                else
                {
                    List<CategoryViewModel> result = db.Category.Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Parant = (int)c.Parant,
                        Name = c.Name,
                        CategoryLevel = c.CategoryLevel,
                        Active = c.Active

                    }).ToList();

                    model.Children = result;
                }
                return PartialView("PartialCategoryList", model.Children);
            }
        }

        public ActionResult Create()
        {
            CategoryViewModel item = new CategoryViewModel();
            using (var db = GetConnection)
            {

                var icons = db.IconStore.Where(c => c.Type == IconType.Category).ToList();
                List<IconStore> iconList = new List<IconStore>();
                iconList.Add(new IconStore { Id = 0, Title = "No image" });
                iconList.AddRange(icons);
                item.Icons = iconList.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Title
                });

                var lastCategory = db.Category.Where(a => !string.IsNullOrEmpty(a.ColorCode)).OrderByDescending(a => a.Updated).FirstOrDefault();
                if (lastCategory != null) {
                    item.ColorCode = lastCategory.ColorCode;
                }

                var categories = db.Category.Where(r => r.Id > 0 && r.Deleted == false).Select(c => new ItemCategoryViewModel
                {
                    CategoryId = c.Id,
                    Parant = (int)c.Parant,
                    Name = c.Name,
                    CategoryLevel = c.CategoryLevel,

                    IsSelected = (c.CategoryLevel == 1)
                }).ToList();
                SeededCategories model = new Models.SeededCategories { Seed = 0, Categories = categories };

                item.SeedCategories = model;
                var selectedCategory = model.Categories.FirstOrDefault(i => i.IsSelected);
                item.Parant = (selectedCategory != null) ? selectedCategory.CategoryId : 0;


                item.Active = true;
            }
            return PartialView(item);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(db))
                {
                    var catRepo = uof.CategoryRepository;

                    var parent = catRepo.FirstOrDefault(c => c.Id == viewModel.Parant);
                    int level = 1;
                    if (parent != null)
                        level = parent.CategoryLevel + 1;

                    int lastId = catRepo.GetAll().Max(c => c.Id);
                    Category category = new Category
                    {
                        Id = lastId + 1,
                        Name = viewModel.Name,
                        Parant = viewModel.Parant,
                        CategoryLevel = level,
                        Active = viewModel.Active,
                        ColorCode = string.IsNullOrEmpty(viewModel.ColorCode) ? RandomHTMLColor() : viewModel.ColorCode,
                        IconId = viewModel.IconId,
                        SortOrder = viewModel.SortOrder,
                        Deleted = false,
                        Type = viewModel.Type
                    };
                    category.Updated = DateTime.Now;
                    category.Created = DateTime.Now;

                    catRepo.Add(category);
                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == category.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = category.Id.ToString(),
                                TableName = TableName.Category

                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }
                    uof.Commit();

                    return RedirectToAction("Index");
                }
            }
            return PartialView(viewModel);
        }
        private string RandomHTMLColor()
        {
            var random = new Random();
            var color = String.Format("#{0:X6}", random.Next(0x1000000));
            return color;
        }
        private void updateCollection(Category category, List<Category> lstCategory, ref List<CategoryViewModel> lst)
        {
            var newRecord = new CategoryViewModel
            {
                Id = category.Id,
                CategoryLevel = category.CategoryLevel,
                Parant = (int)category.Parant,
                Name = category.Name
            };

            var childList = lstCategory.Where(a => a.Parant == newRecord.Id).ToList();
            var tempList = new List<CategoryViewModel>();
            foreach (var item in childList)
            {
                updateCollection(item, lstCategory, ref tempList);
            }
            newRecord.Children = new List<CategoryViewModel>(tempList);
            if (lst == null)
                lst = new List<CategoryViewModel>();
            lst.Add(newRecord);
        }

        public ActionResult GetItemCategories()
        {
            using (var db = GetConnection)
            {


                var lst = db.Category.Where(r => r.Id > 0).ToList();

                var root = lst.Where(a => a.CategoryLevel == 1).ToList();

                var result = new List<CategoryViewModel>();

                foreach (var item in root)
                {
                    updateCollection(item, lst, ref result);
                }

                return PartialView("ItemCategory", result);
            }
            //return PartialView("ItemCategory", lstItemCategory);
        }

        public ActionResult GetCategoryList()
        {
            using (var db = GetConnection)
            {
                List<CategoryViewModel> result = db.Category.Where(c => c.Deleted == false).Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Parant = (int)c.Parant,
                    Name = c.Name,
                    CategoryLevel = c.CategoryLevel
                }).ToList();
                return PartialView("PartialCategoryList", result);
            }
        }

        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = GetConnection)
            {
                Category category = db.Category.FirstOrDefault(c => c.Id == id);
                if (category == null)
                {
                    return HttpNotFound();
                }
                return View(category);
            }
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                CategoryViewModel viewModel = new CategoryViewModel();
                using (var db = GetConnection)
                {
                    var category1 = db.Category.FirstOrDefault(c => c.Id == id);
                    viewModel.Id = category1.Id;
                    viewModel.Name = category1.Name;
                    viewModel.Parant = (int)category1.Parant;
                    viewModel.CategoryLevel = category1.CategoryLevel;
                    viewModel.Active = category1.Active;
                    viewModel.ColorCode = category1.ColorCode;
                    viewModel.IconId = category1.IconId;
                    viewModel.SortOrder = category1.SortOrder;
                    ViewBag.SelectedValue = category1.Type;
                    viewModel.Type = category1.Type;
                    //fill icon list
                    var lst = db.Category.Where(c => c.Deleted == false).ToList();
                    var lstcategories = lst.Select(c => new ItemCategoryViewModel
                    {
                        CategoryId = c.Id,
                        Parant = (int)c.Parant,
                        Name = c.Name,
                        CategoryLevel = c.CategoryLevel,
                        IsSelected = (viewModel.Id == id && viewModel.Parant == c.Id)
                    }).ToList();
                    var icons = db.IconStore.Where(c => c.Type == IconType.Category).ToList();
                    List<IconStore> iconList = new List<IconStore>();
                    iconList.Add(new IconStore { Id = 0, Title = "No image" });
                    iconList.AddRange(icons);
                    viewModel.Icons = iconList.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Title
                    });
                    SeededCategories model = new Models.SeededCategories { Seed = 0, Categories = lstcategories };
                    viewModel.SeedCategories = model;
                }
                return PartialView(viewModel);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        [HttpPost]
        public ActionResult Edit(CategoryViewModel viewModel)
        {
            string msg = "";
            try
            {
                if (ModelState.IsValid)
                {
                    var db = GetConnection;
                    using (var uof = new UnitOfWork(GetConnection))
                    {
                        var catRepo = uof.CategoryRepository;

                        var parent = catRepo.FirstOrDefault(c => c.Id == viewModel.Parant);
                        int level = 1;
                        if (parent != null)
                            level = parent.CategoryLevel + 1;


                        var category = catRepo.First(c => c.Id == viewModel.Id);
                        category.Name = viewModel.Name;
                        category.Parant = viewModel.Parant;
                        category.CategoryLevel = level;
                        category.Active = viewModel.Active;
                        category.IconId = viewModel.IconId;
                        category.ColorCode = viewModel.ColorCode;
                        category.SortOrder = viewModel.SortOrder;
                        category.Updated = DateTime.Now;
                        category.Type = viewModel.Type;
                        catRepo.AddOrUpdate(category);

                        var terminalRepo = uof.TerminalRepository;
                        var terminals = terminalRepo.GetAll().ToList();
                        foreach (var terminal in terminals)
                        {
                            if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == category.Id.ToString()))
                            {
                                var syncLog = new TablesSyncLog
                                {
                                    OutletId = terminal.OutletId,
                                    TerminalId = terminal.Id,
                                    TableKey = category.Id.ToString(),
                                    TableName = TableName.Category

                                };
                                db.TablesSyncLog.Add(syncLog);
                                db.SaveChanges();
                            }
                        }
                        uof.Commit();
                    }

                    msg = "Success:Category updated successfully";
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
        public ActionResult Delete(int id)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(db))
                {
                    var catRepo = uof.CategoryRepository; ;

                    if (catRepo.Any(m => m.Parant == id))
                    {
                        msg = "Error:Cannot delete this because category is associated with other category";
                    }
                    else
                    {
                        var category = catRepo.First(c => c.Id == id);
                        var itemcat = uof.ItemCategoryRepository.FirstOrDefault(i => i.CategoryId == id);
                        if (itemcat == null)
                        {
                            category.Deleted = true;
                            category.Updated = DateTime.Now;
                            catRepo.AddOrUpdate(category);

                            var terminalRepo = uof.TerminalRepository;
                            var terminals = terminalRepo.GetAll().ToList();
                            foreach (var terminal in terminals)
                            {
                                if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == category.Id.ToString()))
                                {
                                    var syncLog = new TablesSyncLog
                                    {
                                        OutletId = terminal.OutletId,
                                        TerminalId = terminal.Id,
                                        TableKey = category.Id.ToString(),
                                        TableName = TableName.Category

                                    };
                                    db.TablesSyncLog.Add(syncLog);
                                    db.SaveChanges();
                                }
                            }
                            uof.Commit();
                            msg = "Success:" + Resource.Deleted + " " + Resource.successfully;

                        }
                        else
                        {
                            msg = "Error:Cannot delete this because category is associated with Product";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Restore(int id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var catRepo = uof.CategoryRepository; ;


                    var category = catRepo.First(c => c.Id == id);
                    category.Deleted = false;
                    category.Updated = DateTime.Now;
                    catRepo.AddOrUpdate(category);
                    uof.Commit();
                    msg = Resource.Restore + " " + Resource.successfully;


                }

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssignItem(int id)
        {
            List<AssignCategoryViewModel> models = new List<AssignCategoryViewModel>();

            using (var uof = new UnitOfWork(GetConnection))
            {
                var prodRepo = uof.ProductRepository;
                var itmCatRepo = uof.ItemCategoryRepository;

                var ItemList = prodRepo.GetAll().ToList();


                foreach (var itms in ItemList)
                {
                    var model = new AssignCategoryViewModel();

                    model.ItemId = itms.Id;
                    model.Name = itms.Description;
                    model.CategoryId = id;
                    model.IsSelected = itmCatRepo.Any(i => i.ItemId == itms.Id && i.CategoryId == id);

                    models.Add(model);
                }
            }
            return View(models);
        }
        public ActionResult SaveItems(List<AssignCategoryViewModel> items)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var itmCatRepo = uof.ItemCategoryRepository;

                    foreach (var itm in items)
                    {
                        if (itm.IsSelected)
                        {

                            var itmCategory = itmCatRepo.GetAll().Where(i => i.ItemId == itm.ItemId && i.CategoryId == itm.CategoryId);
                            if (!itmCategory.Any())
                            {
                                ItemCategory itemcategory = new ItemCategory();
                                itemcategory.ItemId = itm.ItemId;
                                itemcategory.CategoryId = itm.CategoryId;
                                itemcategory.SortOrder = 1;
                                itmCatRepo.Add(itemcategory);
                                uof.Commit();
                            }

                        }
                        else
                        {

                            var itemcategory = itmCatRepo.AsQueryable().Where(s => s.ItemId == itm.ItemId);
                            if (itemcategory.Any())
                            {
                                foreach (var itmCat in itemcategory)
                                {
                                    itmCatRepo.Delete(itmCat);
                                }
                                uof.Commit();
                            }
                        }
                    }
                }
                msg = "Item(s) assigned successfully";
            }
            catch (Exception ex)
            {

                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }


        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<CategoryViewModel> data { get; set; }
        }

        private List<CategoryViewModel> FilterData(ref int recordFiltered, int start, int length, string search, int sortColumn, string sortDirection)
        {

            var uof = new UnitOfWork(GetConnection);
            var catRep = uof.CategoryRepository;
            TOTAL_ROWS = catRep.GetAll().Count();
            var _data = catRep.Where(c => c.Parant > 0).Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                CategoryLevel = c.CategoryLevel,
                Active = c.Active,
                ColorCode = c.ColorCode,
                Deleted = c.Deleted,
                Parant = c.Parant

            }).OrderBy(o => o.Id);
            List<CategoryViewModel> list = new List<CategoryViewModel>();
            if (search == null)
            {
                list = _data.Skip(start).Take(length).ToList();
            }
            else
            {
                // simulate search
                var data = _data.Where(c => c.Name.Contains(search)).Skip(start).Take(length);
                foreach (CategoryViewModel dataItem in data)
                {
                    if (dataItem.Name != null)
                    {
                        dataItem.Edit = "<a class='btn btn-primary btn-gradient btn-sm fa fa-edit' style='margin - right:2px; float:right;'  onclick='Edit(" + dataItem.Id + ")'>" + Resource.Edit + "</a>";
                        dataItem.Delete = "<a class='btn btn-danger btn-gradient btn-sm fa fa-trash-o' style='margin - right:2px; float:right;'  onclick='Delete(" + dataItem.Id + ")'>" + Resource.Delete + "</a>";
                        list.Add(dataItem);
                    }
                }
            }



            recordFiltered = list.Count;

            // get just one page of data
            // list = list.GetRange(start, Math.Min(length, list.Count - start));

            return list;
        }

        public ActionResult AjaxGetJsonData(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (length == -1)
            {
                length = TOTAL_ROWS;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = TOTAL_ROWS;
            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);


        }


        public PartialViewResult CategoryCampaign(int id)
        {
            CategoryCampaignViewModel model = new CategoryCampaignViewModel() { CategoryId = id, Active = true };
            using (var db = GetConnection)
            {

                var data = db.CategoryCampaign.FirstOrDefault(p => p.CategoryId == id && !p.IsDeleted);
                if (data != null)
                {
                    model.Id = data.Id;
                    model.CampaignId = data.CampaignId;
                    model.Active = data.Active;
                }
                var campaings = db.Campaign.Where(c => !c.IsDeleted).ToList();
                model.Campaigns = campaings.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Description
                });
            }
            return PartialView(model);

        }

        public ActionResult SaveCategoryCampaign(CategoryCampaignViewModel viewModel)
        {
            string msg = "";
            try
            {
                if (viewModel.CategoryId == 0)
                    msg = "Error:" + Resource.Select + " " + Resource.Category;
                else
                {
                    using (var uof = new UnitOfWork(GetConnection))
                    {
                        var CategoryCampaignRepo = uof.CategoryCampaignRepository;
                        if (viewModel.CampaignId == 0)
                        {
                            CategoryCampaign catCampaign = CategoryCampaignRepo.FirstOrDefault(ic =>ic.CategoryId == viewModel.CategoryId);
                            if (catCampaign != null)
                            {
                                catCampaign.IsDeleted = true;
                                catCampaign.Updated = DateTime.Now;
                                CategoryCampaignRepo.AddOrUpdate(catCampaign);
                                uof.Commit();
                            }
                            msg = "Success" + ":" + Resource.Category + " " + Resource.Campaign + " " + Resource.saved;
                            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
                        }


                        CategoryCampaign CategoryCampaign = CategoryCampaignRepo.FirstOrDefault(ic => ic.CategoryId == viewModel.CategoryId);
                        bool isEdit = false;
                        if (CategoryCampaign == null)
                        {
                            int lastId = 0;
                            try
                            {
                                lastId = (int)CategoryCampaignRepo.Max(m => m.Id);
                                lastId = lastId + 1;
                            }
                            catch
                            {
                                lastId = 1;
                            }
                            CategoryCampaign = new CategoryCampaign();
                            CategoryCampaign.Id = lastId;
                        }
                        else
                        {
                            CategoryCampaign.CategoryId = viewModel.CategoryId;
                            CategoryCampaign.IsDeleted = false;
                            isEdit = true;
                        }
                        CategoryCampaign.CampaignId = viewModel.CampaignId;
                        CategoryCampaign.Active = viewModel.Active;
                        CategoryCampaign.IsDeleted = false;
                        CategoryCampaign.CategoryId = viewModel.CategoryId;
                        CategoryCampaign.Updated = DateTime.Now;
                        if (isEdit == false)
                            CategoryCampaignRepo.Add(CategoryCampaign);

                        uof.Commit();
                    }
                    msg = "Success" + ":" + Resource.Category + " " + Resource.Campaign + " " + Resource.saved;
                }
            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }




    }
}