using System;
using System.Collections.Generic;
using System.Threading;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Views.CheckOut;
using System.Linq;
namespace POSSUM.Presenters.FoodTables
{
    public class FoodTablePresenter
    {
        private readonly IFoodTableView _view;

        ApplicationDbContext db;
        public FoodTablePresenter(IFoodTableView view)
        {
            _view = view;
            db = PosState.GetInstance().Context;
        }

        public FoodTablePresenter()
        {
            db = PosState.GetInstance().Context;

        }

        public void LoadFloor()
        {
            List<Floor> floors = new FoodTableRepository(PosState.GetInstance().Context).LoadFloor();

            _view.SetFloors(floors);
        }

        internal void LoadTablesClick()
        {
            int floorId = _view.GetFloorId();
            var tables = new List<FoodTable>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(() =>
            {
                tables = GetTables(floorId);
                progressDialog.Closed += (arg, e) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
            });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetFoodTablesResult(tables);
        }





        public List<Order> GetOpenOrderByTable(int tableId)
        {
            return new OrderRepository(db).GetOpenOrderByTable(tableId);
        }
        public bool UpdateBongName(string name)
        {
            var order = _view.GetCurrentOrder();

            if (order == null)
            {
                _view.ShowError(Defaults.AppProvider.AppTitle, UI.Global_Select + "  Order");
                return false;
            }
            order.Comments = name;
            return new OrderRepository(db).UpdateBongName(order.Id, name);
        }
        public List<FoodTable> GetTables(int floorId)
        {
            return new FoodTableRepository(PosState.GetInstance().Context).GetTables(floorId, UI.OpenOrder_TableButton);
        }

        public bool UpdateLocation(FoodTable table)
        {
           return new FoodTableRepository(PosState.GetInstance().Context).UpdateLocation(table);
        }


        internal bool HandelCheckOutClick()
        {
            var res = false;
            
            var selectedCustomer = _view.GetSelectedTable();
            var order = _view.GetCurrentOrder();
            var grossTotal = order.OrderTotal;
            var orderRepo = new OrderRepository(db);
         int   orderDirection = order.Type == OrderType.Return ? -1 : 1;
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(() =>
            {
                progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressDialog.Close();

                    var obj = new CheckOutOrderWindow(grossTotal, order.Id, orderDirection,
                        orderRepo.GetOrderLinesById(order.Id), order.TableId);


                    var statusComplete = obj.ShowDialog() ?? false;
                    if (statusComplete)
                    {
                        res = true;
                        var directPrint = new DirectPrint();
                        directPrint.PrintReceipt(order.Id, false, obj.ReceiptGenerated);

                        App.MainWindow.UpdateOrderCompleted(order.Id);
                    }
                }));
            });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            //_view.SetTableOrderResult(GetOpenOrderByTable(selectedCustomer.Id));
           // _view.NewRecord(res);
            return res;

           
        }

        internal void HandelMergeOrderClick()
        {
            var order = new Order();
            var progressDialog = new ProgressWindow();

            var backgroundThread = new Thread(() =>
            {
                order = MergeOrders();
                progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
            });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetOrderMaster(order);

            //  view.SetTableOrderResult(orders);
        }

        internal void HandleHoldClick(Order order,  List<OrderLine> CurrentorderDetails, FoodTable selectedTable)
        {
            if (CurrentorderDetails.Count == 0)
            {

                App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                return;
            }
            
          
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                order.Status = OrderStatus.AssignedKitchenBar;
                if (selectedTable != null && selectedTable.Id > 0)
                {
                    order.SelectedTable = selectedTable;
                    order.TableId = selectedTable.Id;
                    if (string.IsNullOrEmpty(order.Comments))
                        order.Comments = selectedTable.Name;

                }
                order.Updated = 1;
                var printList = CurrentorderDetails.Where(itm => itm.Product.Bong && itm.ItemStatus != 3).ToList();
                var groupItemList = CurrentorderDetails.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                foreach (var itm in groupItemList)
                {
                    var lst = itm.ItemDetails.Where(i => i.Product.Bong).Select(i => new OrderLine
                    {
                        OrderId = order.Id,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        ItemId = i.Product.Id,
                        ItemComments = i.ItemComments,
                        Product = i.Product


                    }).ToList();
                    printList.AddRange(lst);
                }
                UpdateOrderDetail(CurrentorderDetails, order);
                new OrderRepository(db).SaveOrderMaster(order);

                if (Defaults.BONG && Defaults.SaleType == SaleType.Restaurant)
                    SendPrintToKitchen(order, printList);

               

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

                new OrderRepository(db).SaveOrderLines(lines, order);


                return true;

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }
        public void SendPrintToKitchen(Order order, List<OrderLine> printList, bool viewNew = true)
        {

            DirectPrint directPrint = new DirectPrint();

            if (printList.Count > 0)
            {
                order.OrderLines = printList;
                directPrint.PrintBong(order, true);
            }

        }
        private Order MergeOrders()
        {
            var selectedOrders = _view.GetSelectedOrders();
            var selectedTable = _view.GetSelectedTable();
            var type = OrderType.TableOrder;
           
            try
            {

                var itemForLogs = new List<Guid>();

                Order order = new OrderRepository(db).MergeOrders(selectedOrders, selectedTable, type, Defaults.User.Id, Defaults.Terminal.Id, Defaults.Outlet.Id, App.MainWindow.ShiftNo);
                Guid orderId = order.Id;
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeNew), orderId);
                foreach (var line in order.OrderLines)
                    itemForLogs.Add(line.Id);
                ItemsJournalForMergeOrder(orderId, itemForLogs);
                return order;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return null;
            }
        }
        private void ItemsJournalForMergeOrder(Guid orderId, List<Guid> items)
        {
            foreach (var itmid in items)
            {
                LogWriter.JournalLog(JournalActionCode.ItemAdded, orderId, itmid);
            }
        }
    }
}