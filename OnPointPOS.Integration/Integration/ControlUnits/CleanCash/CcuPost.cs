using System;
using Newtonsoft.Json;

namespace POSSUM.Integration.ControlUnits.CleanCash
{
    public class CcuPost
    {
        [JsonProperty(PropertyName = "brutto")]
        public long brutto { get; set; }
        [JsonProperty(PropertyName = "vatRateToSum")]
        public VatRateToSum vatRateToSum { get; set; }
        [JsonProperty(PropertyName = "refund")]
        public bool refund { get; set; }
        [JsonProperty(PropertyName = "printType")]
        public String printType { get; set; }
        [JsonProperty(PropertyName = "receiptNumber")]
        public int receiptNumber { get; set; }
        [JsonProperty(PropertyName = "date")]
        public long date { get; set; }
    }

    public class VatRateToSum
    {
        [JsonProperty(PropertyName = "0")]
        public long v0 { get; set; }
        [JsonProperty(PropertyName = "6")]
        public long v6 { get; set; }
        [JsonProperty(PropertyName = "12")]
        public long v12 { get; set; }
        [JsonProperty(PropertyName = "25")]
        public long v25 { get; set; }
    }
}
