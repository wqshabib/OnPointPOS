using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.OrderingSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Logs()
        {
            string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";
            var rootpath = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se";
            //string path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\logs.txt";
            string dirPath = rootpath + @"\logs\WChat";
            string path = dirPath + logfile;
            if (!string.IsNullOrEmpty(path))
            {
                string[] readText = System.IO.File.ReadAllLines(path);
                StringBuilder strbuild = new StringBuilder();
                foreach (string s in readText)
                {
                    strbuild.Append(s);
                    strbuild.Append("<br>");
                    strbuild.AppendLine();
                }
                ViewBag.Message = strbuild.ToString();
            }

            return View();
        }
        public ActionResult DeletLogs()
        {
            try
            {
                string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";
                var rootpath = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se";
                //string path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\logs.txt";
                string dirPath = rootpath + @"\logs\WChat";
                string path = dirPath + logfile;
                System.IO.File.WriteAllText(path, string.Empty);
                ViewBag.Message = "Data Deleted";
                return View();

            }
            catch (Exception)
            {
                return View();
            }
        }














    }
}