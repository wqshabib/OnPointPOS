using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration
{
    public interface IPaymentDevice
    {
        void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler);
        void UnRegisterEventHandler();

        bool IsConnected();
        bool Connect();
        bool Disconnect();

        void StartTransaction(PaymentTransactionType transactionType);

        void ProcessPaymentAmount(decimal DebitCardAmount, decimal vatAmount, decimal CashBackAmount, Guid orderId);

        void ReturnPaymentAmount(decimal DebitCardAmount, decimal vatAmount);

        void EndTransaction();

        void SendReferralCode(string authCode);

        PaymentDeviceStatus GetDeviceStatus();

        void TestConnection();

        void GetConfiguration();

        void GetUnsentTransactions();

        void GetTransactionLog(PaymentDataType paymentDataType, int inputNumber);

        void CloseBatch();
        void ResetConnection();
        void Cancel();
        void SetPaymentDialogCloseForced(bool force);
        void GetCustomerReceipt();
        void SetIsConnected();
        void RePrintLastCancelledPayment();
    }

    public enum PaymentDeviceType
    {
        DUMMY,
        NONE,
        BABS_BPTI,
        //BABS_BPTI_INTEROP,
        CONNECT2T,
        //VERIFONE
    }

    public interface IPaymentDeviceEventHandler
    {
        object onInfoEvent(string t);
        object onExceptionEvent(string t);
        //object onTerminalDisplay(string txtrow1, string txtrow2, string txtrow3, string txtrow4);
        object onTerminalDisplay(string txtrow1, string txtrow2, string txtrow3, string txtrow4, int? status, int? transactionType, int? transactionResult);

        object onStatusChanged(bool connected, bool open);

        object onTransactionStart();

        object onTransactionEnd();

        object onTerminalResultEvent(PaymentTransactionType transType, bool success, string cashierText);

        object onTerminalReceiptData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value, double total);

        object onTerminalBatchData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value);

        object onTransactionData(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, string header, string value);

        object onTerminalReferralEvent(string instruction);
        object onTerminalReConnectEvent(string instruction);
        object onTerminalCancelEvent(string instruction);

        object onTerminalDeclinedEvent(string instruction);


        object onBamboraEvent(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType, PaymentTransactionType transType, bool success, string cashierText, List<string> customerreceipt, List<string> merchantreceipt, bool signatureneeded, string productName, double total);
        // Got it ?yes
        object onBamboraOutOfBalance(string message);

        object onBamboraSignatureRequired(string message);


        void onTerminalNeedClose();
        object onVerifoneEvent(PaymentDataType paymentDataType, PaymentDataItemType paymentDataItemType,
             PaymentTransactionType transType, bool success, string cashierText, List<string> customerreceipt,
             List<string> merchantreceipt, bool signatureneeded);
    }

    public enum PaymentDataType
    {
        CUSTOMER_RECEIPT,
        MERCHANT_RECEIPT,
        BATCH_CLOSE,
        BATCH_CURRENT,
        CONFIG,
        TRANSACTION_LOG_DETAILED,
        TRANSACTION_LOG_TOTALS,
        TRANSACTION_UNSENT,
        UNKNOWN


    }

    public enum PaymentDeviceStatus
    {
        DISCONNECTED,
        CONNECTED,
        OPEN,
        CLOSED,
        IN_TRANSACTION
    }

    public enum PaymentDataItemType
    {
        END,
        UNKNOWN


    }

    public enum PaymentTransactionType
    {
        PURCHASE,
        REFUND,
        REVERSAL,
        BATCH,
        UNKNOWN
    }

    public class PaymentTransactionStatus
    {
        public PaymentResult Result;
        public IList<String> MerchantReceipt;
        public IList<String> CustomerReceipt;
        public PaymentTransactionType TransactionType;
        public String PaymentIdentification;
        public String ProductName;
        public double Total;

        public enum PaymentResult
        {
            SUCCESS,
            CANCELLED,
            NO_DEVICE_CONFIGURED
        }

    }

}
