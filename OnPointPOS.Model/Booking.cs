using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class BookingCategory : BaseEntity
    {
        [Key]
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

    public class BookingSpot : BaseEntity
    {
        [Key]
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

    public class BookingArea : BaseEntity
    {
        public bool Deleted { get; set; }
        public int Parant { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        [Key]
        public int BookingAreaId { get; set; }
        public int CategoryLevel { get; set; }
        public int SortOrder { get; set; }
        public string Color { get; set; }
    }

    public class Booking : BaseEntity
    {
        public string Description { get; set; }
        public DateTime End { get; set; }
        public string EndTimezone { get; set; }
        public bool IsAllDay { get; set; }
        public string RecurrenceException { get; set; }
        public int RecurrenceID { get; set; }
        public string RecurrenceRule { get; set; }
        public string OtherDetails { get; set; }
        public DateTime Start { get; set; }
        public string StartTimezone { get; set; }
        public string Title { get; set; }
        [Key]
        public int BookingID { get; set; } 
        public int BookingAreaID { get; set; }
        public bool Deleted { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string NoOfPersons { get; set; }
        public string ProductIDs { get; set; }
        public string BookingCategoryIDs { get; set; }
        public string BookingSpotIDs { get; set; }
        public string Price { get; set; }
    }
}
