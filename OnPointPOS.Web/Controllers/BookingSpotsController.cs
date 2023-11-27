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
    public class BookingSpotsController : MyBaseController
    {
        // GET: BookingSpots
        public ActionResult Index()
        {
            var lst = new BookingRepository(GetConnection).LoadBookingSpots();
            return View(lst);
        }

        // GET: BookingSpots/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lst = new BookingRepository(GetConnection).LoadBookingSpots();
            var bookingSpot = lst.FirstOrDefault(a => a.Id == id);
            if (bookingSpot == null)
            {
                return HttpNotFound();
            }
            return View(bookingSpot);
        }

        // GET: BookingSpots/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BookingSpots/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookingSpotViewModel bookingSpot)
        {
            if (ModelState.IsValid)
            {
                new BookingRepository(GetConnection).CreateBookingSpot(bookingSpot);
                return RedirectToAction("Index");
            }

            return View(bookingSpot);
        }

        // GET: BookingSpots/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lst = new BookingRepository(GetConnection).LoadBookingSpots();
            var bookingSpot = lst.FirstOrDefault(a => a.Id == id);
            if (bookingSpot == null)
            {
                return HttpNotFound();
            }
            return View(bookingSpot);
        }

        // POST: BookingSpots/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookingSpotViewModel bookingSpot)
        {
            //if (ModelState.IsValid)
            {
                new BookingRepository(GetConnection).UpdateBookingSpot(bookingSpot);
                return RedirectToAction("Index");
            }
            //return View(bookingSpot);
        }

        // GET: BookingSpots/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var lst = new BookingRepository(GetConnection).LoadBookingSpots();
            var bookingSpot = lst.FirstOrDefault(a => a.Id == id);
            if (bookingSpot == null)
            {
                return HttpNotFound();
            }
            return View(bookingSpot);
        }

        // POST: BookingSpots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            new BookingRepository(GetConnection).RemoveBookingSpot(id);
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
