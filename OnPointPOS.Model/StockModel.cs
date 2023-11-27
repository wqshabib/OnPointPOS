using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class StockModel
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public string WarehouseName { get; set; }
        public string LocationName { get; set; }
        public int StockCount { get; set; }
        public int StockReservation { get; set; }
    }
}
