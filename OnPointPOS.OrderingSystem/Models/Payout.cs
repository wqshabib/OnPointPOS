using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Payout
    {
        public string Title { get; set; }
        public Guid PayoutGuid { get; set; }
        public int PayoutTypeID { get; set; }
        public DateTime PeriodStartDateTime { get; set; }
        public DateTime PeriodEndDateTime { get; set; }
    }
}