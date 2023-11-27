using System;

namespace POSSUM.Model
{
    public class UserLog : BaseEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? LogOutTime { get; set; }
        public DateTime? LogDate { get; set; }
        public int IsLogedOut { get; set; }
    }
}