using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using POSSUM.Model;
using POSSUM.Presenters.OpenOrder;
using POSSUM.Handlers;
using POSSUM.Res;
using POSSUM.Views.Customers;
using POSSUM.Views.Sales;
using POSSUM.Views.SplitOrder;
using POSSUM.Data;
using POSSUM.Views.FoodTables;
using POSSUM.Utils;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace POSSUM.Views.OpenOrder
{
    public partial class OpenOrderWindow : Window, IOpenOrderView
    {
        public OpenOrderPresenter presenter;
        private Order masterOrder;
        private List<Order> orderList;
        private List<OrderLine> currentOrderDetail;
        private List<FoodTable> tables;
        public decimal OrderTotal = 0;
        private int FloorId = 1;
        public int selectedTableId = 0;
        // bool moveToTakeway = false;
        //   bool tableMode = true;
        private OrderType type = OrderType.TableOrder;
        private OrderType moveType = OrderType.Standard;
        private List<Order> selectedOrders;
        public bool NewOrderMerged = false;
        private Order newOrder;

        //  ApplicationDbContext db;
        public OpenOrderWindow()
        {
            InitializeComponent();
            presenter = new OpenOrderPresenter(this);

            InitialTableSetting();
            SetFloors(presenter.LoadFloor());
            presenter.HandelTableClick();
            GetSelectedColorBrush();
            // db = PosState.GetInstance().Context;

        }
        public OpenOrderWindow(Order newOrder)
        {
            InitializeComponent();
            presenter = new OpenOrderPresenter(this);

            InitialTableSetting();
            selectedTableId = newOrder.TableId;

            this.newOrder = newOrder;
            SetFloors(presenter.LoadFloor());
            presenter.HandelTableClick();
            GetSelectedColorBrush();
            //   db = PosState.GetInstance().Context;

        }

        private void InitialTableSetting()
        {
            if (Defaults.ShowTableGrid)
            {
                TableCanvas.Visibility = Visibility.Collapsed;
                btnArrangeLayout.Visibility = Visibility.Collapsed;
                TablesOuterGrid.Visibility = Visibility.Visible;
                //presenter.LoadTablesClick();
            }
            else
            {
                TableCanvas.Visibility = Visibility.Visible;
                btnArrangeLayout.Visibility = Visibility.Visible;
                TablesOuterGrid.Visibility = Visibility.Collapsed;
                DisplayTables(FloorId);
            }
        }
        private static ControlTemplate CreateTemplate(string imageUrl, string name)
        {
            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = "roundTable.png";
            imageUrl = @"/POSSUM;component/images/" + imageUrl;

            string template =
        "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
             "<StackPanel>" +
                  "<Image HorizontalAlignment=\"Center\"   Source=\"" + imageUrl + "\" Stretch=\"Fill\" Width=\"65\" Height=\"45\"/>" +
                "<TextBlock HorizontalAlignment=\"Center\" Text = \"" + name + "\" />" +
             "</StackPanel>" +
         "</ControlTemplate>";

            //string img= "<Image HorizontalAlignment=\"Center\"   Source=\"" + imageUrl + "\" Stretch=\"Fill\" Width=\"65\" Height=\"45\">" +
            // "<Image.RenderTransform>"+
            //   "<RotateTransform Angle = \"45\" />" +
            //"</Image.RenderTransform >" +
            // "</Image>";
            return (ControlTemplate)XamlReader.Parse(template);

        }
        private void DisplayTables(int floorId)
        {
            btnArrangeLayout.Visibility = Visibility.Visible;
            tables = presenter.GetTables(floorId);
            TableCanvas.Children.Clear();
            ArrangeTable.Children.Clear();
            bool isFirst = true;
            foreach (var table in tables)
            {
                Button btn = new Button();

                //btn.Padding = new Thickness(5);
                Canvas.SetLeft(btn, table.PositionY);
                Canvas.SetTop(btn, table.PositionX);
                btn.DataContext = table;
                if (table.OrderCount > 0)
                {
                    btn.Template = CreateTemplate("FoodTableRes.png", table.Name);
                }
                else
                {
                    btn.Template = CreateTemplate("FoodTable.png", table.Name);
                }

                if (isFirst)
                {
                    btn.Template = CreateTemplate("FoodTableSelected.png", table.Name);
                    isFirst = false;
                }

                btn.Click += Btn_Click;
                // btn.TouchUp += Btn_Click;
                TableCanvas.Children.Add(btn);


                var thumb = new MyThumb();

                thumb.DataContext = table;
                Canvas.SetLeft(thumb, table.PositionY);
                Canvas.SetTop(thumb, table.PositionX);
                if (table.OrderCount > 0)
                    thumb.Template = CreateTemplate("FoodTableRes.png", table.Name);
                else
                    thumb.Template = CreateTemplate("FoodTable.png", table.Name);

                // thumb.Template = CreateTemplate(table.ImageUrl, table.Name);
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += Thumb_DragCompleted;
                thumb.DragStarted += Thumb_DragStarted;
                /*
                var template = CreateTemplate("FoodTable.png", table.Name);
                ContentControl cntrl = new ContentControl();
                cntrl.DataContext = table;
                cntrl.Height = 40;
                cntrl.Width = 60;
                cntrl.SetValue(Selector.IsSelectedProperty, true);
                cntrl.Style = Application.Current.FindResource("DesignerItemStyle") as Style;
                cntrl.Padding = new Thickness(5);
                Button bb = new Button();
                bb.Template = template;
                cntrl.Content = bb;
                Canvas.SetLeft(cntrl, table.PositionY);
                Canvas.SetTop(cntrl, table.PositionX);
               */
                ArrangeTable.Children.Add(thumb);
            }
        }
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {

        }
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var thumb = e.Source as MyThumb;
            FoodTable table = thumb.DataContext as FoodTable;
            var left = Canvas.GetLeft(thumb);// + e.HorizontalChange;
            var top = Canvas.GetTop(thumb);// + e.VerticalChange;
            table.PositionX = Convert.ToInt16(top);
            table.PositionY = Convert.ToInt16(left);
            presenter.UpdateLocation(table);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = e.Source as MyThumb;

            var left = Canvas.GetLeft(thumb) + e.HorizontalChange;
            var top = Canvas.GetTop(thumb) + e.VerticalChange;

            Canvas.SetLeft(thumb, left);
            Canvas.SetTop(thumb, top);
        }
        public bool isTakeaway = false;
        public bool IsNewOrder = false;
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            CleanAllSelection();
            var btn = sender as Button;
            if (btn != null)
            {
                FoodTable table = btn.DataContext as FoodTable;
                btn.Template = CreateTemplate("FoodTableSelected.png", table.Name);
            }

            _lastSelectedFoodOrderTable = (FoodTable)((Button)sender).DataContext;

            if (_lastSelectedFoodOrderTable != null)
            {
                SelectedTable = _lastSelectedFoodOrderTable;
                IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
                masterOrder = null;
                lblSelectedTable.Text = UI.OpenOrder_TableButton + _lastSelectedFoodOrderTable.Id;
                presenter.HandleOrderByTableClick();
                //this.DialogResult = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //if (chkMergeNewOrder.IsChecked == true)
            //{
            //    selectedTableId = selectedTableId > 0 ? selectedTableId : SelectedTable.Id;
            //    if (newOrder != null)
            //    {
            //        NewOrderMerged = presenter.MergeNewOrder(newOrder, selectedTableId);
            //    }
            //}
            IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
            if (SelectedTable != null)// && IsNewOrder)
                this.DialogResult = true;
            else
                this.DialogResult = false;
            //this.Close();
        }

        private void HoldButton_Click(object sender, RoutedEventArgs e)
        {
            //  tableMode = false;
            type = OrderType.Standard;
            lblPendingOrder.Visibility = Visibility.Visible;
            lblTakeawayOrder.Visibility = Visibility.Collapsed;
            presenter.HandelPendingClick();
            TableOrderGrid.Visibility = Visibility.Collapsed;
            PendingOrderGrid.Visibility = Visibility.Visible;
            GetSelectedColorBrush();
        }

        private void TakeawayButton_Click(object sender, RoutedEventArgs e)
        {
            // tableMode = false;
            type = OrderType.TakeAway;
            lblPendingOrder.Visibility = Visibility.Collapsed;
            lblTakeawayOrder.Visibility = Visibility.Visible;
            presenter.HandelPendingClick();
            TableOrderGrid.Visibility = Visibility.Collapsed;
            PendingOrderGrid.Visibility = Visibility.Visible;
            GetSelectedColorBrush();
        }
        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            type = OrderType.TableOrder;
            presenter.HandelTableClick();
            TableOrderGrid.Visibility = Visibility.Visible;
            PendingOrderGrid.Visibility = Visibility.Collapsed;
            GetSelectedColorBrush();
        }
        public void SetFloors(List<Floor> floors)
        {
            STKFloors.Children.Clear();
            if (floors.Count > 1)
            {
                var brush = (Brush)new BrushConverter().ConvertFromString("#FF007ACC");
                var background = new SolidColorBrush(((SolidColorBrush)brush).Color);
                var foreground = (Brush)new BrushConverter().ConvertFromString("#FFFFFF");
                STKFloors.Visibility = Visibility.Visible;
                //STKFloors.Width = 200;
                foreach (var floor in floors)
                {
                    Button floorButton = new Button();
                    floorButton.Name = "btnFloor" + floor.Id;
                    if (floor.Id == 1)
                    {
                        floorButton.Background = background;
                        floorButton.Foreground = foreground;
                    }
                    floorButton.Height = 40;
                    floorButton.Width = 80;
                    floorButton.Margin = new Thickness(1);
                    floorButton.Content = floor.Name;
                    floorButton.DataContext = floor;
                    floorButton.Click += FloorButton_Click;

                    STKFloors.Children.Add(floorButton);
                }
            }
            else
            {
                this.Width = this.Width - 225;
                STKFloors.Visibility = Visibility.Collapsed;
            }
        }
        public string GetKeyword()
        {
            return "";
        }
        private void FloorButton_Click(object sender, RoutedEventArgs e)
        {
            var floor = (sender as Button).DataContext as Floor;
            if (floor != null)
            {

                FloorId = floor.Id;
                if (Defaults.ShowTableGrid)
                {
                    tables = presenter.GetTables(FloorId);
                    FillTablesListBox();
                }
                else
                    presenter.HandelTableClick();

            }
            GetSelectedFloorColorBrush((sender as Button));
        }
        public void GetSelectedFloorColorBrush(Button btnFloor)
        {
            try
            {


                var defaultbrush = (Brush)new BrushConverter().ConvertFromString("#FFDCDEDE");
                var defaultForeground = (Brush)new BrushConverter().ConvertFromString("#000000");
                var defaulColor = new SolidColorBrush(((SolidColorBrush)defaultbrush).Color);
                btnFloor.Background = defaulColor;
                btnFloor.Foreground = defaultForeground;
                btnFloor.Background = defaulColor;
                btnFloor.Foreground = defaultForeground;
                var aa = STKFloors.Children;
                foreach (var a in aa)
                {
                    var btn = (a as Button);
                    if (btn != null)
                    {
                        if (btn.Name == btnFloor.Name)
                        {
                            var brush = (Brush)new BrushConverter().ConvertFromString("#FF007ACC");
                            btn.Background = new SolidColorBrush(((SolidColorBrush)brush).Color);
                            btn.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFFFFF");
                        }
                        else
                        {

                            btn.Background = defaulColor;
                            btn.Foreground = defaultForeground;
                        }
                    }
                }

            }
            catch (Exception)
            {


            }

        }

        public void GetSelectedColorBrush()
        {
            var defaultbrush = (Brush)new BrushConverter().ConvertFromString("#FFDCDEDE");
            var defaultForeground = (Brush)new BrushConverter().ConvertFromString("#000000");
            var defaulColor = new SolidColorBrush(((SolidColorBrush)defaultbrush).Color);
            btnPausade.Background = defaulColor;
            btnPausade.Foreground = defaultForeground;
            btnTakeaway.Background = defaulColor;
            btnTakeaway.Foreground = defaultForeground;
            btnTables.Background = defaulColor;
            btnTables.Foreground = defaultForeground;
            if (type == OrderType.TableOrder)
            {
                var brush = (Brush)new BrushConverter().ConvertFromString("#FF007ACC");
                btnTables.Background = new SolidColorBrush(((SolidColorBrush)brush).Color);
                btnTables.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFFFFF");
            }
            else if (type == OrderType.TakeAway)
            {
                var brush = (Brush)new BrushConverter().ConvertFromString("#FF007ACC");
                btnTakeaway.Background = new SolidColorBrush(((SolidColorBrush)brush).Color);
                btnTakeaway.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFFFFF");
            }
            else
            {
                var brush = (Brush)new BrushConverter().ConvertFromString("#FF007ACC");
                btnPausade.Background = new SolidColorBrush(((SolidColorBrush)brush).Color);
                btnPausade.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFFFFF");
            }
        }

        private void MergeOrder_Click(object sender, RoutedEventArgs e)
        {
            selectedOrders = new List<Order>();
            if (type == OrderType.TableOrder)
            {
                var data = dataGridTableOrder.SelectedItems;

                foreach (var item in data)
                {
                    var order = (Order)item;
                    selectedOrders.Add(order);
                }
            }
            else
            {
                var data = PendingDataGrid.SelectedItems;
                SelectedTable = new FoodTable();
                foreach (var item in data)
                {
                    var order = (Order)item;
                    selectedOrders.Add(order);
                }
            }
            if (selectedOrders.Count > 1)
            {
                if (MessageBox.Show(UI.OpenOrder_ConfirmMerg, UI.OpenOrder_Merge, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    presenter.HandelMergeOrderClick();
                    if (type == OrderType.TableOrder)
                        presenter.HandleOrderByTableClick();
                    else
                        presenter.HandelPendingClick();
                }
            }
            else
                MessageBox.Show(UI.OpenOrder_ValidateToMerge, UI.OpenOrder_Merge, MessageBoxButton.OK, MessageBoxImage.Exclamation);

            /*Reload window again*/
            presenter = new OpenOrderPresenter(this);
            presenter.HandelPendingClick();
        }

        private void MoveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (type == OrderType.TableOrder)
            {
                selectedOrders = new List<Order>();
                var data = dataGridTableOrder.SelectedItems;

                foreach (var item in data)
                {
                    var order = (Order)item;
                    selectedOrders.Add(order);
                }
                if (selectedOrders.Count > 0)
                {
                    //if (MessageBox.Show(UI.OpenOrder_ConfirmMove, Defaults.ProductName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    //{
                    var tableWindow = new TableWindow(true);
                    if (tableWindow.ShowDialog() == true)
                    {
                        if (tableWindow.isTakeaway)
                            moveType = OrderType.TakeAway;
                        else
                            SelectedTable = tableWindow.SelectedTable;
                        presenter.HandelMoveOrderClick();
                        currentOrderDetail = new List<OrderLine>();
                        CalculatOrderTotal(currentOrderDetail);
                        lvCart.ItemsSource = currentOrderDetail;
                        lvCart.Items.Refresh();
                        presenter.HandelTableClick();
                    }
                    //}
                }
            }
            else
            {
                selectedOrders = new List<Order>();
                var data = PendingDataGrid.SelectedItems;

                foreach (var item in data)
                {
                    var order = (Order)item;
                    selectedOrders.Add(order);
                }
                if (selectedOrders.Count > 0)
                {
                    //if (MessageBox.Show(UI.OpenOrder_ConfirmMerg, Defaults.ProductName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    //{
                    var tableWindow = new TableWindow();
                    if (tableWindow.ShowDialog() == true)
                    {
                        if (tableWindow.isTakeaway)
                            moveType = OrderType.TakeAway;
                        else
                            SelectedTable = tableWindow.SelectedTable;
                        presenter.HandelPendingMoveOrderClick();

                    }
                    // }
                }
            }
        }

        private void SplitOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (masterOrder != null)
                {
                    var splitOrderWindow = new SplitOrderWindow(masterOrder);
                    if (splitOrderWindow.ShowDialog() == true)
                    {
                        this.DialogResult = true;
                    }
                    else
                    {
                        if (type == OrderType.TableOrder)
                            presenter.HandleOrderByTableClick();
                        else if (type == OrderType.Standard)
                            presenter.HandelPendingClick();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }
        #region Interface Method Implementation
        public void NewRecord(bool res)
        {
            if (res)
            {
                orderList.Remove(masterOrder);
                dataGridTableOrder.ItemsSource = orderList;
                dataGridTableOrder.Items.Refresh();
                masterOrder = new Order();
                currentOrderDetail = new List<OrderLine>();
                lvCart.ItemsSource = null;
                lvCart.ItemsSource = currentOrderDetail;
                lblGrossTotal.Text = (0).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                OrderTotal = 0;
                lblVatTotal.Text = (0).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                lblTotal.Text = (0).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                lblCurrency.Text = ((CultureInfo)Defaults.UICultureInfo).NumberFormat.CurrencySymbol;
                lblOrderType.Visibility = Visibility.Collapsed;
                lblOrderType.Text = UI.Sales_NewButton;
                type = masterOrder.Type;
                masterOrder = null;
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
        public void CalculatOrderTotal(IList<OrderLine> lst)
        {
            OrderTotal = 0;

            decimal tax = 0;

            //var itemsQty = lst
            //                .GroupBy(g => new
            //                {
            //                    g.ItemId
            //                })
            //                .Select(group => new
            //                {
            //                    ItemId = group.Key.ItemId,
            //                    Qty =Math.Ceiling(group.Sum(a => a.Quantity))
            //                });

            //foreach (var itm in lst)
            //{
            //    int roundUpValue = (int)Math.Ceiling(itm.Quantity);

            //    double qt = Convert.ToDouble(roundUpValue - itm.Quantity);

            //    if (qt <= 0.00000000000001)
            //        itm.Quantity = roundUpValue;

            //}


            foreach (var item in lst)
            {
                if (item.IsValid)
                {
                    if (item.Product.ItemType == ItemType.Individual)
                        tax += item.VatAmount();
                    OrderTotal += item.GrossAmountDiscounted();

                    if (item.IngredientItems != null)
                        foreach (var ingredient in item.IngredientItems)
                        {
                            tax += ingredient.VatAmount();
                            OrderTotal += ingredient.GrossAmountDiscounted();

                        }

                }
            }





            foreach (var detail in lst.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
            {
                // var vatInnerGroups = detail.OrderItemDetails.GroupBy(od => od.TAX);
                if (detail.ItemDetails != null)
                    foreach (var itm in detail.ItemDetails)
                    {

                        decimal vatAmount = itm.VatAmount();
                        tax += vatAmount;
                    }
            }

            decimal nettotal = OrderTotal - tax;
            //OrderTotal = Math.Round(OrderTotal, 0);


            lblGrossTotal.Dispatcher.BeginInvoke(new Action(() => lblGrossTotal.Text = nettotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            lblVatTotal.Dispatcher.BeginInvoke(new Action(() => lblVatTotal.Text = tax.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            lblTotal.Dispatcher.BeginInvoke(new Action(() => lblTotal.Text = OrderTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));

        }
        public OrderType GetMoveType()
        {
            return moveType;
        }

        public decimal GetOrderTotal()
        {
            return OrderTotal;
        }
        public void SetTableResult(List<FoodTable> tables)
        {
            PopupSplit.IsOpen = false;
            txtSplitNumber.Text = "";
            this.tables = tables;
            FillTablesListBox();
            DisplayTables(FloorId);
        }
        public void SetTableOrderResult(List<Order> orders)
        {
            OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
            orderList = orders.OrderBy(o => o.CreationDate).ToList();
            //  orderList = orders.OrderBy(o => o.Id).ToList();
            if (newOrder != null && newOrder.TableId > 0 && newOrder.Status > 0 && orderList.FirstOrDefault(a => a.Id == newOrder.Id) == null)
            {
                orderList.Add(newOrder);
            }


            dataGridTableOrder.ItemsSource = null;
            dataGridTableOrder.ItemsSource = orderList;//.OrderBy(o=>o.CreationDate).ToList();
            if (orderList.Count > 0)
            {
                if (masterOrder != null)
                    presenter.HandelOrderDetailClick(masterOrder.Id);
                else
                {
                    masterOrder = orderList.FirstOrDefault();
                    if (masterOrder != null)
                    {
                        dataGridTableOrder.SelectedItems.Add(masterOrder);
                        var detail = orderRepo.GetOrderLinesById(masterOrder.Id);
                        currentOrderDetail = detail;
                        CalculatOrderTotal(currentOrderDetail);
                        lvCart.ItemsSource = currentOrderDetail;
                        lvCart.Items.Refresh();
                        type = OrderType.TableOrder;
                        if (masterOrder.BalanceAmount > 0 && masterOrder.PartialPaidAmount > 0)
                        {
                            lblRemaining.Visibility = Visibility.Visible;
                            txtRemainingTotal.Visibility = Visibility.Visible;
                            txtRemainingTotal.Text = masterOrder.BalanceAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                        }
                        else
                        {
                            lblRemaining.Visibility = Visibility.Collapsed;
                            txtRemainingTotal.Visibility = Visibility.Collapsed;
                            txtRemainingTotal.Text = masterOrder.BalanceAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);

                        }
                        if (masterOrder.PartialPaidAmount > 0)
                        {
                            lblPaid.Visibility = Visibility.Visible;
                            txtPaidTotal.Text = masterOrder.PartialPaidAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                            txtPaidTotal.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            lblPaid.Visibility = Visibility.Collapsed;
                            txtPaidTotal.Text = masterOrder.PartialPaidAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                            txtPaidTotal.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else
            {
                currentOrderDetail = new List<OrderLine>();
                CalculatOrderTotal(currentOrderDetail);
                lvCart.ItemsSource = currentOrderDetail;
                lvCart.Items.Refresh();
            }

        }
        public void SetPendingOrderResult(List<Order> orders)
        {
            PendingDataGrid.ItemsSource = orders;
            txtSplitNumber.Text = "";
            PopupSplit.IsOpen = false;
            if (orders != null)
            {
                decimal orderTotal = Math.Round(orders.Sum(c => c.BalanceAmount), 2);
                txtOrdersTotal.Text = orderTotal.ToString();
            }
        }
        public void SetOrderDetailResult(List<OrderLine> orderLines)
        {
            try
            {
                currentOrderDetail = orderLines;
                lvCart.ItemsSource = currentOrderDetail;
                CalculatOrderTotal(currentOrderDetail);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
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
        public int GetFloorId()
        {
            return FloorId;
        }
        private void FillTablesListBox()
        {
            //WAQAS = tables = presenter.GetTables(GetFloorId());
            var totalCustomers = tables.Count;
            if (totalCustomers < 35)
            {
                var addmoreItems = 35 - totalCustomers;
                for (var i = 0; i < addmoreItems; i++)
                {
                    tables.Add(new FoodTable
                    {
                        Id = 0,
                        Name = "",
                        ColorCode = "#FFDCDEDE"
                    });
                }
            }
            else if (totalCustomers > 35 && totalCustomers < 42)
            {
                var addmoreCustomers = 42 - totalCustomers;
                for (var i = 0; i < addmoreCustomers; i++)
                {
                    tables.Add(new FoodTable
                    {
                        Id = 0,
                        Name = "",
                        ColorCode = "#FFDCDEDE"
                    });
                }
            }
            else if (totalCustomers > 42 && totalCustomers < 49)
            {
                var addmoreCustomers = 49 - totalCustomers;
                for (var i = 0; i < addmoreCustomers; i++)
                {
                    tables.Add(new FoodTable
                    {
                        Id = 0,
                        Name = "",
                        ColorCode = "#FFDCDEDE"
                    });
                }
            }

            AddTablesToGrid(tables);
            if (tables != null)
            {
                //if (SelectedTable == null || SelectedTable.Id == 0)
                //{
                if (selectedTableId > 0)
                {
                    var customer = tables.FirstOrDefault(c => c.Id == selectedTableId);
                    if (customer != null && customer.Id > 0)
                    {
                        SelectedTable = customer;
                        lblSelectedTable.Text = UI.OpenOrder_TableButton + selectedTableId;
                        //load the open orders of first table
                        //  presenter.HandleOpenTableOrders(); //load the open orders against all tables
                    }
                }
                else
                {
                    var customer = tables.FirstOrDefault();
                    if (customer != null && customer.Id > 0)
                    {
                        SelectedTable = customer;
                        lblSelectedTable.Text = UI.OpenOrder_TableButton + SelectedTable.Id;
                        //load the open orders of first table
                        //  presenter.HandleOpenTableOrders(); //load the open orders against all tables
                    }
                }
                //}

                presenter.HandleOrderByTableClick();
            }
        }


        private void AddTablesToGrid(List<FoodTable> foodTables)
        {
            TableViewGrid.Children.Clear();
            // Style style = Application.Current.FindResource("KeyBoradButton") as Style;


            if (foodTables.Count < 7)
                TableViewGrid.Columns = foodTables.Count;
            else
                TableViewGrid.Columns = 7;
            bool isFirst = true;
            foreach (var foodTable in foodTables)
            {
                var btn1 = new Button();
                if (isFirst)
                {
                    btn1.Background = new SolidColorBrush(Colors.Brown);
                    isFirst = false;
                }

                //btn1.Padding = new Thickness(5);
                // btn1.Style = style;
                btn1.Background = Utilities.GetColorBrush(foodTable.ColorCode);
                btn1.Content = GetButtonContent(foodTable);
                btn1.Width = 112;
                btn1.Height = 51;
                btn1.Tag = foodTable.Id;
                btn1.DataContext = foodTable;
                TableViewGrid.Children.Add(btn1);
                btn1.Click += ButtonTable_Click;
            }
        }
        private StackPanel GetButtonContent(FoodTable foodTable)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;
            txtBlock.Text = foodTable.Name;
            txtBlock.Width = 100;
            txtBlock.TextAlignment = TextAlignment.Center;
            txtBlock.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(txtBlock);

            return stackPanel;
        }

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
                Defaults.IsOpenOrder = true;
                var orderMaster = (Order)(sender as Button).DataContext;
                var uc = new UCSale(orderMaster.Id);
                App.MainWindow.AddControlToMainCanvas(uc);
                //if (Defaults.ScreenResulution == ScreenResulution.SR_1000X768)
                //{
                //    var uc = new UCSales(orderMaster.Id);
                //    App.MainWindow.AddControlToMainCanvas(uc);
                //}
                //else
                //{
                //    var uc = new UCSales1366_768(orderMaster.Id);
                //    App.MainWindow.AddControlToMainCanvas(uc);
                //}

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
                    if (MessageBox.Show(UI.Message_CancelOrder, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
        private void TakeawyButton_Click(object sender, RoutedEventArgs e)
        {
            isTakeaway = true;
            IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
            this.DialogResult = true;
        }
        private void popupTable_Loaded(object sender, RoutedEventArgs e)
        {
            PopupTable.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
        }
        private void Button_TouchLeave(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Button _button = (Button)sender as Button;
            if (_button != null && e.TouchDevice.Captured == _button)
            {
                _button.ReleaseTouchCapture(e.TouchDevice);
            }
            var back = "" + (sender as Button).Content;
            if (back == "Back")
            {
                try
                {
                    var uc = new UCSale();
                    App.MainWindow.AddControlToMainCanvas(uc);
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var customer = (FoodTable)((Button)sender).DataContext;

                if (customer.Id == 0)
                    this.DialogResult = false;
                else
                {
                    SelectedTable = customer;
                    IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
                    this.DialogResult = true;
                }
            }
        }
        public Order GetCurrentOrder()
        {
            throw new NotImplementedException();
        }
        public void SetFoodTablesResult(List<FoodTable> tables)
        {
            this.tables = tables;
            FillTablesListBox();
        }

        private void ArrangeLayout_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = true;
        }

        private void OKLayout_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
            DisplayTables(FloorId);
        }

        private void CancelLayout_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
        }

        private void CleanAllSelection()
        {
            try
            {
                foreach (var item in TableViewGrid.Children)
                {
                    var btn = item as Button;
                    if (btn != null)
                    {
                        var table = (FoodTable)((Button)item).DataContext;
                        btn.Background = Utilities.GetColorBrush(table.ColorCode);
                    }
                }

                foreach (var item in TableCanvas.Children)
                {
                    var btn = item as Button;
                    if (btn != null)
                    {
                        var table = (FoodTable)((Button)item).DataContext;
                        if (table.OrderCount > 0)
                        {
                            btn.Template = CreateTemplate("FoodTableRes.png", table.Name);
                        }
                        else
                        {
                            btn.Template = CreateTemplate("FoodTable.png", table.Name);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        #region Table Order

        private void ButtonTable_Click(object sender, RoutedEventArgs e)
        {
            CleanAllSelection();
            var btn = sender as Button;
            if (btn != null)
            {
                btn.Background = new SolidColorBrush(Colors.Brown);
            }

            var back = "" + (sender as Button).Content;
            if (back == "Back")
            {
                try
                {
                    var uc = new UCSale();
                    App.MainWindow.AddControlToMainCanvas(uc);
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _lastSelectedFoodOrderTable = (FoodTable)((Button)sender).DataContext;

                if (_lastSelectedFoodOrderTable.Id == 0)
                    this.DialogResult = false;
                else
                {
                    masterOrder = null;
                    SelectedTable = _lastSelectedFoodOrderTable;
                    IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
                    lblSelectedTable.Text = UI.OpenOrder_TableButton + _lastSelectedFoodOrderTable.Id;
                    presenter.HandleOrderByTableClick();
                    //this.DialogResult = true;
                }
            }

            /*var customer = (FoodTable)((Button)(sender)).DataContext;
            if (customer != null && customer.Id > 0)
            {
                masterOrder = null;
                SelectedTable = customer;
                lblSelectedTable.Text = UI.OpenOrder_TableButton + customer.Id;
                presenter.HandleOrderByTableClick();
            }*/
        }

        FoodTable _lastSelectedFoodOrderTable;

        private void ButtonTableProforma_Click(object sender, RoutedEventArgs e)
        {

            /*try
            {
                currentOrderMaster = (Order)(sender as Button).DataContext;
                if (currentOrderMaster != null)
                {
                    OrderId = currentOrderMaster.Id;
                    presenter.HandelPerformaClick(OrderId);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }*/

        }


        private void TableOrderCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                masterOrder = (Order)(sender as Button).DataContext;
                if (masterOrder != null)
                {
                    if (MessageBox.Show(UI.Message_CancelOrder, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        presenter.HandelOrderCancelClick(masterOrder.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (selectedOrders.Count > 1)
                {
                    if (MessageBox.Show(UI.OpenOrder_ConfirmMerg, UI.OpenOrder_Merge, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        presenter.HandelMergeOrderClick();
                    }
                }
                if (masterOrder != null)
                {
                    presenter.HandelCheckOutClick();

                }
                presenter.HandelTableClick();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }








        private void add_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orderItem = (OrderLine)((Button)sender).DataContext;
                // LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ItemDeleted), masterOrder.Id, orderItem.ItemId);
                currentOrderDetail.Remove(orderItem);

                currentOrderDetail = presenter.HandelDeleteItemClick(masterOrder.Id, orderItem.Id, currentOrderDetail);
                lvCart.SelectedIndex = -1;
                lvCart.ItemsSource = null;
                lvCart.ItemsSource = currentOrderDetail;
                if (currentOrderDetail.Count == 0)
                {
                    presenter.HandelTableClick();
                }
                else
                {
                    CalculatOrderTotal(currentOrderDetail);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }



        }
        #endregion

        private void dataGridTableOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);

                type = OrderType.TableOrder;

                currentOrderDetail = new List<OrderLine>();
                CalculatOrderTotal(currentOrderDetail);
                masterOrder = dataGridTableOrder.SelectedItem as Order;

                lvCart.ItemsSource = currentOrderDetail;
                lvCart.Items.Refresh();
                selectedOrders = new List<Order>();
                var data = dataGridTableOrder.SelectedItems;

                foreach (var item in data)
                {
                    var order = (Order)item;
                    masterOrder = order;
                    selectedOrders.Add(order);
                    var detail = orderRepo.GetOrderLinesById(order.Id);
                    currentOrderDetail.AddRange(detail);
                    // CalculatOrderTotal(currentOrderDetail);
                    lvCart.ItemsSource = currentOrderDetail;
                    lvCart.Items.Refresh();
                }

                if (data != null)
                    CalculatOrderTotal(currentOrderDetail); //Calculating the Total,vat, Net Total etc

                if (masterOrder != null)
                {
                    if (masterOrder.BalanceAmount > 0 && masterOrder.PartialPaidAmount > 0)
                    {
                        lblRemaining.Visibility = Visibility.Visible;
                        txtRemainingTotal.Visibility = Visibility.Visible;
                        txtRemainingTotal.Text = masterOrder.BalanceAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                    }
                    else
                    {
                        lblRemaining.Visibility = Visibility.Collapsed;
                        txtRemainingTotal.Visibility = Visibility.Collapsed;
                        txtRemainingTotal.Text = masterOrder.BalanceAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);

                    }
                    if (masterOrder.PartialPaidAmount > 0)
                    {
                        lblPaid.Visibility = Visibility.Visible;
                        txtPaidTotal.Text = masterOrder.PartialPaidAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                        txtPaidTotal.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblPaid.Visibility = Visibility.Collapsed;
                        txtPaidTotal.Text = masterOrder.PartialPaidAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                        txtPaidTotal.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {


            }
        }

        private void PendingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            masterOrder = PendingDataGrid.SelectedItem as Order;
            //if (masterOrder != null)
            //{
            //    type = masterOrder.Type;
            //}
        }

        private void SplitWholeOrder_Click(object sender, RoutedEventArgs e)
        {
            if (masterOrder != null)
            {
                PopupSplit.IsOpen = true;
                mainGrid.IsEnabled = false;
            }
        }

        private void popupSplit_Loaded(object sender, RoutedEventArgs e)
        {
            PopupSplit.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
        }

        private void ButtonNumber_Click(object sender, RoutedEventArgs e)
        {
            txtSplitNumber.Text = txtSplitNumber.Text + (sender as Button).Content;
        }

        private void SplitCancel_Click(object sender, RoutedEventArgs e)
        {
            txtSplitNumber.Text = "";
            PopupSplit.IsOpen = false;
        }

        private void ButtonSplittClear_Click(object sender, RoutedEventArgs e)
        {
            txtSplitNumber.Text = "";
        }

        private void SplitYesButton_Click(object sender, RoutedEventArgs e)
        {
            int number = 0;
            int.TryParse(txtSplitNumber.Text, out number);
            if (number == 0)
                MessageBox.Show(UI.Message_InvalidNumber, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
                presenter.SetHandelSplitWholeClick(number);



            /*Reload window again*/
            presenter = new OpenOrderPresenter(this);
            presenter.HandelPendingClick();

            //SetFloors(presenter.LoadFloor());
            //presenter.HandelTableClick();
            //GetSelectedColorBrush();


        }

        public void ShowError(string message, string title)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void PopupSplit_Closed(object sender, EventArgs e)
        {
            mainGrid.IsEnabled = true;
        }


        public int GetFloor()
        {
            return FloorId;
        }

        private void RenameTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedTable != null && SelectedTable.Id > 0 && masterOrder != null)
                {


                    var orderComments = Utilities.PromptInput(UI.Global_TableRename, UI.Global_Enter + " " + UI.Global_Name, "");
                    if (!string.IsNullOrEmpty(orderComments))
                    {
                        var table = SelectedTable;
                        presenter.RenameOrderTable(table.Id, orderComments);
                        presenter.HandelTableClick();
                        masterOrder = null;
                        SelectedTable = table;
                        presenter.HandleOrderByTableClick();

                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void PendingGenerateReceipt_Click(object sender, RoutedEventArgs e)
        {

            var MasterOrder = (sender as Button).DataContext as Order;
            OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
            Receipt receipt = orderRepo.GetOrderReceipt(MasterOrder.Id);
            bool bRes = new InvoiceHandler().ReGenerateInvoice(MasterOrder.Id, Defaults.User.Id, receipt);

            if (bRes)
            {
                // Transaction complete, kick drawer
                try
                {

                    Defaults.PerformanceLog.Add("Sending To Invoice Print      -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                    //  var invoiceWindow = new PrintNewInvoiceWindow();
                    DirectPrint directPrint = new DirectPrint();
                    directPrint.PrintReceipt(MasterOrder.Id, false, bRes);

                    if (type == OrderType.Return)
                    {
                        //  invoiceWindow.PrintReturnInvoice(orderId);
                        LogWriter.JournalLog(
                            Defaults.User.TrainingMode
                                ? Convert.ToInt16(JournalActionCode.ReceiptPrintedForReturnOrderViaTrainingMode)
                                : Convert.ToInt16(JournalActionCode.ReceiptPrintedForReturnOrder),
                            MasterOrder.Id);
                    }
                    else
                    {
                        //   invoiceWindow.PrintInvoice(orderId, false);
                        LogWriter.JournalLog(
                            Defaults.User.TrainingMode
                                ? Convert.ToInt16(JournalActionCode.ReceiptPrintedViaTrainingMode)
                                : Convert.ToInt16(JournalActionCode.ReceiptPrinted),
                            MasterOrder.Id);
                    }
                    presenter.HandelPendingClick();

                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    App.MainWindow.ShowError(UI.Global_Error, ex.Message);
                }

            }
            else
            {
                App.MainWindow.ShowError(UI.Global_Error, "Receipt Faild");
            }
        }

        private void btnUseThisTable_Click(object sender, RoutedEventArgs e)
        {
            SelectedTable = _lastSelectedFoodOrderTable;
            IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
            if (_lastSelectedFoodOrderTable != null)
            {
                lblSelectedTable.Text = UI.OpenOrder_TableButton + _lastSelectedFoodOrderTable.Id;
            }

            presenter.HandleOrderByTableClick();
            this.DialogResult = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}