using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using POSSUM.Base;
using POSSUM.Data;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Presenter.Products;
using POSSUM.Handlers;
using POSSUM.Res;
using POSSUM.Utils;
using POSSUM.Utils.nu.kontantkort.extdev;
using POSSUM.Views.CheckOut;
using POSSUM.Views.PrintOrder;
using System.Text.RegularExpressions;
using System.Globalization;
using POSSUM.Views.FoodTables;
using POSSUM.Views.Sales;
using POSSUM.Views.SplitOrder;
using POSSUM.Views.OpenOrder;
using POSSUM.Views.Customers;
using System.Configuration;

namespace POSSUM.Presenters.Sales
{
    public class SaleOrderPresenter : OrderPresenter
    {
        private readonly ISaleOrderView _view;
        private int _customerId;
        private int _orderDirection = 1;
        private Guid _orderId;
        private OrderType _type = OrderType.Standard;

        ProductPresenter productPresenter = new ProductPresenter();

        public SaleOrderPresenter(ISaleOrderView view, Guid orderId = default(Guid))
        {
            _view = view;
            _orderId = orderId;

            UpdateHistoryGrid();
        }

        #region Get Order
        public Order GetOrderMasterDetailById(Guid id)
        {
            return new OrderRepository(PosState.GetInstance().Context).GetOrderMasterDetailById(id);
        }
        public Order GetOrderMasterDetailByIdAddToCart(Guid id)
        {
            return new OrderRepository(PosState.GetInstance().Context).GetOrderMasterDetailByIdAddToCart(id);
        }

        public List<OrderLine> GetOrderDetailById(Guid id)
        {
            var orderRepository = new OrderRepository(PosState.GetInstance().Context);
            return orderRepository.GetOrderLinesById(id);
        }

        public void UpdateHistoryGrid()
        {
            if (Defaults.SaleType == SaleType.Restaurant && ConfigurationManager.AppSettings["RestaurantGridShow"] == "0" )
            {
                return;
            }

            DateTime dateFrom = DateTime.Now.Date;
            DateTime dateTo = DateTime.Now;

            try
            {
                var orders = new OrderRepository(PosState.GetInstance().Context).UpdateHistoryGrid(dateFrom, dateTo, Defaults.Terminal.Id);
                _view.SetHistoryViewResult(orders);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
        }


        public List<Order> GetOpenOrdersOnTable(int tableId)
        {
            return new OrderRepository(PosState.GetInstance().Context).GetOpenOrdersOnTable(tableId);
        }
        #endregion

        #region  Item Cart Event

        public void HandelAddProductClick(OrderLine orderDetail, decimal qty, int bidsno)
        {
            var item = orderDetail.Product;

            /**/
            decimal quantitiy = orderDetail.Quantity;
            quantitiy = quantitiy + qty;
            decimal grossTotal = orderDetail.UnitPrice;

            if (orderDetail.DiscountPercentage > 0)
            {
                decimal itemdiscount = grossTotal / 100 * orderDetail.DiscountPercentage;
                itemdiscount = quantitiy * itemdiscount;
                orderDetail.ItemDiscount = itemdiscount;
                orderDetail.DiscountedUnitPrice = quantitiy * grossTotal - itemdiscount;
            }


            orderDetail.Quantity = quantitiy;

            ApplyTempPrice(item, orderDetail);
            ApplyCampain(item, orderDetail);
            AddPantProduct(item, orderDetail);

            //Waqas
            _view.SetCartItems(CurrentorderDetails,true);
            RefreshNewItem();

            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                EditItem(orderDetail);
                MasterOrder.Updated = 1;
                OrderEntry(MasterOrder);
            }
        }

        public bool AddGredientsItems(OrderLine orderLine)
        {

            try
            {
                return new OrderRepository(PosState.GetInstance().Context).AddGredientsItems(orderLine);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                //LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public void HandleItemButtonClick()
        {
            int _bidsno = 0;
            Product product = _view.GetSelectedItem();
            decimal currentQty = _view.GetCurrentQty();
            string texBoxValue = GetTexBoxValue();
            if (product.ItemType == ItemType.Grouped)
            {
                var list = product.Products; // Presenter.GetProductByGroup(product.Id);
                if (product.ReceiptMethod == ReceiptMethod.Show_Product_As_Individuals)
                {
                    foreach (var itm in list)
                    {
                        AddItemToCart(itm, 1);
                    }
                }
                else
                {
                    var grpPrice = list.Sum(p => p.Price);
                    product.Price = grpPrice;
                    AddGroupItemToCart(product, 1, list);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(GetAskPriceValue()))
                {
                    currentQty = 1;
                    FocusTextBox();
                }
                if (!string.IsNullOrEmpty(texBoxValue))
                {
                    if (texBoxValue.Length > 10)
                    {
                        MessageBox.Show(UI.Message_InvalidQty, Defaults.AppProvider.AppTitle,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        FocusTextBox();
                        return;
                    }
                    if (texBoxValue.Contains("x"))
                    {
                        texBoxValue = texBoxValue.Remove(texBoxValue.Length - 1);
                    }

                    decimal.TryParse(texBoxValue, out currentQty);
                    if (currentQty == 0)
                    {
                        currentQty = 1;
                    }
                }
                _view.SetSelectedItem(product);
                var orderDetail =
                    CurrentorderDetails.FirstOrDefault(
                        p => p.ItemId == product.Id && p.ItemStatus == 0 && p.BIDSNO == _bidsno && p.IngredientItems == null && string.IsNullOrEmpty(p.ItemComments));
                if (orderDetail != null && product.AskPrice == false &&
                    product.AskWeight == false &&
                    product.AskVolume == false)
                {
                    HandelAddProductClick(orderDetail, currentQty, _bidsno);
                    return;
                }

                EntryModeType entryMode;
                if (product.AskPrice)
                {
                    _view.SetAskByUser(true);
                    SetAskPriceValue(UI.Sales_EnterPrice); // "Ange pris :";
                    entryMode = EntryModeType.PluEntry;
                    _view.SetEntryMode(entryMode);
                    FocusTextBox();
                }
                else
                {

                    if (product.AskWeight)
                    {
                        var weight = GetWeightFromScale();

                        if (weight == -1)
                        {
                            SetAskPriceValue(UI.EnterWeight); // "Ange weight :";
                            entryMode = EntryModeType.PluEntry;
                            _view.SetEntryMode(entryMode);
                            _view.SetAskByUser(true);
                            FocusTextBox();
                        }
                        else
                        {
                            switch (product.Unit)
                            {
                                case ProductUnit.g:
                                    //Grams
                                    currentQty = Convert.ToDecimal(weight);
                                    break;
                                case ProductUnit.kg:
                                    {
                                        //Kilograms
                                        var grams = Convert.ToDecimal(weight);
                                        currentQty = grams / 1000;
                                    }
                                    break;
                                case ProductUnit.hg:
                                    {
                                        //Hectograms
                                        var grams = Convert.ToDecimal(weight);
                                        currentQty = grams / 100;
                                    }
                                    break;
                            }

                            AddItemToCart(product, currentQty, _bidsno);
                            currentQty = 1;

                            _view.SetAskByUser(false);
                        }
                    }
                    else if (product.AskVolume)
                    {
                        decimal volume = -1;
                        // volume = GetWeightFromScale();
                        int askVolumeQty = 0;
                        int.TryParse(texBoxValue, out askVolumeQty);
                        _view.SetAskVolumeQty(askVolumeQty == 0 ? 1 : askVolumeQty);
                        if (volume == -1)
                        {

                            SetAskPriceValue(UI.Message_AskVolume); // "Ange weight :";
                            entryMode = EntryModeType.PluEntry;
                            _view.SetEntryMode(entryMode);
                            FocusTextBox();
                            _view.SetAskByUser(true);
                        }

                    }
                    else
                    {
                        AddItemToCart(product, currentQty, _bidsno);
                        currentQty = 1;
                        _view.SetCurrentQty(currentQty);

                        _view.SetAskByUser(false);
                    }
                }
            }
            //
        }

        internal void SetNewEntery()
        {
            _view.NewEntry();
        }

        public void HandleSelectTabeClick()
        {

            var tableWindow = new TableWindow();
            if (tableWindow.ShowDialog() == true)
            {
                var type = MasterOrder.Type == OrderType.TakeAway ? OrderType.TableTakeAwayOrder : OrderType.TableOrder;
                var isNewOrder = tableWindow.IsNewOrder;
                var selectedTable = tableWindow.SelectedTable;
                _view.SetTableButtonContent(selectedTable.Name);
                _view.SetSelectedTable(selectedTable);
                if (isNewOrder)
                {
                    MasterOrder.SelectedTable = selectedTable;
                    MasterOrder.TableId = selectedTable.Id;
                    MasterOrder.Comments = selectedTable.Name;
                    MasterOrder.TableName = selectedTable.Name;
                    MasterOrder.Type = type;
                    UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                    _view.SetIsMerge(false);
                }
                else
                {
                    List<Order> orders = GetOpenOrdersOnTable(selectedTable.Id);
                    if (orders.Count > 0)
                    {
                        if (orders.Count == 1)
                        {
                            _view.SetIsMerge(true);
                            _view.SetMergeOrder(orders.First());
                        }
                        else
                        {
                            _view.OpenMegerOrderDialog(orders);
                        }

                    }
                    else
                    {
                        _view.SetIsMerge(false);
                        MasterOrder.SelectedTable = selectedTable;
                        MasterOrder.TableId = selectedTable.Id;
                        MasterOrder.Comments = selectedTable.Name;
                        MasterOrder.TableName = selectedTable.Name;
                        MasterOrder.Type = type;
                        UpdateOrderDetail(CurrentorderDetails, MasterOrder);

                    }
                }
                // LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTableSelected), Presenter.MasterOrder.Id, Presenter.MasterOrder.TableId);
            }
            else
            {
                MasterOrder.TableId = 0;
                _view.SetTableButtonContent(UI.Sales_SelectTableButton);
            }

            FocusTextBox();
        }

        internal void SetCartItems(List<OrderLine> masterOrder)
        {
            _view.SetCartItems(masterOrder);
        }

        public void HandleTakeawayClick()
        {
            if (MasterOrder.Type == OrderType.Return || MasterOrder.Type == OrderType.TakeAwayReturn)
            {
                if (_view.GetOrderTypeSecondaryVisibility())
                {
                    MasterOrder.Type = OrderType.Return;
                    // lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
                    _view.SetOrderTypeSecondaryVisibility(false);
                }
                else
                {
                    MasterOrder.Type = OrderType.TakeAwayReturn;
                    // lblOrderTypeSecondary.Visibility = Visibility.Visible;
                    _view.SetOrderTypeSecondaryVisibility(true);
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeReturnTakeAway), MasterOrder.Id);
                }
                return;
            }
            else
                MasterOrder.Type = OrderType.TakeAway;
            // lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
            _view.SetOrderTypeSecondaryVisibility(false);
            //  Presenter.MasterOrder.Type = (Presenter.MasterOrder.Type == OrderType.TakeAway) ? OrderType.Standard : OrderType.TakeAway;
            if (MasterOrder.Type == OrderType.TakeAway)
            {
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeTakeAway), MasterOrder.Id);
                //lblOrderType.Visibility = Visibility.Visible;
                //lblOrderType.Text = UI.Sales_TakeAwayButton;
                _view.SetOrderTypeVisibility(true, UI.Sales_TakeAwayButton);
            }
            else
            {
                //lblOrderType.Visibility = Visibility.Collapsed;
                //lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
                //lblOrderType.Text = "";
                _view.SetOrderTypeVisibility(false, "");
                _view.SetOrderTypeSecondaryVisibility(false);
            }
            if (MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                UpdateOrderEntry();
            }

            FocusTextBox();
        }
        public void UncheckTakeaway()
        {
            _view.SetOrderTypeVisibility(false, "");
            _view.SetOrderTypeSecondaryVisibility(false);
            MasterOrder.Type = OrderType.Standard;
            if (MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                UpdateOrderEntry();
            }

            FocusTextBox();
        }
        /*public void HandleTableOpenOrderClick()
        {
            var selectedTable = _view.GetSelectedTable();
            if (CurrentorderDetails != null && CurrentorderDetails.Count > 0)
            {
                MasterOrder.OrderLines = CurrentorderDetails;
                if (selectedTable != null && MasterOrder.TableId == 0)
                    MasterOrder.TableId = selectedTable.Id;
                var openOrderWindow = new OpenOrderWindow(MasterOrder);
                openOrderWindow.ShowDialog();
                if (openOrderWindow.NewOrderMerged)
                {
                    if (MasterOrder != null && MasterOrder.IsForAdult)
                    {
                        _view.ShowSurvey();
                    }
                    _view.NewRecord();
                }
            }
            else
            {
                var openOrderWindow = new OpenOrderWindow();
                openOrderWindow.ShowDialog();
            }
            FocusTextBox();
        }*/
        public void HandleBarCodeEntry()
        {
            try
            {
                int bidsno = _view.GetBIDSNO();
                string barcode;
                decimal currentQty = _view.GetCurrentQty();
                string value = _view.GetTextBoxValue();
                if (value.Contains("x"))
                {
                    string[] values = value.Split('x');

                    currentQty = Convert.ToInt32(values[0]);
                    _view.SetCurrentQty(currentQty);

                    barcode = values[1];
                }
                else
                    barcode = value;
                if (string.IsNullOrEmpty(barcode))
                {
                    MessageBox.Show(UI.Message_EANMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    FocusTextBox();
                    return;
                }
                var product = GetProductByBarCode(barcode);
                if (product.Id == default(Guid))
                {
                    //if (MessageBox.Show(UI.Message_UnknowBarcode + "\n" + UI.Message_AddConfirm, UI.Global_Warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    //{
                    //NewProductAddWindow productWindow = new NewProductAddWindow(barcode);
                    //productWindow.ShowDialog();
                    // }
                    FocusTextBox();

                    return;
                }

                if (MasterOrder.Status == OrderStatus.Completed)
                    return;
                _view.SetSelectedItem(product);
                if (product.AskPrice)
                {
                    SetAskPriceValue(UI.EnterPrice + " :");
                    var EntryMode = EntryModeType.ItemEntry;
                    _view.SetEntryMode(EntryMode);
                    FocusTextBox();
                }
                else if (VariableWeight(barcode))
                {
                    AddItemToCart(product, _view.GetCurrentQty(), bidsno);
                    _view.SetCurrentQty(1);
                    _view.SetAskByUser(false);
                }
                else if (product.AskWeight)
                {
                    var weight = GetWeightFromScale();

                    if (weight == -1)
                    {
                        _view.SetAskPriceValue(UI.EnterWeight + " :");
                        _view.SetEntryMode(EntryModeType.ItemEntry);
                        FocusTextBox();
                    }
                    else
                    {

                        if (product.Unit == ProductUnit.g)
                        {
                            //Grams
                            currentQty = Convert.ToDecimal(weight);
                        }
                        else if (product.Unit == ProductUnit.kg)
                        {
                            //Kilograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 1000;

                        }
                        else if (product.Unit == ProductUnit.hg)
                        {
                            //Hectograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 100;
                        }
                        AddItemToCart(product, currentQty, bidsno);
                        _view.SetCurrentQty(1);
                        _view.SetAskByUser(false);
                    }
                }
                else
                {
                    AddItemToCart(product, currentQty);
                    _view.SetAskByUser(false);
                    _view.SetCurrentQty(1);
                }
                FocusTextBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                LogWriter.LogWrite(ex);
            }
        }
        public void HandleEnterClick()
        {
            try
            {
                decimal currentQty = _view.GetCurrentQty();
                var entryMode = _view.GetEntryMode();
                var selectedItem = _view.GetSelectedItem();
                if (entryMode == EntryModeType.CodeEntry)
                {
                    string code = _view.GetTextBoxValue();
                    if (!string.IsNullOrEmpty(code))//&& code.Contains("x")
                    {
                        HandleBarCodeEntry();
                        return;
                    }
                }
                if (selectedItem == null)
                {
                    MessageBox.Show(UI.Message_SelectItemToAddInCart, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    FocusTextBox();
                    return;
                }

                string textBoxValue = _view.GetTextBoxValue();
                if (!string.IsNullOrEmpty(textBoxValue))
                {

                    RegionInfo local = RegionInfo.CurrentRegion;
                    var value = local.Name == "SE" ? textBoxValue : textBoxValue.Replace(',', '.');
                    if (selectedItem.AskPrice)
                    {
                        decimal unitPriceWithVat = Convert.ToDecimal(value);
                        selectedItem.Price = unitPriceWithVat;
                    }
                    if (selectedItem.AskWeight)
                    {
                        if (selectedItem.Unit == ProductUnit.g)
                        {
                            //Grams
                            currentQty = Convert.ToDecimal(value);
                        }
                        else if (selectedItem.Unit == ProductUnit.kg)
                        {
                            //Kilograms
                            var grams = Convert.ToDecimal(value);
                            currentQty = grams / 1000;

                        }
                        else if (selectedItem.Unit == ProductUnit.hg)
                        {
                            //Hectograms
                            var grams = Convert.ToDecimal(value);
                            currentQty = grams / 100;
                        }

                    }
                    if (selectedItem.AskVolume)
                    {
                        currentQty = Convert.ToDecimal(value);
                    }
                    FocusTextBox();
                }
                FocusTextBox();
                if (selectedItem.AskVolume)
                {
                    var askVolumeQty = _view.GetAskVolumeQty();
                    List<Product> itemsList = new List<Product>();
                    for (int i = 0; i < askVolumeQty; i++)
                    {
                        itemsList.Add(selectedItem);
                    }
                    decimal qty = currentQty;
                    foreach (var itemList in itemsList)
                        AddItemToCart(itemList, qty);
                }
                else
                    AddItemToCart(selectedItem, currentQty);
                _view.SetCurrentQty(1);
                _view.SetAskByUser(false);
                _view.SetAskVolumeQty(1);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            FocusTextBox();
        }
        private decimal GetWeightFromScale()
        {
            decimal weight = -1;
            try
            {
                if (Defaults.ScaleType == ScaleType.DUMMY || Defaults.ScaleType == ScaleType.NCIPROTOCOL || Defaults.ScaleType == ScaleType.NCIPROTOCOL48 || Defaults.ScaleType == ScaleType.NCRPROTOCOL)
                {
                    var scale = PosState.GetInstance().Scale;
                    scale.Connect();
                    weight = scale.GetWeight();
                    scale.Disconnect();
                }
            }

            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                weight = -1;
            }
            return weight;
        }
        private bool VariableWeight(string barcode)
        {
            try
            {
                var selectedItem = _view.GetSelectedItem();
                if (selectedItem.BarCode.Length != 8)
                    return false;

                Regex ean13 = new Regex(@"\b(?:\d{13})\b");

                if (string.IsNullOrEmpty(barcode) || ean13.IsMatch(barcode) == false)
                    return false;


                string weightSpec = barcode.Substring(8, 5);
                string weight = weightSpec.Substring(0, 4);
                //  decimal Weightdecimal = decimalPosition == "0" ? 1 : decimalPosition == "1" ? 10 : decimalPosition == "2" ? 100 : decimalPosition == "3" ? 100 : decimalPosition == "4" ? 10000 : 100000;
                decimal netWeght = Convert.ToDecimal(weight);// / Weightdecimal;
                var currentQty = netWeght;
                if (selectedItem.Unit == ProductUnit.g)// UnitName == "Kg" ? ProductUnit.kg : ProductUnit.kg;
                    currentQty = netWeght;
                else if (selectedItem.Unit == ProductUnit.kg)
                    currentQty = netWeght / 1000;

                _view.SetCurrentQty(currentQty);
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                return false;


            }
        }

        public void AddItemToCart(Product product, decimal qty, int bidsno = 0, bool isWeightItem = false,bool IsAddToCart=false)
        {
            qty = Math.Round(qty, 3);
            if ((product.PLU == "LOTTER" || (!string.IsNullOrEmpty(product.PLU) && product.PLU.StartsWith("PANT"))) && product.Price > 0)
            {
                product.Price = (-1) * product.Price;
            }

            // EXTRA VALIDATION, INCASE ORDER OBJECT IS NOT ALREADY CREATED
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll && (MasterOrder == null || MasterOrder.Id == default(Guid)))
            {
                _view.NewEntry();
            }

            //CHECK IF IT IS PAINT ITEM THEN THERE MUST NOT BI ANY OTHER ITEM IN CART
            if (!(product.PLU == "LOTTER" || (!string.IsNullOrEmpty(product.PLU) && (!string.IsNullOrEmpty(product.PLU) && product.PLU.StartsWith("PANT")))))
            {
                //We need to restrick that positive price should not be added after pant items
                var cartHasPANTItems = CurrentorderDetails != null &&
                    CurrentorderDetails.Count(a => (a.Product.PLU == "LOTTER" || (!string.IsNullOrEmpty(a.Product.PLU) && a.Product.PLU.StartsWith("PANT")))) > 0;
                if (cartHasPANTItems)
                {
                    MessageBox.Show("After adding PANT item, regular items are not allowed.(Add all regular item first then add PANT items)", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                var hasItemsWithNegaitivePrice = CurrentorderDetails != null && CurrentorderDetails.Count(a => (a.Product.PLU == "LOTTER" || (!string.IsNullOrEmpty(a.Product.PLU) && a.Product.PLU.StartsWith("PANT")))) > 0;
                if (hasItemsWithNegaitivePrice)
                {
                    var hasItemsWithNegaitivePriceButWithSameTax = CurrentorderDetails != null && CurrentorderDetails.Count(a => a.TaxPercent == product.Tax && (a.Product.PLU == "LOTTER" || (!string.IsNullOrEmpty(a.Product.PLU) && a.Product.PLU.StartsWith("PANT")))) > 0;
                    if (!hasItemsWithNegaitivePriceButWithSameTax)
                    {
                        MessageBox.Show("Regular Items cannot be added with PANT/LOTTER Items in Cart. Please create seperate order for this.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
            else
            {
                var hasItemsWithPositivePrice = CurrentorderDetails != null && CurrentorderDetails.Count(a => !(a.Product.PLU == "LOTTER" || (!string.IsNullOrEmpty(a.Product.PLU) && a.Product.PLU.StartsWith("PANT")))) > 0;
                if (hasItemsWithPositivePrice)
                {
                    var hasItemsWithPositivePriceWithSameTax = CurrentorderDetails != null && CurrentorderDetails.Count(a => a.TaxPercent == product.Tax && !(a.Product.PLU == "LOTTER" || (!string.IsNullOrEmpty(a.Product.PLU) && a.Product.PLU.StartsWith("PANT")))) > 0;
                    if (!hasItemsWithPositivePriceWithSameTax)
                    {
                        MessageBox.Show("PANT/LOTTER Items cannot be added with Regular Items in Cart. Please create seperate order for this.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            try
            {
                int addCartIndex = _view.GetCartIndex();
                decimal price = product.Price;
                if (Defaults.DualPriceMode)
                {
                    if (product.ProductPrices != null && Defaults.PriceMode == PriceMode.Night)
                    {
                        PriceMode mode = Defaults.PriceMode;
                        var modeprice = product.ProductPrices.FirstOrDefault(p => p.PriceMode == mode);
                        if (modeprice != null && modeprice.Price > 0)
                            price = modeprice.Price;
                    }
                }

                //       decimal tax = grossTotal / 100 * product.Tax;

                int itemIdex = 0;

                if (CurrentorderDetails.Count > 0)
                {
                    itemIdex = CurrentorderDetails.Max(od => od.ItemIdex);
                }

                var data = CurrentorderDetails.Where(p => p.ItemId == product.Id && p.UnitPrice == product.Price && p.TaxPercent == product.Tax && p.IngredientItems == null && string.IsNullOrEmpty(p.ItemComments));

                //TODO:FIX
                //if (data.Any() && product.AskPrice == false && product.AskWeight == false && product.AskVolume == false && isWeightItem == false)
                if (data.Any() && product.AskPrice == false && product.AskWeight == false && product.AskVolume == false)
                {
                    var orderDetail = CurrentorderDetails.FirstOrDefault(p => p.ItemId == product.Id && p.TaxPercent == product.Tax && p.UnitPrice == product.Price && string.IsNullOrEmpty(p.ItemComments));
                    if (orderDetail != null)
                    {
                        decimal quantity = Math.Round(orderDetail.Quantity, 3);
                        quantity = quantity + Math.Round(qty, 3);

                        var grossTotal = quantity * Math.Round(orderDetail.UnitPrice);
                        if (orderDetail.DiscountPercentage > 0)
                        {
                            decimal itemdiscount = grossTotal / 100 * orderDetail.DiscountPercentage;
                            itemdiscount = quantity * itemdiscount;
                            orderDetail.ItemDiscount = itemdiscount;
                            orderDetail.DiscountDescription = UI.Sales_Discount;
                            orderDetail.DiscountType = DiscountType.General;
                        }

                        orderDetail.Quantity = quantity;


                        //Update Item to database
                        if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                        {
                            EditItem(orderDetail);
                        }

                        ApplyTempPrice(product, orderDetail);
                        ApplyCampain(product, orderDetail);
                        AddPantProduct(product, orderDetail);
                    }
                }
                else
                {
                    if (MasterOrder != null)
                    {
                        var orderDetail = new OrderLine
                        {
                            ItemIdex = itemIdex + 1,
                            TaxPercent = product.Tax,
                            Quantity = qty,
                            UnitPrice = price,
                            DiscountedUnitPrice = price,
                            PurchasePrice = product.PurchasePrice,
                            Active = 1,
                            ItemStatus = (int)OrderStatus.New,
                            Direction = MasterOrder.OrderDirection,
                            BIDSNO = bidsno,
                            PrinterId = product.PrinterId,
                            OrderType = MasterOrder.Type,
                            //Percentage = qty * product.Price / (100 * product.Tax),
                            Percentage = product.Tax == 0 ? 0 : qty * product.Price / (100 * product.Tax),

                            ItemId = product.Id,
                            Product = product
                        };



                        //Add Item to database
                        if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                            orderDetail = AddOrderLine(MasterOrder, orderDetail);

                        if (MasterOrder.Type == OrderType.Return || MasterOrder.Type == OrderType.TakeAwayReturn)
                            orderDetail.ItemStatus = (int)OrderStatus.ReturnOrder;
                        if (addCartIndex > 0)
                        {
                            CurrentorderDetails.Insert(addCartIndex + 1, orderDetail);
                        }
                        else
                        {
                            CurrentorderDetails.Add(orderDetail);
                        }

                        ApplyTempPrice(product, orderDetail);
                        ApplyCampain(product, orderDetail);
                        AddPantProduct(product, orderDetail);
                    }
                }

                if (MasterOrder != null)
                {
                    MasterOrder.OrderLines = CurrentorderDetails;
                    RefreshNewItem();
                    //Waqas
                    
                    _view.SetCartItems(CurrentorderDetails,IsAddToCart);
                   
                    if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        MasterOrder.Updated = 1;
                        OrderEntry(MasterOrder);
                    }
                }


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex.Message + ex.InnerException?.Message + ex + "Product Id: " + product.Id.ToString());
                //LogWriter.LogWrite("Product Id: "+product.Id.ToString());

                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPantProduct(Product product, OrderLine selectedItem)
        {
            if (product.PantProductId != null)
            {
                Product pantProduct = new ProductRepository(PosState.GetInstance().Context).GetPantProductByPantId(Guid.Parse(product.PantProductId));
                if (pantProduct != null)
                {
                    if (selectedItem.IngredientItems == null)
                        selectedItem.IngredientItems = new List<OrderLine>();

                    int itemIdex = 0;
                    if (selectedItem.IngredientItems.Count > 0)
                        itemIdex = selectedItem.IngredientItems.Max(od => od.ItemIdex);

                    var existingItem = selectedItem.IngredientItems.FirstOrDefault(a => a.ItemId == pantProduct.Id);
                    if (existingItem == null)
                    {
                        var orderItem = new OrderLine
                        {
                            ItemIdex = itemIdex + 1,
                            TaxPercent = product.Tax,
                            Quantity = selectedItem.Quantity,
                            UnitPrice = pantProduct.Price,
                            DiscountedUnitPrice = (pantProduct.Price),
                            PurchasePrice = pantProduct.Price,
                            Active = 1,
                            Direction = selectedItem.Direction,
                            PrinterId = product.PrinterId,
                            Percentage = product.Tax,

                            ItemId = pantProduct.Id,
                            Product = pantProduct,
                            ItemType = Model.ItemType.Ingredient,
                            GroupId = selectedItem.ItemId,
                            IngredientMode = "+"
                        };

                        selectedItem.IngredientItems.Add(orderItem);
                    }
                    else
                    {
                        existingItem.Quantity = selectedItem.Quantity;
                    }

                    AddGredientsItems(selectedItem);
                    CalculatOrderTotal(CurrentorderDetails);
                    // DisplayOrderTotal();
                    App.MainWindow.UpdateItems(CurrentorderDetails);
                }
            }
        }

        private void ApplyTempPrice(Product product, OrderLine orderLine, int direction = 1)
        {
            if (MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                if (product.TempPrice != null && product.TempPriceEnd != null && product.TempPriceStart != null)
                {
                    if (DateTime.Now >= product.TempPriceStart && DateTime.Now <= product.TempPriceEnd)
                    {
                        if (orderLine.ItemType == ItemType.Grouped)
                        {
                            var groupedItems = orderLine.ItemDetails;
                            if (groupedItems != null)
                            {
                                foreach (var groupItem in groupedItems)
                                {
                                    decimal innetQty = groupItem.Quantity;
                                    decimal innetGrossTotal = groupItem.UnitPrice;
                                    decimal inneritemdiscount = orderLine.UnitPrice - product.TempPrice.Value;

                                    if (MasterOrder.Type == OrderType.Return)
                                        inneritemdiscount = (-1) * inneritemdiscount;
                                    groupItem.ItemDiscount = inneritemdiscount;
                                    groupItem.DiscountedUnitPrice = innetGrossTotal - inneritemdiscount;
                                    groupItem.DiscountDescription = "Rabatterat pris";
                                }

                                orderLine.ItemDetails = groupedItems;
                            }
                        }

                        decimal qunatity = orderLine.Quantity;
                        decimal grossTotal = orderLine.UnitPrice;
                        decimal itemdiscount = (orderLine.UnitPrice - product.TempPrice.Value) * orderLine.Quantity;

                        if (MasterOrder.Type == OrderType.Return)
                            itemdiscount = (-1) * itemdiscount;
                        orderLine.DiscountedUnitPrice = MasterOrder.Type == OrderType.Return ? grossTotal + itemdiscount : grossTotal - itemdiscount;
                        grossTotal = grossTotal * qunatity;
                        orderLine.ItemDiscount = itemdiscount;
                        orderLine.DiscountDescription = "Rabatterat pris";

                        if (MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                        {
                            EditItem(orderLine);
                        }

                        App.MainWindow.UpdateItems(CurrentorderDetails);
                    }
                }
            }
        }

        private void ApplyCampain(Product product, OrderLine orderLine, int direction = 1)
        {
            var campaign = GetCampaign(product);
            //campaign.IsDiscount = true;
            //campaign.DiscountPercentage = 50;
            //campaign.LimitDiscountPercentage = 2;
#if DEBUG
            //campaign = new Campaign()
            //{
            //    IsDiscount=true,
            //    DiscountPercentage= Convert.ToDecimal(10.25),
            //    LimitDiscountPercentage=1,
            //    Description = "Buy 1 get next at 10",
            //    DiscountType = 1,
            //    StartDate = DateTime.Now.AddDays(-1),
            //    EndDate = DateTime.Now.AddDays(1)
            //};
#endif

            //campaign.IsDiscount = false;
            //campaign.FreeOffer = 1;
            //campaign.BuyLimit = 2;
            //campaign.Description = "Buy 2 Get 1 Free at all";
            if (campaign != null && campaign.StartDate < DateTime.Now && campaign.EndDate > DateTime.Now)
            {
                decimal directAmount = 0;

                if (orderLine.ItemType == ItemType.Grouped)
                {
                    var groupedItems = orderLine.ItemDetails;
                    if (groupedItems != null)
                    {
                        foreach (var groupItem in groupedItems)
                        {
                            decimal innetQty = groupItem.Quantity;
                            decimal innetGrossTotal = groupItem.UnitPrice;
                            decimal inneritemdiscount = 0;
                            if (campaign.IsDiscount)
                            {
                                if (groupItem.Quantity >= campaign.LimitDiscountPercentage)
                                {
                                    if (campaign.LimitDiscountPercentage > 0)
                                    {
                                        if (campaign.DiscountType == 0)
                                        {
                                            var percentageNew = Convert.ToDecimal(campaign.DiscountPercentage) / Convert.ToDecimal(campaign.LimitDiscountPercentage);
                                            groupItem.DiscountPercentage = percentageNew;
                                            inneritemdiscount = (innetGrossTotal / 100) * groupItem.DiscountPercentage;

                                            var roundedQuantity = innetQty;
                                            while (roundedQuantity > 0 && (roundedQuantity % campaign.LimitDiscountPercentage) != 0)
                                            {
                                                roundedQuantity = roundedQuantity - 1;
                                            }

                                            innetGrossTotal = roundedQuantity * inneritemdiscount;
                                        }
                                        else
                                        {
                                            var roundQuantity = innetQty;
                                            while (roundQuantity % (campaign.LimitDiscountPercentage + 1) != 0 && roundQuantity > 0)
                                            {
                                                roundQuantity--;
                                            }

                                            if (roundQuantity % (campaign.LimitDiscountPercentage + 1) == 0)
                                            {
                                                orderLine.DiscountPercentage = 100 * (orderLine.UnitPrice - campaign.DiscountPercentage) / orderLine.UnitPrice;

                                                inneritemdiscount = (innetGrossTotal / 100) * orderLine.DiscountPercentage;
                                                inneritemdiscount = (roundQuantity / (campaign.LimitDiscountPercentage + 1)) * inneritemdiscount;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (campaign.DiscountType == 0)
                                        {
                                            groupItem.DiscountPercentage = campaign.DiscountPercentage;
                                            inneritemdiscount = (innetGrossTotal / 100) * groupItem.DiscountPercentage;
                                            innetGrossTotal = innetQty * innetGrossTotal;
                                        }
                                        else
                                        {
                                            var expectedPrice = groupItem.UnitPrice * groupItem.Quantity;
                                            var discountedPrice = campaign.DiscountPercentage * groupItem.Quantity;
                                            groupItem.DiscountPercentage = 100 * (expectedPrice - discountedPrice) / expectedPrice;
                                            inneritemdiscount = (innetGrossTotal / 100) * groupItem.DiscountPercentage;
                                            inneritemdiscount = groupItem.Quantity * inneritemdiscount;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (campaign.FreeOffer > 0 && campaign.BuyLimit > 0)
                                {
                                        if (orderLine.Quantity >= (campaign.BuyLimit + campaign.FreeOffer))
                                        {
                                            int groups = Convert.ToInt32(orderLine.Quantity / (campaign.BuyLimit + campaign.FreeOffer));
                                            if (groups > 0)
                                            {
                                                var freeItems = groups * campaign.FreeOffer;
                                                inneritemdiscount = freeItems * groupItem.UnitPrice;
                                            }
                                        }
                                }
                                else
                                {
                                    inneritemdiscount = directAmount / groupedItems.Count;
                                }
                            }

                            if (MasterOrder.Type == OrderType.Return)
                                inneritemdiscount = (-1) * inneritemdiscount;
                            groupItem.ItemDiscount = inneritemdiscount;
                            groupItem.DiscountedUnitPrice = innetGrossTotal - inneritemdiscount;
                            groupItem.DiscountDescription = campaign.Description;
                        }

                        orderLine.ItemDetails = groupedItems;
                    }
                }

                decimal qunatity = orderLine.Quantity;
                decimal grossTotal = orderLine.UnitPrice;
                decimal itemdiscount = 0;

                if (campaign.IsDiscount)
                {
                    if (orderLine.Quantity > campaign.LimitDiscountPercentage)
                    {
                        if (campaign.LimitDiscountPercentage > 0)
                        {
                            if (campaign.DiscountType == 0)
                            {
                                var percentageNew = Convert.ToDecimal(campaign.DiscountPercentage) / Convert.ToDecimal(campaign.LimitDiscountPercentage);
                                orderLine.DiscountPercentage = percentageNew;
                                itemdiscount = (grossTotal / 100) * orderLine.DiscountPercentage;
                                var roundedQuantity = qunatity;
                                while (roundedQuantity > 0 && (roundedQuantity % campaign.LimitDiscountPercentage) != 0)
                                {
                                    roundedQuantity = roundedQuantity - 1;
                                }

                                itemdiscount = roundedQuantity * itemdiscount;
                            }
                            else
                            {
                                var roundQuantity = qunatity;
                                while (roundQuantity % (campaign.LimitDiscountPercentage + 1) != 0 && roundQuantity > 0)
                                {
                                    roundQuantity--;
                                }

                                if(roundQuantity % (campaign.LimitDiscountPercentage + 1) == 0)
                                { 
                                    orderLine.DiscountPercentage = 100 * (orderLine.UnitPrice - campaign.DiscountPercentage) / orderLine.UnitPrice;

                                    itemdiscount = (grossTotal / 100) * orderLine.DiscountPercentage;
                                    itemdiscount = (roundQuantity / (campaign.LimitDiscountPercentage + 1)) * itemdiscount;
                                }
                            }
                        }
                        else
                        {
                            if (campaign.DiscountType == 0)
                            {
                                orderLine.DiscountPercentage = campaign.DiscountPercentage;
                                itemdiscount = (grossTotal / 100) * orderLine.DiscountPercentage;
                                itemdiscount = qunatity * itemdiscount;
                            }
                            else
                            {
                                var expectedPrice = orderLine.UnitPrice * orderLine.Quantity;
                                var discountedPrice = campaign.DiscountPercentage * orderLine.Quantity;
                                orderLine.DiscountPercentage = 100 * (expectedPrice - discountedPrice) / expectedPrice;
                                itemdiscount = (grossTotal / 100) * orderLine.DiscountPercentage;
                                itemdiscount = orderLine.Quantity * itemdiscount;
                            }
                        }
                    }
                }
                else
                {
                    if (orderLine.Quantity >= campaign.BuyLimit)
                    {
                            if (orderLine.Quantity >= (campaign.BuyLimit + campaign.FreeOffer))
                            {
                                int groups = Convert.ToInt32(orderLine.Quantity / (campaign.BuyLimit + campaign.FreeOffer));
                                if (groups > 0)
                                {
                                    var freeItems = groups * campaign.FreeOffer;
                                    itemdiscount = freeItems * orderLine.UnitPrice;
                                }
                            }
                    }
                    else
                    {
                        itemdiscount = directAmount;
                    }
                }

                if (MasterOrder.Type == OrderType.Return)
                    itemdiscount = (-1) * itemdiscount;
                orderLine.DiscountedUnitPrice = MasterOrder.Type == OrderType.Return ? grossTotal + itemdiscount : grossTotal - itemdiscount;
                grossTotal = grossTotal * qunatity;
                orderLine.ItemDiscount = itemdiscount;
                orderLine.DiscountDescription = UI.Sales_Discount;
                orderLine.DiscountDescription = "Discount: (" + campaign.Description + ")";

                if (MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    EditItem(orderLine);
                }

                App.MainWindow.UpdateItems(CurrentorderDetails);
            }
        }

        private Campaign GetCampaign(Product product)
        {
            try
            {
                return new ProductRepository(PosState.GetInstance().Context).GetCampaign(product.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void AddGroupItemToCart(Product product, decimal qty, List<Product> products)
        {
            // EXTRA VALIDATION, INCASE ORDER OBJECT IS NOT ALREADY CREATED
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll && (MasterOrder == null || MasterOrder.Id == default(Guid)))
            {
                _view.NewEntry();
            }
            try
            {
                var data =
                    CurrentorderDetails.Where(p => p.ItemId == product.Id && p.UnitPrice == product.Price).ToList();
                if (data.Count > 0)
                {
                    var itm = data.First();
                    AddQuantity(itm, qty);
                    return;
                }

                int addCartIndex = _view.GetCartIndex();
                //product.AskDescription = false;
                //// TODO DANIEL, har ej skapat fält i databasen ännu. Sätter itemname som av ngn anledning bara visas på bongen

                //if (product.AskDescription)
                //    comment = Utilities.PromptInput("Ange kommentar", "Kommentar", product.Description);

                decimal grossTotal = qty * product.Price;

                decimal tax = grossTotal / 100 * product.Tax;

                int itemIdex = 0;

                if (CurrentorderDetails.Count > 0)
                {
                    itemIdex = CurrentorderDetails.Max(od => od.ItemIdex);
                }
                var orderdetailiems =
                    products.Select(
                        item =>
                            new OrderLine
                            {
                                ItemComments = "",
                                ItemIdex = itemIdex + 1,
                                TaxPercent = item.Tax,
                                Quantity = qty,
                                UnitPrice = item.Price,
                                DiscountedUnitPrice = item.Price,
                                PurchasePrice = item.PurchasePrice,
                                Active = 1,
                                ItemStatus = (int)OrderStatus.New,
                                Direction = MasterOrder.OrderDirection,
                                BIDSNO = 0,
                                PrinterId = item.PrinterId,
                                OrderType = MasterOrder.Type,
                                Percentage = tax,
                                ItemId = item.Id,
                                GroupId = product.Id,
                                ItemType = ItemType.Individual,
                                Product = item,
                                Description = item.Description
                            }).ToList();

                if (MasterOrder != null)
                {
                    var orderItem = new OrderLine
                    {
                        ItemComments = "",
                        ItemIdex = itemIdex + 1,
                        TaxPercent = product.Tax,
                        Quantity = qty,
                        UnitPrice = product.Price,
                        DiscountedUnitPrice = product.Price,
                        PurchasePrice = product.PurchasePrice,
                        Active = 1,
                        ItemStatus = (int)OrderStatus.New,
                        Direction = MasterOrder.OrderDirection,
                        BIDSNO = 0,
                        PrinterId = product.PrinterId,
                        OrderType = MasterOrder.Type,
                        Percentage = tax,
                        ItemId = product.Id,
                        GroupId = default(Guid),
                        ItemType = ItemType.Grouped,
                        ItemDetails = orderdetailiems,
                        Product = product
                    };


                    if (MasterOrder.Type == OrderType.Return || MasterOrder.Type == OrderType.TakeAwayReturn)
                        orderItem.ItemStatus = (int)OrderStatus.ReturnOrder;
                    if (addCartIndex != -1)
                    {
                        CurrentorderDetails.Insert(addCartIndex + 1, orderItem);
                    }
                    else
                    {
                        CurrentorderDetails.Add(orderItem);
                    }
                    MasterOrder.OrderLines = CurrentorderDetails;
                    RefreshNewItem();

                    _view.SetCartItems(CurrentorderDetails);
                    _view.SetEntryMode(EntryModeType.CodeEntry);



                    if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        AddOrderLine(MasterOrder, orderItem);
                        MasterOrder.Updated = 1;
                        //  OrderEntry(MasterOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void AddQuantity(OrderLine orderDetail, decimal newqty)
        {
            var product = orderDetail.Product;
            string logmessage = "(" + orderDetail.ItemName + ")" + " - is added for order#: " + MasterOrder.OrderNoOfDay + " |qty:" + orderDetail.Quantity + " unitprice: " + orderDetail.UnitPrice + " vat: " + Math.Round(orderDetail.VatAmount(), 2) + " vat%: " + Math.Round(orderDetail.TaxPercent, 2);
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ItemAdded), MasterOrder.Id, orderDetail.ItemId, null, logmessage);
            //  var productPresenter = new ProductPresenter();

            // var item = productPresenter.GetProductById(orderItem.ItemId);

            decimal quantitiy = orderDetail.Quantity;
            quantitiy = quantitiy + newqty;

            decimal grossTotal = orderDetail.UnitPrice;

            if (orderDetail.DiscountPercentage > 0)
            {
                decimal itemdiscount = grossTotal / 100 * orderDetail.DiscountPercentage;
                itemdiscount = quantitiy * itemdiscount;
                orderDetail.ItemDiscount = itemdiscount;
                orderDetail.DiscountedUnitPrice = quantitiy * grossTotal - itemdiscount;
            }


            orderDetail.Quantity = quantitiy;

            if (orderDetail.ItemType == ItemType.Grouped)
            {
                if (orderDetail.ItemDetails != null)
                {
                    var details = orderDetail.ItemDetails;
                    foreach (var ditem in details)
                    {
                        ditem.Quantity = quantitiy;

                    }
                }
            }

            /*Adding ingredient quantity*/
            if (orderDetail.IngredientItems != null)
            {
                var ingredients = orderDetail.IngredientItems;
                foreach (var gredientItem in ingredients)
                {
                    gredientItem.Quantity = quantitiy;

                }

            }


            //WAQAS
            _view.SetCartItems(CurrentorderDetails);
            ApplyTempPrice(orderDetail.Product, orderDetail);
            ApplyCampain(orderDetail.Product, orderDetail);
            //Update Item to database

            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                EditItem(orderDetail);
                MasterOrder.Updated = 1;
                OrderEntry(MasterOrder);
            }
        }


        public void RemoveItemQuantity(OrderLine orderDetail, decimal qunatity)
        {
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                var detail = new OrderLine
                {
                    Id = orderDetail.Id,
                    OrderId = orderDetail.OrderId,
                    ItemId = orderDetail.ItemId,
                    UnitPrice = orderDetail.UnitPrice,
                    UnitsInPackage = orderDetail.UnitsInPackage,
                    ItemComments = orderDetail.ItemComments,
                    DiscountedUnitPrice = orderDetail.DiscountedUnitPrice,
                    DiscountPercentage = orderDetail.DiscountPercentage,
                    TaxPercent = orderDetail.TaxPercent,
                    PurchasePrice = orderDetail.PurchasePrice,
                    Active = orderDetail.Active,
                    Percentage = orderDetail.Percentage,
                    Product = orderDetail.Product,
                    ItemDetails = orderDetail.ItemDetails,
                    ItemType = orderDetail.ItemType,
                    Quantity = orderDetail.Quantity
                };
                RemoveItem(detail);
            }

            var productPresenter = new ProductPresenter();

            var item = productPresenter.GetProductById(orderDetail.ItemId);

            decimal grossTotal = orderDetail.UnitPrice;

            if (orderDetail.DiscountPercentage > 0)
            {
                decimal itemdiscount = grossTotal / 100 * orderDetail.DiscountPercentage;
                itemdiscount = qunatity * itemdiscount;
                orderDetail.ItemDiscount = itemdiscount;
                orderDetail.DiscountedUnitPrice = qunatity * grossTotal - itemdiscount;
            }


            orderDetail.Quantity = qunatity;



            if (orderDetail.ItemDetails != null)

                foreach (var itm in orderDetail.ItemDetails)
                {
                    itm.Quantity = itm.Quantity - 1;
                    decimal itemdiscount = grossTotal / 100 * itm.DiscountPercentage;
                    itemdiscount = qunatity * itemdiscount;
                    itm.ItemDiscount = itemdiscount;
                    itm.DiscountedUnitPrice = qunatity * grossTotal - itemdiscount;

                }

            /*Removing ingredient quantity*/
            if (orderDetail.IngredientItems != null)
            {
                var ingredients = orderDetail.IngredientItems;
                foreach (var gredientItem in ingredients)
                {
                    gredientItem.Quantity = qunatity;
                }
            }

            ApplyTempPrice(orderDetail.Product, orderDetail, -1); // -1 mean item is deleting from cart
            ApplyCampain(orderDetail.Product, orderDetail, -1); // -1 mean item is deleting from cart

            _view.SetCartItems(CurrentorderDetails);
            //Update Item to database

            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                EditItem(orderDetail);
                MasterOrder.Updated = 1;
                OrderEntry(MasterOrder);
            }
        }


        internal void SendPrintToKitchen(Order masterOrder)
        {
            new DirectPrint().PrintBong(masterOrder, true);
            //if (MasterOrder != null && MasterOrder.IsForAdult)
            //{
            //    _view.ShowSurvey();
            //}
            _view.NewRecord();
        }

        //private void RemoveQuantity(OrderLine orderItem)
        //{
        //    new OrderRepository().RemoveQuantity(orderItem, Defaults.User.Id);
        //}
        internal void SendPrintToKitchenWithoutReset(Order masterOrder)
        {
            new DirectPrint().PrintBong(masterOrder, true);
        }


        #endregion

        #region Event handling

        internal void HandleCashDrawerClick()
        {
            PosState.OpenDrawer();
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
            FocusTextBox();
        }
        internal void HandleHoldClick()
        {
            try
            {
                if (CurrentorderDetails.Count == 0)
                {

                    App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                    return;
                }
                if (Defaults.ShowBongAlert && Defaults.SaleType == SaleType.Restaurant)
                {
                    //ConfirmWindow confirmWindow = new ConfirmWindow(UI.Global_Bong_Confirm);
                    //if (confirmWindow.ShowDialog() == false)
                    //    return;
                    if (MessageBox.Show(UI.Global_Bong_Confirm, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                var mergeOrder = _view.GetMergeOrder();
                var selectedTable = _view.GetSelectedTable();
                if (_view.IsMerge() && mergeOrder != null)
                    SetMergeOrder();
                if ((selectedTable == null || selectedTable.Id == 0) && Defaults.TableNeededOnBong && MasterOrder.Type != OrderType.TakeAway)
                {
                    var tableWindow = new TableWindow();
                    //  this.IsEnabled = false;
                    if (tableWindow.ShowDialog() == true)
                    {
                        selectedTable = tableWindow.SelectedTable;
                        MasterOrder.SelectedTable = selectedTable;
                        MasterOrder.TableId = selectedTable.Id;
                    }
                }
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    MasterOrder.Status = OrderStatus.AssignedKitchenBar;
                    if (selectedTable != null && selectedTable.Id > 0)
                    {
                        MasterOrder.SelectedTable = selectedTable;
                        MasterOrder.TableId = selectedTable.Id;
                        if (string.IsNullOrEmpty(MasterOrder.Comments))
                            MasterOrder.Comments = selectedTable.Name;

                    }
                    MasterOrder.Updated = 1;
                    var printList = CurrentorderDetails.Where(itm => itm.Product.Bong && itm.ItemStatus != 3).ToList();
                    var groupItemList = CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                    foreach (var itm in groupItemList)
                    {
                        var lst = itm.ItemDetails.Where(i => i.Product.Bong).Select(i => new OrderLine
                        {
                            OrderId = MasterOrder.Id,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            ItemId = i.Product.Id,
                            ItemComments = i.ItemComments,
                            Product = i.Product


                        }).ToList();
                        printList.AddRange(lst);
                    }
                    UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                    OrderEntry(MasterOrder);

                    if (Defaults.BONG && Defaults.SaleType == SaleType.Restaurant)
                        SendPrintToKitchen(MasterOrder, printList);
                    if (MasterOrder != null && MasterOrder.IsForAdult)
                    {
                        _view.ShowSurvey();
                    }
                    _view.NewRecord();
                    //_view.NewEntry();

                }

                else
                {
                    SaveOrder();
                    _view.NewEntry();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }


        }



        internal decimal GetCardDiscount()
        {
            return 0;
            // throw new NotImplementedException();
        }

        public void HandleMergeBongOrder()
        {
            if (CurrentorderDetails.Count == 0)
            {
                App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                return;
            }


            var selectedTable = _view.GetSelectedTable();
            var orders = GetOpenOrdersOnTable(selectedTable.Id);
            if (orders.Count > 0)
            {
                //if (Defaults.ShowBongAlert)
                //{
                //    ConfirmWindow confirmWindow = new ConfirmWindow(UI.Global_Bong_Confirm);
                //    if (confirmWindow.ShowDialog() == false)
                //        return;

                //}
                var order = orders.OrderByDescending(o => o.CreationDate).FirstOrDefault();
                MasterOrder.Updated = 1;
                var newList = CurrentorderDetails;
                var printList = CurrentorderDetails.Where(itm => itm.Product.Bong && itm.ItemStatus != 3).ToList();
                var groupItemList = CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                foreach (var itm in groupItemList)
                {
                    var lst = itm.ItemDetails.Where(i => i.Product.Bong).Select(i => new OrderLine
                    {
                        OrderId = MasterOrder.Id,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        ItemId = i.Product.Id,
                        ItemComments = i.ItemComments,
                        Product = i.Product


                    }).ToList();
                    printList.AddRange(lst);
                }
                CancelOrder();
                foreach (var lst in newList)
                {
                    lst.OrderId = order.Id;
                }
                UpdateOrderDetail(newList, order);
                order.OrderTotal += newList.Sum(o => o.GrossTotal);
                MasterOrder = new OrderRepository(PosState.GetInstance().Context).SaveOrderMaster(order);
                if (Defaults.BONG)
                {
                    SendPrintToKitchen(order, printList);

                }
                if (MasterOrder != null && MasterOrder.IsForAdult)
                {
                    _view.ShowSurvey();
                }
                _view.NewRecord();
                _view.NewEntry();
            }
            else
            {
                HandleHoldClick();
            }
        }

        internal void HandlePerformaClick(Guid orderId)
        {
            try
            {
                LogWriter.JournalLog(JournalActionCode.PrintPerforma, orderId);

                var directPrint = new DirectPrint();
                directPrint.PrintBill(orderId);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        internal void HandleCancelOrderClick(Guid id)
        {
            try
            {
                new OrderRepository(PosState.GetInstance().Context).CancelOrder(id, Defaults.User.Id);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        internal void HandleCheckOutClick()
        {
            Defaults.PerformanceLog.Add("Clicked On Check out Button   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            if (CurrentorderDetails.Count == 0)
            {
                App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                return;
            }

            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                MasterOrder.Status = MasterOrder.OrderStatusFromType;
                MasterOrder.Updated = 1;
                MasterOrder.CheckOutUserId = Defaults.User.Id;
                var printList = CurrentorderDetails.Where(i => i.Product.Bong && i.ItemStatus != 3).ToList();
                UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                OrderEntry(MasterOrder);


                //if (Presenter.MasterOrder.Status!=OrderStatus.ReturnOrder && Defaults.BONG)
                //  Presenter.SendPrintToKitchen(Presenter.MasterOrder,false);

                LogWriter.CheckOutLogWrite("Checkout order started", MasterOrder.Id);

                _orderId = MasterOrder.Id;


                bool statusComplete = false;
                _type = _view.GetOrderType();
                _orderDirection = _type == OrderType.Return ? -1 : 1;

                MasterOrder.Type = _type;

                var progressDialog = new ProgressWindow();

                var backgroundThread = new Thread(() =>
                {
                    progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                        decimal grossTotal = MasterOrder.GrossAmount;
                        MasterOrder.OrderTotal = grossTotal;

                        LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.PaymentScreenNavigation), MasterOrder.Id);

                        Defaults.PerformanceLog.Add("Calling Pament Window         ");

                        var obj = new CheckOutOrderWindow(grossTotal, MasterOrder.Id, _orderDirection, CurrentorderDetails,
                            MasterOrder.TableId, this.ToString());
                        statusComplete = obj.ShowDialog() ?? false;

                        if (statusComplete)
                        {

                            //i.ItemStatus == (int)OrderStatus.New &&
                            var groupItemList = CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                            foreach (var itm in groupItemList)
                            {
                                var lst =
                                    itm.ItemDetails.Where(i => i.Product.Bong && i.ItemStatus != 3)
                                        .Select(
                                            i =>
                                                new OrderLine
                                                {
                                                    OrderId = MasterOrder.Id,
                                                    Quantity = i.Quantity,
                                                    UnitPrice = i.UnitPrice,
                                                    ItemId = i.Product.Id,
                                                    Product = i.Product
                                                })
                                        .ToList();
                                printList.AddRange(lst);
                            }

                            if (printList.Count > 0 && Defaults.SaleType == SaleType.Restaurant && Defaults.BONG && MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder)
                            {
                                var drctOrderPrint = new DirectPrint();
                                var order = new Order
                                {
                                    Id = MasterOrder.Id,
                                    CreationDate = MasterOrder.CreationDate,
                                    Bong = MasterOrder.Bong,
                                    DailyBong = MasterOrder.DailyBong,
                                    Type = MasterOrder.Type,
                                    TableId = MasterOrder.TableId,
                                    TableName = MasterOrder.TableName,
                                    OrderComments = MasterOrder.OrderComments,
                                    OrderLines = printList
                                };

                                drctOrderPrint.PrintBong(order, true);
                            }

                            if (obj.ReceiptGenerated)
                            {
                                Defaults.PerformanceLog.Add("Sending for print invoice");
                                LogWriter.CheckOutLogWrite("Sending receipt for print invoice", MasterOrder.Id);
                                //  var invoiceWindow = new PrintNewInvoiceWindow();
                                var directPrint = new DirectPrint();

                                //directPrint.PrintReceipt(MasterOrder.Id, false, obj.ReceiptGenerated);

                                if (Defaults.AskForPrintInvoice)
                                {
                                    if (MessageBox.Show("Do you want to print receipt ?", "POSSUM", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        directPrint.PrintReceipt(MasterOrder.Id, false, obj.ReceiptGenerated);
                                        LogWriter.CheckOutLogWrite("Receipt printed after user permission", MasterOrder.Id);
                                    }
                                    else
                                    {
                                        LogWriter.CheckOutLogWrite("Receipt printed cancel by user", MasterOrder.Id);
                                    }
                                }
                                else
                                {
                                    directPrint.PrintReceipt(MasterOrder.Id, false, obj.ReceiptGenerated);
                                    LogWriter.CheckOutLogWrite("Receipt printed", MasterOrder.Id);
                                }

                                if (_type == OrderType.Return)
                                {
                                    // invoiceWindow.PrintReturnInvoice(orderId);
                                    LogWriter.JournalLog(
                                                Defaults.User.TrainingMode
                                                    ? JournalActionCode.ReceiptPrintedForReturnOrderViaTrainingMode
                                                    : JournalActionCode.ReceiptPrintedForReturnOrder, MasterOrder.Id);
                                }
                                else
                                {
                                    //  invoiceWindow.PrintInvoice(orderId, false);
                                    LogWriter.JournalLog(
                                                Defaults.User.TrainingMode
                                                    ? JournalActionCode.ReceiptPrintedViaTrainingMode
                                                    : JournalActionCode.ReceiptPrinted, MasterOrder.Id);
                                }
                            }
                        }
                    }));
                });
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();

                if (statusComplete)
                {
                    LogWriter.CheckOutLogWrite("Checkout completed successfully", MasterOrder.Id);
                    App.MainWindow.UpdateOrderCompleted(MasterOrder.Id);
                    App.MainWindow.ItemsforPublishMessage(CurrentorderDetails);
                    if (MasterOrder != null && MasterOrder.IsForAdult)
                    {
                        _view.ShowSurvey();
                    }
                    _view.NewRecord();
                    _view.NewEntry();
                    UpdateHistoryGrid();
                }
            }
            else
                HandleHoldClick();
            Defaults.PerformanceLog.Add("Checkout starting....         -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

        }

        internal void HandleDiscountClick()
        {
            var selectedLine = _view.GetSelectedOrderLine();


            if (selectedLine != null && selectedLine.Product.DiscountAllowed)
            {
                DiscountWindow discountWindow = new DiscountWindow(selectedLine.Product.Price);
                if (discountWindow.ShowDialog() == true)
                {
                    if (selectedLine.ItemType == ItemType.Grouped)
                    {
                        var groupedItems = selectedLine.ItemDetails;
                        if (groupedItems != null)
                        {
                            foreach (var groupItem in groupedItems)
                            {
                                decimal innetQty = groupItem.Quantity;
                                decimal innetGrossTotal = groupItem.UnitPrice;
                                decimal inneritemdiscount;
                                if (discountWindow.Percentage)
                                {
                                    groupItem.DiscountPercentage = discountWindow.Amount;
                                    inneritemdiscount = (innetGrossTotal / 100) * groupItem.DiscountPercentage;
                                    innetGrossTotal = innetQty * innetGrossTotal;
                                }
                                else
                                {
                                    // ucCart.SelectedItem.DiscountPercentage = (discountWindow.Amount * 100) / grossTotal;
                                    inneritemdiscount = discountWindow.Amount / groupedItems.Count;
                                }
                                if (MasterOrder.Type == OrderType.Return)
                                    inneritemdiscount = (-1) * inneritemdiscount;
                                groupItem.ItemDiscount = inneritemdiscount;
                                groupItem.DiscountedUnitPrice = innetGrossTotal - inneritemdiscount;


                            }
                            selectedLine.ItemDetails = groupedItems;
                        }
                    }
                    decimal qunatity = selectedLine.Quantity;
                    decimal grossTotal = selectedLine.UnitPrice;
                    decimal itemdiscount;


                    if (discountWindow.Percentage)
                    {
                        selectedLine.DiscountPercentage = discountWindow.Amount;
                        itemdiscount = (grossTotal / 100) * selectedLine.DiscountPercentage;
                        itemdiscount = qunatity * itemdiscount;
                    }
                    else
                    {
                        // ucCart.SelectedItem.DiscountPercentage = (discountWindow.Amount * 100) / grossTotal;
                        itemdiscount = discountWindow.Amount;
                    }
                    // grossTotal = grossTotal - itemdiscount;
                    if (MasterOrder.Type == OrderType.Return)
                        itemdiscount = (-1) * itemdiscount;
                    selectedLine.DiscountedUnitPrice = MasterOrder.Type == OrderType.Return ? grossTotal + itemdiscount : grossTotal - itemdiscount;

                    selectedLine.ItemDiscount = itemdiscount;
                    selectedLine.DiscountDescription = UI.Sales_Discount;

                    if (MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        EditItem(selectedLine);
                    }
                    MasterOrder.OrderLines = CurrentorderDetails;
                    _view.SetCartItems(CurrentorderDetails);
                    // ucCart.RefreshItems();
                    App.MainWindow.UpdateItems(CurrentorderDetails);

                }
            }
            FocusTextBox();
        }

        internal void ShowSurvey()
        {
            _view.ShowSurvey();
        }

        //internal void HandleDiscountCompleteOrderClick()
        //{


        //    if (CurrentorderDetails != null && CurrentorderDetails.Count > 0)
        //    {
        //        DiscountWindow discountWindow = new DiscountWindow();
        //        if (discountWindow.ShowDialog() == true)
        //        {
        //            CurrentorderDetails = new OrderRepository(PosState.GetInstance().Context).GetOrderLinesById(MasterOrder.Id);
        //            MasterOrder.OrderLines = CurrentorderDetails;
        //            _view.SetCartItems(CurrentorderDetails);
        //            // ucCart.RefreshItems();
        //            App.MainWindow.UpdateItems(CurrentorderDetails);

        //        }
        //    }
        //    FocusTextBox();
        //}

        internal void DispenseAmountCG()
        {
            throw new NotImplementedException();
        }

        internal void HandlePluClick()
        {
            int BIDSNO = 0;
            string pluCode;

            var currentQty = _view.GetCurrentQty();
            string value = GetTexBoxValue();
            if (value.Contains("x"))
            {
                string[] values = value.Split('x');
                currentQty = Convert.ToInt16(values[0]);
                pluCode = values[1];
            }
            else
                pluCode = value;

            if (string.IsNullOrEmpty(pluCode))
            {
                _view.ShowError(UI.Global_Warning, UI.Message_PLUMissing);
                FocusTextBox();
                return;
            }
            var product = GetProductByPlu(pluCode);
            if (product.Id == default(Guid))
            {

                _view.ShowError(UI.Global_Warning, UI.Message_InvalidPLU);
                FocusTextBox();
                return;
            }

            if (MasterOrder.Status == OrderStatus.Completed)
                return;

            var selectedItem = product;
            if (product.AskPrice)
            {

                _view.SetEntryMode(EntryModeType.ItemEntry);
                FocusTextBox();
                _view.SetAskPriceValue(UI.Sales_EnterPrice + ":");
            }
            else if (product.AskWeight)
            {
                var weight = GetWeightFromScale();

                if (weight == -1)
                {
                    _view.SetEntryMode(EntryModeType.ItemEntry);
                    FocusTextBox();
                    _view.SetAskPriceValue(UI.EnterWeight + ":");

                }
                else
                {
                    if (selectedItem.Unit == ProductUnit.g)
                    {
                        //Grams
                        currentQty = Convert.ToDecimal(weight);
                    }
                    else if (selectedItem.Unit == ProductUnit.kg)
                    {
                        //Kilograms
                        var grams = Convert.ToDecimal(weight);
                        currentQty = grams / 1000;

                    }
                    else if (selectedItem.Unit == ProductUnit.hg)
                    {
                        //Hectograms
                        var grams = Convert.ToDecimal(weight);
                        currentQty = grams / 100;
                    }
                    AddItemToCart(product, currentQty, BIDSNO);
                    currentQty = 1;
                    _view.SetCurrentQty(currentQty);
                    _view.SetAskByUser(false);
                }
            }
            else
            {
                AddItemToCart(selectedItem, currentQty, BIDSNO);
                _view.SetCurrentQty(currentQty);
                _view.SetAskByUser(false);
            }
        }
        internal void HandleSwishClick()
        {
            bool isMerge = _view.IsMerge();
            var mergeOrder = _view.GetMergeOrder();
            var selectedTable = _view.GetSelectedTable();
            if (CurrentorderDetails.Count == 0)
            {
                App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                return;
            }
            Defaults.PerformanceLog.Add("Clicked On direct Swish payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

            if (isMerge && mergeOrder != null)
                SetMergeOrder();
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                MasterOrder.Status = MasterOrder.OrderStatusFromType;
                if (selectedTable != null)
                {
                    MasterOrder.SelectedTable = selectedTable;
                    MasterOrder.TableId = selectedTable.Id;
                    MasterOrder.Comments = selectedTable.Name;
                }
                MasterOrder.Updated = 1;
                MasterOrder.CheckOutUserId = Defaults.User.Id;
                UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                MasterOrder = OrderEntry(MasterOrder);

                //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                //    Presenter.SendPrintToKitchen(Presenter.MasterOrder,false);
            }
            else
                SaveOrder();
            Defaults.PerformanceLog.Add("Direct Swish Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            HandelDirectSwishPaymentClick(true);
        }
        internal void HandleCreditCardClick()
        {
            if (CurrentorderDetails.Count == 0)
            {
                App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                return;
            }
            var selectedTable = _view.GetSelectedTable();
            Defaults.PerformanceLog.Add("Clicked On direct card payment -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                if (selectedTable != null)
                {
                    MasterOrder.SelectedTable = selectedTable;
                    MasterOrder.TableId = selectedTable.Id;
                    MasterOrder.Comments = selectedTable.Name;
                }
                MasterOrder.Status = MasterOrder.OrderStatusFromType;
                MasterOrder.Updated = 1;
                MasterOrder.CheckOutUserId = Defaults.User.Id;
                UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                MasterOrder = OrderEntry(MasterOrder);

            }
            else
                SaveOrder();
            Defaults.PerformanceLog.Add("Direct Card Payment Started    -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            HandleDirectPaymentClick(true);
        }
        internal void HanleDirectCashClick()
        {

            if (CurrentorderDetails.Count == 0)
            {
                App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                return;
            }
            var selectedTable = _view.GetSelectedTable();
            Defaults.PerformanceLog.Add("Clicked On direct cash payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                MasterOrder.Status = MasterOrder.OrderStatusFromType;
                if (selectedTable != null)
                {
                    MasterOrder.SelectedTable = selectedTable;
                    MasterOrder.TableId = selectedTable.Id;
                    MasterOrder.Comments = selectedTable.Name;
                }
                MasterOrder.Updated = 1;
                MasterOrder.CheckOutUserId = Defaults.User.Id;
                UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                MasterOrder = OrderEntry(MasterOrder);

            }
            else
                SaveOrder();
            Defaults.PerformanceLog.Add("Direct Cash Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            HandleDirectPaymentClick(false);
        }
        internal void HandleTableOrderMergTClick()
        {
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                MasterOrder.Status = OrderStatus.AssignedKitchenBar;
                MasterOrder.Updated = 1;
                MasterOrder.Type = OrderType.TableOrder;
                UpdateOrderDetail(CurrentorderDetails, MasterOrder);

                OrderEntry(MasterOrder);

            }
        }
        internal void HandleCreateNewClick()
        {
            var type = _view.GetOrderType();
            var selectedTable = _view.GetSelectedTable();
            MasterOrder.Type = type;
            MasterOrder.SelectedTable = selectedTable;
            MasterOrder.TableId = selectedTable.Id;
            MasterOrder.Comments = selectedTable.Name;
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                MasterOrder.Status = OrderStatus.AssignedKitchenBar;
                MasterOrder.Updated = 1;
                OrderEntry(MasterOrder);
                if (CurrentorderDetails.Count > 0)
                {
                    UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                }

            }
        }

        internal void HandleSplitOrderClick()
        {
            if (MasterOrder != null && CurrentorderDetails.Count > 0)
            {
                var printList = CurrentorderDetails.Where(itm => itm.Product.Bong && itm.ItemStatus != 3).ToList();
                var groupItemList = CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                foreach (var itm in groupItemList)
                {
                    var lst = itm.ItemDetails.Where(i => i.Product.Bong).Select(i => new OrderLine
                    {
                        OrderId = MasterOrder.Id,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        ItemId = i.Product.Id,
                        ItemComments = i.ItemComments,
                        Product = i.Product


                    }).ToList();
                    printList.AddRange(lst);
                }
                if (Defaults.BONG && MasterOrder.Status == OrderStatus.New)
                    SendPrintToKitchen(MasterOrder, printList, false);
                var splitOrderWindow = new SplitOrderWindow(MasterOrder);

                if (splitOrderWindow.ShowDialog() == true)
                {
                    CurrentorderDetails = new List<OrderLine>();
                    MasterOrder = new Order();
                    MasterOrder.OrderLines = CurrentorderDetails;
                    _view.SetCartItems(CurrentorderDetails);
                    if (MasterOrder != null && MasterOrder.IsForAdult)
                    {
                        _view.ShowSurvey();
                    }
                    _view.NewEntry();
                }
                else
                    DisplayOrder(MasterOrder.Id);
            }
        }

        internal void HandleNewButtonClick()
        {
            _view.SetOrderTypeSecondaryVisibility(false);
            if (MasterOrder.Status == OrderStatus.New)
                CancelOrder();
            _view.NewRecord();
            _view.NewEntry();
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeNew), MasterOrder.Id);

        }

        internal void HandleSearchItemClick()
        {
            SearchProductWindow productWindow = new SearchProductWindow();
            if (productWindow.ShowDialog() == true)
            {
                decimal currentQty = _view.GetCurrentQty();
                _view.SetEntryMode(EntryModeType.ItemEntry);
                var product = productWindow.SelectedProduct;
                int bidsno = _view.GetBIDSNO();
                if (product.Id == default(Guid)) return;
                if (!string.IsNullOrEmpty(_view.GetAskPriceValue()))
                {
                    currentQty = 1;
                    FocusTextBox();
                }
                string textBoxValue = _view.GetTextBoxValue();
                if (!string.IsNullOrEmpty(textBoxValue))
                {
                    if (textBoxValue.Length > 10)
                    {
                        MessageBox.Show(UI.Message_InvalidQty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        FocusTextBox();
                        return;
                    }
                    decimal.TryParse(textBoxValue, out currentQty);
                    if (currentQty == 0)
                    {
                        currentQty = 1;
                    }
                }
                _view.SetSelectedItem(product);
                var orderDetail =
                    CurrentorderDetails.FirstOrDefault(
                        p => p.ItemId == product.Id && p.ItemStatus == 0 && p.BIDSNO == bidsno);
                if (orderDetail != null && product.AskPrice == false && product.AskWeight == false)
                {
                    HandelAddProductClick(orderDetail, currentQty, bidsno);
                    return;
                }
                if (product.AskPrice)
                {
                    _view.SetAskByUser(true);
                    _view.SetAskPriceValue(UI.Sales_EnterPrice); // "Ange pris :";
                    _view.SetEntryMode(EntryModeType.PluEntry);
                    FocusTextBox();
                }
                else if (product.AskWeight)
                {
                    //Weight From Scale
                    var weight = GetWeightFromScale();
                    if (weight == -1)
                    {
                        _view.SetAskByUser(true);
                        _view.SetAskPriceValue(UI.EnterWeight); // "Ange pris :";
                        _view.SetEntryMode(EntryModeType.PluEntry);
                        FocusTextBox();
                    }
                    else
                    {
                        if (product.Unit == ProductUnit.g)
                        {
                            //Grams
                            currentQty = Convert.ToDecimal(weight);
                        }
                        else if (product.Unit == ProductUnit.kg)
                        {
                            //Kilograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 1000;
                        }
                        else if (product.Unit == ProductUnit.hg)
                        {
                            //Hectograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 100;
                        }
                        AddItemToCart(product, currentQty, bidsno);
                        _view.SetCurrentQty(1);
                        _view.SetAskByUser(false);
                    }
                }
                else
                {
                    AddItemToCart(product, currentQty, bidsno);
                    _view.SetCurrentQty(1);
                    _view.SetAskByUser(false);
                }
            }
        }
        internal void HandleCustomerInfoClick()
        {
            if (Defaults.CustomerOrderInfo)
            {
                var customerwindow = new CustomerWindow(false, UI.Sales_CustomerButton, CustomerType.All);
                if (customerwindow.ShowDialog() == true)
                {
                    MasterOrder.CustomerId = customerwindow.SelectedCustomer.Id;
                    MasterOrder.Comments = customerwindow.SelectedCustomer.Name;

                }
            }
            else
            {
                var title = UI.Sales_CustomerButton;
                MasterOrder.Comments = Utilities.PromptInput(title, UI.Sales_EnterComment, MasterOrder.OrderComments);
            }
            FocusTextBox();
        }
        public void CancelOrder()
        {
            try
            {
                if (MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    MasterOrder.Status = OrderStatus.OrderCancelled;
                    UpdateOrderEntry();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        public void DisplayOrder(Guid orderId)
        {
            try
            {
                MasterOrder = GetOrderMasterDetailById(orderId);
                if (MasterOrder != null)
                {
                    if (!string.IsNullOrEmpty(MasterOrder.OrderComments))
                    {
                        _view.SetOrderCommentsVisibility(true, MasterOrder.OrderComments);

                    }
                    else
                    {
                        _view.SetOrderCommentsVisibility(false, "");
                    }
                    CurrentorderDetails = MasterOrder.OrderLines.ToList();
                    MasterOrder.OrderLines = CurrentorderDetails;
                    _view.SetCartItems(CurrentorderDetails);
                    if (CurrentorderDetails.Count == 0)
                        _view.NewEntry();
                }
            }
            catch (Exception exp)
            {
                _view.ShowError(Defaults.AppProvider.AppTitle, exp.Message);
            }
        }

        public void RefreshNewItem()
        {
            FocusTextBox();

            SetSelectedItem(null);
            _view.SetEntryMode(EntryModeType.CodeEntry);
            if (Defaults.CustomerView)
                App.MainWindow.UpdateItems(CurrentorderDetails);
        }
        public void UpdateOrderEntry()
        {
            MasterOrder.Updated = 1;
            OrderEntry(MasterOrder);
        }
        public void SetMergeOrder()
        {
            try
            {
                var mergeOrder = _view.GetMergeOrder();
                DiscardOrder(MasterOrder.Id);

                MasterOrder = new Order
                {
                    Id = mergeOrder.Id,
                    CreationDate = mergeOrder.CreationDate,
                    TableId = mergeOrder.TableId,
                    Status = mergeOrder.Status,
                    PaymentStatus = mergeOrder.PaymentStatus,
                    OrderLines = mergeOrder.OrderLines,
                    OrderNoOfDay = mergeOrder.OrderNoOfDay,
                    ShiftNo = mergeOrder.ShiftNo,
                    ShiftOrderNo = mergeOrder.ShiftOrderNo,
                    OrderTotal = mergeOrder.OrderTotal,
                    TaxPercent = mergeOrder.TaxPercent,
                    UserId = mergeOrder.UserId,
                    Updated = mergeOrder.Updated,
                    // Outlet = _mergeOrder.Outlet,
                    OutletId = Defaults.Outlet.Id,
                    TerminalId = mergeOrder.TerminalId,
                    TrainingMode = mergeOrder.TrainingMode,
                    Type = mergeOrder.Type
                };
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }
        public bool CheckControlUnitStatus()
        {
            try
            {
                var uc = PosState.GetInstance().ControlUnitAction;
                return uc.ControlUnit.CheckStatus() == ControlUnitStatus.OK;
                // return true;
            }
            catch (Exception ex)
            {
                _view.ShowError("Control Unit", ex.Message);
                return false;
            }
        }
        int InsertedAmount = 0;
        internal void HandleDirectPaymentClick(bool isCreditCard, bool isSwish = false)
        {
            if (CheckControlUnitStatus() == false)
            {
                CUConnectionWindow cuConnectionWindow = new CUConnectionWindow();
                if (cuConnectionWindow.ShowDialog() == false)
                    return;
            }
            var dirBong = Defaults.SettingsList[SettingCode.DirectBong] == "1" ? true : false;
            if (isCreditCard)
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.DirectCardPaymentStarted), MasterOrder.Id);
            else if(isSwish)
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.DirectSwishPaymentStarted), MasterOrder.Id);
            else
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.DirectCashPaymentStarted), MasterOrder.Id);

            _type = _view.GetOrderType();

            _orderDirection = MasterOrder.Status == OrderStatus.ReturnOrder ? -1 : 1;

            MasterOrder.OrderTotal = MasterOrder.GrossAmount;

            decimal orderTotal = MasterOrder.OrderTotal * _orderDirection;

            long intPart = (long)MasterOrder.OrderTotal;

            //decimal fracPart = MasterOrder.OrderTotal - intPart;

            decimal roundamount;

            decimal fracPart = MasterOrder.OrderTotal - intPart;
            if (fracPart < (decimal)0.50)
                roundamount = (-1) * fracPart;
            else
            {
                roundamount = Convert.ToDecimal(1) - fracPart;
            }


            decimal vatAmount = MasterOrder.VatAmount;

            decimal cashBackAmount = 0;

            if (_orderDirection == -1)
            {
                cashBackAmount = orderTotal;
            }

            var seamlesProducts = CurrentorderDetails.Where(p => p.Product.Seamless).ToList();

            if (seamlesProducts.Count > 0)
            {
                var products = new List<Product>();
                foreach (var detail in seamlesProducts)
                {
                    for (int i = 1; i <= detail.Quantity; i++)
                    {
                        products.Add(detail.Product);
                    }
                }


            }

            bool bRes = false;


            PaymentTransactionStatus creditcardPaymentResult = null;
            var progressDialog = new ProgressWindow();


            //OrderRepository repo = new OrderRepository();
            //repo.AddOrUpdate(MasterOrder);
            /*eidt*/





            var backgroundThread = new Thread(() =>
            {

                Defaults.PerformanceLog.Add("Starting Check out Order      ");
                LogWriter.CheckOutLogWrite("Starting check out order ", MasterOrder.Id);
                bRes = CheckOut(orderTotal, isCreditCard, roundamount, creditcardPaymentResult,isSwish);

                progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressDialog.Close();

                    if (bRes)
                    {
                        // Transaction complete, kick drawer
                        try
                        {
                            Defaults.PerformanceLog.Add("Check out completed ");

                            if (orderTotal > 0 && isCreditCard == false)
                            {

                                OpenCashDrawer((_orderDirection) * orderTotal);

                            }

                            var printList = CurrentorderDetails.Where(i => i.Product.Bong && i.ItemStatus != 3).ToList();
                            //i.ItemStatus == (int)OrderStatus.New &&
                            var groupItemList =
                                    CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                            foreach (var itm in groupItemList)
                            {
                                var lst =
                                    itm.ItemDetails.Where(i => i.Product.Bong && i.ItemStatus != 3)
                                        .Select(
                                            i =>
                                                new OrderLine
                                                {
                                                    OrderId = MasterOrder.Id,
                                                    Quantity = i.Quantity,
                                                    UnitPrice = i.UnitPrice,
                                                    ItemId = i.Product.Id,
                                                    ItemComments = i.ItemComments,
                                                    Description = i.Description,
                                                    Product = i.Product
                                                })
                                        .ToList();
                                printList.AddRange(lst);
                            }

                            if (printList.Count > 0 && Defaults.SaleType == SaleType.Restaurant && Defaults.BONG)
                            {
                                var drctOrderPrint = new DirectPrint();
                                var order = new Order
                                {
                                    Id = MasterOrder.Id,
                                    CreationDate = MasterOrder.CreationDate,
                                    Bong = MasterOrder.Bong,
                                    DailyBong = MasterOrder.DailyBong,
                                    Type = MasterOrder.Type,
                                    TableId = MasterOrder.TableId,
                                    TableName = MasterOrder.TableName,
                                    OrderComments = MasterOrder.OrderComments,
                                    OrderLines = printList
                                };

                                //drctOrderPrint.PrintBong(order, true);
                            }

                            Defaults.PerformanceLog.Add("Sending To Invoice Print      -> ");
                            LogWriter.CheckOutLogWrite("Sending receipt to print", MasterOrder.Id);
                            var directPrint = new DirectPrint();
                            if (Defaults.IsOpenOrder == false && MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder && isCreditCard)
                            {
                                int dailyBongCounter = 0;
                                InvoiceHandler invoiceHndler = new InvoiceHandler();
                                int bongNo = invoiceHndler.GetLastBongNo(out dailyBongCounter, false);
                                MasterOrder.Bong = bongNo.ToString();
                                MasterOrder.DailyBong = dailyBongCounter.ToString();
                                if (Defaults.BONG && Defaults.DirectBONG)
                                {
                                    SendPrintToKitchenWithoutReset(MasterOrder);
                                }
                                //invoiceHndler.UpdateBongNo(MasterOrder.Id, dailyBongCounter);

                                //var masterorder = GetOrderMasterDetailById(MasterOrder.Id);
                                //MasterOrder.Bong = masterorder.Bong;

                            }

                            if (Defaults.AskForPrintInvoice)
                            {
                                if (MessageBox.Show("Do you want to print receipt ?", "POSSUM", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    directPrint.PrintReceipt(MasterOrder.Id, false, bRes);
                                    LogWriter.CheckOutLogWrite("Receipt printed after user permission", MasterOrder.Id);
                                }
                                else
                                {                                    
                                    var orderRepo = new InvoiceHandler();
                                    int dailyBongCounter = 0;
                                    var tmp = orderRepo.GetLastBongNo(out dailyBongCounter);
                                    if (MasterOrder.Id != default(Guid))// && (string.IsNullOrEmpty(_orderMaster.Bong) || _orderMaster.Bong == "0"))
                                        orderRepo.UpdateBongNo(MasterOrder.Id, dailyBongCounter);


                                    LogWriter.CheckOutLogWrite("Receipt printed cancel by user", MasterOrder.Id);
                                }
                            }
                            else
                            {
                                directPrint.PrintReceipt(MasterOrder.Id, false, bRes);
                                LogWriter.CheckOutLogWrite("Receipt printed", MasterOrder.Id);
                            }


                            // if (MasterOrder.OrderType == OrderType.TakeAway)

                            //updated code to print receipt the bong before or recept commented below condition
                            //if (isCreditCard && Defaults.IsOpenOrder == false)
                            //{
                            //    if (Defaults.BONG)
                            //        SendPrintToKitchenWithoutReset(MasterOrder);
                            //}

                            if (_type == OrderType.Return)
                            {
                                //  invoiceWindow.PrintReturnInvoice(orderId);
                                LogWriter.JournalLog(
                                        Defaults.User.TrainingMode
                                            ? JournalActionCode.ReceiptPrintedForReturnOrderViaTrainingMode
                                            : JournalActionCode.ReceiptPrintedForReturnOrder, MasterOrder.Id);
                            }
                            else
                            {

                                //invoiceWindow.ShowDialog();
                                LogWriter.JournalLog(
                                        Defaults.User.TrainingMode
                                            ? JournalActionCode.ReceiptPrintedViaTrainingMode
                                            : JournalActionCode.ReceiptPrinted, MasterOrder.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogWrite(ex);
                            App.MainWindow.ShowError(UI.Global_Error, ex.Message);
                        }
                    }
                }));
            });

            if (isCreditCard)
            {
                //  creditCardAmount = orderDirection * creditCardAmount; //Deos it is need to send refend payment as minus value when case is return order?
                var result = _orderDirection == 1
                    ? Utilities.ProcessPaymentPurchase(orderTotal, vatAmount, cashBackAmount, MasterOrder.TableId, MasterOrder.Id)
                    : Utilities.ProcessPaymentRefund(orderTotal, vatAmount < 0 ? vatAmount * -1 : vatAmount,
                        cashBackAmount, MasterOrder.TableId, MasterOrder.Id);
                if (result.Result == PaymentTransactionStatus.PaymentResult.SUCCESS ||
                    // Device reported purchase was successful
                    result.Result == PaymentTransactionStatus.PaymentResult.NO_DEVICE_CONFIGURED)
                // Device set to NONE, allow since its most likely an external device
                {
                    creditcardPaymentResult = result;
                    backgroundThread.Start();
                    progressDialog.ShowDialog();
                    backgroundThread.Join();

                    if (bRes)
                    {
                        LogWriter.CheckOutLogWrite("Checkout order completed successfully", MasterOrder.Id);
                        if (MasterOrder != null && MasterOrder.IsForAdult)
                        {
                            _view.ShowSurvey();
                        }
                        _view.NewRecord();
                        UpdateHistoryGrid();
                    }
                }

            }
            else
            {
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();



                App.MainWindow.UpdateOrderCompleted(MasterOrder.Id);
                App.MainWindow.ItemsforPublishMessage(CurrentorderDetails);
                if (MasterOrder != null && MasterOrder.IsForAdult)
                {
                    _view.ShowSurvey();
                }
                _view.NewRecord();
                UpdateHistoryGrid();
            }
        }
        internal void HandelDirectMobileCardClick()
        {

            if (CheckControlUnitStatus() == false)
            {
                CUConnectionWindow cuConnectionWindow = new CUConnectionWindow();
                if (cuConnectionWindow.ShowDialog() == false)
                    return;
            }
            _type = _view.GetOrderType();
            _orderDirection = MasterOrder.Type == OrderType.Return ? -1 : 1;
            OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
            // var total = orderRepo.GetOrderTotal(MasterOrder.Id);
            //MasterOrder.OrderTotal = total;
            // CurrentorderDetails = orderRepo.GetOrderLinesById(MasterOrder.Id);

            decimal orderTotal = MasterOrder.OrderTotal * _orderDirection;
            long intPart = (long)MasterOrder.OrderTotal;
            decimal fracPart = MasterOrder.OrderTotal - intPart;
            decimal _roundamount = 0;
            if (fracPart < (decimal)0.50)
                _roundamount = (-1) * fracPart;
            else
            {
                _roundamount = Convert.ToDecimal(1) - fracPart;
            }
            decimal vatAmount = MasterOrder.VatAmount;
            decimal CashBackAmount = 0;
            if (_orderDirection == -1)
                CashBackAmount = orderTotal;

            var seemlesProducts = CurrentorderDetails.Where(p => p.Product.Seamless == true).ToList();
            if (seemlesProducts.Count > 0)
            {
                List<Product> products = new List<Product>();
                foreach (var detail in seemlesProducts)
                {
                    for (int i = 1; i <= detail.Quantity; i++)
                        products.Add(detail.Product);
                }


            }

            bool bRes = false;

            ProgressWindow progressDialog = new ProgressWindow();
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    Defaults.PerformanceLog.Add("Starting Check out Order      -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));


                    var dtoPayment = new Payment
                    {
                        OrderId = MasterOrder.Id,
                        PaidAmount = MasterOrder.OrderTotal,
                        PaymentDate = DateTime.Now,
                        TypeId = 9,
                        CashCollected = MasterOrder.OrderTotal,
                        CashChange = 0,
                        ReturnAmount = 0,
                        PaymentRef = "Mobile Kortterminal",

                        TipAmount = 0,
                        Direction = _orderDirection
                    };
                    SavePayment(dtoPayment);

                    bRes = GenerateInvoiceLocal(MasterOrder.Id, App.MainWindow.ShiftNo, Defaults.User.Id, null);


                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                        //Print Bong


                        Defaults.PerformanceLog.Add("Check out completed           -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                        var printList = CurrentorderDetails.Where(i => i.Product.Bong && i.ItemStatus != 3).ToList();//i.ItemStatus == (int)OrderStatus.New &&
                        var groupItemList = CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                        foreach (var itm in groupItemList)
                        {
                            var lst = itm.ItemDetails.Where(i => i.Product.Bong && i.ItemStatus != 3).Select(i => new OrderLine
                            {
                                OrderId = MasterOrder.Id,
                                Quantity = i.Quantity,
                                UnitPrice = i.UnitPrice,
                                ItemId = i.Product.Id,

                                ItemComments = i.ItemComments,
                                PrinterId = i.PrinterId,
                                PreparationTime = i.Product.PreparationTime,
                                Product = i.Product// new Product { Id = i.ItemId, Description = i.ItemName, Price = i.UnitPrice }


                            }).ToList();
                            printList.AddRange(lst);
                        }
                        if (printList.Count > 0 && Defaults.BONG)//&& Defaults.SaleType == SaleType.Restaurant
                        {
                            // PrintOrderWindow printOrder = new PrintOrderWindow();
                            // printOrder.PrintOrder(MasterOrder, printList, true, 1);
                            DirectPrint drctOrderPrint = new DirectPrint();
                            var order = new Order
                            {
                                Id = MasterOrder.Id,
                                CreationDate = MasterOrder.CreationDate,
                                Bong = MasterOrder.Bong,
                                DailyBong = MasterOrder.DailyBong,
                                Type = MasterOrder.Type,
                                TableId = MasterOrder.TableId,
                                TableName = MasterOrder.TableName,
                                OrderComments = MasterOrder.OrderComments,
                                OrderLines = printList,
                                CustomerId = MasterOrder.CustomerId,
                                OrderNoOfDay = MasterOrder.OrderNoOfDay
                            };

                            drctOrderPrint.PrintBong(order, true);
                        }

                        if (true)//bRes
                        {
                            // Transaction complete, kick drawer
                            try
                            {
                                Defaults.PerformanceLog.Add("Sending To Invoice Print      -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                                //  var invoiceWindow = new PrintNewInvoiceWindow();
                                DirectPrint directPrint = new DirectPrint();
                                directPrint.PrintReceipt(MasterOrder.Id, false, bRes);

                                if (_type == OrderType.Return)
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


                            }
                            catch (Exception ex)
                            {
                                LogWriter.LogWrite(ex);
                                App.MainWindow.ShowError(UI.Global_Error, ex.Message);
                            }

                        }

                    }));
                }));


            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();

            App.MainWindow.ItemsforPublishMessage(CurrentorderDetails.Where(i => i.Active == 1).ToList());
            if (MasterOrder != null && MasterOrder.IsForAdult)
            {
                _view.ShowSurvey();
            }
            _view.NewRecord();
            UpdateHistoryGrid();

            // if (bRes)
            //{
            //if (OrderDirectCheckOut != null)
            //    OrderDirectCheckOut(this, orderId, isCreditCard);
            // }
        }
        internal void HandelDirectSwishPaymentClick(bool isSwish)
        {
            if (CheckControlUnitStatus() == false)
            {
                CUConnectionWindow cuConnectionWindow = new CUConnectionWindow();
                if (cuConnectionWindow.ShowDialog() == false)
                    return;
            }
            _type = _view.GetOrderType();

            _orderDirection = MasterOrder.Type == OrderType.Return ? -1 : 1;

            MasterOrder.OrderTotal = MasterOrder.GrossAmount;

            decimal orderTotal = MasterOrder.OrderTotal * _orderDirection;

            long intPart = (long)MasterOrder.OrderTotal;

            decimal fracPart = MasterOrder.OrderTotal - intPart;

            decimal roundamount;

            if (fracPart < (decimal)0.50)
            {
                roundamount = -1 * fracPart;
            }
            else
            {
                roundamount = Convert.ToDecimal(1) - fracPart;
            }

            if (_orderDirection == -1)
            {
            }

            var seemlesProducts = CurrentorderDetails.Where(p => p.Product.Seamless).ToList();
            if (seemlesProducts.Count > 0)
            {
                var products = new List<Product>();
                foreach (var detail in seemlesProducts)
                {
                    for (int i = 1; i <= detail.Quantity; i++)
                    {
                        products.Add(detail.Product);
                    }
                }


            }

            bool bRes;

            var progressDialog = new ProgressWindow();

            var backgroundThread = new Thread(() =>
            {
                Defaults.PerformanceLog.Add("Starting Check out Order      ");
                bRes = CheckOutSwish(orderTotal, isSwish, roundamount);

                progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressDialog.Close();

                    if (bRes)
                    {
                        // Transaction complete, kick drawer
                        try
                        {
                            Defaults.PerformanceLog.Add("Check out completed           ");

                            var printList = CurrentorderDetails.Where(i => i.Product.Bong && i.ItemStatus != 3).ToList();
                            //i.ItemStatus == (int)OrderStatus.New &&
                            var groupItemList =
                                    CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                            foreach (var itm in groupItemList)
                            {
                                var lst =
                                    itm.ItemDetails.Where(i => i.Product.Bong && i.ItemStatus != 3)
                                        .Select(
                                            i =>
                                                new OrderLine
                                                {
                                                    OrderId = MasterOrder.Id,
                                                    Quantity = i.Quantity,
                                                    UnitPrice = i.UnitPrice,
                                                    ItemId = i.Product.Id,
                                                    Product = i.Product
                                                })
                                        .ToList();
                                printList.AddRange(lst);
                            }

                            if (printList.Count > 0 && Defaults.SaleType == SaleType.Restaurant && Defaults.BONG)
                            {
                                var drctOrderPrint = new DirectPrint();
                                var order = new Order
                                {
                                    Id = MasterOrder.Id,
                                    CreationDate = MasterOrder.CreationDate,
                                    Bong = MasterOrder.Bong,
                                    DailyBong = MasterOrder.DailyBong,
                                    Type = MasterOrder.Type,
                                    TableId = MasterOrder.TableId,
                                    TableName = MasterOrder.TableName,
                                    OrderComments = MasterOrder.OrderComments,
                                    OrderLines = printList
                                };

                                drctOrderPrint.PrintBong(order, true);
                            }
                            Defaults.PerformanceLog.Add("Sending To Invoice Print      ");

                            var directPrint = new DirectPrint();
                            directPrint.PrintReceipt(MasterOrder.Id, false, bRes);

                            if (_type == OrderType.Return)
                            {
                                LogWriter.JournalLog(
                                    Defaults.User.TrainingMode
                                        ? JournalActionCode.ReceiptPrintedForReturnOrderViaTrainingMode
                                        : JournalActionCode.ReceiptPrintedForReturnOrder, MasterOrder.Id);
                            }
                            else
                            {
                                LogWriter.JournalLog(
                                    Defaults.User.TrainingMode
                                        ? JournalActionCode.ReceiptPrintedViaTrainingMode
                                        : JournalActionCode.ReceiptPrinted, MasterOrder.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogWrite(ex);
                            App.MainWindow.ShowError(UI.Global_Error, ex.Message);
                        }
                    }
                }));
            });

            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();

            App.MainWindow.UpdateOrderCompleted(MasterOrder.Id);
            App.MainWindow.ItemsforPublishMessage(CurrentorderDetails);
            if (MasterOrder != null && MasterOrder.IsForAdult)
            {
                _view.ShowSurvey();
            }
            _view.NewRecord();
            UpdateHistoryGrid();
        }

        private bool CheckOutSwish(decimal totalAMount, bool isSwish, decimal roundedAmount)
        {
            Guid orderId = MasterOrder.Id;
            var paidamount = Math.Round(totalAMount, 0);

            Payment dtoPayment;

            if (isSwish)
            {
                dtoPayment = new Payment
                {
                    OrderId = orderId,
                    PaidAmount = totalAMount,
                    PaymentDate = DateTime.Now,
                    TypeId = 10,
                    CashCollected = 0,
                    CashChange = 0,
                    ReturnAmount = 0,
                    PaymentRef = "Swish",
                    TipAmount = 0,
                    Direction = _orderDirection
                };
                SavePayment(dtoPayment);
            }
            else
            {
                dtoPayment = new Payment
                {
                    OrderId = orderId,
                    PaidAmount = paidamount,
                    PaymentDate = DateTime.Now,
                    TypeId = 11,
                    CashCollected = paidamount,
                    CashChange = 0,
                    ReturnAmount = 0, // if given extra amount in cash
                    PaymentRef = "StudentKort",
                    TipAmount = 0,
                    Direction = _orderDirection
                };

                SavePayment(dtoPayment, roundedAmount);

                if (MasterOrder.Status == OrderStatus.ReturnOrder)
                {
                    dtoPayment = new Payment
                    {
                        OrderId = orderId,
                        PaidAmount = 0,
                        PaymentDate = DateTime.Now,
                        TypeId = 7,
                        CashCollected = 0,
                        CashChange = totalAMount,
                        ReturnAmount = 0,
                        PaymentRef = UI.CheckOutOrder_Label_CashBack,
                        TipAmount = 0,
                        Direction = _orderDirection
                    };

                    SavePayment(dtoPayment);
                }
            }

            try
            {
                bool bRes = GenerateInvoiceLocal(orderId, App.MainWindow.ShiftNo, Defaults.User.Id, null);
                return bRes;
            }
            catch (ControlUnitException e)
            {
                _view.ShowError(Defaults.AppProvider.AppTitle, e.Message);
            }
            return false;
        }

        private bool CheckOut(decimal totalAMount, bool isCreditCard, decimal roundedAmount, PaymentTransactionStatus creditcardPaymentResult, bool isSwish = false)
        {
            Guid orderId = MasterOrder.Id;
            var paidamount = Math.Round(totalAMount, 0);
            Payment dtoPayment;



            if (isCreditCard)
            {

                dtoPayment = new Payment
                {
                    OrderId = orderId,
                    PaidAmount = totalAMount,
                    PaymentDate = DateTime.Now,
                    TypeId = (!string.IsNullOrEmpty(creditcardPaymentResult.ProductName) && creditcardPaymentResult.ProductName == "American Express") ? 14 : 4,
                    CashCollected = 0,
                    CashChange = 0,
                    ReturnAmount = 0,
                    PaymentRef = UI.CheckOutOrder_Method_CreditCard,// "Kort",
                    ProductName = creditcardPaymentResult.ProductName,
                    TipAmount = 0,
                    Direction = _orderDirection
                };
                SavePayment(dtoPayment);


            }
            else if (isSwish)
            {

                dtoPayment = new Payment
                {
                    OrderId = orderId,
                    PaidAmount = totalAMount,
                    PaymentDate = DateTime.Now,
                    TypeId = 10,
                    CashCollected = 0,
                    CashChange = 0,
                    ReturnAmount = 0,
                    PaymentRef = "Swish",
                    ProductName = "Swish",
                    TipAmount = 0,
                    Direction = _orderDirection
                };
                SavePayment(dtoPayment);
            }
            else
            {

                dtoPayment = new Payment
                {
                    OrderId = orderId,
                    PaidAmount = paidamount,
                    PaymentDate = DateTime.Now,
                    TypeId = 1,
                    CashCollected = paidamount,
                    CashChange = 0,
                    ReturnAmount = 0,// if given extra amount in cash
                    PaymentRef = UI.CheckOutOrder_Method_Cash,// "Kontant",
                    TipAmount = 0,
                    Direction = _orderDirection
                };
                SavePayment(dtoPayment, roundedAmount);
                if (MasterOrder.Status == OrderStatus.ReturnOrder)
                {
                    dtoPayment = new Payment
                    {
                        OrderId = orderId,
                        PaidAmount = 0,
                        PaymentDate = DateTime.Now,
                        TypeId = 7,
                        CashCollected = 0,
                        CashChange = totalAMount,
                        ReturnAmount = 0,
                        PaymentRef = UI.CheckOutOrder_Label_CashBack,

                        TipAmount = 0,
                        Direction = _orderDirection
                    };
                    SavePayment(dtoPayment);
                }
            }

            try
            {
                bool bRes = GenerateInvoiceLocal(orderId, App.MainWindow.ShiftNo, Defaults.User.Id, creditcardPaymentResult);
                return bRes;
            }
            catch (ControlUnitException e)
            {
                _view.ShowError(Defaults.AppProvider.AppTitle, e.Message);
            }
            return false;
        }

        internal void HandleReturnClick()
        {
            MasterOrder.Type = OrderType.Return;

            foreach (var item in CurrentorderDetails)
            {
                item.Direction = -1;
                item.ItemDiscount = (-1) * item.ItemDiscount;
                item.OrderType = OrderType.Return;
                EditItem(item);
            }
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                UpdateOrderEntry();
            }

            _view.SetOrderTypeVisibility(true, UI.Sales_ReturnOrder);
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeReturn), MasterOrder.Id);
            MasterOrder.OrderLines = (CurrentorderDetails);
            _view.SetCartItems(CurrentorderDetails);
            FocusTextBox();
        }
        internal void HandleStudentCardClick()
        {
            var selectedTable = _view.GetSelectedTable();
            bool isMerge = _view.IsMerge();
            var mergeOrder = _view.GetMergeOrder();
            if (CurrentorderDetails.Count == 0)
            {
                _view.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                return;
            }
            Defaults.PerformanceLog.Add("Clicked On direct studentkort payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

            if (isMerge && mergeOrder != null)
                SetMergeOrder();
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                MasterOrder.Status = MasterOrder.OrderStatusFromType;
                if (selectedTable != null)
                {
                    MasterOrder.SelectedTable = selectedTable;
                    MasterOrder.TableId = selectedTable.Id;
                    MasterOrder.Comments = selectedTable.Name;
                }
                MasterOrder.Updated = 1;
                MasterOrder.CheckOutUserId = Defaults.User.Id;
                UpdateOrderDetail(CurrentorderDetails, MasterOrder);
                MasterOrder = OrderEntry(MasterOrder);

                //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                //    Presenter.SendPrintToKitchen(Presenter.MasterOrder,false);
            }
            else
                SaveOrder();
            Defaults.PerformanceLog.Add("Direct Studentkort Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            HandelDirectSwishPaymentClick(false);
        }
        internal void HandleOrderCommentClick()
        {
            MasterOrder.OrderComments = Utilities.PromptInput(UI.Sales_OrderCommentButton, UI.Sales_EnterComment, MasterOrder.OrderComments);
            if (!string.IsNullOrEmpty(MasterOrder.OrderComments))
            {

                _view.SetOrderCommentsVisibility(true, MasterOrder.OrderComments);
                LogWriter.JournalLog((int)JournalActionCode.OrderComment, MasterOrder.Id);
            }
            else
            {
                _view.SetOrderCommentsVisibility(false, "");
            }
            FocusTextBox();
        }
        private bool GenerateInvoiceLocal(Guid id, int shifNo, string userId, PaymentTransactionStatus creditcardPaymentResult)
        {
            return new InvoiceHandler().GenerateInvoiceLocal(id, shifNo, userId, creditcardPaymentResult);
        }
        internal void DiscardOrder(Guid id)
        {
            var repo = new OrderRepository(PosState.GetInstance().Context);

            var order = repo.GetOrderMaster(id);

            order.Status = OrderStatus.OrderCancelled;

            repo.SaveOrderMaster(order);
        }
        internal void OpenCashDrawer(decimal amount)
        {
            try
            {
                using (var db = PosState.GetInstance().Context)
                {
                    var cashdrawer = db.CashDrawer.First(x => x.TerminalId == Defaults.Terminal.Id);

                    // Log that drawer was opened
                    cashdrawer.OpenCashDrawerSale(amount, Defaults.User.Id);
                    if (cashdrawer.Logs != null)
                    {
                        db.CashDrawerLog.AddRange(cashdrawer.Logs);
                    }
                    // Kick drawer
                    if (Defaults.CashDrawerType == CashDrawerType.PRINTER)
                    {
                        var directPrint = new DirectPrint();
                        directPrint.OpenCashDrawer();
                    }
                    else
                    {
                        PosState.GetInstance().CashDrawer.Open();
                    }

                    // Save log
                    db.SaveChanges();
                }

                LogWriter.JournalLog(JournalActionCode.OpenCashDrawer);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", "No cashdrawer defined.");
            }
        }

        internal bool SavePayment(Payment payment, decimal roundAmount = 0)
        {
            return new InvoiceHandler().SaveOrderPayment(payment, roundAmount);
        }

        private void SaveOrder(bool cancel = false)
        {
            try
            {
                var res = false;
                MasterOrder.OrderTotal = MasterOrder.GrossAmount;
                _type = _view.GetOrderType();
                _orderDirection = _type == OrderType.Return ? -1 : 1;
                _orderId = MasterOrder.Id;
                var isFormHold = _view.FormHoldStatus();
                _customerId = MasterOrder.TableId;
                var ordRepo = new OrderRepository(PosState.GetInstance().Context);

                if (MasterOrder.Id != default(Guid))
                {
                    MasterOrder.Updated = 1;
                    MasterOrder.CheckOutUserId = Defaults.User.Id;

                    var progressDialog = new ProgressWindow();
                    var backgroundThread = new Thread(() =>
                    {
                        res = EditOrderMaster(MasterOrder);

                        var lstdetail = CurrentorderDetails.Where(itm => itm.ItemStatus == 0).ToList();
                        var printList =
                            CurrentorderDetails.Where(itm => itm.ItemStatus == 0 && itm.Product.Bong).ToList();
                        var groupItemList =
                            CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                        foreach (var itm in groupItemList)
                        {
                            var lst =
                                itm.ItemDetails.Where(i => i.Product.Bong)
                                    .Select(
                                        i =>
                                            new OrderLine
                                            {
                                                OrderId = MasterOrder.Id,
                                                Quantity = i.Quantity,
                                                UnitPrice = i.UnitPrice,
                                                ItemId = i.Product.Id,
                                                Product = i.Product
                                            })
                                    .ToList();
                            printList.AddRange(lst);
                        }
                        if (lstdetail.Count > 0)
                            res = SaveOrderDetail(lstdetail, MasterOrder);
                        progressDialog.Closed += (arg, e) => { progressDialog = null; };
                        progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressDialog.Close();
                            if (res)
                            {
                                if (cancel)
                                {
                                    ordRepo.SetOrderStatus(MasterOrder.Id, OrderStatus.Completed, Defaults.User.Id);
                                }

                                if (printList.Count > 0 && isFormHold && Defaults.SaleType == SaleType.Restaurant &&
                                    Defaults.BONG)
                                {
                                    var drctOrderPrint = new DirectPrint();
                                    var order = new Order
                                    {
                                        Id = MasterOrder.Id,
                                        CreationDate = MasterOrder.CreationDate,
                                        DailyBong = MasterOrder.DailyBong,
                                        Bong = MasterOrder.Bong,
                                        Type = MasterOrder.Type,
                                        TableId = MasterOrder.TableId,
                                        TableName = MasterOrder.TableName,
                                        OrderComments = MasterOrder.OrderComments,
                                        OrderLines = printList
                                    };

                                    drctOrderPrint.PrintBong(order, true);
                                }
                            }
                            else
                            {
                                MessageBox.Show(UI.Message_OrderSavingFailed, Defaults.AppProvider.AppTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }));
                    });

                    backgroundThread.Start();
                    progressDialog.ShowDialog();
                    backgroundThread.Join();

                }
                else
                {
                    var masterOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        TableId = _customerId,
                        ShiftNo = App.MainWindow.ShiftNo,
                        Updated = 1,
                        PaymentStatus = 2
                    };

                    if (masterOrder.Outlet == null)
                    {
                        Defaults.Init();
                        masterOrder.Outlet = Defaults.Outlet;
                        masterOrder.TerminalId = Defaults.Terminal.Id;
                    }
                    var progressDialog = new ProgressWindow();

                    var backgroundThread = new Thread(() =>
                    {
                        var order = SaveOrderMaster(masterOrder);
                        if (order.Id != default(Guid))
                        {
                            masterOrder.Id = order.Id;
                            _orderId = masterOrder.Id;
                            res = SaveOrderDetail(CurrentorderDetails, masterOrder);
                        }
                        progressDialog.Closed += (arg, e) => { progressDialog = null; };
                        progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressDialog.Close();
                            if (res && isFormHold && Defaults.SaleType == SaleType.Restaurant && Defaults.BONG)
                            {
                                var printList = CurrentorderDetails.Where(itm => itm.Product.Bong).ToList();
                                var groupItemList =
                                    CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                                foreach (var itm in groupItemList)
                                {
                                    var lst =
                                        itm.ItemDetails.Where(i => i.Product.Bong)
                                            .Select(
                                                i =>
                                                    new OrderLine
                                                    {
                                                        OrderId = MasterOrder.Id,
                                                        Quantity = i.Quantity,
                                                        UnitPrice = i.UnitPrice,
                                                        ItemId = i.Product.Id,
                                                        Product = i.Product
                                                    })
                                            .ToList();
                                    printList.AddRange(lst);
                                }

                                if (printList.Count > 0)
                                {
                                    var drctOrderPrint = new DirectPrint();
                                    var orderPrint = new Order
                                    {
                                        Id = MasterOrder.Id,
                                        CreationDate = MasterOrder.CreationDate,
                                        Bong = MasterOrder.Bong,
                                        DailyBong = masterOrder.DailyBong,
                                        Type = MasterOrder.Type,
                                        TableId = MasterOrder.TableId,
                                        TableName = MasterOrder.TableName,
                                        OrderComments = MasterOrder.OrderComments,
                                        OrderLines = printList
                                    };

                                    drctOrderPrint.PrintBong(orderPrint, true);
                                }
                            }
                            else
                            {
                                MessageBox.Show(UI.Message_OrderSavingFailed, Defaults.AppProvider.AppTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }));
                    });
                    backgroundThread.Start();
                    progressDialog.ShowDialog();
                    backgroundThread.Join();

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError(UI.Global_Error, ex.Message);

            }
        }

        #endregion

        #region Load Category/Product

        internal List<Category> LoadCategories(int categoryId)
        {
            Defaults.Categories = new ProductRepository(PosState.GetInstance().Context).LoadCategories(categoryId);
            return Defaults.Categories;

        }
        internal List<Category> LoadCategories2(int categoryId)
        {
            Defaults.Categories = new ProductRepository(PosState.GetInstance().Context).LoadCategories(categoryId);
            return Defaults.Categories;

        }

        public void LoadItemByCategory(int catId)
        {
            var lstItems = LoadProductsByCategory(catId);

            _view.SetProductListBox(lstItems, false);
        }

        private List<Product> LoadProductsByCategory(int categoryId)
        {
            return productPresenter.GetProductsByCategory(categoryId);
        }

        public List<Product> GetProductByGroup(Guid groupId)
        {
            var productPresenter = new ProductRepository(PosState.GetInstance().Context);
            return productPresenter.GetProductsByGroup(groupId);
        }

        internal Product GetProductByBarCode(string brandval)
        {
            var productPresenter = new ProductPresenter();
            return productPresenter.GetProductByBarCode(brandval);
        }

        internal Product GetProductByPlu(string pluNo)
        {
            var productPresenter = new ProductPresenter();
            return productPresenter.GetProductByPLU(pluNo);
        }

        #endregion

        #region Save Order

        public Order SaveOrderMaster(Order order)
        {

            try
            {

                order = new OrderRepository(PosState.GetInstance().Context).SaveOrderMaster(order);

                return order;
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return order;
            }
        }

        public bool SaveOrderDetail(List<OrderLine> orderLines, Order order)
        {
            try
            {

                List<OrderLine> lines = new List<OrderLine>();
                foreach (var orderLineModel in orderLines)
                {
                    var line = orderLineModel;
                    if (orderLineModel.ItemDetails != null)
                    {
                        line.ItemDetails = new List<OrderLine>();
                        foreach (var innerItem in orderLineModel.ItemDetails)
                        {

                            line.ItemDetails.Add(innerItem);
                        }
                    }
                    lines.Add(line);
                }


                return new OrderRepository(PosState.GetInstance().Context).SaveOrderLines(lines, order);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }

        internal bool EditOrderMaster(Order order)
        {
            try
            {
                order = new OrderRepository(PosState.GetInstance().Context).SaveOrderMaster(order);
                MasterOrder.Id = order.Id;
                MasterOrder.OrderNoOfDay = order.OrderNoOfDay;
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }
        internal bool MoveOrders(List<Order> orders, FoodTable table)
        {
            try
            {


                new OrderRepository(PosState.GetInstance().Context).MoveOrders(orders, table, OrderType.TableOrder, table.Name);

                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }
        public bool UpdateOrderDetail(List<OrderLine> orderDetail, Order order)
        {
            try
            {
                if (orderDetail.Count == 0)
                    return true;
                List<OrderLine> lines = new List<OrderLine>();
                foreach (var line in orderDetail)
                {

                    line.ItemStatus = (int)order.OrderStatusFromType;
                    lines.Add(line);


                }

                new OrderRepository(PosState.GetInstance().Context).SaveOrderLines(lines, order);

                return true;

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }

        public bool UpdateOrderLine(OrderLine orderDetail, Order order)
        {
            try
            {
                if (orderDetail == null)
                    return true;

                new OrderRepository(PosState.GetInstance().Context).UpdateOrderLine(orderDetail, order);

                return true;

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }
        public bool MoveOrderDetail(List<OrderLine> orderDetail, Order order)
        {
            try
            {
                if (orderDetail.Count == 0)
                    return true;



                new OrderRepository(PosState.GetInstance().Context).MoveOrderLines(orderDetail, order);

                return true;

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }
        #endregion

        #region Save Order when Entry mode=1

        public Order OrderEntry(Order orderViewModel)
        {
            if (orderViewModel.Outlet == null)
            {
                if (Defaults.Outlet == null || Defaults.Terminal == null)
                {
                    Defaults.Init();
                }

                orderViewModel.Outlet = Defaults.Outlet;
                if (Defaults.Outlet != null) orderViewModel.OutletId = Defaults.Outlet.Id;

                if (Defaults.Terminal != null) orderViewModel.TerminalId = Defaults.Terminal.Id;
            }

            if (orderViewModel.Id == default(Guid))
            {

                orderViewModel.CreationDate = DateTime.Now;
                orderViewModel.UserId = Defaults.User.Id;
                orderViewModel.Outlet = null;
                var order = SaveOrderMaster(orderViewModel);
                orderViewModel.Outlet = Defaults.Outlet;
                _orderId = order.Id;
                orderViewModel.Id = order.Id;
                orderViewModel.OrderNoOfDay = order.OrderNoOfDay;
                if (MasterOrder == null)
                    MasterOrder = order;
                MasterOrder.Id = _orderId;
                CampaignDictionary = new Dictionary<int, decimal>();
                CampaignList = new Dictionary<int, List<OrderLine>>();
                // LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.NewOrderEntry), order.Id);
            }
            else
            {

                EditOrderMaster(orderViewModel);
            }
            return orderViewModel;
        }

        public void SendPrintToKitchen(Order order, List<OrderLine> printList, bool viewNew = true)
        {

            DirectPrint directPrint = new DirectPrint();

            if (printList.Count > 0)
            {
                order.OrderLines = printList;
                directPrint.PrintBong(order, true);
            }

            //if (viewNew)
            //    _view.NewRecord();
        }

        public OrderLine AddOrderLine(Order order, OrderLine orderItem)
        {
            return new OrderRepository(PosState.GetInstance().Context).SaveOrderLine(orderItem, order);

        }

        public void EditItem(OrderLine orderItem, bool isLog = true)
        {

            new OrderRepository(PosState.GetInstance().Context).SaveOrderLine(orderItem, MasterOrder);

        }

        public void RemoveItem(OrderLine orderItem)
        {

            new OrderRepository(PosState.GetInstance().Context).RemoveQuantity(orderItem, Defaults.User.Id);

        }

        public void DeleteItem(OrderLine orderItem)
        {

            int campaignBuyLimit = new OrderRepository(PosState.GetInstance().Context).DeleteItem(orderItem, Defaults.User.Id);

            if (campaignBuyLimit > 0)
            {
                //CampaignaddOrUpdate(orderItem.CampaignId, -1);
                //CampaignListRemoveItem(orderItem.CampaignId, orderItem);
                //UpdateCartCampaign(orderItem.CampaignId, campaignBuyLimit);

            }
            //CalculatOrderTotal(CurrentorderDetails);
            _view.SetCartItems(CurrentorderDetails);

            //_view.SetCartItems(MasterOrder);
        }

        public void CalculatOrderTotal(List<OrderLine> lst)
        {
            decimal orderTotal = 0;
            decimal tax = 0;
            foreach (var item in lst)
            {
                if (item.IsValid)
                {
                    if (item.Product.ItemType == ItemType.Individual)
                    {
                        tax += item.VatAmount();
                    }

                    orderTotal += item.GrossAmountDiscounted();
                    if (item.IngredientItems != null)
                        foreach (var ingredient in item.IngredientItems)
                        {
                            tax += ingredient.VatAmount();
                            orderTotal += ingredient.GrossAmountDiscounted();
                        }
                }
            }

            //tax +=
            //    lst.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group)
            //        .SelectMany(detail => detail.OrderItemDetails)
            //        .Sum(itm => itm.VatAmount());

            decimal nettotal = orderTotal - tax;

            //MasterOrder.Tax = tax;
            //MasterOrder.NetTotal = nettotal;

            //MasterOrder.SumAmt = orderTotal;

        }

        #endregion

        #region Implementation  View Methods

        public List<Product> GetProductsList()
        {
            return _view.GetProductsList();
        }
        public void FocusTextBox()
        {
            _view.SetTextBoxFocus();
        }

        public string GetTexBoxValue()
        {
            return _view.GetTextBoxValue();
        }

        public void SetAskByUser(bool ask)
        {
            _view.SetAskByUser(ask);
        }
        public bool GetAskByUser()
        {
            return _view.GetAskByUser();
        }

        public void SetAskPriceValue(string askValue)
        {
            _view.SetAskPriceValue(askValue);
        }
        public string GetAskPriceValue()
        {
            return _view.GetAskPriceValue();
        }
        public void SetAskVolumeQty(int qty)
        {
            _view.SetAskVolumeQty(qty);
        }
        public int GetAskVolumeQty()
        {
            return _view.GetAskVolumeQty();
        }
        public Product GetSelectedItem()
        {
            return _view.GetSelectedItem();
        }

        public void SetSelectedItem(Product item)
        {
            _view.SetSelectedItem(item);
        }
        public void SetSelectedTable(FoodTable foodTable)
        {
            _view.SetSelectedTable(foodTable);
        }
        public void SetSelectedOrderLine(OrderLine orderLine)
        {

            _view.SetSelectedOrderLine(orderLine);
        }
        public OrderLine GetSelectedOrderLine()
        {
            return _view.GetSelectedOrderLine();
        }

        #endregion
    }
}