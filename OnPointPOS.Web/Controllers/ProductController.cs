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
using MvcJqGrid;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data.Entity;

namespace POSSUM.Web.Controllers
{
    //  [Authorize(Roles = "Admin")]
    [Authorize]
    public class ProductController : MyBaseController
    {
        public class UploadExcelModel
        {
            public string Barcode { get; set; }
            public string DiscountAllowed { get; set; }
            public string Category { get; set; }
            public string Accounting { get; set; }
            public string Price { get; set; }
            public string ProductName { get; set; }
            public string AskWeight { get; set; }
            public string PLU { get; set; }
            public string ShowButton { get; set; }
            public string AskPrice { get; set; }
        }

        [HttpPost]
        public JsonResult SaveFilterProducts(List<Guid> model)
        {
            Session["FilterProducts"] = model;
            return Json("Ok");
        }

        [HttpGet]
        public JsonResult ProductBarCodeReport(int catId, int pageNo = 0, int pageSize = 5)
        {
            SaleDataProvider provider = new SaleDataProvider();
            var data = provider.GetItemStockWithPagination(GetConnection, catId, pageNo, pageSize);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Upload(FormCollection formCollection)
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    try
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            var lstMessages = new List<string>();

                            int indexProductName = 1;
                            int indexPrice = 2;
                            int indexAccounting = 3;
                            int indexBarcode = 4;
                            int indexCategory = 5;
                            int indexDiscountAllowed = 6;
                            int indexPLU = 7;
                            int indexAskPrice = 8;
                            int indexAskWeight = 9;
                            int indexShowButton = 10;

                            var lst = new List<UploadExcelModel>();
                            Log.WriteLog("Found " + noOfRow + " rows in file, Reading file now.");
                            #region Reading File
                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                            {
                                var obj = new UploadExcelModel();

                                obj.ProductName = GetString(workSheet, rowIterator, indexProductName);
                                obj.Price = GetString(workSheet, rowIterator, indexPrice);
                                obj.DiscountAllowed = GetString(workSheet, rowIterator, indexDiscountAllowed);
                                obj.Category = GetString(workSheet, rowIterator, indexCategory);
                                obj.Accounting = GetString(workSheet, rowIterator, indexAccounting);
                                obj.Barcode = GetString(workSheet, rowIterator, indexBarcode);
                                obj.PLU = GetString(workSheet, rowIterator, indexPLU);
                                obj.AskPrice = GetString(workSheet, rowIterator, indexAskPrice);
                                obj.AskWeight = GetString(workSheet, rowIterator, indexAskWeight);
                                obj.ShowButton = GetString(workSheet, rowIterator, indexShowButton);
                                if (!string.IsNullOrEmpty(obj.ProductName))
                                {
                                    lst.Add(obj);
                                }
                            }
                            #endregion

                            Log.WriteLog("Found " + lst.Count + " records, Updating in database now.");
                            using (ApplicationDbContext db = GetConnection)
                            {
                                var itemJson = "";
                                var lstAccountings = db.Accounting.ToList();
                                foreach (var item in lst)
                                {
                                    try
                                    {
                                        itemJson = JsonConvert.SerializeObject(item);
                                        Log.WriteLog("Adding Record# " + lst.IndexOf(item) + " in database with following details. " + itemJson);

                                        var accountingId = 1;
                                        decimal tax = Convert.ToDecimal(6);

                                        item.Accounting = Convert.ToInt32(Convert.ToDouble(item.Accounting) * 100) + "%";

                                        var objAccounting = lstAccountings.FirstOrDefault(a => a.Name != null && a.Name.Contains(item.Accounting));
                                        if (objAccounting != null)
                                        {
                                            tax = objAccounting.TAX;
                                            accountingId = objAccounting.Id;
                                        }
                                        else
                                        {
                                            objAccounting = lstAccountings.FirstOrDefault(a => a.Name != null && !a.Name.Contains("%"));
                                            if (objAccounting != null)
                                            {
                                                tax = objAccounting.TAX;
                                                accountingId = objAccounting.Id;
                                            }
                                        }

                                        var objCategory = db.Category.FirstOrDefault(a => a.Name == item.Category);
                                        if (objCategory == null)
                                        {
                                            int lastId = db.Category.Max(c => c.Id);

                                            objCategory = new Category()
                                            {
                                                Id = lastId + 1,
                                                Name = item.Category,
                                                Updated = DateTime.Now,
                                                CategoryLevel = 2,
                                                Parant = 1
                                            };

                                            db.Category.Add(objCategory);
                                            db.SaveChanges();
                                        }

                                        var productIsNew = false;// Using this to identify if its category is primary or not
                                        var duscountAllowed = item.DiscountAllowed == "True";
                                        var price = Convert.ToDecimal(item.Price == "NULL" ? "0" : item.Price);

                                        var product = db.Product.FirstOrDefault(a => a.BarCode == item.Barcode
                                        && !string.IsNullOrEmpty(item.Barcode));
                                        if (product == null)
                                        {
                                            product = db.Product.FirstOrDefault(a => a.Description == item.ProductName
                                            && a.DiscountAllowed == duscountAllowed
                                            && a.Price == price
                                            && a.BarCode == item.Barcode
                                            && a.AccountingId == objAccounting.Id);
                                            if (product == null)
                                            {
                                                productIsNew = true;
                                                product = new Product()
                                                {
                                                    Tax = tax,
                                                    Id = Guid.NewGuid(),
                                                    AccountingId = accountingId,
                                                    Active = true,
                                                    AskPrice = item.AskPrice == "1",
                                                    AskVolume = false,
                                                    AskWeight = item.AskWeight == "1",
                                                    BarCode = item.Barcode,
                                                    Bong = false,
                                                    Created = DateTime.Now,
                                                    Deleted = false,
                                                    Description = item.ProductName,
                                                    Price = price,
                                                    DiscountAllowed = duscountAllowed,
                                                    PLU = item.PLU == "NULL" ? "" : item.PLU,
                                                    ShowItemButton = item.ShowButton == "1"
                                                };

                                                db.Product.Add(product);
                                                db.SaveChanges();
                                            }
                                        }

                                        var objItemCategory = db.ItemCategory.FirstOrDefault(a => a.CategoryId == objCategory.Id && a.ItemId == product.Id);

                                        if (objItemCategory == null)
                                        {
                                            objItemCategory = new ItemCategory()
                                            {
                                                CategoryId = objCategory.Id,
                                                IsPrimary = productIsNew,
                                                ItemId = product.Id,
                                                SortOrder = 1
                                            };

                                            db.ItemCategory.Add(objItemCategory);
                                            db.SaveChanges();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteLog("Some thing went wrong for item with following json = " + itemJson + " with error detail = " + ex.ToString());
                                    }
                                }
                            }

                            ViewBag.DisplayMessages = "Products Imported Successfully!";
                            ViewBag.HasError = false;
                            return View("Index");
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.DisplayMessages = "Some thing went wrong, please try again. Thanks";
                        ViewBag.HasError = true;
                        return View("Index");
                    }
                }
            }
            ViewBag.DisplayMessages = @"Success";
            return View("Index");
        }

        private string GetString(ExcelWorksheet workSheet, int row, int column)
        {
            try
            {
                if (row > 0 && column > 0)
                {
                    return Convert.ToString(workSheet.Cells[row, column].Value);
                }

                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }


        private int TOTAL_ROWS = 0;

        //   static List<ItemViewModel> _data = new List<ItemViewModel>();
        public ProductController()
        {

        }

        [Authorize]
        public ActionResult Index()
        {
            //   _data = CreateData();
            ViewBag.Statuses = new[] { Resource.Active, Resource.Inactive };
            ViewBag.DeleteStatus = new[] { Resource.Deleted };
            ViewBag.Categories = GetCategories().Select(c => c.Name).ToArray();

            ViewBag.Tax = GetTAX().Select(c => c.Name).ToArray();
            ViewBag.AccountingCode = GetAccountingCode().Select(c => c.Name).ToArray();

            return View();


        }
        public ActionResult Reports()
        {

            return View();
        }

        private List<AssignCategoryViewModel> GetCategories()
        {
            using (var db = GetConnection)
            {
                return db.Category.Where(c => c.CategoryLevel > 1 && c.Active == true && c.Deleted == false).Select(c => new AssignCategoryViewModel
                {
                    Name = c.Name
                }).Distinct().ToList();
            }
        }

        private List<AssignCategoryViewModel> GetTAX()
        {
            using (var db = GetConnection)
            {
                return db.Accounting.Select(c => new AssignCategoryViewModel
                {
                    Name = c.TAX.ToString()
                }).Distinct().ToList();
            }
        }

        private List<AssignCategoryViewModel> GetAccountingCode()
        {
            using (var db = GetConnection)
            {
                return db.Accounting.Select(c => new AssignCategoryViewModel
                {
                    Name = c.AcNo.ToString()
                }).Distinct().ToList();
            }
        }

        #region JQ search

        public ActionResult GeProductByFilter(GridSettings gridSettings, string category = "")
        {

            using (var db = GetConnection)
            {
                var accountings = db.Accounting.ToList();
                int catId = 0;
                int.TryParse(category, out catId);
                IQueryable<ItemViewModel> data;
                if (catId > 0)
                    data = from itm in db.Product.Where(a => a.PlaceHolder == false && a.ItemType != ItemType.Pant)
                           join ic in db.ItemCategory.Where(c => c.CategoryId == catId) on itm.Id equals ic.ItemId
                           join accounting in db.Accounting on itm.AccountingId equals accounting.Id
                           //from dataLeft in leftJoinedData.DefaultIfEmpty()
                           select new ItemViewModel
                           {
                               MinStockLevel = itm.MinStockLevel,
                               AccountingCode = accounting.AcNo.ToString(),
                               Id = itm.Id,
                               Description = itm.Description,
                               BarCode = itm.BarCode,
                               //SKU = itm.SKU,
                               Tax = accounting.TAX,
                               //Active = itm.Active,
                               Price = itm.Price,
                               AccountingId = itm.AccountingId,
                               Deleted = itm.Deleted,
                               Created = itm.Created,
                               Updated = itm.Updated,
                               PLU = itm.PLU,
                               StockQuantity = itm.StockQuantity,
                               Weight = itm.Weight,

                           };
                else
                    data = from itm in db.Product.Where(a => a.PlaceHolder == false && a.ItemType != ItemType.Pant)
                           join accounting in db.Accounting on itm.AccountingId equals accounting.Id //into leftJoinedData
                           //from dataLeft in leftJoinedData.DefaultIfEmpty()
                           select new ItemViewModel
                           {
                               MinStockLevel = itm.MinStockLevel,
                               AccountingCode = accounting.AcNo.ToString(),
                               Id = itm.Id,
                               Description = itm.Description,
                               BarCode = itm.BarCode,
                               //SKU = itm.SKU,
                               Tax = accounting.TAX,
                               //Active = itm.Active,
                               Price = itm.Price,
                               AccountingId = itm.AccountingId,
                               Deleted = itm.Deleted,
                               Created = itm.Created,
                               Updated = itm.Updated,
                               PLU = itm.PLU,
                               StockQuantity = itm.StockQuantity,
                               Weight = itm.Weight,

                           };
                var products = GetOrderedProducts(data, gridSettings.SortColumn, gridSettings.SortOrder);

                if (gridSettings.IsSearch)
                {
                    //products = gridSettings.Where.rules.Aggregate(products, FilterProducts);
                    products = FilterProducts(products, gridSettings.Where.rules);
                }
                else
                {
                    products = products.Where(itm => itm.Deleted == false);
                }

                var total = 0;
                if (products != null)
                    total = products.Count();
                var _categories = products.OrderByDescending(o => o.Updated).Skip((gridSettings.PageIndex - 1) * gridSettings.PageSize).Take(gridSettings.PageSize).ToList();

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
                        c.Description,
                        c.BarCode,
                        //c.SKU,
                        //c.PLU,
                        c.StockQuantity.ToString(),
                        c.Weight.ToString(),
                        //c.ActiveStatus,
                        c.Tax.ToString(),
                        c.AccountingCode,//=  accountings.FirstOrDefault(ac => ac.Id == c.AccountingId) != null ? accountings.First(ac => ac.Id == c.AccountingId).AcNo.ToString() : "0",
                        c.Price.ToString(),
                        c.Deleted.ToString(),
                        (c.MinStockLevel != null && c.MinStockLevel > 0 && c.MinStockLevel >= c.StockQuantity) ? "1" : "0"
                        }
                        }).ToArray()
                };

                return Json(d, JsonRequestBehavior.AllowGet);
            }
        }

        private static IQueryable<ItemViewModel> FilterProducts(IQueryable<ItemViewModel> Products, MvcJqGrid.Rule[] rules)
        {
            bool deleted = rules.FirstOrDefault(obj => obj.data == Resource.Deleted) != null ? true : false;

            foreach (var rule in rules)
            {
                if (rule.field.Equals("Description"))
                {
                    Products = Products.Where(c => c.Description.Contains(rule.data));
                }
                else if (rule.field.Equals("BarCode"))
                {
                    Products = Products.Where(c => c.BarCode.Contains(rule.data));
                }
                else if (rule.field.Equals("SKU"))
                {
                    Products = Products.Where(c => c.SKU.Contains(rule.data));
                }
                else if (rule.field.Equals("PLU"))
                {
                    Products = Products.Where(c => c.PLU.Contains(rule.data));
                }
                else if (rule.field.Equals("StockQuantity"))
                {
                    var stock = Convert.ToDecimal(rule.data);
                    Products = Products.Where(c => c.StockQuantity == stock);
                }
                else if (rule.field.Equals("ActiveStatus"))
                {
                    bool status = rule.data == Resource.Active ? true : false;
                    Products = Products.Where(c => c.Active == status);
                }
                else if (rule.field.Equals("Tax"))
                {
                    var value = Convert.ToDecimal(rule.data);
                    Products = Products.Where(c => c.Tax == value);
                }
                else if (rule.field.Equals("AccountingCode"))
                {
                    var value = Convert.ToString(rule.data);
                    Products = Products.Where(c => c.AccountingCode == value);
                }
            }

            return Products.Where(c => c.Deleted == deleted);
        }




        private static IQueryable<ItemViewModel> FilterProducts_Old(IQueryable<ItemViewModel> Products, MvcJqGrid.Rule rule)
        {
            switch (rule.field)
            {


                case "Description":
                    return Products.Where(c => c.Description.Contains(rule.data) && c.Deleted == false);
                case "BarCode":
                    return Products.Where(c => c.BarCode.Contains(rule.data) && c.Deleted == false);
                case "SKU":
                    return Products.Where(c => c.SKU.Contains(rule.data) && c.Deleted == false);
                case "PLU":
                    return Products.Where(c => c.PLU.Contains(rule.data) && c.Deleted == false);
                case "StockQuantity":
                    {
                        var stock = Convert.ToDecimal(rule.data);
                        return Products.Where(c => c.StockQuantity == stock);
                    }

                case "ActiveStatus":
                    {
                        bool status = rule.data == Resource.Active ? true : false;
                        return Products.Where(c => c.Active == status);

                    }

                case "Tax":
                    {
                        var value = Convert.ToDecimal(rule.data);
                        return Products.Where(c => c.Tax == value && c.Deleted == false);
                    }


                case "AccountingCode":
                    {
                        var value = Convert.ToString(rule.data);
                        return Products.Where(c => c.AccountingCode == value && c.Deleted == false);
                    }


                case "Deleted":
                    {
                        bool deleted = rule.data == Resource.Deleted ? true : false;
                        return Products.Where(c => c.Deleted == deleted);

                    }
                default:
                    return Products.Where(c => c.Deleted == false); ;
            }
        }

        private static IQueryable<ItemViewModel> GetOrderedProducts(IQueryable<ItemViewModel> Products, string sortColumn, string sortOrder)
        {
            switch (sortColumn)
            {
                case "Id":
                    return (sortOrder == "desc") ? Products.OrderByDescending(c => Convert.ToInt32(c.Id))
                        : Products.OrderBy(c => Convert.ToInt32(c.Id));

                case "FullName":
                    return (sortOrder == "desc") ? Products.OrderByDescending(c => c.Description) : Products.OrderBy(c => c.Description);
                default:
                    return Products;
            }
        }

        #endregion
        public ActionResult Placeholder()
        {
            return View();
        }


        public ActionResult Seamless()
        {
            return View();
        }

        public ActionResult GetProductsByCategory(int id, int active)
        {
            string search = Request.QueryString["search[value]"];

            List<ItemViewModel> result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {

                if (active == 0 && id == 0)
                {

                    result = (from itm in db.Product.Where(itm => itm.Deleted == false && itm.Description != " " && itm.ItemType == 0)
                                  //where (itm.CategoryId == id)
                              join cat in db.ItemCategory on itm.Id equals cat.ItemId

                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  SortOrder = itm.SortOrder,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).Distinct().ToList();
                }
                else if (id == 0)
                {
                    bool IsActive = active == 1 ? true : false;
                    result = (from itm in db.Product.Where(m => m.Active == IsActive && m.Description != " " && m.ItemType == 0)
                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).Distinct().ToList();

                }
                else if (active == 0)
                {
                    result = (from itm in db.Product
                              join cat in db.ItemCategory on itm.Id equals cat.ItemId
                              where cat.CategoryId == id && itm.Description != " " && itm.ItemType == 0
                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  SortOrder = itm.SortOrder,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).Distinct().ToList();
                }
                else
                {
                    bool IsActive = active == 1 ? true : false;
                    result = (from itm in db.Product
                              join cat in db.ItemCategory on itm.Id equals cat.ItemId
                              where itm.Active == IsActive && cat.CategoryId == id && itm.Description != " " && itm.ItemType == 0
                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).Distinct().ToList();
                }
            }
            //  _data = result;
            ItemViewModel model = new ItemViewModel();

            model.Products = result.Select(itm => new Product
            {
                Id = itm.Id,
                Description = itm.Description,
                Price = itm.Price,
                Tax = itm.Tax,
                AskPrice = itm.AskPrice,
                Active = itm.Active,
                SortOrder = itm.SortOrder,
                BarCode = itm.BarCode,
                Unit = itm.Unit
            }).ToList();
            return PartialView("_productList", result);
            // return Json(result.OrderBy(o => o.SortOrder).ToList(), JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetProductListByCategory(int id)
        {
            List<ItemViewModel> result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                if (id == 0)
                {

                    result = (from itm in db.Product.Where(itm => itm.Seamless == false && itm.Deleted == false && itm.Description != " " && itm.Active == true && itm.ItemType == ItemType.Individual)
                                  //where (itm.CategoryId == id)
                              join cat in db.ItemCategory on itm.Id equals cat.ItemId

                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  SortOrder = itm.SortOrder,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).Distinct().ToList();
                }

                else
                {

                    result = (from itm in db.Product
                              join cat in db.ItemCategory on itm.Id equals cat.ItemId
                              where itm.Seamless == false && itm.Active == true && itm.ItemType == ItemType.Individual && cat.CategoryId == id && itm.Description != " "
                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).Distinct().ToList();
                }

            }
            return PartialView("_productList", result);
            // return PartialView("PartialProductList", result);
        }

        public ActionResult GetPlaceholderByCategory(int id, int placeholder)
        {
            List<ItemViewModel> result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                bool isplaceholder = placeholder == 1 ? true : false;
                if (id == 0)
                {
                    result = (from itm in db.Product.Where(itm => itm.Deleted == false && itm.PlaceHolder == isplaceholder && itm.ItemType != ItemType.Pant)
                                  //where (itm.CategoryId == id)
                                  //join cat in catRepo on itm.Id equals cat.ItemId

                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  SortOrder = itm.SortOrder,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).ToList();
                }
                else
                {
                    result = (from itm in db.Product.Where(itm => itm.Deleted == false && itm.PlaceHolder == isplaceholder)

                              join cat in db.ItemCategory on itm.Id equals cat.ItemId
                              where (cat.CategoryId == id)
                              select new ItemViewModel
                              {
                                  Id = itm.Id,
                                  Description = itm.Description,
                                  Price = itm.Price,
                                  Tax = itm.Tax,
                                  AskPrice = itm.AskPrice,
                                  Active = itm.Active,
                                  SortOrder = itm.SortOrder,
                                  BarCode = itm.BarCode,
                                  Unit = itm.Unit
                              }).ToList();
                }
            }
            //  _data = result;
            ItemViewModel model = new ItemViewModel();

            model.Products = result.Select(itm => new Product
            {
                Id = itm.Id,
                Description = itm.Description,
                Price = itm.Price,
                Tax = itm.Tax,
                AskPrice = itm.AskPrice,
                Active = itm.Active,
                SortOrder = itm.SortOrder,
                BarCode = itm.BarCode,
                Unit = itm.Unit
            }).ToList();
            return PartialView("_placeholderlist", result);
            //   return Json(result.OrderBy(o => o.SortOrder).ToList(), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetDeletedProduct(int deleted)
        {
            List<ItemViewModel> result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                bool isDeleted = deleted == 0 ? false : true;
                result = (from itm in db.Product.Where(itm => itm.Deleted == isDeleted && itm.ItemType != ItemType.Pant)

                          select new ItemViewModel
                          {
                              Id = itm.Id,
                              Description = itm.Description,
                              Price = itm.Price,
                              Tax = itm.Tax,
                              AskPrice = itm.AskPrice,
                              Active = itm.Active,
                              SortOrder = itm.SortOrder,
                              BarCode = itm.BarCode,
                              Deleted = itm.Deleted,
                              Unit = itm.Unit
                          }).ToList();
            }
            // _data = result;
            if (deleted == 1)
                return PartialView("_deletedproductList", result);
            else
                return PartialView("_productList", result);
            // return Json(result.OrderBy(o => o.SortOrder).ToList(), JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SelectListItem> FillUnitType()
        {
            IEnumerable<ProductUnit> unitTypes = Enum.GetValues(typeof(ProductUnit))
                                                       .Cast<ProductUnit>();

            var types = from enumValue in unitTypes
                        select new SelectListItem
                        {
                            Text = enumValue.ToString(),
                            Value = ((int)enumValue).ToString(),

                        };
            return types;
        }
        private IEnumerable<SelectListItem> FillPreparationTime()
        {
            IEnumerable<PrepareTime> preparationTimes = Enum.GetValues(typeof(PrepareTime))
                                                       .Cast<PrepareTime>();

            var times = from enumValue in preparationTimes
                        select new SelectListItem
                        {
                            Text = enumValue.ToString(),
                            Value = ((int)enumValue).ToString(),

                        };
            return times;
        }
        private IEnumerable<SelectListItem> FillReceiptMethod()
        {
            IEnumerable<ReceiptMethod> receiptMethod = Enum.GetValues(typeof(ReceiptMethod))
                                                       .Cast<ReceiptMethod>();

            var types = from enumValue in receiptMethod
                        select new SelectListItem
                        {
                            Text = enumValue.ToString(),
                            Value = ((int)enumValue).ToString(),

                        };
            return types;
        }

        public ActionResult Create()
        {
            var Product = new ItemViewModel { Unit = ProductUnit.Piece, TaxValue = 12, Active = true, Deleted = false, PurchasePrice = 0, DiscountAllowed = true, ShowItemButton = false };

            try
            {
                using (var db = GetConnection)
                {
                    var categories = db.Category.Where(r => r.Id > 0 && r.Deleted == false).Select(c => new ItemCategoryViewModel
                    {
                        CategoryId = c.Id,
                        Parant = (int)c.Parant,
                        Name = c.Name,
                        CategoryLevel = c.CategoryLevel
                    })
                    .ToList();

                    var model = new SeededCategories { Seed = 0, Categories = categories };
                    Product.SeedCategories = model;

                    var printer = db.Printer.ToList();
                    Product.Printers = printer.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.PrinterName
                    })
                    .ToList();

                    var taxes = db.Tax.ToList();
                    Product.Taxes = taxes.Select(p => new SelectListItem
                    {
                        Value = p.TaxValue.ToString(),
                        Text = p.TaxValue + " %"
                    })
                    .ToList();

                    Product.Accountings = db.Accounting.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.AcNo + "-" + p.Name + " (" + p.TAX + "%)"
                    })
                    .ToList();

                    Product.UnitTypes = FillUnitType();
                    Product.PreparationTimes = FillPreparationTime();

                    //   return PartialView(Product);

                    ViewBag.ShowBong = true;

                    var setting = db.Setting.FirstOrDefault(a => a.Description == "SaleType");
                    if (setting != null && setting.Type == SettingType.HardwareSettings)
                    {
                        ViewBag.ShowBong = false;
                    }

                    List<Product> list = new List<Product>();
                    try
                    {
                        list = db.Product.Where(p => !p.Deleted && p.ItemType == ItemType.Pant).ToList();
                    }
                    catch (Exception)
                    {

                    }

                    var lastProduct = db.Product.Where(a => !string.IsNullOrEmpty(a.ColorCode)).OrderByDescending(a => a.Created).FirstOrDefault();
                    if (lastProduct != null)
                    {
                        Product.ColorCode = lastProduct.ColorCode;
                    }

                    ViewBag.PantProduct = new SelectList(list, "Id", "Description");
                }

                return PartialView(Product);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(ItemViewModel viewModel)
        {
            string msg = "";
            try
            {
                if (viewModel.PlaceHolder && string.IsNullOrEmpty(viewModel.Description))
                    viewModel.Description = " ";

                if (ModelState.IsValid)
                {
                    var db = GetConnection;

                    using (var uof = new UnitOfWork(db))
                    {
                        var prodRepo = uof.ProductRepository;
                        var terminalRepo = uof.TerminalRepository;

                        var accountingRepo = uof.AccountingRepository;
                        var accounting = accountingRepo.FirstOrDefault(ac => ac.Id == viewModel.AccountingId);
                        if (accounting != null)
                            viewModel.Tax = accounting.TAX;

                        var product = new Product
                        {
                            Id = Guid.NewGuid(),
                            Description = viewModel.Description,
                            Price = viewModel.Price,
                            PurchasePrice = viewModel.PurchasePrice,
                            TempPrice = viewModel.TempPrice,
                            TempPriceEnd = viewModel.TempPriceEnd,
                            TempPriceStart = viewModel.TempPriceStart,
                            Tax = viewModel.Tax,
                            BarCode = viewModel.BarCode,
                            PLU = viewModel.PLU,
                            Unit = viewModel.Unit,
                            AskWeight = viewModel.AskWeight,
                            AskVolume = viewModel.AskVolume,
                            AskPrice = viewModel.AskPrice,
                            PriceLock = viewModel.PriceLock,
                            ColorCode = viewModel.ColorCode,
                            PrinterId = viewModel.PrinterId,
                            SortOrder = viewModel.SortOrder,
                            ShowItemButton = viewModel.ShowItemButton,
                            Active = true,
                            Deleted = false,
                            SKU = viewModel.SKU,
                            Created = DateTime.Now,
                            Updated = DateTime.Now,
                            PlaceHolder = viewModel.PlaceHolder,
                            //  Seamless = viewModel.Seamless,
                            Sticky = viewModel.Sticky,
                            Bong = viewModel.Bong,
                            AccountingId = viewModel.AccountingId,
                            NeedIngredient = viewModel.NeedIngredient,
                            DiscountAllowed = viewModel.DiscountAllowed,
                            PreparationTime = viewModel.PreparationTime,
                            ReorderLevelQuantity = viewModel.ReorderLevelQuantity,
                            StockQuantity = viewModel.StockQuantity,
                            IsPantEnabled = viewModel.IsPantEnabled,
                            PantProductId = viewModel.PantProductId,
                            Weight = viewModel.Weight,

                        };

                        bool isValided = true;

                        if (!string.IsNullOrEmpty(viewModel.BarCode) || !string.IsNullOrEmpty(viewModel.SKU) || !string.IsNullOrEmpty(viewModel.PLU))
                        {
                            if (!string.IsNullOrEmpty(viewModel.BarCode))
                            {
                                if (IsBarCodeExists(viewModel.BarCode))
                                {
                                    string validationErrors = viewModel.BarCode + " already exists please specify a unique barcode!";
                                    msg = "Error:" + validationErrors;
                                    isValided = false;
                                }
                            }
                            if (!string.IsNullOrEmpty(viewModel.SKU))
                            {
                                if (IsSkuExists(viewModel.SKU))
                                {
                                    string validationErrors = viewModel.SKU + " already exists please specify a unique sku!";
                                    msg = "Error:" + validationErrors;
                                    isValided = false;
                                }
                            }
                            if (!string.IsNullOrEmpty(viewModel.PLU))
                            {
                                if (IsPluExists(viewModel.PLU))
                                {
                                    string validationErrors = viewModel.PLU + " already exists please specify a unique plu!";
                                    msg = "Error:" + validationErrors;
                                    isValided = false;
                                }
                            }
                        }

                        if (isValided)
                        {
                            prodRepo.Add(product);

                            if (!string.IsNullOrEmpty(viewModel.SelectedIds))
                            {
                                SaveItemCategory(uof, viewModel.SelectedIds, product.Id, viewModel.selectedPrimary);
                            }

                            //Add Inventory Task
                            //var inventoryRepo = uof.InventoryTaskRepository;
                            //var invetoryTask = new InventoryTask
                            //{
                            //    Id = Guid.NewGuid(),
                            //    Created = DateTime.Now,
                            //    ItemId = Product.Id,
                            //    StatusMessage = "Product Added"

                            //};
                            //inventoryRepo.Add(invetoryTask);

                            var terminals = terminalRepo.GetAll().ToList();
                            foreach (var terminal in terminals)
                            {
                                if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == product.Id.ToString()))
                                {
                                    var syncLog = new TablesSyncLog
                                    {
                                        OutletId = terminal.OutletId,
                                        TerminalId = terminal.Id,
                                        TableKey = product.Id.ToString(),
                                        TableName = TableName.Product

                                    };

                                    db.TablesSyncLog.Add(syncLog);
                                    db.SaveChanges();
                                }
                            }

                            uof.Commit();

                            msg = "Success:Product saved successfully.";
                        }
                    }
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

        private string RandomHTMLColor()
        {
            var random = new Random();
            var color = String.Format("#{0:X6}", random.Next(0x1000000));
            return color;
        }

        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ItemViewModel data = new ItemViewModel();
            Guid ProductGuid = Guid.Parse(id);

            using (var db = GetConnection)
            {
                var viewModel = db.Product.FirstOrDefault(c => c.Id == ProductGuid);

                data.Id = viewModel.Id;
                data.Description = viewModel.Description;
                data.Price = viewModel.Price;
                data.PurchasePrice = viewModel.PurchasePrice;
                data.TempPrice = viewModel.TempPrice;
                data.MinStockLevel = viewModel.MinStockLevel;
                data.TempPriceEnd = viewModel.TempPriceEnd;
                data.TempPriceStart = viewModel.TempPriceStart;
                data.Tax = viewModel.Tax;
                data.BarCode = viewModel.BarCode;
                data.PLU = viewModel.PLU;
                data.SKU = viewModel.SKU;
                data.Unit = viewModel.Unit;
                data.AskWeight = viewModel.AskWeight;
                data.AskVolume = viewModel.AskVolume;
                data.AskPrice = viewModel.AskPrice;
                data.PriceLock = viewModel.PriceLock;
                data.ColorCode = viewModel.ColorCode;
                data.PrinterId = viewModel.PrinterId;
                data.SortOrder = viewModel.SortOrder;
                data.TaxValue = (int)viewModel.Tax;
                data.Active = viewModel.Active;
                data.Tax = viewModel.Tax;
                data.ReorderLevelQuantity = viewModel.ReorderLevelQuantity;
                data.PlaceHolder = viewModel.PlaceHolder;
                data.ShowItemButton = viewModel.ShowItemButton;
                //  data.Seamless = viewModel.Seamless;
                data.Sticky = viewModel.Sticky;
                data.Bong = viewModel.Bong;
                data.NeedIngredient = viewModel.NeedIngredient;
                data.AccountingId = viewModel.AccountingId;
                data.DiscountAllowed = viewModel.DiscountAllowed;
                data.PreparationTime = viewModel.PreparationTime;
                data.StockQuantity = viewModel.StockQuantity;
                data.IsPantEnabled = viewModel.IsPantEnabled;
                data.PantProductId = viewModel.PantProductId;
                data.Weight = viewModel.Weight;

                var categories = db.Category.Where(r => r.Id > 0 && r.Deleted == false).Select(c => new ItemCategoryViewModel
                {
                    CategoryId = c.Id,
                    Parant = (int)c.Parant,
                    Name = c.Name,
                    CategoryLevel = c.CategoryLevel
                })
                .ToList();

                var ItemCategory = db.ItemCategory.Where(c => c.ItemId == ProductGuid).ToList();

                var itmcategories = categories.Select(c => new ItemCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Parant = (int)c.Parant,
                    Name = c.Name,
                    CategoryLevel = c.CategoryLevel,
                    IsSelected = ItemCategory.Any(ci => ci.CategoryId == c.CategoryId),
                    IsPrimary = ItemCategory.Count > 0 ? ItemCategory.Any(ci => ci.CategoryId == c.CategoryId) ? ItemCategory.FirstOrDefault(ci => ci.CategoryId == c.CategoryId).IsPrimary : false : false

                })
                .ToList();

                SeededCategories model = new SeededCategories { Seed = 0, Categories = itmcategories };
                foreach (var pc in ItemCategory)
                {
                    data.SelectedIds = data.SelectedIds + pc.CategoryId + ",";
                }

                if (!string.IsNullOrEmpty(data.SelectedIds))
                    data.SelectedIds = data.SelectedIds.TrimEnd(',');
                data.SeedCategories = model;

                var printer = db.Printer.ToList();
                data.Printers = printer.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.PrinterName
                });

                var taxes = db.Tax.ToList();
                data.Taxes = taxes.Select(p => new SelectListItem
                {
                    Value = p.TaxValue.ToString(),
                    Text = p.TaxValue + " %"
                });

                var accountings = db.Accounting.ToList();
                data.Accountings = accountings.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.AcNo + "-" + p.Name + " (" + p.TAX + "%)"
                });

                data.UnitTypes = FillUnitType();
                data.PreparationTimes = FillPreparationTime();

                if (ItemCategory.Any(ci => ci.ItemId == data.Id && ci.IsPrimary))
                    data.selectedPrimary = "P-" + ItemCategory.FirstOrDefault(ci => ci.ItemId == data.Id && ci.IsPrimary).CategoryId.ToString();
                else
                    data.selectedPrimary = "P-0";

                ViewBag.ShowBong = true;

                var setting = db.Setting.FirstOrDefault(a => a.Description == "SaleType");
                if (setting != null && setting.Type == SettingType.HardwareSettings)
                {
                    ViewBag.ShowBong = false;
                }

                List<Product> list = new List<Product>();
                try
                {
                    list = db.Product.Where(p => !p.Deleted && p.ItemType == ItemType.Pant).ToList();
                }
                catch (Exception ex)
                {

                }

                ViewBag.PantProduct = new SelectList(list, "Id", "Description");
            }

            return PartialView(data);
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Edit(ItemViewModel viewModel)
        {
            string msg = "";
            try
            {
                if (ModelState.IsValid)
                {
                    var db = GetConnection;

                    Product data = null;
                    decimal productLastStock = 0;
                    decimal productNewStock = 0;
                    decimal? productLastWeight = 0;
                    decimal? productNewWeight = 0;
                    bool isAddStockHistory = true;

                    using (var uof = new UnitOfWork(db))
                    {
                        var prodRepo = uof.ProductRepository;
                        var accountingRepo = uof.AccountingRepository;

                        var accounting = accountingRepo.FirstOrDefault(ac => ac.Id == viewModel.AccountingId);
                        if (accounting != null)
                            viewModel.Tax = accounting.TAX;

                        data = prodRepo.FirstOrDefault(p => p.Id == viewModel.Id);
                        data.Description = viewModel.Description;
                        data.Price = (decimal)viewModel.Price;
                        data.PurchasePrice = viewModel.PurchasePrice;
                        data.TempPrice = viewModel.TempPrice;
                        data.MinStockLevel = viewModel.MinStockLevel;
                        data.TempPriceEnd = viewModel.TempPriceEnd;
                        data.TempPriceStart = viewModel.TempPriceStart;
                        data.Tax = viewModel.Tax;
                        data.BarCode = viewModel.BarCode;
                        data.PLU = viewModel.PLU;
                        data.Unit = viewModel.Unit;
                        data.AskWeight = viewModel.AskWeight;
                        data.AskVolume = viewModel.AskVolume;
                        data.AskPrice = viewModel.AskPrice;
                        data.PriceLock = viewModel.PriceLock;
                        data.ColorCode = viewModel.ColorCode;
                        data.PrinterId = viewModel.PrinterId;
                        data.SortOrder = viewModel.SortOrder;
                        data.Active = viewModel.Active;
                        data.PlaceHolder = viewModel.PlaceHolder;
                        data.AccountingId = viewModel.AccountingId;
                        data.ReorderLevelQuantity = viewModel.ReorderLevelQuantity;
                        // data.Seamless = viewModel.Seamless;
                        data.Sticky = viewModel.Sticky;
                        data.Bong = viewModel.Bong;
                        data.NeedIngredient = viewModel.NeedIngredient;
                        data.Updated = DateTime.Now;
                        data.ShowItemButton = viewModel.ShowItemButton;
                        data.DiscountAllowed = viewModel.DiscountAllowed;
                        data.PreparationTime = viewModel.PreparationTime;
                        data.SKU = viewModel.SKU;
                        if (data.Unit == ProductUnit.Piece)
                        {
                            productLastStock = data.StockQuantity;
                            productNewStock = viewModel.StockQuantity;

                            if (productNewStock == productLastStock)
                                isAddStockHistory = false;
                        }
                        else
                        {
                            productLastWeight = (data.Weight != null) ? data.Weight : 0;
                            productNewWeight = (viewModel.Weight != null) ? viewModel.Weight : 0;

                            if (productNewWeight == productLastWeight)
                                isAddStockHistory = false;
                        }
                        data.StockQuantity = viewModel.StockQuantity;
                        data.IsPantEnabled = viewModel.IsPantEnabled;
                        data.PantProductId = viewModel.PantProductId;
                        data.Weight = viewModel.Weight;
                        prodRepo.AddOrUpdate(data);

                        if (viewModel.SelectedIds != null)
                        {
                            //  DeleteItemCategory(viewModel.Id);
                            SaveItemCategory(uof, viewModel.SelectedIds, viewModel.Id, viewModel.selectedPrimary);
                        }
                        //Add Inventory Task
                        //var inventoryRepo = uof.InventoryTaskRepository;
                        //var invetoryTask = new InventoryTask
                        //{
                        //    Id = Guid.NewGuid(),
                        //    Created = DateTime.Now,
                        //    ItemId = viewModel.Id,
                        //    StatusMessage = "Product Updated"

                        //};
                        //inventoryRepo.Add(invetoryTask);
                        var terminalRepo = uof.TerminalRepository;
                        var terminals = terminalRepo.GetAll().ToList();
                        foreach (var terminal in terminals)
                        {
                            if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == data.Id.ToString()))
                            {
                                var syncLog = new TablesSyncLog
                                {
                                    OutletId = terminal.OutletId,
                                    TerminalId = terminal.Id,
                                    TableKey = data.Id.ToString(),
                                    TableName = TableName.Product

                                };
                                db.TablesSyncLog.Add(syncLog);
                                db.SaveChanges();
                            }
                        }

                        uof.Commit();
                    }

                    if (isAddStockHistory)
                    {
                        using (var db2 = GetConnection)
                        {
                            ProductStockHistory productStockHistory = new ProductStockHistory();
                            productStockHistory.Id = Guid.NewGuid();
                            productStockHistory.ProductId = data.Id;
                            if (data.Unit == ProductUnit.Piece)
                            {
                                productStockHistory.LastStock = productLastStock;
                                productStockHistory.NewStock = productNewStock;
                                productStockHistory.StockValue = productNewStock;
                            }
                            else
                            {
                                productStockHistory.LastStock = productLastWeight.Value;
                                productStockHistory.NewStock = productNewWeight.Value;
                                productStockHistory.StockValue = productNewWeight.Value;
                            }
                            productStockHistory.CreatedOn = DateTime.Now;
                            productStockHistory.UpdatedOn = DateTime.Now;

                            db2.ProductStockHistory.Add(productStockHistory);

                            db2.SaveChanges();
                        }
                    }

                    msg = "Success:Product updated successfully";
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

        public ActionResult SavePlaceholder(int categoryId)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(db))
                {
                    var prodRepo = uof.ProductRepository;


                    ItemViewModel viewModel = new ItemViewModel
                    {
                        Description = " ",
                        PlaceHolder = true,
                        SelectedIds = categoryId.ToString()
                    };
                    var Product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Description = viewModel.Description,
                        Price = viewModel.Price,
                        PurchasePrice = viewModel.PurchasePrice,
                        TempPrice = viewModel.TempPrice,
                        TempPriceEnd = viewModel.TempPriceEnd,
                        TempPriceStart = viewModel.TempPriceStart,
                        Tax = viewModel.Tax,
                        BarCode = viewModel.BarCode,
                        PLU = viewModel.PLU,
                        Unit = viewModel.Unit,
                        AskWeight = viewModel.AskWeight,
                        AskPrice = viewModel.AskPrice,
                        PriceLock = viewModel.PriceLock,
                        ColorCode = viewModel.ColorCode,
                        PrinterId = viewModel.PrinterId,
                        SortOrder = viewModel.SortOrder,
                        ShowItemButton = viewModel.ShowItemButton,
                        Active = true,
                        Deleted = false,
                        SKU = viewModel.SKU,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        PlaceHolder = viewModel.PlaceHolder,
                        //  Seamless = viewModel.Seamless,
                        Sticky = viewModel.Sticky,
                        Bong = viewModel.Bong




                    };
                    prodRepo.Add(Product);

                    if (!string.IsNullOrEmpty(viewModel.SelectedIds))
                    {
                        SaveItemCategory(uof, viewModel.SelectedIds, Product.Id, viewModel.selectedPrimary);
                    }

                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == Product.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = Product.Id.ToString(),
                                TableName = TableName.Product

                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }

                    uof.Commit();
                }
                msg = "Success:Placeholder saved successfully.";
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult ProductCampaign(Guid id)
        {
            ItemCampaignViewModel model = new ItemCampaignViewModel() { ItemId = id, Active = true, StartDate = DateTime.Now, EndDate = DateTime.Now };
            using (var db = GetConnection)
            {

                var data = db.ProductCampaign.FirstOrDefault(p => p.ItemId == id && !p.IsDeleted);
                if (data != null)
                {
                    model.Id = data.Id;
                    model.CampaignId = data.CampaignId;
                    model.EndDate = data.EndDate;
                    model.StartDate = data.StartDate;
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

        public ActionResult SaveProductCampaign(ItemCampaignViewModel viewModel)
        {
            string msg = "";
            try
            {
                if (viewModel.ItemId == default(Guid))
                    msg = "Error:" + Resource.Select + " " + Resource.Product;
                else
                {
                    using (var uof = new UnitOfWork(GetConnection))
                    {
                        var ProductCampaignRepo = uof.ItemCampaignRepository;
                        if (viewModel.CampaignId == 0)
                        {
                            ProductCampaign productCampaign = ProductCampaignRepo.FirstOrDefault(ic => ic.ItemId == viewModel.ItemId);
                            if (productCampaign != null)
                            {
                                productCampaign.IsDeleted = true;
                                productCampaign.Updated = DateTime.Now;
                                ProductCampaignRepo.AddOrUpdate(productCampaign);
                                uof.Commit();
                            }
                            msg = "Success" + ":" + Resource.Category + " " + Resource.Campaign + " " + Resource.saved;
                            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
                        }
                        ProductCampaign ProductCampaign = ProductCampaignRepo.FirstOrDefault(ic => ic.ItemId == viewModel.ItemId);
                        bool isEdit = false;
                        if (ProductCampaign == null)
                        {
                            int lastId = 0;
                            try
                            {
                                lastId = (int)ProductCampaignRepo.Max(m => m.Id);
                                lastId = lastId + 1;
                            }
                            catch
                            {
                                lastId = 1;
                            }
                            ProductCampaign = new ProductCampaign();
                            ProductCampaign.Id = lastId;
                        }
                        else
                        {
                            ProductCampaign.ItemId = viewModel.ItemId;
                            ProductCampaign.IsDeleted = false;
                            isEdit = true;
                        }
                        DateTime endDate = Convert.ToDateTime(viewModel.EndDate.Year + "-" + viewModel.EndDate.Month + "-" + viewModel.EndDate.Day + "  11:59:00 PM");
                        ProductCampaign.CampaignId = viewModel.CampaignId;
                        ProductCampaign.StartDate = viewModel.StartDate;
                        ProductCampaign.EndDate = endDate;
                        ProductCampaign.Active = viewModel.Active;
                        ProductCampaign.ItemId = viewModel.ItemId;
                        ProductCampaign.IsDeleted = false;
                        ProductCampaign.Updated = DateTime.Now;
                        if (isEdit == false)
                            ProductCampaignRepo.Add(ProductCampaign);

                        uof.Commit();
                    }
                    msg = "Success" + ":" + Resource.Product + " " + Resource.Campaign + " " + Resource.saved;
                }
            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddStock(Guid itemId)
        {
            return PartialView(new ItemStock { ItemId = itemId, ExpiryDate = DateTime.Now });
        }

        [HttpPost]
        public ActionResult SaveItemStock(ItemStock viewModel)
        {
            string msg = "";
            try
            {
                if (viewModel.ItemId == default(Guid))
                    msg = "Error:" + Resource.Select + " " + Resource.Product;
                else
                {
                    using (var db = GetConnection)
                    {
                        viewModel.Updated = DateTime.Now;

                        var dbProduct = db.Product.FirstOrDefault(p => p.Id == viewModel.ItemId);

                        ProductStockHistory productStockHistory = new ProductStockHistory();
                        productStockHistory.Id = Guid.NewGuid();
                        productStockHistory.ProductId = dbProduct.Id;

                        if (dbProduct.Unit == ProductUnit.Piece)
                        {
                            decimal productLastStock = dbProduct.StockQuantity;
                            decimal tempProductStockToAdd = viewModel.Quantity;
                            decimal productNewStock = productLastStock + tempProductStockToAdd;

                            dbProduct.StockQuantity = productNewStock;
                            productStockHistory.LastStock = productLastStock;
                            productStockHistory.NewStock = productNewStock;
                        }
                        else
                        {
                            decimal? productLastWeight = dbProduct.Weight != null ? dbProduct.Weight : 0;
                            decimal tempProductWeightToAdd = viewModel.Quantity;
                            decimal? productNewWeight = productLastWeight + tempProductWeightToAdd;

                            dbProduct.Weight = productNewWeight;
                            productStockHistory.LastStock = productLastWeight.Value;
                            productStockHistory.NewStock = productNewWeight.Value;
                        }

                        productStockHistory.CreatedOn = DateTime.Now;
                        productStockHistory.UpdatedOn = DateTime.Now;

                        db.ItemStock.Add(viewModel);
                        db.ProductStockHistory.Add(productStockHistory);
                        db.Entry(dbProduct).State = System.Data.Entity.EntityState.Modified;

                        db.SaveChanges();
                    }
                    msg = "Success" + ":" + Resource.Product + " " + Resource.Stock + " " + Resource.saved;
                }
            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        private void SaveItemCategory(UnitOfWork uof, string selectedCategories, Guid ItemId, string primaryId)
        {
            List<int> oldIds = new List<int>();

            if (!string.IsNullOrEmpty(primaryId))
                primaryId = primaryId.Replace("P-", "");

            int PrimaryId = 0;
            int.TryParse(primaryId, out PrimaryId);
            string[] Ids = selectedCategories.Split(',');

            var ProductCatRepo = uof.ItemCategoryRepository;
            var _existingItemCategory = ProductCatRepo.Where(c => c.ItemId == ItemId).ToList();

            foreach (var cat in _existingItemCategory)
            {
                bool isOld = false;
                foreach (var newId in Ids)
                {
                    if (PrimaryId == 0)
                    {
                        PrimaryId = Convert.ToInt32(newId);
                    }

                    if (cat.CategoryId.ToString() == newId)
                    {
                        isOld = true;
                        break;
                    }
                }
                if (!isOld)
                    oldIds.Add(cat.CategoryId);

                //if (!cat.CategoryId.ToString().Contains(selectedCategories))
                //{
                //    oldIds.Add(cat.CategoryId);
                //}
            }

            foreach (var categroy in Ids)
            {
                if (PrimaryId == 0)
                {
                    PrimaryId = Convert.ToInt32(categroy);
                }

                int catId = 0;
                int.TryParse(categroy, out catId);

                if (catId != 0)
                {
                    int lastSortOrder = 0;
                    try
                    {
                        lastSortOrder = ProductCatRepo.Where(c => c.CategoryId == catId).Max(m => m.SortOrder);
                        lastSortOrder = lastSortOrder + 1;
                    }
                    catch
                    {
                    }

                    var _ItemCategory = ProductCatRepo.FirstOrDefault(c => c.CategoryId == catId && c.ItemId == ItemId);
                    if (_ItemCategory == null)
                    {
                        bool isPrimary = catId == PrimaryId && PrimaryId != 0 ? true : false;
                        var ItemCategory = new ItemCategory
                        {
                            CategoryId = catId,
                            ItemId = ItemId,
                            SortOrder = lastSortOrder < 1 ? 1 : lastSortOrder,
                            IsPrimary = isPrimary

                        };

                        ProductCatRepo.Add(ItemCategory);
                    }
                    else
                    {
                        bool isPrimary = catId == PrimaryId && PrimaryId != 0 ? true : false;
                        _ItemCategory.IsPrimary = isPrimary;
                        if (_ItemCategory.SortOrder < 1)
                            _ItemCategory.SortOrder = 1;
                    }

                    //else if (_ItemCategory != null)
                    //    _ItemCategory.Deleted = false;

                    //if (_ItemCategory == null)
                    //{
                    //    var ItemCategory = new ItemCategory
                    //    {
                    //        CategoryId = catId,
                    //        ItemId = ItemId,
                    //        Deleted = false

                    //    };
                    //    ProductCatRepo.Add(ItemCategory);
                    //}
                }
            }

            foreach (var id in oldIds)
            {
                var itmCat = ProductCatRepo.FirstOrDefault(ic => ic.CategoryId == id && ic.ItemId == ItemId);
                if (itmCat != null)
                {
                    ProductCatRepo.Delete(itmCat);
                }
            }
        }

        public JsonResult GetItemCategories()
        {
            List<ItemCategoryViewModel> lst = new List<ItemCategoryViewModel>();

            using (var db = GetConnection)
            {
                lst = db.Category.Where(r => r.Id > 0 && r.Deleted == false).Select(c => new ItemCategoryViewModel
                {
                    CategoryId = c.Id,
                    Parant = (int)c.Parant,
                    Name = c.Name,
                    CategoryLevel = c.CategoryLevel,
                    SortOrder = c.SortOrder

                }).ToList();
            }

            return Json(lst.OrderBy(o => o.SortOrder).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductList()
        {
            List<ItemViewModel> result = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                result = db.Product.Select(p => new ItemViewModel
                {
                    Id = p.Id,
                    Description = p.Description,
                    Price = p.Price,
                    Tax = p.Tax,
                    AskPrice = p.AskPrice,
                    Active = p.Active
                }).ToList();
            }

            return PartialView("PartialProductList", result);
        }

        public ActionResult GetSearchByName(string searchToken)
        {

            var model = new ItemViewModel();
            using (var db = GetConnection)
            {

                if (!string.IsNullOrEmpty(searchToken))
                {
                    model.Products = db.Product.Where(p => p.Description.ToLower().Contains(searchToken.ToLower())).ToList().Select(p => new Product
                    {
                        Id = p.Id,
                        Description = p.Description,
                        Price = p.Price,
                        Tax = p.Tax,
                        AskPrice = p.AskPrice,
                        Active = p.Active
                    }).ToList();
                }

                else
                {
                    model.Products = db.Product.Where(a => a.ItemType != ItemType.Pant).ToList().Select(p => new Product
                    {
                        Id = p.Id,
                        Description = p.Description,
                        Price = p.Price,
                        Tax = p.Tax,
                        AskPrice = p.AskPrice,
                        Active = p.Active
                    }).ToList();


                }
            }
            return PartialView("PartialProductList", model.Products);
        }

        public ActionResult GetPrinterList()
        {
            List<Printer> result = new List<Printer>();
            using (var db = GetConnection)
            {
                result = db.Printer.ToList();
            }
            return Json(result.OrderBy(o => o.PrinterName).ToList(), JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetCategoryList()
        {
            List<Category> result = new List<Category>();
            using (var db = GetConnection)
            {
                result = db.Category.Where(c => c.Deleted == false).OrderBy(s => s.SortOrder).ToList();
            }

            return PartialView("PartialProductList", result);

        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(GetConnection))
                { //  DeleteItemCategory(id);
                    var prodRepo = uof.ProductRepository;
                    var Product = prodRepo.First(p => p.Id == id);
                    Product.Deleted = true;
                    Product.Updated = DateTime.Now;
                    prodRepo.AddOrUpdate(Product);

                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == Product.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = Product.Id.ToString(),
                                TableName = TableName.Product

                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }
                    uof.Commit();
                    msg = "Success:Deleted successfully";
                }
                //  _data = CreateData();
                //  return RedirectToAction("Index");


            }

            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Restore(Guid id)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(db))
                {
                    var prodRepo = uof.ProductRepository;
                    var Product = prodRepo.First(p => p.Id == id);
                    Product.Deleted = false;
                    Product.Updated = DateTime.Now;
                    prodRepo.AddOrUpdate(Product);

                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == Product.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = Product.Id.ToString(),
                                TableName = TableName.Product

                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }

                    uof.Commit();
                    msg = "Restored successfully";
                }
                //   _data = CreateData();
                //  return RedirectToAction("Index");


            }

            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        private void DeleteItemCategory(Guid Id)
        {
            using (var uof = new UnitOfWork(GetConnection))
            {
                var itmCatRepo = uof.ItemCategoryRepository;
                var cat = itmCatRepo.Where(ic => ic.ItemId == Id).ToList();
                foreach (var Product in cat)
                {
                    itmCatRepo.Delete(Product);
                }
                uof.Commit();
            }
        }

        private List<ItemViewModel> FilterData(ref int recordFiltered, int start, int length, string search, int sortColumn, string sortDirection)
        {
            List<ItemViewModel> list = new List<ItemViewModel>();

            var db = GetConnection;
            var _data = (from itm in db.Product
                         join accData in db.Accounting on itm.AccountingId equals accData.Id
                         where itm.Description != "" && itm.ItemType != ItemType.Pant
                         select new ItemViewModel
                         {
                             Id = itm.Id,
                             Description = itm.Description,
                             AccountingId = itm.AccountingId,
                             Tax = accData.TAX,
                             Active = itm.Active,
                             Price = itm.Price,
                             BarCode = itm.BarCode,
                             Unit = itm.Unit
                         }).ToList();

            if (search == null)
            {
                list = _data;
                TOTAL_ROWS = _data.Count;
            }
            else
            {
                // simulate search
                foreach (ItemViewModel dataProduct in _data)
                {
                    if (dataProduct.Description.ToUpper().Contains(search.ToUpper()))
                    {
                        if (dataProduct.Deleted)
                            dataProduct.Edit = "<a class='btn btn-primary btn-gradient btn-sm fa fa-edit' style='margin - right:2px; float:right;'  onclick='Restore(" + dataProduct.Id + ")'>" + Resource.Restore + "</a>";
                        else
                        {
                            dataProduct.Edit = "<a class='btn btn-primary btn-gradient btn-sm fa fa-edit' style='margin - right:2px; float:right;'  onclick='Edit(" + dataProduct.Id + ")'>" + Resource.Edit + "</a>";
                            dataProduct.EditCampaign = "<a class='btn btn-primary btn-gradient btn-sm fa fa-newspaper-o' style='margin - right:2px; float:right;' data-toggle='modal' data-target='#addEditModal' onclick='EditCampaign(" + dataProduct.Id + ")'>" + Resource.Campaign + "</a>";
                            dataProduct.Delete = "<a class='btn btn-danger btn-gradient btn-sm fa fa-trash-o' style='margin - right:2px; float:right;'  onclick='Delete(" + dataProduct.Id + ")'>" + Resource.Delete + "</a>";
                        }
                        list.Add(dataProduct);
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

            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;
            dataTableData.recordsTotal = TOTAL_ROWS;
            return PartialView("_productList", dataTableData.data);
            // return Json(dataTableData, JsonRequestBehavior.AllowGet);


        }



        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<ItemViewModel> data { get; set; }
        }

        private List<ItemViewModel> CreateData()
        {

            var model = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                List<ItemViewModel> result = db.Product.Where(a => a.ItemType != ItemType.Pant).OrderBy(o => o.SortOrder).Where(c => c.Deleted == false && c.Description != " " && c.ItemType == 0).Select(c => new ItemViewModel
                {
                    Id = c.Id,
                    Description = c.Description,
                    Price = c.Price,
                    Tax = c.Tax,
                    Active = c.Active,
                    BarCode = c.BarCode

                }).ToList();
                model = result;
                TOTAL_ROWS = result.Count();
            }
            return model;
        }

        private List<ItemViewModel> FilterData()
        {


            var model = new List<ItemViewModel>();
            using (var db = GetConnection)
            {
                model = db.Product.Where(c => c.Deleted == false && c.Description != " " && c.ItemType != ItemType.Pant).ToList().Select(c => new ItemViewModel
                {
                    Id = c.Id,
                    Description = c.Description,
                    Price = c.Price,
                    Tax = c.Tax,
                    Active = c.Active,
                    BarCode = c.BarCode

                }).ToList();

            }
            return model;
        }

        private bool IsBarCodeExists(string barcode)
        {
            using (var db = GetConnection)
            {

                var Product = db.Product.Where(p => p.BarCode.ToLower() == barcode.ToLower()).FirstOrDefault();
                if (Product != null)
                    return true;
                else
                    return false;
            }

        }

        private bool IsSkuExists(string sku)
        {
            using (var db = GetConnection)
            {
                var Product = db.Product.Where(p => p.SKU.ToLower() == sku.ToLower()).FirstOrDefault();
                if (Product != null)
                    return true;
                else
                    return false;
            }

        }

        private bool IsPluExists(string plu)
        {
            using (var db = GetConnection)
            {
                var Product = db.Product.Where(p => p.PLU.ToLower() == plu.ToLower()).FirstOrDefault();
                if (Product != null)
                    return true;
                else
                    return false;
            }

        }


        #region Products Sort
        public ActionResult SortProducts()
        {
            return View();
        }
        public ActionResult Categories(int id)
        {
            using (var db = GetConnection)
            {
                var categories = db.Category.Where(p => p.Deleted == false && p.Parant == id).OrderBy(o => o.SortOrder).Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    SortOrder = c.SortOrder,
                    Name = c.Name,
                    ColorCode = string.IsNullOrEmpty(c.ColorCode) ? " #c2bcbc" : c.ColorCode


                }).ToList();

                return PartialView(categories.OrderBy(o => o.SortOrder).ToList());
            }
        }
        public ActionResult Products(int catId)
        {
            List<ItemViewModel> Products = new List<ItemViewModel>();
            using (var db = GetConnection)
            {

                var subcategories = db.Category.Where(c => c.Parant == catId && c.Deleted == false).Select(c => new ItemViewModel
                {
                    Id = default(Guid),
                    Description = c.Name,
                    SortOrder = c.SortOrder,
                    CategoryId = catId,
                    ColorCode = string.IsNullOrEmpty(c.ColorCode) ? "#c2bcbc" : c.ColorCode,
                    IsItem = 0,

                }).ToList();
                if (subcategories.Count > 0)
                {
                    Products.AddRange(subcategories);
                }

                var products = (from itm in db.Product
                                join cat in db.ItemCategory on itm.Id equals cat.ItemId
                                where itm.Active == true && itm.ShowItemButton == true && itm.Deleted == false && (cat.CategoryId == catId)
                                select new ItemViewModel
                                {

                                    Id = itm.Id,
                                    Description = itm.PlaceHolder == true ? " " : itm.Description,
                                    SortOrder = cat.SortOrder,
                                    ColorCode = string.IsNullOrEmpty(itm.ColorCode) ? "#c2bcbc" : itm.ColorCode,
                                    CategoryId = catId,
                                    IsItem = 1
                                }).ToList();
                if (products.Count > 0)
                    Products.AddRange(products);
            }
            return PartialView(Products.OrderBy(o => o.SortOrder).ToList());

        }

        [HttpPost]
        public ActionResult SaveSorting(List<ItemViewModel> products)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var productRepo = uof.ProductRepository;
                    var categoryRepo = uof.CategoryRepository;
                    var ProductCatRepo = uof.ItemCategoryRepository;
                    foreach (var Product in products)
                    {
                        if (Product.IsItem == 1)
                        {
                            var product = productRepo.FirstOrDefault(p => p.Id == Product.Id);
                            if (product != null)
                            {
                                product.SortOrder = Product.SortOrder;
                                product.Updated = DateTime.Now;
                                productRepo.AddOrUpdate(product);
                            }
                            var ItemCategory = ProductCatRepo.FirstOrDefault(p => p.ItemId == Product.Id && p.CategoryId == Product.CategoryId);
                            if (ItemCategory != null)
                            {
                                ItemCategory.SortOrder = Product.SortOrder < 1 ? 1 : Product.SortOrder;
                            }
                        }
                        else
                        {
                            var category = categoryRepo.FirstOrDefault(c => c.Id == Product.CategoryId);
                            if (category != null)
                            {
                                category.SortOrder = Product.SortOrder;
                                category.Updated = DateTime.Now;
                                categoryRepo.AddOrUpdate(category);
                            }
                        }
                    }
                    uof.Commit();
                }
                msg = "Products Sorted successfully";
            }
            catch (Exception ex)
            {

                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveCategorySorting(List<Category> categories)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var catRepo = uof.CategoryRepository;
                    foreach (var cat in categories)
                    {
                        var category = catRepo.FirstOrDefault(p => p.Id == cat.Id);
                        if (category != null)
                        {
                            category.SortOrder = cat.SortOrder;
                            category.Updated = DateTime.Now;
                            catRepo.AddOrUpdate(category);
                        }
                    }
                    uof.Commit();
                }
                msg = "Categories Sorted successfully";
            }
            catch (Exception ex)
            {

                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRootCategory()
        {
            using (var db = GetConnection)
            {

                var categories = db.Category.Where(c => c.Parant == 0).ToList();
                //categories.Add(new Category { Id = 0, Name = Resource.Select + " " + Resource.Root + " " + Resource.Category });
                return Json(categories.OrderBy(o => o.Id).ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetParentCategories(int id)
        {
            using (var db = GetConnection)
            {

                var categories = (from data in db.Category
                                  where data.Parant == id
                                  && db.Category.Count(a => a.Parant == data.Id && a.Deleted == false) > 0
                                  select data).ToList();

                categories.Add(new Category { Id = 0, Name = Resource.Select + " " + Resource.Parent + " " + Resource.Category });

                return Json(categories.OrderBy(o => o.Id).ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        /*
				#region Sort Category
				public ActionResult SortCategory()
				{
					return View();
				}
				public ActionResult ReportViewCategories(int id)
				{
					using (var db = GetConnection)
					{
						var categories = db.Category.Where(p => p.Deleted == false && p.Parant == id).OrderBy(o => o.ReportOrder).Select(c => new CategoryViewModel
						{
							Id = c.Id,
							ReportOrder = c.ReportOrder,
							Name = c.Name,
							ColorCode = string.IsNullOrEmpty(c.ColorCode) ? " #c2bcbc" : c.ColorCode


						}).ToList();

						return PartialView(categories.OrderBy(o => o.ReportOrder).ToList());
					}
				}


				[HttpPost]
				public ActionResult SaveCategoryReportSorting(List<Category> categories)
				{
					string msg = "";
					try
					{
						using (var uof = new UnitOfWork(GetConnection))
						{
							var catRepo = uof.CategoryRepository;
							foreach (var cat in categories)
							{
								var category = catRepo.FirstOrDefault(p => p.Id == cat.Id);
								if (category != null)
								{
									category.ReportOrder = cat.ReportOrder;
									category.Updated = DateTime.Now;
									catRepo.AddOrUpdate(category);
								}
							}
							uof.Commit();
						}
						msg = "Categories Sorted successfully";
					}
					catch (Exception ex)
					{

						msg = ex.Message;
					}
					return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
				}

				#endregion
				*/
        #region Product Price
        public ActionResult UpdatePrices()
        {
            return View();
        }
        public ActionResult _updatePrices(int catid)
        {
            using (var db = GetConnection)
            {
                var data = (from p in db.Product
                            join o in db.ItemCategory on p.Id equals o.ItemId
                            where o.CategoryId == catid && p.PlaceHolder == false
                            select new ItemViewModel
                            {
                                Id = p.Id,
                                Description = p.Description,
                                BarCode = p.BarCode,
                                PurchasePrice = p.PurchasePrice,
                                Price = p.Price,
                                StockQuantity = p.StockQuantity,
                                MinStockLevel = p.MinStockLevel
                            }).ToList();
                return PartialView(data);
            }
        }

        public class UpdatePricesModel
        {
            public Guid Id { get; set; }
            public decimal Price { get; set; }
            public decimal PurchasePrice { get; set; }
            public decimal StockQuantity { get; set; }
            public decimal? MinStockLevel { get; set; }
        }

        [HttpPost]
        public ActionResult UpdatePrices(List<UpdatePricesModel> products)
        {
            string msg = "";
            try
            {
                using (var db = GetConnection)
                {
                    foreach (var product in products)
                    {
                        var _product = db.Product.FirstOrDefault(p => p.Id == product.Id);
                        _product.Price = product.Price;
                        _product.PurchasePrice = product.PurchasePrice;
                        _product.MinStockLevel = product.MinStockLevel;
                        _product.StockQuantity = product.StockQuantity;
                        _product.Updated = DateTime.Now;

                        db.Entry(_product).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.SaveChanges();
                }

                msg = "Success:Prices updated successfully";
            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProductPrice(Guid ItemId)
        {
            ViewBag.ItemId = ItemId;
            using (var db = GetConnection)
            {
                var data = (from p in db.ProductPrice
                            join o in db.Outlet on p.OutletId equals o.Id
                            where p.ItemId == ItemId
                            select new ProductPriceViewModel
                            {
                                Id = p.Id,
                                ItemId = p.ItemId,
                                OutletId = p.OutletId,
                                OutletName = o.Name,
                                Updated = p.Updated,
                                Price = p.Price,
                                PurchasePrice = p.PurchasePrice,
                                PriceMode = p.PriceMode
                            }).ToList();

                return PartialView(data);
            }
        }

        public ActionResult _PriceList(Guid ItemId)
        {
            using (var db = GetConnection)
            {
                var data = (from p in db.ProductPrice
                            join o in db.Outlet on p.OutletId equals o.Id
                            where p.ItemId == ItemId
                            select new ProductPriceViewModel
                            {
                                Id = p.Id,
                                ItemId = p.ItemId,
                                OutletId = p.OutletId,
                                OutletName = o.Name,
                                Updated = p.Updated,
                                Price = p.Price,
                                PurchasePrice = p.PurchasePrice,
                                PriceMode = p.PriceMode
                            }).ToList();

                return PartialView(data);
            }
        }
        public ActionResult EditPrice(int id, Guid ItemId)
        {
            var model = new ProductPriceViewModel();
            model.ItemId = ItemId;
            using (var db = GetConnection)
            {
                var outlets = db.Outlet.ToList();
                model.Outlets = outlets.Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name });
                model.PriceModes = FillPriceModes();

                if (id > 0)
                {
                    int priceId = Convert.ToInt32(id);
                    var price = db.ProductPrice.FirstOrDefault(p => p.Id == priceId);
                    if (price != null)
                    {
                        model.Id = price.Id;
                        model.ItemId = price.ItemId;
                        model.Price = price.Price;
                        model.PurchasePrice = price.PurchasePrice;
                        model.OutletId = price.OutletId;
                        model.PriceMode = price.PriceMode;
                    }
                }
            }

            return PartialView(model);
        }

        private IEnumerable<SelectListItem> FillPriceModes()
        {
            IEnumerable<PriceMode> priceModes = Enum.GetValues(typeof(PriceMode))
                                                       .Cast<PriceMode>();

            var modes = from enumValue in priceModes
                        select new SelectListItem
                        {
                            Text = enumValue.ToString(),
                            Value = ((int)enumValue).ToString(),

                        };
            return modes;
        }

        [HttpPost]
        public ActionResult SavePrice(ProductPriceViewModel viewModel)
        {
            string msg = "";
            try
            {
                var price = new ProductPrice
                {
                    Id = viewModel.Id,
                    ItemId = viewModel.ItemId,
                    OutletId = viewModel.OutletId,
                    Price = viewModel.Price,
                    PriceMode = viewModel.PriceMode,
                    PurchasePrice = viewModel.PurchasePrice
                };
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var priceRepo = uof.ProductPriceRepository;
                    var productRepo = uof.ProductRepository;
                    var _existingprice = priceRepo.FirstOrDefault(p => p.ItemId == viewModel.ItemId && p.OutletId == viewModel.OutletId && p.PriceMode == viewModel.PriceMode);
                    if (_existingprice != null)
                    {
                        _existingprice.Price = viewModel.Price;
                        _existingprice.PurchasePrice = viewModel.PurchasePrice;

                        _existingprice.Updated = DateTime.Now;
                    }
                    else
                    {
                        if (price.Id == 0)
                        {
                            price.PurchasePrice = price.Price;
                            price.PriceMode = price.PriceMode;
                            price.Updated = DateTime.Now;

                        }
                        else
                        {
                            var _price = priceRepo.FirstOrDefault(p => p.Id == price.Id);
                            _price.Price = price.Price;
                            _price.PurchasePrice = price.Price;
                            _price.PriceMode = price.PriceMode;
                            _price.Updated = DateTime.Now;
                        }
                        priceRepo.AddOrUpdate(price);
                    }
                    var product = productRepo.FirstOrDefault(p => p.Id == viewModel.ItemId);
                    product.Updated = DateTime.Now;
                    productRepo.AddOrUpdate(product);
                    uof.Commit();
                }
                msg = "Success:" + Resource.Price + " " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Add Product Name

        public ActionResult ProductName(Guid ItemId)
        {
            ViewBag.ItemId = ItemId;
            using (var db = GetConnection)
            {
                var data = (from t in db.Product_Text
                            join l in db.Language on t.LanguageId equals l.Id
                            where t.ItemId == ItemId
                            select new ProductTextViewModel
                            {
                                Id = t.Id,
                                ItemId = t.ItemId,
                                LanguageName = l.Name,
                                LanguageId = t.LanguageId,
                                TextDescription = t.TextDescription,

                            }).ToList();

                return PartialView(data);
            }
        }
        public ActionResult EditName(int id, Guid ItemId)
        {
            var model = new ProductTextViewModel();
            model.ItemId = ItemId;
            using (var db = GetConnection)
            {
                var lanaguages = db.Language.ToList();
                model.Languages = lanaguages.Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name }).ToList();


                if (id > 0)
                {
                    int priceId = Convert.ToInt32(id);
                    var text = db.Product_Text.FirstOrDefault(p => p.Id == priceId);
                    if (text != null)
                    {
                        model.Id = text.Id;
                        model.ItemId = text.ItemId;
                        model.TextDescription = text.TextDescription;
                        model.LanguageId = text.LanguageId;

                    }
                }
            }

            return PartialView(model);
        }
        [HttpPost]
        public ActionResult SaveName(ProductTextViewModel viewModel)
        {
            string msg = "";
            try
            {
                var _text = new Product_Text
                {
                    Id = viewModel.Id,
                    ItemId = viewModel.ItemId,
                    LanguageId = viewModel.LanguageId,
                    TextDescription = viewModel.TextDescription
                };
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var textRepo = uof.ProductTextRepository;
                    var productRepo = uof.ProductRepository;
                    var _existingtext = textRepo.FirstOrDefault(p => p.ItemId == viewModel.ItemId && p.LanguageId == viewModel.LanguageId);
                    if (_existingtext != null)
                    {
                        _existingtext.TextDescription = viewModel.TextDescription;
                        _existingtext.Updated = DateTime.Now;
                        textRepo.AddOrUpdate(_existingtext);

                    }
                    else
                    {
                        if (_text.Id == 0)
                        {
                            _text.TextDescription = _text.TextDescription;
                            _text.LanguageId = _text.LanguageId;
                            _text.Updated = DateTime.Now;
                            textRepo.AddOrUpdate(_text);

                        }
                        else
                        {
                            var text = textRepo.FirstOrDefault(p => p.Id == _text.Id);
                            text.TextDescription = _text.TextDescription;
                            text.LanguageId = _text.LanguageId;
                            text.Updated = DateTime.Now;
                            textRepo.AddOrUpdate(text);
                        }

                    }
                    var product = productRepo.FirstOrDefault(p => p.Id == viewModel.ItemId);
                    product.Updated = DateTime.Now;
                    productRepo.AddOrUpdate(product);
                    uof.Commit();
                }
                msg = "Success:" + Resource.Name + " " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _NameList(Guid ItemId)
        {
            using (var db = GetConnection)
            {
                var data = (from t in db.Product_Text
                            join l in db.Language on t.LanguageId equals l.Id
                            where t.ItemId == ItemId
                            select new ProductTextViewModel
                            {
                                Id = t.Id,
                                ItemId = t.ItemId,
                                LanguageName = l.Name,
                                LanguageId = t.LanguageId,
                                TextDescription = t.TextDescription,

                            }).ToList();

                return PartialView(data);
            }
        }
        #endregion

        public ActionResult StockHistory()
        {
            return View();
        }

        public ActionResult GetStockHistory(jQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ProductStockHistoryViewModel> history;
                using (var db = GetConnection)
                {
                    var _product = db.ProductStockHistory.Include(s => s.Product);

                    history = (from p in _product
                               select new ProductStockHistoryViewModel
                               {
                                   Id = p.Id,
                                   CreatedOn = p.CreatedOn,
                                   ProductName = p.Product.Description,
                                   ProductStock = p.ProductStock,
                                   LastStock = p.LastStock,
                                   NewStock = p.NewStock,
                                   StockValue = p.StockValue
                               })
                               .OrderByDescending(obj => obj.CreatedOn);

                    var dd = history.ToList();

                    IEnumerable<ProductStockHistoryViewModel> filteredOrders = history;

                    /*var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                    
                    Func<ProductStockHistoryViewModel, string> orderingFunction = (c => sortColumnIndex == 0 ? c.CreatedOn.ToString() :
                                                                        sortColumnIndex == 1 ? c.ProductName.ToString() :
                                                                        sortColumnIndex == 2 ? c.LastStock.ToString() :
                                                                        sortColumnIndex == 3 ? c.NewStock.ToString() : c.NewStock.ToString());

                    var sortDirection = Request["sSortDir_0"]; // asc or desc
                    if (sortDirection == "asc")
                        filteredOrders = filteredOrders.OrderByDescending(orderingFunction);
                    else
                        filteredOrders = filteredOrders.OrderBy(orderingFunction);*/

                    if (!string.IsNullOrEmpty(param.sSearch))
                    {
                        filteredOrders = filteredOrders.Where(c =>
                         !string.IsNullOrEmpty(c.ProductName) && c.ProductName.ToLower().Contains(param.sSearch.ToLower())
                        || !string.IsNullOrEmpty(c.LastStock.ToString()) && c.ProductStock.ToString().Contains(param.sSearch.ToLower())
                        || !string.IsNullOrEmpty(c.LastStock.ToString()) && c.LastStock.ToString().Contains(param.sSearch.ToLower())
                        || c.CreatedOn.ToString().Contains(param.sSearch)
                        || !string.IsNullOrEmpty(c.NewStock.ToString()) && c.NewStock.ToString().Contains(param.sSearch.ToLower())
                        || !string.IsNullOrEmpty(c.StockValue.ToString()) && c.StockValue.ToString().Contains(param.sSearch.ToLower())
                        );
                    }

                    var displayedOrders = filteredOrders.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                    var result = from c in displayedOrders
                                 select new[]
                                 {
                                     c.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                                     c.ProductName.ToString(),
                                     c.ProductStock.ToString(),
                                     c.LastStock.ToString(),
                                     c.NewStock.ToString(),
                                     c.StockValue.ToString()
                                 };

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = filteredOrders.Count(),
                        iTotalDisplayRecords = filteredOrders.Count(),
                        aaData = result.ToList()
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    sEcho = 0,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                }, JsonRequestBehavior.AllowGet); ;
            }
        }

        public ActionResult StockHistoryGrouped()
        {
            return View();
        }

        public ActionResult GetStockHistoryGrouped(jQueryDataTableParamModel param)
        {
            try
            {
                using (var db = GetConnection)
                {
                    var stockHistoryGroup = (from p in db.StockHistoryGroup
                                             select new ProductStockHistoryGroupViewModel
                                             {
                                                 StockHistoryGroupId = p.StockHistoryGroupId,
                                                 GroupName = p.GroupName,
                                                 CreatedDate = p.CreatedDate,
                                             })
                                            .Where(obj => (string.IsNullOrEmpty(param.sSearch) ||
                                            (obj.GroupName != null && obj.GroupName.ToLower().Contains(param.sSearch.ToLower()))))
                                            .OrderByDescending(p => p.CreatedDate)
                                            .Skip(param.iDisplayStart)
                                            .Take(param.iDisplayLength)
                                            .ToList();

                    var count = (from p in db.StockHistoryGroup
                                 select new ProductStockHistoryGroupViewModel
                                 {
                                     StockHistoryGroupId = p.StockHistoryGroupId,
                                     GroupName = p.GroupName,
                                     CreatedDate = p.CreatedDate,
                                 })
                                .Where(obj => (string.IsNullOrEmpty(param.sSearch) ||
                                (obj.GroupName != null && obj.GroupName.ToLower().Contains(param.sSearch.ToLower()))))
                                .Count();

                    foreach (var group in stockHistoryGroup)
                    {
                        group.CreatedDateString = group.CreatedDate.ToString("yyyy-MM-dd hh:mm:ss");
                        group.StockHistory = db.ProductStockHistory.Include(s => s.Product)
                                                                    .Select(obj => new ProductStockHistoryViewModel
                                                                    {
                                                                        Id = obj.Id,
                                                                        ProductId = obj.ProductId,
                                                                        ProductName = obj.Product.Description,
                                                                        ProductStock = obj.ProductStock,
                                                                        LastStock = obj.LastStock,
                                                                        NewStock = obj.NewStock,
                                                                        StockValue = obj.StockValue,
                                                                        StockHistoryGroupId = obj.StockHistoryGroupId,
                                                                        CreatedOn = obj.CreatedOn,
                                                                    })
                                                                    .Where(obj => obj.StockHistoryGroupId != null &&
                                                                    obj.StockHistoryGroupId == group.StockHistoryGroupId)
                                                                    .OrderBy(obj => obj.CreatedOn)
                                                                    .ToList();

                        if (group.StockHistory.Count() > 0)
                            group.StockHistory.ForEach(obj => { obj.CreatedDateString = obj.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"); });
                    }

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = count,
                        iTotalDisplayRecords = count,
                        aaData = stockHistoryGroup
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    sEcho = 0,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult InventoryHistory()
        {
            return View();
        }

        public ActionResult GetInventories(jQueryDataTableParamModel param)
        {
            try
            {
                using (var db = GetConnection)
                {
                    var inventoryHistories = (from dbObject in db.InventoryHistory
                                              select new InventoryHistoryViewModel
                                              {
                                                  InventoryHistoryId = dbObject.InventoryHistoryId,
                                                  InventoryName = dbObject.InventoryName,
                                                  CreatedDate = dbObject.CreatedDate
                                              })
                                              .Where(obj => (string.IsNullOrEmpty(param.sSearch) ||
                                              ((obj.InventoryName != null && obj.InventoryName.ToLower().Contains(param.sSearch.ToLower()))
                                              || obj.CreatedDate.ToString().Contains(param.sSearch))))
                                              .OrderByDescending(obj => obj.CreatedDate)
                                              .Skip(param.iDisplayStart)
                                              .Take(param.iDisplayLength)
                                              .ToList();

                    var count = (from dbObject in inventoryHistories
                                 select new InventoryHistoryViewModel
                                 {
                                     InventoryHistoryId = dbObject.InventoryHistoryId,
                                     InventoryName = dbObject.InventoryName,
                                     CreatedDate = dbObject.CreatedDate
                                 })
                                 .Where(obj => (string.IsNullOrEmpty(param.sSearch) ||
                                 ((obj.InventoryName != null && obj.InventoryName.ToLower().Contains(param.sSearch.ToLower())) ||
                                 obj.CreatedDate.ToString().Contains(param.sSearch))))
                                 .Count();

                    var result = from c in inventoryHistories
                                 select new[]
                                 {
                                     c.InventoryHistoryId.ToString(),
                                     c.CreatedDate.ToString("yyyy-MM-dd hh:mm:ss"),
                                     c.InventoryName.ToString()
                                 };

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = count,
                        iTotalDisplayRecords = count,
                        aaData = result
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    sEcho = 0,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult InventoryDetails(Guid inventoryHistoryId)
        {
            ViewBag.InventoryHistoryId = inventoryHistoryId;
            return View();
        }

        public ActionResult GetInventoryDetails(jQueryDataTableParamModel param, Guid inventoryHistoryId)
        {
            try
            {
                using (var db = GetConnection)
                {
                    var inventoryDetails = (from p in db.StockHistoryGroup
                                            select new ProductStockHistoryGroupViewModel
                                            {
                                                StockHistoryGroupId = p.StockHistoryGroupId,
                                                GroupName = p.GroupName,
                                                CreatedDate = p.CreatedDate,
                                                InventoryHistoryId = p.InventoryHistoryId,
                                            })
                                            .Where(obj => obj.InventoryHistoryId == inventoryHistoryId &&
                                            (string.IsNullOrEmpty(param.sSearch) ||
                                            (obj.GroupName != null && obj.GroupName.ToLower().Contains(param.sSearch.ToLower()))))
                                            .OrderBy(p => p.CreatedDate)
                                            .Skip(param.iDisplayStart)
                                            .Take(param.iDisplayLength)
                                            .ToList();

                    var count = (from p in db.StockHistoryGroup
                                 select new ProductStockHistoryGroupViewModel
                                 {
                                     StockHistoryGroupId = p.StockHistoryGroupId,
                                     GroupName = p.GroupName,
                                     CreatedDate = p.CreatedDate,
                                     InventoryHistoryId = p.InventoryHistoryId,
                                 })
                                .Where(obj => obj.InventoryHistoryId == inventoryHistoryId &&
                                (string.IsNullOrEmpty(param.sSearch) ||
                                (obj.GroupName != null && obj.GroupName.ToLower().Contains(param.sSearch.ToLower()))))
                                .Count();

                    foreach (var group in inventoryDetails)
                    {
                        group.CreatedDateString = group.CreatedDate.ToString("yyyy-MM-dd hh:mm:ss");
                        group.StockHistory = db.ProductStockHistory.Include(s => s.Product)
                                                                    .Select(obj => new ProductStockHistoryViewModel
                                                                    {
                                                                        Id = obj.Id,
                                                                        ProductId = obj.ProductId,
                                                                        ProductName = obj.Product.Description,
                                                                        ProductStock = obj.ProductStock,
                                                                        LastStock = obj.LastStock,
                                                                        NewStock = obj.NewStock,
                                                                        StockValue = obj.StockValue,
                                                                        StockHistoryGroupId = obj.StockHistoryGroupId,
                                                                        CreatedOn = obj.CreatedOn,
                                                                    })
                                                                    .Where(obj => obj.StockHistoryGroupId != null &&
                                                                    obj.StockHistoryGroupId == group.StockHistoryGroupId)
                                                                    .OrderBy(obj => obj.CreatedOn)
                                                                    .ToList();

                        if (group.StockHistory.Count() > 0)
                            group.StockHistory.ForEach(obj => { obj.CreatedDateString = obj.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"); });
                    }

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = count,
                        iTotalDisplayRecords = count,
                        aaData = inventoryDetails
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    sEcho = 0,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}