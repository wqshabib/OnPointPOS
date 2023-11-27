using POSSUM.Events;
using POSSUM.Model;
using POSSUM.Presenters.Customers;
using POSSUM.Res;
using POSSUM.Views.PrintOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POSSUM.Views.Customers
{
    /// <summary>
    /// Interaction logic for TransactionHistoryWindow.xaml
    /// </summary>
    public partial class TransactionHistoryWindow : Window
    {
        private readonly CustomerPresenter _presenter;
        public Customer CurrentCustomer;
        public event UploadCustomerEventHandler UploadCustomer;
        public TransactionHistoryWindow()
        {
            InitializeComponent();
        }
        public TransactionHistoryWindow(Customer customer)
        {
            InitializeComponent();
            _presenter = new CustomerPresenter();
            CurrentCustomer = customer;
            TransactionHistoryDataGrid.ItemsSource = _presenter.GetDepositHistory(customer.Id);
            txtTitle.Text = UI.Transection_History;
            txtTotal.Text = CurrentCustomer.DepositAmount.ToString();
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            var obj = (sender as Button).DataContext as DepositHistoryViewModel;
            if (obj != null && obj.OrderId != null && obj.OrderId != Guid.Empty)
            {
                var billWindow = new PrintInvoiceWindow(obj.OrderId.Value, true);
                billWindow.ShowDialog();
            }
            else
            {
                List<string> lst = new List<string>();
                lst.Add("Deposit Amount: " + obj.CreditAmount);
                lst.Add(UI.Transaction_Old_Balance + ": " + obj.OldBalance);
                lst.Add(UI.Transaction_New_Balance + ": " + obj.NewBalance);
                lst.Add("");
                if (string.IsNullOrEmpty(obj.CustomerReceipt))
                {
                    obj.CustomerReceipt = string.Join(Environment.NewLine, lst);
                }
                else
                {
                    var lstReceipt = obj.CustomerReceipt.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                    lstReceipt.InsertRange(0, lst);
                    obj.CustomerReceipt = string.Join(Environment.NewLine, lstReceipt);
                }

                var printWindow = new PrintReportWindow(obj.CustomerReceipt, ReportType.CustomerReceipt, Guid.Empty);
                printWindow.ShowDialog();
            }

        }
    }
}
