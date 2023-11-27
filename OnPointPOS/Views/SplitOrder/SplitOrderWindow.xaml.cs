using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using POSSUM.Model;
using POSSUM.Presenters.SplitOrder;
using POSSUM.Res;
using POSSUM.Views.Customers;
using POSSUM.Views.FoodTables;

namespace POSSUM.Views.SplitOrder
{
    /// </summary>
    public partial class SplitOrderWindow : Window, ISplitOrderView
    {
        private bool isNewOrder;
        private Guid NewOrderId = default(Guid);
        private decimal NewOrderTotal;
        private decimal OldOrderTotal;

        private OrderLine selectedItem;

        private OrderType type = OrderType.Standard;

        public SplitOrderWindow(Order order)
        {
            InitializeComponent();
            MasterOrder = order;
            LeftOrderLines = new List<OrderLine>();
            RightOrderLines = new List<OrderLine>();
            Presenter = new SplitOrderPresenter(this);
            Presenter.HandelOrderDetailClick();
            lvNewCart.ItemsSource = RightOrderLines;
            type = MasterOrder.Type;
            // lblBongNumber1.Text = "Bong # " + order.Bong;
            // lblBongNumber2.Text = "Bong # " + order.Bong;
        }

        public SplitOrderWindow(Order order, List<OrderLine> orderLines)
        {
            InitializeComponent();
            this.MasterOrder = order;
            if (order.SelectedTable != null)
                SelectedTable = new FoodTable
                {
                    Id = order.SelectedTable.Id,
                    Name = order.SelectedTable.Name
                };
            LeftOrderLines = new List<OrderLine>();
            RightOrderLines = new List<OrderLine>();
            Presenter = new SplitOrderPresenter(this);
            SetOrderDetailResult(orderLines);
            // Presenter.HandelOrderDetailClick();
            lvNewCart.ItemsSource = RightOrderLines;
            type = MasterOrder.Type;

        }


        public SplitOrderPresenter Presenter { get; set; }
        public Order MasterOrder { get; set; }
        public FoodTable SelectedTable { get; set; }
        public List<OrderLine> LeftOrderLines { get; set; }
        public List<OrderLine> RightOrderLines { get; set; }

        public void SetOrderDetailResult(List<OrderLine> lst)
        {
            LeftOrderLines = lst;
            lvCart.ItemsSource = LeftOrderLines;
            CalculatLeftTotal(LeftOrderLines);
        }

        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void CalculatLeftTotal(IList<OrderLine> list)
        {
            OldOrderTotal = 0;
            decimal tax = 0;
            foreach (var item in list)
            {
                if (item.IsValid)
                {
                    if (item.Product.ItemType == ItemType.Individual)
                        tax += item.VatAmount();
                    OldOrderTotal += item.GrossAmountDiscounted();
                    if (item.IngredientItems != null)
                        foreach (var ingredient in item.IngredientItems)
                        {
                            tax += ingredient.VatAmount();
                            OldOrderTotal += ingredient.GrossAmountDiscounted();
                        }
                }
            }
            foreach (var detail in list.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
            {
                // var vatInnerGroups = detail.OrderItemDetails.GroupBy(od => od.TAX);
                if (detail.ItemDetails != null)
                    tax += detail.ItemDetails.Sum(itm => itm.VatAmount());
            }
            decimal nettotal = OldOrderTotal - tax;
            lblGrossTotal.Dispatcher.BeginInvoke(
                new Action(
                    () => lblGrossTotal.Text = nettotal.ToString("C", Defaults.UICultureInfo)));
            lblVatTotal.Dispatcher.BeginInvoke(
                new Action(() => lblVatTotal.Text = tax.ToString("C", Defaults.UICultureInfo)));
            lblTotal.Dispatcher.BeginInvoke(
                new Action(
                    () => lblTotal.Text = OldOrderTotal.ToString("C", Defaults.UICultureInfo)));
        }

        public void CalculatRightTotal(IList<OrderLine> list)
        {
            NewOrderTotal = 0;
            decimal tax = 0;
            foreach (var item in list)
            {
                if (item.IsValid)
                {
                    if (item.Product.ItemType == ItemType.Individual)
                        tax += item.VatAmount();
                    NewOrderTotal += item.GrossAmountDiscounted();
                    if (item.IngredientItems != null)
                        foreach (var ingredient in item.IngredientItems)
                        {
                            tax += ingredient.VatAmount();
                            NewOrderTotal += ingredient.GrossAmountDiscounted();
                        }
                }
            }
            foreach (var detail in list.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
            {
                // var vatInnerGroups = detail.OrderItemDetails.GroupBy(od => od.TAX);
                if (detail.ItemDetails != null)
                    tax += detail.ItemDetails.Sum(itm => itm.VatAmount());
            }
            decimal nettotal = NewOrderTotal - tax;

            lblGrossTotal.Dispatcher.BeginInvoke(
                new Action(
                    () => lblRGrossTotal.Text = nettotal.ToString("C", Defaults.UICultureInfo)));
            lblVatTotal.Dispatcher.BeginInvoke(
                new Action(() => lblRVatTotal.Text = tax.ToString("C", Defaults.UICultureInfo)));
            lblTotal.Dispatcher.BeginInvoke(
                new Action(
                    () => lblRTotal.Text = NewOrderTotal.ToString("C", Defaults.UICultureInfo)));
        }

        public Order GetCurrentOrder()
        {
            return MasterOrder;
        }

        public List<OrderLine> GetOldOrderDetail()
        {
            return LeftOrderLines;
        }

        public List<OrderLine> GetNewOrderDetail()
        {
            return RightOrderLines;
        }

        public Guid GetOrderId()
        {
            return MasterOrder.Id;
        }

        public Guid GetNewOrderId()
        {
            return NewOrderId;
        }

        public void SetNewOrderId(Guid id)
        {
            NewOrderId = id;
        }

        public decimal GetNewOrderTotal()
        {
            return NewOrderTotal;
        }

        public decimal GetOldOrderTotal()
        {
            return OldOrderTotal;
        }

        public FoodTable GetSelectedCustomer()
        {
            return SelectedTable == null ? new FoodTable() : SelectedTable;
        }

        public void OrderSaveCompleted(bool res)
        {
            type = MasterOrder.Type;
            if (res)
            {
                if (OldOrderTotal == 0)
                {
                    try
                    {
                        DialogResult = true;
                    }
                    catch (Exception) {
                    }
                }
                else
                {
                    RightOrderLines = new List<OrderLine>();
                    lvNewCart.ItemsSource = null;
                    lvNewCart.ItemsSource = RightOrderLines;
                    lvNewCart.Items.Refresh();
                    CalculatRightTotal(RightOrderLines);
                    if (OldOrderTotal > 0 == false)
                        DialogResult = true;
                    SelectTableButton.Content = UI.PlaceOrder_SelectTableButton;
                    SelectedTable = new FoodTable();
                }
            }
            else
            {
                RightOrderLines = new List<OrderLine>();
                lvNewCart.ItemsSource = null;
                lvNewCart.ItemsSource = RightOrderLines;
                lvNewCart.Items.Refresh();
                CalculatRightTotal(RightOrderLines);
            }
        }

        public OrderType GetOrderType()
        {
            return type;
        }

        public bool IsNewOrder()
        {
            return isNewOrder;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MoveRightButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lvCart.SelectedIndex;

            if (lvCart.SelectedItems != null && lvCart.SelectedItems.Count > 0)
            {
                if (leftSelectedOrderLines == null)
                    leftSelectedOrderLines = new List<OrderLine>();
                foreach (var item in lvCart.SelectedItems)
                {
                    var d = item as OrderLine;
                    leftSelectedOrderLines.Add(d);
                }
            }
            if (leftSelectedOrderLines != null && leftSelectedOrderLines.Count > 0)
            {
                foreach (var item in leftSelectedOrderLines)
                {
                    decimal discount = 0;

                    var d = item as OrderLine;
                    if (d.Quantity >= 1 && d.Product.Unit == ProductUnit.Piece)
                    {
                        if (d.ItemDiscount > 0)//
                        {
                            discount = d.ItemDiscount / d.Quantity;
                            d.ItemDiscount = d.ItemDiscount - discount;
                        }

                        d.Quantity = d.Quantity - 1;


                        if (d.IngredientItems != null && d.IngredientItems.Count > 0)
                        {
                            foreach (var it in d.IngredientItems)// Decreasing Ingredients
                            {
                                it.Quantity = it.Quantity - 1;
                            }
                        }


                        if (d.ItemDetails != null && d.ItemDetails.Count > 0)
                        {
                            foreach (var detail in d.ItemDetails)
                            {
                                detail.Quantity = d.Quantity;

                            }
                        }
                        AddItemToRighCart(d, 1, discount);
                    }
                    else
                    {
                        AddItemToRighCart(d, d.Quantity, 0);
                        if (d.Quantity <= 1)
                            LeftOrderLines.Remove(d);
                    }

                    if (d.Quantity == 0 || d.Product.Unit != ProductUnit.Piece)
                        LeftOrderLines.Remove(d);
                    else
                        d.IsSelected = false;
                }

                lvCart.ItemsSource = null;
                lvCart.ItemsSource = LeftOrderLines;
                lvCart.Items.Refresh();
                CalculatLeftTotal(LeftOrderLines);
                leftSelectedOrderLines = new List<OrderLine>();
                if (lvCart.Items.Count > 0)
                {
                    // lvCart.SelectedIndex = lvCart.Items.Count - 1 == selectedIndex ? selectedIndex : selectedIndex - 1;
                }
            }
        }


        private void AddItemToRighCart(OrderLine orderLine, decimal qty, decimal discount)
        {
            try
            {

                var orderDetail = RightOrderLines.FirstOrDefault(p => p.Id == orderLine.Id);
                if (orderDetail != null)
                {
                    var quantity = orderDetail.Quantity;
                    quantity = quantity + qty;
                    var grossTotal = (quantity * orderDetail.UnitPrice);
                    orderDetail.Quantity = quantity;

                    orderDetail.ItemDiscount = orderDetail.ItemDiscount + discount;
                    //if (orderDetail.ItemDiscount > 0)
                    //{
                    //    var disc = orderDetail.ItemDiscount / qty;
                    //    orderDetail.ItemDiscount = disc;
                    //}

                    if (orderDetail.IngredientItems != null && orderDetail.IngredientItems.Count > 0) // ingredients
                    {
                        foreach (var detail in orderDetail.IngredientItems)
                        {
                            detail.Quantity = quantity;
                        }
                    }

                    if (orderDetail.ItemDetails != null && orderDetail.ItemDetails.Count > 0)
                    {
                        foreach (var detail in orderDetail.ItemDetails)
                        {
                            detail.Quantity = quantity;
                        }
                    }

                }
                else
                {
                    var orderItem = new OrderLine
                    {
                        Id = orderLine.Id,
                        UnitPrice = orderLine.UnitPrice,
                        TaxPercent = orderLine.TaxPercent,
                        ItemId = orderLine.ItemId,
                        Product = orderLine.Product,
                        Quantity = qty,

                        PrinterId = orderLine.PrinterId,
                        OrderId = orderLine.OrderId,
                        Active = orderLine.Active,
                        PurchasePrice = orderLine.PurchasePrice,
                        ItemStatus = orderLine.ItemStatus,
                        ItemDiscount = discount == 0 ? orderLine.ItemDiscount : discount,
                        DiscountDescription = orderLine.DiscountDescription,

                        BIDS = orderLine.BIDS,
                        BIDSNO = orderLine.BIDSNO,
                        Direction = orderLine.Direction,
                        DiscountedUnitPrice = orderLine.DiscountedUnitPrice,
                        DiscountPercentage = orderLine.DiscountPercentage,
                        IsCoupon = orderLine.IsCoupon,
                        Percentage = orderLine.Percentage,
                        OrderType = orderLine.OrderType,

                        ItemType = orderLine.ItemType,
                        GroupId = orderLine.GroupId
                    };
                    if (orderLine.ItemDetails != null)
                    {
                        var detailItems = orderLine.ItemDetails.Select(od => new OrderLine
                        {
                            Id = od.Id,
                            UnitPrice = od.UnitPrice,
                            TaxPercent = od.TaxPercent,
                            ItemId = od.ItemId,
                            Product = od.Product,
                            Quantity = qty,
                            PrinterId = od.PrinterId,
                            OrderId = od.OrderId,
                            Active = od.Active,
                            PurchasePrice = od.PurchasePrice,
                            ItemStatus = od.ItemStatus,
                            ItemDiscount = od.ItemDiscount,
                            DiscountDescription = od.DiscountDescription,
                            BIDS = od.BIDS,
                            BIDSNO = od.BIDSNO,
                            Direction = od.Direction,
                            DiscountedUnitPrice = od.DiscountedUnitPrice,
                            DiscountPercentage = od.DiscountPercentage,
                            IsCoupon = od.IsCoupon,
                            Percentage = od.Percentage,
                            OrderType = od.OrderType,

                            ItemType = od.ItemType,
                            GroupId = od.GroupId
                        }).ToList();

                        orderItem.ItemDetails = detailItems;
                    }

                    if (orderLine.IngredientItems != null && orderLine.IngredientItems.Count > 0)
                    {
                        var detailItems = orderLine.IngredientItems.Select(od => new OrderLine
                        {
                            Id = od.Id,
                            UnitPrice = od.UnitPrice,
                            TaxPercent = od.TaxPercent,
                            ItemId = od.ItemId,
                            Product = od.Product,
                            Quantity = qty,
                            PrinterId = od.PrinterId,
                            OrderId = od.OrderId,
                            Active = od.Active,
                            PurchasePrice = od.PurchasePrice,
                            ItemStatus = od.ItemStatus,
                            ItemDiscount = od.ItemDiscount,
                            DiscountDescription = od.DiscountDescription,
                            BIDS = od.BIDS,
                            BIDSNO = od.BIDSNO,
                            Direction = od.Direction,
                            DiscountedUnitPrice = od.DiscountedUnitPrice,
                            DiscountPercentage = od.DiscountPercentage,
                            IsCoupon = od.IsCoupon,
                            Percentage = od.Percentage,
                            OrderType = od.OrderType,

                            ItemType = od.ItemType,
                            GroupId = od.GroupId,
                            IngredientMode = od.UnitPrice == 0 ? "-" : "+"
                        }).ToList();

                        orderItem.IngredientItems = detailItems;
                    }

                    RightOrderLines.Add(orderItem);
                }
                //lvNewCart.ItemsSource = null;
                //lvNewCart.ItemsSource = RightOrderLines;
                lvNewCart.Items.Refresh();

                //lvNewCart.Items.Refresh();
                CalculatRightTotal(RightOrderLines);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MoveLefButton_Click(object sender, RoutedEventArgs e)
        {

            if (lvNewCart.SelectedItems != null && lvNewCart.SelectedItems.Count > 0)
            {
                foreach (var item in lvNewCart.SelectedItems)
                {
                    var d = item as OrderLine;
                    rightSelectedOrderLines.Add(d);
                }


            }
            if (rightSelectedOrderLines != null && rightSelectedOrderLines.Count > 0)
            {
                foreach (var item in rightSelectedOrderLines)
                {
                    decimal discount = 0;

                    var d = item as OrderLine;
                    if (d.Quantity >= 1 && d.Product.Unit == ProductUnit.Piece)
                    {
                        if (d.ItemDiscount > 0)//munir
                        {
                            discount = d.ItemDiscount / d.Quantity;
                            d.ItemDiscount = d.ItemDiscount - discount;
                        }

                        d.Quantity = d.Quantity - 1;

                        if (d.IngredientItems != null && d.IngredientItems.Count > 0)
                        {
                            foreach (var it in d.IngredientItems)// Decreasing Ingredients
                            {
                                it.Quantity = it.Quantity - 1;
                            }
                        }

                        if (d.ItemDetails != null && d.ItemDetails.Count > 0)
                        {
                            foreach (var detail in d.ItemDetails)
                            {
                                detail.Quantity = d.Quantity;

                            }
                        }
                        AddItemToLeftCart(d, 1, discount);
                    }
                    else
                    {
                        AddItemToLeftCart(d, d.Quantity, discount);
                        if (d.Quantity <= 1)
                            RightOrderLines.Remove(d);
                    }
                    if (d.Quantity == 0 || d.Product.Unit != ProductUnit.Piece)
                        RightOrderLines.Remove(d);
                    else
                        d.IsSelected = false;
                }
                lvNewCart.ItemsSource = null;
                lvNewCart.ItemsSource = RightOrderLines;
                lvNewCart.Items.Refresh();
                CalculatRightTotal(RightOrderLines);
                rightSelectedOrderLines = new List<OrderLine>();
                lvCart.Items.Refresh();
            }
        }
        private void AddItemToLeftCart(OrderLine orderLine, decimal qty, decimal discount)
        {
            try
            {
                var orderDetail = LeftOrderLines.FirstOrDefault(p => p.Id == orderLine.Id);
                if (orderDetail != null)
                {
                    var quantity = orderDetail.Quantity;
                    quantity = quantity + qty;
                    var grossTotal = quantity * orderDetail.UnitPrice;
                    orderDetail.Quantity = quantity;

                    orderDetail.ItemDiscount = orderDetail.ItemDiscount + discount;


                    if (orderDetail.IngredientItems != null && orderDetail.IngredientItems.Count > 0) // ingredients
                    {
                        foreach (var detail in orderDetail.IngredientItems)
                        {
                            detail.Quantity = detail.Quantity + qty;// quantity;
                        }
                    }

                    if (orderDetail.ItemDetails != null && orderDetail.ItemDetails.Count > 0)
                    {
                        foreach (var detail in orderDetail.ItemDetails)
                        {
                            detail.Quantity = quantity;


                        }
                    }


                }
                else
                {
                    var orderItem = new OrderLine
                    {
                        Id = orderLine.Id,
                        UnitPrice = orderLine.UnitPrice,
                        TaxPercent = orderLine.TaxPercent,
                        ItemId = orderLine.ItemId,
                        Product = orderLine.Product,
                        Quantity = qty,
                        PrinterId = orderLine.PrinterId,
                        OrderId = orderLine.OrderId,
                        Active = orderLine.Active,
                        PurchasePrice = orderLine.PurchasePrice,
                        ItemStatus = orderLine.ItemStatus,
                        BIDS = orderLine.BIDS,
                        BIDSNO = orderLine.BIDSNO,
                        Direction = orderLine.Direction,
                        DiscountedUnitPrice = orderLine.DiscountedUnitPrice,
                        DiscountPercentage = orderLine.DiscountPercentage,
                        IsCoupon = orderLine.IsCoupon,
                        Percentage = orderLine.Percentage,
                        OrderType = orderLine.OrderType,
                        ItemDiscount = discount == 0 ? orderLine.ItemDiscount : discount,
                        DiscountDescription = orderLine.DiscountDescription,

                    };
                    if (orderLine.ItemDetails != null)
                    {
                        var detailItems = orderLine.ItemDetails.Select(od => new OrderLine
                        {
                            Id = od.Id,
                            UnitPrice = od.UnitPrice,
                            TaxPercent = od.TaxPercent,
                            ItemId = od.ItemId,
                            Product = od.Product,
                            Quantity = qty,
                            PrinterId = od.PrinterId,
                            OrderId = od.OrderId,
                            Active = od.Active,
                            PurchasePrice = od.PurchasePrice,
                            ItemStatus = od.ItemStatus,
                            ItemDiscount = od.ItemDiscount,
                            DiscountDescription = od.DiscountDescription,
                            BIDS = od.BIDS,
                            BIDSNO = od.BIDSNO,
                            Direction = od.Direction,
                            DiscountedUnitPrice = od.DiscountedUnitPrice,
                            DiscountPercentage = od.DiscountPercentage,
                            IsCoupon = od.IsCoupon,
                            Percentage = od.Percentage,
                            OrderType = od.OrderType,

                            ItemType = od.ItemType,
                            GroupId = od.GroupId
                        }).ToList();

                        orderItem.ItemDetails = detailItems;
                    }

                    if (orderLine.IngredientItems != null)
                    {
                        var detailItems = orderLine.IngredientItems.Select(od => new OrderLine
                        {
                            Id = od.Id,
                            UnitPrice = od.UnitPrice,
                            TaxPercent = od.TaxPercent,
                            ItemId = od.ItemId,
                            Product = od.Product,
                            Quantity = qty,
                            PrinterId = od.PrinterId,
                            OrderId = od.OrderId,
                            Active = od.Active,
                            PurchasePrice = od.PurchasePrice,
                            ItemStatus = od.ItemStatus,
                            ItemDiscount = od.ItemDiscount,
                            DiscountDescription = od.DiscountDescription,
                            BIDS = od.BIDS,
                            BIDSNO = od.BIDSNO,
                            Direction = od.Direction,
                            DiscountedUnitPrice = od.DiscountedUnitPrice,
                            DiscountPercentage = od.DiscountPercentage,
                            IsCoupon = od.IsCoupon,
                            Percentage = od.Percentage,
                            OrderType = od.OrderType,

                            ItemType = od.ItemType,
                            GroupId = od.GroupId,
                            IngredientMode = od.UnitPrice == 0 ? "-" : "+"
                        }).ToList();

                        orderItem.IngredientItems = detailItems;
                    }
                    //if (orderLine.ItemDetails != null)
                    //{
                    //    var detailItems = orderLine.ItemDetails.ToList();

                    //    orderItem.ItemDetails = detailItems;
                    //}

                    //if (orderLine.IngredientItems != null)
                    //{

                    //    foreach (var d in orderLine.IngredientItems)
                    //    {
                    //        d.Quantity = d.Quantity + qty;
                    //    }

                    //    var ingredients = orderLine.IngredientItems.ToList();

                    //    orderItem.IngredientItems = ingredients;
                    //}
                    LeftOrderLines.Add(orderItem);
                }

                lvCart.Items.Refresh();
                CalculatLeftTotal(LeftOrderLines);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightOrderLines.Count > 0)
            {
                var tableWindow = new TableWindow();
                if (tableWindow.ShowDialog() == true)
                {
                    SelectedTable = tableWindow.SelectedTable;
                    SelectTableButton.Content = tableWindow.SelectedTable.Name;
                    isNewOrder = tableWindow.IsNewOrder;
                    if (isNewOrder)
                    {
                        type = OrderType.TableOrder;
                        Presenter.HandelSplitTableOrderClick();
                    }
                    else
                    {
                        List<Order> orders = Presenter.GetOpenOrdersOnTable();
                        if (orders.Count > 0)
                        {
                            dataGridTableOrder.ItemsSource = orders;

                            if (MasterOrder != null && MasterOrder.Comments == SelectedTable.Name)
                            {
                                datatableColumnMergeButton.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                datatableColumnMergeButton.Visibility = Visibility.Visible;
                            }

                            PopupTable.IsOpen = true;
                            mainGrid.IsEnabled = false;
                        }
                        else
                        {
                            type = OrderType.TableOrder;
                            Presenter.HandelSplitTableOrderClick();
                        }
                    }
                }
            }
        }

        public void CloseWindow()
        {
            DialogResult = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CheckoutNewButton_Click(object sender, RoutedEventArgs e)
        {
            if (MasterOrder != null && RightOrderLines.Count > 0)
            {
                Presenter.HandelSplitOrderClick();
            }
            else
            {
                if (LeftOrderLines.Count > 0)
                    Presenter.HandelCheckoutOrderClick();
            }
        }

        private void popupTable_Loaded(object sender, RoutedEventArgs e)
        {
            PopupTable.Placement = PlacementMode.Center;
        }

        private void TableOrderMergTo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var order = button.DataContext as Order;
                if (order != null)
                {
                    PopupTable.IsOpen = false;
                    type = OrderType.TableOrder;
                    Presenter.HandelMergeTableOrderClick(order);
                }
            }
        }

        private void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
            isNewOrder = true;
            type = OrderType.TableOrder;
            Presenter.HandelSplitTableOrderClick();
        }

        private void CancelPopupButton_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
            SelectedTable = new FoodTable();
            SelectTableButton.Content = UI.PlaceOrder_SelectTableButton;
        }

        private void SplitItemButton_Click(object sender, RoutedEventArgs e)
        {
            //var lines = lvCart.ItemsSource as List<OrderLine>;
            //if (lines != null)
            //{
            //    leftSelectedOrderLines = lines.Where(l => l.IsSelected).ToList();
            //}

            //if (leftSelectedOrderLines.Count > 0)
            //{
            //    selectedItem = leftSelectedOrderLines[0] as OrderLine;
            //    PopupSplit.IsOpen = true;
            //    mainGrid.IsEnabled = false;
            //}

            if (lvCart.SelectedItems.Count > 0)
            {
                selectedItem = lvCart.SelectedItems[0] as OrderLine;
                PopupSplit.IsOpen = true;
                mainGrid.IsEnabled = false;
            }
        }

        private void popupSplit_Loaded(object sender, RoutedEventArgs e)
        {
            PopupSplit.Placement = PlacementMode.Center;
        }

        private void SplitYesButton_Click(object sender, RoutedEventArgs e)
        {
            int number = 0;
            int.TryParse(txtSplitNumber.Text, out number);
            if (number == 0)
                MessageBox.Show(UI.Message_InvalidNumber, Defaults.AppProvider.AppTitle, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            else
            {
                bool res = Presenter.HandelSplitItemClick(selectedItem, number);
                if (res)
                {
                    leftSelectedOrderLines = new List<OrderLine>();
                    Presenter.HandelOrderDetailClick();
                    selectedItem = null;
                    PopupSplit.IsOpen = false;
                    txtSplitNumber.Text = "";
                }
            }
        }

        private void ButtonNumber_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null) txtSplitNumber.Text = txtSplitNumber.Text + button.Content;
        }

        private void ButtonSplittClear_Click(object sender, RoutedEventArgs e)
        {
            txtSplitNumber.Text = "";
        }

        private void SplitCancel_Click(object sender, RoutedEventArgs e)
        {
            selectedItem = null;
            PopupSplit.IsOpen = false;
            txtSplitNumber.Text = "";
        }

        private void MergItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeftOrderLines.Count > 1)
            {
                try
                {
                    var myStoreList = LeftOrderLines.GroupBy(g => new { g.ItemId, g.UnitPrice }).ToList();
                    var groups = myStoreList.Where(c => c.Count() > 1).ToList();
                    if (groups.Count > 0)
                    {
                        if (
                            MessageBox.Show(UI.Message_ConfirmMergItem, UI.Global_Confirm, MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            foreach (var group in groups)
                            {
                                var item = group.First();
                                var items = LeftOrderLines.Where(ol => ol.ItemId == item.ItemId && ol.UnitPrice == item.UnitPrice).ToList();

                                Presenter.MergeItems(items);
                                Presenter.MergeIngredientItems(item);
                            }
                            Presenter.HandelOrderDetailClick();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK);
                }
            }
        }

        private void PopupTable_Closed(object sender, EventArgs e)
        {
            mainGrid.IsEnabled = true;
        }

        private void PopupSplit_Closed(object sender, EventArgs e)
        {
            mainGrid.IsEnabled = true;
        }

        private void PerformaButton_Click(object sender, RoutedEventArgs e)
        {
            if (MasterOrder != null && RightOrderLines.Count > 0)
            {
                var sumAmt = RightOrderLines.Sum(s => s.GrossTotal);
                var orderviewModel = new Order
                {
                    TableId = MasterOrder.TableId,
                    Comments = MasterOrder.Comments,
                    OrderComments = MasterOrder.OrderComments,
                    CreationDate = DateTime.Now,
                    SelectedTable = MasterOrder.SelectedTable,
                    TableName = MasterOrder.TableName,
                    OrderTotal = sumAmt,
                    OrderLines = RightOrderLines
                };
                DirectPrint directPrint = new DirectPrint();
                directPrint.PrintPerfroma(orderviewModel);
            }
        }

        private void ParkeraButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MasterOrder != null && RightOrderLines.Count > 0)
                {
                    decimal orderTotal = RightOrderLines.Sum(s => s.GrossTotal);
                    foreach (var item in RightOrderLines)
                    {
                        if (item.IngredientItems!=null && item.IngredientItems.Count > 0)
                        {
                            foreach (var ingredient in item.IngredientItems)
                            {
                                orderTotal = orderTotal + ingredient.GrossTotal;
                            }
                        }
                    }

                    var order = new Order
                    {
                        CreationDate = DateTime.Now,
                        UserId = Defaults.User.Id,
                        OutletId = Defaults.Outlet.Id,
                        TerminalId = Defaults.Terminal.Id,
                        TableId = MasterOrder.TableId,
                        OrderTotal = orderTotal,
                        Status = MasterOrder.Status,
                        Bong= MasterOrder.Bong,
                        DailyBong=MasterOrder.DailyBong,
                        //  Synced = false,
                        PaymentStatus = MasterOrder.PaymentStatus,
                        Comments = MasterOrder.Comments,
                        OrderComments = MasterOrder.OrderComments,
                        Type = MasterOrder.Type
                    };
                    Presenter.HandelParkeraClick(order);
                }
            }
            catch (Exception ex)
            {
                ShowError(UI.Global_Error, ex.Message);
            }
        }
        List<OrderLine> leftSelectedOrderLines = new List<OrderLine>();
        private void LeftTuggle_Click(object sender, RoutedEventArgs e)
        {
            var orderLine = (sender as Button).DataContext as OrderLine;
            if (orderLine != null)
            {

                if (leftSelectedOrderLines == null)
                    leftSelectedOrderLines = new List<OrderLine>();
                if (orderLine.IsSelected)
                {
                    orderLine.IsSelected = false;
                    leftSelectedOrderLines.Remove(orderLine);
                }
                else
                {

                    orderLine.IsSelected = true;
                    leftSelectedOrderLines.Add(orderLine);

                }

                lvCart.Items.Refresh();
            }

        }
        List<OrderLine> rightSelectedOrderLines = new List<OrderLine>();
        private void ReightTuggle_Click(object sender, RoutedEventArgs e)
        {
            var orderLine = (sender as Button).DataContext as OrderLine;
            if (orderLine != null)
            {

                if (rightSelectedOrderLines == null)
                    rightSelectedOrderLines = new List<OrderLine>();
                if (orderLine.IsSelected)
                {
                    orderLine.IsSelected = false;
                    rightSelectedOrderLines.Remove(orderLine);
                }
                else
                {

                    orderLine.IsSelected = true;
                    rightSelectedOrderLines.Add(orderLine);

                }

                lvNewCart.Items.Refresh();
            }
        }

        private void OldPerformaButton_Click(object sender, RoutedEventArgs e)
        {
            if (MasterOrder != null && leftSelectedOrderLines.Count > 0)
            {
                var sumAmt = leftSelectedOrderLines.Sum(s => s.GrossTotal);
                var orderviewModel = new Order
                {
                    TableId = MasterOrder.TableId,
                    Comments = MasterOrder.Comments,
                    OrderComments = MasterOrder.OrderComments,
                    CreationDate = DateTime.Now,
                    SelectedTable = MasterOrder.SelectedTable,
                    TableName = MasterOrder.TableName,
                    OrderTotal = sumAmt,
                    OrderLines = leftSelectedOrderLines
                };
                DirectPrint directPrint = new DirectPrint();
                directPrint.PrintPerfroma(orderviewModel);
            }
        }

        private void CheckoutOldButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LeftCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var line = (sender as CheckBox).DataContext as OrderLine;
            if (line != null)
                line.IsSelected = true;
        }

        private void LeftCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            var line = (sender as CheckBox).DataContext as OrderLine;
            if (line != null)
                line.IsSelected = false;
        }

        private void RightCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var line = (sender as CheckBox).DataContext as OrderLine;
            if (line != null)
                line.IsSelected = true;
        }

        private void RightCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            var line = (sender as CheckBox).DataContext as OrderLine;
            if (line != null)
                line.IsSelected = false;
        }
    }
}