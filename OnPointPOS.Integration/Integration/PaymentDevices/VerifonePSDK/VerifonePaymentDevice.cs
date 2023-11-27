using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.Integration.PaymentDevices.VerifonePSDK
{
    public class VerifonePaymentDevice : IPaymentDevice
    {
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void CloseBatch()
        {
            throw new NotImplementedException();
        }

        public bool Connect()
        {
            throw new NotImplementedException();
        }

        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        public void EndTransaction()
        {
            throw new NotImplementedException();
        }

        public void GetConfiguration()
        {
            throw new NotImplementedException();
        }

        public void GetCustomerReceipt()
        {
            throw new NotImplementedException();
        }

        public PaymentDeviceStatus GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public void GetTransactionLog(PaymentDataType paymentDataType, int inputNumber)
        {
            throw new NotImplementedException();
        }

        public void GetUnsentTransactions()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public void ProcessPaymentAmount(decimal DebitCardAmount, decimal vatAmount, decimal CashBackAmount, Guid orderId)
        {
            throw new NotImplementedException();
        }

        public void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public void RePrintLastCancelledPayment()
        {
            throw new NotImplementedException();
        }

        public void ResetConnection()
        {
            throw new NotImplementedException();
        }

        public void ReturnPaymentAmount(decimal DebitCardAmount, decimal vatAmount)
        {
            throw new NotImplementedException();
        }

        public void SendReferralCode(string authCode)
        {
            throw new NotImplementedException();
        }

        public void SetIsConnected()
        {
            throw new NotImplementedException();
        }

        public void SetPaymentDialogCloseForced(bool force)
        {
            throw new NotImplementedException();
        }

        public void StartTransaction(PaymentTransactionType transactionType)
        {
            throw new NotImplementedException();
        }

        public void TestConnection()
        {
            throw new NotImplementedException();
        }

        public void UnRegisterEventHandler()
        {
            throw new NotImplementedException();
        }
    }
}
