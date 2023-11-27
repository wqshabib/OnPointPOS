using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class Customer : Result
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }

        public CustomerStatus CustomerStatus { get; set; }
        public List<PunchTicketLog> PunchTicketLogs { get; set; } 
        //Cont...
    }

    public enum CustomerStatus
    {
        Added = 1
        , Updated = 2

    }




}