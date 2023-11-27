using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using POSSUM.Model;
using POSSUM.Presenters.ReturnOrder;
using System.Windows.Controls;

namespace POSSUM.Views.Sales
{
    /// <summary>
    /// Interaction logic for SplitOrderWindow.xaml
    /// </summary>
    public partial class ReturnOrderWindow : Window, IReturnOrderView
    {
        Guid NewOrderId = default(Guid);
        public ReturnOrderPresenter Presenter { get; set; }
        public Order MasterOrder { get; set; }
        public FoodTable SelectedTable { get; set; }

        OrderType type = OrderType.Standard;
        public List<OrderLine> LeftOrderLines { get; set; }
        public List<OrderLine> RightOrderLines { get; set; }

        decimal OldOrderTotal = 0;
        decimal NewOrderTotal = 0;

        public ReturnOrderWindow(Order order)
        {
            InitializeComponent();
            this.MasterOrder = order;
            LeftOrderLines = new List<OrderLine>();
            RightOrderLines = new List<OrderLine>();
            Presenter = new ReturnOrderPresenter(this);
            Presenter.HandelOrderDetailClick();
            lvNewCart.ItemsSource = RightOrderLines;
            type = MasterOrder.Type;
        }
        public ReturnOrderWindow(Order order, List<OrderLine> orderLines)
        {
            InitializeComponent();
            this.MasterOrder = order;
            //if (order.SelectedTable != null)
            //    SelectedTable = new FoodTable
            //    {
            //        Id = order.SelectedTable.Id,
            //        Name = order.SelectedTable.Name
            //    };
            LeftOrderLines = new List<OrderLine>();
            RightOrderLines = new List<OrderLine>();
           Presenter = new ReturnOrderPresenter(this);
            //SetOrderDetailResult(orderLines);
            
            lvNewCart.ItemsSource = RightOrderLines;
            type = MasterOrder.Type;

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        List<OrderLine> leftSelectedOrderLines = new List<OrderLine>();

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
                    }
                    if (d.Quantity < 1 || d.Product.Unit != ProductUnit.Piece)
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
                        ItemDiscount = discount,//orderLine.ItemDiscount,//!=0? orderLine.ItemDiscount / orderLine.Quantity+qty:0,
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
                        ItemDiscount = discount,// //orderLine.ItemDiscount,
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


        List<OrderLine> rightSelectedOrderLines = new List<OrderLine>();

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
                    }
                    if (d.Quantity < 1 || d.Product.Unit != ProductUnit.Piece)
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

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
        public List<OrderLine> GetReturnOrderDetail()
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
        public decimal GetReturnOrderTotal()
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
            try
            {
                type = MasterOrder.Type;
                if (res)
                {

                    if (OldOrderTotal == 0)
                        this.DialogResult = true;
                    else
                    {
                        RightOrderLines = new List<OrderLine>();
                        lvNewCart.ItemsSource = null;
                        lvNewCart.ItemsSource = RightOrderLines;
                        lvNewCart.Items.Refresh();
                        CalculatRightTotal(RightOrderLines);

                        this.DialogResult = true;

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
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void CheckoutNewButton_Click(object sender, RoutedEventArgs e)
        {
            if (MasterOrder != null && RightOrderLines.Count > 0)
            {
                var orderTotal = Convert.ToDecimal(lblRTotal.Text);
                Presenter.HandelCheckoutOrderClick(orderTotal);
            }
        }

        public OrderType GetOrderType()
        {
            return type;
        }
    }

}