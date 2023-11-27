using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.MasterData
{
    public class OutletUser
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OutletId { get; set; }
        [ForeignKey("OutletId")]
        public virtual AdminOutlet Outlet { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual MasterApplicationUser User { get; set; }
    }
}
