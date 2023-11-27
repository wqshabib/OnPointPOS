using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class OrderLineApi
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountedUnitPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal ItemDiscount { get; set; }
        public decimal TaxPercent { get; set; }
        public int Active { get; set; }
        public string ItemComments { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int UnitsInPackage { get; set; }
        public int ItemStatus { get; set; }
        public int IsCoupon { get; set; }
        public int Direction { get; set; }
        public DiscountType DiscountType { get; set; }
        public string DiscountDescription { get; set; }
        public ItemType ItemType { get; set; }
        public Guid GroupId { get; set; }
        public Guid? GroupKey { get; set; }
        public string IngredientMode { get; set; }
        public bool IsInventoryAdjusted { get; set; }
        public int WC_ID { get; set; }
        public string ItemDescription { get; set; }

        public static OrderLineApi ConvertModelToApiModel(OrderLine orderLine)
        {
            return new OrderLineApi
            {
                Id = orderLine.Id,
                OrderId = orderLine.OrderId,
                ItemId = orderLine.ItemId,
                Quantity = orderLine.Quantity,
                DiscountedUnitPrice = orderLine.DiscountedUnitPrice,
                PurchasePrice = orderLine.PurchasePrice,
                ItemDiscount = orderLine.ItemDiscount,
                TaxPercent = orderLine.TaxPercent,
                Active = orderLine.Active,
                ItemComments = orderLine.ItemComments,
                UnitPrice = orderLine.UnitPrice,
                DiscountPercentage = orderLine.DiscountPercentage,
                UnitsInPackage = orderLine.UnitsInPackage,
                ItemStatus = orderLine.ItemStatus,
                IsCoupon = orderLine.IsCoupon,
                Direction = orderLine.Direction,
                DiscountType = orderLine.DiscountType,
                DiscountDescription = orderLine.DiscountDescription,
                ItemType = orderLine.ItemType,
                GroupId = orderLine.GroupId,
                GroupKey = orderLine.GroupKey,
                IngredientMode = orderLine.IngredientMode,
                IsInventoryAdjusted = orderLine.IsInventoryAdjusted,
                WC_ID = orderLine.WC_ID,
                ItemDescription = orderLine.ItemDescription
            };
        }

        public static OrderLine ConvertApiModelToModel(OrderLineApi orderLine)
        {
            return new OrderLine
            {
                Id = orderLine.Id,
                OrderId = orderLine.OrderId,
                ItemId = orderLine.ItemId,
                Quantity = orderLine.Quantity,
                DiscountedUnitPrice = orderLine.DiscountedUnitPrice,
                PurchasePrice = orderLine.PurchasePrice,
                ItemDiscount = orderLine.ItemDiscount,
                TaxPercent = orderLine.TaxPercent,
                Active = orderLine.Active,
                ItemComments = orderLine.ItemComments,
                UnitPrice = orderLine.UnitPrice,
                DiscountPercentage = orderLine.DiscountPercentage,
                UnitsInPackage = orderLine.UnitsInPackage,
                ItemStatus = orderLine.ItemStatus,
                IsCoupon = orderLine.IsCoupon,
                Direction = orderLine.Direction,
                DiscountType = orderLine.DiscountType,
                DiscountDescription = orderLine.DiscountDescription,
                ItemType = orderLine.ItemType,
                GroupId = orderLine.GroupId,
                GroupKey = orderLine.GroupKey,
                IngredientMode = orderLine.IngredientMode,
                IsInventoryAdjusted = orderLine.IsInventoryAdjusted,
                WC_ID = orderLine.WC_ID,
                ItemDescription = orderLine.ItemDescription
            };
        }

        public static OrderLine UpdateModel(OrderLine dbObject, OrderLineApi orderLine)
        {
            dbObject.OrderId = orderLine.OrderId;
            dbObject.ItemId = orderLine.ItemId;
            dbObject.Quantity = orderLine.Quantity;
            dbObject.DiscountedUnitPrice = orderLine.DiscountedUnitPrice;
            dbObject.PurchasePrice = orderLine.PurchasePrice;
            dbObject.ItemDiscount = orderLine.ItemDiscount;
            dbObject.TaxPercent = orderLine.TaxPercent;
            dbObject.Active = orderLine.Active;
            dbObject.ItemComments = orderLine.ItemComments;
            dbObject.UnitPrice = orderLine.UnitPrice;
            dbObject.DiscountPercentage = orderLine.DiscountPercentage;
            dbObject.UnitsInPackage = orderLine.UnitsInPackage;
            dbObject.ItemStatus = orderLine.ItemStatus;
            dbObject.IsCoupon = orderLine.IsCoupon;
            dbObject.Direction = orderLine.Direction;
            dbObject.DiscountType = orderLine.DiscountType;
            dbObject.DiscountDescription = orderLine.DiscountDescription;
            dbObject.ItemType = orderLine.ItemType;
            dbObject.GroupId = orderLine.GroupId;
            dbObject.GroupKey = orderLine.GroupKey;
            dbObject.IngredientMode = orderLine.IngredientMode;
            dbObject.IsInventoryAdjusted = orderLine.IsInventoryAdjusted;
            dbObject.WC_ID = orderLine.WC_ID;
            dbObject.ItemDescription = orderLine.ItemDescription;

            return dbObject;
        }
    }
}
