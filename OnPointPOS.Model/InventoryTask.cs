using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class InventoryTask:BaseEntity
    {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual int Priority { get; set; }
        public virtual int Type { get; set; }
        public virtual int Status { get; set; }
        public virtual string StatusMessage { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Executed { get; set; }
        public virtual DateTime? Processed { get; set; }
     
        public virtual Guid? ItemId { get; set; }

        public virtual Guid? OrderGuid { get; set; }
    }
}
