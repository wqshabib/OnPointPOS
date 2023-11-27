using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Printing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Pdf;
using POSSUM.Presenters.CheckOut;
using POSSUM.Handlers;
using POSSUM.Res;
using POSSUM.Utils;
using POSSUM.Utils.nu.kontantkort.extdev;
using POSSUM.Views.Customers;
using POSSUM.Views.PrintOrder;
using POSSUM.Views.Sales;
using POSSUM.Data;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using POSSUM.Presenters.Sales;
using System.Configuration;
using System.Diagnostics;
using POSSUM.Presenters.Customers;

namespace POSSUM.Views.CheckOut
{
    public partial class CheckOutOrderWindow : Window, ICheckoutView
    {
        //public SaleOrderPresenter Presenter { get; set; }
        CheckoutPresenter presenter;
        private readonly CustomerPresenter _customerPresenter;
        Guid OrderId = default(Guid);
        CheckOutModel orderVal = new CheckOutModel();
        decimal swishAmount = 0;
        decimal accountAmount = 0;
        decimal couponAmount = 0;// Gift card
        decimal creditNoteAmount = 0;
        decimal BeamAmount = 0;
        decimal creditCardAmount = 0;
        decimal DebitCardAmount = 0;
        decimal mobilecardAmount = 0;
        decimal tipAmount = 0;
        decimal studentcardAmount = 0;// ELV card
        decimal vatAmount = 0;
        decimal depositAmount = 0;

        public bool ReceiptGenerated = false;
        Customer selectedCustomer = null;
        List<OrderLine> currentOrderDetail;
        int currentPaymentType = 1;
        CustomerViewCart customerViewCart;
        //  ApplicationDbContext db;
        public CheckOutOrderWindow()
        {
            InitializeComponent();

            // db = PosState.GetInstance().Context;
        }
        

        /**
         * Constructor, called from pending orders
         **/
        public CheckOutOrderWindow(Guid orderId)
        {
            InitializeComponent();
            OrderId = orderId;

            //db = PosState.GetInstance().Context;

            presenter = new CheckoutPresenter(this);




            object totalval = presenter.OrderTotal();
            selectedCustomer = presenter.GetOrderCutomer();
            decimal totalOrderAmount = totalval != DBNull.Value ? Convert.ToDecimal(totalval) : 0;
            orderVal.TotalBillAmount = presenter.OrderTotal();

            object val = presenter.GetTotalPaid();
            decimal paid = val != DBNull.Value ? Convert.ToDecimal(val) : 0;
            init(totalOrderAmount, paid);
        }

        /**
         * Constructor, called from place order
         **/
        int orderDirection = 1;
        int TabelId = 0;
        Order orderMaster = new Order();
        public CheckOutOrderWindow(Order order) //munir
        {
            orderMaster = order;

            InitializeComponent();
            currentOrderDetail = order.OrderLines.ToList();// orderDetail;
            // totalOrderAmount = CalculatOrderTotal(orderDetail);// orderDetail.Sum(s => s.GrossAmountDiscounted());
            OrderId = order.Id;// orderId;
            TabelId = order.TableId;// tableId;
            orderDirection = (int)order.Type;// _direction;


            orderVal.TotalBillAmount = order.OrderTotal < 0 ? (orderDirection) * order.OrderTotal : order.OrderTotal;
            presenter = new CheckoutPresenter(this);
            selectedCustomer = presenter.GetOrderCutomer();
            var pp = presenter.GetTotalPaid();
            decimal paid = 0;
            if (pp != null)
                paid = Convert.ToDecimal(pp);
            _payable = orderVal.TotalBillAmount - paid;
            init(orderVal.TotalBillAmount, paid);
            if (orderDirection == -1)
                lblTotalBill.Dispatcher.BeginInvoke(new Action(() => lblTotalBill.Text = UI.CheckOutOrder_Label_TotalReturnBill));


        }
        private string SourceWindow;
        public CheckOutOrderWindow(decimal totalOrderAmount, Guid orderId, int _direction, List<OrderLine> orderDetail, int tableId, string sourceWindow = null)
        {
            InitializeComponent();
            _customerPresenter = new CustomerPresenter();

            SourceWindow = sourceWindow;
            currentOrderDetail = orderDetail;
            // totalOrderAmount = CalculatOrderTotal(orderDetail);// orderDetail.Sum(s => s.GrossAmountDiscounted());
            OrderId = orderId;
            TabelId = tableId;
            orderDirection = _direction;
            //OrderStatus st = presenter.Order.Status;
            orderVal.TotalBillAmount = totalOrderAmount < 0 ? (_direction) * totalOrderAmount : totalOrderAmount;
            presenter = new CheckoutPresenter(this);
            selectedCustomer = presenter.GetOrderCutomer();
            var pp = presenter.GetTotalPaid();
            decimal paid = 0;
            if (pp != null)
                paid = Convert.ToDecimal(pp);
            _payable = orderVal.TotalBillAmount - paid;
            init(orderVal.TotalBillAmount, paid);
            if (_direction == -1)
                lblTotalBill.Dispatcher.BeginInvoke(new Action(() => lblTotalBill.Text = UI.CheckOutOrder_Label_TotalReturnBill));


        }
        public decimal CalculatOrderTotal(List<OrderLine> lst)
        {

            decimal OrderTotal = 0;
            decimal tax = 0;
            decimal discount = 0;
            foreach (var item in lst)
            {
                if (item.IsValid)
                {
                    if (item.Product.ItemType == ItemType.Individual)
                        tax += item.VatAmount();
                    OrderTotal += item.GrossAmountDiscounted(); //MasterOrder.Type==OrderType.Return?item.ReturnGrossAmountDiscounted():
                    discount += item.ItemDiscount;
                    if (item.IngredientItems != null)
                        foreach (var ingredient in item.IngredientItems)
                        {
                            tax += ingredient.VatAmount();
                            discount += item.ItemDiscount;
                            OrderTotal += ingredient.GrossAmountDiscounted(); //MasterOrder.Type == OrderType.Return ?ingredient.ReturnGrossAmountDiscounted():
                        }
                }
            }
            foreach (var detail in lst.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
            {
                // var vatInnerGroups = detail.OrderItemDetails.GroupBy(od => od.TAX);
                foreach (var itm in detail.ItemDetails)
                {

                    decimal vatAmount = itm.VatAmount();
                    tax += vatAmount;
                }
            }
            decimal nettotal = OrderTotal - tax;
            return OrderTotal;
        }


        /**
         * Shared common constructor init
         **/
        private void init(decimal totalOrderAmount, decimal paid)
        {
            DecimalFormatButton.Content = ((CultureInfo)Defaults.UICultureInfo).NumberFormat.CurrencyDecimalSeparator;
            orderVal.ReceivedPayments = paid;

            decimal payable = totalOrderAmount - paid;
            _payable = (decimal)Math.Round(payable, 2);
            orderVal.RemainingAmount = payable;//AssignValueToTextBox (Math.Round(payable));
            orderVal.TotalBalanceAmount = payable > 0 ? payable : orderVal.TotalBillAmount;
            if (orderDirection == -1)
                orderVal.CashBackAmount = payable;
            OrderGrid.DataContext = orderVal;
            ShowNewPaymentButton();

        }

        private void MainWindow_InsertedAmount(object sender, int amount)
        {
            SetInsertedAmount(amount);
        }



        private void SetInsertedAmount(int amount)
        {

            decimal tryDec;
            currentPaymentType = 1;


            long intPart = (long)orderVal.RemainingAmount;
            decimal fracPart = orderVal.RemainingAmount - intPart;
            if (fracPart < (decimal)0.50)
                _roundamount = (-1) * fracPart;
            else
            {
                _roundamount = Convert.ToDecimal(1) - fracPart;
            }

            orderVal.PaidCashAmount = amount;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                txtReceivedCash.Text = orderVal.PaidCashAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);


                OrderGrid.DataContext = orderVal;
                CalculateRemaining_Returned_CashBack();

                txtPaymentAmount.Text = "";
                txtPaymentAmount.Focus();
            }));
        }
        private void ShowNewPaymentButton()
        {
            string swishName = "Swish";
            string elevkortName = "elevkort";
            var swish = Defaults.PaymentTypes.FirstOrDefault(pt => pt.Id == 10);
            var elevkort = Defaults.PaymentTypes.FirstOrDefault(pt => pt.Id == 11);
            if (Defaults.Language == POSSUM.CurrentLanguage.Swedish)
            {
                if (swish != null)
                    swishName = swish.SwedishName;
                if (elevkort != null)
                    elevkortName = elevkort.SwedishName;
            }
            else
            {
                if (swish != null)
                    swishName = swish.Name;
                if (elevkort != null)
                    elevkortName = elevkort.Name;
            }
            btnSwish.Dispatcher.BeginInvoke(new Action(() => btnSwish.Content = swishName));
            btnElevkort.Dispatcher.BeginInvoke(new Action(() => btnElevkort.Content = elevkortName));

            if (Defaults.ElveCard)
            {
                btnElevkort.Visibility = Visibility.Visible;
                borderStudentCardAmount.Visibility = Visibility.Visible;
            }
            else
            {
                btnElevkort.Visibility = Visibility.Collapsed;
                borderStudentCardAmount.Visibility = Visibility.Collapsed;
            }

            if (Defaults.CreditNote)
            {
                btnCreditNote.Visibility = Visibility.Visible;
                borderCreditNote.Visibility = Visibility.Visible;
            }
            else
            {
                btnCreditNote.Visibility = Visibility.Collapsed;
                borderCreditNote.Visibility = Visibility.Collapsed;
            }

            if (Defaults.BeamPayment)
            {
                btnBeam.Visibility = Visibility.Visible;
                borderBeam.Visibility = Visibility.Visible;
            }
            else
            {
                btnBeam.Visibility = Visibility.Collapsed;
                borderBeam.Visibility = Visibility.Collapsed;
            }
            if (Defaults.OnlineCash)
            {
                btnOnlineCash.Visibility = Visibility.Visible;
                txtOnlineCash.Visibility = Visibility.Visible;
            }
            else
            {
                btnOnlineCash.Visibility = Visibility.Collapsed;
                txtOnlineCash.Visibility = Visibility.Collapsed;
            }
            if (Defaults.TipStatus)
            {
                btnTip.Visibility = Visibility.Visible;
                txtTip.Visibility = Visibility.Visible;

            }
            else
            {
                btnTip.Visibility = Visibility.Collapsed;
                txtTip.Visibility = Visibility.Collapsed;
            }
            if (Defaults.Deposit)
            {
                btnDeposit.Visibility = Visibility.Visible;
            }
            else
            {
                btnDeposit.Visibility = Visibility.Collapsed;
            }


        }

        private void BtnCloseCheckout_OnClick(object sender, RoutedEventArgs e)
        {
            App.MainWindow.customerViewCart.UnloadSwishQRCode();
            CancelDiscount();
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.PaymentScreenCancelled), OrderId);
            //if (Defaults.CASH_GUARD)
            //{

            //    if (PosState.GetInstance().CashGuard.Status == CashGuardStatus.Idle)
            //    {
            //        int amount = PosState.GetInstance().CashGuard.InsertedAmount;
            //        if (amount > 0)
            //        {
            //            PosState.GetInstance().CashGuard.DispenseAmount(amount);
            //        }
            //    }
            //}
            presenter.CancelOrder(OrderId, Defaults.User.Id);
            this.Close();
        }
        decimal _payable = 0;
        decimal _roundamount = 0;
        // int _orderDirection = 1;

        private void BtnCash_Click(object sender, RoutedEventArgs e)
        {

            decimal tryDec;
            currentPaymentType = 1;
            if (string.IsNullOrEmpty(txtPaymentAmount.Text) || !Decimal.TryParse(txtPaymentAmount.Text, out tryDec))
            {
                orderVal.PaidCashAmount = orderVal.PaidCashAmount + orderVal.RemainingAmount;


                orderVal.RemainingAmount = orderVal.RemainingAmount * orderDirection;

                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }
                orderVal.RoundedAmount = _roundamount;
                txtReceivedCash.Text = Math.Round(orderVal.PaidCashAmount)
                    .ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {

                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }
                orderVal.RoundedAmount = _roundamount;
                orderVal.PaidCashAmount = Convert.ToDecimal(txtPaymentAmount.Text, Defaults.UICultureInfo);
                txtReceivedCash.Text =
                    orderVal.PaidCashAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }



            OrderGrid.DataContext = orderVal;
            CalculateRemaining_Returned_CashBack();

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();


        }
        private void BtnMobileTerminal_Click(object sender, RoutedEventArgs e)
        {

            decimal tryDec;
            currentPaymentType = 9;
            if (string.IsNullOrEmpty(txtPaymentAmount.Text) || !Decimal.TryParse(txtPaymentAmount.Text, out tryDec))
            {
                //  orderVal.PaidCashAmount = orderVal.PaidCashAmount + orderVal.RemainingAmount;
                mobilecardAmount = orderVal.RemainingAmount;
                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }
                txtMobileTerminalAmount.Text = mobilecardAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }

                mobilecardAmount = Convert.ToDecimal(txtPaymentAmount.Text, Defaults.UICultureInfo);
                txtMobileTerminalAmount.Text = mobilecardAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }
            PaidOthersCalculation();
            OrderGrid.DataContext = orderVal;
            CalculateRemaining_Returned_CashBack();

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();


        }

        void ReturnAmount_Remaining()
        {
            OrderGrid.DataContext = null;
            //decimal paid = orderVal.PaidOthersAmount;

            decimal paidAmt = orderVal.PaidCashAmount + accountAmount + couponAmount + creditCardAmount + BeamAmount + DebitCardAmount + swishAmount + mobilecardAmount + studentcardAmount + creditNoteAmount + OnlineCashAmount + depositAmount;
            decimal BillAmt = orderVal.TotalBalanceAmount;// Convert.ToDecimal(lblTotalBillPayment.Text);
            if (BillAmt >= paidAmt)
            {
                orderVal.RemainingAmount = BillAmt - paidAmt;
            }
            if (paidAmt > BillAmt)
            {
                orderVal.ReturnedAmount = paidAmt - BillAmt;
                orderVal.RemainingAmount = 0;
            }
            else if (paidAmt == BillAmt)
            {
                orderVal.RemainingAmount = 0;
                orderVal.ReturnedAmount = 0;

            }


            OrderGrid.DataContext = orderVal;

        }
        private void CalculateRemaining_Returned_CashBack()
        {
            try
            {
                ReturnAmount_Remaining();
                //OrderGrid.DataContext = null;
                //decimal paid = orderVal.PaidOthersAmount;
                //decimal RunningBalance = orderVal.TotalBalanceAmount - (paid);

                ///*remaining amount*/
                //if (RunningBalance >= 0)
                //    orderVal.RemainingAmount = RunningBalance;
                //else
                //    orderVal.RemainingAmount = 0;
                ///*cash back amount*/
                ////if (paid > orderVal.TotalBalanceAmount)
                ////    orderVal.CashBackAmount = paid - orderVal.TotalBalanceAmount;
                ////else
                ////    orderVal.CashBackAmount = 0;

                ///*return amount*/
                //if (orderVal.PaidCashAmount > 0 && orderVal.RemainingAmount >= 0)
                //{
                //    if (orderVal.PaidCashAmount > orderVal.RemainingAmount)
                //    {
                //        orderVal.ReturnedAmount = Math.Round(orderVal.PaidCashAmount - orderVal.RemainingAmount);
                //        orderVal.RemainingAmount = 0;
                //    }
                //    else
                //    {
                //        decimal actualpaidAmount = 0;
                //        if (_roundamount < 0)
                //        {
                //            actualpaidAmount = orderVal.PaidCashAmount + (-1) * _roundamount;
                //        }
                //        else
                //            actualpaidAmount = orderVal.PaidCashAmount + _roundamount;
                //        orderVal.RemainingAmount = orderVal.RemainingAmount - actualpaidAmount < 0 ? 0 : orderVal.RemainingAmount - actualpaidAmount;

                //        orderVal.ReturnedAmount = 0;
                //    }

                //}

                //else if (creditCardAmount > orderVal.TotalBalanceAmount)
                //{
                //    orderVal.ReturnedAmount = Math.Round(creditCardAmount - orderVal.TotalBalanceAmount);

                //}
                //else if (mobilecardAmount > orderVal.TotalBalanceAmount)
                //{
                //    orderVal.ReturnedAmount = Math.Round(mobilecardAmount - orderVal.TotalBalanceAmount);

                //}
                //else if (studentcardAmount > orderVal.TotalBalanceAmount)
                //{
                //    orderVal.ReturnedAmount = Math.Round(studentcardAmount - orderVal.TotalBalanceAmount);
                //}
                //else if (swishAmount > orderVal.TotalBalanceAmount)
                //{
                //    orderVal.ReturnedAmount = Math.Round(swishAmount - orderVal.TotalBalanceAmount);

                //}
                //else if (creditNoteAmount > orderVal.TotalBalanceAmount)
                //{
                //    orderVal.ReturnedAmount = Math.Round(creditNoteAmount - orderVal.TotalBalanceAmount);

                //}
                //else if (OnlineCashAmount > orderVal.TotalBalanceAmount)
                //{
                //    orderVal.ReturnedAmount = Math.Round(OnlineCashAmount - orderVal.TotalBalanceAmount);

                //}
                //else if (BeamAmount > orderVal.TotalBalanceAmount)
                //{
                //    orderVal.ReturnedAmount = Math.Round(BeamAmount - orderVal.TotalBalanceAmount);

                //}
                //else
                //    orderVal.ReturnedAmount = 0;

                //OrderGrid.DataContext = orderVal;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCreditNote_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPaymentAmount.Text))
            {
                creditNoteAmount = creditNoteAmount + orderVal.RemainingAmount;

                txtCreditNote.Text = creditNoteAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                creditNoteAmount = Convert.ToDecimal(txtPaymentAmount.Text);
                txtCreditNote.Text = creditNoteAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();

            PaidOthersCalculation();
            CalculateRemaining_Returned_CashBack();
        }

        private void BtnBeam_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPaymentAmount.Text))
            {
                BeamAmount = BeamAmount + orderVal.RemainingAmount;

                txtBeam.Text = BeamAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                BeamAmount = Convert.ToDecimal(txtPaymentAmount.Text);
                txtBeam.Text = BeamAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();

            PaidOthersCalculation();
            CalculateRemaining_Returned_CashBack();
        }

        decimal OnlineCashAmount = 0;
        private void BtnOnline_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtPaymentAmount.Text))
            //{
            //    OnlineCashAmount = OnlineCashAmount + orderVal.RemainingAmount;

            //    txtOnlineCash.Text = OnlineCashAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            //}
            //else
            //{
            //    OnlineCashAmount = Convert.ToDecimal(txtPaymentAmount.Text);
            //    txtOnlineCash.Text = OnlineCashAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            //}

            //txtPaymentAmount.Text = "";
            //txtPaymentAmount.Focus();

            //PaidOthersCalculation();
            //CalculateRemaining_Returned_CashBack();
        }
        private void BtnCoupon_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPaymentAmount.Text))
            {
                couponAmount = couponAmount + orderVal.RemainingAmount;

                txtCouponAmount.Text = couponAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                couponAmount = Convert.ToDecimal(txtPaymentAmount.Text);
                txtCouponAmount.Text = couponAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();

            PaidOthersCalculation();
            CalculateRemaining_Returned_CashBack();

        }

        decimal collectedCardAmount = 0;

        private void BtnCreditCard_Click(object sender, RoutedEventArgs e)
        {
            currentPaymentType = 4;
            decimal tryDec;

            if (string.IsNullOrEmpty(txtPaymentAmount.Text) || !Decimal.TryParse(txtPaymentAmount.Text, out tryDec))
            {
                creditCardAmount = creditCardAmount + orderVal.RemainingAmount;

                txtCreditCardAmount.Text = creditCardAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                creditCardAmount = Convert.ToDecimal(txtPaymentAmount.Text);
                txtCreditCardAmount.Text = creditCardAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                //following line of code done on 14/12/2015, the if user entry more amount than payable, then on receipt the cash back will be printed
                if (creditCardAmount > orderVal.RemainingAmount)
                    orderVal.CashBackAmount = creditCardAmount - orderVal.RemainingAmount;
            }
            PaidOthersCalculation();
            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();
            CalculateRemaining_Returned_CashBack();
        }

        private void BtnDebitCard_Click(object sender, RoutedEventArgs e)
        {

            txtDebitCardAmount.Text = string.IsNullOrEmpty(txtPaymentAmount.Text) ? lblRemaining.Text : txtPaymentAmount.Text;
            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();

            PaidOthersCalculation();
            CalculateRemaining_Returned_CashBack();
        }



        private void BtnTipAmount_Click(object sender, RoutedEventArgs e)
        {
            decimal tryDec;

            if (string.IsNullOrEmpty(txtPaymentAmount.Text) && orderVal.ReturnedAmount == 0)
            {

                MessageBox.Show(UI.Message_EntyerTipAmount, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtPaymentAmount.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtPaymentAmount.Text) || !Decimal.TryParse(txtPaymentAmount.Text, out tryDec))
            {
                tipAmount = orderVal.RemainingAmount + orderVal.ReturnedAmount;
                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }
                orderVal.RoundedAmount = _roundamount;
                txtTip.Text = tipAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }

                orderVal.RoundedAmount = _roundamount;
                tipAmount = Convert.ToDecimal(txtPaymentAmount.Text, Defaults.UICultureInfo);
                txtTip.Text = tipAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }
            PaidOthersCalculation();


            OrderGrid.DataContext = null;

            //if (string.IsNullOrEmpty(txtPaymentAmount.Text))
            //{
            //    MessageBox.Show(UI.Message_EntyerTipAmount, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    txtPaymentAmount.Focus();
            //    return;
            //}
            //txtTip.Text = txtPaymentAmount.Text;
            orderVal.TipAmount = tipAmount;

            orderVal.TotalBalanceAmount = tipAmount + orderVal.TotalBillAmount;
            orderVal.ReturnedAmount = tipAmount + orderVal.TotalBillAmount;
            OrderGrid.DataContext = orderVal;
            CalculateRemaining_Returned_CashBack();

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();
        }


        private void BtnSwish_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.customerViewCart.LoadSwishQRCode();

            decimal tryDec; 
            if (string.IsNullOrEmpty(txtPaymentAmount.Text) || !Decimal.TryParse(txtPaymentAmount.Text, out tryDec))
            {
                swishAmount = orderVal.RemainingAmount;

                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }
                orderVal.RoundedAmount = _roundamount;
                txtSwishAmount.Text = Math.Round(swishAmount).ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }
                orderVal.RoundedAmount = _roundamount;
                swishAmount = Convert.ToDecimal(txtPaymentAmount.Text, Defaults.UICultureInfo);

                txtSwishAmount.Text = swishAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }

            PaidOthersCalculation();
            OrderGrid.DataContext = orderVal;
            CalculateRemaining_Returned_CashBack();

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();
        }
        private void BtnStudentCard_Click(object sender, RoutedEventArgs e)
        {
            decimal tryDec;
            currentPaymentType = 11;
            if (string.IsNullOrEmpty(txtPaymentAmount.Text) || !Decimal.TryParse(txtPaymentAmount.Text, out tryDec))
            {
                studentcardAmount = orderVal.RemainingAmount;

                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }
                orderVal.RoundedAmount = _roundamount;
                txtStudentCardAmount.Text = studentcardAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
            }
            else
            {
                long intPart = (long)orderVal.RemainingAmount;
                decimal fracPart = orderVal.RemainingAmount - intPart;
                if (fracPart < (decimal)0.50)
                    _roundamount = (-1) * fracPart;
                else
                {
                    _roundamount = Convert.ToDecimal(1) - fracPart;
                }

                orderVal.RoundedAmount = _roundamount;
                studentcardAmount = Convert.ToDecimal(txtPaymentAmount.Text, Defaults.UICultureInfo);
                txtStudentCardAmount.Text = studentcardAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            }
            PaidOthersCalculation();
            OrderGrid.DataContext = orderVal;
            CalculateRemaining_Returned_CashBack();

            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();
        }
        private void ButtonNumber_Click(object sender, RoutedEventArgs e)
        {
            txtPaymentAmount.Text = txtPaymentAmount.Text + (sender as Button).Content;
            txtPaymentAmount.Focus();
        }

        private void btnClearBox_Click(object sender, RoutedEventArgs e)
        {
            txtPaymentAmount.Text = "";
            txtPaymentAmount.Focus();
        }

        private bool PaidOthersCalculation()
        {
            try
            {
                //if (swishAmount > orderVal.TotalBillAmount)
                //{
                //    MessageBox.Show(UI.Message_CouponExceedTotal, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //    txtSwishAmount.Text = "";
                //    return false;
                //}

                if (accountAmount > orderVal.TotalBalanceAmount)
                {
                    MessageBox.Show(UI.Message_SumGreaterThanTotalBalance, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtAccountAmount.Text = "";
                    BTNClearAll_Click(null, null);
                    return false;
                }
                if (depositAmount > orderVal.TotalBalanceAmount)
                {
                    MessageBox.Show(UI.Message_SumGreaterThanTotalBalance, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtDepositAmount.Text = "";
                    BTNClearAll_Click(null, null);
                    return false;
                }

                if (swishAmount + accountAmount + couponAmount + creditNoteAmount + BeamAmount + OnlineCashAmount + depositAmount > orderVal.TotalBalanceAmount)
                {
                    MessageBox.Show(UI.Message_SumGreaterThanTotalBalance, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtCouponAmount.Text = "";
                    BTNClearAll_Click(null, null);
                    return false;
                }

                decimal DebitCardAmount = string.IsNullOrEmpty(txtDebitCardAmount.Text) ? 0 : Convert.ToDecimal(txtDebitCardAmount.Text);


                OrderGrid.DataContext = null;



                orderVal.PaidOthersAmount = accountAmount + couponAmount + creditCardAmount + BeamAmount + DebitCardAmount + swishAmount + mobilecardAmount + studentcardAmount + creditNoteAmount + OnlineCashAmount + depositAmount;
                OrderGrid.DataContext = orderVal;



                return true;
            }

            catch (Exception ex)
            {

                throw ex;
                //MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //return false;
            }
        }
        List<TipModViewModel> TipModcls = new List<TipModViewModel>();



        void TipModCalculation()
        {
            try
            {

                decimal TotalBillamount = orderVal.TotalBillAmount;
                //decimal TotalBalanceamount = orderVal.TotalBalanceAmount; 
                decimal tipAmount = orderVal.TipAmount;

                decimal freecouponAmount = 0;
                decimal swishReceivedAmount = string.IsNullOrEmpty(txtSwishAmount.Text) ? 0 : Utilities.AmountParse(txtSwishAmount.Text);
                decimal accountAmount = string.IsNullOrEmpty(txtAccountAmount.Text) ? 0 : Utilities.AmountParse(txtAccountAmount.Text);
                decimal couponAmount = string.IsNullOrEmpty(txtCouponAmount.Text) ? 0 : Utilities.AmountParse(txtCouponAmount.Text);
                decimal creditCardAmount = collectedCardAmount;
                decimal debiCardAmount = string.IsNullOrEmpty(txtDebitCardAmount.Text) ? 0 : Utilities.AmountParse(txtDebitCardAmount.Text);
                decimal cashAmount = string.IsNullOrEmpty(txtReceivedCash.Text) ? 0 : Utilities.AmountParse(txtReceivedCash.Text);
                decimal depositAmount = string.IsNullOrEmpty(txtDepositAmount.Text) ? 0 : Utilities.AmountParse(txtDepositAmount.Text);

                decimal RunningTotal = 0;
                if (tipAmount > 0)
                {
                    TipModcls = new List<TipModViewModel>();
                    RunningTotal = freecouponAmount - TotalBillamount;
                    if (RunningTotal > 0 && RunningTotal >= tipAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "FreeCoupoun",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                    else if (RunningTotal > 0 && freecouponAmount > 0)
                    {
                        //tipBalance = RunningTotal;
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "FreeCoupoun",
                            tipModeAmount = RunningTotal
                        });
                        tipAmount = tipAmount - RunningTotal;
                        RunningTotal = RunningTotal - RunningTotal;
                    }

                    RunningTotal = accountAmount + RunningTotal;
                    if (RunningTotal > 0 && RunningTotal >= tipAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "Account",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                    else if (RunningTotal > 0 && accountAmount > 0)
                    {
                        //tipBalance = RunningTotal;
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "Account",
                            tipModeAmount = RunningTotal
                        });
                        tipAmount = tipAmount - RunningTotal;
                        RunningTotal = RunningTotal - RunningTotal;

                        //return;
                    }
                    RunningTotal = couponAmount + RunningTotal;
                    if (RunningTotal > 0 && RunningTotal >= tipAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "GiftCard",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                    else if (RunningTotal > 0 && couponAmount > 0)
                    {
                        // tipBalance = RunningTotal;

                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "GiftCard",
                            tipModeAmount = RunningTotal
                        });
                        tipAmount = tipAmount - RunningTotal;
                        RunningTotal = RunningTotal - RunningTotal;
                        //return;
                    }


                    RunningTotal = creditCardAmount + RunningTotal;
                    if (RunningTotal > 0 && RunningTotal >= tipAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "CreditCard",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                    else if (RunningTotal > 0 && creditCardAmount > 0)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "CreditCard",
                            tipModeAmount = RunningTotal
                        });
                        tipAmount = tipAmount - RunningTotal;
                        RunningTotal = RunningTotal - RunningTotal;
                    }
                    RunningTotal = debiCardAmount + RunningTotal;
                    if (RunningTotal > 0 && RunningTotal >= tipAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "DebitCard",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                    else if (RunningTotal > 0 && debiCardAmount > 0)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "DebitCard",
                            tipModeAmount = RunningTotal
                        });
                        tipAmount = tipAmount - RunningTotal;
                        RunningTotal = RunningTotal - RunningTotal;
                    }

                    RunningTotal = cashAmount + RunningTotal;
                    if (RunningTotal > 0 && RunningTotal >= tipAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "Cash",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                    RunningTotal = depositAmount + RunningTotal;
                    if (RunningTotal > 0 && RunningTotal >= depositAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "Deposit",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BTNClearAll_Click(object sender, RoutedEventArgs e)
        {
            CancelDiscount();
            txtReceivedCash.Text = "";
            txtCouponAmount.Text = "";
            txtAccountAmount.Text = "";
            txtSwishAmount.Text = "";
            txtStudentCardAmount.Text = "";
            txtCreditCardAmount.Text = "";
            txtDebitCardAmount.Text = "";
            txtMobileTerminalAmount.Text = "";
            txtDiscountAmount.Text = "";
            txtCreditNote.Text = "";
            txtBeam.Text = "";
            txtTip.Text = "";
            txtDepositAmount.Text = "";
            //txtOnlineCash.Text = "";
            currentPaymentType = 1;
            txtPaymentAmount.Focus();

            decimal billAmount = _payable;// orderVal.TotalBillAmount;

            OrderGrid.DataContext = null;

            orderVal.TotalBillAmount = billAmount;
            orderVal.TotalBalanceAmount = billAmount;
            orderVal.RemainingAmount = billAmount;
            orderVal.TipAmount = 0;
            orderVal.PaidCashAmount = 0;
            orderVal.PaidOthersAmount = 0;
            orderVal.CashBackAmount = 0;
            orderVal.ReturnedAmount = 0;
            orderVal.RoundedAmount = 0;
            OnlineCashAmount = 0;
            swishAmount = 0;
            accountAmount = 0;
            couponAmount = 0;
            creditCardAmount = 0;
            mobilecardAmount = 0;
            studentcardAmount = 0;
            creditNoteAmount = 0;
            BeamAmount = 0;
            depositAmount = 0;
            OrderGrid.DataContext = orderVal;

        }
        public bool CheckControlUnitStatus()
        {
            try
            {
                var uc = PosState.GetInstance().ControlUnitAction;
                uc.ControlUnit.CheckStatus();
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Control Unit", ex.Message);
                return false;
            }
        }
        public void ShowSurvey()
        {
            if (ConfigurationManager.AppSettings["ShowSurvey"] == "1")
            {
                //SurveyHTMLWindow window = new SurveyHTMLWindow();
                //window.ShowDialog();
                ProcessStartInfo start = new ProcessStartInfo();
                // Enter in the command line arguments, everything you would enter after the executable name itself
                start.Arguments = "";
                // Enter the executable to run, including the complete path
                start.FileName = ConfigurationManager.AppSettings["SurveyEXEPath"];
                // Do you want to show a console window?
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;
                int exitCode;


                // Run the external process & wait for it to finish
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();

                    // Retrieve the app's exit code
                    exitCode = proc.ExitCode;
                }

                //System.Diagnostics.Process.Start(ConfigurationManager.AppSettings["SurveyEXEPath"]);
            }
        }

        private void CheckOutOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //try
                //{
                //    //paid content
                //    PaidClientConnection(orderMaster.OrderIntID, orderMaster.TableId);
                //}
                //catch (Exception ex)
                //{
                //    LogWriter.LogWrite(ex);

                //}
                //LogWriter.LogWrite(new Exception("CheckOutOrder_Click IsOpenOrder" + OrderId));
                //var dirBong = Defaults.SettingsList[SettingCode.DirectBong] == "1" ? true : false;

                var masterorder = presenter.GetOrderMasterDetailById(OrderId); 
                try
                {
                    //var masterorder = presenter.GetOrderMasterDetailById(OrderId);
                    if (masterorder != null && masterorder.IsForAdult && SourceWindow == null)
                    {
                        ShowSurvey();
                    }


                    if (Defaults.IsOpenOrder == false && masterorder.OrderStatusFromType != OrderStatus.ReturnOrder && string.IsNullOrEmpty(masterorder.Bong))
                    {
                        if (Defaults.BONG && Defaults.DirectBONG)
                        {
                            LogWriter.LogWrite(new Exception("Defaults.BONG" + OrderId));
                            presenter.SendPrintToKitchenWithoutReset(masterorder);
                            //var masterorder = presenter.GetOrderMasterDetailById(OrderId);
                            //orderMaster.Bong = masterorder.Bong;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.CheckOutLogWrite("CheckOutOrder_Click exception" + ex.Message.ToString(), OrderId);

                }

                //  Defaults.PerformanceLog.Add("Checkout from payment window started-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                LogWriter.CheckOutLogWrite("Checkout from payment window started", OrderId);
                if (string.IsNullOrEmpty(lblTotalBillPayment.Text))
                    return;

                if (CheckControlUnitStatus() == false)
                {
                    LogWriter.CheckOutLogWrite("Checking Control Unit Status->", OrderId);
                    CUConnectionWindow cuConnectionWindow = new CUConnectionWindow();
                    if (cuConnectionWindow.ShowDialog() == false)
                        return;
                }

                // var mode = PosState.GetInstance().CashGuard;
                swishAmount = string.IsNullOrEmpty(txtSwishAmount.Text) ? 0 : decimal.Parse(txtSwishAmount.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                studentcardAmount = string.IsNullOrEmpty(txtStudentCardAmount.Text) ? 0 : decimal.Parse(txtStudentCardAmount.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                var DebitCardAmount = string.IsNullOrEmpty(txtDebitCardAmount.Text) ? 0 : decimal.Parse(txtDebitCardAmount.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                var cashAmount = string.IsNullOrEmpty(txtReceivedCash.Text) ? 0 : decimal.Parse(txtReceivedCash.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                decimal mobileTerminalAmount = string.IsNullOrEmpty(txtMobileTerminalAmount.Text) ? 0 : decimal.Parse(txtMobileTerminalAmount.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                couponAmount = string.IsNullOrEmpty(txtCouponAmount.Text) ? 0 : decimal.Parse(txtCouponAmount.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                creditNoteAmount = string.IsNullOrEmpty(txtCreditNote.Text) ? 0 : decimal.Parse(txtCreditNote.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                BeamAmount = string.IsNullOrEmpty(txtBeam.Text) ? 0 : decimal.Parse(txtBeam.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                OnlineCashAmount = string.IsNullOrEmpty(txtOnlineCash.Text) ? 0 : decimal.Parse(txtOnlineCash.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                decimal cardCollected = string.IsNullOrEmpty(txtCreditCardAmount.Text) ? 0 : decimal.Parse(txtCreditCardAmount.Text, NumberStyles.Currency, Defaults.UICultureInfo);

                depositAmount = string.IsNullOrEmpty(txtDepositAmount.Text) ? 0 : decimal.Parse(txtDepositAmount.Text, NumberStyles.Currency, Defaults.UICultureInfo);

                decimal TotalBillAmount = orderVal.TotalBillAmount;

                decimal RemainigAmount = orderVal.RemainingAmount;
                decimal PaidCash = orderVal.PaidCashAmount;
                decimal ReturnAmount = orderVal.ReturnedAmount;
                decimal CashBackAmount = orderVal.CashBackAmount;
                //if (TotalBillAmount == RemainigAmount) 29/01/2020 : need to order from checkout with 0 amount 
                //return; 
                if (TotalBillAmount == 0 && ReturnAmount > 0)
                {
                    MessageBox.Show("Please enter correct amount.", Defaults.AppProvider.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (masterorder != null && masterorder.OrderDirection > 0)
                {
                    if (selectedCustomer != null && TotalBillAmount > selectedCustomer.DepositAmount && selectedCustomer.HasDeposit == true)
                    {
                        MessageBox.Show("Insufficient Deposit Amount!", Defaults.AppProvider.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }

                if (RemainigAmount > 0)
                {
                    MessageBox.Show("Hela beloppet måste fyllas i", Defaults.AppProvider.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }


                var seemlesProducts = currentOrderDetail.Where(p => p.Product.Seamless == true).ToList();
                if (seemlesProducts.Count > 0)
                {
                    List<Product> products = new List<Product>();
                    foreach (var detail in seemlesProducts)
                    {
                        for (int i = 1; i <= detail.Quantity; i++)
                            products.Add(detail.Product);
                    }


                }

                TipModCalculation();
                bool bRes = false;
                PaymentTransactionStatus creditcardPaymentResult = null;
                ProgressWindow progressDialog = new ProgressWindow();
                Thread backgroundThread = new Thread(
                    new ThreadStart(() =>
                    {


                        bRes = CheckOut(swishAmount, couponAmount, accountAmount, creditCardAmount, DebitCardAmount, cashAmount, TotalBillAmount, RemainigAmount, PaidCash, ReturnAmount, CashBackAmount, mobileTerminalAmount, studentcardAmount, creditNoteAmount, BeamAmount, OnlineCashAmount, creditcardPaymentResult, depositAmount);
                        progressDialog.Closed += (arg, ev) =>
                        {
                            progressDialog = null;
                        };
                        progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressDialog.Close();
                            if (orderVal.RemainingAmount == 0.00m)
                            {
                                if (bRes)
                                {
                                    // Transaction complete, kick drawer
                                    try
                                    {
                                        this.IsEnabled = false;
                                        if (cashAmount > 0 || couponAmount > 0 || CashBackAmount > 0)
                                        {
                                            if (Defaults.CASH_GUARD == false)
                                                presenter.OpenCashDrawer((orderDirection) * cashAmount);
                                        }

                                        if (accountAmount > 0 && selectedCustomer.DirectPrint)
                                            PrintCustomerInvoice();


                                    }
                                    catch (Exception ex)
                                    {
                                        LogWriter.LogWrite(ex);
                                        App.MainWindow.ShowError(UI.Global_Error, ex.Message);
                                    }
                                    finally
                                    {
                                        this.IsEnabled = true;
                                    }
                                    this.DialogResult = true;
                                }

                            }
                            else
                                this.DialogResult = true;
                        }));
                    }));
                if (creditCardAmount > 0)
                {
                    //if (creditCardAmount > orderVal.TotalBillAmount && orderDirection==-1)
                    //{
                    //    if (MessageBox.Show("Kontantbeloppet är för högt", Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    //        return;
                    //}
                    var cardvat = vatAmount;
                    if (cashAmount > 0 || swishAmount > 0 || mobilecardAmount > 0 || creditNoteAmount > 0 || BeamAmount > 0 || studentcardAmount > 0 || DebitCardAmount > 0)
                        cardvat = (vatAmount / orderVal.TotalBillAmount) * creditCardAmount;
                    //  creditCardAmount = orderDirection * creditCardAmount; //Deos it is need to send refend payment as minus value when case is return order?
                    var result = orderDirection == 1 ?
                        Utilities.ProcessPaymentPurchase(creditCardAmount, cardvat, cashAmount > 0 ? 0 : CashBackAmount, TabelId, OrderId) :
                        Utilities.ProcessPaymentRefund(creditCardAmount, cardvat < 0 ? cardvat * -1 : cardvat, cashAmount > 0 ? 0 : CashBackAmount, TabelId, OrderId);
                    if (result.Result == PaymentTransactionStatus.PaymentResult.SUCCESS || // Device reported purchase was successful
                        result.Result == PaymentTransactionStatus.PaymentResult.NO_DEVICE_CONFIGURED) // Device set to NONE, allow since its most likely an external device
                    {
                        creditcardPaymentResult = result;
                        backgroundThread.Start();
                        progressDialog.ShowDialog();
                        backgroundThread.Join();

                    }

                }
                else
                {
                    backgroundThread.Start();
                    progressDialog.ShowDialog();
                    backgroundThread.Join();

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        private bool CheckOut(decimal swishAmount, decimal couponAmount, decimal accountAmount, decimal creditCardAmount, decimal DebitCardAmount, decimal cashAmount, decimal TotalBillAmount, decimal RemainigAmount, decimal PaidCash, decimal ReturnAmount, decimal CashBackAmount, decimal mobileTerminalAmount, decimal studentCardAmount, decimal _creditNoteAmount, decimal _beamAmount, decimal _onlineCashAmount, PaymentTransactionStatus creditcardPaymentResult, decimal _depositAmount)
        {
            bool res = false;
            List<TipModViewModel> tipModclass = TipModcls;

            Payment dtoPayment = new Payment();
            bool hasReturnAmount = true;
            bool hasTipIncluded = false;
            LogWriter.CheckOutLogWrite("Saving payment info", OrderId);
            if (cashAmount > 0)
            {
                decimal amountToReturn = 0;
                if (hasReturnAmount && cashAmount >= ReturnAmount)
                {
                    amountToReturn = ReturnAmount;
                    hasReturnAmount = false;
                }

                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "Cash" select p.tipModeAmount).SingleOrDefault(); //cls.Single(x=>x.tipMode=="Account").Where(c => c.tipMode == "account").SingleOrDefault();
                //if (orderDirection == -1)
                //    cashAmount = -cashAmount;

                if (tipamount > 0 && !hasTipIncluded)
                {
                    cashAmount = cashAmount - tipamount;
                }

                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = cashAmount - amountToReturn,// - ReturnAmount,
                    PaymentDate = DateTime.Now,
                    TypeId = 1,
                    CashCollected = cashAmount,
                    CashChange = ReturnAmount,
                    ReturnAmount = amountToReturn,// if given extra amount in cash
                    PaymentRef = UI.CheckOutOrder_Method_Cash,// "Kontant",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                presenter.SavePayment(dtoPayment, _roundamount);
                ReturnAmount = 0;
                hasTipIncluded = true;
            }
            else if(cashAmount < 0) {
                decimal amountToReturn = 0;

                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "Cash" select p.tipModeAmount).SingleOrDefault(); //cls.Single(x=>x.tipMode=="Account").Where(c => c.tipMode == "account").SingleOrDefault();
                //if (orderDirection == -1)
                //    cashAmount = -cashAmount;

                if (tipamount > 0 && !hasTipIncluded)
                {
                    cashAmount = cashAmount - tipamount;
                }

                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = cashAmount - amountToReturn,// - ReturnAmount,
                    PaymentDate = DateTime.Now,
                    TypeId = 1,
                    CashCollected = cashAmount,
                    CashChange = ReturnAmount,
                    ReturnAmount = amountToReturn,// if given extra amount in cash
                    PaymentRef = UI.CheckOutOrder_Method_Cash,// "Kontant",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                presenter.SavePayment(dtoPayment, _roundamount);
                ReturnAmount = 0;
                hasTipIncluded = true;
            }

            if (CashBackAmount > 0)
            {
                if (orderDirection == -1 && _depositAmount <= 0)// "ReturnOrder")
                {
                    decimal amountToReturn = 0;
                    if (hasReturnAmount && CashBackAmount >= ReturnAmount)
                    {
                        amountToReturn = ReturnAmount;
                        hasReturnAmount = false;
                    }
                    int typeId = (orderDirection == -1 && currentPaymentType != 1) ? currentPaymentType : cashAmount > 0 ? 7 : 1;
                    //if (_depositAmount > 0)
                    //    typeId = 16; // Deposit
                    dtoPayment = new Payment
                    {
                        OrderId = OrderId,
                        PaidAmount = 0,
                        PaymentDate = DateTime.Now,
                        TypeId = typeId,
                        CashCollected = 0,
                        CashChange = CashBackAmount,
                        ReturnAmount = amountToReturn,
                        PaymentRef = UI.CheckOutOrder_Label_CashBack,

                        TipAmount = 0,
                        Direction = orderDirection
                    };
                    presenter.SavePayment(dtoPayment);
                    ReturnAmount = 0;

                }
            }
            if (mobileTerminalAmount > 0)
            {
                decimal amountToReturn = 0;
                if (hasReturnAmount && mobileTerminalAmount >= ReturnAmount)
                {
                    amountToReturn = ReturnAmount;
                    hasReturnAmount = false;

                }
                decimal tipamount = orderVal.TipAmount;
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = mobilecardAmount - amountToReturn,// cashAmount > 0 ? mobileTerminalAmount : mobileTerminalAmount - ReturnAmount,
                    PaymentDate = DateTime.Now,
                    TypeId = 9,
                    CashCollected = mobileTerminalAmount,
                    CashChange = cashAmount > 0 ? 0 : ReturnAmount,
                    ReturnAmount = amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_Mobile,
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                // if (CashBackAmount == 0)
                // CashBackAmount = ReturnAmount;
                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;
                hasTipIncluded = true;
            }
            if (creditCardAmount > 0)
            {
                decimal amountToReturn = 0;
                int typeId = (!string.IsNullOrEmpty(creditcardPaymentResult.ProductName) && creditcardPaymentResult.ProductName.ToUpper() == "AMERICAN EXPRESS") ? 14 : 4;
                if (hasReturnAmount && creditCardAmount >= ReturnAmount && typeId == 4)
                {
                    amountToReturn = ReturnAmount;
                    hasReturnAmount = false;

                }
                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "CreditCard" select p.tipModeAmount).SingleOrDefault(); //cls.Single(x=>x.tipMode=="Account").Where(c => c.tipMode == "account").SingleOrDefault();
                if (tipamount > 0 && !hasTipIncluded)
                {
                    creditCardAmount = creditCardAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = creditCardAmount - amountToReturn,// cashAmount > 0 ? creditCardAmount : creditCardAmount - ReturnAmount,
                    PaymentDate = DateTime.Now,
                    TypeId = typeId,
                    CashCollected = creditCardAmount,
                    CashChange = cashAmount > 0 ? 0 : ReturnAmount > 0 ? ReturnAmount : 0,
                    ReturnAmount = amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_CreditCard,// "Kort",
                    ProductName = creditcardPaymentResult.ProductName,
                    DeviceTotal = (decimal)creditcardPaymentResult.Total,
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };

                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;
                hasTipIncluded = true;
            }
            if (swishAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && swishAmount > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}
                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "Swish" select p.tipModeAmount).SingleOrDefault();
                if (tipamount > 0 && !hasTipIncluded)
                {
                    swishAmount = swishAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = swishAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 10,
                    CashCollected = swishAmount,
                    CashChange = ReturnAmount,
                    ReturnAmount = amountToReturn,
                    PaymentRef = "Swish",// UI.CheckOutOrder_Method_FreeCoupon,// "Gratiskupong",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };

                presenter.SavePayment(dtoPayment, _roundamount);
                ReturnAmount = 0;
                hasTipIncluded = true;
            }
            if (studentCardAmount > 0)
            {
                decimal amountToReturn = 0;
                if (hasReturnAmount && studentcardAmount >= ReturnAmount)
                {
                    amountToReturn = ReturnAmount;
                    hasReturnAmount = false;

                }
                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "Studentkort" select p.tipModeAmount).SingleOrDefault();
                if (tipamount > 0 && !hasTipIncluded)
                {
                    studentCardAmount = studentCardAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = studentCardAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 11,
                    CashCollected = studentCardAmount,
                    CashChange = ReturnAmount,
                    ReturnAmount = amountToReturn,
                    PaymentRef = "Studentkort",// UI.CheckOutOrder_Method_FreeCoupon,// "Gratiskupong",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };

                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;
                hasTipIncluded = true;
            }

            if (_beamAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && _beamAmount > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = _beamAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 13,
                    CashCollected = _beamAmount,
                    CashChange = ReturnAmount,
                    ReturnAmount = 0,// amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_Beam,
                    TipAmount = 0,
                    Direction = orderDirection
                };

                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;

            }
            if (_onlineCashAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && _onlineCashAmount > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = _onlineCashAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 15,
                    CashCollected = _onlineCashAmount,
                    CashChange = ReturnAmount,
                    ReturnAmount = 0,// amountToReturn,
                    PaymentRef = "Online " + UI.CheckOutOrder_Method_Cash,
                    TipAmount = 0,
                    Direction = orderDirection
                };

                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;

            }
            if (couponAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && couponAmount > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}
                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "GiftCard" select p.tipModeAmount).SingleOrDefault();
                if (tipamount > 0 && !hasTipIncluded)
                {
                    couponAmount = couponAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = couponAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 3,
                    CashCollected = couponAmount,
                    CashChange = 0,
                    ReturnAmount = 0,//amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_GiftCard,// "Presentkort",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;
                hasTipIncluded = true;
            }
            if (_creditNoteAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && _creditNoteAmount     > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = _creditNoteAmount - amountToReturn,//- ReturnAmount,
                    PaymentDate = DateTime.Now,
                    TypeId = 12,
                    CashCollected = _creditNoteAmount,
                    CashChange = ReturnAmount,
                    ReturnAmount = 0,// amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_CreditNote,// "Presentkort",
                    TipAmount = 0,
                    Direction = orderDirection
                };
                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;


            }
            if (accountAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && accountAmount > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}

                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "Account" select p.tipModeAmount).SingleOrDefault(); //cls.Single(x=>x.tipMode=="Account").Where(c => c.tipMode == "account").SingleOrDefault();
                if (tipamount > 0 && !hasTipIncluded)
                {
                    accountAmount = accountAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = accountAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 2,
                    CashCollected = accountAmount,
                    CashChange = 0,
                    ReturnAmount = 0,// amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_Account,// "Faktura",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                res = presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;
                hasTipIncluded = true;
                if (res)
                {
                    presenter.UpdateOrderCustomerInfo(OrderId, selectedCustomer, OrderComments);

                }

            }

            if (_depositAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && accountAmount > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}

                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "Account" select p.tipModeAmount).SingleOrDefault(); //cls.Single(x=>x.tipMode=="Account").Where(c => c.tipMode == "account").SingleOrDefault();
                if (tipamount > 0 && !hasTipIncluded)
                {
                    depositAmount = depositAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = depositAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 16,
                    CashCollected = depositAmount,
                    CashChange = 0,
                    ReturnAmount = 0,// amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_Deposit,// "Faktura",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                res = presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;
                hasTipIncluded = true;
                if (res)
                {
                    try
                    {
                        if (!_customerPresenter.IsOrderExistInDepositHistory(OrderId))
                        {
                            decimal oldBalance = selectedCustomer.DepositAmount;
                            decimal newBalance = 0;
                            if (orderDirection > 0)
                                newBalance = selectedCustomer.DepositAmount - depositAmount;
                            else
                                newBalance = selectedCustomer.DepositAmount + depositAmount;

                            selectedCustomer.DepositAmount = newBalance;
                            _customerPresenter.UpdateCustomer(selectedCustomer);

                            bool reslt = _customerPresenter.AddDepositHistory(selectedCustomer.Id, depositAmount, Guid.Parse(Defaults.User.Id),
                                OrderId,
                                orderDirection > 0 ? DepositType.Debit : DepositType.CreditViaReturnOrder,
                                "",
                                "",
                                oldBalance,
                                newBalance,
                                Guid.Parse(Defaults.TerminalId)
                                );

                            presenter.UpdateOrderCustomerInfo(OrderId, selectedCustomer, OrderComments);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            if (DebitCardAmount > 0)
            {
                decimal amountToReturn = 0;
                //if (hasReturnAmount && DebitCardAmount > ReturnAmount)
                //{
                //    amountToReturn = ReturnAmount;
                //    hasReturnAmount = false;

                //}
                decimal tipamount = orderVal.TipAmount;// = (from p in tipModclass where p.tipMode == "DebitCard" select p.tipModeAmount).SingleOrDefault(); //cls.Single(x=>x.tipMode=="Account").Where(c => c.tipMode == "account").SingleOrDefault();
                if (tipamount > 0 && !hasTipIncluded)
                {
                    DebitCardAmount = DebitCardAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = DebitCardAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = 5,
                    CashCollected = DebitCardAmount,
                    CashChange = 0,
                    ReturnAmount = amountToReturn,
                    PaymentRef = UI.CheckOutOrder_Method_DebitCard,// "Paid By Debit Card",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                presenter.SavePayment(dtoPayment);
                ReturnAmount = 0;
                hasTipIncluded = true;

            }



            try
            {
                if (orderVal.RemainingAmount == 00m)
                {
                    try
                    {

                        ReceiptGenerated = presenter.GenerateInvoiceLocal(OrderId, App.MainWindow.ShiftNo, Defaults.User.Id, creditcardPaymentResult);
                        if (ReceiptGenerated == false)
                        {
                            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReceiptFail), OrderId);
                        }
                        else
                        {
                            LogWriter.CheckOutLogWrite("Generating receipt process completed successfully", OrderId);
                        }
                    }
                    catch (ControlUnitException e)
                    {
                        LogWriter.CheckOutLogWrite(e.Message, OrderId);
                        ShowError(Defaults.AppProvider.AppTitle, e.Message);
                        //CUConnectionWindow ucConnectionWindow = new CUConnectionWindow();
                        //if (ucConnectionWindow.ShowDialog() == true)
                        //{
                        //    try
                        //    {

                        //        ReceiptGenerated = presenter.GenerateInvoiceLocal(OrderId, App.MainWindow.ShiftNo, Defaults.User.Id, creditcardPaymentResult);
                        //    }
                        //    catch (ControlUnitException exp)
                        //    {

                        //        ShowError(Defaults.AppProvider.AppTitle, exp.Message);
                        //        presenter.UpdateOrderStatus(OrderId, OrderStatus.ReceiptFailed);
                        //    }
                        //}
                        //else
                        //    presenter.UpdateOrderStatus(OrderId, OrderStatus.ReceiptFailed);
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        LogWriter.CheckOutLogWrite(ex.Message, OrderId);
                        LogWriter.LogWrite(ex);
                    }
                    return true;
                }
                else
                {


                    orderVal.PaidCashAmount = 0;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {


                        txtReceivedCash.Text = "";
                        txtCouponAmount.Text = "";
                        txtAccountAmount.Text = "";
                        txtSwishAmount.Text = "";
                        txtStudentCardAmount.Text = "";
                        txtCreditCardAmount.Text = "";
                        txtDebitCardAmount.Text = "";

                        txtTip.Text = "";

                        txtPaymentAmount.Focus();

                        decimal billAmount = orderVal.RemainingAmount;

                        OrderGrid.DataContext = null;

                        orderVal.TotalBillAmount = billAmount;
                        orderVal.TotalBalanceAmount = billAmount;
                        orderVal.RemainingAmount = billAmount;
                        orderVal.TipAmount = 0;
                        orderVal.PaidCashAmount = 0;
                        orderVal.PaidOthersAmount = 0;
                        orderVal.CashBackAmount = 0;
                        orderVal.ReturnedAmount = 0;

                        swishAmount = 0;
                        studentCardAmount = 0;
                        accountAmount = 0;
                        couponAmount = 0;
                        creditCardAmount = 0;

                        OrderGrid.DataContext = orderVal;
                    }));
                    return true;
                }
            }
            catch (ControlUnitException e)
            {
                LogWriter.LogException(e);
                this.ShowError(e.Message, "");
            }

            Defaults.PerformanceLog.Add("Checkout completed...         -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            return false;
        }

        private void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
        {

        }
        string OrderComments = "";
        private void CheckOutAccount_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomer == null)
            {
                string title = UI.CheckOutOrder_Invoice + "kund";
                CustomerWindow customerWindow = new CustomerWindow(true, title, CustomerType.NonDeposit);
                if (customerWindow.ShowDialog() == true)
                {
                    selectedCustomer = customerWindow.SelectedCustomer;
                    accountAmount = accountAmount + orderVal.RemainingAmount;

                    txtAccountAmount.Text = accountAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
                    PaidOthersCalculation();

                    CalculateRemaining_Returned_CashBack();
                    //  OrderComments = Utilities.PromptInput(UI.Sales_OrderCommentButton, UI.Sales_EnterComment, OrderComments);


                }
            }
            else
            {

                accountAmount = accountAmount + orderVal.RemainingAmount;

                txtAccountAmount.Text = accountAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
                PaidOthersCalculation();

                CalculateRemaining_Returned_CashBack();
                OrderComments = Utilities.PromptInput(UI.Sales_OrderCommentButton, UI.Sales_EnterComment, OrderComments);
            }

        }
        private void CheckOutByAccount(Customer customer)
        {
            try
            {
                accountAmount = _payable;
                int tip = 0;
                if (accountAmount > 0)
                {
                    Payment payment = new Payment
                    {
                        OrderId = OrderId,
                        PaidAmount = _payable,
                        PaymentDate = DateTime.Now,
                        TypeId = 2,
                        CashCollected = 0,
                        CashChange = 0,
                        ReturnAmount = 0,
                        PaymentRef = "Account Customer",
                        TipAmount = 0
                    };

                    bool res = presenter.SavePayment(payment);
                    presenter.UpdateOrderCustomerInfo(OrderId, customer, OrderComments);

                    if (res)
                    {
                        lblPaidCash.Text = "";
                        lblReturnCash.Text = "";
                        lblRemaining.Text = "";
                        txtAccountAmount.Text = "";
                        txtReceivedCash.Text = "";
                        txtCouponAmount.Text = "";
                        txtCreditCardAmount.Text = "";
                        txtDebitCardAmount.Text = "";
                        txtSwishAmount.Text = "";
                        txtStudentCardAmount.Text = "";
                        txtPaymentAmount.Text = "";
                        txtTip.Text = "";
                        txtDepositAmount.Text = "";
                        _payable = 0;
                        res = presenter.GenerateInvoiceLocal(OrderId, App.MainWindow.ShiftNo, Defaults.User.Id, null);
                        if (res)
                        {
                            MessageBox.Show(UI.Message_PaymentSavedSucessully);
                        }
                        NewRecord();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                //LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NewRecord()
        {
            this.DialogResult = true;
        }


        Guid ICheckoutView.GetOrderId()
        {
            return OrderId;
        }

        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);
        }

        public void SetVatAmount(decimal p)
        {
            vatAmount = p;
        }

        #region Print Customer Invoice

        private void PrintCustomerInvoice()
        {
            try
            {
                OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
                var invoiceId = orderRepo.GenerateCustomerInvoice(OrderId, selectedCustomer.Id);
                MemoryStream ms = GenerateOcrPdf(invoiceId);
                string fileName = Environment.CurrentDirectory + @"\temp.pdf";
                //  string fileName = @"C:\NIMPOS\temp.pdf";
                FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                ms.WriteTo(file);
                file.Close();
                ms.Close();
                string printerName = GetPrinterName();
                PdfPrinterHelper.SendFileToPrinter(fileName, printerName);

            }
            catch (Exception ex)
            {

                // throw;
            }
        }

        private string GetPrinterName()
        {
            string printerName = "Microsoft XPS Document Writer";
            var printer = Defaults.Printers.FirstOrDefault(p => p.LocationName == "Invoice");
            if (printer != null)
                printerName = printer.PrinterName;
            PrintQueueCollection printers = new PrintServer().GetPrintQueues();
            PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
            var data = printers.Where(pn => pn.Name == printerName);
            if (data.Count() > 0)
            {
                prntque = printers.FirstOrDefault(pn => pn.Name == printerName);
                if (prntque == null)
                    printerName = "Microsoft XPS Document Writer";
            }
            else // Fall back to XPS Document Writer
            {

                printerName = "Microsoft XPS Document Writer";
            }

            return printerName;
        }
        private MemoryStream GenerateOcrPdf(Guid invoiceId)
        {
            //var orderRepo = new OrderRepository(PosState.GetInstance().Context);
            //var customerIvoice = orderRepo.GetCustomerInvoice(invoiceId);
            var orderRepo = new InvoiceHandler();
            var customerIvoice = orderRepo.GetCustomerInvoice2(invoiceId);

            var invoice = customerIvoice.Invoice;
            var orderDetails = customerIvoice.OrderDetails;
            var customer = customerIvoice.Customer;
            //Dictionary<string, string> orderLines = new Dictionary<string, string>();
            //foreach (var orderItem in orderDetails)
            //{
            //    orderLines.Add(orderItem.ItemName, Text.FormatNumber(orderItem.UnitPrice, 2));
            //}


            decimal grossTotal = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
            // decimal itemsDiscount = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ItemDiscount), 2);
            var totalVat = orderDetails.Sum(ol => ol.VatAmount());

            decimal netTotal = grossTotal - totalVat;

            decimal decTotal = grossTotal;
            decimal decVATPercent = totalVat;// orderDetails.FirstOrDefault().VAT; // Get first VAT available

            string strReminderText = string.Empty;
            //if (receipt.ReminderFee > 0 && invoice.Reminder)
            //{
            //    strReminderText = string.Concat("Vid sen betalning tar vi ut en påminnelseavgift på ", Text.FormatNumber(invoice.ReminderFee), " kr.");
            //}

            //decimal decReminderFee = 0;
            //if (invoiceType == InvoiceType.Reminder)
            //{
            //    decReminderFee = invoice.ReminderFee;
            //}
            decimal decReminderFee = 0;
            string strOcr = invoice.InvoiceNumber.ToString();
            DateTime dtExpireDate = DateTime.Now.AddDays(Defaults.InvoiceDueDays);// invoice.LastRetryDateTime.Date.AddDays(invoice.Interval);

            return Pdf.Ocr.Create(
                invoice.InvoiceNumber
                , Defaults.Outlet.Name
                , invoice.CreationDate
                , dtExpireDate
                , customer.Name
                , strOcr
                , invoice.InvoiceNumber
                , invoice.Remarks// orderMaster.InvoiceNumber //  string.Concat(invoice.tOrder.tCustomer.tCompany.tCorporate.FirstName, " ", invoice.tOrder.tCustomer.tCompany.tCorporate.LastName)
                , " "
                , orderDetails
                , Defaults.Terminal.UniqueIdentification + "-" + invoice.InvoiceNumber.ToString()
                , "Customer Invoice"
                , decTotal
                , decVATPercent
                , netTotal
                , strReminderText
                , string.Concat(customer.Name, " ", customer.OrgNo)
                , customer.Address1
                , string.Concat(customer.ZipCode, " ", customer.City)
                , dtExpireDate
                , decReminderFee
                );

            // decTotalAmount * companyAgreement.DibsPartnerPercent / 100
        }


        #endregion

        private void BtnDiscount_Click(object sender, RoutedEventArgs e)
        {
            var total = presenter.GetOrderTotal(OrderId);
            if (total > 0)
            {
                //if (presenter.HasDiscountAvailable(OrderId))
                //{
                    if (!string.IsNullOrEmpty(Defaults.DiscountCode))
                    {
                        DiscountCodeWindow discountCodeWindow = new DiscountCodeWindow();
                        if (discountCodeWindow.ShowDialog() == false)
                            return;
                    }
                    var orderLines1 = new OrderRepository(PosState.GetInstance().Context).GetOrderLinesById(OrderId);
                    var MaxPrice= orderLines1.Where(a => a.Product.DiscountAllowed == true).ToList();
                    if(MaxPrice.Count==0)
                    {
                        MessageBox.Show("Discount can not be applied on these items.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    DiscountWindow discountWindow = new DiscountWindow(MaxPrice.Sum(a => a.UnitPrice * a.Quantity));
                    if (discountWindow.ShowDialog() == true)
                    {
                        txtDiscountAmount.Text = discountWindow.Amount.ToString();
                        if (discountWindow.Percentage)
                            txtDiscountAmount.Text = Math.Round((discountWindow.Amount / 100) * (MaxPrice.Sum(a => a.UnitPrice * a.Quantity)), 2).ToString();
                        decimal totalOrderAmount = presenter.UpdateOrderDiscount(OrderId, discountWindow.Amount, discountWindow.Percentage);

                        var orderLines = new OrderRepository(PosState.GetInstance().Context).GetOrderLinesById(OrderId);
                        App.MainWindow.UpdateItems(orderLines);
                        orderVal.TotalBillAmount = totalOrderAmount;// presenter.OrderTotal();

                        object val = presenter.GetTotalPaid();
                        decimal paid = val != DBNull.Value ? Convert.ToDecimal(val) : 0;
                        OrderGrid.DataContext = null;
                        init(totalOrderAmount, paid);
                    }
                //}
                //else
                //{
                //    MessageBox.Show("Discount on some of the items can not be applied.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //}
            }
            else
            {
                MessageBox.Show("Discount can not be applied on these items.","Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void CancelDiscount()
        {
            if (!string.IsNullOrEmpty(txtDiscountAmount.Text))
            {
                decimal totalOrderAmount = presenter.CancelOrderDiscount(OrderId);

                var orderLines = new OrderRepository(PosState.GetInstance().Context).GetOrderLinesById(OrderId);
                App.MainWindow.UpdateItems(orderLines);
                orderVal.TotalBillAmount = totalOrderAmount;// presenter.OrderTotal();

                object val = presenter.GetTotalPaid();
                decimal paid = val != DBNull.Value ? Convert.ToDecimal(val) : 0;
                OrderGrid.DataContext = null;
                init(totalOrderAmount, paid);
            }
        }


        private void CheckOutOrderWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        public void SetParameters(decimal totalOrderAmount, Guid orderId, int _direction, List<OrderLine> orderDetail, int tableId)
        {

        }

        private void BtnTip_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomer == null)
            {
                string title = UI.CheckOutOrder_Invoice + "kund";
                CustomerWindow customerWindow = new CustomerWindow(true, title, CustomerType.Deposit);
                if (customerWindow.ShowDialog() == true)
                {
                    selectedCustomer = customerWindow.SelectedCustomer;
                    depositAmount = depositAmount + orderVal.RemainingAmount;

                    txtDepositAmount.Text = depositAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
                    PaidOthersCalculation();

                    CalculateRemaining_Returned_CashBack();
                    //  OrderComments = Utilities.PromptInput(UI.Sales_OrderCommentButton, UI.Sales_EnterComment, OrderComments);


                }
            }
            else
            {

                depositAmount = depositAmount + orderVal.RemainingAmount;

                txtDepositAmount.Text = depositAmount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol); //lblRemaining.Text;
                PaidOthersCalculation();

                CalculateRemaining_Returned_CashBack();
                OrderComments = Utilities.PromptInput(UI.Sales_OrderCommentButton, UI.Sales_EnterComment, OrderComments);
            }

        }

    }
}