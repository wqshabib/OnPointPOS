using POSSUM.Model;
using POSSUM.Presenters.Sales;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for UCItemCart.xaml
    /// </summary>
   /* public partial class UCItemCart : UserControl
    {
        public SaleOrderPresenter Presenter { get; set; }
        public OrderLine SelectedItem { get; set; }
        ObservableCollection<OrderLine> Items = new ObservableCollection<OrderLine>();
        int addCartIndex = -1;
        bool TimeSet = false;
        DispatcherTimer nightWatchTimer;
        public UCItemCart(SaleOrderPresenter presenter)
        {
            InitializeComponent();

            Presenter = presenter;
            if (Defaults.DualPriceMode)
            {
                nightWatchTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 5, 0), DispatcherPriority.Normal, delegate
                {

                    CheckNightMode();

                }, this.Dispatcher);
            }
            if (Defaults.CASH_GUARD)
            {
                //lblInsertedAmountTotal.Visibility = Visibility.Visible;
                //txtInsertedAmount.Visibility = Visibility.Visible;
            }
            else
            {
                //lblInsertedAmountTotal.Visibility = Visibility.Collapsed;
                //txtInsertedAmount.Visibility = Visibility.Collapsed;
            }
            App.MainWindow.InsertedAmount += MainWindow_InsertedAmount;
        }

        private void MainWindow_InsertedAmount(object sender, int amout)
        {
            //lblInsertedAmountTotal.Dispatcher.BeginInvoke(new Action(() => lblInsertedAmountTotal.Text = amout.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));

        }

        private void CheckNightMode()
        {

            if ((DateTime.Now.Hour >= Defaults.NightStartHour && DateTime.Now.Hour < Defaults.NightEndHour) && Defaults.PriceMode == PriceMode.Day && TimeSet == false)
            {

                TimeModePanel.Visibility = Visibility.Visible;
                btnOK.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                // 
            }
            else if (DateTime.Now.Hour < Defaults.NightStartHour || DateTime.Now.Hour >= Defaults.NightEndHour)
            {
                TimeModePanel.Visibility = Visibility.Collapsed;
                Defaults.PriceMode = PriceMode.Day;
                TimeSet = false;
            }
        }
        public void SetCartItems(Order order)
        {
            List<OrderLine> list = order.OrderLines != null ? order.OrderLines.ToList() : new List<OrderLine>();
            lvCart.ItemsSource = null;
            lvCart.ItemsSource = list;
            // lvCart.Items.Refresh();
            DisplayOrderTotal();
            try
            {
                if (lvCart.Items.Count > 0)
                {
                    lvCart.SelectedIndex = lvCart.Items.Count - 1;
                    lvCart.ScrollIntoView(lvCart.SelectedItem);
                }
            }
            catch
            {
            }
        }
        public void NewRecord()
        {
            lvCart.ItemsSource = null;
            lvCart.ItemsSource = Presenter.CurrentorderDetails;
            lblGrossTotal.Text = (0).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            addCartIndex = -1;
            lblVatTotal.Text = (0).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            lblTotal.Text = (0).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            lblCurrency.Text = ((CultureInfo)Defaults.UICultureInfo).NumberFormat.CurrencySymbol;
            //   lblInsertedAmountTotal.Text = (0).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
        }
        public void RefreshItems()
        {
            if (addCartIndex != -1)
                lvCart.SelectedIndex = addCartIndex;

        }
        public void DisplayOrderTotal()
        {

            lblGrossTotal.Text = Presenter.MasterOrder.NetAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            lblVatTotal.Text = Presenter.MasterOrder.VatAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            Presenter.MasterOrder.OrderTotal = Presenter.MasterOrder.GrossAmount;
            lblTotal.Text = Presenter.MasterOrder.OrderTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);

            //lblGrossTotal.Dispatcher.BeginInvoke(new Action(() => lblGrossTotal.Text = Presenter.MasterOrder.NetTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            //lblVatTotal.Dispatcher.BeginInvoke(new Action(() => lblVatTotal.Text = Presenter.MasterOrder.Tax.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            //lblTotal.Dispatcher.BeginInvoke(new Action(() => lblTotal.Text = Presenter.MasterOrder.SumAmt.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
        }
        public void SetBEDS()
        {
            if (Presenter.CurrentorderDetails.Count > 1)
            {
                var orderdetail = Presenter.CurrentorderDetails.First();
                orderdetail.BIDS = "-------------------------SUBTOTAL----------------------";
                lvCart.ItemsSource = null;
                lvCart.ItemsSource = Presenter.CurrentorderDetails;
            }
        }
        private void lvCartMouseClick(object sender, MouseButtonEventArgs e)
        {

            if (lvCart.SelectedIndex == addCartIndex)
            {
                addCartIndex = -1;
                lvCart.SelectedIndex = -1;
            }
        }
        private void lvCart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lvCart.SelectedIndex == addCartIndex)
            {
                addCartIndex = -1;
                lvCart.SelectedIndex = -1;
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var product = (OrderLine)((Button)sender).DataContext;
                Presenter.CurrentorderDetails.Remove(product);

                if (product.Id != default(Guid))
                {
                    var lines = Presenter.CurrentorderDetails.Where(c => c.Id != product.Id).ToList();
                    Presenter.CurrentorderDetails = lines;
                }

                addCartIndex = -1;
                lvCart.SelectedIndex = -1;

                Presenter.MasterOrder.OrderLines = Presenter.CurrentorderDetails;

                SetCartItems(Presenter.MasterOrder);
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.DeleteItem(product);
                    UpdateOrderEntry();

                }
                Presenter.FocusTextBox();
                //  RefreshItems();
                if (Defaults.CustomerView)
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
            }
            catch (Exception ex)
            {
                //LogWriter.LogWrite(ex);
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void UpdateOrderEntry()
        {
            Presenter.MasterOrder.Updated = 1;
            Presenter.OrderEntry(Presenter.MasterOrder);
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var product = (OrderLine)((Button)sender).DataContext;
                if (product.Product.Unit != ProductUnit.Piece)
                    return;
                addCartIndex = -1;
                lvCart.SelectedIndex = -1;
                decimal qunatity = product.Quantity;
                qunatity = qunatity - 1;// -product.unitsinpack;
                if (qunatity < 1)
                {
                    Presenter.CurrentorderDetails.Remove(product);
                    if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        Presenter.DeleteItem(product);
                    }
                }
                else
                {
                    Presenter.RemoveItemQuantity(product, qunatity);

                }


                SetCartItems(Presenter.MasterOrder);
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    UpdateOrderEntry();
                }
                Presenter.FocusTextBox();
                // RefreshItems();
                if (Defaults.CustomerView)
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }

        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Presenter.MasterOrder.Status == OrderStatus.Completed)
                    return;

                var product = (OrderLine)((Button)sender).DataContext;
                if (product.Product.Unit != ProductUnit.Piece)
                    return;
                int newqty = 1;
                Presenter.AddQuantity(product, newqty);
                if (addCartIndex != -1)
                    lvCart.SelectedIndex = addCartIndex;

                Presenter.FocusTextBox();
                if (Defaults.CustomerView)
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void lvCart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCart.SelectedIndex != -1)
                addCartIndex = lvCart.SelectedIndex;
            SelectedItem = lvCart.SelectedItem as OrderLine;
        }

        private void Qty_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (OrderLine)(sender as Button).DataContext;
            if (selectedItem != null)
            {
                var Itemcomments = selectedItem.ItemComments;
                var title = UI.Sales_ProductComment;
                Itemcomments = Utilities.PromptInput(title, UI.Sales_EnterComment, Itemcomments);
                selectedItem.ItemComments = Itemcomments;
                lvCart.Items.Refresh();
            }
        }

        private void LvInnet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ListView).SelectedItem as OrderLine;
            if (selectedItem != null)
            {
                var Itemcomments = selectedItem.ItemComments;
                var title = UI.Sales_ProductComment;
                Itemcomments = Utilities.PromptInput(title, UI.Sales_EnterComment, Itemcomments);
                selectedItem.ItemComments = Itemcomments;
            }
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ItemName_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (sender as Button).DataContext as OrderLine;
            if (selectedItem != null)
            {
                var Itemcomments = "";
                var title = UI.Sales_ProductComment;
                Itemcomments = Utilities.PromptInput(title, UI.Sales_EnterComment, Itemcomments);
                selectedItem.ItemComments = Itemcomments;
            }
        }

        private void Ingredient_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (OrderLine)(sender as Button).DataContext;
            if (selectedItem != null)
            {
                IngredientItemWindow ingeredeintWindow = new IngredientItemWindow(selectedItem);
                if (ingeredeintWindow.ShowDialog() == true)
                {
                    Presenter.AddGredientsItems(selectedItem);
                    lvCart.Items.Refresh();

                    DisplayOrderTotal();
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
                }
            }
        }

        private void btnPriceMode_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Defaults.PriceMode = PriceMode.Night;
            TimeSet = true;
            nightWatchTimer.IsEnabled = true;
            btnOK.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Defaults.PriceMode = PriceMode.Day;
            TimeSet = true;
            nightWatchTimer.IsEnabled = true;
            TimeModePanel.Visibility = Visibility.Collapsed;
        }

        private void Price_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (OrderLine)(sender as Button).DataContext;
            if (selectedItem != null)
            {
                if (!string.IsNullOrEmpty(Defaults.DiscountCode))
                {
                    DiscountCodeWindow discountCodeWindow = new DiscountCodeWindow();
                    if (discountCodeWindow.ShowDialog() == false)
                        return;
                }
                PriceChangeWindow priceChangeWindow = new PriceChangeWindow(selectedItem.Product.Price);
                if (priceChangeWindow.ShowDialog() == true)
                {
                    var unitPrice = selectedItem.UnitPrice = priceChangeWindow.Amount;
                    var qunatity = selectedItem.Quantity;
                    selectedItem.DiscountedUnitPrice = unitPrice;
                    var grossTotal = unitPrice * qunatity;


                    if (Presenter.MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        Presenter.EditItem(selectedItem);
                        Presenter.OrderEntry(Presenter.MasterOrder);
                    }

                    SetCartItems(Presenter.MasterOrder);
                    //  ucCart.RefreshItems();
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
                }
            }
        }
    }
    */

    public partial class UCItemCart : UserControl
    {

        public SaleOrderPresenter Presenter { get; set; }
        public OrderLine SelectedItem { get; set; }
        ObservableCollection<OrderLine> Items = new ObservableCollection<OrderLine>();

        private int addCartIndex = -1;
        bool TimeSet = false;
        DispatcherTimer nightWatchTimer;
        public UCItemCart(SaleOrderPresenter presenter)
        {
            InitializeComponent();
            Presenter = presenter;
            if (Defaults.DualPriceMode)
            {
                nightWatchTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 5, 0), DispatcherPriority.Normal, delegate
                {

                    CheckNightMode();

                }, this.Dispatcher);
            }
        }
        private void CheckNightMode()
        {

            if ((DateTime.Now.Hour >= Defaults.NightStartHour && DateTime.Now.Hour < Defaults.NightEndHour) && Defaults.PriceMode == PriceMode.Day && TimeSet == false)
            {

                TimeModePanel.Visibility = Visibility.Visible;
                btnOK.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                // 
            }
            else if (DateTime.Now.Hour < Defaults.NightStartHour || DateTime.Now.Hour >= Defaults.NightEndHour)
            {
                TimeModePanel.Visibility = Visibility.Collapsed;
                Defaults.PriceMode = PriceMode.Day;
                TimeSet = false;
            }
        }

        // public SaleOrderPresenter Presenter { get; set; }
        //  public OrderLine SelectedItem { get; set; }

        public void SetCartItems(IList<OrderLine> list)
        {
            App.MainWindow.UpdateItems(list.ToList());
            lvCart.ItemsSource = null;
            lvCart.ItemsSource = list;
            DisplayOrderTotal();
            if (lvCart.Items.Count > 0)
            {
                lvCart.SelectedIndex = lvCart.Items.Count - 1;
                lvCart.ScrollIntoView(lvCart.SelectedItem);
            }
        }

        public void SetCartItems2(IList<OrderLine> list, bool IsAddToCart=false)
        {
            try
            {
                App.MainWindow.UpdateItems(list.ToList());
                lvCart.ItemsSource = null;
                lvCart.ItemsSource = list;
                if(IsAddToCart==false)
                { 
                    if (Presenter.MasterOrder != null)
                        Presenter.MasterOrder = Presenter.GetOrderMasterDetailById(Presenter.MasterOrder.Id);
                }
                else
                {
                    if (Presenter.MasterOrder != null)
                        Presenter.MasterOrder = Presenter.GetOrderMasterDetailByIdAddToCart(Presenter.MasterOrder.Id);
                }
                DisplayOrderTotal();
                if (lvCart.Items.Count > 0)
                {
                    lvCart.SelectedIndex = lvCart.Items.Count - 1;
                    lvCart.ScrollIntoView(lvCart.SelectedItem);
                }

            }
            catch (Exception ex)
            {

               
            }
            
        }

        public void NewRecord()
        {
            lvCart.ItemsSource = null;
            lvCart.ItemsSource = Presenter.CurrentorderDetails;
            lblGrossTotal.Text = 0.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            addCartIndex = -1;
            lblVatTotal.Text = 0.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            lblTotal.Text = 0.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            // lblCurrency.Text = ((CultureInfo) Defaults.UICultureInfo).NumberFormat.CurrencySymbol;
        }

        public void RefreshItems()
        {
            if (addCartIndex != -1)
                lvCart.SelectedIndex = addCartIndex;
        }

        public void DisplayOrderTotal()
        {
            //lblGrossTotal.Dispatcher.BeginInvoke(
            //    new Action(
            //        () =>
            //            lblGrossTotal.Text =
            //                Presenter.MasterOrder.NetAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            //lblVatTotal.Dispatcher.BeginInvoke(
            //    new Action(
            //        () =>
            //            lblVatTotal.Text =
            //                Presenter.MasterOrder.TaxPercent.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            //lblTotal.Dispatcher.BeginInvoke(
            //    new Action(
            //        () =>
            ////lblTotal.Text =
            if (Presenter.MasterOrder != null)
            {
                Presenter.MasterOrder.NetAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                lblGrossTotal.Text = Presenter.MasterOrder.NetAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                lblVatTotal.Text = Presenter.MasterOrder.VatAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                Presenter.MasterOrder.OrderTotal = Presenter.MasterOrder.GrossAmount;
                lblTotal.Text = Presenter.MasterOrder.OrderTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }
            //lblGrossTotal.Dispatcher.BeginInvoke(new Action(() => lblGrossTotal.Text = Presenter.MasterOrder.NetAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            //lblVatTotal.Dispatcher.BeginInvoke(new Action(() => lblVatTotal.Text = Presenter.MasterOrder.TaxPercent.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
            //lblTotal.Dispatcher.BeginInvoke(new Action(() => lblTotal.Text = Presenter.MasterOrder.OrderTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));

        }

        public void SetBEDS()
        {
            if (Presenter.CurrentorderDetails.Count > 1)
            {
                var orderdetail = Presenter.CurrentorderDetails.First();
                orderdetail.BIDS = "-------------------------SUBTOTAL----------------------";
                lvCart.ItemsSource = null;
                lvCart.ItemsSource = Presenter.CurrentorderDetails;
            }
        }

        private void lvCartMouseClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void lvCart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lvCart.SelectedIndex == addCartIndex)
            {
                addCartIndex = -1;
                lvCart.SelectedIndex = -1;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var product = (OrderLine)((Button)sender).DataContext;
                Presenter.CurrentorderDetails.Remove(product);
                addCartIndex = -1;
                lvCart.SelectedIndex = -1;

                //    Presenter.CalculatOrderTotal(Presenter.CurrentorderDetails);
               // SetCartItems(Presenter.CurrentorderDetails);
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.DeleteItem(product);
                    UpdateOrderEntry();
                }


                //if (Presenter.CurrentorderDetails.Count == 0)
                //{
                //    lblTotal.Text = "0.00";
                //    lblVatTotal.Text = "0.00";
                //    lblGrossTotal.Text = "0.00";
                //    //   Presenter.CurrentorderDetails = new List<OrderLine>();
                //}

                // DisplayOrderTotal();
                SetCartItems2(Presenter.CurrentorderDetails);

                Presenter.FocusTextBox();
                RefreshItems();
                if (Defaults.CustomerView)
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOrderEntry()
        {
            Presenter.MasterOrder.Updated = 1;
            Presenter.OrderEntry(Presenter.MasterOrder);
        }

        private void Remove_ClickOLD(object sender, RoutedEventArgs e)
        {
            try
            {
                var product = (OrderLine)((Button)sender).DataContext;
                Presenter.CurrentorderDetails.Remove(product);

                addCartIndex = -1;
                lvCart.SelectedIndex = -1;

                //    Presenter.CalculatOrderTotal(Presenter.CurrentorderDetails);
                SetCartItems(Presenter.CurrentorderDetails);
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.DeleteItem(product);
                    UpdateOrderEntry();

                }
                Presenter.FocusTextBox();
                RefreshItems();
                if (Defaults.CustomerView)
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);

                DisplayOrderTotal();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var product = (OrderLine)((Button)sender).DataContext;
                if (product.Product.Unit != ProductUnit.Piece)
                    return;
                addCartIndex = -1;
                lvCart.SelectedIndex = -1;
                decimal qunatity = product.Quantity;
                qunatity = qunatity - 1;// -product.unitsinpack;
                if (qunatity < 1)
                {
                    Presenter.CurrentorderDetails.Remove(product);
                    if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        Presenter.DeleteItem(product);
                    }
                }
                else
                {
                    Presenter.RemoveItemQuantity(product, qunatity);

                }

                //Presenter.CalculatOrderTotal(Presenter.CurrentorderDetails);
                SetCartItems2(Presenter.CurrentorderDetails);
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    UpdateOrderEntry();
                }
                Presenter.FocusTextBox();
                RefreshItems();
                if (Defaults.CustomerView)
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Presenter.MasterOrder.Status == OrderStatus.Completed)
                    return;

                var product = (OrderLine)((Button)sender).DataContext;
                if (product.Product.Unit != ProductUnit.Piece)
                    return;
                int newqty = 1;
                Presenter.AddQuantity(product, newqty);
                if (addCartIndex != -1)
                    lvCart.SelectedIndex = addCartIndex;

                SetCartItems2(Presenter.CurrentorderDetails);

                Presenter.FocusTextBox();
                if (Defaults.CustomerView)
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvCart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCart.SelectedIndex != -1)
                addCartIndex = lvCart.SelectedIndex;
            SelectedItem = lvCart.SelectedItem as OrderLine;
        }

        private void Qty_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (OrderLine)(sender as Button).DataContext;
            if (selectedItem != null)
            {
                var Itemcomments = selectedItem.ItemComments;
                var title = UI.Sales_ProductComment;
                Itemcomments = Utilities.PromptInput(title, UI.Sales_EnterComment, Itemcomments);
                selectedItem.ItemComments = Itemcomments;
                //Presenter.MasterOrder.OrderComments = Itemcomments;
                lvCart.Items.Refresh();
            }
        }
        private void Ingredient_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (OrderLine)(sender as Button).DataContext;
            if (selectedItem != null)
            {
                IngredientItemWindow ingeredeintWindow = new IngredientItemWindow(selectedItem);

                if (ingeredeintWindow.ShowDialog() == true)
                {
                    Presenter.AddGredientsItems(selectedItem);                    
                    lvCart.Items.Refresh();
                    Presenter.CalculatOrderTotal(Presenter.CurrentorderDetails);
                   // DisplayOrderTotal();

                    SetCartItems2(Presenter.CurrentorderDetails);

                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
                }
            }


        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Defaults.PriceMode = PriceMode.Night;
            TimeSet = true;
            nightWatchTimer.IsEnabled = true;
            btnOK.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Defaults.PriceMode = PriceMode.Day;
            TimeSet = true;
            nightWatchTimer.IsEnabled = true;
            TimeModePanel.Visibility = Visibility.Collapsed;
        }

        private void Price_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (OrderLine)(sender as Button).DataContext;
            if (selectedItem != null)
            {
                PriceChangeWindow priceChangeWindow = new PriceChangeWindow(selectedItem.Product.Price);
                if (priceChangeWindow.ShowDialog() == true)
                {
                    var unitPrice = selectedItem.UnitPrice = priceChangeWindow.Amount;
                    var qunatity = selectedItem.Quantity;
                    selectedItem.DiscountedUnitPrice = unitPrice;
                    var grossTotal = unitPrice * qunatity;
                    //selectedItem.GrossTotal=(decimal) grossTotal;


                    if (Presenter.MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        Presenter.EditItem(selectedItem);
                        Presenter.OrderEntry(Presenter.MasterOrder);
                    }
                    Presenter.CalculatOrderTotal(Presenter.CurrentorderDetails);
                    SetCartItems2(Presenter.CurrentorderDetails);
                    //  ucCart.RefreshItems();
                    App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
                }
            }

            //try
            //{
            //    if (SelectedItem == null)
            //    {
            //        MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //        return;
            //    }

            //    if (SelectedItem != null && SelectedItem.Product.DiscountAllowed)
            //    {
            //        if (!string.IsNullOrEmpty(Defaults.DiscountCode))
            //        {
            //            DiscountCodeWindow discountCodeWindow = new DiscountCodeWindow();
            //            if (discountCodeWindow.ShowDialog() == false)
            //                return;
            //        }
            //        DiscountWindow discountWindow = new DiscountWindow(SelectedItem.Product.Price);
            //        if (discountWindow.ShowDialog() == true)
            //        {
            //            if (SelectedItem.ItemType == ItemType.Grouped)
            //            {
            //                var groupedItems = SelectedItem.ItemDetails;
            //                if (groupedItems != null)
            //                {
            //                    foreach (var groupItem in groupedItems)
            //                    {
            //                        decimal innetQty = groupItem.Quantity;
            //                        decimal innetGrossTotal = groupItem.UnitPrice;
            //                        decimal inneritemdiscount = 0;
            //                        if (discountWindow.Percentage)
            //                        {
            //                            groupItem.DiscountPercentage = discountWindow.Amount;
            //                            inneritemdiscount = (innetGrossTotal / 100) * groupItem.DiscountPercentage;
            //                            innetGrossTotal = innetQty * innetGrossTotal;
            //                        }
            //                        else
            //                        {
            //                            // ucCart.SelectedItem.DiscountPercentage = (discountWindow.Amount * 100) / grossTotal;
            //                            inneritemdiscount = discountWindow.Amount / groupedItems.Count;
            //                        }
            //                        if (Presenter.MasterOrder.Type == OrderType.Return)
            //                            inneritemdiscount = (-1) * inneritemdiscount;
            //                        groupItem.ItemDiscount = inneritemdiscount;
            //                        groupItem.DiscountedUnitPrice = innetGrossTotal - inneritemdiscount;


            //                    }
            //                    SelectedItem.ItemDetails = groupedItems;
            //                }
            //            }
            //            decimal qunatity = SelectedItem.Quantity;
            //            decimal grossTotal = SelectedItem.UnitPrice;
            //            decimal itemdiscount = 0;

            //            if (discountWindow.Percentage)
            //            {
            //                SelectedItem.DiscountPercentage = discountWindow.Amount;
            //                itemdiscount = (grossTotal / 100) * SelectedItem.DiscountPercentage;
            //                itemdiscount = qunatity * itemdiscount;
            //            }
            //            else
            //            {
            //                // ucCart.SelectedItem.DiscountPercentage = (discountWindow.Amount * 100) / grossTotal;f
            //                itemdiscount = discountWindow.Amount;
            //            }
            //            // grossTotal = grossTotal - itemdiscount;
            //            if (Presenter.MasterOrder.Type == OrderType.Return)
            //                itemdiscount = (-1) * itemdiscount;
            //            SelectedItem.DiscountedUnitPrice = Presenter.MasterOrder.Type == OrderType.Return ? grossTotal + itemdiscount : grossTotal - itemdiscount;
            //            grossTotal = grossTotal * qunatity;
            //            SelectedItem.ItemDiscount = itemdiscount;
            //            SelectedItem.DiscountDescription = UI.Sales_Discount;

            //            if (Presenter.MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
            //            {
            //                Presenter.EditItem(SelectedItem);
            //            }

            //            SetCartItems(Presenter.CurrentorderDetails);
            //            //  ucCart.RefreshItems();
            //            App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    LogWriter.LogWrite(ex);
            //    WAQAS_COMMENT//MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            //    LogWriter.LogWrite(ex);
            //}
        }

        private void LvInnet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }


}
