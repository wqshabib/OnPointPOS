using POSSUM.Events;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Presenters.CheckOut;
using POSSUM.Presenters.Customers;
using POSSUM.Res;
using POSSUM.Views.PrintOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POSSUM.Views.Customers
{
    /// <summary>
    /// Interaction logic for AddCustomerDepositWindow.xaml
    /// </summary>
    public partial class AddCustomerDepositWindow : ICustomerView
    {
        private readonly CustomerPresenter _presenter; 
        public Customer CurrentCustomer;
        public event UploadCustomerEventHandler UploadCustomer;
        
        public AddCustomerDepositWindow(Customer customer)
        {
            InitializeComponent();
            _presenter = new CustomerPresenter(this);
            CurrentCustomer = customer;
            //UploadCustomer += AddCustomerWindow_UploadCustomer;
            layoutGrid.DataContext = CurrentCustomer;
            lblTitle.Text = UI.Deposit_Title;
            lblCurrentAmount.Text = CurrentCustomer.DepositAmount.ToString();
        }

        private void BtnCloseAccountInfo_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonCash_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal depositAmount = 0;
                decimal.TryParse(txtAmount.Text, out depositAmount);
                if (depositAmount <= 0)
                {
                    MessageBox.Show("Amount should be greater than zero!", UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(CurrentCustomer.Name))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show(UI.Global_Deposit_Confirm, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }

                CurrentCustomer.DepositAmount = CurrentCustomer.DepositAmount + depositAmount;
                CurrentCustomer.LastBalanceUpdated = DateTime.Now;

                bool res = _presenter.UpdateCustomer(CurrentCustomer);
                if (res)
                {
                    bool reslt = _presenter.AddDepositHistory(CurrentCustomer.Id, depositAmount, Guid.Parse(Defaults.User.Id), Guid.Empty, DepositType.CreditViaCash,
                        "",
                        "",
                        CurrentCustomer.DepositAmount - depositAmount,
                        CurrentCustomer.DepositAmount,
                        Guid.Parse(Defaults.TerminalId));

                    this.DialogResult = true;
                    PrintReportWindow prwi = new PrintReportWindow();
                    List<string> lst = new List<string>();
                   
                    lst.Add("_____________________");
                    lst.Add("");
                    lst.Add("Deposit Amount: " + depositAmount);
                    lst.Add(UI.Transaction_Old_Balance + ": " + (CurrentCustomer.DepositAmount - depositAmount));
                    lst.Add(UI.Transaction_New_Balance + ": " + CurrentCustomer.DepositAmount);
                    lst.Add("");
                    //prwi.GenerateReport(string.Join(Environment.NewLine, lst), ReportType.CustomerReceipt);
                    Printing printing = new Printing();
                    printing.setPrinterName(Defaults.PrinterName);

                    printing.print(string.Join("\n",lst));

                    MessageBox.Show("Cash Deposit Successfully", UI.Message_Saved_Success, MessageBoxButton.OK, MessageBoxImage.Information);
                    //CustomerUpload(CurrentCustomer);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void ButtonCreditCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal depositAmount = 0;
                decimal.TryParse(txtAmount.Text, out depositAmount);
                if (depositAmount <= 0)
                {
                    MessageBox.Show("Amount should be greater than zero!", UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(CurrentCustomer.Name))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show(UI.Global_Deposit_Confirm, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }

                HandleDirectPaymentClick(depositAmount);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        private void CodeVisibility()
        {

        }
        private void AddCustomerWindow_UploadCustomer(object sender, Customer customer)
        {
            _presenter.UploadCustomer(customer);
        }
        public void SetCustomerResult(List<Customer> customers)
        {

        }

        public void SetFloors(List<Floor> floors)
        {

        }

        public string GetKeyword()
        {
            return "";
        }

        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);
        }


        private void CustomerUpload(Customer customer)
        {
            if (customer.Id != default(Guid))
            {

                var progressDialog = new ProgressWindow();
                var backgroundThread = new Thread(
                    new ThreadStart(() =>
                    {
                        if (UploadCustomer != null)
                        {
                            UploadCustomer(this, customer);
                        }
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
                this.DialogResult = true;
            }
        }


        public List<Customer> GetCustomers()
        {
            return new List<Customer>();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        #region Payment Credit card
        internal void HandleDirectPaymentClick(decimal depositAmount)
        {
            var orderId = Guid.Empty;
            //if (CheckControlUnitStatus() == false)
            //{
            //    CUConnectionWindow cuConnectionWindow = new CUConnectionWindow();
            //    if (cuConnectionWindow.ShowDialog() == false)
            //        return;
            //}

            //var dirBong = Defaults.SettingsList[SettingCode.DirectBong] == "1" ? true : false;
            //LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.DirectCardPaymentStarted), );

            long intPart = (long)depositAmount;

            decimal roundamount;

            decimal fracPart = depositAmount - intPart;
            if (fracPart < (decimal)0.50)
                roundamount = (-1) * fracPart;
            else
            {
                roundamount = Convert.ToDecimal(1) - fracPart;
            }

            decimal cashBackAmount = 0;

            bool bRes = true;

            PaymentTransactionStatus creditcardPaymentResult = null;
            var progressDialog = new ProgressWindow();

            var backgroundThread = new Thread(() =>
            {
                Defaults.PerformanceLog.Add("Starting Check out Order      ");
                //bRes = CheckOut(amount, true, roundamount, creditcardPaymentResult);

                progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressDialog.Close();

                    if (bRes)
                    {
                        // Transaction complete, kick drawer
                        try
                        {
                            //Defaults.PerformanceLog.Add("Check out completed ");

                            //Defaults.PerformanceLog.Add("Sending To Invoice Print      -> ");

                            //ar directPrint = new DirectPrint();
                            
                            //directPrint.PrintReceipt(orderId, false, bRes);
                            //LogWriter.CheckOutLogWrite("Receipt printed", orderId);

                            //LogWriter.JournalLog(Defaults.User.TrainingMode? JournalActionCode.ReceiptPrintedViaTrainingMode : JournalActionCode.ReceiptPrinted, MasterOrder.Id);
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogWrite(ex);
                            App.MainWindow.ShowError(UI.Global_Error, ex.Message);
                        }
                    }
                }));
            });

            var result = Utilities.ProcessPaymentPurchase(depositAmount, 0, cashBackAmount, 0, orderId);
            if (result.Result == PaymentTransactionStatus.PaymentResult.SUCCESS ||
                // Device reported purchase was successful
                result.Result == PaymentTransactionStatus.PaymentResult.NO_DEVICE_CONFIGURED)
            // Device set to NONE, allow since its most likely an external device
            {
                creditcardPaymentResult = result;
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();

                if (bRes)
                {
                    //LogWriter.CheckOutLogWrite("Checkout order completed successfully", MasterOrder.Id);
                    CurrentCustomer.DepositAmount = CurrentCustomer.DepositAmount + depositAmount;
                    CurrentCustomer.LastBalanceUpdated = DateTime.Now;
                    bool res = _presenter.UpdateCustomer(CurrentCustomer);
                    if (res)
                    {
                        var customerReceipt = string.Join(Environment.NewLine, result.CustomerReceipt);
                        var merchantReceipt = string.Join(Environment.NewLine, result.MerchantReceipt);
                        bool reslt = _presenter.AddDepositHistory(CurrentCustomer.Id, depositAmount, Guid.Parse(Defaults.User.Id), Guid.Empty, DepositType.CreditViaCard, 
                            customerReceipt, 
                            merchantReceipt,
                        CurrentCustomer.DepositAmount - depositAmount,
                        CurrentCustomer.DepositAmount,
                        Guid.Parse(Defaults.TerminalId));
                        PrintReportWindow prwi = new PrintReportWindow();
                        List<string> lst = new List<string>();
                        lst.Add("_____________________");
                        lst.Add("");
                        lst.Add("Deposit Amount: " + depositAmount);
                        lst.Add(UI.Transaction_Old_Balance + ": " + (CurrentCustomer.DepositAmount - depositAmount));
                        lst.Add(UI.Transaction_New_Balance + ": " + CurrentCustomer.DepositAmount);
                        lst.Add("");

                        var lstReceipt = result.CustomerReceipt.ToList();
                        lstReceipt.InsertRange(0, lst);
                        //prwi.GenerateReport(string.Join(Environment.NewLine, lstReceipt), ReportType.CustomerReceipt);
                        Printing printing = new Printing();
                        printing.setPrinterName(Defaults.PrinterName);

                        printing.print(string.Join("\n", lst));
                        this.DialogResult = true;
                        MessageBox.Show("Cash Deposit Successfully", UI.Message_Saved_Success, MessageBoxButton.OK, MessageBoxImage.Information);
                        //CustomerUpload(CurrentCustomer);
                    }
                }
            }
        }
        public bool CheckControlUnitStatus()
        {
            try
            {
                var uc = PosState.GetInstance().ControlUnitAction;
                return uc.ControlUnit.CheckStatus() == ControlUnitStatus.OK;
                // return true;
            }
            catch (Exception ex)
            {
                ShowError("Control Unit", ex.Message);
                return false;
            }
        }
        #endregion
    }
}
