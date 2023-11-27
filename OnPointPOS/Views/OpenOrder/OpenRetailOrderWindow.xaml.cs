using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POSSUM.Model;
using POSSUM.Presenters.OpenOrder;
using POSSUM.Res;
using POSSUM.Views.CheckOut;
using POSSUM.Views.Sales;

namespace POSSUM.Views.OpenOrder
{
    public partial class OpenRetailOrderWindow : Window, IOpenOrderView
    {
        public OpenOrderPresenter presenter;
        private Order masterOrder;
        private List<OrderLine> currentOrderDetail;

        public decimal OrderTotal = 0;
        OrderType type = OrderType.Standard;
        private OrderType moveType = OrderType.Standard;
        private List<Order> selectedOrders;
        public OpenRetailOrderWindow()
        {
            InitializeComponent();
            presenter = new OpenOrderPresenter(this);
            presenter.HandelPendingClick();

        }
        public int GetFloorId()
        {
            return 1;
        }
        public void SetFoodTablesResult(List<FoodTable> tables)
        {

        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #region Interface Method Implementation
        public void NewRecord(bool res)
        {
            if (res)
            {

                masterOrder = new Order();
                currentOrderDetail = new List<OrderLine>();
            }
        }

        public Order GetOrder()
        {
            return masterOrder;
        }

        public void SetOrderMaster(Order order)
        {
            masterOrder = order;
        }
        public List<OrderLine> GetOrderDetail()
        {
            return currentOrderDetail;
        }
        public void CalculatOrderTotal(IList<OrderLine> list)
        {

        }
        public OrderType GetMoveType()
        {
            return moveType;
        }

        public decimal GetOrderTotal()
        {
            return OrderTotal;
        }
        public void SetTableResult(List<FoodTable> customers)
        {
        }
        public void SetTableOrderResult(List<Order> orders)
        {

        }
        public void SetPendingOrderResult(List<Order> orders)
        {
            PendingDataGrid.ItemsSource = orders;
        }
        public void SetOrderDetailResult(List<OrderLine> orderLines)
        {

        }

        public List<Order> GetSelectedOrders()
        {
            return selectedOrders;
        }

        public OrderType GetOrderType()
        {
            return type;
        }

        public FoodTable SelectedTable { get; set; }
        public FoodTable GetSelectedTable()
        {
            return SelectedTable;
        }
        #endregion

        #region Pending Area
        private void Proforma_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                masterOrder = (Order)(sender as Button).DataContext;
                if (masterOrder != null)
                {
                    Cursor = Cursors.AppStarting;
                    presenter.HandelPerformaClick(masterOrder.Id);
                    Cursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PendingCheckOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orderMaster = (Order)(sender as Button).DataContext;

                var uc = new UCSale(orderMaster.Id);
                App.MainWindow.AddControlToMainCanvas(uc);

                this.Close();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnRowLoading(object sender, DataGridRowEventArgs e)
        {
        }
        private void PendingCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                masterOrder = (Order)(sender as Button).DataContext;
                if (masterOrder != null)
                {
                    if (MessageBox.Show(UI.Message_SureToCancelOrder, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {

                        presenter.HandelPendingCancelClick(masterOrder.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion


        private void PendingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            masterOrder = PendingDataGrid.SelectedItem as Order;
            //if (masterOrder != null)
            //{
            //    type = masterOrder.Type;
            //}
        }

        public void ShowError(string message, string title)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public int GetFloor()
        {
            return 1;
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}