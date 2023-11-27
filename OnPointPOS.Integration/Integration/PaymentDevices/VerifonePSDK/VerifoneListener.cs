using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifoneSdk;

namespace POSSUM.Integration.Integration.PaymentDevices.VerifonePSDK
{
    public class VerifoneListener : CommerceListener2
    {
        VerifoneAPIs _verifoneAPIs;
        public VerifoneListener(VerifoneAPIs verifoneAPIs)
        {
            _verifoneAPIs = verifoneAPIs;
        }

        public override void HandlePinEvent(PinEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("PinEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("PinEvent: ", "Failure.\n");
            }

            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }

        public override void HandleAmountAdjustedEvent(AmountAdjustedEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("AmountAdjustedEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("AmountAdjustedEvent: ", "Failure.\n");
            }

            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }

        public override void HandleBasketAdjustedEvent(BasketAdjustedEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("BasketAdjustedEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("BasketAdjustedEvent: ", "Failure.\n");
            }

            string type = sdk_event.Type;
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }

        public override void HandleBasketEvent(BasketEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("BasketEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("BasketEvent: ", "Failure.\n");
            }

            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }

        public override void HandleCardInformationReceivedEvent(CardInformationReceivedEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("CardInformationReceivedEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("CardInformationReceivedEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleCommerceEvent(CommerceEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("CommerceEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("CommerceEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleDeviceManagementEvent(DeviceManagementEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("DeviceManagementEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("DeviceManagementEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleHostAuthorizationEvent(HostAuthorizationEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("HostAuthorizationEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("HostAuthorizationEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }

        public override void HandleHostFinalizeTransactionEvent(HostFinalizeTransactionEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("HostFinalizeTransactionEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("HostFinalizeTransactionEvent: ", "Failure.\n");
            }

            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleLoyaltyReceivedEvent(LoyaltyReceivedEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("LoyaltyReceivedEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("LoyaltyReceivedEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleNotificationEvent(NotificationEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("NotificationEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("NotificationEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandlePaymentCompletedEvent(PaymentCompletedEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("PaymentCompletedEvent: ", "Payment Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("PaymentCompletedEvent: ", "Payment Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandlePrintEvent(PrintEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("PrintEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("PrintEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleReceiptDeliveryMethodEvent(ReceiptDeliveryMethodEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("ReceiptDeliveryMethodEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("ReceiptDeliveryMethodEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleReconciliationEvent(ReconciliationEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("ReconciliationEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("ReconciliationEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleReconciliationsListEvent(ReconciliationsListEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("ReconciliationsListEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("ReconciliationsListEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleStatus(Status status)
        {
            if (status.StatusCode == 0)
            {
                _verifoneAPIs.UpdateConsole("Status: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("Status: ", "Failure.\n");
            }
            string type = status.Type.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status.StatusCode.ToString(), type, status.Message);

        }
        public override void HandleStoredValueCardEvent(StoredValueCardEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("StoredValueCardEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("StoredValueCardEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleTransactionEvent(TransactionEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("TransactionEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("TransactionEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleTransactionQueryEvent(TransactionQueryEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("TransactionQueryEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("TransactionQueryEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleUserInputEvent(UserInputEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("UserInputEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("UserInputEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);
        }
        public override void HandleDeviceVitalsInformationEvent(DeviceVitalsInformationEvent sdk_event)
        {
            if (sdk_event.Status == 0)
            {
                _verifoneAPIs.UpdateConsole("DeviceVitalsInformationEvent: ", "Success.\n");
            }
            else
            {
                _verifoneAPIs.UpdateConsole("DeviceVitalsInformationEvent: ", "Failure.\n");
            }
            string type = sdk_event.Type == null ? "(null)" : sdk_event.Type.ToString();
            string status = sdk_event.Status.ToString();
            string message = sdk_event.Message == null ? "(null)" : sdk_event.Message.ToString();
            _verifoneAPIs.UpdateConsolveWithEvent(status, type, message);

        }
    };
}
