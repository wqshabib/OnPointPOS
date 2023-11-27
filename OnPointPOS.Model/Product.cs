using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Product : BaseEntity
    {
        public Product()
        {
            Created = DateTime.Now;
            AccountingId = 1;
            ShowItemButton = true;
        }

        [Key]
        public Guid Id { get; set; }
        [MaxLength(150)]
        public string Description { get; set; }
        [MaxLength(50)]
        public string SKU { get; set; }
        [MaxLength(50)]
        public string BarCode { get; set; }
        [MaxLength(50)]
        public string PLU { get; set; }
        public decimal Price { get; set; }
        public decimal? TempPrice { get; set; }
        public decimal? MinStockLevel { get; set; }
        public DateTime? TempPriceStart { get; set; }
        public DateTime? TempPriceEnd { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Tax { get; set; }
        public ProductUnit Unit { get; set; }
        public bool AskPrice { get; set; }
        public bool AskWeight { get; set; }
        [NotMapped]
        public bool BackupAskWeight { get; set; }
        public bool PriceLock { get; set; }
        [MaxLength(10)]
        public string ColorCode { get; set; }
        public int PrinterId { get; set; }
        public int SortOrder { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public bool PlaceHolder { get; set; }
        public bool AskVolume { get; set; }
        public bool NeedIngredient { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool Bong { get; set; }
        public int Sticky { get; set; }
        public bool Seamless { get; set; }
        public bool ShowItemButton { get; set; }
        public int AccountingId { get; set; }
        public decimal ReorderLevelQuantity { get; set; }
        public decimal StockQuantity { get; set; }

        public virtual ReceiptMethod ReceiptMethod { get; set; }
        public virtual ItemType ItemType { get; set; }
        public virtual bool DiscountAllowed { get; set; }
        public virtual PrepareTime PreparationTime { get; set; }

        //[NotMapped]
        public string ExternalID { get; set; }
        public string ImageURL { get; set; }
        public string ProductDescription { get; set; }

        //new fields for Pant products
        //[NotMapped]
        public bool IsPantEnabled { get; set; }
        //[NotMapped]
        public string PantProductId { get; set; }

        

        public Nullable<decimal> Weight { get; set; }




        #region Not Mapped Properties
        public Product(Product itm)
        {
            Id = itm.Id;
            Description = itm.Description;
            SortOrder = itm.SortOrder;
            Price = itm.Price;
            Unit = itm.Unit;
            AskPrice = itm.AskPrice;
            AskWeight = itm.AskWeight;
            AskVolume = itm.AskVolume;
            Active = itm.Active;
            PurchasePrice = itm.PurchasePrice;
            ColorCode = string.IsNullOrEmpty(itm.ColorCode) ? "#FFDCDEDE" : itm.ColorCode;
            BarCode = itm.BarCode;
            PlaceHolder = itm.PlaceHolder;
            PLU = itm.PLU;
            PriceLock = itm.PriceLock;
            PrinterId = itm.PrinterId;
            Tax = itm.Tax;
            Seamless = itm.Seamless;
            Sticky = itm.Sticky;
            Bong = itm.Bong;
            ShowItemButton = itm.ShowItemButton;
            ReceiptMethod = itm.ReceiptMethod;
            ItemType = itm.ItemType;
            SKU = itm.SKU;
            NeedIngredient = itm.NeedIngredient;
            PreparationTime = itm.PreparationTime;
            DiscountAllowed = itm.DiscountAllowed;
            //HasPricePolicy = itm.HasPricePolicy;
            AccountingId = itm.AccountingId;
            IsPantEnabled = itm.IsPantEnabled;
            PantProductId = itm.PantProductId;
            TempPrice = itm.TempPrice;
            TempPriceStart = itm.TempPriceStart;
            TempPriceEnd = itm.TempPriceEnd;
            ProductIngredients = itm.ProductIngredients;
        }
        [NotMapped]
        public int CategoryId { get; set; }
        [NotMapped]
        public Campaign Campaign { get; set; }
        [NotMapped]
        public bool HasPricePolicy { get; set; }
        [NotMapped]
        public virtual List<ItemCategory> ItemCategory { get; set; }
        [NotMapped]
        public int ItemIdex { get; set; }
        [NotMapped]
        public bool HasCampaign { get { return Campaign != null; } }
        [NotMapped]
        public List<Product> Products { get; set; }
        [NotMapped]
        public string ColorName { get; set; }
        [NotMapped]
        public int Type { get; set; }
        [NotMapped]
        public string ImagePath { get; set; }
        [NotMapped]
        public int IconId { get; set; }
        [NotMapped]
        public bool ImageVisibility { get; set; }
        [NotMapped]
        public string ifExists { get; set; }
        [NotMapped]
        public decimal DiscountPrice { get; set; }
        [NotMapped]
        public List<ProductPrice> ProductPrices { get; set; }
        [NotMapped]
        public int ItemCategory_SortOrder { get; set; }
        [NotMapped]
        public bool ItemCategory_IsPrimary { get; set; }
        [NotMapped]
        public string ProductId { get; set; }

        [NotMapped]
        public List<ProductIngredients> ProductIngredients { get; set; }

        #endregion
    }
    public class Product_Text : BaseEntity
    {
        public int Id { get; set; }

        public Guid ItemId { get; set; }

        public int LanguageId { get; set; }
        public string TextDescription { get; set; }

        public DateTime Updated { get; set; }

    }
    public class Product_PricePolicy : BaseEntity
    {
        public virtual int Id { get; set; }
        public Guid ItemId { get; set; }

        public virtual int BuyLimit { get; set; }
        public virtual decimal DiscountAmount { get; set; }
        public virtual bool Active { get; set; }
        public virtual bool Deleted { get; set; }
        public virtual DateTime Updated { get; set; }
        [NotMapped]
        public virtual int PolicyId { get; set; }
    }
    public enum ProductUnit
    {
        Piece = 0,
        g = 1,
        hg = 2,
        kg = 3,
        cl = 4
    }
    public enum PrepareTime
    {
        Standard = 0,
        Quick = 1
    }
    public class ProductGroup : BaseEntity
    {
        public int Id { get; set; }
        public Guid ItemId { get; set; }
        public Guid GroupId { get; set; }
        public decimal Price { get; set; }
        public DateTime Updated { get; set; }

        [NotMapped]
        public string Description { get; set; }
    }

    public enum ItemType
    {
        Individual = 0,
        Grouped = 1,
        Ingredient = 2,
        Pant = 3,
    }

    public enum ReceiptMethod
    {
        Show_Product_As_Individuals = 0,
        Show_Product_As_Group = 1
    }

    /*munir*/
    public class ProductOpening
    {
        public int Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal OpeningQuantity { get; set; }
        public string BatchNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class ProductIngredients
    {
        [Key]
        public int ProductIngredientId { get; set; }
        public Guid ProductId { get; set; }
        public Guid IngredientId { get; set; }
        [ForeignKey("IngredientId")]
        public Product Product { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ProductStockHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public decimal ProductStock { get; set; }
        public decimal LastStock { get; set; }
        public decimal NewStock { get; set; }
        public decimal StockValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? StockHistoryGroupId { get; set; }
    }

    public class StockHistoryGroup
    {
        [Key]
        public Guid StockHistoryGroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? InventoryHistoryId { get; set; }
    }

    public class InventoryHistory
    {
        [Key]
        public Guid InventoryHistoryId { get; set; }
        public string InventoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
    }
}
