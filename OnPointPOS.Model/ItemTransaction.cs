using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
	public class ItemTransaction:BaseEntity
	{
		[Key]
		public Guid ItemTransactionID { get; set; }
		public Guid ItemID { get; set; }
		public Guid OutletID { get; set; }
		public Guid TerminalID { get; set; }
		public Guid OrderID { get; set; }
		public Guid WarehouseID { get; set; }
		public decimal Qty { get; set; }
		public int Direction { get; set; }
		public DateTime TransactionDate { get; set; }

	}
}
