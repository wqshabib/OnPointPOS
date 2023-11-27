using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.ControlUnits.CleanCash
{
    public class CleanCashControlUnit2 : IControlUnit
    {
        SerialPort _port;
        SerialPort _prnPort;
        string ControlUnitConnectionString;
        public CleanCashControlUnit2(string ControlUnitConnectionString)
        {
            this.ControlUnitConnectionString = ControlUnitConnectionString;
            _port = new SerialPort();
            //_port = new SerialPort("COM7", 57600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        }
        public bool Open()
        {
            try
            {
                string[] _setting1 = ControlUnitConnectionString.Split(':');
                string portName = _setting1[0];
                int _baudRate = 0;
                string[] _setting2 = _setting1[1].Split(',');
                int.TryParse(_setting2[0], out _baudRate);
                if (_port == null)
                    _port = new SerialPort();
                if (_port.IsOpen)
                    _port.Close();
                _port.PortName = portName;
                if (_baudRate > 0)
                    _port.BaudRate = _baudRate;
                else
                    _port.BaudRate = 9600;
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                _port.ReadTimeout = 1000;
                _port.WriteTimeout = 5000;
                _port.NewLine = "\r";
                _port.Open();
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.LogWrite(ex);

                return false;
            }

            if (1 == 2)
            {
                try
                {
                    _prnPort.Open();
                }
                catch (Exception ex)
                {
                    IntegrationLogWriter.LogWrite(ex);
                    return false;
                }
            }

            //        if (SendReceiveCcspMessage(CcspClient.OutgoingMessageType.StatusInformationRequest)
            //!= CcspClient.IncomingMessageType.StatusInformationResponse)
            //        {
            //            throw new Exception("Ingen kontakt med kontrollenhet eller felaktigt svar\n");


            //        }
            return _port.IsOpen;


        }
        private CcspClient.IncomingMessageType SendReceiveCcspMessage(CcspClient.OutgoingMessageType type, string data)
        {
            List<string> fields;
            return SendReceiveCcspMessage(type, data, out fields);
        }

        private CcspClient.IncomingMessageType SendReceiveCcspMessage(CcspClient.OutgoingMessageType type)
        {
            List<string> fields;
            return SendReceiveCcspMessage(type, null, out fields);
        }

        private CcspClient.IncomingMessageType SendReceiveCcspMessage(CcspClient.OutgoingMessageType type, string data, out List<string> fields)
        {
            if (data == null)
            {
                data = "";
            }
            IntegrationLogWriter.CleanCashLogWrite("SendReceiveCcspMessage Attempt Data1 = " + data + "type : " + type );

            fields = null;
            CcspClient ccsp = new CcspClient();
            string msg = ccsp.Compose(CcspClient.Mode.A, type, data);
            try
            {
                for (int tries = 0; tries < 3; ++tries)
                {
                    _port.Write(msg);
                    string rsp = _port.ReadLine() + "\r";

                    try
                    {
                        ccsp.Parse(rsp);
                        fields = ccsp.Fields;
                        break;
                    }
                    catch (CcspNakException ex)
                    {
                        IntegrationLogWriter.CleanCashLogWrite("SendReceiveCcspMessage Attempt Data2 = " + ex.ToString() + ", (CcspNakException ex)");
                        //SHOW ALERT   
                    }
                }
            }
            catch (System.TimeoutException ex)
            {
                IntegrationLogWriter.CleanCashLogWrite("SendReceiveCcspMessage Attempt Data3 = " + ex.ToString() + ", System.TimeoutException ex");
                //SHOW ALERT
            }
            IntegrationLogWriter.CleanCashLogWrite("SendReceiveCcspMessage Attempt Data4 = " + ccsp.MessageType + ", Responce");
            return ccsp.MessageType;
        }



        public bool RegisterPOS(string orgNumber, string posId)
        {
            List<string> fields = new List<string>();
            //CcspClient.IncomingMessageType rsp;
            //if ((rsp = SendReceiveCcspMessage(CcspClient.OutgoingMessageType.IdentityRequest,
            //    null, out fields)) != CcspClient.IncomingMessageType.IdentityResponse)
            //{
            //    return false;
            //}
            //else
            //    return true;

            return true;
        }

        public ControlUnitStatus CheckStatus()
        {
            //if (SendReceiveCcspMessage(CcspClient.OutgoingMessageType.StatusInformationRequest)
            //    != CcspClient.IncomingMessageType.StatusInformationResponse)
            //{
            //    return ControlUnitStatus.ERROR_PLEASE_RETRY;
            //}
            //else
            //    return ControlUnitStatus.OK;
            return ControlUnitStatus.OK;
        }

        public ControlUnitResponse SendReceipt(DateTime dateTime, long reciptId, decimal totalAmount, decimal negativeAmount, VAT vat1, VAT vat2, VAT vat3, VAT vat4, int attemptNo, bool isCopy = false, bool isPerforma = false)
        {
            try
            {
                if (DefaultsIntegration.DebugCleanCash)
                {
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", Date: " + dateTime);
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", reciptId: " + reciptId);
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", totalAmount: " + totalAmount);
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", negativeAmount: " + negativeAmount);
                    if (vat1 == null)
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat1 is : null");
                    else
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat1: " + formatVat(vat1));
                    if (vat2 == null)
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat2 is : null");
                    else
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat2: " + formatVat(vat2));
                    if (vat3 == null)
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat3 is : null");
                    else
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat3: " + formatVat(vat3));
                    if (vat4 == null)
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat4 is : null");
                    else
                        IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", vat4: " + formatVat(vat4));
                }
                CcspClient.IncomingMessageType rsp;
                List<string> fields = new List<string>();

                if ((rsp = SendReceiveCcspMessage(CcspClient.OutgoingMessageType.StatusInformationRequest)) != CcspClient.IncomingMessageType.StatusInformationResponse)
                {
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", Control Unit is not connected");
                    return null;
                }

                if ((rsp = SendReceiveCcspMessage(CcspClient.OutgoingMessageType.StartReceipt)) != CcspClient.IncomingMessageType.Ack)
                {
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", Start receipt failed in control unit");
                    return null;
                }

                // LogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", Identitying Request");
                if ((rsp = SendReceiveCcspMessage(CcspClient.OutgoingMessageType.IdentityRequest, null, out fields)) != CcspClient.IncomingMessageType.IdentityResponse)
                {
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", Invalied indentification by Control Unit");
                    fields = new List<string>();
                    fields.Add("xxxxxxx");
                    return null;
                }

                string cleancashId = fields[0];

                string receiptType = "normal";

                if (isCopy)
                {
                    receiptType = "kopia";
                }
                else if (isPerforma)
                {
                    receiptType = "profo";
                }

                //if (Dafault DefaultsIntegration.User.TrainingMode)
                //{
                //    receiptType = "ovning";
                //}

                //Fix by Waqas, in case we have total amount in 
                //negative then it should be passed in begativeamount variable
                bool checkForNegativeAmount = totalAmount < 0 && negativeAmount != 0;

                if (totalAmount < 0)
                {
                    negativeAmount = totalAmount * (-1);
                }

                //TODO: 0 should be sent from orignal source : Quick fix by shahid
                if (totalAmount < 0 || negativeAmount > 0)
                    totalAmount = 0m;
                else
                    negativeAmount = 0m;

                
                // Build receipt header fields...
                //"201611011538#13#1#TedF# #1234567890#15,00#0,00#normal#6,00;0,85# # # "
                //"201611011539#21#1#111# #1234567890#15,00#0#normal#0,00;0,00#6,00;2,26#12,00;47,14#25,00;9,00"
                if (DefaultsIntegration.DebugCleanCash)
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", Formating Receipt");


                ///****Test code for Regular Expession*** Munir*/
                //Outlet outlet = new Outlet();
                //outlet.Active = true;
                //outlet.Address1 = "abc";
                //outlet.OrgNo = "559108-9890";
                //outlet.Phone = "555585";
                //outlet.Updated = DateTime.Now.Date;

                //User u = new User();

                //TillUser tu = new  TillUser();

                //Terminal t = new Terminal();


                //DefaultsIntegration.Outlet = outlet;

                ///********************/

                if (!DefaultsIntegration.IsInitialized)
                {
                    DefaultsIntegration.Init();
                }

                if (string.IsNullOrEmpty(DefaultsIntegration.Outlet.OrgNo))
                {
                    IntegrationLogWriter.CleanCashLogWrite("Attempt No = "+ attemptNo + ", Org No is null");
                }

                string data = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}" +
              "#{7}#{8}#{9}#{10}#{11}#{12}",
              string.Format(dateTime.ToString("yyyyMMddHHmm")),
              reciptId,
             DefaultsIntegration.Terminal.TerminalNo,
             DefaultsIntegration.User.UserCode,
              " ",
              DefaultsIntegration.Outlet.OrgNo.Replace("-", ""),
              FormatDecimal(totalAmount),
              FormatDecimal(negativeAmount),
              receiptType,
               formatVat(vat2, checkForNegativeAmount),
               formatVat(vat3, checkForNegativeAmount),
               formatVat(vat4, checkForNegativeAmount),
               formatVat(vat1, checkForNegativeAmount));

                if ((rsp = SendReceiveCcspMessage(CcspClient.OutgoingMessageType.ReceiptHeader,
                data, out fields)) != CcspClient.IncomingMessageType.Ack)
                {

                    IntegrationLogWriter.CleanCashLogWrite("Attempt Data = " + data + ",  ReceiptHeader");
                    return null;
                }
                if (DefaultsIntegration.DebugCleanCash)
                {
                    IntegrationLogWriter.CleanCashLogWrite("Attempt Data = " + data + ", Cleancash data to SKV");
                }
                // ++r.ReceiptNumber;


                // Note: only request signature for normal/kopia receipts
                if ((rsp = SendReceiveCcspMessage(CcspClient.OutgoingMessageType.SignatureRequest,
                    null, out fields)) != CcspClient.IncomingMessageType.SignatureResponse)
                {
                    IntegrationLogWriter.CleanCashLogWrite("SignatureRequest Attempt No = " + attemptNo + ", Signature failed in Control Unit");
                    fields = new List<string>();
                    fields.Add("xxxxxxx");
                    return null;

                    //  return null;
                }


                string signature = fields[0];
                ControlUnitResponse response = new ControlUnitResponse(true, cleancashId, signature);

                return response;
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.LogException(ex);
                return null;
            }

        }
        
        private string formatVat(VAT vat, bool checkForNegativeAmount = false)
        {
            if (vat.VATTotal == 0m) return " ";

            if(checkForNegativeAmount)
                return FormatDecimal(vat.VATPercent) + ";" + FormatDecimal(vat.VATTotal > 0 ? vat.VATTotal : (vat.VATTotal)*(-1));
            else
                return FormatDecimal(vat.VATPercent) + ";" + FormatDecimal(vat.VATTotal);
        }

        private string FormatDecimal(decimal amount)
        {
            return amount.ToString("0.00", CultureInfo.GetCultureInfo("sv-SE"));
        }

        public bool Close()
        {
            if (_port.IsOpen)
                _port.Close();
            return true;
        }

        public ControlUnitResponse SendReceipt(Receipt r, int attemptNo, bool isCopy = false)
        {
            var vat1 = r.VatDetails.First(v => v.VATPercent == 0);
            var vat2 = r.VatDetails.First(v => v.VATPercent == 6);
            var vat3 = r.VatDetails.First(v => v.VATPercent == 12);
            var vat4 = r.VatDetails.First(v => v.VATPercent == 25);

         

            return SendReceipt(r.PrintDate, r.ReceiptNumber, r.GrossAmount, r.NegativeAmount, vat1, vat2, vat3, vat4,attemptNo, isCopy);
        }

        public bool Dispose()
        {
            if (_port != null)
                _port = null;
            return true;
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
