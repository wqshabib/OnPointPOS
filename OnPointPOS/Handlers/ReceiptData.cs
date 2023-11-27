using System;
using System.Collections.Generic;
using System.Linq;


namespace POSSUM.Handlers
{
    public class ReceiptData : List<string>
    {
        public ReceiptData(IEnumerable<string> collection) : base(collection)
        {
        }

        public ReceiptData()
        {
        }

        private static string FormatValue(string value)
        {
            string rslt = "";
            if (value.Contains(';'))
            {
                var sVal = value.Split(';').ToList();
                rslt = sVal.Aggregate(rslt, (current, s) => current + s + "\t");
            }
            else
            {
                rslt = value;
            }

            return rslt;
        }

        public virtual void AddBatch(string header, string value)
        {
            Add((!string.IsNullOrEmpty(header) ? header.PadRight(20) + "\t" : "") +
                                   FormatValue(value));
        }

        public virtual void Add(string label, int? value)
        {
            if (value.HasValue)
            {
                Add(label, value.Value);
            }
        }

        public virtual void Add(string label, long value)
        {
            Add(label, value.ToString());
        }

        public virtual void Add(char separator, params string[] values)
        {
            var sep = new string(separator, 1);

            var str = string.Join(sep, values);

            str = str.Trim();

            Add(str);
        }

        public virtual int Add(string value1, string value2, int blanks)
        {
            string str = new string(' ', blanks);

            Add(value1, value2, str);

            return Count - 1;
        }

        public int Add(string value1, string value2, string separator)
        {
            string format = string.Format("{0}{1}{2}", value1, separator, value2);

            Add(format);

            return Count - 1;
        }

        public void Add(string label, string value1, string value2, string separator)
        {
            string format = string.Format("{0}: {1} {2} {3}", label, value1, separator, value2);

            Add(format);
        }

        public void Add(string label, int value)
        {
            string format = string.Format("{0}: {1}", label, value);
            Add(format);
        }

        public int Add(string label, string value)
        {
            string format = string.Format("{0}: {1}", label, value);

            Add(format);

            return Count - 1;
        }

        public virtual void AddAccount(char? account)
        {
            if (account.HasValue)
            {
                char value = account.Value;

                if (value.Equals('C'))
                {
                    Add("Belastat kredit");
                }
                else if (value.Equals('D'))
                {
                    Add("Belastat konto");
                }
            }
        }

        public void AddAccount(string cardmethod)
        {
            if (cardmethod == "C")
            {
                Add("Belastat kredit");
            }
            if (cardmethod == "D")
            {
                Add("Belastat bankkonto");
            }
        }

        public void AddAmount(string label, double amount, string currency)
        {
            string value = string.Format("{0} ({1}):", label, currency);

            string str = value.PadRight(27);

            str = string.Format("{0}{1,8}", value.PadRight(22), amount.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol) + " kr");

            Add(str);
        }

        public void AddAmount(string label, double amount)
        {
            AddAmount(label, amount, "SEK");
        }

        public void AddDottedLine()
        {
            Add("------------------------------------");
        }

        public void AddEmpty()
        {
            Add("");
        }

        public void AddEmpty(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddEmpty();
            }
        }

        public virtual void AddEmv(List<KeyValuePair<string, string>> parameters)
        {
            foreach (var element in parameters)
            {
                switch (element.Key)
                {
                    case "ATC":
                    case "AED":
                    case "AID":
                    case "TVR":
                    case "TSI":
                        Add(element.Key, element.Value);
                        break;
                }
            }
        }

        public virtual void AddHeader(string header)
        {
            string format = string.Format("************ {0} ************", header);

            Add(format);
        }

        public virtual void AddTimeStamp(string timestamp)
        {
            Add(timestamp.Substring(0, 6) + " " +
                             timestamp.Substring(6, 2) + ":" +
                             timestamp.Substring(8, 2));
        }

        public int AddToPrevious(int index)
        {
            return Concat(index - 1, index);
        }

        public int Concat(int index1, int index2, int fixedSpace = -1)
        {
            var str1 = this[index1];

            var str2 = this[index2];

            int length = 0;

            string separator = string.Empty;

            if (fixedSpace > 0)
            {
                separator = new string(' ', fixedSpace);
            }
            else
            {
                length = str1.Length + str2.Length;

                if (length < 42)
                {
                    separator = new string(' ', 42 - length);
                }
            }

            this[index1] = string.Join(separator, str1, str2);

            RemoveAt(index2);

            return Count - 1;
        }
    }
}