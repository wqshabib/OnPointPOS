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
    public class PaymentTerminalClosePresenter
    {
        private IPaymentTerminalUtilView view;
        private IPaymentDevice paymentDevice;
        private StringBuilder infoWindow = new StringBuilder();


        public PaymentTerminalClosePresenter(IPaymentTerminalUtilView view)
        {
            this.view = view;
            PosState state = PosState.GetInstance();
            paymentDevice = state.PaymentDevice;
            if (paymentDevice != null) // paymentdevice is not set to NONE
            {
                // Register an event handler
                paymentDevice.RegisterEventHandler(new TerminalCloseEventHandler(this, view));

                // Connect to device if its not already connected



            }
        }


        public void PrintCloseTerminal()
        {
            updateInfoWindow();
            paymentDevice.CloseBatch();
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



        internal void onTerminalInfo(string t)
        {
            infoWindow.AppendLine(t);
            updateInfoWindow();
        }

        internal void onTerminalResult(PaymentTransactionType transType, bool success, string cashierText)
        {
            infoWindow.AppendLine(cashierText);
            if (success == false)
            {
                App.MainWindow.ShowError("Dagsavslut för terminal", cashierText); //Fel vid
            }
            // updateInfoWindow();
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

        /// <summary>
        /// Event Handler class
        /// </summary>
        public class TerminalCloseEventHandler : IPaymentDeviceEventHandler
        {
            private PaymentTerminalClosePresenter paymentPresenter;
            //private IPaymentTerminalUtilView view;
            IList<string> resultValue = new List<string>();

            public TerminalCloseEventHandler(PaymentTerminalClosePresenter paymentPresenter, IPaymentTerminalUtilView view)
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
                //  paymentPresenter.onTerminalInfo(t);
                App.MainWindow.ShowError("Dagsavslut för terminal", t);//Fel vid 
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

            public object onBamboraEvent(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, PaymentTransactionType transType, bool success, string cashierText, List<string> customerreceipt, List<string> merchantreceipt, bool signatureneeded, string productName, double total)
            {
                if (paymentDataItemType == PaymentDataItemType.END)
                {
                    paymentPresenter.onTerminalResult(transType, success, cashierText);
                    resultValue = customerreceipt;
                }
                return null;
            }

            public object onTerminalReceiptData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value, double total)
            {
                if (paymentDataType == PaymentDataType.BATCH_CLOSE)
                {
                    if (paymentDataItemType == PaymentDataItemType.END)
                    {
                        //paymentPresenter.onTerminalReceiptResult(paymentDataType, resultValue);
                        resultValue = new List<string>();
                    }
                    else
                        resultValue.Add((!String.IsNullOrEmpty(header) ? header.PadRight(20) + "\t" : "") + formatValue(value));
                }
                return null;
            }

            public object onTerminalBatchData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value)
            {
                if (paymentDataType == PaymentDataType.BATCH_CLOSE)
                {
                    if (paymentDataItemType == PaymentDataItemType.END)
                    {
                        paymentPresenter.onTerminalBatchResult(paymentDataType, resultValue);
                        resultValue = new List<string>();
                    }
                    else
                        resultValue.Add((!String.IsNullOrEmpty(header) ? header.PadRight(20) + "\t" : "") + formatValue(value));
                }
                return null;
            }

            public object onTransactionData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value)
            {
                if (paymentDataType == PaymentDataType.BATCH_CLOSE)
                {
                    if (paymentDataItemType == PaymentDataItemType.END)
                    {
                        paymentPresenter.onTerminalTransactionResult(paymentDataType, resultValue);
                        resultValue = new List<string>();
                    }
                    else
                        resultValue.Add((!String.IsNullOrEmpty(header) ? header.PadRight(20) + "\t" : "") + formatValue(value));
                }
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

        internal void UnRegisterEvent()
        {
            paymentDevice.UnRegisterEventHandler();
        }
    }
}
