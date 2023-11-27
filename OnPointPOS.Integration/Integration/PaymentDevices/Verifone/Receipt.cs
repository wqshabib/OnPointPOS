using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;


namespace POSSUM.Integration.PaymentDevices.Verifone
{
    //public enum AuthMethod
    //{
    //    PIN,
    //    SIGNATURE,
    //    UNKNOWN
    //}

    //public partial class dataResult
    //{
    //    public ReceiptData ToList()
    //    {
    //        var list = new ReceiptData();

    //        list.AddBatch(merchantaddress, merchantaddress);

    //        list.AddBatch(merchantcity, merchantcity);

    //        return list;
    //    }
    //}

    public class VerifoneReceipt1
    {
        public int accounttype { get; set; }

        public long acquierrefnumbr { get; set; }

        public string aid { get; set; }

        public int amount { get; set; }

        [XmlIgnore]
        public double Amount => amount / 100.0;

        public int amount_other { get; set; }

        public string authapprovalcode { get; set; }

        public AuthMethod Authentication
        {
            get
            {
                string temp = idmethod.Trim();

                if (temp == "a")
                {
                    return AuthMethod.PIN;
                }
                else if (temp == "@")
                {
                    return AuthMethod.SIGNATURE;
                }

                return AuthMethod.UNKNOWN;
            }
        }

        public long authentity { get; set; }

        public long authmethod { get; set; }

        public string bank { get; set; }

        public long batchnr { get; set; }

        public string cardmethod { get; set; }

        public string cardname { get; set; }

        public int cashback { get; set; }

        public long date { get; set; }

        public string expdate { get; set; }

        public string idmethod { get; set; }

        public string issuer { get; set; }

        public string merchantaddress { get; set; }

        public string merchantcity { get; set; }

        public string merchantname { get; set; }

        public string merchantorgnbr { get; set; }

        public string merchantphone { get; set; }

        public bool NeedToSign => idmethod.Trim() == "@";

        public string offlinedata { get; set; }

        public string pan { get; set; }

        public int paymentcode { get; set; }

        public long refnr { get; set; }

        public string responsecode { get; set; }

        public int resultcode { get; set; }

        public long terminalid { get; set; }

        public long time { get; set; }

        public string Time
        {
            get
            {
                var str1 = date.ToString();
                var str2 = time.ToString();

                var provider = CultureInfo.CurrentCulture;

                var datePart = DateTime.ParseExact(str1, "yymmdd", provider);

                var timePart = DateTime.ParseExact(str2, "HHmmss", provider);

                return string.Format("{0}:{1}", datePart.ToShortDateString(), timePart.ToShortTimeString());
            }
        }

        public PaymentTransactionType transactionType { get; set; }

        public string tsi { get; set; }

        public int tvr { get; set; }

        public int vat { get; set; }

        [XmlIgnore]
        public double Vat
        {
            get { return vat / 100.0; }
        }

        public List<string> GetCustomerReceipt()
        {
            var list = new ReceiptData();

            list.Add(merchantname.Trim());
            list.Add(merchantaddress.Trim());
            list.Add(merchantcity.Trim());
            list.Add("TEL: " + merchantphone.Trim());
            list.Add("Org.nr: " + merchantorgnbr.Trim());
            list.Add(bank.Trim());
            list.Add("Termid: " + terminalid.ToString());

            list.Add("Butiksnr", acquierrefnumbr.ToString());

            list.Add(Time);

            if (transactionType == PaymentTransactionType.PURCHASE)
            {
                list.Add("KÖP");
            }
            else if (transactionType == PaymentTransactionType.REFUND)
            {
                list.Add("Retur");
            }

            list.AddAmount("Belopp", Amount);
            list.AddAmount("Varav moms", Vat);

            list.AddAmount("TOTALT", Amount);

            list.AddEmpty();

            list.Add(pan.Trim());

            list.AddEmpty();

            list.Add(issuer.Trim());
            //i have removed extra {5}:Arshad
            string chstring = string.Format("{0}{1}{2} {3} {4} ", cardmethod.Trim(), idmethod.Trim(), paymentcode, responsecode.Trim(), issuer.Trim());

            list.Add(' ', chstring, batchnr.ToString(), authapprovalcode.Trim());

            list.Add("Ref.nr", refnr.ToString());

            return list;
        }

        public List<string> GetMerchantReceipt()
        {
            var list = new ReceiptData();

            list.Add(merchantname.Trim());
            list.Add(merchantaddress.Trim());
            list.Add(merchantcity.Trim());
            list.Add("TEL: " + merchantphone.Trim());
            list.Add("Org.nr: " + merchantorgnbr.Trim());
            list.Add(bank.Trim());
            list.Add("Termid: " + terminalid.ToString());

            list.Add("Butiksnr", acquierrefnumbr.ToString());

            list.Add(Time);

            list.Add("KÖP");

            list.AddAmount("Belopp", Amount);
            list.AddAmount("Varav moms", Vat);

            list.AddAmount("TOTALT", Amount);

            if (Authentication == AuthMethod.PIN)
            {
                list.Add("Personlig kod");
                list.AddEmpty();
                list.Add(pan.Trim());
            }
            else if (Authentication == AuthMethod.SIGNATURE)
            {
                list.Add(pan.Trim());
            }

            list.AddEmpty();

            list.Add(cardname.Trim());

            list.AddAccount(cardmethod.Trim());

            string chstring = string.Format("{0}{1}{2} {3} {4} ", cardmethod.Trim(), idmethod.Trim(), paymentcode, responsecode.Trim(), issuer.Trim());

            list.Add(' ', chstring, batchnr.ToString(), authapprovalcode.Trim());

            list.Add("Ref.nr", refnr.ToString());

            list.Add("AID", aid);

            list.Add("TVR", tvr);

            if (tsi != null)
            {
                list.Add("TSI", tsi.Trim());
            }

            return list;
        }
    }
}