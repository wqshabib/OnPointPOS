using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Utils;
using POSSUM.Utils.Controller;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Data.Entity;

namespace POSSUM.Presenter.Products
{
    public class ProductPresenter
    {
        IProductView view;
        ApplicationDbContext db;
        public ProductPresenter(IProductView view)
        {
            this.view = view;
            db = PosState.GetInstance().Context;

        }
        public ProductPresenter()
        {
            db = PosState.GetInstance().Context;
        }
        public List<Product> GetProductsByGroup(Guid groupId)
        {
            var productList = new List<Product>();
            try
            {
                return new ProductRepository(db).GetByGroup(groupId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return productList;
        }

        internal int GetCategoryByProduct(Guid id)
        {
            int categoryId = 2;
            try
            {
                categoryId = new ProductRepository(db).GetCategoryByProduct(id);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
            return categoryId;
        }

        internal List<StockModel> GetItemStock()
        {
            List<StockModel> stockes = new List<StockModel>();
            try
            {
                stockes = new ProductRepository(db).GetItemStock();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
            return stockes;
        }

        public List<Category> GetIngredientCategories()
        {
            List<Category> categories = new List<Category>();
            try
            {
                categories = new ProductRepository(db).GetIngredientCategories();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
            return categories;
        }

        public List<Product> GetProductsByGroup(IUnitOfWork uof, Guid groupId)
        {
            var productList = new List<Product>();

            productList = new ProductRepository(db).GetProductsByGroup(groupId);

            return productList;
        }
        public List<Product> GetIngredeintProducts()
        {
            var productList = new List<Product>();
            try
            {
                productList = new ProductRepository(db).GetIngredeintProducts();
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return productList;
        }


        public List<Product> GetProductsByCategory(int categoryId)
        {
            var productList = new List<Product>();
            try
            {
                productList = new ProductRepository(db).GetProductsByCategory(categoryId, Defaults.PricePolicy);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return productList;
        }


        internal void UploadProduct(Product product, int categoryId)
        {
            ProductController productController = new ProductController(PosState.GetInstance().Context);
            productController.UploadProduct(product, categoryId, Defaults.SyncAPIUri, Defaults.APIUSER, Defaults.APIPassword);
        }

        internal void DownloadProduct()
        {
            //DateTime LAST_EXECUTED_DATETIME = DateTime.Now.Date;
            //Guid outletId = default(Guid);
            //int settingId = 0;
            //using (IUnitOfWork uofLocal = PosState.GetInstance().CreateUnitOfWork())
            //{
            //    var localSettingRepo = new Repository<Setting, int>(uofLocal.Session);
            //    var setting = localSettingRepo.FirstOrDefault(s => s.OutletId == outletId && s.Type == 2);
            //    if (setting != null)
            //    {
            //        LAST_EXECUTED_DATETIME = setting.Updated != null ? Convert.ToDateTime(setting.Updated) : DateTime.Now.Date;
            //        settingId = setting.Id;
            //    }
            //}
            //ProductController productController = new ProductController(Defaults.LocalConnectionString);
            //productController.DownloadProduct(Defaults.Terminal.Id, LAST_EXECUTED_DATETIME, DateTime.Now, Defaults.SyncAPIUri, Defaults.APIUSER, Defaults.APIPassword);
        }

        internal Product SaveProduct(Product product, int categoryId)
        {
            try
            {


                using (var db = PosState.GetInstance().Context)
                {


                    var item = new Product
                    {
                        Id = Guid.NewGuid(),
                        Description = product.Description,
                        Price = product.Price,
                        PurchasePrice = product.Price,
                        Tax = product.Tax,
                        BarCode = product.BarCode,
                        PLU = product.SKU,
                        Unit = product.Unit,
                        AskWeight = product.AskWeight,
                        AskPrice = product.AskPrice,
                        PriceLock = product.PriceLock,
                        ColorCode = product.ColorCode,
                        PrinterId = product.PrinterId,
                        SortOrder = product.SortOrder,
                        Active = true,
                        Deleted = false,
                        SKU = product.SKU,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        PlaceHolder = product.PlaceHolder,
                        Seamless = false,
                        Sticky = product.Sticky,
                        Bong = product.Bong,
                        ShowItemButton = product.ShowItemButton,
                        AccountingId = product.AccountingId,
                        DiscountAllowed = product.DiscountAllowed,
                        PreparationTime = product.PreparationTime,
                        StockQuantity = product.StockQuantity


                    };
                    db.Product.Add(item);
                    int lastSortOrder = 0;
                    try
                    {
                        lastSortOrder = db.ItemCategory.Where(c => c.CategoryId == categoryId).Max(m => m.SortOrder);
                        lastSortOrder = lastSortOrder + 1;
                    }
                    catch
                    {


                    }

                    if (categoryId != 0)
                    {
                        var _itemCategory = new ItemCategory
                        {
                            ItemId = item.Id,
                            CategoryId = categoryId,
                            SortOrder = lastSortOrder < 1 ? 1 : lastSortOrder,
                            IsPrimary = true

                        };

                        db.ItemCategory.Add(_itemCategory);
                        db.SaveChanges();
                    }
                    else
                    {
                        //POSSUMDataLog.WriteLog("P9-CategoryIdIs0Exception category is is 0");
                    }

                    return item;
                }

            }
            catch (Exception ex)
            {

                view.ShowError(Defaults.AppProvider.AppTitle, ex.Message);
                product.Id = default(Guid);
                return product;
            }
        }

        internal Product UpdateProduct(Product product, int categoryId)
        {
            try
            {

                using (var db = PosState.GetInstance().Context)
                {

                    var item = db.Product.FirstOrDefault(p => p.Id == product.Id);

                    item.Description = product.Description;
                    item.Price = product.Price;
                    item.MinStockLevel = product.MinStockLevel;
                    item.Tax = product.Tax;
                    item.BarCode = product.BarCode;
                    item.PLU = product.PLU;
                    item.Unit = product.Unit;
                    item.AskWeight = product.AskWeight;
                    item.AskPrice = product.AskPrice;
                    item.PriceLock = product.PriceLock;
                    item.ColorCode = product.ColorCode;
                    item.PrinterId = product.PrinterId;
                    item.SortOrder = product.SortOrder;
                    // item.Active = true;
                    // item.Deleted = false;
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
                    item.StockQuantity = product.StockQuantity;

                    var itemcat = db.ItemCategory.FirstOrDefault(c => c.ItemId == product.Id && c.IsPrimary);
                    var oldItemcat = db.ItemCategory.FirstOrDefault(c => c.ItemId == product.Id && c.CategoryId == categoryId);
                    if (itemcat != null && categoryId!=0)
                    {
                        itemcat.ItemId = item.Id;
                        itemcat.CategoryId = categoryId;
                        itemcat.SortOrder = itemcat.SortOrder;
                        itemcat.IsPrimary = true;
                        db.Entry(itemcat).State = EntityState.Modified;
                    }
                    else if (oldItemcat != null)
                    {
                        oldItemcat.ItemId = item.Id;
                        oldItemcat.CategoryId = categoryId;
                        oldItemcat.SortOrder = oldItemcat.SortOrder;
                        oldItemcat.IsPrimary = true;
                        db.Entry(oldItemcat).State = EntityState.Modified;
                    }
                    else
                    {
                        int lastSortOrder = 0;
                        try
                        {
                            lastSortOrder = db.ItemCategory.Where(c => c.CategoryId == categoryId).Max(m => m.SortOrder);
                            lastSortOrder = lastSortOrder + 1;
                        }
                        catch
                        {
                        }

                        if (categoryId != 0)
                        {
                            var _itemCategory = new ItemCategory
                            {
                                ItemId = item.Id,
                                CategoryId = categoryId,
                                SortOrder = lastSortOrder < 1 ? 1 : lastSortOrder,
                                IsPrimary = true
                            };

                            db.ItemCategory.Add(_itemCategory);
                        }
                        else
                        {
                            //POSSUMDataLog.WriteLog("P10-CategoryIdIs0Exception category is is 0");
                        }
                    }

                    db.SaveChanges();
                    return item;
                }

            }
            catch (Exception ex)
            {

                view.ShowError(Defaults.AppProvider.AppTitle, ex.Message);
                product.Id = default(Guid);
                return product;
            }
        }

        public List<EnumValue> FillUnitType()
        {
            IEnumerable<ProductUnit> unitTypes = Enum.GetValues(typeof(ProductUnit))
                                                       .Cast<ProductUnit>();

            var types = from enumValue in unitTypes
                        select new EnumValue
                        {
                            Text = enumValue.ToString(),
                            Value = ((int)enumValue).ToString(),

                        };
            return types.ToList();
        }
        public List<EnumValue> FillPreparationTime()
        {
            IEnumerable<PrepareTime> preparationTimes = Enum.GetValues(typeof(PrepareTime))
                                                       .Cast<PrepareTime>();

            var times = from enumValue in preparationTimes
                        select new EnumValue
                        {
                            Text = enumValue.ToString(),
                            Value = ((int)enumValue).ToString(),

                        };
            return times.ToList();
        }

        public List<Product> GetProductsBykeyword(string keyword)
        {
            var productList = new List<Product>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    productList = SearchProductsBykeyword(keyword);
                    progressDialog.Closed += (arg, ev) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();

                    }));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            return productList;
        }
        public List<Product> SearchProductsBykeyword(string keyword)
        {
            var productList = new List<Product>();
            try
            {
                if (!string.IsNullOrEmpty(keyword))
                    keyword = keyword.ToLower();
                List<Product> products = new List<Product>();
                using (var db = PosState.GetInstance().Context)
                {

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        products = db.Product.Where(itm => itm.ItemType != ItemType.Pant && itm.Active == true && itm.Deleted == false && (itm.Description.ToLower().Contains(keyword) || itm.SKU.Contains(keyword) || itm.PLU == keyword || (itm.BarCode!=null && itm.BarCode.Contains(keyword)))).ToList();
                    }
                    else
                    {
                        products = db.Product.Where(itm => itm.ItemType != ItemType.Pant && itm.Active == true && itm.Deleted == false).ToList();
                    }

                    foreach (var itm in products)
                    {
                        productList.Add(new Product
                        {
                            Id = itm.Id,
                            Description = itm.Description,
                            SortOrder = itm.SortOrder,
                            Price = itm.Price,
                            Unit = itm.Unit,
                            AskPrice = itm.AskPrice,
                            AskWeight = itm.AskWeight,
                            Active = itm.Active,
                            PurchasePrice = itm.PurchasePrice,
                            ColorCode = string.IsNullOrEmpty(itm.ColorCode) ? "#FFDCDEDE" : itm.ColorCode,
                            BarCode = itm.BarCode,
                            PlaceHolder = itm.PlaceHolder,
                            PLU = itm.PLU,
                            SKU = itm.SKU,
                            PriceLock = itm.PriceLock,
                            PrinterId = itm.PrinterId,
                            Tax = itm.Tax,
                            Seamless = itm.Seamless,
                            Sticky = itm.Sticky,
                            Bong = itm.Bong,
                            AskVolume = itm.AskVolume,
                            NeedIngredient = itm.NeedIngredient,
                            DiscountAllowed = itm.DiscountAllowed,
                            PreparationTime = itm.PreparationTime,
                            ShowItemButton = itm.ShowItemButton,
                            AccountingId = itm.AccountingId,
                            ItemType = itm.ItemType,
                            ReceiptMethod = itm.ReceiptMethod,
                            HasPricePolicy = itm.HasPricePolicy,
                            StockQuantity = itm.StockQuantity,
                            IsPantEnabled = itm.IsPantEnabled,
                            PantProductId=itm.PantProductId,
                            TempPrice = itm.TempPrice,
                            TempPriceEnd=itm.TempPriceEnd,
                            TempPriceStart=itm.TempPriceStart
                            // StockQty = GetStockQty(uof, itm.Id)

                        });
                    }
                    productList = productList.OrderBy(s => s.SortOrder).ToList();

                }
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return productList;
        }

        public int GetStockQty(ApplicationDbContext db, Guid itemId)
        {
            int StockQty = 0;
            try
            {


                Guid warehouseID = Defaults.Outlet.WarehouseID;
                var stocks = db.ItemInventory.Where(s => s.ItemId == itemId && s.WarehouseID == warehouseID).ToList();
                if (stocks.Count > 0)
                    return stocks.Sum(s => s.StockCount);

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return StockQty;
        }



        public Product GetProductByBarCode(string barcode)
        {
            var Product = new Product();
            try
            {
                Product = new ProductRepository(db).GetProductByBarCode(barcode);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return Product;
        }

        public Product GetProductByBarCodeEAN13StartCode(string barcode)
        {
            var Product = new Product();
            try
            {
                Product = new ProductRepository(db).GetProductByBarCodeEAN13StartCode(barcode);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return Product;
        }

        public Product GetProductByPLU(string pluNO)
        {
            var Product = new Product();
            try
            {
                Product = new ProductRepository(db).GetProductByPlu(pluNO, Defaults.RootCategoryId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return Product;
        }
        public Product GetProductByPANT(string pluNO)
        {
            var Product = new Product();
            try
            {
                Product = new ProductRepository(db).GetProductByPANT(pluNO, Defaults.RootCategoryId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return Product;
        }
        public Product GetProductById(Guid id)
        {
            var Product = new Product();
            try
            {
                Product = new ProductRepository(db).GetProductById(id);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return Product;
        }
        public List<Category> GetCategories()
        {
            var categoryList = new List<Category>();
            try
            {
                categoryList = new ProductRepository(db).GetCategories(Defaults.RootCategoryId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return categoryList;
        }

        public List<Category> GetCategoryHierarichy()
        {

            List<Category> categories = new List<Category>();
            categories = new ProductRepository(db).GetCategoryHierarichy(Defaults.RootCategoryId);

            return categories;
        }
        public List<Accounting> GetAccountings()
        {
            var accountingList = new List<Accounting>();
            try
            {
                accountingList = new ProductRepository(db).GetAccountings();

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return accountingList;
        }
        public List<Tax> GetTaxes()
        {
            var taxList = new List<Tax>();
            try
            {
                taxList = new ProductRepository(db).GetTaxes();

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return taxList;
        }


        internal void SaveProduct(Product product)
        {
            try
            {
                new ProductRepository(db).SaveProduct(product);



            }
            catch (Exception ex)
            {

                LogWriter.LogException(ex);
            }
        }
        internal void SaveStock(ItemInventory inventory)
        {


            using (var db = PosState.GetInstance().Context)
            {

                //if (inventory.ItemId == default(Guid))
                //{

                //    var item = db.Product.FirstOrDefault(it => it.SKU == inventory.);
                //    if (item != null)
                //        inventory.ItemID = item.Id;
                //    else
                //        return;
                //}
                var _inventory = db.ItemInventory.FirstOrDefault(c => c.ItemId == inventory.ItemId && c.WarehouseID == inventory.WarehouseID && c.WarehouseLocationID == inventory.WarehouseLocationID);
                if (_inventory != null)
                {
                    //if (inventory.method == "adjust")
                    //    _inventory.StockCount = _inventory.StockCount + inventory.StockCount;
                    //else
                    _inventory.StockCount = inventory.StockCount;
                    _inventory.StockReservations = inventory.StockReservations;

                }
                else
                {
                    inventory.ItemInventoryID = Guid.NewGuid();
                    db.ItemInventory.Add(inventory);
                }
                db.SaveChanges();

            }
        }
    }
}
