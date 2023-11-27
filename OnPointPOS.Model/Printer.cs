using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Printer : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string LocationName { get; set; }
        public string PrinterName { get; set; }
        public Guid? TerminalId { get; set; }
        public string IPAddress { get; set; }
        public DateTime? Updated { get; set; }
    }
}
