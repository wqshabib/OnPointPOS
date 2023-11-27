//using System;
//using System.Diagnostics;
//using System.Threading;
//using System.Windows.Threading;
//using POSSUM.Integration;
//using POSSUM.Integration.PaymentDevices.Verifone;

//namespace POSSUM.Payments.PaymentDevices.Verifone
//{
//    public class VerifonePaymentDevice : IPaymentDevice
//    {
//        private readonly string paymentDeviceTypeConnectionString;

//        private volatile VerifoneApi api;

//        private IPaymentDeviceEventHandler eventHandler;

//        private volatile bool isConnected;

//        private bool sentAmountForCurrentTransaction;

//        private volatile bool shouldRun;

//        public VerifonePaymentDevice(string paymentDeviceTypeConnectionString)
//        {
//            this.paymentDeviceTypeConnectionString = paymentDeviceTypeConnectionString;
//            isConnected = false;
//            dispatcher = Dispatcher.CurrentDispatcher;
//        }

//        public Dispatcher dispatcher { get; set; }

//        private void runInBackground(Action target)
//        {
//            new Thread(new ThreadStart(target)).Start();
//        }

//        public void Cancel()
//        {
//            runInBackground(() => api.Cancel());
//        }

//        public void CloseBatch()
//        {
//            runInBackground(() =>
//            {
//                Debug.WriteLine("Close batch");
//                api.CloseBatch();
//            });
//        }

//        public bool Connect()
//        {
//            sentAmountForCurrentTransaction = false;

//            if (isConnected && api != null && api.IsConnected())
//            {
//                return true;
//            }

//            // Send command to connect to terminal
//            runInBackground(() =>
//            {
//                api = new VerifoneApi(paymentDeviceTypeConnectionString, dispatcher);
//                if (eventHandler != null)
//                {
//                    api.RegisterEventHandler(eventHandler);
//                }

//                isConnected = true;
//            });

//            // Wait for api initialization before
//            // Replace with better mutex/syncronization
//            while (api == null)
//            {
//                Thread.Sleep(100);
//            }
//            return true;
//        }

//        public bool Disconnect()
//        {
//            shouldRun = false;
//            api.Disconnect();
//            isConnected = false;
//            sentAmountForCurrentTransaction = false;
//            return shouldRun;
//        }

//        public void EndTransaction()
//        {
//            runInBackground(() =>
//            {
//                sentAmountForCurrentTransaction = false;
//                api.EndTransaction();
//            });
//        }

//        public void GetConfiguration()
//        {
//            runInBackground(() =>
//            {
//                Debug.WriteLine("Request config");
//                api.TerminalConfig();
//            });
//        }

//        public void GetCustomerReceipt()
//        {
//            runInBackground(() => api.apiCustomerReceipt());
//        }

//        public PaymentDeviceStatus GetDeviceStatus()
//        {
//            int status = api.LastStatus;
//            switch (status)
//            {
//                case 0:
//                    return PaymentDeviceStatus.DISCONNECTED;

//                case 1:
//                    return PaymentDeviceStatus.CONNECTED;

//                case 2:
//                    return PaymentDeviceStatus.OPEN;

//                case 3:
//                    return PaymentDeviceStatus.CLOSED;

//                case 4:
//                    return PaymentDeviceStatus.IN_TRANSACTION;

//                case 5:
//                    return PaymentDeviceStatus.OPEN;

//                case 6:
//                    return PaymentDeviceStatus.DISCONNECTED;

//                case 7:
//                    return PaymentDeviceStatus.DISCONNECTED;

//                default:
//                    return PaymentDeviceStatus.DISCONNECTED;
//            }
//        }

//        public void GetTransactionLog(PaymentDataType paymentDataType, int inputNumber)
//        {
//            runInBackground(() =>
//            {
//                Debug.WriteLine("Get transaction log");
//                switch (paymentDataType)
//                {
//                    case PaymentDataType.TRANSACTION_LOG_DETAILED:
//                        api.TransLogByNr(11, inputNumber);
//                        break;

//                    case PaymentDataType.TRANSACTION_LOG_TOTALS:
//                        api.TransLogByNr(12, inputNumber);
//                        break;

//                    case PaymentDataType.BATCH_CLOSE:
//                        api.TransLogByNr(41, inputNumber);
//                        break;
//                }
//            });
//        }

//        public void GetUnsentTransactions()
//        {
//            runInBackground(() =>
//            {
//                Debug.WriteLine("Get unsent transactions");
//                api.UnsentTransactions();
//            });
//        }

//        public bool IsConnected() => isConnected;

//        public void ProcessPaymentAmount(decimal debitCardAmount, decimal vatAmount, decimal cashBackAmount,Guid orderId)
//        {
//            if (!sentAmountForCurrentTransaction)
//            {
//                runInBackground(() =>
//                {
//                    Debug.WriteLine("Sending amounts");
//                    api.SendAmount(debitCardAmount, vatAmount, cashBackAmount);
//                    sentAmountForCurrentTransaction = true;
//                });
//            }
//        }

//        public void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
//        {
//            if (this.eventHandler != null)
//            {
//                throw new Exception("eventHandler already registered");
//            }
//            this.eventHandler = eventHandler;
//            runInBackground(() => api?.RegisterEventHandler(this.eventHandler));
//        }

//        public void ResetConnection()
//        {
//            Disconnect();
//            api.UnRegisterEventHandler();
//            api = null;
//            Connect();
//        }

//        public void ReturnPaymentAmount(decimal DebitCardAmount, decimal vatAmount)
//        {
//        }

//        public void SendReferralCode(string authCode)
//        {
//            runInBackground(() => api?.SendReferralCode(authCode));
//        }

//        public void SetIsConnected()
//        {
//            isConnected = api.IsConnected();
//        }

//        public void StartTransaction(PaymentTransactionType transactionType)
//        {
//            //runInBackground(() =>
//            //{
//            Debug.WriteLine("Start transaction");
//            switch (transactionType)
//            {
//                case PaymentTransactionType.PURCHASE:
//                    api.StartTransaction(VerifoneApi.TransactionTypes.LPP_PURCHASE);
//                    break;

//                case PaymentTransactionType.REFUND:
//                    api.StartTransaction(VerifoneApi.TransactionTypes.LPP_REFUND);
//                    break;

//                case PaymentTransactionType.REVERSAL:
//                    api.StartTransaction(VerifoneApi.TransactionTypes.LPP_REVERSAL);
//                    break;

//                case PaymentTransactionType.BATCH:
//                    api.StartTransaction(VerifoneApi.TransactionTypes.LPP_CLOSEBATCH);
//                    break;
//            }
//            //});
//        }

//        public void TestConnection()
//        {
//            runInBackground(() =>
//            {
//                Debug.WriteLine("Testing connection");
//                api.TestConnection();
//                sentAmountForCurrentTransaction = false;
//                api.EndTransaction();
//            });
//        }

//        public void UnRegisterEventHandler()
//        {
//            eventHandler = null;
//            runInBackground(() => api?.UnRegisterEventHandler());
//        }

//        public void SetPaymentDialogCloseForced(bool force)
//        {
           
//        }
//    }
//}