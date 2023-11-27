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
using POSSUM.Presenters.Bookings;
using POSSUM.Data.Repositories;

namespace POSSUM.Views.Bookings
{
    /// <summary>
    /// Interaction logic for BookingListWindow.xaml
    /// </summary>
    public partial class BookingListWindow : Window, IBookingListView
    {
        BookingListPresenter presenter;

        List<OrderLine> currentorderDetails = new List<OrderLine>();
        List<OrderLine> returnList = new List<OrderLine>();

        public BookingListWindow()
        { 
            try
            {
                InitializeComponent();
                presenter = new BookingListPresenter(this);
                dtpFrom.Text = DateTime.Now.ToShortDateString();
                dtpTo.Text = DateTime.Now.ToShortDateString();
                dtpFromcc.Text = DateTime.Now.ToShortDateString();
                dtpTocc.Text = DateTime.Now.ToShortDateString();
                presenter.HandleSearchClick();
                presenter.HandleSearchClickCCFailed();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleSearchClick();
        }

        private void btnViewCCFailed_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleSearchClickCCFailed();
        }


        private void OnRowLoading(object sender, DataGridRowEventArgs e)
        {
            //if (e.Row.Item != null)
            //{
            //    var order = e.Row.Item as Order;
            //    if (order != null)
            //        e.Row.Background = Utilities.GetColorBrushFromOrderStatus(order.Status);
            //}
        }
       

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var order = (BookingViewModel)(sender as Button).DataContext;
                //LogWriter.JournalLog(Defaults.User.TrainingMode ? Convert.ToInt16(JournalActionCode.ReceiptViewedViaTrainingMode) : Convert.ToInt16(JournalActionCode.ReceiptViewed), order.Id);
                presenter.HandleCheckOutClick(12, Guid.Empty, 1);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        private void txtOrderNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        DateTime IBookingListView.GetStartdate()
        {
            return Convert.ToDateTime(dtpFrom.Text);
        }

        DateTime IBookingListView.GetEnddate()
        {
            return Convert.ToDateTime(dtpTo.Text + "  11:59:00 PM");
        }

        DateTime IBookingListView.GetStartdateCCFailed()
        {
            return Convert.ToDateTime(dtpFromcc.Text);
        }

        DateTime IBookingListView.GetEnddateCCFailed()
        {
            return Convert.ToDateTime(dtpTocc.Text + "  11:59:00 PM");
        }

        string IBookingListView.GetQueryText()
        {
            return txtOrderNumber.Text;
        }

        string IBookingListView.GetQueryTextCCFailed()
        {
            return txtOrderNumbercc.Text;
        }

        public void SetResult(List<BookingViewModel> bookings)
        {
            var _bookings = bookings.OrderByDescending(o => o.Created).ToList();
            dataGrid1.ItemsSource = _bookings;
        }

        public void SetResultCCFailed(List<BookingViewModel> bookings)
        {
            var _bookings = bookings.OrderByDescending(o => o.Updated).ToList();
            dataGrid1cc.ItemsSource = _bookings;
        }

        public void SetTotalAmount(decimal total)
        {
            txtOrdersTotal.Text = total.ToString("C");
        }

        public void SetTotalAmountCCFailed(decimal total)
        {
            txtOrdersTotalcc.Text = total.ToString("C");
        }

        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonReturn_Click(object sender, RoutedEventArgs e)
        {
            var order = (Order)(sender as Button).DataContext;
            if (order != null)
            {
                ReturnOrderWindow retrunOrderWindow = new ReturnOrderWindow(order);
                retrunOrderWindow.ShowDialog();
            }
        }

        private void ButtonRetry_Click(object sender, RoutedEventArgs e)
        {
            var order = (Order)(sender as Button).DataContext;
            if (order != null)
            {
                presenter.RetryOrder(order);
                presenter.HandleSearchClick();
                presenter.HandleSearchClickCCFailed();
            }
        }
    }
    public class TextBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(TextBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));



        public static int GetTextLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetTextLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("TextLength", typeof(int), typeof(TextBoxMonitor), new UIPropertyMetadata(0));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as TextBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.TextChanged += TextChanged;
            }
            else
            {
                pb.TextChanged -= TextChanged;
            }
        }

        static void TextChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as TextBox;
            if (pb == null)
            {
                return;
            }
            SetTextLength(pb, pb.Text.Length);
        }
    }
}
