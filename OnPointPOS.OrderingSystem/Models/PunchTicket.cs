using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class PunchTicketRoot : Result
    {
        public List<TagListHeader> TagListHeader { get; set; }
        public List<PunchTicket> PunchTickets { get; set; }
    }
    
    public class PunchTicket : Result
    {
        public string PunchTicketId { get; set; }
        public string ContentName { get; set; }
        public int Quantity { get; set; }
        public int Used { get; set; }
        public DateTime ExpireDateTime { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<Tag> Tags { get; set; }
        public string CustomerId { get; set; }
        public string CustomerFullName { get; set; }
        public List<PunchTicketLog> PunchTicketLogs { get; set; }
        public string Text { get; set; }
        public string OrderNo { get; set; }
    }

    public class TagListHeader : Result
    {
        public string TagListId { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class PunchTicketReport : Result
    {
        public List<TagListHeader> TagListHeader { get; set; }
        public List<PunchTicket> PunchTickets { get; set; }
        public PunchTicketdUsedFactor PunchTicketUsedFactor { get; set; }
    }

    public class PunchTicketdUsedFactor
    {
        public int Count { get; set; }
        public int Used { get; set; }
        public decimal UsedFactor { get; set; }
    }

    public class PunchTicketLog
    {
        public string Comment { get; set; }
    }



}