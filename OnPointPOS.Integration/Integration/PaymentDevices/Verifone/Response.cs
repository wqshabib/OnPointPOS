using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace POSSUM.Integration.PaymentDevices.Verifone
{
    [Serializable]
    public partial class Command
    {
        public commandCancel cancel { get; set; }
    }

    public partial class commandCancel
    {
        [XmlAttribute]
        public string reason { get; set; }

        [XmlAttribute]
        public int resultcode { get; set; }
    }

    public partial class dataResult
    {
        public string bank { get; set; }
        public dataResultEnd end { get; set; }
        public string merchantaddress { get; set; }
        public string merchantcity { get; set; }
        public string merchantname { get; set; }
        public string merchantorgnbr { get; set; }
        public string merchantphone { get; set; }
        public VerifoneReceipt receipt { get; set; }

        [XmlAttribute]
        public long resultcode { get; set; }

        [XmlIgnore]
        public bool resultcodeSpecified { get; set; }

        public long terminal_id { get; set; }
        public string text { get; set; }

        [XmlText]
        public string[] Text { get; set; }

        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        [XmlText]
        public string Value { get; set; }
    }

    [Serializable]
    public partial class dataResult
    {
        public dataResultBatch batch { get; set; }

        public dataResultFrom from { get; set; }

        public dataResultTO to { get; set; }
    }

    [Serializable]
    public partial class dataResultBatch
    {
        public int number { get; set; }

        [XmlElement("record")]
        public dataResultBatchRecord[] record { get; set; }

        public dataResultBatchTotal total { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchRecord
    {
        public dataResultBatchRecordField field { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchRecordField
    {
        public dataResultBatchRecordFieldCashback cashback { get; set; }

        public dataResultBatchRecordFieldCredit credit { get; set; }

        public dataResultBatchRecordFieldDebit debit { get; set; }

        public string name { get; set; }

        public dataResultBatchRecordFieldSum sum { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchRecordFieldCashback
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchRecordFieldCredit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchRecordFieldDebit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchRecordFieldSum
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchTotal
    {
        public dataResultBatchTotalCashback cashback { get; set; }

        public dataResultBatchTotalCredit credit { get; set; }

        public dataResultBatchTotalDebit debit { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchTotalCashback
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchTotalCredit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultBatchTotalDebit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class dataResultEnd
    {
        [XmlAttribute]
        public int resultcode { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    [Serializable]
    public partial class dataResultFrom
    {
        public int date { get; set; }

        public int time { get; set; }
    }

    [Serializable]
    public partial class dataResultTO
    {
        public int date { get; set; }

        public int time { get; set; }
    }

    [Serializable]
    public class information
    {
        [XmlAttribute]
        public string resultcode { get; set; }

        [XmlAttribute]
        public string text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "data")]
    public partial class Response : VerifoneBase
    {
        public override bool ForceMag => false;

        [XmlElement(IsNullable = true)]
        public information information { get; set; }

        [XmlIgnore]
        public override bool IsConnected => information?.resultcode == "904";

        [XmlIgnore]
        public override bool Ok => result?.resultcode == 0;

        [XmlIgnore]
        public override bool Proceed => Ok;

        [XmlIgnore]
        public override VerifoneReceipt Receipt => result?.receipt;

        public dataResult result { get; set; }

        public int resultcode { get; set; }

        public dataResultBatch Transactions => result.batch;
        public override bool Wait => result?.resultcode == 101;

        public List<string> GetBatch()
        {
            if (result?.batch != null)
            {
                return result.ToList();
            }

            return null;
        }
    }

    [Serializable]
    public partial class resultBatch
    {
        public int number { get; set; }

        [XmlElement("record")]
        public resultBatchRecord[] record { get; set; }

        public resultBatchTotal total { get; set; }
    }

    [Serializable]
    public partial class resultBatchRecord
    {
        public resultBatchRecordField field { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultBatchRecordField
    {
        public resultBatchRecordFieldCashback cashback { get; set; }

        public resultBatchRecordFieldCredit credit { get; set; }

        public resultBatchRecordFieldDebit debit { get; set; }

        public string name { get; set; }

        public resultBatchRecordFieldSum sum { get; set; }
    }

    [Serializable]
    public partial class resultBatchRecordFieldCashback
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultBatchRecordFieldCredit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultBatchRecordFieldDebit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultBatchRecordFieldSum
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultBatchTotal
    {
        public resultBatchTotalCashback cashback { get; set; }

        public resultBatchTotalCredit credit { get; set; }

        public resultBatchTotalDebit debit { get; set; }
    }

    [Serializable]
    public partial class resultBatchTotalCashback
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultBatchTotalCredit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultBatchTotalDebit
    {
        public int amount { get; set; }

        public int number { get; set; }
    }

    [Serializable]
    public partial class resultFrom
    {
        public int date { get; set; }

        public int time { get; set; }
    }

    [Serializable]
    public partial class resultTO
    {
        public int date { get; set; }

        public int time { get; set; }
    }

    [Serializable]
    public class action
    {
        public card card { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "data")]
    public class Approval
    {
        public Approval()
        {
            purchase = new dataPurchase();
        }

        public Approval(int authCode)
        {
            purchase = new dataPurchase()
            {
                approvalcode = authCode
            };
        }

        public dataPurchase purchase { get; set; }
    }

    [Serializable]
    public class card
    {
        [XmlAttribute]
        public string forcemag { get; set; }
    }

    [Serializable]
    public partial class dataPurchase
    {
        [XmlAttribute]
        public int approvalcode { get; set; }
    }
}