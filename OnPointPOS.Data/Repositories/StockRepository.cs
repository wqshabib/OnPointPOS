using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class StockRepository
    {
        public bool SaveStock(ItemInventory inventory)
        {
            using (var db = new ApplicationDbContext())
            {
                var _inventory = db.ItemInventory.FirstOrDefault(c => c.ItemId == inventory.ItemId && c.WarehouseID == inventory.WarehouseID && c.WarehouseLocationID == inventory.WarehouseLocationID);
                if (_inventory != null)
                {
                    _inventory.StockCount = inventory.StockCount;
                    _inventory.StockReservations = inventory.StockReservations;
                    db.Entry(_inventory).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    inventory.ItemInventoryID = Guid.NewGuid();
                    db.ItemInventory.Add(inventory);
                }
                db.SaveChanges();
                return true;
            }
        }


        public void UpdateStock(string filePath = @"D:\Luqon-IT\POSSUM\test.csv")
        {
            try
            {
                var alltext = File.ReadAllText(filePath);
                List<string> lstItems = new List<string>(alltext
                     .Replace("\n", "").Replace(";;   ", "")
                     .Replace(";;", "")
                     .Replace("\0", "")
                     .Replace("�", "")
                     .Replace("Totalt", ";Totalt")
                     .Split(new string[] { "PLU: " }, StringSplitOptions.None));
                lstItems.RemoveAt(0);
                using (var db = new ApplicationDbContext())
                {
                    foreach (var item in lstItems)
                    {
                        try
                        {
                            var stock = "";
                            var weight = "";
                            var itemLst = item.Split(';').ToList();
                            foreach (var lst in itemLst)
                            {
                                if (lst.Contains("Styck"))
                                {
                                    stock = lst.Split(':')[1];
                                }
                                if (lst.Contains("Vikt"))
                                {
                                    weight = lst.Split(':')[1];
                                    weight = !string.IsNullOrEmpty(weight) ? weight.Remove(weight.LastIndexOf(" k")).Replace(" ", "") : "";
                                }

                            }

                            //var stock = itemLst[2].Split(':');
                            var plu = itemLst[0];
                            var product = db.Product.Where(p => p.PLU == plu).FirstOrDefault();
                            if (product != null)
                            {
                                product.StockQuantity = product.StockQuantity + Convert.ToDecimal(stock);
                                POSSUMDataLog.WriteLog(weight + stock);
                                product.Weight = product.Weight + (!string.IsNullOrEmpty(weight) ? Math.Round(Convert.ToDecimal(weight, CultureInfo.CurrentCulture), 3) : 0);
                                db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception e)
                        {
                            POSSUMDataLog.LogException(e);
                            POSSUMDataLog.WriteLog(e.ToString());

                        }


                    }
                }

            }
            catch (Exception e)
            {

            }


        }









    }
}
