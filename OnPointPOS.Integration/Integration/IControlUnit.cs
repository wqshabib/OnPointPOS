using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration
{
    public interface IControlUnit 
    {
        bool Open();
        bool RegisterPOS(String orgNumber, String posId);
        ControlUnitStatus CheckStatus();

        ControlUnitResponse SendReceipt(DateTime dateTime, long reciptId, decimal totalAmount, decimal negativeAmount, VAT vat1, VAT vat2, VAT vat3, VAT vat4, int attemptNo, bool isCopy = false, bool isPerforma = false);

        bool Close();

        ControlUnitResponse SendReceipt(Model.Receipt receipt, int attemptNo, bool isCopy = false);

        bool Dispose();
        //WAQAS_CHANGES
        ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo);
        //WAQAS_CHANGES
        ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo, bool value);
    }

    public class ControlUnitAction : IDisposable
    {
        IControlUnit controlUnit;

        public ControlUnitAction(IControlUnit cu)
        {
            controlUnit = cu;
            controlUnit.Open();
        }

        public IControlUnit ControlUnit { get { return controlUnit; } }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                controlUnit.Dispose();
            }
            else
            {

            }
        }


        ~ControlUnitAction()
        {
            Dispose(false);
        }

    }


    public enum ControlUnitType
    {
        DUMMY,
        CLEAN_CASH,
        CLEAN_CASH2,
        CLOUD_CLEAN_CASH
    }

    public enum ControlUnitStatus
    {
        COMMUNICATION_ERROR,
        OK,
        BUSY,
        ERROR_PLEASE_RETRY,
        FATAL_ERROR,
        UNKNOWN


    }

    public class ControlUnitResponse
    {
        public bool Success { get; private set; }
        public String UnitName { get; private set; }
        public String ControlCode { get; private set; }

        public ControlUnitResponse(bool result)
        {
            this.Success = result;
        }

        public ControlUnitResponse(bool result, string unitName, string controlCode)
        {
            this.Success = result;
            this.UnitName = unitName;
            this.ControlCode = controlCode;
        }

    }

    public class ControlUnitException : Exception
    {
        public ControlUnitException(String message, Exception innerException) : base(message, innerException)
        {

        }
    }


    //public class VAT
    //{
    //    public static decimal NetFromGross(decimal unitPriceWithVat, decimal vatPercent)
    //    {
    //        return unitPriceWithVat * (1 - ((vatPercent / 100) / (1 + (vatPercent / 100))));

    //    }

    //    public static decimal GrossFromNet(decimal unitPriceWithoutVat, decimal vatPercent)
    //    {
    //        return unitPriceWithoutVat * vatPercent / 100;

    //    }

    //    public decimal VATPercent { get; set; }
    //    public decimal VATTotal { get; set; }


    //    public VAT(decimal vat, decimal vatAmount)
    //    {
    //        VATPercent = vat;
    //        VATTotal = vatAmount;
    //    }
    //}


}
