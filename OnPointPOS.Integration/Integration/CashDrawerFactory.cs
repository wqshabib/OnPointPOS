using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POSSUM.Integration
{
    public class CashDrawerFactory
    {
        public static POSSUM.Integration.ICashDrawer GetCashDrawer(CashDrawerType type)
        {
            
            switch(type)
            {
                case CashDrawerType.DIRECT_HARDWARE_48C:
                    return new CashDrawers.DirectHardware.DirectHWCashDrawer(DefaultsIntegration.CashDrawerHardwarePort);//0x48C here need to pass app setting key?
                case CashDrawerType.DIRECT_HARDWARE:
                    return new CashDrawers.DirectHardware.DirectHWCashDrawer(DefaultsIntegration.CashDrawerHardwarePort);// here need to pass app setting key?
                case CashDrawerType.PARTNER:
                    return new CashDrawers.PartnerPOS.PartnerCashDrawer();
                case CashDrawerType.PRINTER:
                    return new CashDrawers.Printer.PrinterCashDrawer();
                case CashDrawerType.DUMMY:
                    return new CashDrawers.Dummy.DummyCashDrawer();
            }

            //todo khalil 
            //MessageBox.Show(UI.Message_CashDrawerUnitControlNotDefined);
            MessageBox.Show(DefaultsIntegration.Message_CashDrawerUnitControlNotDefined);
            return null;
        }

        //WAQAS_CHANGES
        public static ICashDrawer GetCashDrawer(CashDrawerType type, short cashDrawerHardwarePort)
        { 
            switch (type)
            {
                case CashDrawerType.DIRECT_HARDWARE_48C:
                    return new CashDrawers.DirectHardware.DirectHWCashDrawer(cashDrawerHardwarePort);//0x48C here need to pass app setting key?
                case CashDrawerType.DIRECT_HARDWARE:
                    return new CashDrawers.DirectHardware.DirectHWCashDrawer(cashDrawerHardwarePort);// here need to pass app setting key?
                case CashDrawerType.PARTNER:
                    return new CashDrawers.PartnerPOS.PartnerCashDrawer();
                case CashDrawerType.PRINTER:
                    return new CashDrawers.Printer.PrinterCashDrawer();
                case CashDrawerType.DUMMY:
                    return new CashDrawers.Dummy.DummyCashDrawer();
            }


            //todo khalil 
                MessageBox.Show(DefaultsIntegration.Message_CashDrawerUnitControlNotDefined);
            return null;
        }
    }
}
