using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using POSSUM.Web.Models;
using System.Data.Entity;
using POSSUM.Model;
using System.Net;

namespace POSSUM.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class InventoryHistoryController : MyBaseController
    {
        public InventoryHistoryController()
        {

        }

        public ActionResult Index()
        {
            return View();
        }

        [ValidateInput(false)]
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
                                                  CreatedDate = dbObject.CreatedDate,
                                                  Status = dbObject.Status
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
                                     CreatedDate = dbObject.CreatedDate,
                                     Status = dbObject.Status
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
                                     c.InventoryName.ToString(),
                                     c.Status.ToString()
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
            try
            {
                using (var db = GetConnection)
                {
                    var inventoryHistory = db.InventoryHistory.FirstOrDefault(obj => obj.InventoryHistoryId == inventoryHistoryId);

                    ViewBag.InventoryHistoryId = inventoryHistoryId;
                    ViewBag.InventoryHistoryStatus = inventoryHistory.Status;

                    return View();
                }
            }
            catch (Exception e)
            {
                return View();
            }
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
                                                                        BarCode = !string.IsNullOrEmpty(obj.Product.BarCode) ? obj.Product.BarCode : "",
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

        public ActionResult InventorySummary(Guid inventoryHistoryId)
        {
            ViewBag.InventoryHistoryId = inventoryHistoryId;
            return View();
        }

        public ActionResult GetInventorySummary(jQueryDataTableParamModel param, Guid inventoryHistoryId)
        {
            try
            {
                using (var db = GetConnection)
                {
                    List<ProductStockHistoryViewModel> productStockHistories = new List<ProductStockHistoryViewModel>();

                    var inventoryDetails = (from p in db.StockHistoryGroup
                                            select new ProductStockHistoryGroupViewModel
                                            {
                                                StockHistoryGroupId = p.StockHistoryGroupId,
                                                GroupName = p.GroupName,
                                                CreatedDate = p.CreatedDate,
                                                InventoryHistoryId = p.InventoryHistoryId,
                                            })
                                            .Where(obj => obj.InventoryHistoryId == inventoryHistoryId)
                                            .OrderBy(p => p.CreatedDate)
                                            .ToList();

                    foreach (var group in inventoryDetails)
                    {
                        group.StockHistory = db.ProductStockHistory.Include(s => s.Product)
                                                                    .Select(obj => new ProductStockHistoryViewModel
                                                                    {
                                                                        ProductId = obj.ProductId,
                                                                        ProductName = obj.Product.Description,
                                                                        BarCode = !string.IsNullOrEmpty(obj.Product.BarCode) ? obj.Product.BarCode : "",
                                                                        PurchasePrice = obj.Product.PurchasePrice,
                                                                        Tax = obj.Product.Tax,
                                                                        ProductStock = obj.ProductStock,
                                                                        NewStock = obj.StockValue,
                                                                        StockValue = obj.StockValue,
                                                                        StockHistoryGroupId = obj.StockHistoryGroupId,
                                                                        CreatedOn = obj.CreatedOn,
                                                                    })
                                                                    .Where(obj => (obj.StockHistoryGroupId != null && 
                                                                    obj.StockHistoryGroupId == group.StockHistoryGroupId) &&
                                                                    (string.IsNullOrEmpty(param.sSearch) ||
                                                                    (obj.ProductName != null && obj.ProductName.ToLower().Contains(param.sSearch.ToLower()))
                                                                    || (obj.ProductName != null && obj.BarCode.ToLower().Contains(param.sSearch.ToLower()))))
                                                                    .ToList();

                        if (group.StockHistory.Count() > 0)
                        {
                            group.StockHistory.ForEach(obj => { 
                                obj.CreatedDateString = obj.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss");

                                var productStockHistory = productStockHistories.FirstOrDefault(objHistory => objHistory.ProductId == obj.ProductId);
                                if (productStockHistory != null)
                                {
                                    var moms = (obj.PurchasePrice * obj.Tax) / (100 + obj.Tax);
                                    productStockHistory.NewStock = productStockHistory.NewStock + obj.StockValue;
                                    productStockHistory.Value = (obj.ProductStock - productStockHistory.NewStock) * (obj.PurchasePrice - moms);
                                }
                                else
                                {
                                    var moms = (obj.PurchasePrice * obj.Tax) / (100 + obj.Tax);
                                    obj.Value = (obj.ProductStock - obj.NewStock) * (obj.PurchasePrice - moms);
                                    productStockHistories.Add(obj);
                                }
                                    
                            });
                        }
                    }

                    productStockHistories = productStockHistories.Where(obj => (obj.ProductStock - obj.NewStock) != 0)
                                                                .OrderBy(obj => obj.ProductName)
                                                                .ToList();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = productStockHistories.Count(),
                        iTotalDisplayRecords = productStockHistories.Count(),
                        aaData = productStockHistories
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

        [HttpPost]
        public ActionResult UpdateInventoryHistory(Guid inventoryHistoryId, List<ProductStockHistoryViewModel> productStockHistories)
        {
            try
            {
                using (var db = GetConnection)
                {
                    productStockHistories.ForEach(obj => {
                        var productStockHistory = db.ProductStockHistory.FirstOrDefault(objHistory => objHistory.Id == obj.Id);
                        productStockHistory.NewStock = productStockHistory.ProductStock + obj.StockValue;
                        productStockHistory.StockValue = obj.StockValue;

                        db.Entry(productStockHistory).State = EntityState.Modified;
                    });

                    db.SaveChanges();

                    return Json("Success");
                }
            } 
            catch (Exception e)
            {
                return Json("Error");
            }
        }

        [HttpPost]
        public ActionResult ApproveInventoryHistory(Guid inventoryHistoryId, List<ProductStockHistoryViewModel> productStockHistories)
        {
            try
            {
                using (var db = GetConnection)
                {
                    List<ProductStockHistoryViewModel> productsTotalStock = new List<ProductStockHistoryViewModel>();

                    var inventoryHistory = db.InventoryHistory.FirstOrDefault(obj => obj.InventoryHistoryId == inventoryHistoryId);
                    inventoryHistory.Status = 1;

                    for (var i = 0; i < productStockHistories.Count; i++)
                    {
                        var productFinalStock = productsTotalStock.FirstOrDefault(obj => obj.ProductId == productStockHistories[i].ProductId);
                        if (productFinalStock != null)
                        {
                            decimal totalStock = productFinalStock.NewStock + productStockHistories[i].StockValue;
                            productsTotalStock.ForEach(obj => { 
                                if (obj.ProductId == productFinalStock.ProductId) 
                                    obj.NewStock = totalStock; 
                            });
                        }
                        else
                        {
                            productStockHistories[i].NewStock = productStockHistories[i].StockValue;
                            productsTotalStock.Add(productStockHistories[i]);
                        }
                    }

                    //Updating Histories
                    productStockHistories.ForEach(obj => {
                        var productStockHistory = db.ProductStockHistory.FirstOrDefault(objHistory => objHistory.Id == obj.Id);
                        productStockHistory.NewStock = productStockHistory.ProductStock + obj.StockValue;
                        productStockHistory.StockValue = obj.StockValue;

                        db.Entry(productStockHistory).State = EntityState.Modified;
                    });

                    //Updating Product
                    productsTotalStock.ForEach(obj =>
                    {
                        var _product = db.Product.FirstOrDefault(u => u.Id == obj.ProductId);
                        _product.Updated = DateTime.Now;

                        //Also Create History
                        ProductStockHistory productStockHistory = new ProductStockHistory();
                        productStockHistory.Id = Guid.NewGuid();
                        productStockHistory.ProductId = obj.ProductId;
                        productStockHistory.CreatedOn = DateTime.Now;
                        productStockHistory.UpdatedOn = DateTime.Now;

                        //Update Product Stock
                        if (_product.Unit == ProductUnit.Piece)
                        {
                            productStockHistory.ProductStock = _product.StockQuantity;
                            productStockHistory.LastStock = _product.StockQuantity;

                            _product.StockQuantity = obj.NewStock;
                        }
                        else
                        {
                            productStockHistory.ProductStock = (_product.Weight != null) ? _product.Weight.Value : 0;
                            productStockHistory.LastStock = (_product.Weight != null) ? _product.Weight.Value : 0;

                            _product.Weight = obj.NewStock;
                        }

                        productStockHistory.NewStock = obj.NewStock;
                        productStockHistory.StockValue = obj.NewStock;

                        db.Entry(_product).State = EntityState.Modified;
                        //Also Create History
                        db.ProductStockHistory.Add(productStockHistory);
                    });

                    db.SaveChanges();

                    return Json("Success");
                }
            } 
            catch (Exception e)
            {
                return Json("Error");
            }
        }

        public ActionResult StockHistory()
        {
            return View();
        }

        [ValidateInput(false)]
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
                                   BarCode = !string.IsNullOrEmpty(p.Product.BarCode) ? p.Product.BarCode : "",
                                   ProductStock = p.ProductStock,
                                   LastStock = p.LastStock,
                                   NewStock = p.NewStock,
                                   StockValue = p.StockValue,
                                   StockHistoryGroupId = p.StockHistoryGroupId
                               })
                               .Where(obj => obj.StockHistoryGroupId == null)
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
                        || !string.IsNullOrEmpty(c.BarCode.ToString()) && c.BarCode.ToString().Contains(param.sSearch.ToLower())
                        || !string.IsNullOrEmpty(c.ProductStock.ToString()) && c.ProductStock.ToString().Contains(param.sSearch.ToLower())
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
                                     c.BarCode.ToString(),
                                     //c.ProductStock.ToString(),
                                     c.LastStock.ToString(),
                                     c.StockValue.ToString(),
                                     c.NewStock.ToString()
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
                                                 InventoryHistoryId = p.InventoryHistoryId
                                             })
                                            .Where(obj => obj.InventoryHistoryId == null && 
                                            (string.IsNullOrEmpty(param.sSearch) ||
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
                                                                        BarCode = !string.IsNullOrEmpty(obj.Product.BarCode) ? obj.Product.BarCode : "",
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

        public ActionResult Settings()
        {
            ViewBag.MinStockLevel = "0";

            using (var db = GetConnection)
            {
                var MinStock = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.MinStockLevel);
                if (MinStock != null)
                    ViewBag.MinStockLevel = MinStock.Value;
            }

            return View();
        }

        public ActionResult SaveInventorySettings(decimal? MinStockValue)
        {
            try
            {
                using (var db = GetConnection)
                {
                    if (MinStockValue == null)
                        MinStockValue = 0;

                    var MinStock = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.MinStockLevel);
                    if (MinStock != null)
                    {
                        MinStock.Value = MinStockValue.ToString();
                        db.Entry(MinStock).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else {
                        var maxId = db.Setting.Max(a => a.Id) + 1;
                        var setting = new Setting()
                        {
                            Id = maxId,
                            Updated = DateTime.Now,
                            Code = Model.SettingCode.MinStockLevel,
                            Value = MinStockValue.ToString(),
                            Created = DateTime.Now,
                            Description = "Default Min Stock Quantity",
                            OutletId = Guid.Empty,
                            Sort = 0,
                            Type = SettingType.MiscSettings,
                            TerminalId = Guid.Empty
                        };

                        db.Setting.Add(setting);
                        db.SaveChanges();
                    }
                    
                    string query = "UPDATE Product SET MinStockLevel=" + MinStockValue + ", Updated='" + DateTime.Now + "'";
                    db.Database.ExecuteSqlCommand(query);

                    return Json("Success");
                }
            }
            catch(Exception e)
            {
                return Json("Error");
            }
        }
    }
}