using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    // [Authorize(Roles = "Admin")]
    [Authorize]
    public class EmployeeLogController : MyBaseController
    {


        public EmployeeLogController()
        {

        }
        // GET: EmployeeLog
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogList(string employeeId, string dtFrom, string dtTo)
        {
            ViewBag.DTFrom = dtFrom;
            ViewBag.DTTo = dtTo;
            Guid empGuid = Guid.Parse(employeeId);
            DateTime dateFrom = Convert.ToDateTime(dtFrom).Date;
            DateTime dateTo = Convert.ToDateTime(dtTo + "  11:59:00 PM");
            List<EmployeeLogModel> employeeLogs = new List<EmployeeLogModel>();
            try
            {
                //::TODO need to create map for store procedure "GenerateReportByTerminal" in nhibernate
                using (var db = GetConnection)
                {

                    if (empGuid == default(Guid))
                    {
                        var logs = db.EmployeeLog.Include("Employee").Where(el => ((el.CheckIn >= dateFrom && el.CheckIn <= dateTo) || (el.CheckOut >= dateFrom && el.CheckOut <= dateTo))).ToList();
                        employeeLogs = logs.Select(el => new EmployeeLogModel
                        {
                            EmployeeName = el.Employee.SSNO + "\n" + el.Employee.FirstName + " " + el.Employee.LastName,
                            SSNo = el.Employee.SSNO,
                            CheckIn = el.CheckIn,
                            CheckOut = el.CheckOut,
                            Hours = GetHours(el.CheckIn, el.CheckOut),
                            Minutes = GetMinuts(el.CheckIn, el.CheckOut)
                        }).OrderBy(o => o.CheckIn).ToList();
                    }
                    else
                    {
                        var logs = db.EmployeeLog.Include("Employee").Where(el => ((el.CheckIn >= dateFrom && el.CheckIn <= dateTo) || (el.CheckOut >= dateFrom && el.CheckOut <= dateTo)) && el.EmployeeId == empGuid).ToList();
                        employeeLogs = logs.Select(el => new EmployeeLogModel
                        {
                            EmployeeName = el.Employee.SSNO + "\n" + el.Employee.FirstName + " " + el.Employee.LastName,
                            SSNo = el.Employee.SSNO,
                            CheckIn = el.CheckIn,
                            CheckOut = el.CheckOut,
                            Hours = GetHours(el.CheckIn, el.CheckOut),
                            Minutes = GetMinuts(el.CheckIn, el.CheckOut)
                        }).OrderBy(o => o.CheckIn).ToList();
                        if (employeeLogs.Count > 0)
                        {
                            ViewBag.EmployeeName = employeeLogs.First().EmployeeName;
                            int hours = employeeLogs.Sum(h => h.Hours);
                            int minuts = employeeLogs.Sum(m => m.Minutes);

                            if (minuts > 59)
                            {
                                int minutsHours = minuts / 60;
                                hours = hours + minutsHours;
                                minuts = minuts % 60;
                            }

                            ViewBag.TotalHours = hours;
                            ViewBag.TotalMinutes = minuts;
                        }
                        else
                        {
                            ViewBag.EmployeeName = " ";
                            ViewBag.TotalHours = 0;
                            ViewBag.TotalMinutes = 0;
                        }
                    }
                    return PartialView("EmpLogList", employeeLogs);
                }


            }
            catch (Exception ex)
            {


                employeeLogs = new List<EmployeeLogModel>();

            }

            return PartialView(employeeLogs);
        }
        private int GetHours(DateTime? dtCheckIn, DateTime? dtCheckout)
        {
            if (dtCheckIn == null || dtCheckout == null)
                return 0;

            TimeSpan ts = Convert.ToDateTime(dtCheckout) - Convert.ToDateTime(dtCheckIn);

            return ts.Hours;
        }
        private int GetMinuts(DateTime? dtCheckIn, DateTime? dtCheckout)
        {
            if (dtCheckIn == null || dtCheckout == null)
                return 0;
            TimeSpan ts = Convert.ToDateTime(dtCheckout) - Convert.ToDateTime(dtCheckIn);
            return ts.Minutes;
        }
        public JsonResult FillEmployees()
        {
            var employees = new List<Employee>();
            using (var db = GetConnection)
            {
                employees = db.Employee.ToList();
                employees.Add(new Employee { Id = default(Guid), Name = "All" });
            }
            return Json(employees.OrderBy(o => o.Id).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}