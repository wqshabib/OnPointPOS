using BPTI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace POSSUM.Integration.PaymentDevices.BabsBpti
{
    public class BabsBpiApi
    {

        #region BpTiConstants
        //==============================
        //Constanst for start method
        public enum TransactionTypes
        {
            OTHER = 0,
            LPP_SIGNATUREPURCHASE = 4000,
            LPP_PURCHASE = 4352,
            LPP_REFUND = 4353,
            LPP_REVERSAL = 4354,
            LPP_CLOSEBATCH = 4358
        };
        public enum ResultDataTypes
        {
            rdCUSTOMERRECEIPT = 1,
            rdMERCHANTRECEIPT,
            rdCLOSEBATCHRESULT,
            rdTRMCONFIG,
            rdCURRENTBATCH,
            rdTRANSLOGDETAILED,
            rdTRANSLOGTOTALS,
            rdUNSENTTRANS,
            rdUNDEFINED = -1
        };
        public enum ReceiptItems
        {
            riName = 1,
            riAddress = 2,
            riPostAddress = 3,
            riPhone = 4,
            riOrgNbr = 5,
            riBank = 6,
            riMerchantNbr = 7,
            riTrmId = 8,
            riTime = 9,
            riAmount = 10,
            riTotal = 11,
            riVAT = 12,
            riCashBack = 13,
            riCardNo = 14,
            riCardNoMasked = 15,
            riExpDate = 16,
            riCardProduct = 17,
            riAccountType = 18,
            riAuthInfo = 19,
            riReferenceNo = 20,
            riAcceptanceText = 21,
            riIdLine = 22,
            riSignatureLine = 23,
            riRefundAcceptanceText = 24,
            riCashierSignatureText = 25,
            riCashierNameText = 26,
            riTxnType = 27,
            riSaveReceipt = 28,
            riCustomerEx = 29,
            riPinUsed = 30,
            riEnd = -1,
        };

        public enum ResultDataValues { rdEnd = -1 };
        #endregion



        private string paymentDeviceTypeConnectionString;
        static readonly object _locker = new object();
        private int cookie;
        private CoBpTiX2 api;
        private bool isConnected;
        private IPaymentDeviceEventHandler eventHandler = null;
        public bool transactionStarted;
        public Dispatcher uiDispatcher { get; set; }
        public Dispatcher terminalDispatcher { get; set; }
        BabsEventHandler babsEventHandler = null;

        public static String TAG = "BabsBpiApi: ";
        private TransactionTypes transactionType;

        /*

          
C:\Users\frjo1.LUQON\Source\Workspaces\POS\POSSUM\POSSUM\Integration\P
aymentDevices\BabsBpti>tlbimp BPTI.dll /out:BPTI /out:BPTIInterop.dll
Microsoft (R) .NET Framework Type Library to Assembly Converter 4.0.30319.33440
Copyright (C) Microsoft Corporation.  All rights reserved.

TlbImp : Type library imported to C:\Users\frjo1.LUQON\Source\Workspaces\POS\Luq
onRetail\POSSUM\Integration\PaymentDevices\BabsBpti\BPTIInterop.dll          
          
         
        */
        public BabsBpiApi(string paymentDeviceTypeConnectionString, Dispatcher dispatcher)
        {
            this.LastStatus = 0;
            this.uiDispatcher = dispatcher;
            this.terminalDispatcher = Dispatcher.CurrentDispatcher;
            this.paymentDeviceTypeConnectionString = paymentDeviceTypeConnectionString;
            this.isConnected = false;

            this.api = new CoBpTiX2();


            IConnectionPointContainer icpc = (IConnectionPointContainer)api;
            IConnectionPoint icp;
            Guid IID_ICoBpTiX2Events = typeof(ICoBpTiX2Events).GUID;

            icpc.FindConnectionPoint(ref IID_ICoBpTiX2Events, out icp);
            babsEventHandler = new BabsEventHandler(this, eventHandler);
            icp.Advise(babsEventHandler, out cookie);
            Debug.WriteLine(TAG + "Constructor");
            IntegrationLogWriter.BabsLogWrite(string.Format("Constructor"));



            Connect();
        }

        public class BabsEventHandler : ICoBpTiX2Events
        {
            private BabsBpiApi babsBpiApi;
            private IPaymentDeviceEventHandler eventHandler;

            public BabsEventHandler(BabsBpiApi babsBpiApi, IPaymentDeviceEventHandler eventHandler)
            {
                this.babsBpiApi = babsBpiApi;
                this.eventHandler = eventHandler;
            }


            //-----------------------------------------------------------------------
            // Occurs only if bonus is activated and if track 3 is present on a 
            // magstripe card or track 1 is present in a chip card.
            // NOTE! Track 2 or its cardnumber is never sent to sales application.
            //-----------------------------------------------------------------------
            public void cardDataEvent(ref string text, ref string cardNo, ref string expDate, ref string track2)
            {
                //throw new NotImplementedException();
                Debug.WriteLine(TAG + "REC (cardDataEvent)  {0} {1} {2} {3}", text, cardNo, expDate, track2);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (cardDataEvent)  {0} {1} {2} {3}", text, cardNo, expDate, track2));
                return;

            }

            /// <summary>
            /// exceptionEvent - BPTI event that occures to inform cashier that an exception has occured in BPTI.
            /// </summary>
            /// <param name="text">A describing text of the exception.</param>
            /// <param name="code">A code for the exception.</param>
            public void exceptionEvent(ref string text, int code)
            {
                //throw new NotImplementedException();
                Debug.WriteLine(TAG + "REC (exceptionEvent) {0} {1}", text, code);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (exceptionEvent) {0} {1}", text, code));
                babsBpiApi.isConnected = false;
                return;

            }

            /// <summary>
            /// infoEvent - BPTI event that occures to inform cashier with a text.
            /// </summary>
            /// <param name="text">Text to be shown for cashier</param>
            public void infoEvent(ref string text)
            {
                String t = text;
                Debug.WriteLine(TAG + "REC (infoEvent) {0}", text);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (infoEvent) {0}", text));
                Action a = () => eventHandler.onInfoEvent(t);

                dispatchToUi(a);

            }

            /// <summary>
            ///  lppCmdFailedEvent - BPTI event that occures when terminal reports a failure. 
            /// </summary>
            /// <param name="cmd">LPP tag, and not neccessarily the message, that contains an error code.</param>
            /// <param name="code">LPP error code, Most imortant is 1011, meaning; do close batch.</param>
            /// <param name="text">Text description och LPP error code.</param>
            public void lppCmdFailedEvent(int cmd, int code, ref string text)
            {
                String t = text;
                Action a = () => eventHandler.onInfoEvent(t);

                Debug.WriteLine(TAG + "REC (lppCmdFailedEvent) {0} {1} {2}", cmd, code, text);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (lppCmdFailedEvent) {0} {1} {2}", cmd, code, text));

                dispatchToUi(a);

                int xcode = code;

                if (code > 2000 && code < 3000)             // Card complaint. Give customer a new chance.
                    xcode = 2000;
                if (code > 3000 && code < 4000)
                    xcode = 3000;


                switch (xcode)
                {
                    case 1011:
                        // Already dispatched
                        Action a3 = () => eventHandler.onTerminalNeedClose();
                        dispatchToUi(a3);
                        break;
                    case 2000:             // Give customer a new chance by doing nothing.
                        break;
                    case 3000:             // Some kind of amount failure, Restart transaction.
                        babsBpiApi.api.end();
                        Action a2 = () => eventHandler.onTerminalNeedClose();
                        dispatchToUi(a2);
                        //api.start(currentTransType);
                        break;
                    case -1:
                        //this.babsBpiApi.isConnected = false;
                        break;

                }


                dispatchToUi(a);
                //throw new NotImplementedException();
                Action a4 = () => eventHandler.onTerminalNeedClose();
                dispatchToUi(a4);
                return;

            }

            public void paymentCodeEvent(ref string text)
            {
                Debug.WriteLine(TAG + "REC (paymentCodeEvent) {0} ", text);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (paymentCodeEvent) {0} ", text));

                //throw new NotImplementedException();
                return;

            }

            /// <summary>
            /// referralEvent - BPTI event that occures when cashier is supposed to make a call for authorisation.
            /// Show text and enable cashier to enter an authorisation code.
            /// If cashier chooses to authorize sale without calling, let program enter
            /// "9999".
            /// This event must be followed by a call to sendApprovalCode() or 
            /// end()/endTransaction().
            /// </summary>
            /// <param name="text">Text to show. Includes the phone number to call for authorisation.</param>
            public void referralEvent(ref string text)
            {
                Debug.WriteLine(TAG + "REC (referralEvent) {0}", text);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (referralEvent) {0}", text));

                String instruction = text;
                Action a = () => eventHandler.onTerminalReferralEvent(instruction);
                //throw new NotImplementedException();
                dispatchToUi(a);
                return;

            }

            public void resultDataEvent(int resultType, int item, ref string description, ref string Value)
            {
                Debug.WriteLine(TAG + "REC (resultDataEvent) {0} {1} {2} {3}", resultType, item, description, Value);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (resultDataEvent) {0} {1} {2} {3}", resultType, item, description, Value));
                String desc = description;
                String val = Value;

                Action a = null;
                switch ((ResultDataTypes)resultType)
                {
                    case ResultDataTypes.rdCUSTOMERRECEIPT:
                    case ResultDataTypes.rdMERCHANTRECEIPT:
                        a = () => eventHandler.onTerminalReceiptData(getResultType(resultType), getItem(item), getDescription(desc), getValue(val), 0);
                        //receiptData(resultType, item, description, value);
                        break;
                    case ResultDataTypes.rdCLOSEBATCHRESULT:
                    case ResultDataTypes.rdCURRENTBATCH:
                        a = () => eventHandler.onTerminalBatchData(getResultType(resultType), getItem(item), getDescription(desc), getValue(val));
                        //closeBatchData(item, description, value);
                        break;
                    case ResultDataTypes.rdTRMCONFIG:
                    case ResultDataTypes.rdTRANSLOGDETAILED:
                    case ResultDataTypes.rdTRANSLOGTOTALS:
                    case ResultDataTypes.rdUNSENTTRANS:
                        a = () => eventHandler.onTransactionData(getResultType(resultType), getItem(item), getDescription(desc), getValue(val));
                        //trmConfig(item, description, value);
                        break;
                }
                if (a != null)
                {
                    dispatchToUi(a);

                }
                //throw new NotImplementedException();
                return;

            }

            private PaymentDataType getResultType(int resultType)
            {
                switch ((ResultDataTypes)resultType)
                {
                    case ResultDataTypes.rdCUSTOMERRECEIPT:
                        return PaymentDataType.CUSTOMER_RECEIPT;
                    case ResultDataTypes.rdMERCHANTRECEIPT:
                        return PaymentDataType.MERCHANT_RECEIPT;
                    case ResultDataTypes.rdCLOSEBATCHRESULT:
                        return PaymentDataType.BATCH_CLOSE;
                    case ResultDataTypes.rdCURRENTBATCH:
                        return PaymentDataType.BATCH_CURRENT;
                    case ResultDataTypes.rdTRMCONFIG:
                        return PaymentDataType.CONFIG;
                    case ResultDataTypes.rdTRANSLOGDETAILED:
                        return PaymentDataType.TRANSACTION_LOG_DETAILED;
                    case ResultDataTypes.rdTRANSLOGTOTALS:
                        return PaymentDataType.TRANSACTION_LOG_TOTALS;
                    case ResultDataTypes.rdUNSENTTRANS:
                        return PaymentDataType.TRANSACTION_UNSENT;
                    default:
                        return PaymentDataType.UNKNOWN;
                }
            }

            private PaymentDataItemType getItem(int item)
            {
                if (item == -1)
                {
                    return PaymentDataItemType.END;
                }
                else
                {
                    return PaymentDataItemType.UNKNOWN;
                }
            }

            private string getDescription(string description)
            {
                return description;
            }

            private string getValue(string value)
            {
                return value;
            }

            public void statusChangeEvent(int newStatus)
            {
                Debug.WriteLine(TAG + "REC (statusChangeEvent) {0}", newStatus);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (statusChangeEvent) {0}", newStatus));
                babsBpiApi.LastStatus = newStatus;
                Action a;
                switch (newStatus)
                {
                    case 0:
                        a = () => eventHandler.onStatusChanged(connected: false, open: false);

                        dispatchToUi(a);


                        //eventsList.Items.Add("Terminalen frånkopplad.");
                        //closeButton.Text = "Öppna";
                        //initButton.Text = "Återanslut";
                        break;

                    case 1:
                        a = () => eventHandler.onStatusChanged(connected: true, open: false);

                        dispatchToUi(a);


                        //eventsList.Items.Add("Terminal ansluten och klar att användas.");
                        //initButton.Text = "Koppla ifrån";
                        break;

                    case 2:
                        a = () => eventHandler.onStatusChanged(connected: true, open: true);

                        dispatchToUi(a);



                        //eventsList.Items.Add("Status öppen.");
                        //opened = true;
                        //closeButton.Text = "Stäng";
                        break;

                    case 3:
                        a = () => eventHandler.onStatusChanged(connected: true, open: false);

                        dispatchToUi(a);


                        //eventsList.Items.Add("Status stängd.");
                        //opened = false;
                        //closeButton.Text = "Öppna";
                        //transactionStarted = false;
                        //currentTransType = -1;
                        break;

                    case 4:
                        this.babsBpiApi.transactionStarted = true;
                        a = () => eventHandler.onTransactionStart();

                        dispatchToUi(a);


                        //eventsList.Items.Add("Transaktion startad.");
                        //transactionStarted = true;
                        //currentTransType = pendingTransType;
                        break;

                    case 5:
                        this.babsBpiApi.transactionStarted = false;
                        a = () => eventHandler.onTransactionEnd();

                        dispatchToUi(a);


                        //eventsList.Items.Add("Transaktion avslutad.");
                        //transactionStarted = false;
                        //currentTransType = -1;
                        break;

                    case 6:
                        //eventsList.Items.Add("statusChangeEvent med värde=" + new string('0', newStatus));
                        break;

                    case 7:
                        a = () => eventHandler.onStatusChanged(connected: false, open: false);

                        dispatchToUi(a);

                        babsBpiApi.isConnected = false;
                        babsBpiApi.Connect();

                        //eventsList.Items.Add("Kommunikation med terminalen bruten. Anropa connect()?");
                        //transactionStarted = false;
                        //currentTransType = -1;
                        //opened = false;
                        //closeButton.Text = "Öppna";
                        //initButton.Text = "Återanslut";
                        break;
                }

            }

            public void terminalDspEvent(ref string row1, ref string row2, ref string row3, ref string row4)
            {
                Debug.WriteLine(TAG + "REC (terminalDspEvent) {0} {1} {2} {3}", row1, row2, row3, row4);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (terminalDspEvent) {0} {1} {2} {3}", row1, row2, row3, row4));

                String txtrow1 = row1;
                String txtrow2 = row2;
                String txtrow3 = row3;
                String txtrow4 = row4;
                Action a = () => eventHandler.onTerminalDisplay(txtrow1, txtrow2, txtrow3, txtrow4, null, null, null);

                dispatchToUi(a);

            }

            public void terminatedEvent(ref string reason, int reasonCode)
            {
                Debug.WriteLine(TAG + "REC (terminatedEvent) {0} {1}", reason, reasonCode);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (terminatedEvent) {0} {1}", reason, reasonCode));
                //throw new NotImplementedException();
                return;
            }

            /// <summary>
            /// txnResultEvent - BPTI event that occurs when a transaction result is ready in BPTI.
            /// The event occures for purchase, refund, reversal, closebatch and test host connection.
            /// </summary>
            /// <param name="txnType">Type of transaction the reply applies for.</param>
            /// <param name="resultCode">0=transaction successful, 
            /// 4003=Close batch indicates a difference between host and terminal.
            /// All other values indicates the transaction was unsuccessful.
            /// </param>
            /// <param name="text">Text to be shown for cashier.</param>
            public void txnResultEvent(int txnType, int resultCode, ref string text, ref string clearingCompany)
            {
                Debug.WriteLine(TAG + "REC (txnResultEvent) {0} {1} {2}", txnType, resultCode, text);
                IntegrationLogWriter.BabsLogWrite(string.Format("REC (txnResultEvent) {0} {1} {2}", txnType, resultCode, text));

                String cashierText = text;

                PaymentTransactionType transType = PaymentTransactionType.UNKNOWN;
                switch ((TransactionTypes)txnType)
                {
                    case TransactionTypes.LPP_REFUND:
                        transType = PaymentTransactionType.REFUND;
                        break;
                    case TransactionTypes.LPP_SIGNATUREPURCHASE:
                        transType = PaymentTransactionType.PURCHASE;
                        break;
                    case TransactionTypes.LPP_PURCHASE:
                        transType = PaymentTransactionType.PURCHASE;
                        break;
                    case TransactionTypes.LPP_REVERSAL:
                        transType = PaymentTransactionType.REVERSAL;
                        break;
                    case TransactionTypes.LPP_CLOSEBATCH:
                        transType = PaymentTransactionType.BATCH;
                        break;
                    default:
                        transType = PaymentTransactionType.UNKNOWN;
                        break;
                }

                Action a = () => eventHandler.onTerminalResultEvent(transType, resultCode == 0, cashierText);

                dispatchToUi(a);

                if (resultCode == 0)
                {
                    // Payment Successful
                    // Internally handle the next step
                    if (txnType == (int)TransactionTypes.LPP_SIGNATUREPURCHASE ||
                                    txnType == (int)TransactionTypes.LPP_PURCHASE ||
                                        txnType == (int)TransactionTypes.LPP_REFUND ||
                                        txnType == (int)TransactionTypes.LPP_REVERSAL)
                    {
                        this.babsBpiApi.apiMerchantReceipt();
                        //                        this.babsBpiApi.apiCustomerReceipt();
                    }
                    else if (txnType == (int)TransactionTypes.LPP_CLOSEBATCH)
                    { 
                        this.babsBpiApi.apiBatchReport();
                        this.babsBpiApi.Disconnect();
                    }
                }
                else
                {
                    // Internally handle the next step
                    if (resultCode == 0)
                    {
                        this.babsBpiApi.apiCustomerReceipt();
                    }
                    else if (resultCode == 4002 &&
                        ((TransactionTypes)txnType == TransactionTypes.LPP_PURCHASE ||
                        (TransactionTypes)txnType == TransactionTypes.LPP_REFUND))
                    {
                        this.babsBpiApi.apiCustomerReceipt();
                    }
                    else if (resultCode == 4002 &&
                        (TransactionTypes)txnType == TransactionTypes.LPP_CLOSEBATCH
                        )
                    {
                        this.babsBpiApi.apiBatchReport();
                    }
                    Action a2 = () => eventHandler.onTerminalNeedClose();
                    dispatchToUi(a2);
                    this.babsBpiApi.EndTransaction();
                }
                return;

            }

            private void dispatchToUi(Action a)
            {
                if (this.eventHandler != null)
                {
                    this.babsBpiApi.uiDispatcher.Invoke(a);
                }
            }

            internal void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
            {
                Debug.WriteLine(TAG + "RegisterEventHandler");
                IntegrationLogWriter.BabsLogWrite(string.Format("RegisterEventHandler"));

                if (this.eventHandler != null)
                {
                    throw new Exception("eventHandler already registered");
                }
                this.eventHandler = eventHandler;
            }

            internal void UnRegisterEventHandler()
            {
                Debug.WriteLine(TAG + "UnRegisterEventHandler");
                IntegrationLogWriter.BabsLogWrite(string.Format("UnRegisterEventHandler"));

                this.eventHandler = null;
            }
        }


        #region Connect/Disconnect

        public bool IsConnected()
        {
            return this.isConnected;
        }

        public bool Connect()
        {
            try
            {
                if (isConnected)
                {
                    return false;
                }

                Uri uri = new Uri(paymentDeviceTypeConnectionString);
                if (uri.Scheme.ToLower() == "babstcp")
                {
                    string host = uri.Host;
                    int port = uri.Port;
                    Debug.WriteLine(TAG + "SND (initLan) {0} {1}", host, port);
                    IntegrationLogWriter.BabsLogWrite(string.Format("SND (initLan) {0} {1}", host, port));

                    api.initLan(host, port);
                    api.connect();
                    isConnected = true;
                }
                return isConnected;
            }
            catch (Exception ex)
            {
                MessageBox.Show("paymentDeviceTypeConnectionString = " + paymentDeviceTypeConnectionString + ", Exception = " + ex.ToString());
                return false;
            }
        }

        public bool Disconnect()
        {
            if (IsConnected())
            {
                Debug.WriteLine(TAG + "SND (Disconnect)");
                IntegrationLogWriter.BabsLogWrite(string.Format("SND (Disconnect)"));
                api.disconnect();
                isConnected = false;

            }
            return true;
        }

        #endregion

        internal void StartTransaction(TransactionTypes transactionType)
        {
            Debug.WriteLine(TAG + "SND (start[Transaction]) {0}", transactionType);
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (start[Transaction]) {0}", transactionType));
            this.transactionType = transactionType;
            api.start((int)transactionType);
            /*
            Action a = () => {
                if(!transactionStarted)
                {
                    api.start((int)transactionType);
                }

            };
            uiDispatcher.Invoke(a);*/
        }


        internal void EndTransaction()
        {
            Debug.WriteLine(TAG + "SND (endTransaction)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (endTransaction)"));

            api.endTransaction();
        }

        internal void SendAmount(decimal debitCardAmount, decimal vat, decimal cashback)
        {
            int amount = (int)Math.Round(debitCardAmount * 100, 0);
            int vatamount = (int)Math.Round(vat * 100, 0);
            int cbAmount = (int)Math.Round(cashback * 100, 0);
            //For test return case :done by Arshad

            //if (transactionType==TransactionTypes.LPP_REFUND && cashback > 0)
            //{
            //    amount = cbAmount;
            //    cbAmount = 0;
            //}
            Debug.WriteLine(TAG + "SND (sendAmounts) {0} {1} {2}", amount, vatamount, cbAmount);
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (sendAmounts) {0} {1} {2}", amount, vatamount, cbAmount));

            api.sendAmounts(amount, vatamount, cbAmount);
            /*
                        Action a = () =>
                        {

                        };
                        uiDispatcher.Invoke(a);*/
        }

        /// <summary>
        /// Called from other thread
        /// </summary>
        internal void apiMerchantReceipt()
        {
            Debug.WriteLine(TAG + "SND (merchantReceipt)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (merchantReceipt)"));
            api.merchantReceipt();
        }

        internal void apiCustomerReceipt()
        {
            Debug.WriteLine(TAG + "SND (customerReceipt)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (customerReceipt)"));
            api.customerReceipt();
        }

        /// <summary>
        /// Called from other thread
        /// </summary>
        internal void apiBatchReport()
        {
            Debug.WriteLine(TAG + "SND (batchReport)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (batchReport)"));
            api.batchReport();
        }


        internal void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
        {
            if (this.eventHandler != null)
            {
                throw new Exception("eventHandler already registered");
            }
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (customerReceipt)"));
            this.eventHandler = eventHandler;
            babsEventHandler.RegisterEventHandler(this.eventHandler);
        }

        internal void UnRegisterEventHandler()
        {
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (customerReceipt)"));
            this.eventHandler = null;
            babsEventHandler.UnRegisterEventHandler();
        }

        internal void SendReferralCode(string authCode)
        {
            Debug.WriteLine(TAG + "SND (sendApprovalCode) {0}", authCode);
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (sendApprovalCode) {0}", authCode));
            api.sendApprovalCode(authCode);
        }

        public volatile int LastStatus;

        internal void TestConnection()
        {
            Debug.WriteLine(TAG + "SND (testHostConnection)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (testHostConnection)"));
            api.testHostConnection();
        }

        internal void TerminalConfig()
        {
            Debug.WriteLine(TAG + "SND (unsentTransactions)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (unsentTransactions)"));
            api.terminalConfig();
        }

        internal void UnsentTransactions()
        {
            Debug.WriteLine(TAG + "SND (unsentTransactions)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (unsentTransactions)"));
            api.unsentTransactions();
        }

        internal void TransLogByNr(int type, int inputNumber)
        {
            Debug.WriteLine(TAG + "SND (transLogByNbr) {0} {1}", type, inputNumber);
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (transLogByNbr) {0} {1}", type, inputNumber));
            api.transLogByNbr(type, inputNumber);
        }

        internal void CloseBatch()
        {
            Debug.WriteLine(TAG + "SND (start((int)TransactionTypes.LPP_CLOSEBATCH)");
            IntegrationLogWriter.BabsLogWrite("SND (start((int)TransactionTypes.LPP_CLOSEBATCH)");
            api.start((int)TransactionTypes.LPP_CLOSEBATCH);
        }
        internal void ResetConnection()
        {

            Debug.WriteLine(TAG + "SND (Need to implement reset)");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (Need to implement reset)"));
            api.open();
            api.connect();
            // api.close();
            //   UnRegisterEventHandler();
            //  api.end();
            //  api.connect();            
            //   api.start((int)TransactionTypes.OTHER);
        }
        internal void Cancel()
        {
            Debug.WriteLine(TAG + "SND (cancel())");
            IntegrationLogWriter.BabsLogWrite(string.Format("SND (cancel())"));

            IntegrationLogWriter.LogWrite("Cancel API Call=>  " + DateTime.Now  + "  Datail = >  "+ System.Environment.StackTrace);

            api.cancel();
        }
    }
}
