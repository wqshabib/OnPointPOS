using System;
using System.Collections.Generic;
using POSSUM.Base;
using POSSUM.Model;

namespace POSSUM.Presenters.ReturnOrder
{
    public interface IReturnOrderView : IBaseView
    {
        Order GetCurrentOrder();
        List<OrderLine> GetOldOrderDetail();
        List<OrderLine> GetReturnOrderDetail();
        Guid GetOrderId();
        Guid GetNewOrderId();
        void SetNewOrderId(Guid id);
        decimal GetReturnOrderTotal();
        decimal GetOldOrderTotal();
        void SetOrderDetailResult(List<OrderLine> lst);

        void OrderSaveCompleted(bool res);
        FoodTable GetSelectedCustomer();
        OrderType GetOrderType();
        void CalculatLeftTotal(IList<OrderLine> list);
        void CalculatRightTotal(IList<OrderLine> list);
    }
}