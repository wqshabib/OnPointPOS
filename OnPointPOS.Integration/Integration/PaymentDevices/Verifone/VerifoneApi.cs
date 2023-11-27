//using System;
//using System.Diagnostics;
//using System.Windows.Threading;
//using POSSUM.Sockets;

//namespace POSSUM.Integration.PaymentDevices.Verifone
//{
//    public class VerifoneApi : SocketClient
//    {
//        public static string TAG = "VerifoneApi: ";
//        public volatile int LastStatus;
//        public bool transactionStarted;
//        private static readonly object locker = new object();
//        private readonly string paymentDeviceTypeConnectionString;
//        private readonly VerifoneEventHandler verifoneEventHandler;

//        private IPaymentDeviceEventHandler eventHandler;
//        private TransactionTypes transactionType;

//        public VerifoneApi(string host, int port) : base(host, port)
//        {
//        }

//        public VerifoneApi(string paymentDeviceTypeConnectionString, Dispatcher dispatcher)
//        {
//            LastStatus = 0;
//            uiDispatcher = dispatcher;
//            terminalDispatcher = Dispatcher.CurrentDispatcher;
//            this.paymentDeviceTypeConnectionString = paymentDeviceTypeConnectionString;
//            isConnected = false;

//            verifoneEventHandler = new VerifoneEventHandler(this, eventHandler);

//            Debug.WriteLine(TAG + "Constructor");

//            Connect();
//        }

//        public enum PaymentStep
//        {
//            INSERT_CARD,
//            TERMINAL_CONNECTED,
//            REFERRAL_REQUEST
//        }

//        public enum ReceiptItems
//        {
//            riName = 1,
//            riAddress = 2,
//            riPostAddress = 3,
//            riPhone = 4,
//            riOrgNbr = 5,
//            riBank = 6,
//            riMerchantNbr = 7,
//            riTrmId = 8,
//            riTime = 9,
//            riAmount = 10,
//            riTotal = 11,
//            riVAT = 12,
//            riCashBack = 13,
//            riCardNo = 14,
//            riCardNoMasked = 15,
//            riExpDate = 16,
//            riCardProduct = 17,
//            riAccountType = 18,
//            riAuthInfo = 19,
//            riReferenceNo = 20,
//            riAcceptanceText = 21,
//            riIdLine = 22,
//            riSignatureLine = 23,
//            riRefundAcceptanceText = 24,
//            riCashierSignatureText = 25,
//            riCashierNameText = 26,
//            riTxnType = 27,
//            riSaveReceipt = 28,
//            riCustomerEx = 29,
//            riPinUsed = 30,
//            riEnd = -1
//        }

//        public enum ResultDataTypes
//        {
//            rdCUSTOMERRECEIPT = 1,
//            rdMERCHANTRECEIPT,
//            rdCLOSEBATCHRESULT,
//            rdTRMCONFIG,
//            rdCURRENTBATCH,
//            rdTRANSLOGDETAILED,
//            rdTRANSLOGTOTALS,
//            rdUNSENTTRANS,
//            rdUNDEFINED = -1
//        }

//        public enum ResultDataValues
//        {
//            rdEnd = -1
//        }

//        //==============================
//        //Constanst for start method
//        public enum TransactionTypes
//        {
//            OTHER = 0,
//            LPP_SIGNATUREPURCHASE = 4000,
//            LPP_PURCHASE = 4352,
//            LPP_REFUND = 4353,
//            LPP_REVERSAL = 4354,
//            LPP_CLOSEBATCH = 4358
//        }

//        public Dispatcher terminalDispatcher { get; set; }

//        public Dispatcher uiDispatcher { get; set; }

//        public Response Close()
//        {
//            if (isOpen)
//            {
//                var cmd = new Command("close");

//                var result = Send(cmd);

//                LastStatus = 3;
//            }

//            return null;
//        }

//        public void CloseBatch()
//        {
//            Debug.WriteLine(TAG + "SND (start((int)TransactionTypes.LPP_CLOSEBATCH)");

//            if (!isOpen)
//            {
//                Connect();
//                Open();
//            }

//            var cmd = new Command("closebatch");

//            Send(cmd, false);

//            var result = GetResult("<data>", "</data>", true);

//            var response = GetResponse(result);

//            if (response?.Ok == true)
//            {
//                Close();
//            }

//            var batch = response.GetBatch();

//            Action a = () => eventHandler.onVerifoneEvent(PaymentDataType.BATCH_CLOSE, PaymentDataItemType.END, PaymentTransactionType.BATCH, true, response.result.merchantname, batch, batch, false);

//            dispatchToUi(a);
//        }

//        public bool Disconnect()
//        {
//            if (IsConnected())
//            {
//                Debug.WriteLine(TAG + "SND (Disconnect)");
//                isConnected = false;
//            }

//            LastStatus = 7;

//            return true;
//        }

//        public bool IsConnected()
//        {
//            return isConnected;
//        }

//        public void SendAmount(decimal debitCardAmount, decimal vat, decimal cashback)
//        {
//            int amount = (int)Math.Round(debitCardAmount * 100, 0);
//            int vatamount = (int)Math.Round(vat * 100, 0);
//            int cbAmount = (int)Math.Round(cashback * 100, 0);

//            Transaction transaction = null;

//            switch (transactionType)
//            {
//                case TransactionTypes.LPP_PURCHASE:
//                    transaction = new Transaction
//                    {
//                        vat = vatamount,
//                        cashback = cbAmount,
//                        amount = amount,
//                        type = "purchase"
//                    };

//                    HandlePurchase(transaction);

//                    break;

//                case TransactionTypes.LPP_REFUND:
//                    transaction = new Transaction
//                    {
//                        vat = vatamount,
//                        type = "refund",
//                        amount = cbAmount,
//                        cashback = 0
//                    };

//                    HandleRefund(transaction);
//                  //  EndTransaction();

//                    break;
//            }

//            Debug.WriteLine(TAG + "SND (sendAmounts) {0} {1} {2}", amount, vatamount, cbAmount);
//        }

//        /// <summary>
//        /// Called from other thread
//        /// </summary>
//        internal void apiBatchReport()
//        {
//            Debug.WriteLine(TAG + "SND (batchReport)");
//        }

//        internal void apiCustomerReceipt()
//        {
//            Debug.WriteLine(TAG + "SND (customerReceipt)");
//        }

//        /// <summary>
//        /// Called from other thread
//        /// </summary>
//        internal void apiMerchantReceipt()
//        {
//            Debug.WriteLine(TAG + "SND (merchantReceipt)");
//        }

//        internal void Cancel()
//        {
//            Debug.WriteLine(TAG + "SND (cancel())");
//        }

//        internal void EndTransaction()
//        {
//            switch (transactionType)
//            {
//                case TransactionTypes.LPP_PURCHASE:
//                    EndPurchase();
//                    break;

//                case TransactionTypes.LPP_REFUND:
//                    EndRefund();
//                    break;
//            }
//        }

//        internal void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
//        {
//            if (this.eventHandler != null)
//            {
//                throw new Exception("eventHandler already registered");
//            }

//            this.eventHandler = eventHandler;

//            verifoneEventHandler.RegisterEventHandler(this.eventHandler);
//        }

//        internal void ResetConnection()
//        {
//            Debug.WriteLine(TAG + "SND (Need to implement reset)");
//        }

//        internal void SendReferralCode(string authCode)
//        {
//            if (isOpen)
//            {
//                int code = 0;
//                if (int.TryParse(authCode, out  code))
//                {
//                    var approval = new Approval(code);

//                    var response = Send(approval);

//                    if (response.Ok)
//                    {
//                        var receipt = response.Receipt;

//                        if (receipt != null)
//                        {
//                            Action a = () => eventHandler.onVerifoneEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.PURCHASE, true, receipt.bank, receipt.GetMerchantReceipt(), receipt.GetMerchantReceipt(), receipt.NeedToSign);

//                            dispatchToUi(a);
//                        }
//                    }
//                }
//            }
//        }

//        internal void StartTransaction(TransactionTypes transactionType)
//        {
//            this.transactionType = transactionType;

//            Response response = null;

//            Debug.WriteLine(TAG + "SND (start[Transaction]) {0}", transactionType);

//            LastStatus = 3;

//            var result = Open();

//            if (isOpen)
//            {
//                eventHandler.onInfoEvent("Terminalen kopplad");

//                switch (transactionType)
//                {
//                    case TransactionTypes.LPP_PURCHASE:
//                        response = StartPurchase();
//                        break;

//                    case TransactionTypes.LPP_REFUND:
//                        response = StartRefund();
//                        break;

//                    case TransactionTypes.LPP_CLOSEBATCH:
//                        CloseBatch();
//                        break;
//                }
//            }
//        }

//        internal void TerminalConfig()
//        {
//            Debug.WriteLine(TAG + "SND (terminalConfig)");
//        }

//        internal void TestConnection()
//        {
//            Debug.WriteLine(TAG + "SND (testHostConnection)");
//        }

//        internal void TransLogByNr(int type, int inputNumber)
//        {
//            Debug.WriteLine(TAG + "SND (transLogByNbr) {0} {1}", type, inputNumber);
//        }

//        internal void UnRegisterEventHandler()
//        {
//            eventHandler = null;
//            verifoneEventHandler.UnRegisterEventHandler();
//        }

//        internal void UnsentTransactions()
//        {
//            Debug.WriteLine(TAG + "SND (unsentTransactions)");
//        }

//        private static string FormatResponse(string start, string end, string str)
//        {
//            if (!string.IsNullOrEmpty(str))
//            {
//                int first = str.LastIndexOf(start);
//                int last = str.LastIndexOf(end);

//                if (first >= 0 || last >= 1)
//                {
//                    str = str.Substring(first, (last - first) + end.Length);
//                }

//                return str.Trim();
//            }
//            else
//            {
//                return str;
//            }
//        }

//        private VerifoneBase Confirm(TransactionTypes type)
//        {
//            VerifoneBase request = null;

//            switch (type)
//            {
//                case TransactionTypes.LPP_PURCHASE:
//                    return Confirm("purchase");

//                case TransactionTypes.LPP_REFUND:
//                    return Confirm("refund");
//            }

//            return request;
//        }

//        private VerifoneBase Confirm(string type)
//        {
//            var cmd = new Command(type, "action", "confirm");

//            Send(cmd, false);

//            return GetResponse();
//        }

//        private bool Connect()
//        {
//            if (isConnected)
//            {
//                return false;
//            }

//            var uri = new Uri(paymentDeviceTypeConnectionString);

//            string host = uri.Host;
//            int port = uri.Port;
//            Debug.WriteLine(TAG + "SND (initLan) {0} {1}", host, port);

//            if (Connect(host, port))
//            {
//                var response = GetResponse();

//                if (response.IsConnected)
//                {
//                    isConnected = true;

//                    LastStatus = 1;
//                }
//            }

//            return isConnected;
//        }

//        private void dispatchToUi(Action a)
//        {
//            if (eventHandler != null)
//            {
//                uiDispatcher.Invoke(a);
//            }
//        }

//        private Response EndPurchase()
//        {
//            var cmd = new Command("purchase", "action", "end");
//            return Send(cmd);
//        }

//        private Response EndRefund()
//        {
//            var cmd = new Command("refund", "action", "end");

//            return Send(cmd);
//        }

//        private VerifoneBase GetResponse()
//        {
//            var str = GetResult("<request ", "</request>");

//            Response response = null;
//            Request request = null;
//            if (SerializeUtility.TryDeserialize(str, out request))
//            {
//                return request;
//            }
//            else
//            {
//                str = FormatResponse("<data>", "</data>", str);

//                if (SerializeUtility.TryDeserialize(str, out response))
//                {
//                    return response;
//                }
//            }

//            return null;
//        }

//        private Response GetResponse(string str)
//        {
//            Response response = null;
//            if (SerializeUtility.TryDeserialize(str, out response))
//            {
//                return response;
//            }

//            return null;
//        }

//        private string GetResult(string start, string end, bool reverse = false)
//        {
//            var str = Reveice();

//            int first = -1;
//            int last = -1;

//            if (!string.IsNullOrEmpty(str))
//            {
//                if (reverse)
//                {
//                    first = str.LastIndexOf(start);
//                    last = str.LastIndexOf(end);
//                }
//                else
//                {
//                    first = str.IndexOf(start);
//                    last = str.IndexOf(end);
//                }

//                if (first >= 0 || last >= 1)
//                {
//                    str = str.Substring(first, (last - first) + end.Length);
//                }

//                return str.Trim();
//            }
//            else
//            {
//                return str;
//            }
//        }

//        private void HandlePurchase(Transaction transaction)
//        {
//            VerifoneReceipt receipt = null;

//            var response = GetResponse();

//            if (response!=null && response.Ok)
//            {
//                UpdateDisplay(PaymentStep.INSERT_CARD);

//                var resp = GetResponse();

//                if (resp.Proceed || resp.ForceMag)
//                {
//                    var cmd = new Command(transaction);

//                    response = Send(cmd);

//                    if (response.Wait)
//                    {
//                        response = GetResponse();
//                    }

//                    if (!resp.ForceMag)
//                    {
//                        response = GetResponse();
//                    }

//                    if (response.IsReferral && response is Request)
//                    {
//                        var request = response as Request;

//                        Action a = () => eventHandler.onTerminalReferralEvent(request.referral.Value);

//                        dispatchToUi(a);
//                    }

//                    if (response.Ok)
//                    {
//                        receipt = response.Receipt;

//                        var res = Confirm(transactionType);

//                        if (receipt != null)
//                        {
//                            Action a = () => eventHandler.onVerifoneEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.PURCHASE, true, receipt.bank, receipt.GetMerchantReceipt(), receipt.GetMerchantReceipt(), receipt.NeedToSign);

//                            dispatchToUi(a);
//                        }
//                    }
//                    else
//                    {
//                        var failed = response.Receipt;
//                    }
//                }
//            }
//        }

//        private void HandleRefund(Transaction transaction)
//        {
//            VerifoneReceipt receipt = null;

//            var response = GetResponse();
//            //check it if it is null: Arshad
//            if (response != null && response.Proceed)
//            {
//                var cmd = new Command(transaction);

//                response = Send(cmd);

//                var request = GetResponse();

//                if (request.Proceed)
//                {
//                    UpdateDisplay(PaymentStep.INSERT_CARD);

//                    response = GetResponse();

//                    if (response.Receipt != null)
//                    {
//                        receipt = response.Receipt;
//                    }

//                    if (request.Proceed)
//                    {
//                        var res = Confirm(transactionType);

//                        if (receipt != null)
//                        {
//                            Action a = () => eventHandler.onVerifoneEvent(PaymentDataType.CUSTOMER_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.REFUND, true, receipt.bank, receipt.GetCustomerReceipt(), receipt.GetMerchantReceipt(), receipt.NeedToSign);

//                            dispatchToUi(a);
//                        }
//                    }
//                }
//            }
//        }

//        private Response Open()
//        {
//            if (isConnected)
//            {
//                var cmd = new Command("open");

//                var response = Send(cmd);

//                if (response.Ok)
//                {
//                    isOpen = true;

//                    LastStatus = 2;

//                    return response;
//                }

//                return response;
//            }
//            else
//            {
//                return null;
//            }
//        }

//        private VerifoneBase Send(Approval approval, bool returnResult = true)
//        {
//            var value = XmlUtils.ToXml(approval);

//            var str = Send(value, returnResult);

//            if (!string.IsNullOrEmpty(str))
//            {
//                var result = FormatResponse("<data>", "</data>", str);

//                return SerializeUtility.XmlDeserialize<VerifoneBase>(result);
//            }

//            return null;
//        }

//        private Response Send(Command cmd, bool returnResult = true)
//        {
//            var value = XmlUtils.ToXml(cmd);

//            var str = Send(value, returnResult);

//            if (!string.IsNullOrEmpty(str))
//            {
//                var result = FormatResponse("<data>", "</data>", str);

//                return SerializeUtility.XmlDeserialize<Response>(result);
//            }

//            return null;
//        }

//        private Response StartPurchase()
//        {
//            var cmd = new Command("purchase", "action", "start");

//            var response = Send(cmd);

//            if (response.Ok)
//            {
//                eventHandler.onInfoEvent("Öppen");
//            }

//            return response;
//        }

//        private Response StartRefund()
//        {
//            var cmd = new Command("refund", "action", "start");

//            if (isOpen)
//            {
//                var result = Send(cmd);

//                if (result.Ok)
//                {
//                    eventHandler.onInfoEvent("Öppen");
//                }

//                return result;
//            }
//            else
//            {
//                var res = Open();

//                if (isOpen)
//                {
//                    return Send(cmd);
//                }
//            }

//            return null;
//        }

//        private void UpdateDisplay(PaymentStep step, string message)
//        {
//            if (step == PaymentStep.REFERRAL_REQUEST)
//            {
//                Action a = () => eventHandler.onTerminalDisplay("         Kräver verifiering via telefon        ", "  " + message + "  ", " ", " ");

//                dispatchToUi(a);
//            }
//        }

//        private void UpdateDisplay(PaymentStep step)
//        {
//            if (step == PaymentStep.INSERT_CARD)
//            {
//                Action a = () => eventHandler.onTerminalDisplay("         ---1        ", "  SÄTT I / DRA KORT  ", " ", " ");

//                dispatchToUi(a);
//            }
//            else if (step == PaymentStep.TERMINAL_CONNECTED)
//            {
//                Action a = () => eventHandler.onTerminalDisplay("         ---1        ", "  SÄTT I / DRA KORT  ", " ", " ");

//                dispatchToUi(a);
//            }
//        }

//        public class VerifoneEventHandler
//        {
//            private readonly VerifoneApi verifoneApi;

//            private IPaymentDeviceEventHandler eventHandler;

//            public VerifoneEventHandler(VerifoneApi verifoneApi, IPaymentDeviceEventHandler eventHandler)
//            {
//                this.verifoneApi = verifoneApi;
//                this.eventHandler = eventHandler;
//            }

//            public void infoEvent(ref string text)
//            {
//                string t = text;
//                Debug.WriteLine(TAG + "REC (infoEvent) {0}", text);
//                Action a = () => eventHandler.onInfoEvent(t);

//                dispatchToUi(a);
//            }

//            internal void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
//            {
//                Debug.WriteLine(TAG + "RegisterEventHandler");

//                if (this.eventHandler != null)
//                {
//                    throw new Exception("eventHandler already registered");
//                }

//                this.eventHandler = eventHandler;
//            }

//            internal void UnRegisterEventHandler()
//            {
//                Debug.WriteLine(TAG + "UnRegisterEventHandler");

//                eventHandler = null;
//            }

//            private void dispatchToUi(Action a)
//            {
//                if (eventHandler != null)
//                {
//                    verifoneApi.uiDispatcher.Invoke(a);
//                }
//            }

//            private string getDescription(string description)
//            {
//                return description;
//            }

//            private PaymentDataItemType getItem(int item)
//            {
//                if (item == -1)
//                {
//                    return PaymentDataItemType.END;
//                }
//                return PaymentDataItemType.UNKNOWN;
//            }

//            private PaymentDataType getResultType(int resultType)
//            {
//                switch ((ResultDataTypes)resultType)
//                {
//                    case ResultDataTypes.rdCUSTOMERRECEIPT:
//                        return PaymentDataType.CUSTOMER_RECEIPT;

//                    case ResultDataTypes.rdMERCHANTRECEIPT:
//                        return PaymentDataType.MERCHANT_RECEIPT;

//                    case ResultDataTypes.rdCLOSEBATCHRESULT:
//                        return PaymentDataType.BATCH_CLOSE;

//                    case ResultDataTypes.rdCURRENTBATCH:
//                        return PaymentDataType.BATCH_CURRENT;

//                    case ResultDataTypes.rdTRMCONFIG:
//                        return PaymentDataType.CONFIG;

//                    case ResultDataTypes.rdTRANSLOGDETAILED:
//                        return PaymentDataType.TRANSACTION_LOG_DETAILED;

//                    case ResultDataTypes.rdTRANSLOGTOTALS:
//                        return PaymentDataType.TRANSACTION_LOG_TOTALS;

//                    case ResultDataTypes.rdUNSENTTRANS:
//                        return PaymentDataType.TRANSACTION_UNSENT;

//                    default:
//                        return PaymentDataType.UNKNOWN;
//                }
//            }

//            private string getValue(string value)
//            {
//                return value;
//            }
//        }
//    }
//}