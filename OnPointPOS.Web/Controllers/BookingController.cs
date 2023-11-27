using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using POSSUM.Data;
using POSSUM.Data.Repositories;
using POSSUM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    public class MeetingViewModel : ISchedulerEvent
    {
        public override string ToString()
        {
            return Environment.NewLine + "Description: " + Description +
                Environment.NewLine + "NoOfPersons: " + NoOfPerson +
                Environment.NewLine + "Price: " + Price +
                Environment.NewLine + "BookingCategoryIDs: " + (BookingCategory == null ? "" : string.Join(",", BookingCategory)) +
                Environment.NewLine + "BookingSpotIDs: " + (BookingSpots == null ? "" : string.Join(",", BookingSpots)) +
                Environment.NewLine + "ProductIDs: " + (Products == null ? "" : string.Join(",", Products)) +
                Environment.NewLine + "Updated: " + Updated +
                Environment.NewLine + "Created: " + Created +
                Environment.NewLine + "EventTypeID: " + EventTypeID +
                Environment.NewLine + "Deleted: " + Deleted +
                Environment.NewLine + "Title: " + Title +
                Environment.NewLine + "StartTimezone: " + StartTimezone +
                Environment.NewLine + "Start: " + Start +
                Environment.NewLine + "RecurrenceRule: " + RecurrenceRule +
                Environment.NewLine + "RecurrenceException: " + RecurrenceException +
                Environment.NewLine + "MeetingID: " + MeetingID +
                Environment.NewLine + "IsAllDay: " + IsAllDay +
                Environment.NewLine + "EndTimezone: " + EndTimezone +
                Environment.NewLine + "End: " + End +
                Environment.NewLine + "Description: " + Description;
        }

        public MeetingViewModel(BookingViewModel model)
        {
            this.Description = model.Description;
            this.End = model.End;
            this.EndTimezone = model.EndTimezone;
            this.IsAllDay = model.IsAllDay;
            this.MeetingID = model.MeetingID;
            this.RecurrenceException = model.RecurrenceException == null ? "" : model.RecurrenceException;
            this.RecurrenceID = model.RecurrenceID;
            this.RecurrenceRule = model.RecurrenceRule;
            this.EventTypeID = model.EventTypeID;
            this.Start = model.Start;
            this.StartTimezone = model.StartTimezone;
            this.Title = model.Title;
            this.Deleted = model.Deleted;
            this.Created = model.Created;
            this.Updated = model.Updated;
            this.Products = string.IsNullOrEmpty(model.ProductIDs) ? new List<int>() : model.ProductIDs.Split(',').Select(a => Convert.ToInt32(a)).ToList();
            this.BookingCategory = string.IsNullOrEmpty(model.BookingCategoryIDs) ? new List<int>() : model.BookingCategoryIDs.Split(',').Select(a => Convert.ToInt32(a)).ToList();
            this.BookingSpots = string.IsNullOrEmpty(model.BookingSpotIDs) ? new List<int>() : model.BookingSpotIDs.Split(',').Select(a => Convert.ToInt32(a)).ToList();
            this.Price = model.Price == null ? "" : model.Price;
            this.NoOfPerson = model.NoOfPersons == null ? "" : model.NoOfPersons;
        }

        public MeetingViewModel()
        {
        }

        public BookingViewModel ToBooking()
        {
            var result = new BookingViewModel()
            {
                Description = this.Description,
                End = this.End,
                EndTimezone = this.EndTimezone,
                IsAllDay = this.IsAllDay,
                MeetingID = this.MeetingID,
                RecurrenceException = this.RecurrenceException,
                RecurrenceID = this.RecurrenceID,
                RecurrenceRule = this.RecurrenceRule,
                Start = this.Start,
                StartTimezone = this.StartTimezone,
                Title = this.Title,
                Deleted = this.Deleted,
                EventTypeID = this.EventTypeID,
                Created = this.Created,
                Updated = this.Updated,
                ProductIDs = this.Products == null ? "" : string.Join(",", this.Products),
                BookingSpotIDs = this.BookingSpots == null ? "" : string.Join(",", this.BookingSpots),
                BookingCategoryIDs = this.BookingCategory == null ? "" : string.Join(",", this.BookingCategory),
                Price = this.Price,
                NoOfPersons = this.NoOfPerson
            };
            return result;
        }

        public int MeetingID { get; set; }

        [Required]
        public string Title { get; set; }

        [Display(Name = "No Of Persons")]
        public string NoOfPerson { get; set; }

        public string Price { get; set; }

        public string Description { get; set; }

        private DateTime start;
        [Required]
        public DateTime Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value.ToUniversalTime();
            }
        }

        public string StartTimezone { get; set; }

        private DateTime end;

        [Required]
        [DateGreaterThan(OtherField = "Start")]
        public DateTime End
        {
            get
            {
                return end;
            }
            set
            {
                end = value.ToUniversalTime();
            }
        }

        public string EndTimezone { get; set; }

        [Display(Name = "Recurrence Rule")]
        public string RecurrenceRule { get; set; }
        public int? RecurrenceID { get; set; }
        public string RecurrenceException { get; set; }
        [Display(Name = "Is All Day")]
        public bool IsAllDay { get; set; }
        public string Timezone { get; set; }
        [Display(Name = "Event Type")]
        public int? EventTypeID { get; set; }
        [Display(Name = "Products")]
        public IEnumerable<int> Products { get; set; }
        [Display(Name = "Booking Category")]
        public IEnumerable<int> BookingCategory { get; set; }
        [Display(Name = "Booking Spots")]
        public IEnumerable<int> BookingSpots { get; set; }
        public bool Deleted { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

    [Authorize]
    public class BookingController : MyBaseController
    {
        public BookingController()
        {

        }
        
        public ActionResult Index()
        {
            try
            {
                var bookingRepository = new BookingRepository(GetConnection);
                ViewBag.Message = "Bookings!";
                var lst = bookingRepository.LoadBookingAreas();
                ViewBag.BookingAreas = lst.Where(a => a.CategoryLevel == 1).Select(a => new { Text = a.Name, Value = a.BookingAreaId, Color = @"\\" + a.Color }).ToList();
                ViewBag.Products = lst.Where(a => a.CategoryLevel == 2).Select(a => new { Text = a.Name, Value = a.BookingAreaId, Color = @"\\" + a.Color }).ToList();
                ViewBag.BookingCategory = bookingRepository.LoadBookingCategories().Select(a => new { Text = a.Name, Value = a.Id, Color = @"\\" + a.Color }).ToList();
                ViewBag.BookingSpots = bookingRepository.LoadBookingSpots().Select(a => new { Text = a.Name, Value = a.Id, Color = @"\\" + a.Color }).ToList();
                return View(new MeetingViewModel());
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return View(new MeetingViewModel());
            }
        }

        public ActionResult About()
        {
            return View();
        }

        public virtual JsonResult Meetings_Read([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                var lst = new BookingRepository(GetConnection).LoadBookings().Select(a => new MeetingViewModel(a)).AsQueryable();
                return Json(lst.ToDataSourceResult(request));
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return Json(null);
            }
        }

        public virtual JsonResult Meetings_Destroy([DataSourceRequest] DataSourceRequest request, IEnumerable<MeetingViewModel> models)
        {
            try
            {
                if (models != null && models.Count() > 0)
                {
                    foreach (var item in models)
                    {
                        DataLogger.SendBookingEmail("Meetings_Destroy", item.ToString());
                    }
                }

                if (ModelState.IsValid)
                {
                    //var list = models.ToList();

                    //for (int i = 0; i < list.Count; i++)
                    //{
                    //    var meeting = list[i];

                    //    if (meeting.RecurrenceID != null)
                    //    {
                    //        for (int j = 0; j < list.Count; j++)
                    //        {
                    //            var potentialParent = list[j];

                    //            if (meeting.RecurrenceID == potentialParent.MeetingID)
                    //            {
                    //                models = models.Where(m => m.MeetingID != potentialParent.MeetingID);
                    //            }
                    //        }
                    //    }
                    //}

                    if (models.Count() > 0)
                    {
                        var repository = new BookingRepository(GetConnection);

                        foreach (var meeting in models)
                        {
                            repository.Remove(meeting.MeetingID);
                        }
                    }
                }

                return Json(models.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return Json(null);
            }
        }

        public virtual JsonResult Meetings_Create([DataSourceRequest] DataSourceRequest request, IEnumerable<MeetingViewModel> models)
        {
            try
            {
                if (models != null && models.Count() > 0)
                {
                    foreach (var item in models)
                    {
                        DataLogger.SendBookingEmail("Meetings_Create", item.ToString());
                    }
                }

                if (ModelState.IsValid)
                {
                    var repository = new BookingRepository(GetConnection);
                    foreach (var meeting in models)
                    {
                        var booking = meeting.ToBooking();
                        repository.Create(booking);
                    }
                }

                return Json(models.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return Json(null);
            }
        }

        public virtual JsonResult Meetings_Update([DataSourceRequest] DataSourceRequest request, IEnumerable<MeetingViewModel> models)
        {
            try
            {
                if (models != null && models.Count() > 0)
                {
                    foreach (var item in models)
                    {
                        DataLogger.SendBookingEmail("Meetings_Update", item.ToString());
                    }
                }

                if (ModelState.IsValid)
                {
                    var repository = new BookingRepository(GetConnection);
                    foreach (var meeting in models)
                    {
                        var booking = meeting.ToBooking();
                        repository.Update(booking);
                    }
                }

                return Json(models.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return Json(null);
            }
        }
    }
}