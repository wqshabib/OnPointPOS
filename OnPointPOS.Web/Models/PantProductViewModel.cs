using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class PantProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool Deleted { get; set; }
        public bool Status { get; set; } 
    }
}