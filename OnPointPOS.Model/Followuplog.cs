using System;

namespace POSSUM.Model
{
    public class FollowupLog
    {
        public virtual int Id { get; set; }
        public virtual int? ItemId { get; set; }
        public virtual int? OldQty { get; set; }
        public virtual int? NewQty { get; set; }
        public virtual int UserId { get; set; }
        public virtual Guid OrderId { get; set; }
        public virtual int? TableId { get; set; }
        public virtual DateTime? EntryDate { get; set; }
        public virtual string Action { get; set; }
        public virtual int? ItemStatus { get; set; }
        public virtual string FormName { get; set; }
        public virtual string Remarks { get; set; }

    }
}
