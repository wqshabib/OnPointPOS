using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class FoodTable : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public int FloorId { get; set; }
        [ForeignKey("FloorId")]
        public virtual Floor Floor { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Chairs { get; set; }
        public TableStatus Status { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Updated { get; set; }
        [NotMapped]
        public string ColorCode { get; set; }
        [NotMapped]
        public int OrderCount { get; set; }
        [NotMapped]
        public string CustomerName { get; set; }

    }
    public enum TableStatus
    {
        Available = 0,
        Partial = 1,
        Reserved = 2
    }
}
