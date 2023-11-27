using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Diagnostics;


namespace POSSUM.Web.Controllers
{
    public class EcommerceController : Controller
    {
       
        public ActionResult Dashboard()
        {
            return View();
        }

        //public ActionResult Products()
        //{
        //    return View();
        //}

      

        public ActionResult Categories()
        {
            return View();
        }
        public ActionResult Orders()
        {
            return View();
        }

        public ActionResult Customers()
        {
            return View();
        }

        public ActionResult StoreSettings()
        {
            return View();
        }



    }
}