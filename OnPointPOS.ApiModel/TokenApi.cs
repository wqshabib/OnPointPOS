using Newtonsoft.Json;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.ApiModel
{
    public class TokenApi
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public decimal expires_in { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int UserType { get; set; }
        public string UserTypeString { get; set; }
        public List<OutletApi> Outlets { get; set; }
        public int Status { get; set; }
    }
}