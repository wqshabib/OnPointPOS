using System;
using System.ComponentModel.DataAnnotations;

namespace POSSUM.Model
{
    public class ReportData : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public virtual Guid ReportId { get; set; }
        public string DataType { get; set; }
        public string TextValue { get; set; }
        public int? ForeignId { get; set; }
        public decimal? Value { get; set; }
        public decimal? TaxPercent { get; set; }
        public DateTime? DateValue { get; set; }
        public int SortOrder { get; set; }
    }
}