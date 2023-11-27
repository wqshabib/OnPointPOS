using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class CustomerCard
    {
        [Key]
        public Guid CardId { get; set; }
        [Required]
        [MaxLength(200)]      
        public string UniqueId { get; set; }
        public Guid  CustomerId { get; set; }
        public string Title { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        [NotMapped]
        public string Status { get { return Active ? "Aktiva" : "Inaktiv"; } }
    }
}
