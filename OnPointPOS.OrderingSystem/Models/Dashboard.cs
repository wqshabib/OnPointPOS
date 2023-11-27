using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Models
{
    //[Serializable]
    public class Dashboard : System.Dynamic.DynamicObject // : Result
    {
        public string DashboardName { get; set; }
    }




}