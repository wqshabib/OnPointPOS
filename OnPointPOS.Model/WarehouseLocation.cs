using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class WarehouseLocation
    {
		[Key]
		public virtual Guid WarehouseLocationID { get; set; }
        public virtual Guid WarehouseID { get; set; }
        public virtual string Name { get; set; }
        public virtual string Path { get; set; }

    }
}
