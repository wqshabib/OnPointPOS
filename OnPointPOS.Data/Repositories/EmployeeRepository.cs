using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class EmployeeRepository : GenericRepository<Employee>, IDisposable
    {
        private readonly ApplicationDbContext context;

        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public EmployeeRepository() : base(new ApplicationDbContext())
        {
        }
        public List<EmployeeLog> PrintEmployeeLog(DateTime dtFrom, DateTime dtTo)
        {

           

                var employeeLogs =
                    context.EmployeeLog.Where(
                            el =>
                                el.CheckIn >= dtFrom && el.CheckIn <= dtTo ||
                                el.CheckOut >= dtFrom && el.CheckOut <= dtTo).ToList()
                        .Select(
                            el =>
                                new EmployeeLog
                                {
                                    EmployeeName =
                                        el.Employee.SSNO + "\n" + el.Employee.FirstName + " " + el.Employee.LastName,
                                    SSNo = el.Employee.SSNO,
                                    CheckIn = el.CheckIn,
                                    CheckOut = el.CheckOut
                                })
                        .OrderBy(o => o.CheckIn)
                        .ToList();
                return employeeLogs;
            

        }

        public List<Employee> GetEmployees()
        {
            return context.Employee.ToList();

        }

        public Employee GetEmplooyeeBySsn(string ssn)
        {
            Employee employee = null;


            employee = context.Employee.FirstOrDefault(s => s.SSNO == ssn);
            if (employee != null)
            {
                Guid id = employee.Id;
                employee.Logs =
                    context.EmployeeLog.Where(el => el.EmployeeId == id && el.Completed == false).ToList();
            }

            return employee;
        }

        public bool SaveEmployee(Employee currentEmployee)
        {


            context.Employee.Add(currentEmployee);
            context.SaveChanges();

            return true;

        }

        public bool SaveLog(Employee selectedEmployee, bool isCheckIn)
        {

            using (var db = new ApplicationDbContext())
            {

                var employeeLog = new EmployeeLog();
                if (isCheckIn)
                {
                    employeeLog.LogId = Guid.NewGuid();
                    //  employeeLog.Employee = selectedEmployee;
                    employeeLog.EmployeeId = selectedEmployee.Id;
                    employeeLog.CheckIn = DateTime.Now;
                    employeeLog.Completed = false;
                    db.EmployeeLog.Add(employeeLog);
                }
                else
                {
                    employeeLog =
                        db.EmployeeLog.FirstOrDefault(
                            el => el.EmployeeId == selectedEmployee.Id && el.Completed == false);
                    if (employeeLog == null)
                    {

                        return false;
                    }
                    employeeLog.CheckOut = DateTime.Now;
                    employeeLog.Completed = true;
                    db.Entry(employeeLog).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
            }
            return true;

        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
