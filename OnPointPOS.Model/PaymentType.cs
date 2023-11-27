using System;

namespace POSSUM.Model
{
    public class PaymentType:BaseEntity
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string SwedishName { get; set; }
        public int AccountingCode { get; set; }
        public DateTime Updated { get; set; }
    }
}
