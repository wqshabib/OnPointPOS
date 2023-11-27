using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class ZReportSetting:BaseEntity
    {
        public virtual int Id { get; set; }
        public virtual string ReportTag { get; set; }
        public virtual bool Visiblity { get; set; }
        public virtual DateTime Updated { get; set; }
    }
   
}
