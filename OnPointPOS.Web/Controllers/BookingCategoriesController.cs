using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Data.Repositories;

namespace POSSUM.Web.Controllers
{ 
    public class BookingCategoriesController : MyBaseController
    {
        // GET: BookingCategories
        public ActionResult Index()
        {
            var lst = new BookingRepository(GetConnection).LoadBookingCategories();
            return View(lst);
        }

        // GET: BookingCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lst = new BookingRepository(GetConnection).LoadBookingCategories();
            var bookingCategory = lst.FirstOrDefault(a => a.Id == id);
            if (bookingCategory == null)
            {
                return HttpNotFound();
            }
            return View(bookingCategory);
        }

        // GET: BookingCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BookingCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookingCategoryViewModel bookingCategory)
        {
            if (ModelState.IsValid)
            {
                new BookingRepository(GetConnection).CreateBookingCategory(bookingCategory);
                return RedirectToAction("Index");
            }

            return View(bookingCategory);
        }

        // GET: BookingCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lst = new BookingRepository(GetConnection).LoadBookingCategories();
            var bookingCategory = lst.FirstOrDefault(a => a.Id == id);
            if (bookingCategory == null)
            {
                return HttpNotFound();
            }
            return View(bookingCategory);
        }

        // POST: BookingCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookingCategoryViewModel bookingCategory)
        {
            //if (ModelState.IsValid)
            {
                new BookingRepository(GetConnection).UpdateBookingCategory(bookingCategory);
                return RedirectToAction("Index");
            }
            //return View(bookingCategory);
        }

        // GET: BookingCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lst = new BookingRepository(GetConnection).LoadBookingCategories();
            var bookingCategory = lst.FirstOrDefault(a => a.Id == id);
            if (bookingCategory == null)
            {
                return HttpNotFound();
            }
            return View(bookingCategory);
        }

        // POST: BookingCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            new BookingRepository(GetConnection).RemoveBookingCategory(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }
    }
}
