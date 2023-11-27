using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class Postal : Result
    {
        public string PostalId { get; set; }
        public Guid PostalGroupGuid { get; set; }
        public string Street { get; set; }
        public string StreetNumber { get; set; }
        public string StreetNumberLetter { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
    }



}