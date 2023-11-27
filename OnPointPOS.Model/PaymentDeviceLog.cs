using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class PaymentDeviceLog : BaseEntity
    {
        public virtual long Id { get; set; }
        public virtual Guid OrderId { get; set; }
        public virtual decimal OrderTotal { get; set; }
        public virtual decimal VatAmount { get; set; }
        public virtual decimal CashBack { get; set; }
        public virtual DateTime SendDate { get; set; }
        public virtual bool Synced { get; set; }

    }
}
