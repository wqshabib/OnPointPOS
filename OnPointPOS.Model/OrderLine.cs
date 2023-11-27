using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace POSSUM.Model
{
    public enum EntryModeType
    {
        CodeEntry = 0,
        PluEntry = 1,
        ItemEntry = 2
    }

    public enum DiscountType
    {
        NON = 0,
        General = 1,
        Offer = 2
    }

    [Table("OrderDetail")]
    public partial class OrderLine : BaseEntity
    {
        public OrderLine()
        {

        }
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public Guid ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Product Product { get; set; }
        //public Guid? PantItemId { get; set; }

        [Column("Qty")]
        public decimal Quantity { get; set; }

        public decimal DiscountedUnitPrice { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal ItemDiscount { get; set; }

        public decimal TaxPercent { get; set; }

        public int Active { get; set; }
        //[MaxLength(150)]
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



        [NotMapped]
        public PrepareTime PreparationTime { get; set; }

        [NotMapped]
        public string ProductName
        {
            get
            {
                if (Product != null)
                    return Product.Description;
                else
                    return "";
            }
        }

        [NotMapped]
        public List<OrderLine> Ingredients { get; set; }


        //[NotMapped]
        //public string ExternalId { get; set; } 


        public int WC_ID { get; set; }
        public string ItemDescription { get; set; }



    }
    public partial class OrderLine
    {
        public OrderLine(OrderLine line)
        {
            Id = line.Id;
            OrderId = line.OrderId;
            Quantity = line.Quantity;
            UnitPrice = line.UnitPrice;
            UnitsInPackage = line.UnitsInPackage;
            Direction = line.Direction;
            Active = line.Active;
            DiscountedUnitPrice = line.DiscountedUnitPrice;
            DiscountPercentage = line.DiscountPercentage;
            PurchasePrice = line.PurchasePrice;
            IsCoupon = line.IsCoupon;
            ItemDiscount = line.ItemDiscount;
            ItemComments = line.ItemComments;
            ItemStatus = line.ItemStatus;
            TaxPercent = line.TaxPercent;
            ItemId = line.ItemId;
            ItemType = line.ItemType;
            GroupId = line.GroupId;
            GroupKey = line.GroupKey;
            IngredientMode = line.IngredientMode;
            DiscountDescription = line.DiscountDescription;
            DiscountType = line.DiscountType;

            BIDS = line.BIDS;
            BIDSNO = line.BIDSNO;
            CampaignApplied = line.CampaignApplied;
            CampaignId = line.CampaignId;
            IngredientItems = line.IngredientItems;
            ItemDetails = line.ItemDetails;
            PrinterId = line.PrinterId;
            Product = line.Product;
            RewardApplied = line.RewardApplied;
            Description = line.Product != null ? line.Product.Description : line.ItemName;
            ExternalId = line.Product != null ? line.Product.ExternalID : "";
            WC_ID = line.WC_ID;
            ItemDescription = line.ItemDescription;
        }

        public OrderLine GetFrom()
        {
            return new OrderLine
            {
                Id = this.Id,
                OrderId = this.OrderId,
                Quantity = this.Quantity,
                UnitPrice = this.UnitPrice,
                UnitsInPackage = this.UnitsInPackage,
                Direction = this.Direction,
                Active = this.Active,
                DiscountedUnitPrice = this.DiscountedUnitPrice,
                DiscountPercentage = this.DiscountPercentage,
                PurchasePrice = this.PurchasePrice,
                IsCoupon = this.IsCoupon,
                ItemDiscount = (this.ItemDiscount > 0 && this.Direction == -1) ? (this.ItemDiscount * -1) : this.ItemDiscount,
                ItemComments = this.ItemComments,
                ItemStatus = this.ItemStatus,
                TaxPercent = this.TaxPercent,
                ItemId = this.Product.Id,
                ItemType = this.ItemType,
                GroupId = this.GroupId,
                GroupKey = this.GroupKey,
                IngredientMode = this.IngredientMode,
                DiscountDescription = this.DiscountDescription,
                DiscountType = this.DiscountType




            };
        }
        #region Not Mapped

        [NotMapped]
        public bool IsValid => (this.Active == 1 && this.IsCoupon != 1);

        [NotMapped]
        public List<OrderLine> ItemDetails { get; set; }

        [NotMapped]
        public List<OrderLine> IngredientItems { get; set; }

        [NotMapped]
        public int CampaignId { get; set; }

        [NotMapped]
        public bool HasGroupItems => Product != null ? Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group && ItemDetails != null : false;
        [NotMapped]
        public decimal GrossTotal { get { return GrossAmount(); } }
        [NotMapped]
        public decimal DisplayGrossTotal { get { return GrossAmount() < 0 ? (-1) * GrossAmount() : GrossAmount(); } }
        [NotMapped]
        public decimal GrossDiscountTotal { get { return GrossAmountDiscounted() + IngredientTotal; } }
        [NotMapped]
        public decimal IngredientTotal { get { return IngredientAmount(); } }
        [NotMapped]
        public bool ShowButton { get { return ItemStatus == 0 ? true : false; } }
        [NotMapped]
        public string ItemName { get { return Product != null ? Product.Description : ""; } }

        [NotMapped]
        public int UpdateColorName { get { return (int)ItemStatus; } }
        [NotMapped]
        public decimal VAT { get { return VatAmount(); } }

        [NotMapped]
        public decimal ItemTotalDiscount { get { return ItemDiscount + IngredientDiscount(); } }

        [NotMapped]
        public decimal ItemTotalDiscountDisplay { get { return (-1) * (ItemDiscount + IngredientDiscount()); } }

        [NotMapped]
        public bool DiscountVisibility { get { return ItemDiscount != 0 ? true : false; } }
        [NotMapped]
        public decimal ItemDiscountDisplay { get { return (-1) * ItemDiscount; } }
        [NotMapped]
        public bool DiscountSignVisibility { get { return ItemDiscount > 0 ? true : false; } }

        [NotMapped]
        public bool IngredientListVisibility { get { return (IngredientItems != null && IngredientItems.Count > 0) ? true : false; } }
        [NotMapped]
        public bool RecieptQtyVisibility => Product != null ? Product.Unit == ProductUnit.Piece ? true : false : false;
        [NotMapped]
        public bool QtyVisibility => Product != null ? Product.Unit == ProductUnit.Piece ? true : false : false;
        [NotMapped]
        public bool GroupQtyVisibility => Quantity > 1 ? true : false;
        [NotMapped]
        public virtual bool IngredientVisibility { get { return Product != null ? Product.NeedIngredient ? true : false : false; } }
        [NotMapped]
        public string imagePath => IngredientMode == "+" ? "/POSSUM;component/images/pluse.png" : "/POSSUM;component/images/remove.png";

        [NotMapped]
        public bool IsSelected { get; set; }
        [NotMapped]
        public string SelecttedImagePath => IsSelected ? "/POSSUM;component/images/selecteditem.png" : "/POSSUM;component/images/unselecteditem.png";

        [NotMapped]
        public bool CampaignApplied { get; set; }
        [NotMapped]
        public bool RewardApplied { get; set; }

        [NotMapped]
        public string CartQty => GetQty();
        [NotMapped]
        public string RecieptQty => GetReciptQty();

        [NotMapped]
        public bool HasIngredients => IngredientItems != null;
        [NotMapped]
        public List<ReceiptItems> ReceiptItems { get; set; }
        [NotMapped]
        public int ItemIdex { get; set; }
        [NotMapped]
        public int PrinterId { get; set; }
        [NotMapped]
        public int BIDSNO { get; set; }
        [NotMapped]
        public decimal Percentage { get; set; }
        [NotMapped]
        public OrderType OrderType { get; set; }
        [NotMapped]
        public string BIDS { get; set; }
        [NotMapped]
        public string Category { get; set; }
        [NotMapped]
        public string Description { get; set; }
        [NotMapped]
        public string ExternalId { get; set; }

        #endregion


    }
    public partial class OrderLine
    {
        #region Extension Method

        private string GetQty()
        {
            if (Product == null)
                return "0";
            if (Product.Unit == ProductUnit.g)
            {
                return Quantity.ToString("N") + "g";
            }
            if (Product.Unit == ProductUnit.kg)
            {
                int intPart = (int)Quantity;
                decimal fracPart = Quantity - intPart;

                if (fracPart.ToString().Length == 3)
                    return Quantity.ToString("N") + "00kg";
                if (fracPart.ToString().Length == 4)
                    return Quantity.ToString("N") + "0kg";
                if (fracPart.ToString().Length == 5)
                    return Quantity.ToString("N3") + "kg";
                return Quantity.ToString("N") + "kg";
                //Kilograms
                // return this.Quantity.ToString().Length < 5 ? this.Quantity.ToString() + "00kg" : this.Quantity + "kg";
            }
            if (Product.Unit == ProductUnit.hg)
            {
                int intPart = (int)Quantity;
                decimal fracPart = Quantity - intPart;

                if (fracPart.ToString().Length == 3)
                    return Quantity.ToString("N") + "0kg";
                return Quantity.ToString("N") + "kg";
            }
            if (Product.Unit == ProductUnit.cl)
            {
                return Quantity.ToString("N") + "cl";
            }
            if (Quantity >= 1)
                return Quantity.ToString("N");
            return Quantity.ToString("N");
        }

        private string GetReciptQty()
        {
            if (Product == null)
                return "0";
            string qty = "";
            if (Product.Unit == ProductUnit.g)
            {
                qty = Math.Round(Quantity, 0) + "g*" + UnitPrice + "Kr/g";
            }
            else if (Product.Unit == ProductUnit.kg)
            {
                //Kilograms
                qty = Quantity + "Kg*" + UnitPrice + "Kr/kg";
            }
            else if (Product.Unit == ProductUnit.hg)
            {
                //Hectograms
                return Math.Round(Quantity, 5) + "hg*" + UnitPrice + "Kr/hg";
            }
            else
            {
                if (Quantity >= 1)
                    qty = Quantity.ToString();
                else
                    qty = Quantity.ToString();
            }
            return qty;
        }

        public decimal GrossAmount() => (Direction * Quantity * UnitPrice);

        public decimal GrossAmountDiscounted() => ((Direction * Quantity) * UnitPrice) - ItemDiscount;

        public decimal NetAmount() => GrossAmountDiscounted() / (1 + (TaxPercent / 100));

        public decimal VatAmount() => GrossAmountDiscounted() - NetAmount();

        public virtual decimal ReturnGrossAmountDiscounted()
        {

            return ((Direction * Quantity) * UnitPrice) - ItemDiscount;
        }
        public virtual decimal ReturnNetAmount()
        {
            return ReturnGrossAmountDiscounted() / (1 + (TaxPercent / 100));
        }
        public virtual decimal ReturnVatAmount()
        {
            return ReturnGrossAmountDiscounted() - NetAmount();
        }
        public virtual decimal IngredientAmount()
        {
            return IngredientItems == null ? 0 : IngredientItems.Where(m => m.IngredientMode == "+").Sum(s => s.GrossAmountDiscounted());
        }
        public virtual decimal IngredientDiscount()
        {
            return IngredientItems == null ? 0 : IngredientItems.Where(m => m.IngredientMode == "+").Sum(s => s.ItemDiscount);
        }

        //  public List<OrderLine> OrderItemDetails { get; set; }
        #endregion
    }
}