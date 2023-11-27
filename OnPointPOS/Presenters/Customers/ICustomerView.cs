using System.Collections.Generic;
using POSSUM.Base;
using POSSUM.Model;

namespace POSSUM.Presenters.Customers
{
    public interface ICustomerView : IBaseView
    {
        void SetCustomerResult(List<Customer> customers);
        List<Customer> GetCustomers();
        string GetKeyword();
    }
}