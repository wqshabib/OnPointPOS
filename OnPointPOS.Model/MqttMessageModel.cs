using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    [Serializable]
    public class MqttMessageModel
    {
        public string messageId { get; set; }
        public string senderId { get; set; }
        public string name { get; set; }
        public string errorMessage { get; set; }
        public string action { get; set; }
        public string itemId { get; set; }
        public string orderId { get; set; }
        public string terminalId { get; set; }
        public string warehouseId { get; set; }
        public string warehouseLocationId { get; set; }
        public decimal qty { get; set; }
        public int reservation { get; set; }
        public int stockCount { get; set; }
        public string timestamp { get; set; }
        public dynamic Data { get; set; }
    }
}
