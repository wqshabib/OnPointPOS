using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.PaymentDevices.Dummy
{
    public class DummyPaymentDevice : IPaymentDevice
    {
        private string PaymentDeviceTypeConnectionString;
        private IPaymentDeviceEventHandler eventHandler;

        public DummyPaymentDevice(string paymentDeviceTypeConnectionString)
        {
            this.PaymentDeviceTypeConnectionString = paymentDeviceTypeConnectionString;
        }
        public bool IsConnected()
        {
            return true;
        }

        public bool Connect()
        {
            return true;
        }

        public bool Disconnect()
        {
            return false;
        }



        public void ProcessPaymentAmount(decimal DebitCardAmount, decimal vatAmount, decimal CashBackAmount, Guid orderId)
        {
        }

        public void ReturnPaymentAmount(decimal DebitCardAmount, decimal vatAmount)
        {
        }


        public void StartTransaction(PaymentTransactionType transactionType)
        {
        }


        public void EndTransaction()
        {
        }

        public void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
        {
            if (this.eventHandler != null)
            {
                throw new Exception("eventHandler already registered");
            }
            this.eventHandler = eventHandler;
        }

        public void UnRegisterEventHandler()
        {
            this.eventHandler = null;
        }


        public void SendReferralCode(string authCode)
        {

        }


        public PaymentDeviceStatus GetDeviceStatus()
        {
            return PaymentDeviceStatus.DISCONNECTED;
        }


        public void TestConnection()
        {
        }


        public void GetConfiguration()
        {
        }


        public void GetUnsentTransactions()
        {
        }


        public void GetTransactionLog(PaymentDataType paymentDataType, int inputNumber)
        {
        }


        public void CloseBatch()
        {
            throw new NotImplementedException();
        }


        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void GetCustomerReceipt()
        {
            throw new NotImplementedException();
        }

        public void ResetConnection()
        {
            throw new NotImplementedException("Not implemented exception");
        }

        public void SetIsConnected()
        {
            throw new NotImplementedException();
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void SetPaymentDialogCloseForced(bool force)
        {

        }

        public void RePrintLastCancelledPayment()
        {

        }
    }
}
