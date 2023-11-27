using CleanCash_1_1;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO.Ports;

namespace POSSUM.Integration.ControlUnits.CleanCash
{
    public class CleanCashControlUnit : IControlUnit
    {
        Communication mCleanCashCom;
        String connectionString = String.Empty;
        private bool isConnected = false;
        private string orgNumber;
        private string posId;
        public CleanCashControlUnit(String connectionString, string orgNumber, string posId)
        {
            this.connectionString = connectionString;
            this.mCleanCashCom = new Communication();
            this.orgNumber = orgNumber;
            this.posId = posId;
        }

        //SerialPort _port;
        //SerialPort _prnPort;
        //string ControlUnitConnectionString; 
        //public CleanCashControlUnit(string ControlUnitConnectionString)
        //{
        //    this.ControlUnitConnectionString = ControlUnitConnectionString;
        //    _port = new SerialPort();
        //}

        public bool Open()
        {
            if (!isConnected)
            {
                CommunicationResult resultCode = mCleanCashCom.Open(connectionString);
                isConnected = true;
                this.RegisterPOS(orgNumber, posId);
                if (DefaultsIntegration.DebugCleanCash)
                    IntegrationLogWriter.CleanCashLogWrite(string.Format("Open: connectionString:{0}", connectionString));

                bool res = resultCode == CommunicationResult.RC_SUCCESS;
                if (DefaultsIntegration.DebugCleanCash)
                    IntegrationLogWriter.CleanCashLogWrite(string.Format("Response: {0}", resultCode));

                return res;
            }
            return isConnected;

        }

        public bool RegisterPOS(string orgNumber, string posId)
        {
            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite(string.Format("RegisterPOS: OrgNo:{0}, POSID: {1}", orgNumber.Replace("-", ""), DefaultsIntegration.ReplacePosIdSpecialChars ? posId.Replace("-", "") : posId));


            CommunicationResult resultCode = mCleanCashCom.RegisterPos(orgNumber.Replace("-", ""), DefaultsIntegration.ReplacePosIdSpecialChars ? posId.Replace("-", "") : posId);

            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite(string.Format("Response: OrgNo:{0}", resultCode));

            //todo khalil
            if (resultCode != CommunicationResult.RC_SUCCESS)
                throw new ControlUnitException(DefaultsIntegration.CannotRegisterToController + " (" + resultCode.ToString() + ")", null);

            return resultCode == CommunicationResult.RC_SUCCESS;
        }









        public ControlUnitStatus CheckStatus()
        {
            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite("mCleanCashCom.CheckStatus()");

            CommunicationResult resultCode = mCleanCashCom.CheckStatus();
            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite("Response: " + resultCode.ToString());

            if (resultCode != CommunicationResult.RC_SUCCESS)
            {
                if (resultCode != CommunicationResult.RC_SUCCESS)
                    throw new ControlUnitException(DefaultsIntegration.CannotConnectToController + " (" + resultCode.ToString() + ")", null);

                switch (resultCode)
                {
                    case CommunicationResult.RC_E_FAILURE:
                    case CommunicationResult.RC_E_ILLEGAL:
                    case CommunicationResult.RC_E_INVALID_PARAMETER:
                    case CommunicationResult.RC_E_INVALID_PORT:
                    case CommunicationResult.RC_E_LICENSE_EXCEEDED:
                    case CommunicationResult.RC_E_NOT_CLEANCASH:
                    case CommunicationResult.RC_E_NOT_SUPPORTED:
                    case CommunicationResult.RC_E_NULL_PARAMETER:
                    case CommunicationResult.RC_E_PORT_BUSY:
                        return ControlUnitStatus.COMMUNICATION_ERROR;

                }
                return ControlUnitStatus.COMMUNICATION_ERROR;
            }
            CommunicationStatus status = mCleanCashCom.LastUnitStatus;
            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite("Status: " + status.ToString());


            if (status == CommunicationStatus.STATUS_OK)
            {
                return ControlUnitStatus.OK;
            }
            else
            {
                String failureCode = mCleanCashCom.LastUnitStatusCodeList;
                if (DefaultsIntegration.DebugCleanCash)
                    IntegrationLogWriter.CleanCashLogWrite("Status: " + failureCode);


                if (status != CommunicationStatus.STATUS_OK)
                    throw new ControlUnitException("Kan ej prata med kontrollenheten (" + status.ToString() + ")", null);

                switch (status)
                {
                    case CommunicationStatus.STATUS_BUSY:
                        return ControlUnitStatus.BUSY;
                    case CommunicationStatus.STATUS_WARNING:
                    case CommunicationStatus.STATUS_PROTOCOL_ERROR:
                    case CommunicationStatus.STATUS_ERROR:
                        return ControlUnitStatus.ERROR_PLEASE_RETRY;
                    case CommunicationStatus.STATUS_FATAL_ERROR:
                        return ControlUnitStatus.FATAL_ERROR;
                    case CommunicationStatus.STATUS_COMMUNICATION_ERROR:
                    case CommunicationStatus.STATUS_UNKNOWN:
                    default:
                        return ControlUnitStatus.UNKNOWN;
                }

            }
        }

        public ControlUnitResponse SendReceipt(DateTime dateTime, long reciptId, decimal totalAmount, decimal negativeAmount, VAT vat1, VAT vat2, VAT vat3, VAT vat4, int attemptNo, bool isCopy = false, bool isPerforma = false)
        {
            if (DefaultsIntegration.DebugCleanCash)
            {
                IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", mCleanCashCom.StartReceipt() - " + reciptId);
            }

            CommunicationResult startResultCode = mCleanCashCom.StartReceipt();

            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", CommunicationResult:" + startResultCode.ToString() + " - " + reciptId);


            if (startResultCode != CommunicationResult.RC_SUCCESS)
            {
                if (startResultCode != CommunicationResult.RC_SUCCESS)
                    throw new ControlUnitException("Kan ej skicka data till kontrollenheten (" + startResultCode.ToString() + ")", null);

                return new ControlUnitResponse(false);
            }
            CommunicationReceipt receiptType = isPerforma ? CommunicationReceipt.RECEIPT_PRO_FORMA : isCopy ? CommunicationReceipt.RECEIPT_COPY : CommunicationReceipt.RECEIPT_NORMAL;



            // Send training/övning if övningsläge
            //if( PosState.GetInstance().TRAINING )
            if (DefaultsIntegration.User.TrainingMode)
            {
                receiptType = CommunicationReceipt.RECEIPT_TRAINING;
            }

            //Communi cationResult sendResultCode = mCleanCashCom.SendReceipt(dateTime.ToString("yyyyMMddHHmm"), reciptId.ToString(), isCopy ? CommunicationReceipt.RECEIPT_COPY : CommunicationReceipt.RECEIPT_NORMAL, "100,00", "0,00", "25,00;30,00", "0,00;0,00", "0,00;0,00", "0,00;0,00");
            if (DefaultsIntegration.DebugCleanCash)
            {

                var obj = new
                {
                    ExecutionDate = DateTime.Now,
                    ReceiptId = reciptId,
                    Date = dateTime,
                    TotalAmount = totalAmount,
                    NegativeAmount = negativeAmount,
                    VAT1 = vat1,
                    VAT2 = vat2,
                    VAT3 = vat3,
                    VAT4 = vat4,
                    IsCopy = isCopy,
                    IsPerforma = isPerforma
                };
                string log = JsonConvert.SerializeObject(obj);

                IntegrationLogWriter.CleanCashJsonLogWrite("ReceiptData_" + reciptId, log);
            }

            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", mCleanCashCom.SendReceipt() - " + reciptId);

            CommunicationResult sendResultCode = mCleanCashCom.SendReceipt(dateTime.ToString("yyyyMMddHHmm"), reciptId.ToString(), receiptType, totalAmount.ToString("0.00", CultureInfo.GetCultureInfo(DefaultsIntegration.CultureString)), negativeAmount.ToString("0.00", CultureInfo.GetCultureInfo(DefaultsIntegration.CultureString)), formatVat(vat1), formatVat(vat2), formatVat(vat3), formatVat(vat4));
            int lastExtendedError = mCleanCashCom.LastExtendedError;

            if (DefaultsIntegration.DebugCleanCash)
            {
                IntegrationLogWriter.CleanCashLogWrite(string.Format("Response:{0} - {1}", sendResultCode.ToString(), reciptId));
                IntegrationLogWriter.CleanCashLogWrite(string.Format("mCleanCashCom.LastExtendedError:{0} - {1}", lastExtendedError.ToString(), reciptId));
            }

            if (lastExtendedError != 0)
            {
                throw new ControlUnitException("Fel " + lastExtendedError.ToString() + " - Ring support", null);
            }
            if (sendResultCode == CommunicationResult.RC_E_LICENSE_EXCEEDED || sendResultCode == CommunicationResult.RC_E_INVALID_PARAMETER)
                throw new ControlUnitException("Fel " + lastExtendedError.ToString() + sendResultCode.ToString(), null);
            if (sendResultCode != CommunicationResult.RC_SUCCESS && sendResultCode != CommunicationResult.RC_E_NOT_CLEANCASH)
            {
                if (startResultCode != CommunicationResult.RC_SUCCESS)
                    throw new ControlUnitException("Kan ej skicka data till kontrollenheten (" + startResultCode.ToString() + ")", null);

                return new ControlUnitResponse(false);
            }
            String unitName = mCleanCashCom.UnitId;
            // LogWriter.LogWrite("C");

            if (String.IsNullOrEmpty(unitName))
            {
                unitName = "ERROR";
                //LogWriter.LogWrite("lastExtendedError");
            }

            String controlCode = mCleanCashCom.LastControlCode;
            // LogWriter.LogWrite("D");
            return new ControlUnitResponse(true, unitName, controlCode);
        }


        public ControlUnitResponse SendReceipt(Model.Receipt receipt, int attemptNo, bool isCopy = false)
        {
            return SendReceipt(receipt.PrintDate, receipt.ReceiptNumber, receipt.GrossAmount, receipt.NegativeAmount, receipt.VatDetails[0], receipt.VatDetails[1], receipt.VatDetails[2], receipt.VatDetails[3], attemptNo, isCopy);
        }


        private string formatVat(VAT vat)
        {
            string fmt = string.Format("{0};{1}", vat.VATPercent.ToString("0.00", CultureInfo.GetCultureInfo(DefaultsIntegration.CultureString)), vat.VATTotal.ToString("0.00", CultureInfo.GetCultureInfo("sv-SE")));
            return fmt;
        }

        public bool Dispose()
        {
            //mCleanCashCom = null;

            return true;
        }

        public bool Close()
        {
            isConnected = false;
            CommunicationResult resultCode = mCleanCashCom.Close();
            if (DefaultsIntegration.DebugCleanCash)
                IntegrationLogWriter.CleanCashLogWrite(string.Format("Close Response: {0}", resultCode));
            return resultCode == CommunicationResult.RC_SUCCESS;
        }
        //WAQAS_CHANGES
        public ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo)
        {
            return SendReceipt(receipt, attemptNo);
        }
        //WAQAS_CHANGES
        public ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo, bool isCopy = false)
        {
            return SendReceipt(receipt, attemptNo, isCopy);
        }
    }
}
