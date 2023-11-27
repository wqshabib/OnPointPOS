using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration
{
    public class TillUser
    {
        public virtual string Id { get; set; }
        public virtual Guid OutletId { get; set; }
        public virtual string UserCode { get; set; }
        public virtual string Email { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual bool TrainingMode { get; set; }
        public virtual bool Active { get; set; }
        public virtual string DallasKey { get; set; }
        public virtual DateTime? Updated { get; set; }


        public TillUser()
        {
            Active = true;
        }

    }
}
