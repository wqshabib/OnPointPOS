using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using POSSUM.Data.Repositories;

namespace POSSUM.Web.Models
{
    public class TaskViewModel: ISchedulerEvent
    {
        public int RoomID { get; set; }
        public IEnumerable<int> Attendees { get; set; }
        public string Description { get; set; }
        public string NoOfPersons { get; set; }
        public string Price { get; set; }
        public DateTime End { get; set; }
        public string EndTimezone { get; set; }
        public bool IsAllDay { get; set; }
        public int MeetingID { get; set; }
        public string RecurrenceException { get; set; }
        public int RecurrenceID { get; set; }
        public string RecurrenceRule { get; set; }
        public DateTime Start { get; set; }
        public string StartTimezone { get; set; }
        public string Title { get; set; }
        public bool Deleted { get; set; }
        public int BookingAreaID { get; set; }
        public string ProductIDs { get; set; }
        public string BookingCategoryIDs { get; set; }
        public string BookingSpotIDs { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public BookingViewModel ToBooking()
        {
            var result = new BookingViewModel();
            result.Description = this.Description;
            result.End = this.End;
            result.EndTimezone = this.EndTimezone;
            result.IsAllDay = this.IsAllDay;
            result.MeetingID = this.MeetingID;
            result.RecurrenceException = this.RecurrenceException;
            result.RecurrenceID = this.RecurrenceID;
            result.RecurrenceRule = this.RecurrenceRule;
            result.EventTypeID = this.RoomID;
            result.Start = this.Start;
            result.StartTimezone = this.StartTimezone;
            result.Title = this.Title;
            result.Deleted = this.Deleted;
            result.EventTypeID = this.BookingAreaID;
            result.Created = this.Created;
            result.Updated = this.Updated;
            result.ProductIDs = this.ProductIDs;
            result.Price = this.Price;
            result.NoOfPersons = this.NoOfPersons;
            result.BookingCategoryIDs = this.BookingCategoryIDs;
            result.BookingSpotIDs = this.BookingSpotIDs;
            return result;
        }

        public TaskViewModel()
        {

        }

        public TaskViewModel(BookingViewModel model)
        {
            this.Description = model.Description;
            this.End = model.End;
            this.EndTimezone = model.EndTimezone;
            this.IsAllDay = model.IsAllDay;
            this.MeetingID = model.MeetingID;
            this.RecurrenceException = model.RecurrenceException;
            this.RecurrenceID = model.RecurrenceID.Value;
            this.RecurrenceRule = model.RecurrenceRule;
            this.RoomID = model.EventTypeID.Value;
            this.Start = model.Start;
            this.StartTimezone = model.StartTimezone;
            this.Title = model.Title;
            this.Deleted = model.Deleted;
            this.BookingAreaID = model.EventTypeID.Value;
            this.Created = model.Created;
            this.Updated = model.Updated;
            this.ProductIDs = model.ProductIDs;
            this.BookingCategoryIDs = model.BookingCategoryIDs;
            this.BookingSpotIDs = model.BookingSpotIDs;
            this.Price = model.Price;
            this.NoOfPersons = model.NoOfPersons;
        }
    }
}