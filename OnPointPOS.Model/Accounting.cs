using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Accounting : BaseEntity
    {
        public Accounting()
        {
            IsDeleted = false;
        }
      
        public int Id { get; set; }
        public int AcNo { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public decimal TAX { get; set; }
        public DateTime Updated { get; set; }
        public int SortOrder { get; set; }
    }
}