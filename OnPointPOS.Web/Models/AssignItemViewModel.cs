using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class AssignItemViewModel
    {
        public string CategoryId { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string asc { get; set; }
    }
}