using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POSSUM.Model;
using POSSUM.Utility;
using POSSUM.Utils;
using System.Data.Entity;
using POSSUM.Data;

namespace POSSUM.Handlers
{
    public class ProductHandler
    {
        public void SyncSeamlessProducts()
        {


            var uof = new UnitOfWork(new ApplicationDbContext());


            var service = new eProducts();

            service.Connect(Defaults.ClientId, Defaults.ClientPassword);

            int result = 0;

            var categories = service.GetData(out result);



            //  var helper2 = new NHibernateHelper(client.connectionString);

            //  var conn = new UnitOfWork(helper2.SessionFactory);

            var productRepo = uof.ProductRepository;
            var categoryRepo = uof.CategoryRepository;
            var itmcatRepo = uof.ItemCategoryRepository;

            int parent = categoryRepo.First(c => c.Parant == 0).Id;

            int before = productRepo.GetAll().Count();

            int lastId = categoryRepo.AsQueryable().Max(c => c.Id);

            var seamlessCategory = categoryRepo.FirstOrDefault(c => c.Name == "Seamless") ?? new Category { Id = lastId + 1, Name = "Seamless", Parant = parent, CategoryLevel = 2 };

            seamlessCategory.CategoryLevel = 2;
            seamlessCategory.Parant = parent;


            categoryRepo.AddOrUpdate(seamlessCategory);
            var seamCategories = categories as IList<SeamCategory> ?? categories.ToList();
            foreach (var item in seamCategories)
            {
                lastId = categoryRepo.AsQueryable().Max(c => c.Id);

                var cat = categoryRepo.FirstOrDefault(c => c.Name == item.Name) ?? new Category { Id = lastId + 1, Parant = parent, CategoryLevel = 1 };

                Trace.WriteLine("Saving " + item.Name);

                cat.Parant = seamlessCategory.Id;
                cat.Name = item.Name;
                cat.CategoryLevel = seamlessCategory.CategoryLevel + 1;

                categoryRepo.AddOrUpdate(cat);

                Trace.WriteLine("Saved " + item.Name);

                foreach (var prod in item.Products)
                {

                    Trace.WriteLine("Saving " + prod.Description);

                  

                    var seamlessProduct = (productRepo.FirstOrDefault(p => p.SKU == prod.SKU) ?? new Product { Id = Guid.NewGuid(), Description = prod.Description });
                    seamlessProduct.SKU = prod.SKU;
                    seamlessProduct.Seamless = true;
                    seamlessProduct.Price = prod.Price;
                    seamlessProduct.Active = true;
                    seamlessProduct.Updated = DateTime.Now;
                    productRepo.AddOrUpdate(seamlessProduct);
                    var itemcategory = itmcatRepo.FirstOrDefault(p => p.ItemId == prod.Id && p.CategoryId == cat.Id) ?? new ItemCategory() { ItemId = seamlessProduct.Id, CategoryId = cat.Id };
                    itmcatRepo.AddOrUpdate(itemcategory);
                    Trace.WriteLine("Finished " + prod.Description);

                }
            }

            uof.Commit();

            int expected = seamCategories.Sum(c => c.Products.Count);

            service.Close();


        }

        public Product SaveProduct(Product product, int categoryId)
        {

            using (var uof = new UnitOfWork(new ApplicationDbContext()))
            {
                var productRepo = uof.ProductRepository;
              
                var item = new Product
                {
                    Id = Guid.NewGuid(),
                    Description = product.Description,
                    BrandNumber = product.BrandNumber,
                    Price = product.Price,
                    PurchasePrice = product.Price,
                    Tax = product.Tax,
                    BrandId = product.BrandId,
                    BarCode = product.BarCode,
                    PLU = product.PLU,
                    Unit = product.Unit,
                    AskWeight = product.AskWeight,
                    AskPrice = product.AskPrice,
                    PriceLock = product.PriceLock,
                    ColorCode = product.ColorCode,
                    PrinterId = product.PrinterId,
                    SortOrder = product.SortOrder,
                    Active = true,
                    Deleted = false,
                    SKU = product.PLU,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    PlaceHolder = product.PlaceHolder,
                    Seamless = false,
                    Sticky = product.Sticky,
                    Bong = product.Bong,
                    ShowItemButton = product.ShowItemButton,
                    AccountingId = product.AccountingId
                };
                productRepo.Add(item);
                int lastSortOrder = 0;
                try
                {
                    lastSortOrder = uof.ItemCategoryRepository.AsQueryable().Where(c => c.CategoryId == categoryId).Max(m => m.SortOrder);
                    lastSortOrder = lastSortOrder + 1;
                }
                catch
                {
                }

                var itemCategory = new ItemCategory
                {
                    ItemId = item.Id,
                    CategoryId = categoryId,
                    SortOrder = lastSortOrder,
                    IsPrimary=true
                };

                uof.ItemCategoryRepository.Add(itemCategory);
                uof.Commit();
                item.ItemCategory = new List<ItemCategory>();
                item.ItemCategory.Add(itemCategory);
                return item;
            }

        }


    }
}
