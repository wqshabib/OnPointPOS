using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
	public class ItemInventory
	{
		[Key]
		public virtual Guid ItemInventoryID { get; set; }
        public virtual Guid ItemId { get; set; }	
        public virtual Guid WarehouseID { get; set; }
		public virtual Guid WarehouseLocationID { get; set; }
		public virtual int StockCount { get; set; }
		public virtual int StockReservations { get; set; }

	}
}
