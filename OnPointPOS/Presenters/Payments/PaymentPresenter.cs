using POSSUM.Data;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Presenters.Payments;
using POSSUM.Res;
using POSSUM.ViewModels;
using POSSUM.Views.PrintOrder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace POSSUM.PromptInfo
{
    public class PaymentPresenter
    {
        private IPaymentView view;
        private IPaymentDevice paymentDevice;
        private String terminalInfo = "";
        private bool open = false;
        private bool connected = false;
        private bool inTransaction = false;
        private decimal totalAmount;
        private decimal vatAmount;
        private decimal cashbackAmount;
        private bool success = false;
        private bool inReferralMode = false;
        private String authCode = "";
        public PaymentTransactionStatus status;
        private bool amountSent = false;
        private Guid orderId;
        ApplicationDbContext db;
        public PaymentPresenter(IPaymentView view, decimal totalAmount, decimal vatAmount, decimal cashbackAmount, PaymentTransactionStatus status, Guid orderId)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP1=> " + "constructor called"));
                db = PosState.GetInstance().Context;
                this.orderId = orderId;
                this.view = view;
                this.status = status;
                this.totalAmount = totalAmount - cashbackAmount;
                this.vatAmount = vatAmount;
                this.cashbackAmount = cashbackAmount;
                /* view.SetTitle(config.Title);
                 view.SetDescription(config.Description);
                 view.SetValue(config.Value);*/
                PosState state = PosState.GetInstance();

                view.ShowKeypad(false);
                view.ShowAbort(false);
                view.ShowOk(false);
                view.ShowOption(false);
                view.SetStatusText("");
                LogWriter.LogWrite(new Exception("PP2=> " + "Calling set payment text"));
                view.SetPaymentText(String.Format(UI.Main_Amount + ": {0}\n " + UI.Global_VAT + " {1}\n " + UI.CheckOutOrder_Method_Cash + ": {2}", totalAmount.ToString("C2", Defaults.UICultureInfo), vatAmount.ToString("C2", Defaults.UICultureInfo), cashbackAmount.ToString("C2", Defaults.UICultureInfo)));
                LogWriter.LogWrite(new Exception("PP2=> " + "Called set payment text"));
                paymentDevice = state.PaymentDevice;
                if (paymentDevice != null) // paymentdevice is not set to NONE
                {
                    LogWriter.LogWrite(new Exception("PP3=> " + "Payment device is not null"));
                    // Register an event handler
                    paymentDevice.RegisterEventHandler(new PaymentEventHandler(this, view));

                    // Connect to device if its not already connected
                    paymentDevice.Connect();
                    LogWriter.LogWrite(new Exception("PP4=> " + "Payment device connect is called"));
                    // Wait TODO, remove eventually
                    //Thread.Sleep(1000);

                    if (!tryStartTransaction())
                    {
                        LogWriter.LogWrite(new Exception("PP5=> " + "Payment device set waiting text"));
                        view.SetStatusText(UI.PaymentTerminal_Waiting);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => Constructor: " + ex);
            }
        }

        public void Reprint()
        {
            try
            {
                if (!paymentDevice.IsConnected())
                {
                    paymentDevice.RegisterEventHandler(new PaymentEventHandler(this, view));
                    paymentDevice.Connect();
                }
                paymentDevice.RePrintLastCancelledPayment();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => Reprint: " + ex);
            }
        }

        private bool tryStartTransaction()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "tryStartTransaction"));
                var dStatus = paymentDevice.GetDeviceStatus();
                if (!amountSent && (dStatus == PaymentDeviceStatus.CONNECTED || dStatus == PaymentDeviceStatus.CLOSED || dStatus == PaymentDeviceStatus.OPEN))
                {
                    LogWriter.LogWrite(new Exception("PP6.1=> " + "tryStartTransaction => " + status.TransactionType + "totalAmount=" + totalAmount + ", vatAmount=" + vatAmount + ", cashbackAmount=" + cashbackAmount + ", orderId=" + orderId + ", and paymentDevice=" + paymentDevice));
                    paymentDevice.StartTransaction(status.TransactionType);
                    LogWriter.LogWrite(new Exception("PP6.2=> " + "tryStartTransaction => " + status.TransactionType + "totalAmount=" + totalAmount + ", vatAmount=" + vatAmount + ", cashbackAmount=" + cashbackAmount + ", orderId=" + orderId));
                    paymentDevice.ProcessPaymentAmount(this.totalAmount, vatAmount, cashbackAmount, orderId);
                    LogWriter.LogWrite(new Exception("PP6.3=> " + "tryStartTransaction => " + status.TransactionType + "totalAmount=" + totalAmount + ", vatAmount=" + vatAmount + ", cashbackAmount=" + cashbackAmount + ", orderId=" + orderId));

                    // Process payment 

                    amountSent = true;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => tryStartTransaction: " + ex);
                throw ex;
            }
        }



        internal void onTerminalInfo(string infoText)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalInfo"));
                this.terminalInfo = infoText;

                updateTerminalStatusText();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalInfo: " + ex);
                throw ex;
            }
        }
        
        private void updateTerminalStatusText()
        { // {0},{1},{2}-
            try
            {
                view.SetStatusText(String.Format("{3}", connected ? 1 : 0, open ? 1 : 0, inTransaction ? 1 : 0, terminalInfo));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => updateTerminalStatusText: " + ex);
                throw ex;
            }
        }

        internal void onTerminalStatus(bool connected, bool open)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalStatus = " + connected + ", and open = " + open));
                if (!this.connected && connected) // Going from disconnected to connected, send transaction type
                {
                    tryStartTransaction();
                    //paymentDevice.StartTransaction(PaymentTransactionType.PURCHASE);
                }

                if (!this.open && open) // Going from closed to open
                {
                    //paymentDevice.ProcessPaymentAmount(totalAmount, vatAmount, cashbackAmount);
                }
                this.open = open;
                this.connected = connected;
                updateTerminalStatusText();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalStatus: " + ex);
                throw ex;
            }
        }

        internal void onTerminalDisplay(string txtrow1, string txtrow2, string txtrow3, string txtrow4, int? status, int? transactionType, int? transactionResult)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalDisplay=>" + "txtrow1" + txtrow1 + ",txtrow2=" + txtrow2 + ",txtrow3=" + txtrow3 + ",txtrow4=" + txtrow4 + ",status=" + status + "transactionType=" + transactionType + ",transactionResult=" + transactionResult));
                view.SetTerminalDisplay(String.Format("{0}\n{1}\n{2}\n{3}", txtrow1, txtrow2, txtrow3, txtrow4), status, transactionType, transactionResult);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalDisplay: " + ex);
                throw ex;
            }
        }

        internal void onTerminalInTransaction(bool inTransaction)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalInTransaction"));
                this.inTransaction = inTransaction;
                updateTerminalStatusText();
                if (inTransaction == false)
                {
                    tryStartTransaction();
                }
                else
                {
                    // paymentDevice.StartTransaction(status.TransactionType);
                    paymentDevice.ProcessPaymentAmount(this.totalAmount, vatAmount, cashbackAmount, orderId);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalInTransaction: " + ex);
                throw ex;
            }
        }

        internal void onTerminalResult(PaymentTransactionType transType, bool success, string cashierText)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalResult = " + cashierText + ", success = " + success));
                this.success = success;
                this.terminalInfo = cashierText;
                updateTerminalStatusText();
                tryPrintFailedReceipt();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalResult: " + ex);
                throw ex;
            }
        }

        private void tryPrintFailedReceipt()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "tryPrintFailedReceipt"));
                if (!success && status.CustomerReceipt != null)
                {
                    StringBuilder b = new StringBuilder();
                    status.CustomerReceipt.ToList().ForEach(r => b.AppendLine(r));
                    Printing printing = new Printing();
                    printing.setPrinterName(Defaults.PrinterName);
                    printing.print(b.ToString());
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => tryPrintFailedReceipt: " + ex);
                throw ex;
            }
        }



        internal void onTerminalReceiptResult(PaymentDataType paymentDataType, IList<string> resultValue, bool isSignature, string productName, double total)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalReceiptResult"));
                // throw new NotImplementedException();
                status.ProductName = productName;
                status.Total = total;
                if (paymentDataType == PaymentDataType.CUSTOMER_RECEIPT)
                {
                    status.CustomerReceipt = new List<string>(resultValue);
                }
                else if (paymentDataType == PaymentDataType.MERCHANT_RECEIPT)
                {
                    status.MerchantReceipt = new List<string>(resultValue);

                    if (isSignature)//|| status.TransactionType == PaymentTransactionType.REFUND
                    {
                        //TODO: Enable only if we have on hold implemented
                        //MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(UI.PaymentTerminal_Endorsement + "?", UI.PaymentTerminal_SignatureBuy, System.Windows.MessageBoxButton.YesNo);
                        //if (messageBoxResult == MessageBoxResult.Yes)
                        //{
                        paymentDevice.GetCustomerReceipt();
                        //}
                        //else
                        //{
                        //    paymentDevice.Cancel();
                        //}
                    }
                    else
                    {
                        paymentDevice.GetCustomerReceipt();

                    }
                }
                if (paymentDataType == PaymentDataType.CUSTOMER_RECEIPT)
                {
                    tryPrintFailedReceipt();
                }
                if (status.CustomerReceipt != null && status.MerchantReceipt != null)
                {
                    paymentDevice.EndTransaction();
                    close(success);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalReceiptResult: " + ex);
                throw ex;
            }
        }

        internal void SetPaymentDialogCloseForced(bool force)
        {
            try
            {
                paymentDevice.SetPaymentDialogCloseForced(force);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => SetPaymentDialogCloseForced: " + ex);
                throw ex;
            }
        }

        internal void onTerminalBatchResult(PaymentDataType paymentDataType, IList<string> resultValue)
        {
            // throw new NotImplementedException();
        }

        internal void onTerminalTransactionResult(PaymentDataType paymentDataType, IList<string> resultValue)
        {
            // throw new NotImplementedException();
        }

        private void close(bool success)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "close is callsed with " + success));
                if (paymentDevice != null)
                {
                    //paymentDevice.UnRegisterEventHandler();
                }
                if (success)
                {
                    status.Result = PaymentTransactionStatus.PaymentResult.SUCCESS;
                }
                else
                {
                    status.Result = PaymentTransactionStatus.PaymentResult.CANCELLED;
                }
                view.CloseWithStatus(success);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => close: " + ex);
                throw ex;
            }
        }



        internal void HandleKeypadKeyPress(String p)
        {
            try
            {
                switch (p)
                {
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                        authCode += p;
                        break;
                    case "C":
                        authCode = "";
                        break;
                    case "OK":
                        paymentDevice.SendReferralCode(authCode);
                        inReferralMode = false;
                        authCode = "";

                        break;


                }
                updateBox();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => HandleKeypadKeyPress: " + ex);
                throw ex;
            }
        }

        private void updateBox()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "updateBox"));
                if (inReferralMode)
                {
                    view.SetPaymentText(String.Format(UI.PaymentTerminal_ResponseCode + ":\n{0}", authCode));
                }
                else
                {
                    view.SetPaymentText(String.Format(UI.Main_Amount + ": {0}\n " + UI.Global_VAT + " {1}\n " + UI.CheckOutOrder_Method_Cash + ": {2}", totalAmount.ToString("C2", Defaults.UICultureInfo), vatAmount.ToString("C2", Defaults.UICultureInfo), cashbackAmount.ToString("C2", Defaults.UICultureInfo)));
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => updateBox: " + ex);
                throw ex;
            }
        }


        internal void HandleCancelClick()
        {
            try
            {
                if (inReferralMode)
                {
                    LogWriter.LogWrite(new Exception("PP6=>inReferralMode " + "HandleCancelClick"));
                    // send end transaction to cancel 
                    //TODO: Close button should not show here
                    paymentDevice.Cancel();
                    //paymentDevice.EndTransaction();
                    inReferralMode = false;
                    view.ShowKeypad(false);
                    view.ShowOption(false);
                    view.ShowClose(false);
                    view.ShowAbort(false);
                    view.ShowAbort(POSSUM.Res.UI.Global_Cancel, false);
                    view.ShowOk(POSSUM.Res.UI.Payment_Skip_Referral, false);
                }
                else
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "HandleCancelClick"));
                    paymentDevice.Cancel();
                    //paymentDevice.EndTransaction();
                    // close(true);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => HandleCancelClick: " + ex);
                throw ex;
            }
        }
        internal void CancelPayment()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "CancelPayment"));
                if (inReferralMode)
                {
                    paymentDevice.Cancel();
                }
                else
                {
                    paymentDevice.Cancel();
                    // close(true);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => CancelPayment: " + ex);
                throw ex;
            }
        }
        internal void HandleReconnectClick()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "HandleReconnectClick"));
                paymentDevice.ResetConnection();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => HandleReconnectClick: " + ex);
                throw ex;
            }
        }
        internal void HandleOkClick()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "HandleOkClick"));
                if (inReferralMode)
                {
                    paymentDevice.SendReferralCode("9999");
                    view.ShowKeypad(false);
                    view.ShowOption(false);
                    // send 9999 to sendApprovalCode to let cashier skip calling for verification
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => HandleOkClick: " + ex);
                throw ex;
            }
        }

        internal void onTerminalReferral(string instruction)
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalReferral"));
                view.SetStatusText(instruction);
                updateBox();
                inReferralMode = true;
                view.ShowKeypad(false);
                view.ShowOption(UI.PaymentTerminal_EnterCode, true);
                var x = POSSUM.Res.UI.CheckOutOrder_C;
                view.ShowAbort(POSSUM.Res.UI.Global_Cancel, true);
                view.ShowOk(POSSUM.Res.UI.Payment_Skip_Referral, true);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalReferral: " + ex);
                throw ex;
            }
        }

        internal void onTerminalNeedClose()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalNeedClose"));
                view.ShowAbort(POSSUM.Res.UI.Global_Cancel, true);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => onTerminalNeedClose: " + ex);
                throw ex;
            }
        }

        internal void HandleClosing()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "HandleClosing"));
                if (paymentDevice != null)
                {
                    paymentDevice.UnRegisterEventHandler();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => HandleClosing: " + ex);
                throw ex;
            }
        }

        internal void HandleOptionClick()
        {
            try
            {
                LogWriter.LogWrite(new Exception("PP6=> " + "HandleOptionClick"));
                if (inReferralMode)
                {
                    authCode = Utilities.PromptInput(UI.Global_Enter + " " + UI.PaymentTerminal_ResponseCode, UI.PaymentTerminal_EnterCode, "", false);
                    view.ShowOk(false);
                    view.ShowOption(false);
                    view.ShowAbort(false);
                    paymentDevice.SendReferralCode(String.IsNullOrWhiteSpace(authCode) ? "9999" : authCode);
                    inReferralMode = false;
                    authCode = "";
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("PaymentPresenter => HandleOptionClick: " + ex);
                throw ex;
            }
        }

        internal void CancelOrder(Guid orderId, string id)
        {
            bool isCancelled = new OrderRepository(db).CancelOrder(orderId, Defaults.User.Id);
        }

        public class PaymentEventHandler : IPaymentDeviceEventHandler
        {
            private PaymentPresenter paymentPresenter;
            private IPaymentView view;
            IList<string> resultValue = new List<string>();
            private bool isSignature = false;

            public PaymentEventHandler(PaymentPresenter paymentPresenter, IPaymentView view)
            {
                this.paymentPresenter = paymentPresenter;
                this.view = view;
            }

            public object onInfoEvent(string t)
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onInfoEvent=" + t));
                    paymentPresenter.onTerminalInfo(t);
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onInfoEvent: " + ex);
                    throw ex;
                }
            }
            public object onExceptionEvent(string t)
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onExceptionEvent=" + t));
                    paymentPresenter.onTerminalInfo(t);
                    view.SetErroText(t);
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onExceptionEvent: " + ex);
                    throw ex;
                }
            }
            public object onTerminalReConnectEvent(string t)
            {
                try
                {
                    view.SetStatusText(t);
                    view.SetTerminalDisplay("", null, null, null);//Clear Body Text when Reconnecting
                    view.ShowReconnect(true);
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalReConnectEvent: " + ex);
                    throw ex;
                }
            }
            public object onTerminalCancelEvent(string t)
            {
                try
                {
                    view.SetStatusText(t);

                    view.ShowClose(true);
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalCancelEvent: " + ex);
                    throw ex;
                }
            }
            public object onTerminalDisplay(string txtrow1, string txtrow2, string txtrow3, string txtrow4, int? status, int? transactionType, int? transactionResult)
            {
                try
                {
                    paymentPresenter.onTerminalDisplay(txtrow1, txtrow2, txtrow3, txtrow4, status, transactionType, transactionResult);

                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalDisplay: " + ex);
                    throw ex;
                }
            }


            public object onStatusChanged(bool connected, bool open)
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onStatusChanged" + connected + open));
                    paymentPresenter.onTerminalStatus(connected, open);
                    bool openAndConnected = open && connected;
                    Debug.WriteLine("IsConnected " + connected + " isopen " + open);

                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onStatusChanged: " + ex);
                    throw ex;
                }
            }

            public object onTransactionStart()
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onTransactionStart"));
                    Console.WriteLine("onTransactionStart");
                    paymentPresenter.onTerminalInTransaction(true);
                    Console.WriteLine("Transaction started");
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTransactionStart: " + ex);
                    throw ex;
                }
            }

            public object onTransactionEnd()
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onTransactionEnd"));
                    paymentPresenter.onTerminalInTransaction(false);
                    Debug.WriteLine("Transaction done");
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTransactionEnd: " + ex);
                    throw ex;
                }
            }


            public object onTerminalResultEvent(PaymentTransactionType transType, bool success, string cashierText)
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalResultEvent=" + cashierText));
                    paymentPresenter.onTerminalResult(transType, success, cashierText);
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalResultEvent: " + ex);
                    throw ex;
                }
            }

            public object onBamboraSignatureRequired(string message)
            {
                try
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("PaymentTerminal_Endorsement" + "?", "PaymentTerminal_SignatureBuy", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.No)
                    {
                        paymentPresenter.CancelPayment();

                        // paymentDevice.GetCustomerReceipt();
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onBamboraSignatureRequired: " + ex);
                    throw ex;
                }
            }

            public object onBamboraOutOfBalance(string message)
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onBamboraOutOfBalance=" + message));
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Balance is not enough. ", "Balance is not enough", System.Windows.MessageBoxButton.OK);
                    if (messageBoxResult == MessageBoxResult.OK)
                    {
                        paymentPresenter.CancelPayment();
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onBamboraOutOfBalance: " + ex);
                    throw ex;
                }
            }
            public object onBamboraEvent(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, PaymentTransactionType transType, bool success, string cashierText, List<string> customerreceipt, List<string> merchantreceipt, bool needtosign, string productName, double total)
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onBamboraEvent=>paymentDataType=" + paymentDataType + ", cashierText=" + cashierText));
                    if (paymentDataItemType == PaymentDataItemType.END)
                    {

                        if (!success)
                        {

                            paymentPresenter.onTerminalReceiptResult(PaymentDataType.CUSTOMER_RECEIPT, customerreceipt, isSignature, productName, total);
                            paymentPresenter.onTerminalResult(transType, success, cashierText);

                        }
                        else if (success)
                        {

                            paymentPresenter.onTerminalResult(transType, success, cashierText);

                            if (transType == PaymentTransactionType.REFUND)
                            {
                                customerreceipt.Add("GODKÄNNES FÖR KREDITERING TILL");
                                customerreceipt.Add("KONTO ENLIGT OVAN");
                                customerreceipt.Add("");
                                customerreceipt.Add("SIGNATUR BUTIK:");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("....................................");
                                customerreceipt.Add("");
                                customerreceipt.Add("ID:");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("....................................");
                                customerreceipt.Add("");

                            }
                            customerreceipt.Add("");
                            // customerreceipt.Add("*** KUNDENS EXEMPLAR - SPARA KVITTOT");
                            customerreceipt.Add("");

                            paymentPresenter.onTerminalReceiptResult(PaymentDataType.CUSTOMER_RECEIPT, customerreceipt, isSignature, productName, total);

                            if (transType == PaymentTransactionType.REFUND)
                            {
                                isSignature = true;
                                merchantreceipt.Add("");
                                merchantreceipt.Add("KUNDENS NAMN:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("KUNDENS TELEFON:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("SIGNATUR:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("*** SPARA KVITTOT");
                                merchantreceipt.Add("");
                            }
                            else if (needtosign)
                            {
                                isSignature = true;
                                var signatureList = new List<string>();
                                signatureList.Add("");
                                signatureList.Add("GODKÄNNES FÖR DEBITERING AV MITT");
                                signatureList.Add("KONTO ENLIGT OVAN");
                                signatureList.Add("");
                                signatureList.Add("Legitimation:");
                                signatureList.Add("");
                                signatureList.Add("");
                                signatureList.Add("");
                                signatureList.Add("....................................");
                                signatureList.Add("");
                                signatureList.Add("Signatur:");
                                signatureList.Add("");
                                signatureList.Add("");
                                signatureList.Add("");
                                signatureList.Add("....................................");
                                signatureList.Add("");

                                var psnInfo = merchantreceipt.FirstOrDefault(a => a.StartsWith("REF:"));
                                var index = merchantreceipt.IndexOf(psnInfo) - 2;
                                if (index > 0)
                                    merchantreceipt.InsertRange(index, signatureList);
                                else
                                    merchantreceipt.Concat(signatureList);
                            }
                            else
                            {
                                isSignature = false;
                            }

                            paymentPresenter.onTerminalReceiptResult(PaymentDataType.MERCHANT_RECEIPT, merchantreceipt, isSignature, productName, total);

                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onBamboraEvent: " + ex);
                    throw ex;
                }
            }


            public object onTerminalReceiptData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value, double total)
            {
                try
                {
                    LogWriter.LogWrite(new Exception("PP6=> " + "onTerminalReceiptData=" + header));
                    if (paymentDataItemType == PaymentDataItemType.END && value != null)
                    {
                        resultValue.Add((!String.IsNullOrEmpty(header) ? header + "\t" : "") + value);
                        paymentPresenter.onTerminalReceiptResult(paymentDataType, resultValue, isSignature, "", total);

                    }
                    else if (paymentDataItemType == PaymentDataItemType.END)
                    {
                        paymentPresenter.onTerminalReceiptResult(paymentDataType, resultValue, isSignature, "", total);
                        resultValue = new List<string>();
                        isSignature = false;
                    }
                    else
                    {
                        resultValue.Add((!String.IsNullOrEmpty(header) ? header + "\t" : "") + value);
                        if (paymentDataType == PaymentDataType.MERCHANT_RECEIPT &&
                            (
                             value == "SIGN:" ||
                             value.Contains("rens namnteckning") ||
                             value.Contains("rens namn") ||
                             value == "ID:"))
                        { // For SIGN and LEG add more space
                            isSignature = true;
                            resultValue.Add("");
                            resultValue.Add("");
                            resultValue.Add("");
                            resultValue.Add("");
                            resultValue.Add("....................................");
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalReceiptData: " + ex);
                    throw ex;
                }
            }

            public object onTerminalBatchData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value)
            {
                try
                {
                    if (paymentDataItemType == PaymentDataItemType.END)
                    {
                        paymentPresenter.onTerminalBatchResult(paymentDataType, resultValue);
                        resultValue = new List<string>();
                    }
                    else
                    {
                        resultValue.Add(!String.IsNullOrEmpty(header) ? header + ": " : "" + value);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalBatchData: " + ex);
                    throw ex;
                }
            }

            public object onTransactionData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value)
            {
                try
                {
                    if (paymentDataItemType == PaymentDataItemType.END)
                    {
                        paymentPresenter.onTerminalTransactionResult(paymentDataType, resultValue);
                        resultValue = new List<string>();
                    }
                    else
                    {
                        resultValue.Add(!String.IsNullOrEmpty(header) ? header + ": " : "" + value);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTransactionData: " + ex);
                    throw ex;
                }
            }


            public object onTerminalReferralEvent(string instruction)
            {
                try
                {
                    view.SetStatusText(instruction);
                    paymentPresenter.onTerminalReferral(instruction);
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalReferralEvent: " + ex);
                    throw ex;
                }
            }


            public void onTerminalNeedClose()
            {
                try
                {
                    paymentPresenter.onTerminalNeedClose();
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalNeedClose: " + ex);
                    throw ex;
                }
            }


            public object onVerifoneEvent(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType,
             PaymentTransactionType transType, bool success, string cashierText, List<string> customerreceipt,
             List<string> merchantreceipt, bool needtosign)
            {
                try
                {
                    if (paymentDataItemType == PaymentDataItemType.END)
                    {
                        if (!success)
                        {

                            paymentPresenter.onTerminalReceiptResult(PaymentDataType.CUSTOMER_RECEIPT, customerreceipt,
                                isSignature, "", 0);
                            paymentPresenter.onTerminalResult(transType, false, cashierText);
                        }
                        else
                        {
                            paymentPresenter.onTerminalResult(transType, true, cashierText);

                            if (transType == PaymentTransactionType.REFUND)
                            {
                                customerreceipt.Add("GODKÄNNES FÖR KREDITERING TILL");
                                customerreceipt.Add("KONTO ENLIGT OVAN");
                                customerreceipt.Add("");
                                customerreceipt.Add("SIGNATUR BUTIK:");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("....................................");
                                customerreceipt.Add("");
                                customerreceipt.Add("ID:");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("");
                                customerreceipt.Add("....................................");
                                customerreceipt.Add("");
                            }
                            customerreceipt.Add("");
                            customerreceipt.Add("*** KUNDENS EXEMPLAR - SPARA KVITTOT");
                            customerreceipt.Add("");

                            paymentPresenter.onTerminalReceiptResult(PaymentDataType.CUSTOMER_RECEIPT, customerreceipt,
                                isSignature, "", 0);

                            if (transType == PaymentTransactionType.REFUND)
                            {
                                isSignature = true;
                                merchantreceipt.Add("");
                                merchantreceipt.Add("KUNDENS NAMN:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("KUNDENS TELEFON:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("SIGNATUR:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("*** SPARA KVITTOT");
                                merchantreceipt.Add("");
                            }
                            else if (needtosign)
                            {
                                isSignature = true;
                                merchantreceipt.Add("GODKÄNNES FÖR DEBITERING AV MITT");
                                merchantreceipt.Add("KONTO ENLIGT OVAN");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("SIGN:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("ID:");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("....................................");
                                merchantreceipt.Add("");
                                merchantreceipt.Add("*** SPARA KVITTOT");
                                merchantreceipt.Add("");
                            }
                            else
                            {
                                isSignature = false;
                            }

                            paymentPresenter.onTerminalReceiptResult(PaymentDataType.MERCHANT_RECEIPT, merchantreceipt,
                                isSignature, "", 0);
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onVerifoneEvent: " + ex);
                    throw ex;
                }
            }

            public object onTerminalDeclinedEvent(string instruction)
            {
                try
                {
                    view.SetStatusText(instruction);

                    view.ShowWindowClose(true);
                    return null;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("PaymentEventHandler => onTerminalDeclinedEvent: " + ex);
                    throw ex;
                }
            }
        }

    }



}
