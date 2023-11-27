using System;
using System.ComponentModel.DataAnnotations;

namespace POSSUM.Model
{
    public class AssignedPrtinter
    {
        public int Id { get; set; }
        [MaxLength(150)]
        public string PrinterName { get; set; }
        public int PrintLocationId { get; set; }

    }
}
