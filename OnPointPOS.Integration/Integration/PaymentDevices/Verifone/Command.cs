using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace POSSUM.Integration.PaymentDevices.Verifone
{
    [XmlRoot("command")]
    public partial class Command : IXmlSerializable
    {
        private readonly string action;
        private readonly string attribute;
        private readonly decimal? decimalValue;
        private readonly string root;
        private readonly string stringValue;
        private readonly Transaction transaction;

        public Command()
        {
            root = "command";
        }

        public Command(string action)
        {
            this.action = action;
            root = "command";
        }

        public Command(Transaction transaction)
        {
            this.transaction = transaction;
        }

        public Command(string action, string attribute, string value, string root = "command")
        {
            this.action = action;
            this.attribute = attribute;
            stringValue = value;
            this.root = root;
        }

        public Command(string action, string attribute, decimal value, string root = "command")
        {
            this.action = action;
            this.attribute = attribute;
            decimalValue = value * 100;
            this.root = root;
        }

        [XmlIgnore]
        public string Action { get; set; }

        public commandTransactionlog transactionlog { get; set; }

        [XmlIgnore]
        public string Value { get; set; }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            Action = reader.ReadString();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (transaction != null)
            {
                writer.WriteStartElement(transaction.type, "");

                if (transaction.amount.HasValue)
                {
                    writer.WriteAttributeString("amount", transaction.amount.Value.ToString());
                }

                if (transaction.vat.HasValue)
                {
                    writer.WriteAttributeString("vat", transaction.vat.Value.ToString());
                }

                if (transaction.cashback.HasValue && transaction.cashback.Value > 0 && transaction.type == "purchase")
                {
                    writer.WriteAttributeString("cashback", transaction.cashback.Value.ToString());
                }

                writer.WriteEndElement();
            }
            else
            {
                if (!string.IsNullOrEmpty(attribute))
                {
                    writer.WriteStartElement(action, "");

                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        writer.WriteAttributeString(attribute, stringValue);
                    }
                    else if (decimalValue.HasValue)
                    {
                        writer.WriteAttributeString(attribute, decimalValue.Value.ToString());
                    }

                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteElementString(action, string.Empty);
                }
            }
        }

        [Serializable]
        public class commandTransactionlog
        {
            [XmlAttribute]
            public bool detailed { get; set; }

            [XmlAttribute]
            public string span { get; set; }

            [XmlAttribute]
            public string type { get; set; }
        }
    }

    [XmlRoot(ElementName = "request")]
    [Serializable]
    public class Request : VerifoneBase
    {
        public requestAmount amount { get; set; }

        public requestCard card { get; set; }

        [XmlIgnore]
        public override bool ForceMag => card?.resultcode == 124;

        public override bool IsConnected => false;

        public override bool IsReferral
        {
            get { return referral==null?false: (referral.resultcode == 107) || referral.resultcode == 207; }
        }

        public override bool Ok => amount?.resultcode == 102;

        [XmlIgnore]
        public override bool Proceed => amount?.resultcode == 102;

        public override VerifoneReceipt Receipt => null;
        public requestReferral referral { get; set; }

        [XmlAttribute]
        public string type { get; set; }

        public override bool Wait => amount?.resultcode == 101;
    }

    [Serializable]
    public class requestAmount
    {
        [XmlAttribute]
        public int resultcode { get; set; }
    }

    [Serializable]
    public class requestCard
    {
        [XmlAttribute]
        public int resultcode { get; set; }
    }

    [Serializable]
    public partial class requestReferral
    {
        [XmlAttribute]
        public int resultcode { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "transaction")]
    public class Transaction
    {
        public decimal? amount { get; set; }

        public decimal? cashback { get; set; }

        [XmlIgnore]
        public string type { get; set; }

        public decimal? vat { get; set; }
    }

    public abstract class VerifoneBase
    {
        public abstract bool ForceMag { get; }
        public abstract bool IsConnected { get; }

        public virtual bool IsReferral
        {
            get { return false; }
        }

        public abstract bool Ok { get; }
        public abstract bool Proceed { get; }
        public abstract VerifoneReceipt Receipt { get; }
        public abstract bool Wait { get; }
    }
}