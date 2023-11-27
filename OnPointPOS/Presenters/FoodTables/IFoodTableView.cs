using System.Collections.Generic;
using POSSUM.Base;
using POSSUM.Model;

namespace POSSUM.Presenters.FoodTables
{
    public interface IFoodTableView : IBaseView
    {
        void SetFoodTablesResult(List<FoodTable> tables);
        void SetFloors(List<Floor> floors);
        Order GetCurrentOrder();
        int GetFloorId();
        void SetOrderMaster(Order order);
        List<Order> GetSelectedOrders();
        FoodTable GetSelectedTable();

        void SetTableOrderResult(List<Order> orders);
       void  NewRecord(bool res);
    }
}