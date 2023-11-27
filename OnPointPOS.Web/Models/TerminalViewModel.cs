using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class TerminalViewModel:Terminal
    {
        public IEnumerable<SelectListItem> Outlets { get; set; }
        public string OutletName { get; set; }
        public virtual int RootCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string TerminalId { get; set; }
        public Guid OutletId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; } 
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> TerminalTypes { get; set; }
        
    }
    public enum TerminalStatus
    {
        Select=0,
        Active=1,
        InActive=2
    }
    public class TerminalData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public DateTime dtFrom { get; set; }
        public DateTime dtTo { get; set; }
    }
}