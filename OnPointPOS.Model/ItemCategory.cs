using System;
namespace POSSUM.Model
{
    public class ItemCategory : BaseEntity
    {
        public int Id { get; set; }
        public Guid ItemId { get; set; }      
        public int CategoryId { get; set; }
        public int SortOrder { get; set; }
		public bool IsPrimary { get; set; }
    }
}