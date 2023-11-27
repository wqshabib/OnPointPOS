using System;
using System.Collections.Generic;
using System.Linq;
using POSSUM.Base;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Views.PrintOrder;
using POSSUM.Handlers;
using POSSUM.Presenters.OrderHistory;
using POSSUM.Data.Repositories;
using POSSUM.Views.CheckOut;

namespace POSSUM.Presenters.Bookings
{
    public class BookingListPresenter// : OrderPresenter
    {
        private readonly IBookingListView _view;

         
        public BookingListPresenter(IBookingListView view)
        {
            _view = view;
            // db = PosState.GetInstance().Context;
        }

        public void RetryOrder(Order order)
        {
            try
            {
                new InvoiceHandler().RetryOrder(order);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
        }

        public List<OrderLine> HandleItemSaleSearchClick()
        {
            DateTime startDate = _view.GetStartdate();
            DateTime endDate = _view.GetEnddate();

            string queryText = _view.GetQueryText();
            List<OrderLine> orders = new List<OrderLine>();

            try
            {
                orders = new OrderRepository(PosState.GetInstance().Context).GetSaleHistory(queryText, startDate, endDate);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
            return orders;
        }

        internal void HandleCustomerInvoiceSearchClick()
        {

            DateTime startDate = _view.GetStartdate();
            DateTime endDate = _view.GetEnddate();


            string queryText = _view.GetQueryText();
            try
            {
                //var orders = new CustomerInvoiceRepository().GetCustomerInvoices(queryText, startDate, endDate);
                //decimal total = orders.Sum(o => o.OrderTotal);
                var bookings = new BookingRepository(new ApplicationDbContext()).LoadBookings();
                _view.SetResult(bookings);
                _view.SetTotalAmount(0);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
        }

        internal void HandleCheckOutClick(int grossTotal, Guid bookingGuid, int orderDirection)
        {
            var obj = new CheckOutOrderWindow(grossTotal, bookingGuid, orderDirection, null,
                            1);
            var statusComplete = obj.ShowDialog() ?? false;
        }

        internal void HandleSearchClick()
        {

            DateTime startDate = _view.GetStartdate();
            DateTime endDate = _view.GetEnddate();


            string queryText = _view.GetQueryText();
            try
            {
                //List<Order> orders = new OrderRepository(PosState.GetInstance().Context).SearchGeneralOrder(queryText, startDate, endDate);
                //decimal total = orders.Sum(o => o.OrderTotal);
                var bookings = new BookingRepository(new ApplicationDbContext()).LoadBookings();

                _view.SetResult(bookings);
                _view.SetTotalAmount(0);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
        }

        internal void HandleSearchClickCCFailed()
        {

            DateTime startDate = _view.GetStartdateCCFailed();
            DateTime endDate = _view.GetEnddateCCFailed();


            string queryText = _view.GetQueryTextCCFailed();
            try
            {
                //List<Order> orders = new OrderRepository(PosState.GetInstance().Context).SearchGeneralOrderCCFailed(queryText, startDate, endDate);
                //decimal total = orders.Sum(o => o.OrderTotal);
                var bookings = new BookingRepository(new ApplicationDbContext()).LoadBookings();

                _view.SetResultCCFailed(bookings);
                _view.SetTotalAmountCCFailed(0);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
        }

        internal List<Order> HandleCustomerPendingOrderSearchClick(Guid customerId, DateTime startDate, DateTime endDate)
        {

            try
            {
                return new CustomerInvoiceRepository().GetCustomerPendingOrders(customerId, startDate, endDate);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
                return null;
            }

        }

        internal void HandleCustomerOrderSearchClick()
        {
            DateTime startDate = _view.GetStartdate();
            DateTime endDate = _view.GetEnddate();
            string queryText = _view.GetQueryText();
            try
            {
                //var orders = new CustomerInvoiceRepository().GetCustomerCompletedOrder(queryText, startDate, endDate);
                //decimal total = orders.Sum(o => o.OrderTotal);
                var bookings = new BookingRepository(new ApplicationDbContext()).LoadBookings();

                _view.SetResult(bookings);
                _view.SetTotalAmount(0);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
        }

        internal void HandlePrintClick(Order orderMaster)
        {
            try
            {
                if (orderMaster.InvoiceGenerated == 0)
                {
                    var billWindow = new PrintBillWindow(orderMaster.Id);
                    billWindow.ShowDialog();
                }
                else
                {
                    var billWindow = new PrintInvoiceWindow(orderMaster.Id, true);
                    billWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", ex.Message);
            }
        }
    }
}
