using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using POSSUM.Model;

namespace POSSUM.Integration
{
    public class ControlUnitFactory
    {
        public static IControlUnit GetControlUnit(ControlUnitType type, String controlUnitConnectionString)
        {

            switch (type)
            {
                case ControlUnitType.CLEAN_CASH:
                    return new ControlUnits.CleanCash.CleanCashControlUnit(controlUnitConnectionString, DefaultsIntegration.Outlet.OrgNo, DefaultsIntegration.PosIdType == PosIdType.UniqueId ? DefaultsIntegration.Terminal.UniqueIdentification : DefaultsIntegration.Terminal.TerminalNo.ToString());
                case ControlUnitType.CLEAN_CASH2:
                    return new ControlUnits.CleanCash.CleanCashControlUnit2(controlUnitConnectionString);
                case ControlUnitType.DUMMY:
                    return new ControlUnits.Dummy.DummyControlUnit();
            }

            //todo khalil
            MessageBox.Show(DefaultsIntegration.Message_CashDrawerUnitControlNotDefined);
            return null;
        }

        //WAQAS_CHANGES
        public static IControlUnit GetControlUnit(ControlUnitType type, string controlUnitConnectionString, string orgNo, string posId, Terminal terminal)
        { 
            switch (type)
            {
                case ControlUnitType.CLEAN_CASH:
                    return new ControlUnits.CleanCash.CleanCashControlUnit(controlUnitConnectionString, orgNo, posId);//(controlUnitConnectionString, orgNo, posId);
                case ControlUnitType.CLEAN_CASH2:
                    return new ControlUnits.CleanCash.CleanCashControlUnit2(controlUnitConnectionString);
                case ControlUnitType.CLOUD_CLEAN_CASH:
                    return new ControlUnits.CleanCash.CloudCashControlUnit(controlUnitConnectionString, orgNo, posId);
                case ControlUnitType.DUMMY:
                    return new ControlUnits.Dummy.DummyControlUnit();
            }

            //todo khalil
            MessageBox.Show(DefaultsIntegration.Message_CashDrawerUnitControlNotDefined);
            return null;
        }
    }
}
