using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class OutletUser : BaseEntity
    {
        [Key]
        public string Id { get; set; }
        public Guid OutletId { get; set; }
        public string Email { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool TrainingMode { get; set; }
        public bool Active { get; set; }
        public string DallasKey { get; set; }
        public DateTime? Updated { get; set; }

        public OutletUser()
        {
            Active = true;
        }
        [NotMapped]
        public string ConfirmPassword { get; set; }
    }
}
