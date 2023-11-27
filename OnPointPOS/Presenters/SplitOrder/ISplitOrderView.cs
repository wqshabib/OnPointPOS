using System;
using System.Collections.Generic;
using POSSUM.Base;
using POSSUM.Model;

namespace POSSUM.Presenters.SplitOrder
{
    public interface ISplitOrderView : IBaseView
    {
        void CloseWindow();
        Order GetCurrentOrder();
        List<OrderLine> GetOldOrderDetail();
        List<OrderLine> GetNewOrderDetail();
        Guid GetOrderId();
        Guid GetNewOrderId();
        void SetNewOrderId(Guid id);
        decimal GetNewOrderTotal();
        decimal GetOldOrderTotal();
        void SetOrderDetailResult(List<OrderLine> lst);

        void OrderSaveCompleted(bool res);
        FoodTable GetSelectedCustomer();
        OrderType GetOrderType();
        void CalculatLeftTotal(IList<OrderLine> list);
        void CalculatRightTotal(IList<OrderLine> list);
        bool IsNewOrder();
    }
}