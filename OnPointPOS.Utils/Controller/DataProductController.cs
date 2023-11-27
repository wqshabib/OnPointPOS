using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
namespace POSSUM.Utils.Controller
{
    public class DataProductController
    {
        List<Product> liveProducts = new List<Product>();
        List<Category> liveCategories = new List<Category>();
        List<ItemCategory> liveItemCategories = new List<ItemCategory>();
        List<Accounting> liveAccountings = new List<Accounting>();
        List<Model.ProductCampaign> liveItemCampaigns = new List<ProductCampaign>();
        List<Model.ProductGroup> liveItemGroups = new List<ProductGroup>();
        List<Model.ProductPrice> liveProductPrices = new List<Model.ProductPrice>();
        List<Product_PricePolicy> liveItemPricePolicy = new List<Product_PricePolicy>();
        // private string connectionString = "";
        ApplicationDbContext db;
        public DataProductController(ApplicationDbContext uof)
        {
            db = uof; 
        }
        public DataProductController(ProductData productData, ApplicationDbContext uof)
        {
            db = uof;
            this.liveProducts = productData.Products;
            this.liveCategories = productData.Categories;
            this.liveItemCategories = productData.ItemCategories;
            this.liveAccountings = productData.Accountings;
            this.liveItemCampaigns = productData.ProductCampaigns;
            this.liveItemGroups = productData.ProductGroups;
            this.liveProductPrices = productData.ProductPrices;
            this.liveItemPricePolicy = productData.PricePolicies;
        }
        public bool UpdateProduct()
        {
            try
            {
                if (liveProducts != null)
                {
                    foreach (var product in liveProducts)
                    {
                        try
                        {
                            db.Product.AddOrUpdate(product);
                            var existingrelations = db.ItemCategory.Where(ic => ic.ItemId == product.Id).ToList();
                            foreach (var exisiting in existingrelations)
                            {
                                db.ItemCategory.Remove(exisiting);
                            }
                            var existinggroupItems = db.ProductGroup.Where(ic => ic.GroupId == product.Id).ToList();
                            foreach (var exisiting in existinggroupItems)
                            {
                                db.ProductGroup.Remove(exisiting);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(" liveProducts " + ex.ToString());
                            //return false;
                        }
                    }
                }
                //save category
                if (liveCategories != null)
                {
                    foreach (var category in liveCategories)
                    {
                        db.Category.AddOrUpdate(category);

                    }
                }
                //Save Accounting
                if (liveAccountings != null)
                {
                    foreach (var accounting in liveAccountings)
                    {
                        db.Accounting.AddOrUpdate(accounting);

                    }
                }
                if (liveItemCategories != null)
                {
                    foreach (var itemCategory in liveItemCategories)
                    {
                        try
                        {

                            var _itemCategories = db.ItemCategory.Where(u => u.ItemId == itemCategory.ItemId).ToList();// && u.CategoryId == itemCategory.CategoryId);
                            foreach (var itmCat in _itemCategories)
                            {
                                db.ItemCategory.Remove(itmCat);
                            }
                            if (itemCategory.CategoryId != 0)
                            {
                                var _itemCategory = new ItemCategory
                                {
                                    Id = itemCategory.Id,
                                    CategoryId = itemCategory.CategoryId,
                                    ItemId = itemCategory.ItemId,
                                    SortOrder = itemCategory.SortOrder < 1 ? 1 : itemCategory.SortOrder,
                                    IsPrimary = itemCategory.IsPrimary
                                };
                                db.ItemCategory.Add(_itemCategory);
                            }
                            else
                            {
                                Log.WriteLog("P7-CategoryIdIs0Exception: Item category is 0.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(" liveItemCategories " + ex.ToString());
                            //return false;
                        }

                    }
                }
                if (liveItemCampaigns != null && liveItemCampaigns.Count > 0)
                {
                    int lastId = 0;
                    try
                    {
                        lastId = (int)db.ProductCampaign.Max(m => m.Id);
                        lastId = lastId + 1;
                    }
                    catch
                    {
                        lastId = 1;
                    }
                    foreach (var itemCampaign in liveItemCampaigns)
                    {
                        try
                        {
                            var _itemCampaign = db.ProductCampaign.FirstOrDefault(u => u.ItemId == itemCampaign.ItemId);
                            if (_itemCampaign == null)
                            {
                                var NewitemCampaign = new ProductCampaign
                                {
                                    Id = lastId,
                                    CampaignId = itemCampaign.CampaignId,
                                    ItemId = itemCampaign.ItemId,
                                    EndDate = itemCampaign.EndDate,
                                    StartDate = itemCampaign.StartDate,
                                    Active = itemCampaign.Active,
                                    Updated = itemCampaign.Updated
                                };
                                db.ProductCampaign.Add(NewitemCampaign);
                            }
                            else
                            {
                                db.ProductCampaign.Remove(_itemCampaign);
                                var NewitemCampaign = new ProductCampaign
                                {
                                    Id = lastId,
                                    CampaignId = itemCampaign.CampaignId,
                                    ItemId = itemCampaign.ItemId,
                                    EndDate = itemCampaign.EndDate,
                                    StartDate = itemCampaign.StartDate,
                                    Active = itemCampaign.Active,
                                    Updated = itemCampaign.Updated
                                };
                                db.ProductCampaign.Add(NewitemCampaign);
                            }
                            lastId++;
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(" liveItemCampaigns " + ex.ToString());
                            //return false;
                        }
                    }
                        
                }
                //item group
                if (liveItemGroups != null && liveItemGroups.Count > 0)
                {
                    int lastId = 0;
                    try
                    {
                        lastId = (int)db.ProductGroup.Max(m => m.Id);
                        lastId = lastId + 1;
                    }
                    catch
                    {
                        lastId = 1;
                    }
                    foreach (var itemGroup in liveItemGroups)
                    {
                        try
                        {
                            var _itemGroup = db.ProductGroup.FirstOrDefault(u => u.ItemId == itemGroup.ItemId && u.GroupId == itemGroup.GroupId);
                            if (_itemGroup == null)
                            {
                                var NewitemGroup = new ProductGroup
                                {
                                    Id = lastId,
                                    GroupId = itemGroup.GroupId,
                                    ItemId = itemGroup.ItemId,
                                    Price = itemGroup.Price,
                                    Updated = itemGroup.Updated
                                };
                                db.ProductGroup.Add(NewitemGroup);
                            }
                            else
                            {
                                db.ProductGroup.Remove(_itemGroup);
                                var Newitemgroup = new ProductGroup
                                {
                                    Id = lastId,
                                    GroupId = itemGroup.GroupId,
                                    ItemId = itemGroup.ItemId,

                                    Price = itemGroup.Price,
                                    Updated = itemGroup.Updated
                                };
                                db.ProductGroup.Add(Newitemgroup);
                            }
                            lastId++;
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(" liveItemGroups " + ex.ToString());
                            //return false;
                        }
                    }
                        

                }
                //Product Prices 
                if (liveProductPrices != null && liveProductPrices.Count > 0)
                {

                    foreach (var productPrice in liveProductPrices)
                    {
                        try
                        {
                            var _productPrice = db.ProductPrice.FirstOrDefault(u => u.ItemId == productPrice.ItemId && u.OutletId == productPrice.OutletId && u.PriceMode == productPrice.PriceMode);
                            if (_productPrice == null)
                            {

                                var _NewProductPrice = new ProductPrice
                                {

                                    OutletId = productPrice.OutletId,
                                    ItemId = productPrice.ItemId,
                                    Price = productPrice.Price,
                                    PurchasePrice = productPrice.PurchasePrice,
                                    PriceMode = productPrice.PriceMode,
                                    Updated = productPrice.Updated

                                };
                                db.ProductPrice.Add(_NewProductPrice);

                            }
                            else
                            {
                                _productPrice.Price = productPrice.Price;
                                _productPrice.PurchasePrice = productPrice.PurchasePrice;
                                _productPrice.PriceMode = productPrice.PriceMode;
                                _productPrice.Updated = productPrice.Updated;
                                db.ProductPrice.AddOrUpdate(_productPrice);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(" liveProductPrices " + ex.ToString());
                            //return false;
                        }

                    }


                }
                //Product Prices Policy
                if (liveItemPricePolicy != null && liveItemPricePolicy.Count > 0)
                {

                    foreach (var productPricepolicy in liveItemPricePolicy)
                    {
                        try
                        {
                            var _productPricePolicy = db.Product_PricePolicy.FirstOrDefault(u => u.Id == productPricepolicy.Id);
                            if (_productPricePolicy == null)
                            {
                                _productPricePolicy = new Product_PricePolicy
                                {

                                    Id = productPricepolicy.Id,
                                    ItemId = productPricepolicy.ItemId,
                                    BuyLimit = productPricepolicy.BuyLimit,
                                    DiscountAmount = productPricepolicy.DiscountAmount,
                                    Active = productPricepolicy.Active,
                                    Deleted = productPricepolicy.Deleted,
                                    Updated = productPricepolicy.Updated

                                };
                            }
                            else
                            {

                                _productPricePolicy.ItemId = productPricepolicy.ItemId;
                                _productPricePolicy.BuyLimit = productPricepolicy.BuyLimit;
                                _productPricePolicy.DiscountAmount = productPricepolicy.DiscountAmount;
                                _productPricePolicy.Active = productPricepolicy.Active;
                                _productPricePolicy.Deleted = productPricepolicy.Deleted;
                                _productPricePolicy.Updated = productPricepolicy.Updated;
                            }

                            db.Product_PricePolicy.AddOrUpdate(_productPricePolicy);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(" liveItemPricePolicy " + ex.ToString());
                            //return false;
                        }
                    }


                }
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }
        }

        public bool UploadProducts(DateTime dt, string baseUrl, string apiUser , string apiPassword )
        {
            try
            {
                var lst = db.Product.Where(a => a.Updated >= dt).ToList();
                Log.WriteLog("Found " + lst.Count + " Products to be uploaded.");
                foreach (var item in lst)
                {
                    Log.WriteLog("Uploading product with Id = " + item.Id + ", baseUrl=" + baseUrl + " , apiUser = " + apiUser + ", apiPassword = "+ apiPassword);
                    UploadProduct(item, item.CategoryId, baseUrl, apiUser, apiPassword);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }
        }

        public void UploadProduct(Product product, int categoryId, string baseUrl , string apiUser , string apiPassword )
        {
            try
            {
                var category = db.ItemCategory.FirstOrDefault(a => a.CategoryId == categoryId);
                if (category!=null)
                {
                    product.ItemCategory_IsPrimary = category.IsPrimary;
                    product.ItemCategory_SortOrder = category.SortOrder;
                }

                product.CategoryId = categoryId;
                DataServiceClient client = new DataServiceClient(baseUrl, apiUser, apiPassword);
                client.PostProduct(product);


            }
            catch (Exception ex)
            {
                
                Log.WriteLog(ex.ToString());
            }
        }

    }
}
