using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data.Repositories
{
    public class BookingRepository
    {
        private readonly ApplicationDbContext context;

        public BookingRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<BookingAreaViewModel> LoadBookingAreas()
        {
            try
            {
                var lst = context.BookingArea.Where(r => r.BookingAreaId > 0 && r.Deleted == false).Select(c => new BookingAreaViewModel
                {
                    BookingAreaId = c.BookingAreaId,
                    Parant = (int)c.Parant,
                    Name = c.Name,
                    CategoryLevel = c.CategoryLevel,
                    SortOrder = c.SortOrder,
                    Color = c.Color
                }).ToList();

                return lst;
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return new List<BookingAreaViewModel>();
            }
        }

        public List<BookingCategoryViewModel> LoadBookingCategories()
        {
            try
            {
                var lst = context.BookingCategory.Where(r => r.IsDeleted == false).Select(c => new BookingCategoryViewModel
                {
                    Name = c.Name,
                    Created = c.Created,
                    IsDeleted = c.IsDeleted,
                    Color = c.Color,
                    CreatedBy = c.CreatedBy,
                    Details = c.Details,
                    Id = c.Id,
                    Updated = c.Updated,
                    UpdatedBy = c.UpdatedBy
                }).ToList();

                return lst;
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return new List<BookingCategoryViewModel>();
            }
        }

        public void RemoveBookingCategory(int id)
        {
            try
            {
                var target = context.BookingCategory.FirstOrDefault(e => e.Id == id);

                if (target != null)
                {
                    context.BookingCategory.Remove(target);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public void CreateBookingCategory(BookingCategoryViewModel model)
        {
            try
            {
                model.Created = DateTime.Now;
                model.Updated = DateTime.Now;

                var newObj = new BookingCategory()
                {
                    Name = model.Name,
                    Created = model.Created,
                    IsDeleted = model.IsDeleted,
                    Color = "#f8a3ff",
                    CreatedBy = model.CreatedBy,
                    Details = model.Details,
                    Updated = model.Updated,
                    UpdatedBy = model.UpdatedBy
                };

                context.BookingCategory.Add(newObj);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public void UpdateBookingCategory(BookingCategoryViewModel model)
        {
            try
            {
                var target = context.BookingCategory.FirstOrDefault(e => e.Id == model.Id);

                if (target != null)
                {
                    target.Name = model.Name;
                    target.Created = model.Created;
                    target.IsDeleted = model.IsDeleted;
                    target.Color = "#f8a3ff";
                    target.CreatedBy = model.CreatedBy;
                    target.Details = model.Details;
                    target.Updated = DateTime.Now;
                    target.UpdatedBy = model.UpdatedBy;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public List<BookingSpotViewModel> LoadBookingSpots()
        {
            try
            {
                var lst = context.BookingSpot.Where(r => r.IsDeleted == false).Select(c => new BookingSpotViewModel
                {
                    Name = c.Name,
                    Created = c.Created,
                    IsDeleted = c.IsDeleted,
                    Color = c.Color,
                    CreatedBy = c.CreatedBy,
                    Details = c.Details,
                    Id = c.Id,
                    Updated = c.Updated,
                    UpdatedBy = c.UpdatedBy
                }).ToList();

                return lst;
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return new List<BookingSpotViewModel>();
            }
        }

        public void RemoveBookingSpot(int id)
        {
            try
            {
                var target = context.BookingSpot.FirstOrDefault(e => e.Id == id);

                if (target != null)
                {
                    context.BookingSpot.Remove(target);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public void CreateBookingSpot(BookingSpotViewModel model)
        {
            try
            {
                model.Created = DateTime.Now;
                model.Updated = DateTime.Now;

                var newObj = new BookingSpot()
                {
                    Name = model.Name,
                    Created = model.Created,
                    IsDeleted = model.IsDeleted,
                    Color = "#f8a3ff",
                    CreatedBy = model.CreatedBy,
                    Details = model.Details,
                    Updated = model.Updated,
                    UpdatedBy = model.UpdatedBy
                };

                context.BookingSpot.Add(newObj);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public void UpdateBookingSpot(BookingSpotViewModel model)
        {
            try
            {
                var target = context.BookingSpot.FirstOrDefault(e => e.Id == model.Id);

                if (target != null)
                {
                    target.Name = model.Name;
                    target.Created = model.Created;
                    target.IsDeleted = model.IsDeleted;
                    target.Color = "#f8a3ff";
                    target.CreatedBy = model.CreatedBy;
                    target.Details = model.Details;
                    target.Updated = DateTime.Now;
                    target.UpdatedBy = model.UpdatedBy;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public void Remove(int bookingId)
        {
            try
            {
                var target = context.Booking.FirstOrDefault(e => e.BookingID == bookingId);

                if (target != null)
                {
                    target.Deleted = true;
                    target.Updated = DateTime.Now;
                    context.SaveChanges();
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public void Create(BookingViewModel model)
        {
            try
            {
                var booking = new Booking();
                booking.Title = model.Title;
                booking.Start = model.Start;
                booking.End = model.End;
                booking.StartTimezone = model.StartTimezone;
                booking.EndTimezone = model.EndTimezone;
                booking.Description = model.Description;
                booking.IsAllDay = model.IsAllDay;
                booking.RecurrenceRule = model.RecurrenceRule;
                booking.RecurrenceException = model.RecurrenceException;
                booking.RecurrenceID = model.RecurrenceID == null ? -1 : model.RecurrenceID.Value;
                booking.BookingAreaID = model.EventTypeID == null ? 0 : model.EventTypeID.Value;
                booking.Created = DateTime.Now;
                booking.Deleted = false;
                booking.NoOfPersons = model.NoOfPersons;
                booking.Updated = DateTime.Now;
                booking.Price = model.Price;
                booking.ProductIDs = model.ProductIDs;
                booking.BookingCategoryIDs = model.BookingCategoryIDs;
                booking.BookingSpotIDs = model.BookingSpotIDs;
                context.Booking.Add(booking);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public void Update(BookingViewModel model)
        {
            try
            {
                var target = context.Booking.FirstOrDefault(e => e.BookingID == model.MeetingID);

                if (target != null)
                {
                    target.Title = model.Title;
                    target.Start = model.Start;
                    target.End = model.End;
                    target.StartTimezone = model.StartTimezone;
                    target.EndTimezone = model.EndTimezone;
                    target.Description = model.Description;
                    target.IsAllDay = model.IsAllDay;
                    target.RecurrenceRule = model.RecurrenceRule;
                    target.RecurrenceException = model.RecurrenceException;
                    target.RecurrenceID = model.RecurrenceID.Value;
                    target.BookingAreaID = model.EventTypeID.Value;
                    target.Updated = DateTime.Now;
                    target.Price = model.Price;
                    target.NoOfPersons = model.NoOfPersons;
                    target.ProductIDs = model.ProductIDs;
                    target.BookingSpotIDs = model.BookingSpotIDs;
                    target.BookingCategoryIDs = model.BookingCategoryIDs;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
            }
        }

        public List<BookingViewModel> LoadBookings()
        {
            try
            {
                var result = context.Booking.Where(a=>a.Deleted == false).Select(task => new BookingViewModel
                {
                    MeetingID = task.BookingID,
                    Title = task.Title,
                    Start = task.Start,
                    End = task.End,
                    StartTimezone = task.StartTimezone,
                    EndTimezone = task.EndTimezone,
                    Description = task.Description,
                    IsAllDay = task.IsAllDay,
                    RecurrenceRule = task.RecurrenceRule,
                    RecurrenceException = task.RecurrenceException,
                    RecurrenceID = task.RecurrenceID,
                    EventTypeID = task.BookingAreaID,
                    Deleted = task.Deleted,
                    Created = task.Created,
                    Updated = task.Updated,
                    NoOfPersons = task.NoOfPersons,
                    Price = task.Price,
                    ProductIDs = task.ProductIDs,
                    BookingCategoryIDs = task.BookingCategoryIDs,
                    BookingSpotIDs = task.BookingSpotIDs,
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                DataLogger.SendBookingEmail("Exception", ex.ToLog());
                return new List<BookingViewModel>();
            }
        }
    }

    public class BookingCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string Color { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class BookingSpotViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string Color { get; set; }
        public bool IsDeleted { get; set; }
    }


    public class BookingAreaViewModel
    {
        public Guid ItemId { get; set; }
        public int BookingAreaId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public bool IsPrimary { get; set; }

        public bool Deleted { get; set; }
        public int Parant { get; set; }
        public int CategoryLevel { get; set; }

        public int SortOrder { get; set; }
        public string Color { get; set; }
    }

    public class BookingViewModel
    {
        public string Description { get; set; }
        public DateTime End { get; set; }
        public string EndTimezone { get; set; }
        public bool IsAllDay { get; set; }
        public int MeetingID { get; set; }
        public string RecurrenceException { get; set; }
        public int? RecurrenceID { get; set; }
        public string RecurrenceRule { get; set; }
        public int? EventTypeID { get; set; }
        public DateTime Start { get; set; }
        public string StartTimezone { get; set; }
        public string Title { get; set; }
        public bool Deleted { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string ProductIDs { get; set; }
        public string BookingCategoryIDs { get; set; }
        public string BookingSpotIDs { get; set; }
        public string Price { get; set; }
        public string NoOfPersons { get; set; }
    }
}
