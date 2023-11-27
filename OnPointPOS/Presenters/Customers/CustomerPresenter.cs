using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Utils.Controller;

namespace POSSUM.Presenters.Customers
{
    public class CustomerPresenter
    {
        private readonly ICustomerView _view;

        public CustomerPresenter(ICustomerView view)
        {
            _view = view;
        }

        public CustomerPresenter()
        {
        }



        internal void LoadCustomerClick(CustomerType customerType)
        {
            var customers = new List<Customer>();
            string keyword = _view.GetKeyword();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(() =>
            {
                customers = GetCustomers(keyword, customerType);
                progressDialog.Closed += (arg, e) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
            });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetCustomerResult(customers);
        }

        public List<Customer> GetCustomers(string keyword, CustomerType customerType)
        {
            return new CustomerRepository().SearchCustomer(keyword, customerType);
        }

        public bool SaveCustomer(Customer customer)
        {
            try
            {
                return new CustomerRepository().SaveCustomer(customer);
            }
            catch (Exception ex)
            {
                _view.ShowError(UI.Global_Error, ex.Message);
                return false;
            }
        }
        public bool UpdateCustomer(Customer customer)
        {
            try
            {
                return new CustomerRepository().UpdateCustomer(customer);

            }
            catch (Exception ex)
            {
                _view.ShowError(UI.Global_Error, ex.Message);
                return false;
            }
        }
               
        public bool HasDepositHistory(Guid id)
        {
            try
            {
                return new CustomerRepository().HasDepositHistory(id);
            }
            catch (Exception ex)
            {
                _view.ShowError(UI.Global_Error, ex.Message);
                return false;
            }
        }

        public bool IsOrderExistInDepositHistory(Guid orderId)
        {
            try
            {
                return new CustomerRepository().IsOrderExistInDepositHistory(orderId);
            }
            catch (Exception ex)
            {
                _view.ShowError(UI.Global_Error, ex.Message);
                return false;
            }
        }

        public bool AddDepositHistory(Guid customerId, decimal amount, Guid userId,Guid orderid, DepositType depositType, 
            string customerReceipt, 
            string merchantReceipt,
            decimal oldBalance,
            decimal newBalance,
            Guid terminalId)
        {
            try
            {
                return new CustomerRepository().AddDepositHistory(customerId, amount, userId, orderid, depositType, 
                    customerReceipt, 
                    merchantReceipt,
                    oldBalance,
                    newBalance,
                    terminalId);
            }
            catch (Exception ex)
            {
                _view.ShowError(UI.Global_Error, ex.Message);
                return false;
            }
        }

        public List<DepositHistoryViewModel> GetDepositHistory(Guid customerId)
        {
            return new CustomerRepository().GetDepositHistory(customerId);
        }


        //Upload to Live DB
        internal void UploadCustomer(Customer customer)
        {
            CustomerController productController = new CustomerController(Defaults.LocalConnectionString);
            productController.UploadCustomer(customer, Defaults.SyncAPIUri, Defaults.APIUSER, Defaults.APIPassword);
        }

    }
}