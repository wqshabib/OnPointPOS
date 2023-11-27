using System;
using System.Collections.Generic;
using System.Windows;
using POSSUM.Model;
using POSSUM.Presenters.Customers;
using POSSUM.Res;
using POSSUM.Events;
using System.Threading;

namespace POSSUM.Views.Customers
{
    /// <summary>
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class AddCustomerWindow : ICustomerView
    {
        private readonly CustomerPresenter _presenter;
        public Customer CurrentCustomer;
        public event UploadCustomerEventHandler UploadCustomer;
        public AddCustomerWindow(bool isInvoiceBuy, string title)
        {
            InitializeComponent();
            _presenter = new CustomerPresenter(this);
            CurrentCustomer = new Customer();
            UploadCustomer += AddCustomerWindow_UploadCustomer;
            layoutGrid.DataContext = CurrentCustomer;
            lblTitle.Text = title;
            if (isInvoiceBuy)
                CodeVisibility();
        }

        public AddCustomerWindow(Customer customer, bool isInvoiceBuy, string title)
        {
            InitializeComponent();
            _presenter = new CustomerPresenter(this);
            CurrentCustomer = customer;
            UploadCustomer += AddCustomerWindow_UploadCustomer;
            layoutGrid.DataContext = CurrentCustomer;
            if (CurrentCustomer.FloorNo > 0)
                txtFloorNo.Text = CurrentCustomer.FloorNo.ToString();
            if (CurrentCustomer.PortCode > 0)
                txtPortCode.Text = CurrentCustomer.PortCode.ToString();
            if (!string.IsNullOrEmpty(CurrentCustomer.CustomerNo))
                txtCustomerNo.Text = CurrentCustomer.CustomerNo.ToString();
            lblTitle.Text = title;
            if (isInvoiceBuy)
                CodeVisibility();
            lblCurrentBalance.Text = CurrentCustomer.DepositAmount.ToString();

            var hasDepositHistory = _presenter.HasDepositHistory(CurrentCustomer.Id);
            if (hasDepositHistory || CurrentCustomer.DepositAmount > 0)
            {
                chkHasDeposit.IsChecked = true;
                chkHasDeposit.IsEnabled = false;
            }
        }

        private void CodeVisibility()
        {
            txtFloorNo.Visibility = Visibility.Collapsed;
            txtReference.Visibility = Visibility.Collapsed;
            txtPortCode.Visibility = Visibility.Collapsed;

            lblFloorNo.Visibility = Visibility.Collapsed;
            lblRefNo.Visibility = Visibility.Collapsed;
            lblPortCode.Visibility = Visibility.Collapsed;
        }
        private void AddCustomerWindow_UploadCustomer(object sender, Customer customer)
        {
            _presenter.UploadCustomer(customer);
        }
        public void SetCustomerResult(List<Customer> customers)
        {

        }

        public void SetFloors(List<Floor> floors)
        {

        }

        public string GetKeyword()
        {
            return "";
        }

        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentCustomer.Name))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                
                int flooNo = 0;
                int portCode = 0;
                string customerNo = txtCustomerNo.Text;
                int.TryParse(txtFloorNo.Text, out flooNo);
                int.TryParse(txtPortCode.Text, out portCode);
                //int.TryParse(txtCustomerNo.Text, out customerNo);
                CurrentCustomer.FloorNo = flooNo;
                CurrentCustomer.PortCode = portCode;
                CurrentCustomer.CustomerNo = customerNo;
                if (string.IsNullOrEmpty(CurrentCustomer.CustomerNo))
                {
                    MessageBox.Show("Customer no is missing", UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (CurrentCustomer.Id == null || CurrentCustomer.Id == default(Guid))
                {
                    CurrentCustomer.Id = Guid.NewGuid();
                    CurrentCustomer.Updated = DateTime.Now;
                    bool res = _presenter.SaveCustomer(CurrentCustomer);
                    if (res)
                    {// this.DialogResult = true;
                        CustomerUpload(CurrentCustomer);
                    }
                }
                else
                {
                    CurrentCustomer.Updated = DateTime.Now;
                    bool res = _presenter.UpdateCustomer(CurrentCustomer);
                    if (res)
                    {//  this.DialogResult = true;
                        CustomerUpload(CurrentCustomer);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        private void CustomerUpload(Customer customer)
        {
            if (customer.Id != default(Guid))
            {
                var progressDialog = new ProgressWindow();
                var backgroundThread = new Thread(
                    new ThreadStart(() =>
                    {
                        if (UploadCustomer != null)
                        {
                            UploadCustomer(this, customer);
                        }
                        progressDialog.Closed += (arg, ev) =>
                        {
                            progressDialog = null;
                        };
                        progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressDialog.Close();
                        }));
                    }));
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();
                this.DialogResult = true;
            }
        }
        private void BtnCloseAccountInfo_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public List<Customer> GetCustomers()
        {
            return new List<Customer>();
        }
    }
}