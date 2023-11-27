using System;
using System.Collections.Generic;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Res;

namespace POSSUM.Presenters.Employees
{
    public class EmployeePresenter
    {
        private readonly IEmployeeView _view;

        public EmployeePresenter(IEmployeeView view)
        {
            _view = view;
        }

        public List<Employee> GetEmplooyees()
        {
           
            try
            {
                return new EmployeeRepository(PosState.GetInstance().Context).GetEmployees();
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return null;
            }
           
        }

        public Employee GetEmplooyeeBySSN(string ssn)
        {
            Employee employee = null;
            try
            {
                employee = new EmployeeRepository(PosState.GetInstance().Context).GetEmplooyeeBySsn(ssn);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return employee;
        }

        public bool SaveEmployee(Employee currentEmployee)
        {
            try
            {
                return new EmployeeRepository(PosState.GetInstance().Context).SaveEmployee(currentEmployee);
            }
            catch (Exception ex)
            {
                _view.ShowError(UI.Global_Error, ex.Message);
                return false;
            }
        }

        public bool SaveLog(Employee selectedEmployee, bool isCheckIn)
        {
            try
            {
                bool res = new EmployeeRepository(PosState.GetInstance().Context).SaveLog(selectedEmployee, isCheckIn);
                if(res==false)
                     _view.ShowError(UI.Global_Warning, "No checkin entry exists");
                return res;
            }
            catch (Exception ex)
            {
                _view.ShowError(UI.Global_Error, ex.Message);
                return false;
            }
        }
    }
}