using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class MQTTBuffer
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? OrderId { get; set; }
        public string Action { get; set; }
        public string JsonData { get; set; }
        public DateTime Created { get; set; }
        public bool Status { get; set; }

    }
}
