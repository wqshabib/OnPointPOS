using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.MasterData
{
    [Table("Terminal")]
    public class AdminTerminal
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OutletId { get; set; }
        [Required]
        [MaxLength(100)]
        [Index(IsUnique = true)]
        public string UniqueIdentification { get; set; }
        [MaxLength(200)]
        public string Customer { get; set; }
    }
}
