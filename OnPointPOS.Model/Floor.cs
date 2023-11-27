using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class Floor:BaseEntity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }       
        public virtual IList<FoodTable> Tables { get; set; }
    }
}
