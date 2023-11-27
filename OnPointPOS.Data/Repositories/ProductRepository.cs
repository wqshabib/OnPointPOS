using System;
using System.Collections.Generic;
using System.Linq;
using POSSUM.Model;
using System.Collections.ObjectModel;

namespace POSSUM.Data
{
    public class ProductRepository : GenericRepository<Product>, IDisposable
    {
        private readonly ApplicationDbContext context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public ProductRepository() : base(new ApplicationDbContext())
        {
        }
        public Product GetProductByBarCode(string barcode)
        {
            var model = new Product();

            using (var db = new ApplicationDbContext())
            {
                string ean7 = barcode.Length > 7 ? barcode.Substring(0, 8) : barcode;

                var product = db.Product.FirstOrDefault(i => i.BarCode.ToLower() == barcode.ToLower() && i.Active == true && i.Deleted == false);

                if (product == null)
                    product = db.Product.FirstOrDefault(i => i.BarCode.ToLower() == ean7.ToLower() && i.Active == true && i.Deleted == false);
                if (product == null)
                    model = new Product();
                else
                {
                    DateTime currentDate = new DateTime();
                    var pricPlocies = db.Product_PricePolicy.ToList();
                    var prices = db.ProductPrice.ToList();
                    model = new Product(product);
                    var itemCampaign =
                   db.ProductCampaign.FirstOrDefault(
                       ic =>
                           ic.ItemId == model.Id && ic.Active && currentDate >= ic.StartDate && currentDate <= ic.EndDate);
                    if (itemCampaign != null)
                    {

                        model.Campaign = db.Campaign.FirstOrDefault(c => c.Id == itemCampaign.CampaignId);
                    }

                    model.HasPricePolicy = pricPlocies.Any(p => p.ItemId == model.Id);
                    model.ProductPrices = db.ProductPrice.Where(p => p.ItemId == model.Id).ToList();
                }
            }

            return model;
        }

        public Product GetProductByBarCodeEAN13StartCode(string barcode)
        {
            var model = new Product();
            barcode = barcode.ToLower();
            using (var db = new ApplicationDbContext())
            {
                var product = db.Product.FirstOrDefault(i => i.BarCode.ToLower().StartsWith(barcode) && i.Active == true && i.Deleted == false);
                if (product == null)
                {
                    barcode = barcode.Substring(0, 7); 
                    product = db.Product.FirstOrDefault(i => i.BarCode.ToLower().StartsWith(barcode) && i.Active == true && i.Deleted == false);
                }
                if (product == null)
                    return null;
                else
                {
                    DateTime currentDate = new DateTime();
                    var pricPlocies = db.Product_PricePolicy.ToList();
                    var prices = db.ProductPrice.ToList();
                    model = new Product(product);
                    var itemCampaign =
                   db.ProductCampaign.FirstOrDefault(
                       ic =>
                           ic.ItemId == model.Id && ic.Active && currentDate >= ic.StartDate && currentDate <= ic.EndDate);
                    if (itemCampaign != null)
                    {

                        model.Campaign = db.Campaign.FirstOrDefault(c => c.Id == itemCampaign.CampaignId);
                    }

                    model.HasPricePolicy = pricPlocies.Any(p => p.ItemId == model.Id);
                    model.ProductPrices = db.ProductPrice.Where(p => p.ItemId == model.Id).ToList();
                }
            }

            return model;
        }

        public Product GetProductByPlu(string pluNo, int RootCategoryId)
        {
            var model = new Product();

            using (var db = new ApplicationDbContext())
            {

                var product = (from cat in db.Category
                               join icat in db.ItemCategory on cat.Id equals icat.CategoryId
                               join itm in db.Product on icat.ItemId equals itm.Id
                               where
                               cat.Parant == RootCategoryId && itm.PLU == pluNo && itm.Active && itm.Deleted == false
                               select itm).FirstOrDefault();
                //var product = prodRepo.FirstOrDefault(i => i.PLU == pluNO && i.Active == true && i.Deleted == false);

                if (product == null)
                {
                    model = new Product();
                }
                else
                {
                    DateTime currentDate = new DateTime();
                    var pricPlocies = db.Product_PricePolicy.ToList();
                    var prices = db.ProductPrice.ToList();
                    model = new Product(product);
                    var itemCampaign =
                   db.ProductCampaign.FirstOrDefault(
                       ic =>
                           ic.ItemId == model.Id && ic.Active && currentDate >= ic.StartDate && currentDate <= ic.EndDate);
                    if (itemCampaign != null)
                    {

                        model.Campaign = db.Campaign.FirstOrDefault(c => c.Id == itemCampaign.CampaignId);
                    }

                    model.HasPricePolicy = pricPlocies.Any(p => p.ItemId == model.Id);
                    model.ProductPrices = db.ProductPrice.Where(p => p.ItemId == model.Id).ToList();
                }
            }

            return model;
        }

        public Product GetProductByPANT(string pluNo, int RootCategoryId)
        {
            var model = new Product();

            using (var db = new ApplicationDbContext())
            {

                var product = (from cat in db.Category
                               join icat in db.ItemCategory on cat.Id equals icat.CategoryId
                               join itm in db.Product on icat.ItemId equals itm.Id
                               where itm.PLU == pluNo && itm.Active && itm.Deleted == false
                               select itm).FirstOrDefault();
                //var product = prodRepo.FirstOrDefault(i => i.PLU == pluNO && i.Active == true && i.Deleted == false);

                if (product == null)
                {
                    model = new Product();
                }
                else
                {
                    DateTime currentDate = new DateTime();
                    var pricPlocies = db.Product_PricePolicy.ToList();
                    var prices = db.ProductPrice.ToList();
                    model = new Product(product);
                    var itemCampaign =
                   db.ProductCampaign.FirstOrDefault(
                       ic =>
                           ic.ItemId == model.Id && ic.Active && currentDate >= ic.StartDate && currentDate <= ic.EndDate);
                    if (itemCampaign != null)
                    {

                        model.Campaign = db.Campaign.FirstOrDefault(c => c.Id == itemCampaign.CampaignId);
                    }

                    model.HasPricePolicy = pricPlocies.Any(p => p.ItemId == model.Id);
                    model.ProductPrices = db.ProductPrice.Where(p => p.ItemId == model.Id).ToList();
                }
            }

            return model;
        }
        public Product GetFirstOrDefault()
        {
            var model = new Product();

            using (var db = new ApplicationDbContext())
            {

                var product = db.Product.FirstOrDefault();
                if (product == null)
                {
                    model = new Product(product);
                }
                else
                {

                    DateTime currentDate = DateTime.Now;
                    var pricPlocies = db.Product_PricePolicy.ToList();
                    var prices = db.ProductPrice.ToList();
                    model = new Product(product);
                    var itemCampaign =
                        db.ProductCampaign.FirstOrDefault(
                            ic =>
                                ic.ItemId == model.Id && ic.Active && currentDate >= ic.StartDate && currentDate <= ic.EndDate);
                    if (itemCampaign != null)
                    {

                        model.Campaign = db.Campaign.FirstOrDefault(c => c.Id == itemCampaign.CampaignId);
                    }

                    model.HasPricePolicy = pricPlocies.Any(p => p.ItemId == model.Id);
                    model.ProductPrices = db.ProductPrice.Where(p => p.ItemId == model.Id).ToList();
                }
            }

            return model;
        }
        public Product GetProductById(Guid id)
        {
            var model = new Product();

            using (var db = new ApplicationDbContext())
            {

                var product = db.Product.FirstOrDefault(i => i.Id == id);
                if (product == null)
                {
                    model = new Product(product);
                }
                else
                {

                    DateTime currentDate = DateTime.Now;
                    var pricPlocies = db.Product_PricePolicy.ToList();
                    var prices = db.ProductPrice.ToList();
                    model = new Product(product);
                    var itemCampaign =
                   db.ProductCampaign.FirstOrDefault(
                       ic =>
                           ic.ItemId == model.Id && ic.Active && currentDate >= ic.StartDate && currentDate <= ic.EndDate);
                    if (itemCampaign != null)
                    {

                        model.Campaign = db.Campaign.FirstOrDefault(c => c.Id == itemCampaign.CampaignId);
                    }

                    model.HasPricePolicy = pricPlocies.Any(p => p.ItemId == model.Id);
                    model.ProductPrices = db.ProductPrice.Where(p => p.ItemId == model.Id).ToList();
                }
            }

            return model;
        }

        public List<Product> GetByKeyword(string keyword)
        {
            List<Product> items = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();

                items = (from itm in base.GetAll()
                         where
                         itm.Active && !itm.Deleted
                         && (itm.Description != null && itm.Description.ToLower().Contains(keyword)) || (itm.PLU != null && itm.PLU.ToLower() == keyword)
                          || (itm.BarCode != null && itm.BarCode.ToLower() == keyword)
                         select itm).OrderBy(o => o.SortOrder).ToList();
            }
            else
            {
                items = (from itm in GetAll()
                         where itm.Active && !itm.Deleted
                         select itm).OrderBy(i => i.SortOrder).ToList();
            }

            return items;
        }

        public List<Product> GetByGroup(Guid groupId)
        {
            var data = (from itm in base.GetAll()
                        join grp in context.ProductGroup on itm.Id equals grp.ItemId
                        where grp.GroupId == groupId
                        select itm).ToList();

            //  data.ForEach(d => d.ProductGroups = d.ItemGroups.Where(g => g.GroupId == groupId).ToList());

            //  data.ForEach(d => d.Price = d.ItemGroups[0].Price > 0 ? d.ItemGroups[0].Price : d.Price);

            return data;
        }


        public List<Product> GetProductsByCategory(int categoryId, bool pricePolicyActive)
        {
            var productList = new List<Product>();

            using (var db = new ApplicationDbContext())
            {

                var Products = (from itm in db.Product
                                join cat in db.ItemCategory on itm.Id equals cat.ItemId
                                where itm.Active && itm.ShowItemButton && itm.Deleted == false && cat.CategoryId == categoryId
                                select
                                new
                                {
                                    cat = cat,
                                    Product = itm

                                }).ToList();

                var pricPlocies = db.Product_PricePolicy.ToList();

                var prices = db.ProductPrice.ToList();
                var itemGroups = (from grp in db.ProductGroup
                                  join itm in db.Product on grp.ItemId equals itm.Id
                                  select new { grp, itm }).ToList().Select(g => new ProductGroup { Id = g.grp.Id, GroupId = g.grp.GroupId, ItemId = g.grp.ItemId, Price = g.grp.Price, Description = g.itm.Description }).ToList();
                var itemCampaigns = db.ProductCampaign.ToList();
                var campaigns = db.Campaign.ToList();
                DateTime currentDate = DateTime.Now;
                foreach (var itm in Products)
                {
                    itm.Product.SortOrder = itm.cat.SortOrder;

                    var itemCampaign =
                itemCampaigns.FirstOrDefault(
                     ic =>
                         ic.ItemId == itm.Product.Id && ic.Active && currentDate >= ic.StartDate && currentDate <= ic.EndDate);
                    if (itemCampaign != null)
                    {

                        itm.Product.Campaign = campaigns.FirstOrDefault(c => c.Id == itemCampaign.CampaignId);
                    }

                    if (pricePolicyActive)
                        itm.Product.HasPricePolicy = pricPlocies.Any(p => p.ItemId == itm.Product.Id);
                    itm.Product.Products = itm.Product.ItemType == ItemType.Grouped ? GetProductsByGroup(itemGroups, itm.Product) : null;
                    productList.Add(itm.Product);
                }
                var producs = productList.OrderBy(o => o.SortOrder);
                productList = producs.ToList();


            }


            return productList;
        }
        public List<Product> GetProductsByGroup(Guid groupId)
        {
            var productList = new List<Product>();

            using (var db = new ApplicationDbContext())
            {

                var data = (from itm in db.Product
                            join grp in db.ProductGroup on itm.Id equals grp.ItemId
                            where grp.GroupId == groupId
                            select
                            new
                            {
                                Product = itm,
                                ItemGroup = grp,

                            }).ToList();
                foreach (var product in data)
                {
                    if (product.ItemGroup.Price > 0)
                        product.Product.Price = product.ItemGroup.Price;
                }
            }

            return productList;
        }

        public List<Product> GetIngredeintProducts()
        {
            var productList = new List<Product>();

            using (var db = new ApplicationDbContext())
            {
               // var category = db.Category.FirstOrDefault(c => c.Name.ToLower().Contains("ingredien"));
                var category = db.Category.FirstOrDefault(n => n.Active == true && n.Type == CategoryType.Ingredient);

             
                int categoryId = 0;
                if (category != null)
                    categoryId = category.Id;

                var data = from itm in db.Product
                           join cat in db.ItemCategory on itm.Id equals cat.ItemId
                           where itm.Active == true && itm.Deleted == false && (cat.CategoryId == categoryId)
                           select new
                           {
                               Product = itm,
                               ItemCategory = cat


                           };
                List<Product> items = new List<Product>();
                foreach (var itm in data)
                {
                    itm.Product.SortOrder = itm.ItemCategory.SortOrder;
                    items.Add(itm.Product);
                }
                var producs = items.OrderBy(o => o.SortOrder);
                productList = producs.ToList();

            }

            return productList;
        }

        private List<Product> GetProductsByGroup(List<ProductGroup> groups, Product itm)
        {

            var productList = new List<Product>();

            foreach (var grp in groups.Where(p => p.GroupId == itm.Id))
            {
                productList.Add(new Product
                {
                    Id = grp.ItemId,
                    Description = grp.Description,
                    Price = grp.Price == 0 ? itm.Price : grp.Price,
                    Unit = itm.Unit,
                    AskPrice = itm.AskPrice,
                    AskWeight = itm.AskWeight,
                    Active = itm.Active,
                    PurchasePrice = itm.PurchasePrice,

                    ColorCode = string.IsNullOrEmpty(itm.ColorCode) ? "#FFDCDEDE" : itm.ColorCode,
                    BarCode = itm.BarCode,
                    PlaceHolder = itm.PlaceHolder,
                    PLU = itm.PLU,
                    PriceLock = itm.PriceLock,
                    PrinterId = itm.PrinterId,
                    Tax = itm.Tax,
                    Seamless = itm.Seamless,
                    Sticky = itm.Sticky,
                    Bong = itm.Bong,
                    ShowItemButton = itm.ShowItemButton,
                    ReceiptMethod = itm.ReceiptMethod,
                    ItemType = itm.ItemType,
                    SKU = itm.SKU
                });
            }


            return productList;
        }


        //public PantProduct GetPantsProductById(Guid id)
        //{
        //    return context.PantProduct.FirstOrDefault(p => p.Id == id);
        //}



        public void Dispose()
        {

        }


        public bool SaveProduct(Product product)
        {
            using (var db = new ApplicationDbContext())
            {
                bool isEdit = false;
                var item = db.Product.FirstOrDefault(p => p.Id == product.Id);
                if (item != null)
                {
                    isEdit = true;
                    item.Description = product.Description;
                    item.Price = product.Price;
                    item.PurchasePrice = product.Price;
                    item.Tax = product.Tax;
                    item.BarCode = product.BarCode;
                    item.MinStockLevel = product.MinStockLevel;
                    item.PLU = product.PLU;
                    item.Unit = product.Unit;
                    item.AskWeight = product.AskWeight;
                    item.AskPrice = product.AskPrice;
                    item.PriceLock = product.PriceLock;
                    item.ColorCode = product.ColorCode;
                    item.PrinterId = product.PrinterId;
                    item.SortOrder = product.SortOrder;
                    item.Active = product.Active;
                    item.Deleted = product.Deleted;
                    item.SKU = product.SKU;
                    item.Updated = product.Created;
                    item.PlaceHolder = product.PlaceHolder;
                    item.Seamless = false;
                    item.Sticky = product.Sticky;
                    item.Bong = product.Bong;
                    item.ShowItemButton = product.ShowItemButton;
                    item.AccountingId = product.AccountingId;
                    item.DiscountAllowed = product.DiscountAllowed;
                    item.PreparationTime = product.PreparationTime;
                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    product.Created = DateTime.Now;
                    product.Updated = DateTime.Now;
                    db.Product.Add(product);
                }
                if (product.ItemCategory != null && product.ItemCategory.Count > 0)
                {
                    if (isEdit)
                    {
                        var oldcategories = db.ItemCategory.Where(i => i.ItemId == product.Id);
                        if (oldcategories != null && oldcategories.Count() > 0)
                        {
                            db.ItemCategory.RemoveRange(oldcategories);
                        }
                    }
                    foreach (var cat in product.ItemCategory)
                    {
                        cat.IsPrimary = true;
                        if (cat.CategoryId != 0)
                        {
                            if (cat.SortOrder < 1)
                            {
                                cat.SortOrder = 1;
                            }
                            db.ItemCategory.Add(cat);
                        }
                        else
                        {
                            POSSUMDataLog.WriteLog("P4-CategoryIdIs0Exception category id is logged as 0.");
                        }
                    }
                }

                db.SaveChanges();
                return true;
            }
        }
        public Product UpdateProduct(Product product, int categoryId)
        {


            using (var db = new ApplicationDbContext())
            {

                var item = db.Product.FirstOrDefault(p => p.Id == product.Id);
                item.Description = product.Description;
                item.Price = product.Price;
                item.PurchasePrice = product.Price;
                item.Tax = product.Tax;
                item.BarCode = product.BarCode;
                item.MinStockLevel = product.MinStockLevel;
                item.PLU = product.PLU;
                item.Unit = product.Unit;
                item.AskWeight = product.AskWeight;
                item.AskPrice = product.AskPrice;
                item.PriceLock = product.PriceLock;
                item.ColorCode = product.ColorCode;
                item.PrinterId = product.PrinterId;
                item.SortOrder = product.SortOrder;
                item.Active = product.Active;
                item.Deleted = product.Deleted;
                item.SKU = product.SKU;
                item.Updated = DateTime.Now;
                item.PlaceHolder = product.PlaceHolder;
                item.Seamless = false;
                item.Sticky = product.Sticky;
                item.Bong = product.Bong;
                item.ShowItemButton = product.ShowItemButton;
                item.AccountingId = product.AccountingId;
                item.DiscountAllowed = product.DiscountAllowed;
                item.PreparationTime = product.PreparationTime;

                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                var existingCategories = db.ItemCategory.Where(c => c.ItemId == item.Id && c.CategoryId == categoryId);
                if (existingCategories != null && existingCategories.Count() > 0)
                    db.ItemCategory.RemoveRange(existingCategories);
                if (categoryId != 0)
                {
                    var itemCategory = new ItemCategory
                    {
                        ItemId = item.Id,
                        CategoryId = categoryId,
                        IsPrimary = true,
                        SortOrder = 1
                    };
                    db.ItemCategory.Add(itemCategory);
                    db.SaveChanges();

                    item.ItemCategory = new List<ItemCategory>();
                    item.ItemCategory.Add(itemCategory);
                }
                else
                {
                    POSSUMDataLog.WriteLog("P5-CategoryIdIs0Exception: Item category is 0.");
                }

                return item;
            }


        }
        public List<StockModel> GetItemStock()
        {
            List<StockModel> stockes = new List<StockModel>();
            using (var db = new ApplicationDbContext())
            {


                stockes = (from itm in db.Product
                           join inv in db.ItemInventory on itm.Id equals inv.ItemId
                           join wh in db.Warehouse on inv.WarehouseID equals wh.WarehouseID
                           join loc in db.WarehouseLocation on inv.WarehouseLocationID equals loc.WarehouseLocationID
                           select new StockModel
                           {
                               ItemId = itm.Id,
                               ItemName = itm.Description,
                               WarehouseName = wh.Name,
                               LocationName = loc.Name,
                               //StockCount = inv.StockCount,
                               StockCount = Convert.ToInt32(itm.StockQuantity),
                               StockReservation = inv.StockReservations
                           }).ToList();

            }
            return stockes;
        }


        #region get Categories
        public List<Category> GetCategories(int RootCategoryId)
        {

            using (var db = new ApplicationDbContext())
            {

                //Category with Level 2
                return
                      db.Category.Where(
                              i => i.Deleted == false && i.Parant == RootCategoryId)
                          .ToList();
            }

        }
        public List<Category> LoadCategories(int categoryId)
        {
            var categories = new List<Category>();

            using (var db = new ApplicationDbContext())
            {
                var parents =
                    db.Category.Where(n => n.Parant == categoryId && n.Active && n.Deleted == false)
                        .OrderBy(o => o.SortOrder)
                        .ToList();

                var children =
                    db.Category.Where(n => n.CategoryLevel == 3 && n.Active && n.Deleted == false)
                        .OrderBy(o => o.SortOrder)
                        .ToList();

                categories.AddRange(
                    parents.Where(p => !p.Name.ToLower().Contains("ingredien")).ToList().Select(
                        child =>
                            new Category
                            {
                                Id = child.Id,
                                Name = child.Name,
                                Parant = child.Parant,
                                ColorCode = string.IsNullOrEmpty(child.ColorCode) ? "#FFDCDEDE" : child.ColorCode,
                                IconId = child.IconId,
                                SortOrder = child.SortOrder,
                                SubCategories =
                                    children.Where(n => n.Parant == child.Id)
                                        .Select(
                                            sc =>
                                                new Category
                                                {
                                                    Id = sc.Id,
                                                    Name = sc.Name,
                                                    Parant = sc.Parant,
                                                    ColorCode =
                                                        string.IsNullOrEmpty(sc.ColorCode)
                                                            ? "#FFDCDEDE"
                                                            : sc.ColorCode,
                                                    IconId = sc.IconId,
                                                    SortOrder = sc.SortOrder
                                                })
                                        .ToList()
                            }));
            }

            return categories;
        }


        public List<Category> LoadCategories2(int categoryId)
        {
            var categories = new List<Category>();

            using (var db = new ApplicationDbContext())
            {
                var parents =
                    db.Category.Where(n => n.Parant == categoryId && n.Active && n.Deleted == false)
                        .OrderBy(o => o.SortOrder)
                        .ToList();

                var children =
                    db.Category.Where(n => n.CategoryLevel == 3 && n.Active && n.Deleted == false)
                        .OrderBy(o => o.SortOrder)
                        .ToList();

                categories.AddRange(
                    parents.Where(p => !p.Name.ToLower().Contains("ingredien")).ToList().Select(
                        child =>
                            new Category
                            {
                                Id = child.Id,
                                Name = child.Name,
                                Parant = child.Parant,
                                ColorCode = string.IsNullOrEmpty(child.ColorCode) ? "#FFDCDEDE" : child.ColorCode,
                                IconId = child.IconId,
                                SortOrder = child.SortOrder,
                                SubCategories =
                                    children.Where(n => n.Parant == child.Id)
                                        .Select(
                                            sc =>
                                                new Category
                                                {
                                                    Id = sc.Id,
                                                    Name = sc.Name,
                                                    Parant = sc.Parant,
                                                    ColorCode =
                                                        string.IsNullOrEmpty(sc.ColorCode)
                                                            ? "#FFDCDEDE"
                                                            : sc.ColorCode,
                                                    IconId = sc.IconId,
                                                    SortOrder = sc.SortOrder
                                                })
                                        .ToList()
                            }));
            }

            return categories;
        }

        public Product GetPantProductByPantId(Guid id)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Product.FirstOrDefault(a => a.Id == id);
            }
        }

        public List<Category> GetIngredientCategories()
        {
            List<Category> categories = new List<Category>();
            using (var db = new ApplicationDbContext())
            {

                //var parents = db.Category.Where(n => n.Name.ToLower().Contains("ingredien")).ToList();
                var parents = db.Category.Where(n => n.Active == true && n.Type == CategoryType.Ingredient).ToList();
                var children = db.Category.Where(n => n.CategoryLevel == 3 && n.Deleted == false).ToList();

                foreach (var parent in parents)
                {
                    var model = new Category { Id = parent.Id, Name = parent.Name, ColorCode = parent.ColorCode };

                    categories.Add(model);
                    foreach (var child in children.Where(p => p.Parant == parent.Id))
                    {
                        var _model = new Category { Id = child.Id, Name = child.Name, ColorCode = child.ColorCode };

                        categories.Add(_model);
                    }
                }
            }
            return categories;
        }
        public int GetCategoryByProduct(Guid id)
        {
            int categoryId = 2;
            using (var db = new ApplicationDbContext())
            {

                var itemcat = db.ItemCategory.FirstOrDefault(c => c.ItemId == id && c.IsPrimary == true);
                if (itemcat != null)
                {
                    categoryId = itemcat.CategoryId;
                }
            }
            return categoryId;
        }
        public List<Category> GetCategoryHierarichy(int RootCategoryId)
        {
            var categories = new List<Category>();
            using (var db = new ApplicationDbContext())
            {

                var parents =
                    db.Category.Where(n => n.Parant == RootCategoryId && n.CategoryLevel == 2 && n.Deleted == false)
                        .ToList();
                var children = db.Category.Where(n => n.CategoryLevel == 3 && n.Deleted == false).ToList();

                foreach (var parent in parents)
                {
                    var model = new Category { Id = parent.Id, Name = parent.Name };

                    var childs = new ObservableCollection<SubCategoryModel>();
                    foreach (var child in children.Where(n => n.Parant == parent.Id))
                    {
                        childs.Add(new SubCategoryModel { Id = child.Id, Name = child.Name });
                    }
                    model.Children = childs;
                    categories.Add(model);
                }
            }
            return categories;
        }
        #endregion

        public List<Accounting> GetAccountings()
        {

            return context.Accounting.Where(ac => ac.IsDeleted == false).ToList();

        }

        public List<Tax> GetTaxes()
        {

            return context.Tax.OrderBy(o => o.TaxValue).ToList();

        }

        public Campaign GetCampaign(Guid productId)
        {
            //productId = Guid.Parse("BD35775A-1853-4ABB-8378-73D594A3B43A");

            var productCompaign = (from data in context.ProductCampaign
                                   join camData in context.Campaign on data.CampaignId equals camData.Id
                                   where data.ItemId == productId
                                   orderby data.Updated descending
                                   select new { camData, data.Updated }).FirstOrDefault();

            var categoryCampaign = (from data in context.ItemCategory
                                     join campCatData in context.CategoryCampaign on data.CategoryId equals campCatData.CategoryId
                                     join camData in context.Campaign on campCatData.CampaignId equals camData.Id
                                     where data.ItemId == productId
                                     orderby campCatData.Updated descending
                                     select new { camData, campCatData.Updated }).FirstOrDefault();

            if (productCompaign != null && categoryCampaign != null)
            {
                if (productCompaign.Updated > categoryCampaign.Updated)
                {
                    return productCompaign.camData;
                }
                else
                {
                    return categoryCampaign.camData;
                }
            }
            else if (productCompaign != null)
            {
                return productCompaign.camData;
            }
            else if (categoryCampaign != null)
            {
                return categoryCampaign.camData;
            }
            else 
                return null;
        
        }

        public bool SaveProductStockHistory(List<ProductStockHistory> productStockHistories)
        {
            using (var db = new ApplicationDbContext())
            {
                foreach (var productStockHistory in productStockHistories)
                {
                    productStockHistory.Id = Guid.NewGuid();
                    productStockHistory.CreatedOn = DateTime.Now;
                    productStockHistory.UpdatedOn = DateTime.Now;

                    db.ProductStockHistory.Add(productStockHistory);
                }
                
                db.SaveChanges();
                
                return true;
            }
        }
    }
}