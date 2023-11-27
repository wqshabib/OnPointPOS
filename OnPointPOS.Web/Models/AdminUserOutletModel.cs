using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class AdminUserOutletModel
    {

        public string UserId { get; set; }
        public Guid OutletId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

    }
}