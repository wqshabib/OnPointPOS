using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace POSSUM.Api.Models
{
    public class InventoryData
    {
        public string InventoryName { get; set; }
        public List<Inventory> Inventory { get; set; }
    }

    public class Inventory
    {
        public string Section { get; set; }
        public List<InventroyHistoryApi> ProductStockHistory { get; set; }
    }

    public class InventroyHistoryApi
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal ProductStock { get; set; }
        public decimal LastStock { get; set; }
        public decimal NewStock { get; set; }
        public decimal StockValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? StockHistoryGroupId { get; set; }
    }

    public class TotalInventory
    {
        public Guid ProductId { get; set; }
        public decimal TotalStock { get; set; }
    }
}