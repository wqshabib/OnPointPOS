using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Journal : BaseEntity
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ItemId { get; set; }
        public int? TableId { get; set; }
        public int ActionId { get; set; }
        public DateTime Created { get; set; }
        public string LogMessage { get; set; }
        public Guid? TerminalId { get; set; } 


        [NotMapped]
        public string Action { get; set; }

    }
}