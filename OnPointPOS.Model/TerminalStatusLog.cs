using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    [Serializable]
    public class TerminalStatusLog:BaseEntity
    {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual Guid TerminalId { get; set; }
        [ForeignKey("TerminalId")]
        public virtual Terminal Terminal { get; set; }
        public virtual DateTime ActivityDate { get; set; }
        public virtual string UserId { get; set; }
        public virtual Guid ReportId { get; set; }
        public virtual Terminal.TerminalStatus Status { get; set; }
        public virtual int Synced { get; set; }
        [NotMapped]
        public virtual List<ReportData> ReportData { get; set; }
        [NotMapped]
        public virtual Report Report { get; set; }
    }
}
