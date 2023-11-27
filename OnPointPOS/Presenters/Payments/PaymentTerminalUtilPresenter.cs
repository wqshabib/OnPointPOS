using POSSUM.Integration;
using POSSUM.Integration.PaymentDevices.Verifone;
using POSSUM.Model;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSSUM.Presenters.Payments
{
    public class PaymentTerminalUtilPresenter
    {
        private IPaymentTerminalUtilView view;
        private IPaymentDevice paymentDevice;
        private StringBuilder infoWindow = new StringBuilder();
        private MENU MenuMode = MENU.DEFAULT;
        private int inputNumber;
        private int defaultNumber = 10;
        private string input = "";
        public enum MENU
        {
            DEFAULT,
            TRANSACTION_REPORT
        }
        public PaymentTerminalUtilPresenter(IPaymentTerminalUtilView view)
        {
            this.view = view;
            PosState state = PosState.GetInstance();
            paymentDevice = state.PaymentDevice;
            if (paymentDevice != null) // paymentdevice is not set to NONE
            {
                // Register an event handler
                paymentDevice.RegisterEventHandler(new PaymentUtilEventHandler(this, view));

                // Connect to device if its not already connected
                paymentDevice.Connect();
                view.ShowKeypad(false);
                view.ShowAbort(true);
                setMenuOptions();
            }
        }

        private void setMenuOptions()
        {
            switch (MenuMode)
            {
                case MENU.DEFAULT:
                    {
                        view.ShowOption(1, false);
                        view.ShowOption(2, false);
                        view.ShowOption(3, false);
                        view.ShowOption(4, false);
                        view.ShowOption(5, false);
                        view.ShowOption(6, false);

                        view.ShowOption(1, UI.PaymentTerminal_TestConnection, true);
                        view.ShowOption(2, UI.PaymentTerminal_ShowSettings, true);
                        view.ShowOption(3, UI.PaymentTerminal_ShowUnsentTransaction, true);
                        view.ShowOption(4, UI.PaymentTerminal_TransactionReport, true);
                        view.ShowOption(5, UI.PaymentTerminal_CloseBatch, true);
                        view.ShowOption(6, UI.PaymentTerminal_ResetConnection, true);

                        view.ShowOk(false);

                        view.ShowKeypad(false);
                        view.ShowResetMenu("<<", false);
                    }
                    break;
                case MENU.TRANSACTION_REPORT:
                    {
                        view.ShowOption(1, false);
                        view.ShowOption(2, false);
                        view.ShowOption(3, false);
                        view.ShowOption(4, false);
                        view.ShowOption(5, false);
                        view.ShowOption(6, false);
                        view.ShowOption(1, UI.PaymentTerminal_Detailed, true);
                        view.ShowOption(2, UI.PaymentTerminal_Total, true);
                        view.ShowOption(3, UI.PaymentTerminal_CloseBatch, true);

                        view.ShowResetMenu("<<", true);
                        view.ShowOk(false);

                        view.ShowKeypad(true);
                    }
                    break;
            }
        }

        internal void HandleOptionClick(int optionButton)
        {
            switch (MenuMode)
            {
                case MENU.DEFAULT:
                    {

                        switch (optionButton)
                        {
                            case 1: // test connection
                                infoWindow.Clear();
                                infoWindow.Append(UI.PaymentTerminal_TestConnection + " .......");// "TODO: Testar anslutning..."
                                updateInfoWindow();
                                paymentDevice.TestConnection();
                                break;
                            case 2: // get config
                                infoWindow.Clear();
                                infoWindow.Append(UI.PaymentTerminal_GettingConfig + " .......");//"TODO: Getting config ..."
                                updateInfoWindow();
                                paymentDevice.GetConfiguration();
                                break;
                            case 3: // get unsent transactions
                                infoWindow.Clear();
                                infoWindow.Append(UI.PaymentTerminal_ShowUnsentTransaction + " ......");//"TODO: Unsent transactions"
                                updateInfoWindow();
                                paymentDevice.GetUnsentTransactions();
                                break;
                            case 4: // get get transaction report
                                infoWindow.Clear();
                                updateInfoWindow();
                                MenuMode = MENU.TRANSACTION_REPORT;
                                setMenuOptions();
                                askForNumericInput(defaultNumber);
                                break;
                            case 5: // close batch
                                infoWindow.Clear();
                                infoWindow.Append(UI.PaymentTerminal_ShowUnsentTransaction);
                                updateInfoWindow();
                                paymentDevice.CloseBatch();
                                break;
                            case 6: // Reset Connection
                                infoWindow.Clear();
                                infoWindow.Append(UI.PaymentTerminal_ResetConnection);
                                updateInfoWindow();
                                paymentDevice.ResetConnection();
                                break;
                        }
                    }
                    break;
                case MENU.TRANSACTION_REPORT:
                    switch (optionButton)
                    {
                        case 1: // detailed transactionLog
                            infoWindow.Clear();
                            infoWindow.AppendLine(UI.PaymentTerminal_GettingDetailLog + " ......");//"TODO: Getting detailed transactionlog"
                            updateInfoWindow();
                            paymentDevice.GetTransactionLog(PaymentDataType.TRANSACTION_LOG_DETAILED, inputNumber);
                            //translogByNbr( type, inputNumber, )
                            //paymentDevice.TestConnection();
                            break;
                        case 2: // totals
                            infoWindow.Clear();
                            infoWindow.AppendLine(UI.PaymentTerminal_GettingTotalLog + " ......");//"TODO: Getting totals transactionlog ..."
                            updateInfoWindow();
                            paymentDevice.GetTransactionLog(PaymentDataType.TRANSACTION_LOG_TOTALS, inputNumber);

                            //paymentDevice.GetConfiguration();
                            break;
                        case 3: // close batch log
                            infoWindow.Clear();
                            infoWindow.AppendLine(UI.PaymentTerminal_GettingCloseBatch + " ......");//"TODO: Getting batch closes"
                            updateInfoWindow();
                            paymentDevice.GetTransactionLog(PaymentDataType.BATCH_CLOSE, inputNumber);
                            break;
                    }
                    break;
            }
        }

        private void askForNumericInput(int defaultCount, string actualInput = "")
        {
            defaultNumber = defaultCount;
            infoWindow.Clear();
            infoWindow.Append(String.Format(UI.PaymentTerminal_EnterTransactionCount + " [{0}]: ", defaultCount));
            if (!String.IsNullOrEmpty(actualInput))
            {
                infoWindow.Append(actualInput);
            }
            updateInfoWindow();
        }

        private void updateInfoWindow()
        {
            view.SetInfoWindow(infoWindow.ToString());
        }

        internal void HandleOkClick()
        {
            //throw new NotImplementedException();
        }
        internal void HandleCloseClick()
        {
            view.Close(true);
        }
        internal void HandleResetMenuClick()
        {
            if (MenuMode == MENU.TRANSACTION_REPORT)
            {
                MenuMode = MENU.DEFAULT;
                setMenuOptions();
            }

        }

        internal void HandleKeypadKeyPress(string key)
        {
            switch (key)
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
                    input += key;
                    askForNumericInput(defaultNumber, input);
                    inputNumber = int.Parse(input);
                    break;
                case "C":
                    input = "";
                    break;
            }
        }

        public class PaymentUtilEventHandler : IPaymentDeviceEventHandler
        {
            private PaymentTerminalUtilPresenter paymentPresenter;
            //private IPaymentTerminalUtilView view;
            IList<string> resultValue = new List<string>();

            public PaymentUtilEventHandler(PaymentTerminalUtilPresenter paymentPresenter, IPaymentTerminalUtilView view)
            {
                this.paymentPresenter = paymentPresenter;
                //this.view = view;
            }

            public object onInfoEvent(string t)
            {
                paymentPresenter.onTerminalInfo(t);
                return null;
            }

            public object onExceptionEvent(string t)
            {
                paymentPresenter.onTerminalInfo(t);
                return null;
            }

            public object onTerminalDisplay(string txtrow1, string txtrow2, string txtrow3, string txtrow4, int? status, int? transactionType, int? transactionResult)
            {
                //paymentPresenter.onTerminalDisplay(txtrow1, txtrow2, txtrow3, txtrow4);
                return null;
            }


            public object onStatusChanged(bool connected, bool open)
            {
                //paymentPresenter.onTerminalStatus(connected, open);
                //bool openAndConnected = open && connected;
                return null;
            }

            public object onTransactionStart()
            {
                //paymentPresenter.onTerminalInTransaction(true);
                return null;
            }

            public object onTransactionEnd()
            {
                //paymentPresenter.onTerminalInTransaction(false);
                return null;
            }


            public object onTerminalResultEvent(PaymentTransactionType transType, bool success, string cashierText)
            {
                paymentPresenter.onTerminalResult(transType, success, cashierText);
                return null;
            }

            public object onBamboraEvent(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, PaymentTransactionType transType, bool success, string cashierText, List<string> customerreceipt, List<string> merchantreceipt, bool signatureneeded,string productName,double total)
            {
                if (paymentDataItemType == PaymentDataItemType.END)
                {
                    paymentPresenter.onTerminalResult(transType, success, cashierText);
                    resultValue = customerreceipt;
                }
                return null;
            }

            public object onTerminalReceiptData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value,double total)
            {
                if (paymentDataItemType == PaymentDataItemType.END)
                {
                    //paymentPresenter.onTerminalReceiptResult(paymentDataType, resultValue);
                    resultValue = new List<string>();
                }
                else
                    resultValue.Add((!String.IsNullOrEmpty(header) ? header.PadRight(20) + "\t" : "") + formatValue(value));
                return null;
            }

            public object onTerminalBatchData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value)
            {
                if (paymentDataItemType == PaymentDataItemType.END)
                {
                    paymentPresenter.onTerminalBatchResult(paymentDataType, resultValue);
                    resultValue = new List<string>();
                }
                else
                    resultValue.Add((!String.IsNullOrEmpty(header) ? header.PadRight(20) + "\t" : "") + formatValue(value));
                return null;
            }

            public object onTransactionData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value)
            {
                if (paymentDataItemType == PaymentDataItemType.END)
                {
                    paymentPresenter.onTerminalTransactionResult(paymentDataType, resultValue);
                    resultValue = new List<string>();
                }
                else
                    resultValue.Add((!String.IsNullOrEmpty(header) ? header.PadRight(20) + "\t" : "") + formatValue(value));
                return null;
            }

            private string formatValue(string value)
            {
                string rslt = "";
                if (value.Contains(';'))
                {
                    var sVal = value.Split(';').ToList();
                    foreach (string s in sVal)
                    {
                        rslt += (s + "\t");
                    }
                }
                else
                    rslt = value;
                return rslt;
            }


            public object onTerminalReferralEvent(string instruction)
            {
                //paymentPresenter.onTerminalReferral(instruction);
                return null;
            }


            public void onTerminalNeedClose()
            {
                //paymentPresenter.onTerminalNeedClose();
            }
            public object onVerifoneReceipt(PaymentDataType type, VerifoneReceipt customerReceipt, VerifoneReceipt merchantReceipt)
            {
                throw new NotImplementedException();
            }

            public object onVerifoneEvent(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, PaymentTransactionType transType, bool success, string cashierText, List<string> customerreceipt, List<string> merchantreceipt, bool signatureneeded)
            {
                throw new NotImplementedException();
            }
            public object onTerminalReConnectEvent(string instruction)
            {
                return null;
            }

            public object onTerminalCancelEvent(string instruction)
            {
                return null;
            }

            public object onTerminalDeclinedEvent(string instruction)
            {
                return null;
            }

            public object onBamboraOutOfBalance(string message)
            {
                return null;
            }

            public object onBamboraSignatureRequired(string message)
            {
                return null;
            }
        }


        internal void onTerminalInfo(string t)
        {
            infoWindow.AppendLine(t);
            updateInfoWindow();
        }

        internal void onTerminalResult(PaymentTransactionType transType, bool success, string cashierText)
        {
            infoWindow.AppendLine(cashierText);
            updateInfoWindow();
        }


        internal void onTerminalBatchResult(PaymentDataType paymentDataType, IList<string> resultValue)
        {
            resultValue.ToList().ForEach(rv => infoWindow.AppendLine(rv));
            updateInfoWindow();
        }



        internal void onTerminalTransactionResult(PaymentDataType paymentDataType, IList<string> resultValue)
        {
            resultValue.ToList().ForEach(rv => infoWindow.AppendLine(rv));
            updateInfoWindow();
        }

        internal void HandleClosing()
        {
            if (paymentDevice != null) // paymentdevice is not set to NONE
            {
                // Register an event handler
                paymentDevice.UnRegisterEventHandler();
                // Connect to device if its not already connected
            }
        }
    }
}
