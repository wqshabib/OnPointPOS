using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class EmployeeLogModel:EmployeeLog
    {
        public string SSNo { get; set; }
        public string EmployeeName { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }
}