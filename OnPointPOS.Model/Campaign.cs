using System;
using System.ComponentModel.DataAnnotations;

namespace POSSUM.Model
{
    public class Campaign : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int BuyLimit { get; set; }
        public int FreeOffer { get; set; }
        public string Description { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
        public bool IsDiscount { get; set; } 
        //public bool IsFixedPrice { get; set; }
        //public bool IsBuyXGetN { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int LimitDiscountPercentage { get; set; }
        public int DiscountType { get; set; }
        public bool IsDeleted { get; set; }
        public bool OnceOnly { get; set; }

    }

    public class ProductCampaign : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public virtual Guid ItemId { get; set; }
   
        public int CampaignId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Active { get; set; }
        public DateTime? Updated { get; set; }
        public bool IsDeleted { get; set; }

    }


    public class CategoryCampaign : BaseEntity 
    {
        [Key]
        public int Id { get; set; }
        public virtual int CategoryId { get; set; }
        public int CampaignId { get; set; }
        public bool IsDeleted { get; set; }
        public bool Active { get; set; }
        public DateTime? Updated { get; set; }
    }


}