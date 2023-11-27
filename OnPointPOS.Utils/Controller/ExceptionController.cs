using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
    public class ExceptionController
    {
        string connectionString;
        public ExceptionController(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public List<ExceptionLog> GetExceptionLog()
        {
            List<ExceptionLog> preparedLogs = new List<ExceptionLog>();
            try
            {

                List<long> localLogIds = new List<long>();
               using (var db = new ApplicationDbContext(connectionString))
                {
                    
                    preparedLogs = db.ExceptionLog.Where(o => o.Synced == false).ToList();

                }


            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());

            }
            return preparedLogs;
        }
        public void UpdateExceptionStatus(long Id)
        {

           
            using (var db = new ApplicationDbContext(connectionString))
            {

                var _exception = db.ExceptionLog.FirstOrDefault(exp => exp.Id == Id);
                if (_exception != null)
                    _exception.Synced = true;

                db.SaveChanges();
            }
        }



        public List<PaymentDeviceLog> GetDeviceLog()
        {
            List<PaymentDeviceLog> preparedLogs = new List<PaymentDeviceLog>();
            try
            {

                List<long> localLogIds = new List<long>();
                using (var db = new ApplicationDbContext(connectionString))
                {

                    preparedLogs = db.PaymentDeviceLog.Where(o => o.Synced == false).ToList();

                }


            }
            catch (Exception ex)
            {
              Log.WriteLog(ex.ToString());

            }
            return preparedLogs;
        }
        public void UpdateDeviceLogStatus(long Id)
        {


            using (var db = new ApplicationDbContext(connectionString))
            {

                var _exception = db.PaymentDeviceLog.FirstOrDefault(exp => exp.Id == Id);
                if (_exception != null)
                    _exception.Synced = true;

                db.SaveChanges();
            }
        }


        public List<Model.EmployeeLog> GetEmployeeLog()
        {
            List<Model.EmployeeLog> preparedLogs = new List<Model.EmployeeLog>();
            try
            {


                using (var  db = new ApplicationDbContext(connectionString))
                {
                    var empLogs = db.EmployeeLog.Where(o => o.Synced == 0 && o.Completed == true).Take(20).ToList();
                    foreach(var empLog in empLogs)
                    {
                        var log = new EmployeeLog
                        {
                            LogId = empLog.LogId,
                            CheckIn = empLog.CheckIn,
                            CheckOut = empLog.CheckOut,
                            Completed = empLog.Completed,
                            EmployeeId = empLog.EmployeeId,
                            Synced = empLog.Synced                            
                           
                        };
                        var employee = db.Employee.FirstOrDefault(emp => emp.Id == empLog.EmployeeId);
                        if (employee != null)
                        {
                            employee.Logs = new List<EmployeeLog>();
                            log.Employee = employee;
                        }
                        preparedLogs.Add(log);
                    }
                  

                }


            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());

            }
            return preparedLogs;
        }
        public void UpdateEmployeeLogStatus(Guid Id)
        {


            using (var db = new ApplicationDbContext())
            {


                var _exception = db.EmployeeLog.FirstOrDefault(exp => exp.LogId == Id);
                if (_exception != null)
                {
                    _exception.Synced = 1;
                    db.Entry(_exception).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
            }
        }

    }
}
