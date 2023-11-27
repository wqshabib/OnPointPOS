using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace POSSUM.Integration
{
    public class PaymentDeviceFactory
    {
        public static IPaymentDevice GetPaymentDevice(PaymentDeviceType type, String paymentDeviceTypeConnectionString)
        {
            
            switch(type)
            {
                case PaymentDeviceType.NONE:
                    return null;
                case PaymentDeviceType.DUMMY:
                    return new PaymentDevices.Dummy.DummyPaymentDevice(paymentDeviceTypeConnectionString);
                case PaymentDeviceType.BABS_BPTI:
                    return new PaymentDevices.BabsBpti.BabsBptiPaymentDevice(paymentDeviceTypeConnectionString);
                //case PaymentDeviceType.BABS_BPTI_INTEROP:
                //    return new PaymentDevices.BabsBptiInterop.BabsBptiInteropPaymentDevice(paymentDeviceTypeConnectionString);
                case PaymentDeviceType.CONNECT2T:
                    return new PaymentDevices.BamboraConnect2T.BamboraConnect2TPaymentDevice(paymentDeviceTypeConnectionString);
                //case PaymentDeviceType.VERIFONE:
                //    return new VerifonePaymentDevice(paymentDeviceTypeConnectionString);
            }


            MessageBox.Show(DefaultsIntegration.Message_CashDrawerUnitControlNotDefined);
            return null;
        }


    }
}
