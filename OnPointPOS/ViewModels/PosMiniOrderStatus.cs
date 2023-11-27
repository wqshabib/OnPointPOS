using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.ViewModels
{
    public class PosMiniOrderStatus
    {
        public bool OrderStatus { get; set; }
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderNumber { get; set; }
        public string ReceiptNumber { get; set; }
    }

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
        public string sku { get; set; }
        public string method { get; set; }
        public dynamic Data { get; set; }
    }


    public class ImagePathClass
    {
        public string Id { get; set; }
        public string ImgPath { get; set; }
    }





}
