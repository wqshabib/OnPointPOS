using System.Collections.Generic;
using POSSUM.Model;
using System.Windows;
using System;

namespace POSSUM.Presenters.Sales
{
    public interface ISaleOrderView
    {
        void ShowError(string errorTitle, string errorMessage);

        void SetCartItems(List<OrderLine> order,bool IsAddToCart = false);
        //Order GetMasterOrder();
        //List<OrderLine> GetCurrentOrderDetail();
        OrderLine GetSelectedOrderLine();
        void SetSelectedOrderLine(OrderLine orderLine);
        Product GetSelectedItem();
        void SetSelectedItem(Product item);
        string GetTextBoxValue();
        void SetTextBoxFocus();
        void SetAskPriceValue(string Sales_EnterPrice);
        string GetAskPriceValue();
        void SetProductListBox(List<Product> itemsList, bool isButton);
        List<Product> GetProductsList();
        bool FormHoldStatus();
        OrderType GetOrderType();
        void NewRecord();
        void ShowSurvey();
        void SetHistoryViewResult(List<Order> orders);
        void SetOrderTotalOfDay(string orderTotalOfDay);
        void NewEntry();
        int GetCartIndex();
        void SetEntryMode(EntryModeType mode);
        EntryModeType GetEntryMode();

        void SetAskByUser(bool ask);
        bool GetAskByUser();

        void SetAskVolumeQty(int qty);
        int GetAskVolumeQty();

        void SetCurrentQty(decimal qty);
        decimal GetCurrentQty();

        bool IsMerge();
        void SetIsMerge(bool merge);
        bool IsNewOrder();
        int GetBIDSNO();
        Order GetMergeOrder();
        void SetMergeOrder(Order order);
        FoodTable GetSelectedTable();
        void SetSelectedTable(FoodTable table);
        void SetTableButtonContent(string text);
        void SetOrderCommentsVisibility(bool visibility, string comments);
        void SetOrderTypeVisibility(bool visibility, string text);
        void SetOrderCustomerTypeVisibility(bool visibility, string text);
        void SetOrderTypeSecondaryVisibility(bool visibility);
        bool GetOrderTypeSecondaryVisibility();
        void OpenMegerOrderDialog(List<Order> orders);

    }
}