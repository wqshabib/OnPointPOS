using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class Tax : BaseEntity
    {
        public int Id { get; set; }
        public decimal TaxValue { get; set; }
        public int AccountingCode { get; set; }


    }
}
