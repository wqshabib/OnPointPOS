using System;
using System.Globalization;
using POSSUM.Model;

namespace POSSUM.Integration.ControlUnits.Dummy
{
    public class DummyControlUnit : IControlUnit
    {
        public bool Open()
        {
            return true;
        }

        public bool RegisterPOS(string orgNumber, string posId)
        {
            return true;
        }

        public ControlUnitStatus CheckStatus()
        {
            return ControlUnitStatus.OK;
        }

        public ControlUnitResponse SendReceipt(DateTime dateTime, long reciptId, decimal totalAmount,
            decimal negativeAmount, VAT vat1, VAT vat2, VAT vat3, VAT vat4, int attemptNo, bool isCopy = false, bool isPerforma = false)
        {
            string unitName = "Box001";
            string controlCode = "1234567890abcdefghijklmnopqrstuvwxyz";
            return new ControlUnitResponse(true, unitName, controlCode);
        }

        public ControlUnitResponse SendReceipt(Receipt receipt, int attemptNo, bool isCopy = false)
        {
            string unitName = "Box001";
            string controlCode = "1234567890abcdefghijklmnopqrstuvwxyz";
            return new ControlUnitResponse(true, unitName, controlCode);
        }

        public bool Close()
        {
            return true;
        }

        public bool Dispose()
        {
            return true;
        }

        private string formatVat(VAT vat)
        {
            string fmt = string.Format("{0};{1}",
                vat.VATPercent.ToString("#.##", CultureInfo.GetCultureInfo(DefaultsIntegration.CultureString)),
                vat.VATTotal.ToString("#.##", CultureInfo.GetCultureInfo(DefaultsIntegration.CultureString)));

            return fmt;
        }
        //WAQAS_CHANGES
        public ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo)
        {
            return SendReceipt(receipt,attemptNo);
        }
        //WAQAS_CHANGES
        public ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo, bool value)
        {
            return SendReceipt(receipt,attemptNo, value);
        }
    }
}