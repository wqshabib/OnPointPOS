using System.Collections.Generic;

namespace POSSUM.Model
{
    public class ProductData
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public List<ItemCategory> ItemCategories { get; set; }
        public List<Accounting> Accountings { get; set; }
        public List<ProductCampaign> ProductCampaigns { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<ProductPrice> ProductPrices { get; set; }
        public List<Product_PricePolicy> PricePolicies { get; set; }
    }

    public class CampaignData
    {
        public List<ProductCampaign> ProductCampaign { get; set; }
        public List<Campaign> Campaign { get; set; }
        public List<CategoryCampaign> CategoryCampaign { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }
    }

    public class ProductsData   
    {
        public List<Product> Products { get; set; }
    }

}