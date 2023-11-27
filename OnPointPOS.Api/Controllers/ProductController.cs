using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

using System.IO;
//using NHibernate.Proxy;

using System.Reflection;
//using NHibernate;
using System.Configuration;
//using FluentNHibernate.Cfg;
//using FluentNHibernate.Cfg.Db;
using System.Runtime;
using Microsoft.CSharp;
using System.Threading.Tasks;

using System.Web.Http;

using System.Text;
using System.Diagnostics;
using POSSUM.Model;
using System.Collections;
//using POSSUM.Data;
using Newtonsoft.Json;
//using System.Web.Http.Cors;
using System.Data.Entity;
//using PosserAPI.Models;
using POSSUM.Data;
using POSSUM.Api.Providers;

namespace POSSUM.Api.Controllers
{
    // [EnableCors(origins: "*", headers: "*", methods: "*")]
    //  [Authorize]
    [System.Web.Http.RoutePrefix("api/Product")]
    public class ProductController : BaseAPIController
    {
        /// <summary>
        /// 
        /// </summary>
        private string connectionString = "";

        /// <summary>
        /// 
        /// </summary>
        private bool nonAhenticated = true;

        /// <summary>
        /// 
        /// </summary>
        public ProductController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        /// <summary>
        /// Get All Products and Categories
        /// </summary>
        /// <returns></returns>
        [Route("GetAllProducts")]
        public async Task<ProductData> GetAllProducts()
        {
            try
            {

                LogWriter.LogWrite("GetAllProducts calling and connectionstrings is:  " + connectionString);

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    string databaseName = db.Database.Connection.Database;

                    var liveAccountings = db.Accounting.ToList();
                    var liveProducts = await db.Product.ToListAsync();
                    var liveCategories = await db.Category.ToListAsync();
                    var liveItemCampaigns = await db.ProductCampaign.ToListAsync();
                    var liveProductPrices = await db.ProductPrice.ToListAsync();
                    var livePricePolicy = await db.Product_PricePolicy.ToListAsync();

                    var liveItemCategories = new List<ItemCategory>();
                    var liveItemGroups = new List<ProductGroup>();

                    if (liveProducts.Count > 0)
                        foreach (var prod in liveProducts)
                        {
                            var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                            if (itmCats.Count > 0)
                                liveItemCategories.AddRange(itmCats);
                            var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                            if (itemGroups.Count > 0)
                                liveItemGroups.AddRange(itemGroups);
                            //if (databaseName == "Sannegården_1_0"||databaseName=="Jos")
                            //{

                            // }

                        }
                    ProductData productData = new ProductData();
                    productData.Products = liveProducts;
                    productData.Categories = liveCategories;
                    productData.ItemCategories = liveItemCategories;
                    productData.Accountings = liveAccountings;
                    productData.ProductCampaigns = liveItemCampaigns;
                    productData.ProductGroups = liveItemGroups;
                    //  if (liveProductPrices.Count > 0)
                    // liveProductPrices = liveProductPrices.ToList();
                    productData.ProductPrices = liveProductPrices;
                    productData.PricePolicies = livePricePolicy;
                    return productData;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                return productData;
            }

        }





        /// <summary>
        /// Get Product and categories updated in btween a date range
        /// </summary>
        /// <param name="dates">Last Sync Date, Curretn Date, Terminal Id</param>
        /// <returns>Model of ProductData that includes List of Products, Categories and Item Campaing , Accouting</returns>
        [Route("GetProducts")]
        public async Task<ProductData> GetProducts([FromUri] Dates dates)
        {
            try
            {
                LogWriter.LogWrite("GetProducts");

                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate.AddMinutes(-5);
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                var terminalId = dates.TerminalId;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    string databaseName = db.Database.Connection.Database;
                    var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                    var outletId = terminal.Outlet.Id;
                    var liveAccountings = db.Accounting.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveProducts = db.Product.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveCategories = db.Category.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveItemCampaigns = db.ProductCampaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveProductPrices = db.ProductPrice.ToList();
                    var livePricePolicy = db.Product_PricePolicy.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();

                    var liveItemCategories = new List<ItemCategory>();
                    var liveItemGroups = new List<ProductGroup>();

                    if (liveProducts.Count > 0)
                        foreach (var prod in liveProducts)
                        {
                            var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                            if (itmCats.Count > 0)
                                liveItemCategories.AddRange(itmCats);
                            var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                            if (itemGroups.Count > 0)
                                liveItemGroups.AddRange(itemGroups);
                            //if (databaseName == "Sannegården_1_0"||databaseName=="Jos")
                            //{
                            var outletPrice = liveProductPrices.FirstOrDefault(p => p.ItemId == prod.Id && p.OutletId == outletId && p.PriceMode == PriceMode.Day);
                            if (outletPrice != null)
                                prod.Price = outletPrice.Price;
                            // }

                        }
                    ProductData productData = new ProductData();
                    productData.Products = liveProducts;
                    productData.Categories = liveCategories;
                    productData.ItemCategories = liveItemCategories;
                    productData.Accountings = liveAccountings;
                    productData.ProductCampaigns = liveItemCampaigns;
                    productData.ProductGroups = liveItemGroups;
                    if (liveProductPrices.Count > 0)
                        liveProductPrices = liveProductPrices.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    productData.ProductPrices = liveProductPrices;
                    productData.PricePolicies = livePricePolicy;
                    return productData;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                return productData;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        [Route("GetCampaignsData")]
        public async Task<CampaignData> GetCampaignsData([FromUri] Dates dates)
        {
            try
            {
                LogWriter.LogWrite("GetCampaignsData");

                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate.AddMinutes(-5);
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                var terminalId = dates.TerminalId;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    string databaseName = db.Database.Connection.Database;
                    var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                    var outletId = terminal.Outlet.Id;

                    var lstProductCampaign = db.ProductCampaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var lstCampaign = db.Campaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var lstCategoryCampaign = db.CategoryCampaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();

                    CampaignData campaignData = new CampaignData();
                    campaignData.Campaign = lstCampaign;
                    campaignData.CategoryCampaign = lstCategoryCampaign;
                    campaignData.ProductCampaign = lstProductCampaign;
                    campaignData.Result = true;
                    campaignData.Message = "Success";
                    return campaignData;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                CampaignData campaignData = new CampaignData();
                campaignData.Message = ex.ToString();
                campaignData.Result = false;
                return campaignData;
            }
        }

        /// <summary>
        /// Get Product and categories updated in btween a date range
        /// </summary>
        /// <param name="dates">Last Sync Date, Curretn Date, Terminal Id</param>
        /// <returns>Model of ProductData that includes List of Products, Categories and Item Campaing , Accouting</returns>
        [Route("GetProductsInPaging")]
        public async Task<ProductData> GetProductsInPaging([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate.AddMinutes(-5);
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                var terminalId = dates.TerminalId;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    int skip = dates.PageNo * dates.PageSize;
                    int take = dates.PageSize;

                    string databaseName = db.Database.Connection.Database;
                    var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                    var outletId = terminal.Outlet.Id;
                    var liveAccountings = db.Accounting.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveProducts = db.Product.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).OrderByDescending(a => a.Updated).Skip(skip).Take(take).ToList();
                    var liveCategories = db.Category.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveItemCampaigns = db.ProductCampaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveProductPrices = db.ProductPrice.ToList();
                    var livePricePolicy = db.Product_PricePolicy.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();

                    var liveItemCategories = new List<ItemCategory>();
                    var liveItemGroups = new List<ProductGroup>();

                    if (liveProducts.Count > 0)
                    {
                        foreach (var prod in liveProducts)
                        {
                            var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                            if (itmCats.Count > 0)
                                liveItemCategories.AddRange(itmCats);
                            var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                            if (itemGroups.Count > 0)
                                liveItemGroups.AddRange(itemGroups);

                            var outletPrice = liveProductPrices.FirstOrDefault(p => p.ItemId == prod.Id && p.OutletId == outletId && p.PriceMode == PriceMode.Day);
                            if (outletPrice != null)
                                prod.Price = outletPrice.Price;
                        }
                    }

                    ProductData productData = new ProductData();
                    productData.Products = liveProducts;
                    productData.Categories = liveCategories;
                    productData.ItemCategories = liveItemCategories;
                    productData.Accountings = liveAccountings;
                    productData.ProductCampaigns = liveItemCampaigns;
                    productData.ProductGroups = liveItemGroups;
                    if (liveProductPrices.Count > 0)
                        liveProductPrices = liveProductPrices.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    productData.ProductPrices = liveProductPrices;
                    productData.PricePolicies = livePricePolicy;
                    return productData;
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                return productData;
            }

        }



        /// <summary>
        /// Get Product and categories updated in btween a date range
        /// </summary>
        /// <param name="dates">Last Sync Date, Curretn Date, Terminal Id</param>
        /// <returns>Model of ProductData that includes List of Products, Categories and Item Campaing , Accouting</returns>
        [HttpGet]
        [Route("GetCategoryWithProducts")]
        public HttpResponseMessage GetCategoryWithProducts()
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var liveCategories = db.Category.Where(p => !p.Deleted).ToList();

                    List<Category> lstCategory = new List<Category>();

                    foreach (var category in liveCategories)
                    {

                        //var ddd = db.ProductIngredients.Include(p => p.Product);
                        //var results = ddd.GroupBy(p => p.ProductId, p => p.Product, (key, g) => new { ProductId = key, Ingredients = g.ToList() }).ToList();

                        var products = (from itm in db.Product
                                        join cat in db.ItemCategory on itm.Id equals cat.ItemId
                                        where itm.Active == true && itm.Deleted == false && (cat.CategoryId == category.Id)
                                        select itm).ToList();

                        foreach (var item in products.Where(p => p.NeedIngredient))
                        {
                            var ingredients = db.ProductIngredients.Include(p => p.Product).Where(p => p.ProductId == item.Id).ToList();
                            item.ProductIngredients = ingredients;
                        }

                        category.Products = products.Select(o => new Product(o)).ToList();
                        lstCategory.Add(category);
                    }

                    ProductData productData = new ProductData();
                    productData.Categories = lstCategory;
                    return Request.CreateResponse(HttpStatusCode.OK, lstCategory);
                    //return productData;
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                //return productData;
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, productData);

            }

        }



        public class CatProductModel
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductsWithoutExternalID")]
        public List<Product> GetProductsWithoutExternalID([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate.AddMinutes(-5);
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                var terminalId = dates.TerminalId;
                var pageNo = dates.PageNo;
                var pagesize = dates.PageSize;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    string databaseName = db.Database.Connection.Database;
                    var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                    var outletId = terminal.Outlet.Id;
                    var pgNo = pageNo * pagesize;
                    var currrentTime = DateTime.Now.AddMinutes(-10);

                    var liveProducts = db.Product.Where(p => string.IsNullOrEmpty(p.ExternalID) || (!string.IsNullOrEmpty(p.ExternalID) && p.Updated >= currrentTime)).OrderBy(o => o.Created).Skip(pgNo).Take(pagesize).ToList();
                    //LogWriter.LogWrite("liveProducts: " + liveProducts);
                    LogWriter.LogWrite("liveProducts withour externalid: +CURRENT TIME: " + liveProducts.Count + " : " + currrentTime);


                    var liveProductPrices = db.ProductPrice.ToList();

                    var liveItemCategories = new List<ItemCategory>();
                    var liveItemGroups = new List<ProductGroup>();

                    if (liveProducts.Count > 0)
                        foreach (var prod in liveProducts)
                        {
                            var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                            if (itmCats.Count > 0)
                                liveItemCategories.AddRange(itmCats);
                            var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                            if (itemGroups.Count > 0)
                                liveItemGroups.AddRange(itemGroups);

                            var outletPrice = liveProductPrices.FirstOrDefault(p => p.ItemId == prod.Id && p.OutletId == outletId && p.PriceMode == PriceMode.Day);
                            if (outletPrice != null)
                                prod.Price = outletPrice.Price;


                        }
                    ProductsData productData = new ProductsData();
                    productData.Products = liveProducts;
                    return liveProducts.ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                List<Product> productData = new List<Product>();
                return productData;
            }

        }

        // it was created because GetProducts was prolem with await function, so we created a new instance with name api-2.possum.com
        /* [Route("GetProductsV2")]
         public async Task<ProductData> GetProductsV2([FromUri] Dates dates)
         {
             try
             {
                 DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate.AddMinutes(-5);
                 DateTime EXECUTED_DATETIME = dates.CurrentDate;
                 var terminalId = dates.TerminalId;

                 using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                 {
                     string databaseName = db.Database.Connection.Database;
                     var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                     var outletId = terminal.Outlet.Id;
                     var liveAccountings = db.Accounting.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                     var liveProducts = db.Product.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                     var liveCategories = db.Category.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                     var liveItemCampaigns = db.ProductCampaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                     var liveProductPrices = db.ProductPrice.ToList();
                     var livePricePolicy = db.Product_PricePolicy.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();

                     var liveItemCategories = new List<ItemCategory>();
                     var liveItemGroups = new List<ProductGroup>();

                     if (liveProducts.Count > 0)
                         foreach (var prod in liveProducts)
                         {
                             var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                             if (itmCats.Count > 0)
                                 liveItemCategories.AddRange(itmCats);
                             var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                             if (itemGroups.Count > 0)
                                 liveItemGroups.AddRange(itemGroups);
                             //if (databaseName == "Sannegården_1_0"||databaseName=="Jos")
                             //{
                             var outletPrice = liveProductPrices.FirstOrDefault(p => p.ItemId == prod.Id && p.OutletId == outletId && p.PriceMode == PriceMode.Day);
                             if (outletPrice != null)
                                 prod.Price = outletPrice.Price;
                             // }

                         }
                     ProductData productData = new ProductData();
                     productData.Products = liveProducts;
                     productData.Categories = liveCategories;
                     productData.ItemCategories = liveItemCategories;
                     productData.Accountings = liveAccountings;
                     productData.ProductCampaigns = liveItemCampaigns;
                     productData.ProductGroups = liveItemGroups;
                     if (liveProductPrices.Count > 0)
                         liveProductPrices = liveProductPrices.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                     productData.ProductPrices = liveProductPrices;
                     productData.PricePolicies = livePricePolicy;
                     return productData;
                 }
             }
             catch (Exception ex)
             {
                 Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                 ProductData productData = new ProductData();
                 return productData;
             }

         }
         */

        /// <summary>
        /// Get Product by Id
        /// </summary>
        /// <param name="id">Item Id as Parameter</param>
        /// <returns>Model of ProductData that includes List of Products, Categories and Item Campaing , Accouting</returns>
        [Route("GetProductById")]
        public async Task<ProductData> GetProductById(Guid id)
        {
            try
            {


                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    string databaseName = db.Database.Connection.Database;

                    var liveProducts = await db.Product.Where(p => p.Id == id).ToListAsync();
                    var liveItemCampaigns = await db.ProductCampaign.Where(p => p.ItemId == id).ToListAsync();
                    var liveProductPrices = await db.ProductPrice.Where(p => p.ItemId == id).ToListAsync();
                    var livePricePolicy = await db.Product_PricePolicy.Where(p => p.ItemId == id).ToListAsync();

                    var liveItemCategories = new List<ItemCategory>();
                    var liveItemGroups = new List<ProductGroup>();

                    if (liveProducts.Count > 0)
                        foreach (var prod in liveProducts)
                        {
                            var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                            if (itmCats.Count > 0)
                                liveItemCategories.AddRange(itmCats);
                            var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                            if (itemGroups.Count > 0)
                                liveItemGroups.AddRange(itemGroups);


                        }
                    ProductData productData = new ProductData();
                    productData.Products = liveProducts;
                    productData.Categories = new List<Category>();
                    productData.ItemCategories = liveItemCategories;
                    productData.Accountings = new List<Accounting>();
                    productData.ProductCampaigns = liveItemCampaigns;
                    productData.ProductGroups = liveItemGroups;
                    if (liveProductPrices.Count > 0)
                        liveProductPrices = liveProductPrices.Where(p => p.ItemId == id).ToList();
                    productData.ProductPrices = liveProductPrices;
                    productData.PricePolicies = livePricePolicy;
                    return productData;
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                return productData;
            }

        }

        /// <summary>
        /// Get Item Stock detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("GetItemInventory")]
        public async Task<List<ItemInventory>> GetItemInventory(Guid id)
        {
            try
            {
                List<ItemInventory> liveItemInventories = new List<ItemInventory>();

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    string databaseName = db.Database.Connection.Database;
                    if (id != default(Guid))
                        liveItemInventories = await db.ItemInventory.Where(p => p.ItemId == id).ToListAsync();
                    else
                        liveItemInventories = await db.ItemInventory.ToListAsync();


                    return liveItemInventories;
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));

                return new List<ItemInventory>();
            }

        }

        /// <summary>
        /// Post Product added at NIMPOS side
        /// </summary>
        /// <param name="product">Product Model</param>
        /// <returns></returns>
		[HttpPost]
        [Route("PostProduct")]
        public IHttpActionResult PostProduct(Product product)
        {
            try
            {
                //   var helper = new NHibernateHelper(ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString).SessionFactory;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    //var prodRepo = new Repository<Product, int>(uof.Session);
                    //var itemCategoryRepo = new Repository<ItemCategory, int>(uof.Session);
                    var _product = db.Product.FirstOrDefault(u => u.Id == product.Id);
                    if (_product == null)
                    {
                        db.Product.Add(product);
                    }
                    else
                    {
                        _product.AccountingId = product.AccountingId;
                        _product.Description = product.Description;
                        _product.Price = product.Price;
                        _product.PurchasePrice = product.PurchasePrice;
                        _product.Tax = product.Tax;
                        _product.BarCode = product.BarCode;
                        //_product.MinStockLevel = product.MinStockLevel;
                        _product.PLU = product.PLU;
                        _product.Unit = product.Unit;
                        _product.AskPrice = product.AskPrice;
                        _product.AskWeight = product.AskWeight;
                        _product.PriceLock = product.PriceLock;
                        _product.ColorCode = product.ColorCode;
                        _product.PrinterId = product.PrinterId;
                        _product.SortOrder = product.SortOrder;
                        _product.Active = product.Active;
                        _product.Deleted = product.Deleted;
                        _product.Tax = product.Tax;
                        _product.PlaceHolder = product.PlaceHolder;
                        _product.Sticky = product.Sticky;
                        _product.Seamless = product.Seamless;
                        _product.Bong = product.Bong;
                    }

                    db.SaveChanges();

                    if (product.CategoryId == 0)
                    {
                        LogWriter.LogWrite("P1-CategoryIdIs0Exception Category Id is posted as 0 in sync which is invalid. Product object is " + product.Id);
                    }

                    if (product.ItemCategory != null && product.ItemCategory.Count > 0)
                    {
                        var oldItemCategories = db.ItemCategory.Where(a => a.ItemId == product.Id).ToList();
                        if (oldItemCategories.Count > 0)
                        {
                            db.ItemCategory.RemoveRange(oldItemCategories);
                            db.SaveChanges();
                        }

                        foreach (var item in product.ItemCategory)
                        {
                            var itemCategory = new ItemCategory
                            {
                                ItemId = item.ItemId,
                                CategoryId = item.CategoryId,
                                IsPrimary = item.IsPrimary,
                                SortOrder = item.SortOrder < 1 ? 1 : item.SortOrder
                            };

                            db.ItemCategory.Add(itemCategory);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var _itemCategory = db.ItemCategory.FirstOrDefault(u => u.ItemId == product.Id && u.CategoryId == product.CategoryId);
                        if (_itemCategory == null && product.CategoryId != 0)
                        {
                            var oldItemCategories = db.ItemCategory.Where(a => a.ItemId == product.Id).ToList();
                            if (oldItemCategories.Count > 0)
                            {
                                db.ItemCategory.RemoveRange(oldItemCategories);
                                db.SaveChanges();
                            }

                            _itemCategory = new ItemCategory
                            {
                                ItemId = product.Id,
                                CategoryId = product.CategoryId,
                                IsPrimary = product.ItemCategory_IsPrimary,
                                SortOrder = product.ItemCategory_SortOrder < 1 ? 1 : product.ItemCategory_SortOrder
                            };
                            db.ItemCategory.Add(_itemCategory);
                            db.SaveChanges();
                        }
                    }

                    db.SaveChanges();
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// Post Product added at NIMPOS side
        /// </summary>
        /// <param name="product">Product Model</param>
        /// <returns></returns>
		[HttpPost]
        [Route("PostProductWithItemCategory")]
        public IHttpActionResult PostProductWithItemCategory(Product product)
        {
            try
            {
                //   var helper = new NHibernateHelper(ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString).SessionFactory;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    //var prodRepo = new Repository<Product, int>(uof.Session);
                    //var itemCategoryRepo = new Repository<ItemCategory, int>(uof.Session);
                    var _product = db.Product.FirstOrDefault(u => u.Id == product.Id);
                    if (_product == null)
                    {
                        db.Product.Add(product);
                    }
                    else
                    {

                        _product.Description = product.Description;
                        _product.Price = product.Price;
                        _product.PurchasePrice = product.PurchasePrice;
                        _product.Tax = product.Tax;
                        _product.BarCode = product.BarCode;
                        //_product.MinStockLevel = product.MinStockLevel;
                        _product.PLU = product.PLU;
                        _product.Unit = product.Unit;
                        _product.AskPrice = product.AskPrice;
                        _product.AskWeight = product.AskWeight;
                        _product.PriceLock = product.PriceLock;
                        _product.ColorCode = product.ColorCode;
                        _product.PrinterId = product.PrinterId;
                        _product.SortOrder = product.SortOrder;
                        _product.Active = product.Active;
                        _product.Deleted = product.Deleted;
                        _product.Tax = product.Tax;
                        _product.PlaceHolder = product.PlaceHolder;
                        _product.Sticky = product.Sticky;
                        _product.Seamless = product.Seamless;
                        _product.Bong = product.Bong;
                    }

                    foreach (var itmCat in product.ItemCategory)
                    {
                        try
                        {
                            if (itmCat.CategoryId != 0)
                            {
                                var _itemCategories = db.ItemCategory.FirstOrDefault(u => u.ItemId == itmCat.ItemId && u.CategoryId == itmCat.CategoryId);// && u.CategoryId == itemCategory.CategoryId);
                                if (_itemCategories == null)
                                {
                                    var _itemCategory = new ItemCategory
                                    {
                                        CategoryId = itmCat.CategoryId,
                                        ItemId = itmCat.ItemId,
                                        SortOrder = itmCat.SortOrder < 1 ? 1 : itmCat.SortOrder,
                                        IsPrimary = itmCat.IsPrimary
                                    };
                                    db.ItemCategory.Add(_itemCategory);
                                }
                                else
                                {
                                    _itemCategories.CategoryId = itmCat.CategoryId;
                                    _itemCategories.ItemId = itmCat.ItemId;
                                    _itemCategories.SortOrder = itmCat.SortOrder;
                                    _itemCategories.IsPrimary = itmCat.IsPrimary;
                                    db.Entry(_itemCategories).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                LogWriter.LogWrite("P2-P1-CategoryIdIs0Exception Category Id is logged as 0");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogWrite(ex);
                            //Log.WriteLog(" liveItemCategories " + ex.ToString());
                            //return false;
                        }

                    }


                    //  uof.Commit();
                    db.SaveChanges();
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostProductsBatch")]
        public IHttpActionResult PostProductsBatch(List<Product> products)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    var defaultCategoryForVISMA = ConfigurationManager.AppSettings["DefaultCategoryForVISMA"];
                    if (string.IsNullOrEmpty(defaultCategoryForVISMA))
                    {
                        defaultCategoryForVISMA = "WebShop";
                    }

                    var category = db.Category.FirstOrDefault(a => a.Name == defaultCategoryForVISMA);

                    int categoryId = 83;
                    //if (category != null)
                    //{
                    //    categoryId = category.Id;
                    //}

                    //var data = JsonConvert.SerializeObject(products);
                    //LogWriter.LogWrite("PostProductsBatch + products data " + data);

                    foreach (var product in products)
                    {
                        try
                        {

                            var _product = db.Product.FirstOrDefault(u => u.ExternalID == product.ExternalID);
                            if (_product == null)
                            {
                                product.CategoryId = categoryId;
                                product.Active = true;
                                product.ShowItemButton = true;
                                product.DiscountAllowed = true;
                                product.Id = Guid.NewGuid();
                                db.Product.Add(product);
                                _product = new Product { Id = product.Id };
                                db.SaveChanges();
                            }
                            else
                            {
                                try
                                {
                                    var currentTime = DateTime.Now.AddMinutes(-30);
                                    if (_product.Updated >= currentTime)
                                    {
                                        LogWriter.LogWrite("PostProductsBatch product not going to upate product id: " + _product.Id + " updated time:" + _product.Updated);

                                    }
                                    else
                                    {
                                        if (product.MinStockLevel != null && product.MinStockLevel > 0)
                                            _product.MinStockLevel = product.MinStockLevel;
                                        _product.Price = product.Price;
                                        _product.Description = product.Description;
                                        _product.Tax = product.Tax;
                                        _product.StockQuantity = product.StockQuantity;
                                        _product.Updated = DateTime.Now;
                                        _product.ExternalID = product.ExternalID;
                                        db.Entry(_product).State = EntityState.Modified;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    LogWriter.LogWrite(ex);

                                }
                            }

                            var _itemCategory = db.ItemCategory.FirstOrDefault(u => u.ItemId == _product.Id);
                            if (categoryId == 0)
                            {
                                LogWriter.LogWrite("P3-CategoryIdIs0Exception category id is 0 in Post product bach API.");
                            }

                            if (_itemCategory == null && categoryId != 0)
                            {
                                _itemCategory = new ItemCategory
                                {
                                    ItemId = _product.Id,
                                    CategoryId = categoryId,
                                    IsPrimary = true,
                                    SortOrder = 1
                                };
                                db.ItemCategory.Add(_itemCategory);
                            }

                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogWrite(ex);
                        }
                    }
                }
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return BadRequest(ex.ToString());// StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostProductsBatchNew")]
        public IHttpActionResult PostProductsBatchNew(List<Product> products)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    var defaultCategoryForVISMA = ConfigurationManager.AppSettings["DefaultCategoryForVISMA"];
                    if (string.IsNullOrEmpty(defaultCategoryForVISMA))
                    {
                        defaultCategoryForVISMA = "WebShop";
                    }

                    var category = db.Category.FirstOrDefault(a => a.Name == defaultCategoryForVISMA);

                    int categoryId = 83;
                    //if (category != null)
                    //{
                    //    categoryId = category.Id;
                    //}

                    foreach (var product in products)
                    {
                        try
                        {
                            var _product = db.Product.FirstOrDefault(u => u.Id == product.Id);
                            if (_product == null)
                            {
                                product.CategoryId = categoryId;
                                product.Active = true;
                                product.ShowItemButton = true;
                                product.DiscountAllowed = true;
                                product.Id = Guid.NewGuid();
                                db.Product.Add(product);
                            }
                            else
                            {
                                if (product.MinStockLevel != null && product.MinStockLevel > 0)
                                    _product.MinStockLevel = product.MinStockLevel;

                                _product.ExternalID = product.ExternalID;
                                _product.StockQuantity = product.StockQuantity;
                            }
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogWrite(ex);
                        }
                    }
                }
                return Ok(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return BadRequest(ex.ToString());// StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        /// <summary>
        /// Get Product by Id
        /// </summary>
        /// <param name="id">Item Id as Parameter</param>
        /// <returns>Model of ProductData that includes List of Products, Categories and Item Campaing , Accouting</returns>
        [Route("GetProductByOrderId")]
        public async Task<ProductData> GetProductByOrderId(Guid id)
        {
            try
            {


                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    string databaseName = db.Database.Connection.Database;

                    var liveProducts = await db.Product.Where(p => p.Id == id).ToListAsync();
                    var liveItemCampaigns = await db.ProductCampaign.Where(p => p.ItemId == id).ToListAsync();
                    var liveProductPrices = await db.ProductPrice.Where(p => p.ItemId == id).ToListAsync();
                    var livePricePolicy = await db.Product_PricePolicy.Where(p => p.ItemId == id).ToListAsync();

                    var liveItemCategories = new List<ItemCategory>();
                    var liveItemGroups = new List<ProductGroup>();

                    if (liveProducts.Count > 0)
                        foreach (var prod in liveProducts)
                        {
                            var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                            if (itmCats.Count > 0)
                                liveItemCategories.AddRange(itmCats);
                            var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                            if (itemGroups.Count > 0)
                                liveItemGroups.AddRange(itemGroups);


                        }
                    ProductData productData = new ProductData();
                    productData.Products = liveProducts;
                    productData.Categories = new List<Category>();
                    productData.ItemCategories = liveItemCategories;
                    productData.Accountings = new List<Accounting>();
                    productData.ProductCampaigns = liveItemCampaigns;
                    productData.ProductGroups = liveItemGroups;
                    if (liveProductPrices.Count > 0)
                        liveProductPrices = liveProductPrices.Where(p => p.ItemId == id).ToList();
                    productData.ProductPrices = liveProductPrices;
                    productData.PricePolicies = livePricePolicy;
                    return productData;
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                return productData;
            }

        }

        [HttpPost]
        [Route("PostProductStock")]
        public HttpResponseMessage PostProductStock(ProductStockHistory model)
        {
            try
            {
                #if DEBUG
                connectionString = "Data Source=DESKTOP-FFLGUA4; Initial Catalog=demoretailtestuser20230106; Integrated Security=SSPI; persist security info=True;";
                #endif

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var _product = db.Product.FirstOrDefault(u => u.Id == model.ProductId);
                    if (_product != null)
                    {
                        if (_product.Unit == ProductUnit.Piece)
                        {
                            model.ProductStock = _product.StockQuantity;
                            model.LastStock = _product.StockQuantity;
                            model.StockValue = model.NewStock;

                            _product.StockQuantity = model.NewStock;
                        }
                        else
                        {
                            model.ProductStock = (_product.Weight != null) ? _product.Weight.Value : 0;
                            model.LastStock = (_product.Weight != null) ? _product.Weight.Value : 0;
                            model.StockValue = model.NewStock;

                            _product.Weight = model.NewStock;
                        }

                        _product.Updated = DateTime.Now;

                        db.ProductStockHistory.Add(model);
                        db.Entry(_product).State = EntityState.Modified;

                        db.SaveChanges();

                        string json = JsonConvert.SerializeObject(model);
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        return httpResponseMessage;
                    } 
                    else
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Product Not Found") };
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new HttpResponseMessage(HttpStatusCode.ExpectationFailed) { Content = new StringContent(ex.ToString()) };

            }
        }

        [HttpPost]
        [Route("AddProductStock")]
        public HttpResponseMessage AddProductStock(ProductStockHistory model)
        {
            try
            {
                #if DEBUG
                connectionString = "Data Source=DESKTOP-FFLGUA4; Initial Catalog=demoretailtestuser20230106; Integrated Security=SSPI; persist security info=True;";
                #endif

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var _product = db.Product.FirstOrDefault(u => u.Id == model.ProductId);
                    if (_product != null)
                    {
                        decimal newStock = model.NewStock;

                        if (_product.Unit == ProductUnit.Piece)
                        {
                            model.ProductStock = _product.StockQuantity;
                            model.LastStock = _product.StockQuantity;
                            model.NewStock = model.LastStock + newStock;
                            model.StockValue = newStock;

                            _product.StockQuantity = model.NewStock;
                        }
                        else
                        {
                            model.ProductStock = (_product.Weight != null) ? _product.Weight.Value : 0;
                            model.LastStock = (_product.Weight != null) ? _product.Weight.Value : 0;
                            model.NewStock = model.LastStock + newStock;
                            model.StockValue = newStock;

                            _product.Weight = model.NewStock;
                        }

                        _product.Updated = DateTime.Now;

                        db.ProductStockHistory.Add(model);
                        db.Entry(_product).State = EntityState.Modified;

                        db.SaveChanges();

                        string json = JsonConvert.SerializeObject(model);
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        return httpResponseMessage;
                    }
                    else
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Product Not Found") };
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new HttpResponseMessage(HttpStatusCode.ExpectationFailed) { Content = new StringContent(ex.ToString()) };
            }
        }

        [Route("GetProductStockHistory")]
        public HttpResponseMessage GetProductStockHistory([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var _product = db.ProductStockHistory.Include(s => s.Product)
                                                        .Where(p => p.CreatedOn >= LAST_EXECUTED_DATETIME && p.CreatedOn <= EXECUTED_DATETIME && 
                                                        p.StockHistoryGroupId == null)
                                                        .OrderByDescending(obj => obj.CreatedOn);

                    var results = (from p in _product
                                   group p by p.ProductId into g
                                   select new
                                   {
                                       ProductId = g.Key,
                                       History = g.OrderByDescending(obj => obj.CreatedOn).ToList()
                                   });

                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(results)) };
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new HttpResponseMessage(HttpStatusCode.ExpectationFailed) { Content = new StringContent(ex.ToString()) };

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class BasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
        {
            public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
            {
                if (actionContext.Request.Headers.Authorization == null)
                {
                    actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }
                else
                {
                    string authToken = actionContext.Request.Headers.Authorization.Parameter;
                    string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));

                    string username = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                    string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);
                }
            }
        }
    }
}





