using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using Bambora.Connect2T;

namespace POSSUM.Integration.PaymentDevices.BamboraConnect2T
{
    public class BamboraConnect2TPaymentDevice : IPaymentDevice
    {
        private string paymentDeviceTypeConnectionString;
        private int cookie;
        private volatile BamboraConnect2TApi api;
        private volatile bool isConnected;
        private volatile bool shouldRun;

        private bool SentAmountForCurrentTransaction = false;
        private IPaymentDeviceEventHandler eventHandler;
        public Dispatcher dispatcher { get; set; }

        public BamboraConnect2TPaymentDevice(string paymentDeviceTypeConnectionString)
        {
            this.paymentDeviceTypeConnectionString = paymentDeviceTypeConnectionString;
            this.isConnected = false;
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public bool IsConnected()
        {
            return isConnected;
        }

        public bool Connect()
        {
            SentAmountForCurrentTransaction = false;

            if (isConnected && api != null && api.IsConnected())
            {
                return true;
            }

            // Send command to connect to terminal
            runInBackground(() =>
            {
                api = new BamboraConnect2TApi(paymentDeviceTypeConnectionString, dispatcher);
                if (eventHandler != null)
                {
                    api.RegisterEventHandler(this.eventHandler);
                }

                isConnected = true;// api.IsConnected();//true; Arshad:return api connected status instead of true;
            });

            // Wait for api initialization before 
            // Replace with better mutex/syncronization
            while (api == null)
            {
                Thread.Sleep(100);
            }
            return true;
        }

        public bool Disconnect()
        {
            shouldRun = false;
            api.Disconnect();
            this.isConnected = false;
            SentAmountForCurrentTransaction = false;
            return shouldRun;
        }

        public void ProcessPaymentAmount(decimal debitCardAmount, decimal vatAmount, decimal cashbackAmount, Guid orderId)
        {
            if (!SentAmountForCurrentTransaction)
            {
                runInBackground(() =>
                {
                    Debug.WriteLine("Sending amounts");
                    api.SendAmount(debitCardAmount, vatAmount, cashbackAmount, orderId);
                    SentAmountForCurrentTransaction = true;
                });
            }
        }

        public void ReturnPaymentAmount(decimal DebitCardAmount, decimal vatAmount)
        {
        }

        public void StartTransaction(PaymentTransactionType transactionType)
        {
            //runInBackground(() =>
            //{
            Debug.WriteLine("Start transaction");
            switch (transactionType)
            {
                case PaymentTransactionType.PURCHASE:
                    api.StartTransaction(BamboraConnect2TApi.TransactionTypes.LPP_PURCHASE);
                    break;
                case PaymentTransactionType.REFUND:
                    api.StartTransaction(BamboraConnect2TApi.TransactionTypes.LPP_REFUND);
                    break;
                case PaymentTransactionType.REVERSAL:
                    api.StartTransaction(BamboraConnect2TApi.TransactionTypes.LPP_REVERSAL);
                    break;
                case PaymentTransactionType.BATCH:
                    api.StartTransaction(BamboraConnect2TApi.TransactionTypes.LPP_CLOSEBATCH);
                    break;
            }
            //});
        }

        public void EndTransaction()
        {
            runInBackground(() =>
            {
                SentAmountForCurrentTransaction = false;
                api.EndTransaction();
            });
        }

        public void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
        {
            if (this.eventHandler != null)
            {
                //throw new Exception("eventHandler already registered");
            }
            this.eventHandler = eventHandler;
            runInBackground(() =>
            {
                if (api != null)
                {
                    api.RegisterEventHandler(this.eventHandler);
                }
            });
        }

        public void UnRegisterEventHandler()
        {
            this.eventHandler = null;
            runInBackground(() =>
            {
                if (api != null)
                {
                    api.UnRegisterEventHandler();
                }
            });
        }

        public void SendReferralCode(string authCode)
        {
            runInBackground(() =>
            {
                if (api != null)
                {
                    api.SendReferralCode(authCode);
                }
            });
        }

        private void runInBackground(Action target)
        {
            target.Invoke();
            //new Thread(new ThreadStart(target)).Start();
        }


        public PaymentDeviceStatus GetDeviceStatus()
        {
            int status = api.LastStatus;
            switch (status)
            {
                case 0:
                    return PaymentDeviceStatus.DISCONNECTED;
                case 1:
                    return PaymentDeviceStatus.CONNECTED;
                case 2:
                    return PaymentDeviceStatus.OPEN;
                case 3:
                    return PaymentDeviceStatus.CLOSED;
                case 4:
                    return PaymentDeviceStatus.IN_TRANSACTION;
                case 5:
                    return PaymentDeviceStatus.OPEN;
                case 6:
                    return PaymentDeviceStatus.DISCONNECTED;
                case 7:
                    return PaymentDeviceStatus.DISCONNECTED;
                default:
                    return PaymentDeviceStatus.DISCONNECTED;
            }
        }


        public void TestConnection()
        {
            runInBackground(() =>
            {
                Debug.WriteLine("Testing connection");
                api.TestConnection();
                SentAmountForCurrentTransaction = false;
                api.EndTransaction();
            });
        }


        public void GetConfiguration()
        {
            runInBackground(() =>
            {
                Debug.WriteLine("Request config");
                api.TerminalConfig();
            });
        }


        public void GetUnsentTransactions()
        {
            runInBackground(() =>
            {
                Debug.WriteLine("Get unsent transactions");
                api.UnsentTransactions();
            });
        }


        public void GetTransactionLog(PaymentDataType paymentDataType, int inputNumber)
        {
            runInBackground(() =>
            {
                Debug.WriteLine("Get transaction log");
                switch (paymentDataType)
                {
                    case PaymentDataType.TRANSACTION_LOG_DETAILED:
                        api.TransLogByNr(11, inputNumber);
                        break;
                    case PaymentDataType.TRANSACTION_LOG_TOTALS:
                        api.TransLogByNr(12, inputNumber);
                        break;
                    case PaymentDataType.BATCH_CLOSE:
                        api.TransLogByNr(41, inputNumber);
                        break;
                }
            });
        }


        public void CloseBatch()
        {
            runInBackground(() =>
            {
                Debug.WriteLine("Close batch");
                api.CloseBatch();
            });
        }
        public void ResetConnection()
        {

            Disconnect();
            api.UnRegisterEventHandler();
            api = null;
            Connect();
            //api.ResetConnection();
            //runInBackground(() =>
            //{
            //    Debug.WriteLine("Reset Connection");
            //    api.ResetConnection();
            //});
        }

        public void Cancel()
        {
            runInBackground(() =>
            {
                api.Cancel();
            });
        }

        public void GetCustomerReceipt()
        {
            runInBackground(() =>
            {
                api.apiCustomerReceipt();
            });
        }

        public void SetIsConnected()
        {
            this.isConnected = api.IsConnected();
        }

        public void SetPaymentDialogCloseForced(bool force)
        {
            api.SetPaymentDialogCloseForced(force);
        }

        public void RePrintLastCancelledPayment()
        {
            if (!isConnected)
            {
                Connect();
            }

            api.RePrintLastCancelledPayment();
        }
    }
}
