using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class Warehouse
    {
		[Key]
        public virtual Guid WarehouseID { get; set; }
        public virtual string Alias { get; set; }
        public virtual string Name { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string Address3 { get; set; }
        public virtual string Zipcode { get; set; }
        public virtual string City { get; set; }
        public virtual string Country { get; set; }

    }
}
