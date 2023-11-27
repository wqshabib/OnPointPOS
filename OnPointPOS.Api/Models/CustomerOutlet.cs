using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Api.Models
{
    public class CustomerOutlet
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<CustomerTerminal> Terminals { get; set; }
    }
    public class CustomerTerminal
    {
        public Guid Id { get; set; }
        public Guid OutletId { get; set; }
        public string Name { get; set; }
    }
}