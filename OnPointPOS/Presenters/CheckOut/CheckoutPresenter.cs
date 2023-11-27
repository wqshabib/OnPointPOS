using System;
using System.Linq;
using POSSUM.Data;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Handlers;
using POSSUM.Res;
using System.Windows;

namespace POSSUM.Presenters.CheckOut
{
    public class CheckoutPresenter
    {
        private readonly ICheckoutView _view;

        ApplicationDbContext db;
        public CheckoutPresenter(ICheckoutView checkOutOrderWindow)
        {
            _view = checkOutOrderWindow;
            OrderId = _view.GetOrderId();

            db = PosState.GetInstance().Context;
       
        }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }

       

        public decimal OrderTotal()
        {
            Guid id = _view.GetOrderId();
           
            var lines = new OrderRepository(db).GetOrderLinesById(id);
            var order = new Order
            {
                Id = id,
                OrderLines = lines
            };
            return order.GrossAmount;
        }

        internal Customer GetOrderCutomer()
        {
            Guid id = _view.GetOrderId();

            using (var db = PosState.GetInstance().Context)
            {
               
                var order = db.OrderMaster.FirstOrDefault(o=>o.Id==id);
                if (order.CustomerId != null || order.CustomerId != default(Guid))
                {
                   
                    return db.Customer.FirstOrDefault(c=>c.Id== order.CustomerId);
                }
                else
                    return null;
            }
        }

        public object GetTotalPaid()
        {
            // DBAccess.GetSingleValueByQuery("Select SUM(Isnull(PaidAmount,0))-(SUM(Isnull(ReturnAmount,0))+SUM(Isnull(cashchange,0))+SUM(Isnull(tipAmount,0))) as Bal from payment Where OrderId=" + orderId);
            Guid orderId = _view.GetOrderId();
            decimal total = 0;
            using (var db = new ApplicationDbContext())
            {

                var payments = db.Payment.Where(o => o.OrderId == orderId).ToList();
                if (payments.Count > 0)
                {
                    var result = payments.Sum(p => p.PaidAmount);
                    total = result;
                }
            }
            return total;
        }
        public decimal CancelOrderDiscount(Guid orderId)
        {
            return new OrderRepository(db).CancelOrderDiscount(OrderId);
        }

        internal void SendPrintToKitchenWithoutReset(Order masterOrder)
        {
            new DirectPrint().PrintBong(masterOrder, true);

        }
        public Order GetOrderMasterDetailById(Guid id)
        {
            return new OrderRepository(PosState.GetInstance().Context).GetOrderMasterDetailById(id);
        }

        internal void OpenCashDrawer(decimal amount)
        {
            try
            {    //Save Cashdrawer Log
                new CashDrawerRepository().OpenCashDrawer(Defaults.Terminal.Id, amount, Defaults.User.Id);
                // Kick drawer
                if (Defaults.CashDrawerType == CashDrawerType.PRINTER)
                {
                    var directPrint = new DirectPrint();
                    directPrint.OpenCashDrawer();
                }
                else
                {
                    PosState.GetInstance().CashDrawer.Open();
                }
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError("Error", "No cashdrawer defined.");
            }
        }

        public bool SavePayment(Payment payment, decimal roundAmount = 0)
        {
            //MessageBox.Show("");
           

            return new InvoiceHandler().SaveOrderPayment(payment, roundAmount);
        }

       

        internal void SaveCashDrawLog(CashDrawerLog cashDrawLog)
        {
            new InvoiceHandler().SaveCashDrawLog(cashDrawLog);
        }

        public bool GenerateInvoiceLocal(Guid id, int shifNo, string userId,
            PaymentTransactionStatus creditcardPaymentResult)
        {
            return new InvoiceHandler().GenerateInvoiceLocal(id, shifNo, userId, creditcardPaymentResult);
        }

        public bool UpdateOrderCustomerInfo(Guid id, Customer customer, string orderComments = "")
        {
           return new OrderRepository(db).UpdateOrderCustomerInfo(id, customer, orderComments);
        }
        public decimal UpdateOrderDiscount(Guid orderId, decimal amount, bool percentage)
        {
            return new OrderRepository(db).UpdateOrderDiscount(orderId, amount, percentage,Defaults.User.Id);
        }

        public decimal GetOrderTotal(Guid orderId)
        {
            return new OrderRepository(db).GetOrderTotal(orderId);
        }

        public bool HasDiscountAvailable(Guid orderId)
        {
            return new OrderRepository(db).HasDiscountAvailable(orderId);
        }

        public Customer GetOrderCustomer()
        {
            Guid id = _view.GetOrderId();

            return new CustomerRepository().GetCustomerById(id);
        }

        internal void CancelOrder(Guid orderId, string id)
        {
            bool isCancelled = new OrderRepository(db).CancelOrder(OrderId, Defaults.User.Id);
        }


        //public bool AddPayment(int orderDirection, decimal swishAmount, decimal couponAmount,  decimal creditCardAmount, decimal debitCardAmount, decimal cashAmount, decimal totalBillAmount, decimal remainigAmount, decimal paidCash, decimal returnAmount, decimal cashBackAmount, decimal mobileTerminalAmount, decimal studentCardAmount, decimal creditNoteAmount, decimal beamAmount, PaymentTransactionStatus creditcardPaymentResult,decimal roundamount,int currentPaymentType)
        //{
        //    bool res = false;

        //    Payment dtoPayment;


        //    if (swishAmount > 0)
        //    {

        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = swishAmount - returnAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 10,
        //            CashCollected = swishAmount,
        //            CashChange = returnAmount,
        //            ReturnAmount = returnAmount,
        //            PaymentRef = "Swish",// UI.CheckOutOrder_Method_FreeCoupon,// "Gratiskupong",
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };

        //        res= SavePayment(dtoPayment);

        //    }
        //    if (studentCardAmount > 0)
        //    {

        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = studentCardAmount - returnAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 11,
        //            CashCollected = studentCardAmount,
        //            CashChange = returnAmount,
        //            ReturnAmount = returnAmount,
        //            PaymentRef = "Studentkort",// UI.CheckOutOrder_Method_FreeCoupon,// "Gratiskupong",
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };
        //        if (cashBackAmount == 0)
        //            cashBackAmount = returnAmount;
        //        res = SavePayment(dtoPayment);

        //    }
        //    if (mobileTerminalAmount > 0)
        //    {

        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = mobileTerminalAmount - returnAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 9,
        //            CashCollected = mobileTerminalAmount,
        //            CashChange = returnAmount,
        //            ReturnAmount = returnAmount,
        //            PaymentRef = UI.CheckOutOrder_Method_Mobile,
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };

        //        res = SavePayment(dtoPayment);

        //    }
        //    if (beamAmount > 0)
        //    {

        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = beamAmount - returnAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 13,
        //            CashCollected = beamAmount,
        //            CashChange = returnAmount,
        //            ReturnAmount = returnAmount,
        //            PaymentRef = UI.CheckOutOrder_Method_Beam,
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };

        //        res = SavePayment(dtoPayment);

        //    }
        //    if (couponAmount > 0)
        //    {
        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = couponAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 3,
        //            CashCollected = couponAmount,
        //            CashChange = 0,
        //            ReturnAmount = 0,
        //            PaymentRef = UI.CheckOutOrder_Method_GiftCard,// "Presentkort",
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };
        //        res = SavePayment(dtoPayment);


        //    }
        //    if (creditNoteAmount > 0)
        //    {

        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = creditNoteAmount - returnAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 12,
        //            CashCollected = creditNoteAmount,
        //            CashChange = returnAmount,
        //            ReturnAmount = returnAmount,
        //            PaymentRef = UI.CheckOutOrder_Method_CreditNote,// "Presentkort",
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };
        //        res = SavePayment(dtoPayment);


        //    }           
        //    if (creditCardAmount > 0)
        //    {

        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = creditCardAmount - returnAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = (!string.IsNullOrEmpty(creditcardPaymentResult.ProductName) && creditcardPaymentResult.ProductName == "American Express") ? 14 : 4,
        //            CashCollected = creditCardAmount,
        //            CashChange = returnAmount > 0 ? returnAmount : 0,
        //            ReturnAmount = returnAmount,
        //            PaymentRef = UI.CheckOutOrder_Method_CreditCard,// "Kort",
        //            ProductName = creditcardPaymentResult.ProductName,
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };
        //        res = SavePayment(dtoPayment);

        //    }
        //    if (debitCardAmount > 0)
        //    {
        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = debitCardAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 5,
        //            CashCollected = debitCardAmount,
        //            CashChange = 0,
        //            ReturnAmount = 0,
        //            PaymentRef = UI.CheckOutOrder_Method_DebitCard,// "Paid By Debit Card",
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };
        //        res = SavePayment(dtoPayment);

        //    }
        //    if (cashAmount > 0)
        //    {
        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = cashAmount - returnAmount,
        //            PaymentDate = DateTime.Now,
        //            TypeId = 1,
        //            CashCollected = cashAmount,
        //            CashChange = returnAmount,
        //            ReturnAmount = returnAmount,// if given extra amount in cash
        //            PaymentRef = UI.CheckOutOrder_Method_Cash,// "Kontant",
        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };
        //        res = SavePayment(dtoPayment, roundamount);

        //    }
        //    if (cashBackAmount > 0)
        //    {
        //        dtoPayment = new Payment
        //        {
        //            OrderId = OrderId,
        //            PaidAmount = 0,
        //            PaymentDate = DateTime.Now,
        //            TypeId = (orderDirection == -1 && currentPaymentType != 1) ? currentPaymentType : 7,
        //            CashCollected = 0,
        //            CashChange = cashBackAmount,
        //            ReturnAmount = 0,
        //            PaymentRef = UI.CheckOutOrder_Label_CashBack,

        //            TipAmount = 0,
        //            Direction = orderDirection
        //        };
        //        res = SavePayment(dtoPayment);
        //    }


        //    Defaults.PerformanceLog.Add("Checkout completed...         -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
        //    return res;
        //}


    }
}