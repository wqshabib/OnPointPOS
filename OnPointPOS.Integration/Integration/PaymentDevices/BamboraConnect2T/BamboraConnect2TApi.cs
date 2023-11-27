using Bambora.Connect2T;
using Newtonsoft.Json;
using POSSUM.Integration.Integration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace POSSUM.Integration.PaymentDevices.BamboraConnect2T
{
    public class BamboraConnect2TApi
    {

        #region BamboraConstants
        //==============================
        //Constanst for start method
        public enum TransactionTypes
        {
            OTHER = 0,
            LPP_SIGNATUREPURCHASE = 4000,
            LPP_PURCHASE = 4352,
            LPP_REFUND = 4353,
            LPP_REVERSAL = 4354,
            LPP_CLOSEBATCH = 4358
        };
        public enum ResultDataTypes
        {
            rdCUSTOMERRECEIPT = 1,
            rdMERCHANTRECEIPT,
            rdCLOSEBATCHRESULT,
            rdTRMCONFIG,
            rdCURRENTBATCH,
            rdTRANSLOGDETAILED,
            rdTRANSLOGTOTALS,
            rdUNSENTTRANS,
            rdUNDEFINED = -1
        };
        public enum ReceiptItems
        {
            riName = 1,
            riAddress = 2,
            riPostAddress = 3,
            riPhone = 4,
            riOrgNbr = 5,
            riBank = 6,
            riMerchantNbr = 7,
            riTrmId = 8,
            riTime = 9,
            riAmount = 10,
            riTotal = 11,
            riVAT = 12,
            riCashBack = 13,
            riCardNo = 14,
            riCardNoMasked = 15,
            riExpDate = 16,
            riCardProduct = 17,
            riAccountType = 18,
            riAuthInfo = 19,
            riReferenceNo = 20,
            riAcceptanceText = 21,
            riIdLine = 22,
            riSignatureLine = 23,
            riRefundAcceptanceText = 24,
            riCashierSignatureText = 25,
            riCashierNameText = 26,
            riTxnType = 27,
            riSaveReceipt = 28,
            riCustomerEx = 29,
            riPinUsed = 30,
            riEnd = -1,
        };

        public enum ResultDataValues { rdEnd = -1 };
        #endregion



        private string paymentDeviceTypeConnectionString;
        static readonly object _locker = new object();
        private int cookie;
        private Bambora.Connect2T.Connection api;
        private bool isConnected;
        private IPaymentDeviceEventHandler eventHandler = null;
        public bool transactionStarted;
        private TransactionTypes transactionType;
        public Dispatcher uiDispatcher { get; set; }
        public Dispatcher terminalDispatcher { get; set; }
        //BabsEventHandler babsEventHandler = null;
        private bool paymentDialogCloseForced = false;

        //public static String TAG = "BamboraConnect2TApi: ";

        private string _tag = null;

        public string TAG
        {
            get
            {
                if (string.IsNullOrEmpty(_tag))
                {
                    if (api != null)
                    {
                        _tag = "BamboraConnect2TApi ";
                        _tag = _tag + ": Version = " + api.CurrentLibVersion;
                        _tag = _tag + ": IntegrationKey = " + api.IntegrationKey;
                        _tag = _tag + ": Protocol Version = " + "1.0";
                        _tag = _tag + ": Log = ";
                        return _tag;
                    }
                    else
                    {
                        return "BamboraConnect2TApi: ";
                    }
                }
                else
                {
                    return _tag;
                }
            }
        }

        /*  PaymentDevices\BabsBpti>tlbimp BPTI.dll /out:BPT
         *  I /out:BPTIInterop.dll
            Microsoft (R) .NET Framework Type Library to Assembly Converter 4.0.30319.33440
            Copyright (C) Microsoft Corporation.  All rights reserved.


            POSSUM\Integration\PaymentDevices\BabsBpti\BPTIInterop.dll */
        public BamboraConnect2TApi(string paymentDeviceTypeConnectionString, Dispatcher dispatcher)
        {
            this.LastStatus = 0;
            this.uiDispatcher = dispatcher;
            this.terminalDispatcher = Dispatcher.CurrentDispatcher;
            this.paymentDeviceTypeConnectionString = paymentDeviceTypeConnectionString;
            this.isConnected = false;

            Connect();
        }



        private void ResponseReceived(object source, MessageReceivedArgs e)
        {
            Action a = null;
            bool signatureneeded = false;

            try
            {

                Response Parameters = e.GetData(); Console.WriteLine(); Console.WriteLine("-- ResponseReceived --");
                IntegrationLogWriter.LogWrite(TAG + string.Format("ResponseReceived 1 == >> ", e.GetData()));
                if (Parameters != null)
                {
                    string jSonOutput = JsonConvert.SerializeObject(Parameters);
                    Console.WriteLine(jSonOutput);
                    IntegrationLogWriter.LogWrite(TAG + string.Format("Response Received {0}", jSonOutput));

                    // LogWriter.BamboraWrite("Response_"+DateTime.Now.Day+"_" +DateTime.Now.Minute.ToString()+"_"+DateTime.Now.Second, jSonOutput);
                }

                if (Parameters != null && Parameters.DialogText == "Ange PIN + OK:")
                {
                    a = () => eventHandler.onBamboraSignatureRequired("Ange PIN + OK:");

                    // a = () => eventHandler.onTerminalCancelEvent(Parameters.DialogText + " " + Parameters.DialogTitle);
                    //MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("PaymentTerminal_Endorsement" + "?", "PaymentTerminal_SignatureBuy", System.Windows.MessageBoxButton.YesNo);
                    //if (messageBoxResult == MessageBoxResult.No)
                    //{
                    //    Cancel();
                    //    // paymentDevice.GetCustomerReceipt();
                    //}

                }


                //if (Parameters != null && Parameters.DialogText != null && Parameters.DialogText.Contains("medges ej lokalt nekad")) ;
                //{
                //    a = () => eventHandler.onBamboraOutOfBalance("Insufficient balance");
                //    return;
                //    // a = () => eventHandler.onTerminalCancelEvent(Parameters.DialogText + " " + Parameters.DialogTitle);
                //    //MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Balance is not enough. ", "Balance is not enough", System.Windows.MessageBoxButton.OK);
                //    //    if (messageBoxResult == MessageBoxResult.OK)
                //    //    {
                //    //        Cancel();
                //    //        // paymentDevice.GetCustomerReceipt();
                //    //    }
                //}

                Console.WriteLine("MessageInfo: " + e.GetMessageInfo());
                // Result is mandatory
                Console.WriteLine("Result = " + Parameters.Result.ToString());

                string CardholderLanguage; // null check, CardholderLanguage is optional. if (Parameters.CardholderLanguage != null) CardholderLanguage = Parameters.CardholderLanguage.ToString();

                Console.WriteLine("DID = " + Parameters.DialogId.ToString());
                Console.WriteLine("Status = " + Parameters.Status.ToString());
                Console.WriteLine("SettleReference = " + Parameters.TransactionType.ToString());
                Console.WriteLine("TransactionResult = " + Parameters.TransactionResult.ToString());

                if (Parameters.DialogId != null)
                {

                    if (this.eventHandler != null)
                    {

                        String txtrow1 = Parameters.DialogId.ToString();
                        String txtrow2 = Parameters.DialogTitle;
                        String txtrow3 = Parameters.DialogText;
                        String txtrow4 = "";
                        a = () => eventHandler.onTerminalDisplay(txtrow1, txtrow2, txtrow3, txtrow4, Parameters.Status, Parameters.TransactionType, Parameters.TransactionResult);

                        //dispatchToUi(a);

                    }
                }
                else if (Parameters.Result == 0)
                {
                    if (Parameters.TransactionResult.ToString().Length == 0)
                    {

                        this.LastStatus = 1;
                        Console.WriteLine("Connected to Bambora Terminal.");
                        IntegrationLogWriter.LogWrite(TAG + string.Format("Connected to Bambora Terminal."));
                        this.transactionStarted = true;
                        this.eventHandler.onTransactionStart();

                    }
                    else if (Parameters.TransactionResult.ToString().Length != 0 && Parameters.TransactionResult == (int)BamboraTransactionResult.Approved)
                    {
                        //Payment approved
                        Console.WriteLine("TransactionResult: " + Parameters.TransactionResult);
                        IntegrationLogWriter.LogWrite(TAG + "TransactionResult: " + Parameters.TransactionResult);

                        List<string> receiptdata = new List<string>();

                        String psn = "";
                        String arc = "";

                        foreach (var element in Parameters.EMVData)
                        {
                            if (element.key == "PSN")
                            {
                                psn = element.value;
                            }
                            else if (element.key == "ARC")
                            {
                                arc = element.value;
                            }
                            Console.WriteLine(element.key + ": " + element.value);
                        }

                        Console.WriteLine("verificationMethod" + Parameters.VerificationMethod);

                        double amount = System.Convert.ToDouble(Parameters.BaseAmount) / 100;
                        double vat = System.Convert.ToDouble(Parameters.VATAmount) / 100;
                        double total = System.Convert.ToDouble(Parameters.TotalAmount) / 100;

                        if (this.transactionType == TransactionTypes.LPP_REFUND)
                        {
                            receiptdata.Add("ÅTERKÖP GODKÄNT");
                            receiptdata.Add("");

                            receiptdata.Add("Total (SEK):  " + total.ToString("C", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");
                        }
                        else if (this.transactionType == TransactionTypes.LPP_REVERSAL)
                        {
                            receiptdata.Add("KÖP MAKULERAT");
                            //TODO: Take Órignal order Recipt using ReversalReference and use that just replace new InternalReference ('KVITTERING')  As amount and reset of the information is coming null. Handle here or under Receipt Handler
                            /*receiptdata.Add("");

                            receiptdata.Add("Belopp (SEK): " + amount.ToString("N") + " kr");
                            receiptdata.Add("Moms (SEK):   " + vat.ToString("N", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");
                            receiptdata.Add("------------------------------------");
                            receiptdata.Add("Total (SEK):  " + total.ToString("C", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");*/
                        }
                        else
                        {
                            receiptdata.Add("KÖP GODKÄNT");
                            receiptdata.Add("");

                            receiptdata.Add("Belopp (SEK): " + amount.ToString("N") + " kr");
                            receiptdata.Add("Moms (SEK):   " + vat.ToString("N", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");
                            receiptdata.Add("------------------------------------");
                            receiptdata.Add("Total (SEK):  " + total.ToString("C", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");
                        }
                        if (Parameters.DccAmount > 0)
                        {

                            double exchangeRate = System.Convert.ToDouble(Parameters.DccMarginRate) * 100;
                            double dccAmount = System.Convert.ToDouble(Parameters.DccAmount) / 100;
                            receiptdata.Add("");
                            receiptdata.Add("");
                            receiptdata.Add($"TRANSACTION CURRENCY:  {Parameters.DccCurrencySymbol} {dccAmount}");
                            receiptdata.Add("");
                            receiptdata.Add($"EXCHANGE RATE:  1 SEK = {Parameters.DccExchangeRate}");
                            receiptdata.Add("");
                            receiptdata.Add($"MARK UP ON ECHANGE RATE %:  {exchangeRate}");
                            receiptdata.Add("");
                            receiptdata.Add(Parameters.DccCardholderDisclaimer);
                        }
                        receiptdata.Add("");

                        //amount = 30;

                        //receiptdata.Add("Cashback:     " + amount.ToString("C", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");
                        //receiptdata.Add("");

                        if (Parameters.AccountType != null)
                        {
                            if (Parameters.AccountType.Equals('C'))
                            {
                                receiptdata.Add("Belastat Kredit");
                            }
                            else if (Parameters.AccountType.Equals('D'))
                            {
                                receiptdata.Add("Belastat Konto");
                            }
                        }

                        bool isVisaCard = false;
                        bool hasContactLess = false;
                        foreach (var element in Parameters.EMVData)
                        {
                            if (element.key == "AID" && element.value != null && element.value.StartsWith("A000000003"))
                            {
                                isVisaCard = true;
                            }
                        }

                        if (Parameters.EntryMode == 'K' || Parameters.EntryMode == 'L')
                        {
                            hasContactLess = true;
                        }

                        if (hasContactLess)
                        {
                            if (isVisaCard)
                            {
                                receiptdata.Add("VISA CONTACTLESS");
                            }
                            else
                            {
                                receiptdata.Add("CONTACTLESS");
                            }
                        }

                        receiptdata.Add(Parameters.ProductName);
                        if (string.IsNullOrEmpty(psn))
                            receiptdata.Add(Parameters.MaskedPAN);
                        else
                            receiptdata.Add(Parameters.MaskedPAN + "             PSN: " + psn);
                        receiptdata.Add("");

                        String handlingstring = "";

                        handlingstring = (Parameters.EntryMode == null || Parameters.EntryMode.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.EntryMode.ToString();
                        handlingstring = (Parameters.VerificationMethod == null || Parameters.VerificationMethod.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.VerificationMethod.ToString();
                        handlingstring = (Parameters.AuthorizationMethod == null || Parameters.AuthorizationMethod.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.AuthorizationMethod.ToString();
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.AuthorizationResponder == null || Parameters.AuthorizationResponder.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.AuthorizationResponder.ToString();
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.SPDHResponseCode == null || Parameters.SPDHResponseCode == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.SPDHResponseCode;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.FinancialInstitution == null || Parameters.FinancialInstitution == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.FinancialInstitution;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.BatchReference == null || Parameters.BatchReference == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.BatchReference;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.ApprovalCode == null || Parameters.ApprovalCode == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.ApprovalCode;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.TransactionResult == null || Parameters.TransactionResult.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.TransactionResult.ToString();


                        receiptdata.Add(handlingstring);
                        if (Parameters.TransactionReference != null)
                        {
                            receiptdata.Add("REF: " + Parameters.TransactionReference + " / " + Parameters.RetrievalReference);
                        }
                        else
                        {
                            receiptdata.Add("REF: " + Parameters.RetrievalReference);
                        }
                        receiptdata.Add("KVITTERING: " + Parameters.InternalReference);
                        // receiptdata.Add("AUT.CODE: " + Parameters.ApprovalCode);
                        receiptdata.Add("");

                        receiptdata.Add("BUTIK: " + Parameters.AcquirerId + "    " + Parameters.MerchantId);
                        receiptdata.Add("TERMINAL: " + Parameters.TerminalId);
                        receiptdata.Add("");

                        foreach (var element in Parameters.EMVData)
                        {
                            if (element.key == "ATC")
                            {
                                if (string.IsNullOrEmpty(arc))
                                {
                                    receiptdata.Add(element.key + ": " + element.value);
                                }
                                else
                                {
                                    receiptdata.Add(element.key + ": " + element.value + "                   ARC: " + arc);
                                }
                            }
                            else if (element.key == "AED" || element.key == "AID" || element.key == "TVR" || element.key == "TSI")
                            {
                                receiptdata.Add(element.key + ": " + element.value);
                            }
                        }
                        receiptdata.Add("");

                        if (Parameters.VerificationMethod == '@')
                        {
                            signatureneeded = true;
                        }
                        if (Parameters.EncryptedInformation.Count > 0)
                        {
                            receiptdata.Add("");

                            receiptdata.Add("ENCRYPTED INFORMATION");
                            if (!string.IsNullOrEmpty(Parameters.EncryptedInformation[0]))
                                receiptdata.Add("KSN: " + Parameters.EncryptedInformation[0]);
                            if (Parameters.EncryptedInformation.Count >= 2 && !string.IsNullOrEmpty(Parameters.EncryptedInformation[1]))
                                receiptdata.Add("INFO-DATA: " + Parameters.EncryptedInformation[1]);
                            receiptdata.Add("");
                        }

                        //List<string> customerreceiptdata = new List<string>(receiptdata);

                        //List<string> merchantreceiptdata = new List<string>(receiptdata);


                        // a = () => this.eventHandler.onTerminalReceiptData(PaymentDataType.CUSTOMER_RECEIPT, PaymentDataItemType.END, "A", "B");
                        if (this.transactionType == TransactionTypes.LPP_REFUND)
                        {
                            if (receiptdata != null &&
                                (receiptdata.Count(b => b.Contains("Failed to send message")) > 0)
                                || receiptdata.Count(b => b.Contains("Terminal Connection Failure")) > 0)

                            {
                                IntegrationLogWriter.LogWrite(TAG + "Returning due to invalid receipt: " + string.Join("|", receiptdata));
                                return;
                            }

                            a = () => eventHandler.onBamboraEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.REFUND, true, "Köp klart", receiptdata, receiptdata, true, Parameters.ProductName, total);
                        }
                        else if (this.transactionType == TransactionTypes.LPP_REVERSAL)
                        {
                            //TODO: MERCHANT RECEIPT IS NOT NEEDED FOR Reversal
                            if (receiptdata != null &&
                               (receiptdata.Count(b => b.Contains("Failed to send message")) > 0)
                               || receiptdata.Count(b => b.Contains("Terminal Connection Failure")) > 0)

                            {
                                IntegrationLogWriter.LogWrite(TAG + "Returning due to invalid receipt: " + string.Join("|", receiptdata));
                                return;
                            }

                            a = () => eventHandler.onBamboraEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.REVERSAL, true, "KÖP MAKULERAT", receiptdata, receiptdata, false, Parameters.ProductName, total);
                        }
                        else
                        {
                            // You see this ?
                            // Here it is throwing this event
                            // and we are capturing this event in our code and doing our stuff
                            // So you have to create an another event
                            // same like this
                            // and capture it in your application.
                            // Got it ?
                            // You will write like this for example
                            if (receiptdata != null &&
                               (receiptdata.Count(b => b.Contains("Failed to send message")) > 0)
                               || receiptdata.Count(b => b.Contains("Terminal Connection Failure")) > 0)

                            {
                                IntegrationLogWriter.LogWrite(TAG + "Returning due to invalid receipt: " + string.Join("|", receiptdata));
                                return;
                            }

                            a = () => eventHandler.onBamboraEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.PURCHASE, true, "Köp klart", receiptdata, receiptdata, signatureneeded, Parameters.ProductName, total);
                        }
                        //dispatchToUi(a);

                        //a = () => this.eventHandler.onTerminalReceiptData(, "C", "D");
                        //dispatchToUi(a);

                        //this.EndTransaction();
                    }
                    else if (Parameters.TransactionResult.ToString().Length != 0 && (Parameters.TransactionResult == (int)BamboraTransactionResult.Declined || Parameters.TransactionResult == (int)BamboraTransactionResult.Cancelled))
                    {
                        if (Parameters.TransactionResult == (int)BamboraTransactionResult.Cancelled)
                        {
                            a = () => this.eventHandler.onTerminalCancelEvent("Payment cancelled");
                            Console.WriteLine("Payment cancelled");
                            IntegrationLogWriter.LogWrite(TAG + string.Format("Payment cancelled"));
                        }
                        else
                        {
                            a = () => this.eventHandler.onTerminalDeclinedEvent("Payment declined");
                            Console.WriteLine("Payment declined");
                            IntegrationLogWriter.LogWrite(TAG + string.Format("Payment declined"));
                        }
                        Console.WriteLine("TransactionResult: " + Parameters.TransactionResult);

                        String psn = "";
                        String arc = "";

                        foreach (var element in Parameters.EMVData)
                        {
                            if (element.key == "PSN")
                            {
                                psn = element.value;
                            }
                            else if (element.key == "ARC")
                            {
                                arc = element.value;
                            }
                            Console.WriteLine(element.key + ": " + element.value);
                        }

                        double amount = System.Convert.ToDouble(Parameters.BaseAmount) / 100;
                        double vat = System.Convert.ToDouble(Parameters.VATAmount) / 100;
                        double total = System.Convert.ToDouble(Parameters.TotalAmount) / 100;
                        var paymentStatus = Parameters.Status.ToString().Trim();

                        List<string> receiptdata = new List<string>();
                        receiptdata.Add("");
                        receiptdata.Add("");
                        receiptdata.Add("                              KÖP");
                        receiptdata.Add("***************************************");
                        if (Parameters.TransactionResult == (int)BamboraTransactionResult.Cancelled)
                        {
                            receiptdata.Add("                           AVBRUTET");
                        }
                        else
                        {
                            receiptdata.Add("                          MEDGES EJ");
                        }

                        if (paymentStatus.Equals("1832")) receiptdata.Add("                Kontakta din kortutgivare");
                        else if (paymentStatus.Equals("1825")) receiptdata.Add("                          Tekniskt fel");
                        else if (paymentStatus.Equals("1103")) receiptdata.Add("                         Endast online");
                        else if (paymentStatus.Equals("1091")) receiptdata.Add("                           Ej tillåtet");
                        else if (paymentStatus.Equals("790") || paymentStatus.Equals("791")) receiptdata.Add("                Medges ej - Lokalt nekad");
                        else if (paymentStatus.Equals("775")) receiptdata.Add("                              kortfel");
                        receiptdata.Add("***************************************");
                        receiptdata.Add("");
                        receiptdata.Add("20" + Parameters.Timestamp.Substring(0, 2) + "-" + Parameters.Timestamp.Substring(2, 2) + "-" + Parameters.Timestamp.Substring(4, 2) + " " + Parameters.Timestamp.Substring(6, 2) + ":" + Parameters.Timestamp.Substring(8, 2) + ":" + Parameters.Timestamp.Substring(10, 2));
                        receiptdata.Add("");
                        receiptdata.Add("Belopp (SEK): " + amount.ToString("C", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");
                        receiptdata.Add("Moms (SEK):   " + vat.ToString("C", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");
                        receiptdata.Add("------------------------------------");
                        receiptdata.Add("Total (SEK):  " + total.ToString("C", Utility.UICultureInfoWithoutCurrencySymbol) + " kr");

                        receiptdata.Add("");
                        receiptdata.Add("");

                        bool isVisaCard = false;
                        bool hasContactLess = false;
                        foreach (var element in Parameters.EMVData)
                        {
                            if (element.key == "AID" && element.value != null && element.value.StartsWith("A000000003"))
                            {
                                isVisaCard = true;
                            }
                        }

                        if (Parameters.EntryMode == 'K' || Parameters.EntryMode == 'L')
                        {
                            hasContactLess = true;
                        }

                        if (hasContactLess)
                        {
                            if (isVisaCard)
                            {
                                receiptdata.Add("VISA CONTACTLESS");
                            }
                            else
                            {
                                receiptdata.Add("CONTACTLESS");
                            }
                        }

                        receiptdata.Add(Parameters.ProductName);
                        receiptdata.Add(Parameters.MaskedPAN + "             PSN: " + psn);
                        receiptdata.Add("");

                        String handlingstring = "";

                        handlingstring = (Parameters.EntryMode == null || Parameters.EntryMode.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.EntryMode.ToString();
                        handlingstring = (Parameters.VerificationMethod == null || Parameters.VerificationMethod.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.VerificationMethod.ToString();
                        handlingstring = (Parameters.AuthorizationMethod == null || Parameters.AuthorizationMethod.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.AuthorizationMethod.ToString();
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.AuthorizationResponder == null || Parameters.AuthorizationResponder.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.AuthorizationResponder.ToString();
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.SPDHResponseCode == null || Parameters.SPDHResponseCode == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.SPDHResponseCode;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.FinancialInstitution == null || Parameters.FinancialInstitution == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.FinancialInstitution;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.BatchReference == null || Parameters.BatchReference == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.BatchReference;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.ApprovalCode == null || Parameters.ApprovalCode == "") ? handlingstring = handlingstring + "___" : handlingstring = handlingstring + Parameters.ApprovalCode;
                        handlingstring = handlingstring + " ";
                        handlingstring = (Parameters.TransactionResult == null || Parameters.TransactionResult.ToString() == "") ? handlingstring = handlingstring + "-" : handlingstring = handlingstring + Parameters.TransactionResult.ToString();


                        receiptdata.Add(handlingstring);
                        if (Parameters.TransactionReference != null)
                        {
                            receiptdata.Add("REF: " + Parameters.TransactionReference + " / " + Parameters.RetrievalReference);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Parameters.RetrievalReference))
                                receiptdata.Add("REF: " + Parameters.RetrievalReference);
                        }
                        receiptdata.Add("KVITTERING: " + Parameters.InternalReference);
                        //if (!string.IsNullOrEmpty(Parameters.ApprovalCode))
                        //    receiptdata.Add("AUT.CODE: " + Parameters.ApprovalCode);
                        receiptdata.Add("");

                        receiptdata.Add("BUTIK: " + Parameters.AcquirerId + "    " + Parameters.MerchantId);
                        receiptdata.Add("TERMINAL: " + Parameters.TerminalId);
                        receiptdata.Add("");

                        foreach (var element in Parameters.EMVData)
                        {
                            if (element.key == "ATC")
                            {
                                receiptdata.Add(element.key + ": " + element.value);
                            }
                            else if (element.key == "AED" || element.key == "AID" || element.key == "TVR" || element.key == "TSI")
                            {
                                receiptdata.Add(element.key + ": " + element.value);
                            }
                        }
                        receiptdata.Add("");

                        foreach (var element in Parameters.EncryptedInformation)
                        {
                            receiptdata.Add(element);
                        }

                        if (Parameters.EncryptedInformation.Count > 0)
                        {
                            receiptdata.Add("");
                        }

                        receiptdata.Add("Status:" + paymentStatus);
                        receiptdata.Add("");
                        receiptdata.Add("");
                        receiptdata.Add("");
                        receiptdata.Add("");
                        receiptdata.Add("*** SPARA KVITTOT, KUNDENS KOPIA ");
                        var paymentWindowTitle = (Parameters.TransactionResult == (int)BamboraTransactionResult.Cancelled) ? "KÖP AVBRUTET" : "KÖP MEDGES EJ";
                        if (this.transactionType == TransactionTypes.LPP_REFUND)
                        {
                            //TODO: Should only give close button
                            a = () => eventHandler.onBamboraEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.REFUND, false, paymentWindowTitle, receiptdata, receiptdata, signatureneeded, Parameters.ProductName, total);
                        }
                        else if (this.transactionType == TransactionTypes.LPP_REVERSAL)
                        {
                            //TODO: Test the behaviour
                            a = () => eventHandler.onBamboraEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.REVERSAL, false, paymentWindowTitle, receiptdata, receiptdata, false, Parameters.ProductName, total);
                        }
                        else
                        {
                            //TODO: Should only give close button
                            a = () => eventHandler.onBamboraEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.PURCHASE, false, paymentWindowTitle, receiptdata, receiptdata, signatureneeded, Parameters.ProductName, total);
                        }


                    }

                    else if (Parameters.TransactionResult.ToString().Length != 0 && Parameters.TransactionResult == 2)
                    {
                        // Payment canceled
                        // When reach here, show close button
                        a = () => eventHandler.onTerminalCancelEvent("Avbrutet");

                    }


                }
                else if (Parameters.Result == 204)
                {
                    // this.ResetConnection();
                    this.CloseBatch();
                    a = () => eventHandler.onTerminalReConnectEvent("Attempting settlement");
                }
                else if (Parameters.Result == 218)
                {
                    // this.ResetConnection();
                    a = () => eventHandler.onTerminalReConnectEvent("Terminal busy");
                }

                else if (Parameters.Result == null && Parameters.ReceiptData != null)
                {
                    //var receipt = Parameters.ReceiptData.Split('\n').ToList();

                    //for (int i = 0; i < receipt.Count; i++)
                    //{
                    //    if (receipt[i] != null &&  receipt[i].Trim() == "RKÖP")
                    //    {
                    //        receipt[i] = receipt[i].Replace("RKÖP", "KÖP");
                    //    }

                    //    if (receipt[i] != null && receipt[i].Contains("BELOPPBSEK"))
                    //    {
                    //        receipt[i] = receipt[i].Replace("BELOPPBSEK", "BELOPP (SEK): ");
                    //    }

                    //    if (receipt[i] != null && receipt[i].Contains("MOMSBSEK"))
                    //    {
                    //        receipt[i] = receipt[i].Replace("MOMSBSEK", "MOMS (SEK): ");
                    //    }

                    //    if (receipt[i] != null && receipt[i].Contains("RTOTALBSEK"))
                    //    {
                    //        receipt[i] = receipt[i].Replace("RTOTALBSEK", "TOTAL (SEK): ");
                    //    }
                    //}

                    //a = () => eventHandler.onBamboraEvent(PaymentDataType.MERCHANT_RECEIPT, PaymentDataItemType.END, PaymentTransactionType.PURCHASE, false, "Re-Print", receipt, receipt, true, Parameters.ProductName, 0);
                }

                if (a != null)
                {
                    dispatchToUi(a);

                }

            }
            catch (Exception ex)
            {
                IntegrationLogWriter.LogWrite(TAG + "Exception: " + ex.ToString());
            }
        }

        public void RePrintLastCancelledPayment()
        {
            IntegrationLogWriter.LogWrite(TAG + "Calling Reprint...");
            GetTransactionRequest getTransactionRequest = new GetTransactionRequest();
            getTransactionRequest.PrintReceipt = true;
            getTransactionRequest.SendReceipt = true;

            IntegrationLogWriter.LogWrite(TAG + string.Format("SND (GetTransactionRequest)"));

            api.SendRequest(getTransactionRequest);
        }

        internal void SetPaymentDialogCloseForced(bool forced)
        {
            paymentDialogCloseForced = forced;
        }

        private void ErrorMessage(object source, MessageReceivedArgs e)
        {
            try
            {
                Action a = null;

                Response Parameters = e.GetData(); Console.WriteLine();
                Console.WriteLine("-- ErrorMessage --");
                IntegrationLogWriter.LogWrite("ErrorMessage 1 = >> " + TAG + string.Format(e.GetMessageInfo()));
                if (this.eventHandler != null)
                {

                    a = () => eventHandler.onInfoEvent(e.GetMessageInfo());

                    dispatchToUi(a);

                }

                // Result is mandatory Console.WriteLine("Result = " + Parameters.Result.ToString());
                string CardholderLanguage; // null check, CardholderLanguage is optional. if (Parameters.CardholderLanguage != null) CardholderLanguage = Parameters.CardholderLanguage.ToString();
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.LogWrite(TAG + "Exception: " + ex.ToString());
            }
        }

        private static void InternetMessage(object source, MessageReceivedArgs e)
        {
            try
            {
                Response Parameters = e.GetData(); Console.WriteLine();

                Console.WriteLine("-- InternetMessage --");
                IntegrationLogWriter.LogWrite("InternetMessage 1 >> = " + e.GetData());
                if (Parameters != null)
                {
                    string jSonOutput = JsonConvert.SerializeObject(Parameters);
                    Console.WriteLine(jSonOutput);
                    IntegrationLogWriter.LogWrite("BamboraConnect2TApi: " + string.Format(jSonOutput));
                }

                Console.WriteLine("MessageInfo: " + e.GetMessageInfo());
                Console.WriteLine("DID = " + Parameters.DialogId.ToString());
                Console.WriteLine("Status = " + Parameters.Status.ToString());
                Console.WriteLine("SettleReference = " + Parameters.TransactionType.ToString());
                Console.WriteLine("TransactionResult = " + Parameters.TransactionResult.ToString());
                // Result is mandatory Console.WriteLine("Result = " + Parameters.Result.ToString());
                string CardholderLanguage; // null check, CardholderLanguage is optional. if (Parameters.CardholderLanguage != null) CardholderLanguage = Parameters.CardholderLanguage.ToString();
            }
            catch (Exception ex) { }
        }

        #region Connect/Disconnect

        public bool IsConnected()
        {
            return this.isConnected;
        }

        public bool Connect()
        {
            if (isConnected)
            {
                return false;
            }

            try
            {


                Uri uri = new Uri(paymentDeviceTypeConnectionString);
                if (uri.Scheme.ToLower() == "connect2t")
                {
                    string host = uri.Host;
                    int port = uri.Port;
                    Debug.WriteLine(TAG + "SND (Connect2T Connection) {0} {1}", host, port);
                    IntegrationLogWriter.LogWrite(TAG + string.Format("SND (Connect2T Before Connection) {0} {1}", host, port));
                    this.api = new Bambora.Connect2T.Connection(host, port, new Connection.ResponseCallback(ResponseReceived), new Connection.ResponseCallback(ErrorMessage), new Connection.ResponseCallback(InternetMessage));
                    IntegrationLogWriter.LogWrite("this.api == > " + this.api);
                    var integrationKey = ConfigurationManager.AppSettings["IntegrationKey"];
                    if (!string.IsNullOrEmpty(integrationKey))
                    {
                        this.api.IntegrationKey = integrationKey;
                    }

                    IntegrationLogWriter.LogWrite(TAG + string.Format("SND (Connect2T Connection) {0} {1}", host, port));

                    int logLevel = 0;
                    int.TryParse(ConfigurationManager.AppSettings["Bambora_LogLevel"], out logLevel);
                    if (logLevel > 0 && logLevel <= 3)
                    {
                        this.api.LogPath = Directory.GetCurrentDirectory() + "\\Logs";
                        this.api.LogPrefix = "Bambora_";
                        this.api.LogMode = logLevel;
                    }
                    this.LastStatus = 1;
                    isConnected = true;// this.api != null ? this.api.ConnectionStatus : true;// true; //Change By Arshad:assigned connection status
                    paymentDialogCloseForced = false;
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.LogWrite("Connect() ==>> " + ex + "  isConnected=>>" + isConnected);
            }
            return isConnected;
        }

        public bool Disconnect()
        {
            if (IsConnected())
            {
                Debug.WriteLine(TAG + "SND (Disconnect / Dispose)");
                IntegrationLogWriter.LogWrite(TAG + "SND (Disconnect / Dispose)");

                api.Dispose();
                isConnected = false;

            }
            return true;
        }

        #endregion

        internal void StartTransaction(TransactionTypes transactionType)
        {
            Debug.WriteLine(TAG + "SND (start[Transaction]) {0}", transactionType);
            IntegrationLogWriter.LogWrite(TAG + "SND (start[Transaction]) " + transactionType);
            this.transactionStarted = true;
            this.transactionType = transactionType;

            //api.start((int)transactionType);
            /*
            Action a = () => {
                if(!transactionStarted)
                {
                    api.start((int)transactionType);
                }

            };
            uiDispatcher.Invoke(a);*/
        }


        internal void EndTransaction()
        {
            Debug.WriteLine(TAG + "SND (endTransaction)");
            IntegrationLogWriter.LogWrite(TAG + "SND (endTransaction)");
            //this.Cancel();

            //api.endTransaction();
        }

        internal void SendAmount(decimal debitCardAmount, decimal vat, decimal cashback, Guid orderId)
        {
            int amount = (int)Math.Round(debitCardAmount * 100, 0);
            int vatamount = (int)Math.Round(vat * 100, 0);
            int cbAmount = (int)Math.Round(cashback * 100, 0);

            Debug.WriteLine(TAG + "SND (sendAmounts) {0} {1} {2}", amount, vatamount, cbAmount);
            IntegrationLogWriter.LogWrite(TAG + string.Format("SND (sendAmounts) {0} {1} {2}", amount, vatamount, cbAmount));
            //  LogWriter.LogWrite(TAG +string.Format( "SND (sendAmounts) {0} {1} {2}", amount, vatamount, cbAmount));
            if (this.transactionType == TransactionTypes.LPP_REFUND)
            {
                RefundRequest refund = new RefundRequest();
                refund.CurrencyCode = "0752";
                refund.BaseAmount = amount + cbAmount;
                refund.EnableDialog = '1';
                refund.EnableExternalNetworking = false;
                refund.PrintReceipt = false;
                IntegrationLogWriter.LogWrite(TAG + string.Format("SND (RefundRequest) {0} {1} {2}", amount, vatamount, cbAmount));

                api.SendRequest(refund);
            }
            else if (this.transactionType == TransactionTypes.LPP_REVERSAL)
            {
                var reversal = new ReversalRequest();
                reversal.OriginalInternalReference = "000412"; //Optional: InternalReference, else manual entry
                reversal.EnableDialog = '1';
                reversal.EnableExternalNetworking = false;
                reversal.PrintReceipt = false;
                IntegrationLogWriter.LogWrite(TAG + string.Format("SND (ReversalRequest) {0} {1} {2}", amount, vatamount, cbAmount));

                api.SendRequest(reversal);

            }
            else
            {
                SaleRequest sale = new SaleRequest();
                sale.CurrencyCode = "0752";
                sale.BaseAmount = amount + cbAmount - vatamount;
                sale.VATAmount = vatamount;
                //sale.CashbackAmount = cbAmount;
                sale.EnableDialog = '1';
                sale.EnableExternalNetworking = false;
                sale.PrintReceipt = false;
                //sale.EntryOption = 'T';  // Manual Entry of PAN
                IntegrationLogWriter.LogWrite(TAG + string.Format("SND (SaleRequest) {0} {1} {2}", amount, vatamount, cbAmount));

                api.SendRequest(sale);
            }

        }

        /// <summary>
        /// Called from other thread
        /// </summary>
        internal void apiMerchantReceipt()
        {
            Debug.WriteLine(TAG + "SND (merchantReceipt)");
            IntegrationLogWriter.LogWrite(TAG + "SND (merchantReceipt)");

            //api.merchantReceipt();
        }

        internal void apiCustomerReceipt()
        {
            Debug.WriteLine(TAG + "SND (customerReceipt)");
            IntegrationLogWriter.LogWrite(TAG + "SND (customerReceipt)");

            //api.customerReceipt();
        }

        /// <summary>
        /// Called from other thread
        /// </summary>
        internal void apiBatchReport()
        {
            Debug.WriteLine(TAG + "SND (batchReport)");
            //api.batchReport();
        }


        internal void RegisterEventHandler(IPaymentDeviceEventHandler eventHandler)
        {
            if (this.eventHandler != null)
            {
                //throw new Exception("eventHandler already registered");
            }
            this.eventHandler = eventHandler;
            IntegrationLogWriter.LogWrite(TAG + "RegisterEventHandler");
        }

        internal void UnRegisterEventHandler()
        {
            //this.eventHandler = null;
            //babsEventHandler.UnRegisterEventHandler();
        }

        internal void SendReferralCode(string authCode)
        {
            Debug.WriteLine(TAG + "SND (sendApprovalCode) {0}", authCode);
            IntegrationLogWriter.LogWrite(TAG + "SND (sendApprovalCode) " + authCode);

            //api.sendApprovalCode(authCode);
        }

        public volatile int LastStatus;

        internal void TestConnection()
        {
            Debug.WriteLine(TAG + "SND (testHostConnection)");
            IntegrationLogWriter.LogWrite(TAG + "SND (testHostConnection)");
            //LogonRequest logon = new LogonRequest();

            //api.SendRequest(logon);

            // api.testHostConnection();
        }

        internal void TerminalConfig()
        {
            Debug.WriteLine(TAG + "SND (terminalConfig)");
            IntegrationLogWriter.LogWrite(TAG + "SND (terminalConfig)");

            //api.terminalConfig();
        }

        internal void UnsentTransactions()
        {
            Debug.WriteLine(TAG + "SND (unsentTransactions)");
            IntegrationLogWriter.LogWrite(TAG + "SND (unsentTransactions)");

            //api.unsentTransactions();
        }

        internal void TransLogByNr(int type, int inputNumber)
        {
            Debug.WriteLine(TAG + "SND (transLogByNbr) {0} {1}", type, inputNumber);
            IntegrationLogWriter.LogWrite(TAG + string.Format("SND (transLogByNbr) {0} {1}", type, inputNumber));

            //api.transLogByNbr(type, inputNumber);
        }

        internal void CloseBatch()
        {
            Debug.WriteLine(TAG + "SND (start((int)TransactionTypes.LPP_CLOSEBATCH)");

            SettleRequest settle = new SettleRequest();
            settle.EnableExternalNetworking = false;
            //settle.EnableDialog = true;
            settle.PrintReceipt = false;

            api.SendRequest(settle);
            IntegrationLogWriter.LogWrite(TAG + "SND SettleRequest (start((int)TransactionTypes.LPP_CLOSEBATCH)");
            //api.start((int)TransactionTypes.LPP_CLOSEBATCH);
        }
        internal void ResetConnection()
        {

            Debug.WriteLine(TAG + "SND (Need to implement reset)");
            IntegrationLogWriter.LogWrite(TAG + "SND (Need to implement reset)");
            RestartRequest restart = new RestartRequest();
            api.SendRequest(restart);
            this.isConnected = false;

            //Connect();



            //api.open();
            //api.connect();
            // api.close();
            //   UnRegisterEventHandler();
            //  api.end();
            //  api.connect();            
            //   api.start((int)TransactionTypes.OTHER);
        }
        internal void Cancel()
        {
            Debug.WriteLine(TAG + "SND (cancel())");
            IntegrationLogWriter.LogWrite(TAG + "SND (cancel())");
            CancelRequest cancel = new CancelRequest();
            if (api != null)
                api.SendRequest(cancel);

            //api.cancel();
        }

        private void dispatchToUi(Action a)
        {
            if (this.eventHandler != null)
            {
                IntegrationLogWriter.LogWrite(TAG + "dispatchToUi");

                this.uiDispatcher.Invoke(a);
            }
        }
    }
    public enum BamboraTransactionResult
    {
        Approved = 0,
        Declined = 1,
        Cancelled = 2
    }
}
