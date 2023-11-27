using POSSUM.Base;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Views.CheckOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace POSSUM.Presenters.OpenOrder
{
    public class OpenOrderPresenter : OrderPresenter
    {
        IOpenOrderView view;
        private bool IsReturn = false;
        private long orderId = 0;
        int orderDirection = 1;
        internal void LoadTablesClick()
        {
            int floorId = view.GetFloorId();
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
            view.SetFoodTablesResult(tables);
        }

        public bool UpdateLocation(FoodTable table)
        {
            return new FoodTableRepository(PosState.GetInstance().Context).UpdateLocation(table);
        }

        ApplicationDbContext db;
        public OpenOrderPresenter(IOpenOrderView view, long orderId = 0)
        {
            this.view = view;
            this.orderId = orderId;
            db = PosState.GetInstance().Context;
        }
        internal Order GetOrderMasterDetailById(Guid orderId)
        {
            var orderRepository = new OrderRepository(db);
            return orderRepository.GetOrderMasterDetailById(orderId);
        }
        internal void HandelMergeOrderClick()
        {
            var order = new Order();
            var progressDialog = new ProgressWindow();

            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    order = MergeOrders();
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));

                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.SetOrderMaster(order);
            //  view.SetTableOrderResult(orders);
        }
        internal void HandelMoveOrderClick()
        {
            var orders = new List<Order>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    orders = MoveOrders();
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();

                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.SetTableOrderResult(orders);
        }

        internal bool MergeNewOrder(Order newOrder, int selectedTableId)
        {
            try
            {
                using (var db = PosState.GetInstance().Context)
                {

                    var order = db.OrderMaster.OrderByDescending(o => o.CreationDate).FirstOrDefault(o => o.TableId == selectedTableId && o.Status == OrderStatus.AssignedKitchenBar);
                    var orderId = newOrder.Id;
                    if (order == null)
                    {
                        order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
                    }
                    else
                    {
                        db.OrderMaster.Remove(newOrder.GetFrom());
                    }

                    foreach (var line in newOrder.OrderLines)
                    {

                        var _line = db.OrderDetail.FirstOrDefault(o => o.Id == line.Id);
                        _line.OrderId = order.Id;
                        _line.ItemStatus = (int)OrderStatus.AssignedKitchenBar;
                    }
                    var newTotal = newOrder.OrderLines.Where(s => s.Active == 1 && s.ItemType == ItemType.Individual).Sum(s => s.GrossAmountDiscounted());
                    order.OrderTotal = order.OrderTotal + newTotal;
                    order.Status = OrderStatus.AssignedKitchenBar;
                    order.TableId = selectedTableId;
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    newOrder.Id = order.Id;
                    newOrder.TableId = selectedTableId;


                }

                var printList = newOrder.OrderLines.Where(i => i.Product.Bong).ToList();//i.ItemStatus == (int)OrderStatus.New &&
                var groupItemList = newOrder.OrderLines.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                foreach (var itm in groupItemList)
                {
                    var lst = itm.ItemDetails.Where(i => i.Product.Bong).Select(i => new OrderLine
                    {
                        OrderId = MasterOrder.Id,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        ItemId = i.Product.Id,
                        ItemComments = i.ItemComments,
                        Product = i.Product,
                        PreparationTime = i.Product.PreparationTime,
                        PrinterId = i.Product.PrinterId


                    }).ToList();
                    printList.AddRange(lst);
                }
                if (printList.Count > 0 && Defaults.BONG && Defaults.SaleType == SaleType.Restaurant)//
                {
                    //PrintOrderWindow printOrder = new PrintOrderWindow();
                    //printOrder.PrintOrder(MasterOrder, printList, true, 1);
                    DirectPrint drctOrderPrint = new DirectPrint();
                    var order = new Order
                    {
                        Id = newOrder.Id,
                        CreationDate = newOrder.CreationDate,
                        Type = newOrder.Type,
                        TableId = newOrder.TableId,
                        TableName = newOrder.TableName,
                        OrderComments = newOrder.OrderComments,
                        OrderLines = printList,
                        CustomerId = newOrder.CustomerId
                    };

                    drctOrderPrint.PrintBong(order, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                return false;
            }
        }

        internal void HandelPendingMoveOrderClick()
        {
            var orders = new List<Order>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    orders = MoveOrders();
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            HandelPendingClick();
        }

        private Order MergeOrders()
        {
            var selectedOrders = view.GetSelectedOrders();
            var selectedTable = view.GetSelectedTable();
            var type = view.GetOrderType();
            Guid orderId = default(Guid);
            var orderLines = new List<OrderLine>();


            try
            {

                List<Guid> itemForLogs = new List<Guid>();
                using (var db = PosState.GetInstance().Context)
                {

                    decimal orderTotal = selectedOrders.Sum(c => c.OrderTotal);
                    string  bong = selectedOrders.Select(c=>c.Bong).FirstOrDefault();
                    string dailybong = selectedOrders.Select(c => c.DailyBong).FirstOrDefault();


                    foreach (var currentOrder in selectedOrders) // adding all selected orders in orderLines list
                    {
                        var lines = db.OrderDetail.Where(ol => ol.OrderId == currentOrder.Id && ol.Active == 1).ToList();
                        foreach (var line in lines)
                        {
                            line.ItemId = line.ItemId;
                            orderLines.Add(line);
                        }

                    }

                    if (orderLines.Count > 0)
                    {
                        var newLines = new List<OrderLine>();

                        var groups = orderLines.Where(c=>c.GroupKey==null).GroupBy(g => new { g.ItemId, g.UnitPrice, g.GroupId }).ToList();
                        var groupItemDiscount = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice, g.GroupId }).ToList();

                        foreach (var grp in groups) /*Summation of Items*/
                        {
                            var item = grp.First();
                            item.Quantity = grp.Sum(s => s.Quantity);
                            newLines.Add(item);

                            var groupIngredients = orderLines.Where(c =>c.GroupId==item.ItemId && c.GroupKey != null).GroupBy(g => new { g.ItemId, g.UnitPrice, g.GroupId }).ToList();

                            foreach (var grp2 in groupIngredients) /*Summation of Ingredients*/
                            {
                                var igredient = grp2.First();
                                igredient.GroupKey = item.Id;
                                igredient.Quantity = grp2.Sum(s => s.Quantity);
                                newLines.Add(igredient);
                            }
                        }

                    

                        foreach (var grp in groupItemDiscount) /*Summation of ItemDiscount*/
                        {
                            var line = grp.First();
                            line.ItemDiscount = grp.Sum(s => s.ItemDiscount);
                            newLines.Add(line);
                        }



                        //var orderTotal = newLines.Select(s => new
                        //{
                        //    GrossTotal = s.UnitPrice * (s.Direction * s.Quantity)-s.ItemDiscount}).Sum(ol => ol.GrossTotal);

                        int lastNo = 0;
                      //  var total = newLines.Where(ol => ol.ItemType != ItemType.Grouped).Sum(s => s.GrossAmountDiscounted());
                        var ord = db.OrderMaster.OrderByDescending(o => o.CreationDate).Where(o => o.OrderNoOfDay != "" && o.CreationDate >= DateTime.Now).SingleOrDefault();
                        if (ord != null)
                        {
                            string[] orNo = ord.OrderNoOfDay.Split('-');
                            if (orNo.Length > 1)
                                int.TryParse(orNo[1], out lastNo);
                        }

                        var endDate = DateTime.Now.Day;
                        string OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + endDate + "-" + (lastNo + 1);

                        var order = new Order
                        {
                            Id = Guid.NewGuid(),
                            TableId = selectedTable.Id,
                            CreationDate = DateTime.Now,
                            OrderTotal =orderTotal, //total,
                            UserId = Defaults.User.Id,
                            Status = OrderStatus.AssignedKitchenBar,
                            ShiftNo = App.MainWindow.ShiftNo,
                            Updated = 1,
                            PaymentStatus = 2,
                            Comments = selectedTable.Name,
                            Type = type,
                            TerminalId = Defaults.Terminal.Id,
                            OutletId = Defaults.Terminal.Outlet.Id,
                            OrderNoOfDay = OrderNoOfDay,
                            Bong=bong,
                            DailyBong=dailybong
                        };
                        db.OrderMaster.Add(order);
                        var journal = new Journal
                        {
                            OrderId = order.Id,
                            ActionId = Convert.ToInt32(JournalActionCode.NewOrderEntry),
                            Created = DateTime.Now,
                            TerminalId = order.TerminalId
                        };
                        if (!string.IsNullOrEmpty(Defaults.User.Id))
                            journal.UserId = Defaults.User.Id;
                        db.Journal.Add(journal);
                        foreach (var line in newLines)
                        {
                            itemForLogs.Add(line.ItemId);
                            line.OrderId = order.Id;
                            var _journal = new Journal
                            {
                                OrderId = order.Id,
                                ItemId = line.ItemId,
                                ActionId = Convert.ToInt32(JournalActionCode.ItemAdded),
                                Created = DateTime.Now,
                                TerminalId = order.TerminalId
                            };
                            if (!string.IsNullOrEmpty(Defaults.User.Id))
                                _journal.UserId = Defaults.User.Id;
                            db.Journal.Add(_journal);

                        }
                        string[] ary = new string[selectedOrders.Count];

                        int i = 0;
                        Guid ordId;
                        foreach (var currentOrder in selectedOrders)
                        {
                            ary[i] = currentOrder.Id.ToString();
                            ordId = currentOrder.Id;
                            ////  orderRepo.Remove(currentOrder.GetOrderFrom());
                            i++;
                        }

                        foreach (string id in ary)
                        {
                            var id2 = Guid.Parse(id);
                            var _ord = db.OrderMaster.FirstOrDefault(o => o.Id == id2);
                            _ord.Status = OrderStatus.OrderCancelled;
                            var _journal = new Journal
                            {
                                Created = DateTime.Now,
                                OrderId = _ord.Id,
                                ActionId = Convert.ToInt16(JournalActionCode.OrderCancelled),
                                UserId = Defaults.User.Id,
                                TerminalId = _ord.TerminalId
                            };

                            db.Journal.Add(_journal);
                            //var journs=  journalRepo.Where(j=>j.OrderId== _ord.Id).ToList();
                            //  if (journs != null && journs.Count > 0)
                            //  {
                            //      foreach (var journ in journs)
                            //      {
                            //         // journ.OrderId = order.Id;
                            //      }
                            //  }

                        }
                        db.SaveChanges();
                        orderId = order.Id;

                    }
                }
                return GetOrderMasterDetailById(orderId);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return null;
            }
        }
        private void ItemsJournalForMergeOrder(Guid orderId, List<Guid> items)
        {
            int actionId = Convert.ToInt16(JournalActionCode.ItemAdded);
            foreach (Guid itmid in items)
                LogWriter.JournalLog(actionId, orderId, itmid, null, "");
        }

        private List<Order> MoveOrders()
        {
            var selectedOrders = view.GetSelectedOrders();
            var selectedTable = view.GetSelectedTable();
            var type = view.GetMoveType();
            var previouseTableId = 0;
            try
            {
                using (var db = PosState.GetInstance().Context)
                {

                    foreach (var currentOrder in selectedOrders)
                    {
                        previouseTableId = currentOrder.TableId;
                        var order = db.OrderMaster.FirstOrDefault(o => o.Id == currentOrder.Id);
                        if (order != null)
                        {
                            int actionId = 0;
                            order.Type = type;
                            if (type == OrderType.TakeAway)
                            {
                                order.TableId = 0;
                                order.Comments = UI.Sales_TakeAwayButton;
                                actionId = Convert.ToInt16(JournalActionCode.OrderTypeTakeAway);
                            }
                            else
                            {
                                order.TableId = selectedTable.Id;
                                order.Comments = selectedTable.Name;
                                actionId = Convert.ToInt16(JournalActionCode.OrderTableSelected);
                            }
                            var journal = new Journal
                            {
                                Created = DateTime.Now,
                                OrderId = order.Id,
                                ActionId = actionId,
                                TableId = order.TableId,
                                UserId = Defaults.User.Id,
                                TerminalId = order.TerminalId
                            };

                            db.Journal.Add(journal);
                        }
                    }
                    db.SaveChanges();
                }
                return GetOpenOrderByTable(previouseTableId);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new List<Order>();
            }
        }

        internal void HandelCheckOutClick()
        {
            var res = false;

            decimal GrossTotal = view.GetOrderTotal();

            var selectedCustomer = view.GetSelectedTable();
            var order = view.GetOrder();
            OrderRepository orderRepo = new OrderRepository(db);
            orderDirection = order.Type == OrderType.Return ? -1 : 1;
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();

                        //GrossTotal = orderRepo.GetOrderMaster(order.Id).OrderTotal;

                        var obj = new CheckOutOrderWindow(GrossTotal, order.Id, orderDirection, orderRepo.GetOrderLinesById(order.Id), order.TableId);

                        var statusComplete = obj.ShowDialog() ?? false;
                        if (statusComplete)// && obj.ReceiptGenerated
                        {
                            res = true;
                            DirectPrint directPrint = new DirectPrint();
                            directPrint.PrintReceipt(order.Id, false, obj.ReceiptGenerated);

                            //var invoiceWindow = new PrintNewInvoiceWindow();
                            //if (IsReturn)
                            //    invoiceWindow.PrintReturnInvoice(order.Id);
                            //else
                            //{
                            //    invoiceWindow.PrintInvoice(order.Id, false);
                            //    //if (IsReturn == false)
                            //    //{
                            //    //    PrintOrderWindow printWindow = new PrintOrderWindow();
                            //    //    printWindow.PrintOrder(currentOrderMaster, currentorderDetails, true, 1, takeAway);
                            //    //}
                            //}
                            App.MainWindow.UpdateOrderCompleted(order.Id);
                        }

                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.NewRecord(res);

            view.SetTableOrderResult(GetOpenOrderByTable(selectedCustomer.Id));

        }

        internal void HandelOrderDetailClick(Guid orderId)
        {
            var orderLines = new List<OrderLine>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    var orderRepository = new OrderRepository(db);
                    orderLines = orderRepository.GetOrderLinesById(orderId);

                    progressDialog.Closed += (arg, e) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.SetOrderDetailResult(orderLines);
        }
        internal void HandelPerformaClick(Guid orderId)
        {
            try
            {
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.PrintPerforma), orderId);
                // PrintBillWindow billWindow = new PrintBillWindow(orderId);
                // billWindow.PrintBill(orderId);
                DirectPrint directPrint = new DirectPrint();
                directPrint.PrintBill(orderId);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        internal void HandelOrderCancelClick(Guid orderId)
        {
            if (CancelOrder(orderId))
                HandleOrderByTableClick();

        }
        internal void HandelPendingCancelClick(Guid orderId)
        {
            if (CancelOrder(orderId))
                HandelPendingClick();
        }
        internal void HandelPendingClick()
        {
            //TODO:SHAHID NEED TO GET TYPE
            //INSETAD SPLIT TO [GetTableOrders,GetPendingOrders,GetTakeAwayOrders], 
            //Preferably should use single function, where i can pass list of status to filter
            OrderType type = view.GetOrderType();
            var orders = new List<Order>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    orders = GetPendingOrder(type);
                    progressDialog.Closed += (arg, e) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.SetPendingOrderResult(orders);
        }
        internal List<Floor> LoadFloor()
        {
            List<Floor> floors = new List<Floor>();
            using (var db = PosState.GetInstance().Context)
            {

                floors = db.Floor.ToList();
            }
            return floors;
        }
        internal void HandelTableClick()
        {
            var tables = new List<FoodTable>();
            int floorId = view.GetFloor();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    tables = GetTables(floorId);
                    progressDialog.Closed += (arg, e) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.SetTableResult(tables);
        }
        public List<FoodTable> GetTables(int floorId)
        {
            return new FoodTableRepository(PosState.GetInstance().Context).GetTables(floorId, UI.OpenOrder_TableButton);


        }
        public void HandleOrderByTableClick()
        {
            var selectedTable = view.GetSelectedTable();
            var orders = new List<Order>();

            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    if (selectedTable != null)
                        orders = GetOpenOrderByTable(selectedTable.Id);
                    progressDialog.Closed += (arg, e) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.SetTableOrderResult(orders);
        }

        public List<Order> GetOpenOrderByTable(int tableId)
        {
            return new FoodTableRepository(PosState.GetInstance().Context).GetOpenOrderByTable(tableId);
        }

        public void HandleOpenTableOrders()
        {

            var orders = new List<Order>();

            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    orders = GetOpenTableOrders();
                    progressDialog.Closed += (arg, e) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            view.SetTableOrderResult(orders);
        }
        public List<Order> GetOpenTableOrders()
        {
            return new OrderRepository(db).GetOpenTableOrders(Defaults.Terminal.Id);

        }

        public bool CancelOrder(Guid orderId)
        {
            try
            {
                bool isCancelled = new OrderRepository(db).CancelOrder(orderId, Defaults.User.Id);
                if (isCancelled)
                    ((MainWindow)Application.Current.MainWindow).PostPosMiniOrderOrderToMqtt(orderId.ToString());
                return isCancelled;
            }
            catch
            {
                return false;
            }
        }
        public List<Order> GetPendingOrder(OrderType type)
        {
            return new OrderRepository(db).GetPendingOrders(type, Defaults.Terminal.Id);
        }
        private decimal OrderBalance(Order order)
        {

            decimal balance = order.OrderTotal;
            try
            {
                var db = PosState.GetInstance().Context;

                var payments = db.Payment.Where(p => p.OrderId == order.Id && p.Direction == 1).ToList();
                var paidAmount = payments.Sum(s => s.PaidAmount);
                balance = balance - paidAmount;
            }
            catch
            {
            }
            return balance;
        }

        internal List<OrderLine> HandelDeleteItemClick(Guid orderId, Guid lineId, List<OrderLine> details)
        {

            try
            {
                List<OrderLine> lines = new List<OrderLine>();
                using (var db = PosState.GetInstance().Context)
                {

                    var orderLine = db.OrderDetail.FirstOrDefault(ol => ol.Id == lineId);
                    var ingredients = db.OrderDetail.Where(ol => ol.GroupKey == lineId);

                    if (orderLine != null)
                    {
                        orderLine.Active = 0;
                        db.Entry(orderLine).State = System.Data.Entity.EntityState.Modified;

                        if (ingredients.Count() > 0 && ingredients != null)
                        {
                            foreach (var ingredient in ingredients)
                            {
                                ingredient.Active = 0;
                            db.Entry(ingredient).State = System.Data.Entity.EntityState.Modified;
                            }


                        }
                        db.SaveChanges();
                    }
                }
                using (var db = PosState.GetInstance().Context)
                {

                    var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
                    lines = db.OrderDetail.Where(o => o.OrderId == orderId && o.Active == 1).ToList();
                    if (lines.Count == 0)
                    {
                        order.Status = OrderStatus.OrderCancelled;
                    }
                    else
                    {
                        decimal total = 0;
                        decimal _Tax = 0;
                        foreach (OrderLine lineItem in lines)
                        {
                            if (lineItem.Active == 1 && lineItem.IsCoupon != 1 && lineItem.ItemType != ItemType.Grouped)
                            {
                                _Tax += lineItem.VatAmount();
                                total += lineItem.GrossAmount();
                            }

                        }
                        decimal ordertotal = total;// + _Tax;
                        order.OrderTotal = ordertotal;
                    }
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                if (lines.Count == 0)
                {
                    return new List<OrderLine>();
                }
                else
                {
                    return details;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return details;
            }

        }

        internal void RenameOrderTable(int tableId, string name)
        {
            using (var db = PosState.GetInstance().Context)
            {

                var orders = db.OrderMaster.Where(o => o.TableId == tableId && o.Status == OrderStatus.AssignedKitchenBar).ToList();
                foreach (var order in orders)
                {
                    order.Comments = name;
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        internal void SetHandelSplitWholeClick(int number)
        {
            bool res = false;
            var order = view.GetOrder();

            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    List<OrderLine> lines = new List<OrderLine>();
                    OrderRepository repository = new OrderRepository(db);
                    lines = repository.GetOrderLinesById(order.Id);

                    res = SplitOrder(order, lines, number);   /*Spliting OrderMaster and OrderDetail*/

                    if (res)
                    {
                        var _order = repository.GetOrderMaster(order.Id);
                        repository.EditOrderMaster(_order, number); /* Updating previous OrderMaster*/

                    }
                    progressDialog.Closed += (arg, e) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            if (res)
            {
                if (order.Type == OrderType.TableOrder)
                    HandelTableClick();
                else
                    HandelPendingClick();
            }
        }
        internal bool SplitOrder(Order order, List<OrderLine> lines, int number)
        {

            try
            {

                using (var db = PosState.GetInstance().Context)
                {

                    if (order != null)
                    {
                        int lastNo = 0;
                        var currentDate = DateTime.Now.Date;
                        var ord = db.OrderMaster.OrderByDescending(o => o.CreationDate).FirstOrDefault(o => o.OrderNoOfDay != "" && o.CreationDate >= currentDate);
                        if (ord != null)
                        {
                            string[] orNo = ord.OrderNoOfDay.Split('-');
                            if (orNo.Length > 1)
                                int.TryParse(orNo[1], out lastNo);
                        }



                        for (int i = 1; i < number; i++)
                        {
                            string OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day + "-" + (lastNo + 1);
                            lastNo++;
                            var _order = new Order
                            {
                                Id = Guid.NewGuid(),
                                CreationDate = DateTime.Now,
                                TableId = order.TableId,
                                Status = order.Status,
                                PaymentStatus = order.PaymentStatus,
                                OrderLines = order.OrderLines,
                                OrderNoOfDay = OrderNoOfDay,
                                ShiftNo = order.ShiftNo,
                                ShiftOrderNo = order.ShiftOrderNo,
                                OrderTotal = order.OrderTotal,
                                TaxPercent = order.TaxPercent,
                                UserId = Defaults.User.Id,
                                Updated = order.Updated,
                                OutletId = Defaults.Outlet.Id, //order.Outlet == null ? Defaults.Outlet : order.Outlet,
                                TerminalId = order.TerminalId,
                                TrainingMode = order.TrainingMode,
                                Type = order.Type,
                                Comments = order.Comments,
                                Bong = order.Bong,
                                DailyBong=order.DailyBong
                            };

                            _order.OrderTotal = _order.OrderTotal / number;
                            _order.OrderLines = null;// Order detail is creating below 

                            db.OrderMaster.Add(_order);




                            var journal = new Journal
                            {
                                OrderId = _order.Id,
                                ActionId = Convert.ToInt32(JournalActionCode.NewOrderEntry),
                                Created = DateTime.Now,
                                TerminalId = order.TerminalId
                            };
                            if (!string.IsNullOrEmpty(Defaults.User.Id))
                                journal.UserId = Defaults.User.Id;
                            db.Journal.Add(journal);
                            foreach (var line in lines)
                            {
                                var _line = line.GetFrom();
                                _line.Id = Guid.NewGuid();
                                _line.Quantity = _line.Quantity / number;
                                _line.ItemDiscount = _line.ItemDiscount / number;
                                _line.OrderId = _order.Id;
                                db.OrderDetail.Add(_line);
                                 
                                /***********************************/
                                if (line.ItemDetails != null)
                                {
                                    foreach (var itmdetail in line.ItemDetails)
                                    {
                                        var _orderLine = itmdetail.GetFrom();
                                        _orderLine.Id = Guid.NewGuid();
                                        _orderLine.OrderId = _order.Id;
                                        db.OrderDetail.Add(_orderLine);
                                    }
                                }

                                /********* Ingredient **********/
                                if (line.IngredientItems != null)
                                {
                                    foreach (var itmdetail in line.IngredientItems)
                                    {
                                        var _orderLine = itmdetail.GetFrom();
                                        _orderLine.Id = Guid.NewGuid();
                                        _orderLine.Quantity = itmdetail.Quantity / number;
                                        _orderLine.OrderId = _order.Id;
                                        _orderLine.GroupKey = _line.Id;//********
                                        db.OrderDetail.Add(_orderLine);
                                    }
                                }

                                /*************************************/
                                var _journal = new Journal
                                {
                                    OrderId = _order.Id,
                                    ItemId = line.ItemId,
                                    ActionId = Convert.ToInt32(JournalActionCode.ItemAdded),
                                    Created = DateTime.Now,
                                    TerminalId = _order.TerminalId 
                                };
                                if (!string.IsNullOrEmpty(Defaults.User.Id))
                                    _journal.UserId = Defaults.User.Id;
                                db.Journal.Add(_journal);
                            }

                        }


                        foreach (var line in lines)  /*Updating Quantity of previous Order detail*/
                        {
                            var _line = db.OrderDetail.First(o => o.Id == line.Id);
                            _line.Quantity = _line.Quantity / number;
                            _line.ItemDiscount= _line.ItemDiscount / number;

                            db.Entry(_line).State = System.Data.Entity.EntityState.Modified;

                            foreach (var i in line.IngredientItems)
                            {
                                var itm = db.OrderDetail.First(o => o.Id == i.Id); /*Selecting record of Group key against OrderDeail id*/
                                itm.Quantity= itm.Quantity / number;
                               // itm.GroupKey = line.Id;
                                db.Entry(itm).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                    }
                    db.SaveChanges();
                }


                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                view.ShowError(ex.Message, Defaults.AppProvider.AppTitle);
                return false;
            }

        }

    }
}
