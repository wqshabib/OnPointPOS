using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using POSSUM.Model;
using System.Threading;
using POSSUM.Presenters.OrderHistory;
using POSSUM.Data;
using POSSUM.Res;

namespace POSSUM.View.OrderHistory
{
    public partial class SaleByItemsWindow : Window, IOrderHistoryView
    {
        OrderHistoryPresenter presenter;

        List<OrderLine> currentorderDetails = new List<OrderLine>();
        List<OrderLine> returnList = new List<OrderLine>();

        public SaleByItemsWindow()
        {
            InitializeComponent();
            presenter = new OrderHistoryPresenter(this);
            if (App.MainWindow.ShiftNo == 0)
                App.MainWindow.ShiftNo = 1;
            dtpFrom.Text = DateTime.Now.ToShortDateString();
            dtpTo.Text = DateTime.Now.ToShortDateString();

        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            currentorderDetails = presenter.HandleItemSaleSearchClick();
            dataGrid1.ItemsSource = currentorderDetails;
        }


        private void OnRowLoading(object sender, DataGridRowEventArgs e)
        {

            if (e.Row.Item != null)
            {
                var order = e.Row.Item as Order;
                if (order != null)
                    e.Row.Background = Utilities.GetColorBrushFromOrderStatus(order.Status);
            }
        }
        #region Remove Item from Order and Show Order Detail
        Order currentOrderMaster = OrderFactory.CreateEmtpy();
        #endregion

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var order = (Order)(sender as Button).DataContext;
                LogWriter.JournalLog(Defaults.User.TrainingMode ? Convert.ToInt16(JournalActionCode.ReceiptViewedViaTrainingMode) : Convert.ToInt16(JournalActionCode.ReceiptViewed), order.Id);
                presenter.HandlePrintClick(order);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        private void txtOrderNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //decimal val = 0;
            //e.Handled = !decimal.TryParse(e.Text, out val);
        }




        DateTime IOrderHistoryView.GetStartdate()
        {
            return Convert.ToDateTime(dtpFrom.Text);
        }

        DateTime IOrderHistoryView.GetEnddate()
        {
            return Convert.ToDateTime(dtpTo.Text + "  11:59:00 PM");
        }

        string IOrderHistoryView.GetQueryText()
        {
            return "";
        }

        public void SetResult(List<Order> orders)
        {
            var _orders = orders.OrderByDescending(o => o.InvoiceDate).ToList();
            dataGrid1.ItemsSource = _orders;
        }

        public void SetTotalAmount(decimal total)
        {

        }

        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Type officeType = Type.GetTypeFromProgID("Excel.Application");
                if (officeType == null)
                {
                    MessageBox.Show("MS Excel is not installed on your system", Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                //DataGridExcelTools.ExportToExcel(dataGrid1);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DateTime GetStartdateCCFailed()
        {
            throw new NotImplementedException();
        }

        public DateTime GetEnddateCCFailed()
        {
            throw new NotImplementedException();
        }

        public string GetQueryTextCCFailed()
        {
            throw new NotImplementedException();
        }

        public void SetTotalAmountCCFailed(decimal total)
        {
            throw new NotImplementedException();
        }

        public void SetResultCCFailed(List<Order> orders)
        {
            throw new NotImplementedException();
        }
    }

}
