using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Presenters.OpenOrder
{
    public interface IOpenOrderView
    {
        void SetFoodTablesResult(List<FoodTable> tables);
        int GetFloorId();
        void NewRecord(bool res);
        List<Order> GetSelectedOrders();
        Order GetOrder();
        void SetOrderMaster(Order order);
        List<OrderLine> GetOrderDetail();
        //long GetOrderId();    
        FoodTable GetSelectedTable();
        void CalculatOrderTotal(IList<OrderLine> list);
        //string GetHoldComments();
        //string GetOrderComments();
        OrderType GetOrderType();
        decimal GetOrderTotal();
        void SetTableResult(List<FoodTable> customers);
        void SetTableOrderResult(List<Order> orders);
        void SetPendingOrderResult(List<Order> orders);
        void SetOrderDetailResult(List<OrderLine> orderLines);
        OrderType GetMoveType();
        void ShowError(string message, string title);

        int GetFloor();


    }
}
