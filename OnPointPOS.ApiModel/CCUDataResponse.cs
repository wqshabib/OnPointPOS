using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class CCUDataResponse
    {
        public string apiKey { get; set; }
        public string productionNumber { get; set; }
        public long activationId { get; set; }
        public string address { get; set; }
        public string zip { get; set; }
        public string city { get; set; }
        public string phone { get; set; }
        public string companyName { get; set; }
    }
}
