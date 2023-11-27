using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifoneSdk;

namespace POSSUM.Integration.Integration.PaymentDevices.VerifonePSDK
{
    public class VerifoneAPIs
    {
        public PaymentSdk _paymentSdk;
        public VerifoneListener _verifoneListener;
        VerifonePaymentDevice _verifonePaymentDevice;
        public VerifoneAPIs(VerifonePaymentDevice verifonePaymentDevice)
        {
            _verifonePaymentDevice = verifonePaymentDevice;
            _paymentSdk = new PaymentSdk();
            _verifoneListener = new VerifoneListener(this);
            PaymentSdk.ConfigureLogFile(@"C:\PSDKLogs\" + DateTime.Now.ToFileTime() + "-psdk.log", 2048);
        }

        public void UpdateConsole(string title, string content)
        {
            //Update UI based on title
            //_verifonePaymentDevice.methodName
        }

        public void UpdateConsolveWithEvent(string status, string type, string message)
        {
            //Update UI based on status
            //_verifonePaymentDevice.methodName
        }

        public void DeviceDiscovery_Click()
        {
            this._paymentSdk.DisplayConfiguration(this._verifoneListener);
        }

        public void TearDownSdkButton_Click()
        {
            _paymentSdk.TearDown();
        }

        string DEVICE_ADDRESS_KEY = "";
        string DEVICE_CONNECTION_TYPE_KEY = "";
        bool slim = false;


        public void CommunicateWithPaymentSDK_Click()
        {
            string a = "ADDRESS";
            var dict = new Dictionary<string, string>
            {
                { "DEVICE_ADDRESS_KEY", DEVICE_ADDRESS_KEY },
                { "DEVICE_CONNECTION_TYPE_KEY", DEVICE_CONNECTION_TYPE_KEY}
            };

            if (slim)
            {
                dict.Add(TransactionManager.DEVICE_HOST_AUTHENTICATION_KEY, TransactionManager.DEVICE_HOST_AUTHENTICATION_ENABLED);
            }

            _paymentSdk.InitializeFromValues(_verifoneListener, dict);

        }
        
        public void LoginWithCredentials_Click()
        {
            UpdateConsole("Update :", "Login...");
            var credentials = LoginCredentials.Create();
            credentials.UserId = "username";
            credentials.Password = "password";
            credentials.ShiftNumber = "shift";
            var result = _paymentSdk.TransactionManager.LoginWithCredentials(credentials);
        }

        public void StartSession_Click()
        {
            Transaction transaction = Transaction.Create();
            transaction.InvoiceId = "abc123";
            var result = _paymentSdk.TransactionManager.StartSession(transaction);
        }

        public void AddMerchandise_Click()
        {
            Merchandise merchandise = Merchandise.Create();
            var current_amount_totals = _paymentSdk.TransactionManager.BasketManager.CurrentAmountTotals;

            VerifoneSdk.Decimal gratuity = new VerifoneSdk.Decimal(0);
            if (current_amount_totals == null)
            {
                current_amount_totals = AmountTotals.Create(true);
                current_amount_totals.SetWithAmounts(merchandise.ExtendedPrice, merchandise.Tax, gratuity,
                        new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(108));
            }
            else
            {
                current_amount_totals.AddAmounts(merchandise.ExtendedPrice, merchandise.Tax, gratuity,
                        new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(108));
            }

            var merch_list = new System.Collections.Generic.List<Merchandise>
            {
                merchandise
            };

            var result = _paymentSdk.TransactionManager.BasketManager.AddMerchandise(merch_list, current_amount_totals);
        }

        public void FinalizeBasket_Click()
        {
            UpdateConsole("Update: ", "FinalizeBasket...");

            var result = _paymentSdk.TransactionManager.BasketManager.FinalizeBasket();
        }

        public void Logout_Click()
        {
            UpdateConsole("Update", "Logout...");
            var result = _paymentSdk.TransactionManager.Logout();
        }

        public void EndSession_Click()
        {
            UpdateConsole("Update", "EndSession...");
            var result = _paymentSdk.TransactionManager.EndSession();
        }

        public void PaymentTransaction_Click()
        {
            UpdateConsole("Update: ", "StartPayment...");

            var payment = Payment.Create();
            payment.RequestedAmounts = _paymentSdk.TransactionManager.BasketManager.CurrentAmountTotals;
            payment.PaymentType = PaymentType.CREDIT;


            var result = _paymentSdk.TransactionManager.StartPayment(payment);
        }

        public void Print_Click(string data)
        {
            UpdateConsole("Printing: ", data);
            var result = _paymentSdk.TransactionManager.Print(data, ContentType.TEXT, ReceiptType.DOCUMENT, DeliveryMethod.PRINT, "");
        }

        public void RemoveMerchandis_Click()
        {
            var basket_manager = _paymentSdk.TransactionManager.BasketManager;
            var basket = basket_manager.Basket;
            IList<Merchandise> merchandise_list = new List<Merchandise>();
            if (basket != null)
            {
                merchandise_list = basket.Merchandise;
            }
            if (merchandise_list.Count > 0)
            {
                var merchandise = merchandise_list[merchandise_list.Count - 1];
                var amount = merchandise.Amount;
                var amount_totals = basket_manager.CurrentAmountTotals;
                if (amount_totals != null)
                {
                    amount_totals.SubtractAmounts(amount, new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(0),
                       new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(0), new VerifoneSdk.Decimal(0), amount);
                    var removed = new List<Merchandise>(); removed.Add(merchandise);
                    basket_manager.RemoveMerchandise(removed, amount_totals);
                }
            }
        }

        public void Abort_Click()
        {
            UpdateConsole("Update: ", "Abort...");
            var result = _paymentSdk.TransactionManager.Abort();
        }
    }
}
