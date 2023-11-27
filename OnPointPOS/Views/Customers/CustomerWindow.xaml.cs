using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Presenters.Customers;
using POSSUM.Res;

namespace POSSUM.Views.Customers
{
    public partial class CustomerWindow : Window, ICustomerView
    {
        private readonly CustomerPresenter _presenter;
        CustomerType _customerType;
        public List<Customer> Customers = new List<Customer>();
        //public string SelectedTableName = "Select Table";
        public bool IsTakeaway = false;
        bool _isinovieBy = false;
        string _title = "";

        //public CustomerWindow(bool isbuyInovice, string title, CustomerType customerType)
        //{
        //    try
        //    {
        //        InitializeComponent();
        //        _isinovieBy = isbuyInovice;
        //        _title = title;
        //        txtTitle.Text = UI.Global_Select + " " + title;
        //        _presenter = new CustomerPresenter(this);
        //        _presenter.LoadCustomerClick();
        //    }
        //    catch (System.Exception ex)
        //    {
        //        LogWriter.LogWrite(ex);
        //    }
        //}

        public CustomerWindow(bool isbuyInovice, string title, CustomerType customerType)
        {
            try
            {
                InitializeComponent();
                _isinovieBy = isbuyInovice;
                _title = title;
                _customerType = customerType;
                txtTitle.Text = UI.Global_Select + " " + title;
                _presenter = new CustomerPresenter(this);
                _presenter.LoadCustomerClick(customerType);
            }
            catch (System.Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        public Customer SelectedCustomer { get; set; }

        public void SetCustomerResult(List<Customer> customers)
        {
            Customers = customers;
            CustomerDataGrid.ItemsSource = Customers;
        }

        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(title);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void SetFloors(List<Floor> floors)
        {
        }

        public string GetKeyword()
        {
            return txtSearchBox.Text;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            _presenter.LoadCustomerClick(_customerType);
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedCustomer = CustomerDataGrid.SelectedItem as Customer;
            OKButton.IsEnabled = SelectedCustomer != null;
        }

        private void popupError_Loaded(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.Placement = PlacementMode.Center;
        }

        private void txtSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.IsOpen = true;
        }

        private void txtSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.IsOpen = false;
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var newCustomer = new AddCustomerWindow(_isinovieBy, _title);
            if (newCustomer.ShowDialog() == true)
            {
                SelectedCustomer = newCustomer.CurrentCustomer;
                DialogResult = true;
            }
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupKeyborad.IsOpen = false;
                _presenter.LoadCustomerClick(_customerType);
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var customer = button?.DataContext as Customer;
            if (customer != null)
            {
                var newCustomer = new AddCustomerWindow(customer, _isinovieBy, _title);
                if (newCustomer.ShowDialog() == true)
                {
                    SelectedCustomer = newCustomer.CurrentCustomer;
                    DialogResult = true;
                }
            }
        }
        public List<Customer> GetCustomers()
        {
            return Customers;
        }

        private void ButtonEditDeposit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var customer = button?.DataContext as Customer;
            if (customer != null)
            {
                var newCustomer = new AddCustomerDepositWindow(customer);
                if (newCustomer.ShowDialog() == true)
                {
                    SelectedCustomer = newCustomer.CurrentCustomer;
                    _presenter.LoadCustomerClick(_customerType);
                }
            }
        }

        private void ButtonTransactionHistory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var customer = button?.DataContext as Customer;
            if (customer != null)
            {
                var newCustomer = new TransactionHistoryWindow(customer);
                if (newCustomer.ShowDialog() == true)
                {
                    SelectedCustomer = newCustomer.CurrentCustomer;
                    //DialogResult = true;
                }
            }
        }

    }
}