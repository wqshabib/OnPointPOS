using System;
using System.Collections.Generic;
using System.Threading;
using POSSUM.Base;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Views.CheckOut;
using System.Linq;

namespace POSSUM.Presenters.ReturnOrder
{
    public class ReturnOrderPresenter : OrderPresenter
    {
        ApplicationDbContext db;

        private List<OrderLine> _returnList = new List<OrderLine>();
        private readonly IReturnOrderView _view;

        private Guid _orderId;
        private bool _returnOrder;
        private decimal _returnOrderTotal;

        public ReturnOrderPresenter(IReturnOrderView view)
        {
            _view = view;
            db = PosState.GetInstance().Context;
        }

        public void HandelCheckoutOrderClick(decimal orderTotal)
        {
            OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
            var orderDetails = _view.GetReturnOrderDetail();
            var order = _view.GetCurrentOrder();
            _returnOrder = true;
            bool res = false;
            bool statusComplete = false;
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    res = SaveOrderMaster(orderDetails);
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();
                        if (res)
                        {
                            var obj = new CheckOutOrderWindow(_returnOrderTotal, _orderId, -1, _returnList, order.TableId);

                            statusComplete = obj.ShowDialog() ?? false;
                            if (statusComplete)
                            {
                                res = true;
                                // var invoiceWindow = new PrintNewInvoiceWindow();
                                DirectPrint directPrint = new DirectPrint();
                                if (_returnOrder)
                                    directPrint.PrintReceipt(_orderId, false, obj.ReceiptGenerated);
                                // invoiceWindow.PrintReturnInvoice(orderId);
                                else
                                {
                                    directPrint.PrintReceipt(_orderId, false, obj.ReceiptGenerated);
                                    // invoiceWindow.PrintInvoice(orderId, false);
                                }


                            }
                        }

                    }));

                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            if (res)
                _view.CalculatLeftTotal(new List<OrderLine>());
            _view.OrderSaveCompleted(true);
            if (statusComplete)
                App.MainWindow.UpdateOrderCompleted(_orderId);




            //var orderDetails = _view.GetReturnOrderDetail();
            //var order = _view.GetCurrentOrder();
            //_returnOrder = true;
            //bool res = false;
            //bool statusComplete = false;
            //var progressDialog = new ProgressWindow();
            //var backgroundThread = new Thread(() =>
            //{
            //    res = SaveOrderMaster(orderDetails);
            //    progressDialog.Closed += (arg, ev) => { progressDialog = null; };
            //    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        progressDialog.Close();
            //        if (res)
            //        {
            //            var obj = new CheckOutOrderWindow(_returnOrderTotal, _orderId, -1, _returnList, order.TableId);
            //            statusComplete = obj.ShowDialog() ?? false;
            //            if (statusComplete)
            //            {
            //                res = true;

            //                var directPrint = new DirectPrint();
            //                if (_returnOrder)
            //                {
            //                    directPrint.PrintReceipt(_orderId, false, obj.ReceiptGenerated);
            //                }

            //                else
            //                {
            //                    directPrint.PrintReceipt(_orderId, false, obj.ReceiptGenerated);
            //                }
            //            }
            //        }
            //    }));
            //});
            //backgroundThread.Start();
            //progressDialog.ShowDialog();
            //backgroundThread.Join();
            //if (res)
            //    _view.CalculatLeftTotal(new List<OrderLine>());
            //_view.OrderSaveCompleted(true);
            //if (statusComplete)
            //    App.MainWindow.UpdateOrderCompleted(_orderId);
        }

        public bool SaveOrderMaster(List<OrderLine> orderDetail)
        {
            var originalOrder = _view.GetCurrentOrder();
            foreach (var detail in orderDetail)
            {
                detail.Direction = -1;
                detail.ItemStatus =(int) OrderStatus.ReturnOrder;
               detail.Quantity= detail.Quantity;
            }
            int tableId = originalOrder.TableId;
            var comment = originalOrder.Comments;
            var type = OrderType.Return;

            CalculatReturnTotal(orderDetail);// add total in _returnOrderTotal
            
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.Now,
                UserId = Defaults.User.Id,
                OutletId = Defaults.Outlet.Id,
                TerminalId = Defaults.Terminal.Id,
                TableId = tableId,
                OrderTotal = _returnOrderTotal,
                Status = OrderStatus.ReturnOrder,
                //  Synced = false,
                PaymentStatus = 2,
                Comments = comment,
                OrderComments = "",
                Type = type

            };

            try
            {
                _orderId = order.Id;
                _returnList = new OrderRepository(PosState.GetInstance().Context).SaveOrderReturnOrder(order, orderDetail);
               

                return true;


            }
            catch (Exception exp)
            {

                LogWriter.LogWrite(exp);
                return false;
            }


            //var originalOrder = _view.GetCurrentOrder();
            //int tableId = originalOrder.TableId;
            //var comment = originalOrder.Comments;
            //var type = OrderType.Return;
            //decimal orderTotal = _view.GetReturnOrderTotal();
            //if (Defaults.Terminal == null || Defaults.Outlet == null)
            //{
            //    Defaults.Init();
            //}

            //if (Defaults.Terminal != null)
            //{
            //    var order = new Order
            //    {
            //        Id = Guid.NewGuid(),
            //        CreationDate = DateTime.Now,
            //        UserId = Defaults.User.Id,
            //        OutletId = Defaults.Outlet.Id,
            //        TerminalId = Defaults.Terminal.Id,
            //        TableId = tableId,
            //        OrderTotal = orderTotal,
            //        Status = OrderStatus.ReturnOrder,
            //        ShiftNo = App.MainWindow.ShiftNo,
            //        Updated = 1,
            //        PaymentStatus = 2,
            //        Comments = comment,
            //        OrderComments = "",
            //        Type = type
            //    };
            //    _orderId = order.Id;
            //    try
            //    {
            //        order.CreationDate = DateTime.Now;
            //        _returnList = new OrderRepository(db).SaveOrderReturnOrder(order, orderDetail);
            //        _returnOrderTotal = orderTotal;
            //        return true;

            //    }
            //    catch (Exception exp)
            //    {
            //        LogWriter.LogWrite(exp);
            //        return false;
            //    }
            // }

            return false;
        }

        public void CalculatReturnTotal(IList<OrderLine> list)
        {
            _returnOrderTotal = 0;
            decimal Tax = 0;
            foreach (var lineItem in list)
            {
                if (lineItem.IsValid)
                {
                    _returnOrderTotal += lineItem.GrossAmount();
                    _returnOrderTotal = _returnOrderTotal + lineItem.ItemDiscount;

                    if (lineItem.IngredientItems != null && lineItem.IngredientItems.Count>0)
                        foreach (var ingredient in lineItem.IngredientItems)
                        {
                            Tax += ingredient.VatAmount();
                            _returnOrderTotal -= ingredient.GrossAmountDiscounted();
                        }

                }
            }
        }

        internal void HandelOrderDetailClick()
        {
            _orderId = _view.GetOrderId();
            var orderLines = new List<OrderLine>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(() =>
            {
                var orderRepository = new OrderRepository(PosState.GetInstance().Context);
                orderLines = orderRepository.GetOrderLinesById(_orderId);
                progressDialog.Closed += (arg, e) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
            });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            _view.SetOrderDetailResult(orderLines);

            //_orderId = _view.GetOrderId();
            //var orderLines = new List<OrderLine>();
            //var progressDialog = new ProgressWindow();
            //var backgroundThread = new Thread(() =>
            //{
            //    var orderRepository = new OrderRepository(db);
            //    orderLines = orderRepository.GetOrderLinesById(_orderId);
            //    progressDialog.Closed += (arg, e) => { progressDialog = null; };
            //    progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
            //});
            //backgroundThread.Start();
            //progressDialog.ShowDialog();
            //backgroundThread.Join();
            //_view.SetOrderDetailResult(orderLines);
        }
    }
}