using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MvcJqGrid;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;

namespace POSSUM.Web.Controllers
{
    [Authorize]
    public class PantProductsController : MyBaseController
    {

        public ActionResult IndexOld()
        {
            using (var db = GetConnection)
            {
                return View(db.Product.ToList());
            }
        }

        public ActionResult Index()
        {
            ViewBag.Statuses = new[] { Resource.Active, Resource.Inactive };
            ViewBag.DeleteStatus = new[] { Resource.Deleted };
            return View();
        }
        #region JQ search

        public ActionResult GeProductByFilter(GridSettings gridSettings)
        {
            using (var db = GetConnection)
            {

                var data = from itm in db.Product
                           where itm.ItemType == ItemType.Pant
                           select new PantProductViewModel
                           {
                               Id = itm.Id,
                               Name = itm.Description,
                               Price = itm.Price,
                               Deleted = itm.Deleted
                           };

                var products = GetOrderedProducts(data, gridSettings.SortColumn, gridSettings.SortOrder);

                if (gridSettings.IsSearch)
                {
                    products = gridSettings.Where.rules.Aggregate(products, FilterProducts);
                }
                else
                {
                    products = products.Where(itm => itm.Deleted == false);
                }
                var total = 0;
                if (products != null)
                    total = products.Count();
                var _products = products.OrderBy(o => o.Id).Skip((gridSettings.PageIndex - 1) * gridSettings.PageSize).Take(gridSettings.PageSize).ToList();


                var d = new
                {
                    total = total / gridSettings.PageSize + 1,
                    page = gridSettings.PageIndex,
                    records = total,
                    rows = (
                        from c in _products
                        select new
                        {
                            id = c.Id,
                            cell = new string[]
                        {
                        c.Id.ToString(),
                        c.Name,
                        c.Price.ToString(),
                        c.Deleted.ToString()
                        }
                        }).ToArray()
                };

                return Json(d, JsonRequestBehavior.AllowGet);
            }
        }

        private static IQueryable<PantProductViewModel> FilterProducts(IQueryable<PantProductViewModel> items, MvcJqGrid.Rule rule)
        {
            switch (rule.field)
            {
                case "Id":
                    return items.Where(c => c.Id == Guid.Parse(rule.data));

                case "Name":
                    return items.Where(c => c.Name.Contains(rule.data) && c.Deleted == false);

                case "Deleted":
                    {
                        bool deleted = rule.data == Resource.Deleted ? true : false;
                        return items.Where(c => c.Deleted == deleted);

                    }
                default:
                    return items.Where(c => c.Deleted == false);
            }
        }

        private static IQueryable<PantProductViewModel> GetOrderedProducts(IQueryable<PantProductViewModel> items, string sortColumn, string sortOrder)
        {
            switch (sortColumn)
            {
                case "Id":
                    return (sortOrder == "desc") ? items.OrderByDescending(c => c.Id)
                        : items.OrderBy(c => c.Id);

                case "Name":
                    return (sortOrder == "desc") ? items.OrderByDescending(c => c.Name) : items.OrderBy(c => c.Name);
                default:
                    return items;
            }
        }

        #endregion

        // GET: Products/Details/5
        public ActionResult Details(Guid? id)
        {
            using (var db = GetConnection)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Product pantProduct = db.Product.Find(id);
                if (pantProduct == null)
                {
                    return HttpNotFound();
                }
                return View(pantProduct);
            }
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            //using (var db = GetConnection)
            //{
            //    ViewBag.Accountings = db.Accounting.ToList().Select(p => new SelectListItem
            //    {
            //        Value = p.Id.ToString(),
            //        Text = p.AcNo + "-" + p.Name + " (" + p.TAX + "%)"
            //    });
            //}

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product pantProduct)
        {
            using (var db = GetConnection)
            {
                if (ModelState.IsValid)
                {
                    string userID = User.Identity.GetUserId();
                    pantProduct.Id = Guid.NewGuid();
                    pantProduct.Created = DateTime.Now;
                    pantProduct.Updated = DateTime.Now;
                    pantProduct.ItemType = ItemType.Pant;
                    db.Product.Add(pantProduct);
                    db.SaveChanges();

                    var category = db.Category.FirstOrDefault(a => a.Name == "Pant");
                    if(category == null)
                    {
                        int lastId = db.Category.Max(c => c.Id);

                        category = new Category();
                        category.Id = lastId + 1;
                        category.CategoryLevel = 1;
                        category.Name = "Pant";
                        category.Created = DateTime.Now;
                        category.Updated = DateTime.Now;
                        db.Category.Add(category);
                        db.SaveChanges();
                    }

                    var itemCategory = new ItemCategory();
                    itemCategory.CategoryId = category.Id;
                    itemCategory.IsPrimary = true;
                    itemCategory.ItemId = pantProduct.Id;
                    db.ItemCategory.Add(itemCategory);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                return PartialView();

            }
        }

        // GET: Products/Edit/5
        public ActionResult Edit(Guid? id)
        {
            using (var db = GetConnection)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Product pantProduct = db.Product.Find(id);
                if (pantProduct == null)
                {
                    return HttpNotFound();
                }

                //ViewBag.Accountings = db.Accounting.ToList().Select(p => new SelectListItem
                //{
                //    Value = p.Id.ToString(),
                //    Text = p.AcNo + "-" + p.Name + " (" + p.TAX + "%)"
                //});

                return PartialView(pantProduct);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product pantProduct)
        {
            using (var db = GetConnection)
            {
                if (ModelState.IsValid)
                {
                    pantProduct.Updated = DateTime.Now;
                    pantProduct.ItemType = ItemType.Pant;
                    db.Entry(pantProduct).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return PartialView(pantProduct);
            }
        }


        public ActionResult Deleteoled(Guid? id)
        {
            using (var db = GetConnection)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Product pantProduct = db.Product.Find(id);
                if (pantProduct != null)
                {
                    pantProduct.Deleted = true;
                    db.Entry(pantProduct).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
        }
        public ActionResult Restoreold(Guid? id)
        {
            using (var db = GetConnection)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Product pantProduct = db.Product.Find(id);
                if (pantProduct != null)
                {
                    pantProduct.Deleted = false;
                    db.Entry(pantProduct).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(string id)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(db))
                {
                    var productRepo = uof.ProductRepository;

                    var product = productRepo.FirstOrDefault(c => c.PantProductId == id);
                    Guid productId = Guid.Parse(id);
                    var pantPproduct = productRepo.FirstOrDefault(c => c.Id == productId);
                    if (product == null)
                    {
                        pantPproduct.Deleted = true;
                        pantPproduct.Updated = DateTime.Now;
                        db.Entry(pantPproduct).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = Resource.Deleted + " " + Resource.successfully;

                    }
                    else
                    {
                        msg = "Cannot delete this because product is associated with other Product " + product.Description;
                    }
                }

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Restore(Guid? id)
        {
            string msg = "";
            try
            {
                using (var db = GetConnection)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    Product pantProduct = db.Product.Find(id);
                    if (pantProduct != null)
                    {
                        pantProduct.Deleted = false;
                        pantProduct.Updated = DateTime.Now;
                        db.Entry(pantProduct).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = Resource.Restore + " " + Resource.successfully;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }



        protected override void Dispose(bool disposing)
        {
            using (var db = GetConnection)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }



    }
}
