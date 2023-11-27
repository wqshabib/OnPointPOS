using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using POSSUM.Base;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Views.CheckOut;

namespace POSSUM.Presenters.SplitOrder
{
    public class SplitOrderPresenter : OrderPresenter
    {
        //  ApplicationDbContext db;
        readonly ISplitOrderView _view;

        Guid _orderId;
        readonly bool returnOrder = false;
        public SplitOrderPresenter(ISplitOrderView view)
        {
            _view = view;
            // db = PosState.GetInstance().Context;
        }
        public void HandelSplitTableOrderClick()
        {
            bool res = false;
            bool isnew = _view.IsNewOrder();
            ProgressWindow progressDialog = new ProgressWindow();
            Thread backgroundThread = new Thread(
                () =>
                {
                    if (isnew)
                    {
                        Order order = SaveOrderMaster();
                        res = SaveOrderDetail(order);
                        _orderId = order.Id;
                    }
                    else
                    {
                        Order order = LastOpenOrderOnTable();
                        if (order == null)
                            order = SaveOrderMaster();
                        res = SaveOrderDetail(order);
                        _orderId = order.Id;
                    }
                    UpdateOrderMaster();
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();



                    }));
                });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetNewOrderId(_orderId);
            _view.OrderSaveCompleted(res);
        }
        public void HandelMergeTableOrderClick(Order order)
        {
            bool res = false;

            ProgressWindow progressDialog = new ProgressWindow();
            Thread backgroundThread = new Thread(
                () =>
                {

                    res = SaveOrderDetail(order);
                    _orderId = order.Id;

                    UpdateOrderMaster();

                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();



                    }));
                });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetNewOrderId(_orderId);
            _view.OrderSaveCompleted(res);
        }

        public void HandelSplitOrderClick()
        {
            bool res = false;
            bool statusComplete = false;
            ProgressWindow progressDialog = new ProgressWindow();
            Thread backgroundThread = new Thread(
                () =>
                {
                    Order order = SaveOrderMaster();
                    res = SaveOrderDetail(order);
                    int direction = order.Type == OrderType.Return ? -1 : 1;
                    UpdateOrderMaster();
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();

                        _orderId = order.Id;
                        decimal orderTotal = _view.GetNewOrderTotal();

                        var obj = new CheckOutOrderWindow(orderTotal, _orderId, direction, _view.GetNewOrderDetail(), order.TableId);

                        statusComplete = obj.ShowDialog() ?? false;
                        if (statusComplete)
                        {
                            DirectPrint directPrint = new DirectPrint();
                            if (returnOrder)
                                directPrint.PrintReceipt(_orderId, false, obj.ReceiptGenerated);
                            else
                                directPrint.PrintReceipt(_orderId, false, obj.ReceiptGenerated);

                            _view.CloseWindow();
                        }
                        else
                        {
                            CancelOrderDetail(order.Id);
                        }

                    }));
                });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetNewOrderId(_orderId);
            _view.OrderSaveCompleted(res);
            if (statusComplete)
                App.MainWindow.UpdateOrderCompleted(_orderId);
        }

        private void CancelOrder(Guid guid)
        {
            try
            {
                new OrderRepository(PosState.GetInstance().Context).CancelOrder(guid, Defaults.User.Id);
            }
            catch (Exception ex)
            {

                LogWriter.LogException(ex);
            }
        }

        public void HandelCheckoutOrderClick()
        {
            OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
            MasterOrder = _view.GetCurrentOrder();
            _orderId = _view.GetOrderId();

            var orderDetails = orderRepo.GetOrderLinesById(_orderId);
            bool res = false;
            bool statusComplete = false;
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                () =>
                {
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();

                        decimal orderTotal = _view.GetOldOrderTotal();
                        var obj = new CheckOutOrderWindow(orderTotal, _orderId, 1, orderDetails, MasterOrder.TableId);
                        statusComplete = obj.ShowDialog() ?? false;
                        if (statusComplete)
                        {
                            res = true;
                            DirectPrint directPrint = new DirectPrint();
                            directPrint.PrintReceipt(_orderId, false, obj.ReceiptGenerated);
                            _view.CloseWindow();
                        }


                    }));

                });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            if (res)
                _view.CalculatLeftTotal(new List<OrderLine>());
            _view.OrderSaveCompleted(true);
            if (statusComplete)
                App.MainWindow.UpdateOrderCompleted(_orderId);



        }

        public void HandelParkeraClick(Order newOrder)
        {
            bool res = false;
            ProgressWindow progressDialog = new ProgressWindow();
            Thread backgroundThread = new Thread(
                () =>
                {
                    //Order orderViewModel = _view.GetCurrentOrder();
                    //newOrder.Bong = orderViewModel.Bong;
                    Order order = SaveNewOrderMaster(newOrder);
                    res = SaveOrderDetail(order);
                    UpdateOrderMaster();
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();

                        _orderId = order.Id;


                    }));
                });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetNewOrderId(_orderId);
            _view.OrderSaveCompleted(res);

        }
        public Order SaveOrderMaster()
        {
            var oldOrder = _view.GetCurrentOrder();
            var selectedTable = _view.GetSelectedCustomer();
            var type = _view.GetOrderType();
            decimal orderTotal = _view.GetNewOrderTotal();
            if (Defaults.Terminal == null || Defaults.Outlet == null)
                Defaults.Init();
            var order = new Order
            {
                CreationDate = DateTime.Now,
                Bong = oldOrder.Bong,
                DailyBong = oldOrder.DailyBong,
                UserId = Defaults.User.Id,
                OutletId = Defaults.Outlet != null ? Defaults.Outlet.Id : Guid.Empty,
                TerminalId = Defaults.Terminal != null ? Defaults.Terminal.Id : Guid.Empty,
                TableId = selectedTable.Id == 0? oldOrder.TableId : selectedTable.Id,
                OrderTotal = orderTotal,
                Status = OrderStatus.AssignedKitchenBar,
                ShiftNo = App.MainWindow.ShiftNo,
                Updated = 1,
                PaymentStatus = 2,
                Comments = selectedTable.Id == 0 ? oldOrder.Comments : selectedTable.Name,
                OrderComments = "",
                Type = type

            };

            try
            {
                OrderRepository orderhandler = new OrderRepository(PosState.GetInstance().Context);
                order = orderhandler.SaveOrderMaster(order);
                order.Outlet = Defaults.Outlet;
                return order;

            }
            catch (Exception exp)
            {

                LogWriter.LogWrite(exp);
                return order;
            }
        }

        public Order SaveNewOrderMaster(Order order)
        {

            try
            {
                order.CreationDate = DateTime.Now;
                return new OrderRepository(PosState.GetInstance().Context).SaveOrderMaster(order);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return order;
            }
        }

        public bool SaveOrderDetail(Order order)
        {
            try
            {
                List<OrderLine> orderDetail = GetLineItem(_view.GetNewOrderDetail());
                OrderRepository orderRepository = new OrderRepository(PosState.GetInstance().Context);
                List<OrderLine> lines = new List<OrderLine>();
                foreach (var line in orderDetail)
                {
                    line.ItemStatus = (int)order.OrderStatusFromType;
                    line.OrderId = order.Id;
                    lines.Add(line);
                }

                if (orderRepository.SaveOrderLines(lines, order))
                {
                    var newsum = orderDetail.Sum(s => s.GrossAmountDiscounted());
                    order.OrderTotal = newsum;
                    return true;
                }
                else
                    throw new Exception("Failed to save order detail during split order");


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }


        public bool CancelOrderDetail(Guid orderId)
        {
            try
            {
                var oldOrderId = _view.GetOrderId();
                var lines = new OrderRepository(PosState.GetInstance().Context).CancelOrderDetail(orderId, oldOrderId, Defaults.User.Id);
                _view.SetOrderDetailResult(lines);
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }


        public Order UpdateOrderMaster()
        {
            Order orderViewModel = _view.GetCurrentOrder();
            List<OrderLine> orderDetails = GetLineItem(_view.GetOldOrderDetail());

            try
            {
                return new OrderRepository(PosState.GetInstance().Context).UpdateSpliteOrder(orderViewModel, orderDetails, _view.GetOldOrderTotal());
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new Order();
            }
        }

        public bool UpdateOrderDetail(List<OrderLine> orderDetail, Order order)
        {
            try
            {
                return new OrderRepository(PosState.GetInstance().Context).UpdateOrderDetail(orderDetail, order);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }

        public bool HandelSplitItemClick(OrderLine selectedItem, int number)
        {
            try
            {
                using (OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context))
                {
                    return orderRepo.SplitOrderLine(selectedItem, number, Defaults.User.Id);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message, Defaults.AppProvider.AppTitle);
                return false;
            }
        }

        public void HandelOrderDetailClick()
        {
            _orderId = _view.GetOrderId();
            var orderLines = new List<OrderLine>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                () =>
                {
                    var orderRepository = new OrderRepository(PosState.GetInstance().Context);
                    orderLines = orderRepository.GetOrderLinesById(_orderId);
                    progressDialog.Closed += (arg, e) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                    }));
                });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetOrderDetailResult(orderLines);
        }

        public void MergeItems(List<OrderLine> items)
        {
            using (var db = new ApplicationDbContext())
            {
                foreach (var item in items)
                {
                    var line = db.OrderDetail.First(ol => ol.Id == item.Id);
                    db.OrderDetail.Remove(line);
                }

                //db.OrderDetail.RemoveRange(items);

                var qty = items.Sum(ol => ol.Quantity);
                var discount = items.Sum(ol => ol.ItemDiscount);


                var newLine = items.First();
                newLine.Id = Guid.NewGuid();
                newLine.Quantity = qty;
                newLine.ItemDiscount = discount;

                db.OrderDetail.Add(newLine);
                db.Entry(newLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                db.SaveChanges();
            }
        }
        public void MergeIngredientItems(OrderLine item)
        {
            using (var db = new ApplicationDbContext())
            {
                // var ingredientItems = item.IngredientItems.ToList(); //db.OrderDetail.Where(c =>c.OrderId==orderId && c.GroupId== itemId).ToList();
                var items = db.OrderDetail.Where(c => c.OrderId == item.OrderId && c.GroupId == item.ItemId).ToList();
                foreach (var it in items)
                {
                    var line = db.OrderDetail.First(ol => ol.Id == it.Id);
                    db.OrderDetail.Remove(line);
                }

                db.SaveChanges();

                var myStoreList = items.GroupBy(g => new { g.ItemId, g.UnitPrice }).ToList();
                var ingredients = myStoreList.Where(c => c.Count() > 1).ToList();

                foreach (var ingredient in myStoreList)
                {
                    var qty = ingredient.Sum(ol => ol.Quantity);
                    var newLine = ingredient.First();
                    newLine.Id = Guid.NewGuid();
                    newLine.ItemId = ingredient.Key.ItemId;
                    newLine.Quantity = qty;
                    newLine.GroupId = item.ItemId;
                    newLine.GroupKey = item.Id;
                    db.OrderDetail.Add(newLine);
                    // db.Entry(newLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                }
                db.SaveChanges();
            }
        }

        private Order LastOpenOrderOnTable()
        {
            using (var db = new ApplicationDbContext())
            {
                var selectedTable = _view.GetSelectedCustomer();

                return db.OrderMaster.OrderByDescending(o => o.CreationDate).FirstOrDefault(o => o.TableId == selectedTable.Id && o.Status == OrderStatus.AssignedKitchenBar);
            }

        }
        public List<Order> GetOpenOrdersOnTable()
        {
            using (var db = new ApplicationDbContext())
            {
                var selectedTable = _view.GetSelectedCustomer();

                return db.OrderMaster.Where(o => o.TableId == selectedTable.Id && o.Status == OrderStatus.AssignedKitchenBar).ToList();
            }
        }

        private List<OrderLine> GetLineItem(List<OrderLine> details)
        {
            List<OrderLine> lines = new List<OrderLine>();
            foreach (var item in details)
            {
                var lineItem = new OrderLine
                {
                    Id = item.Id,
                    Active = item.Active,
                    BIDS = item.BIDS,
                    BIDSNO = item.BIDSNO,
                    CampaignApplied = item.CampaignApplied,
                    CampaignId = item.CampaignId,
                    Direction = item.Direction,
                    DiscountDescription = item.DiscountDescription,
                    DiscountedUnitPrice = item.DiscountedUnitPrice,
                    DiscountPercentage = item.DiscountPercentage,
                    DiscountType = item.DiscountType,
                    GroupId = item.GroupId,
                    IsCoupon = item.IsCoupon,
                    ItemComments = item.ItemComments,
                    ItemDiscount = item.ItemDiscount,
                    ItemId = item.ItemId,
                    ItemIdex = item.ItemIdex,
                    ItemStatus = item.ItemStatus,
                    ItemType = item.ItemType,
                    OrderType = item.OrderType,
                    Percentage = item.Percentage,
                    PrinterId = item.PrinterId,
                    Product = new Product
                    {
                        Id = item.Product.Id,
                        Description = item.Product.Description,
                        Price = item.Product.Price
                    },
                    PurchasePrice = item.PurchasePrice,
                    Quantity = item.Quantity,
                    ReceiptItems = item.ReceiptItems,
                    TaxPercent = item.TaxPercent,
                    UnitPrice = item.UnitPrice

                };
                if (item.ItemDetails != null)
                {
                    lineItem.ItemDetails = item.ItemDetails.Select(line => new OrderLine
                    {
                        Id = line.Id,
                        Active = line.Active,
                        BIDS = line.BIDS,
                        BIDSNO = line.BIDSNO,
                        CampaignApplied = line.CampaignApplied,
                        CampaignId = line.CampaignId,
                        Direction = line.Direction,
                        DiscountDescription = line.DiscountDescription,
                        DiscountedUnitPrice = line.DiscountedUnitPrice,
                        DiscountPercentage = line.DiscountPercentage,
                        DiscountType = line.DiscountType,
                        GroupId = line.GroupId,
                        IsCoupon = line.IsCoupon,
                        ItemComments = line.ItemComments,
                        ItemDiscount = line.ItemDiscount,
                        ItemId = line.ItemId,
                        ItemIdex = line.ItemIdex,
                        ItemStatus = line.ItemStatus,
                        ItemType = line.ItemType,
                        OrderType = line.OrderType,
                        Percentage = line.Percentage,
                        PrinterId = line.PrinterId,
                        Product = new Product
                        {
                            Id = line.Product.Id,
                            Description = line.Product.Description,
                            Price = line.Product.Price
                        },
                        PurchasePrice = line.PurchasePrice,
                        Quantity = line.Quantity,
                        ReceiptItems = line.ReceiptItems,
                        TaxPercent = line.TaxPercent,
                        UnitPrice = line.UnitPrice,
                        GroupKey = line.GroupKey,
                        IngredientMode = line.IngredientMode

                    }).ToList();
                }
                if (item.IngredientItems != null)
                {
                    lineItem.IngredientItems = item.IngredientItems.Select(line => new OrderLine
                    {
                        Id = line.Id,
                        Active = line.Active,
                        BIDS = line.BIDS,
                        BIDSNO = line.BIDSNO,
                        CampaignApplied = line.CampaignApplied,
                        CampaignId = line.CampaignId,
                        Direction = line.Direction,
                        DiscountDescription = line.DiscountDescription,
                        DiscountedUnitPrice = line.DiscountedUnitPrice,
                        DiscountPercentage = line.DiscountPercentage,
                        DiscountType = line.DiscountType,
                        GroupId = line.GroupId,
                        IsCoupon = line.IsCoupon,
                        ItemComments = line.ItemComments,
                        ItemDiscount = line.ItemDiscount,
                        ItemId = line.ItemId,
                        ItemIdex = line.ItemIdex,
                        ItemStatus = line.ItemStatus,
                        ItemType = line.ItemType,
                        OrderType = line.OrderType,
                        Percentage = line.Percentage,
                        PrinterId = line.PrinterId,
                        Product = new Product
                        {
                            Id = line.Product.Id,
                            Description = line.Product.Description,
                            Price = line.Product.Price
                        },
                        PurchasePrice = line.PurchasePrice,
                        Quantity = line.Quantity,
                        ReceiptItems = line.ReceiptItems,
                        TaxPercent = line.TaxPercent,
                        UnitPrice = line.UnitPrice,
                        IngredientMode = line.IngredientMode,
                        GroupKey = line.GroupKey

                    }).ToList();
                }
                lines.Add(lineItem);

            }
            return lines;
        }

    }
}