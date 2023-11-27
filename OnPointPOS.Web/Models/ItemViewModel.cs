using BarcodeLib;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class ItemViewModel : Product
    {
        public decimal TaxValue { get; set; }
        public string AccountingCode { get; set; }
        public string asc { get; set; }

        public decimal PriceExcludeMom { get; set; }


        public IEnumerable<SelectListItem> Printers { get; set; }
        public IEnumerable<SelectListItem> Taxes { get; set; }
        public IEnumerable<SelectListItem> UnitTypes { get; set; }
        public IEnumerable<SelectListItem> PreparationTimes { get; set; }
        public IEnumerable<SelectListItem> Accountings { get; set; }
        public IEnumerable<SelectListItem> ReceiptMethods { get; set; }

        //public IEnumerable<SelectListItem> ColorCodes { get; set; }

        //  public List<ItemCategoryViewModel> ItemCategoryList { get; set; }
        public SeededCategories SeedCategories { get; set; }
        public string UnitName
        {
            get
            { return Unit == ProductUnit.Piece ? "st" : Unit.ToString(); }
        }

        public List<ItemViewModel> Items { get; set; }

        public string SelectedIds { get; set; }
        public decimal DiscountPrice { get; set; }
        public string Edit { get; internal set; }
        public string EditCampaign { get; internal set; }
        public string NewStock { get; internal set; }
        public string Delete { get; internal set; }

        public decimal SoldQty { get; set; }
        public decimal TotalSale { get; set; }
        public decimal NetSale { get; set; }
        public decimal VatSum { get; set; }
        public string PricePerUnit { get; set; }
        public Guid GroupId { get; set; }
        public int IsItem { get; set; }

        public int Parent { get; set; }
        public decimal AvailableQty { get; set; }
        public string CategoryName { get; set; }
        public int IconId { get; set; }

        public string selectedPrimary { get; set; }

        public decimal NetSaleExlMoms { get; set; }

        public decimal MarginInSEK { get
            {
                return PriceExclMoms - PurchasePriceExcMoms;
            } 
        }

        public string ActiveStatus
        {
            get
            {
                return Active == true ? Resource.Active : Resource.Inactive;
            }
        }

        public Decimal ProfitPercentage
        {
            get
            {
                if (SoldQty > 0 && PriceExclMoms > 0)
                {
                    return Math.Round((MarginInSEK / PriceExclMoms) * 100, 2);
                    //var profitpercentageExl = (NetSaleExlMoms / (PurchasePriceExcMoms * SoldQty)) * 100;
                    ////profitpercentageExl = profitpercentageExl / (1 + (Tax / 100));
                    //return Math.Round(profitpercentageExl, 2);
                }
                else
                    return 100;
            }
        }
        public Decimal PriceExclMoms
        {
            get
            {
                if (Price > 0)
                    return Math.Round((Price - DiscountPrice) - (((Price - DiscountPrice) * Tax) / (Tax + 100)), 2);
                else
                    return 0;
            }
        }

        public Decimal PurchasePriceExcMoms
        {
            get
            {
                return PurchasePrice; // Purchase price is already mom excluded
                //if (PurchasePrice > 0)
                //    return Math.Round(((PurchasePrice) - (DiscountPrice)) / (1 + (Tax / 100)), 2);
                //else
                //    return 0;
            }
        }

        public byte[] BarCodeImage { get { return GetImage(BarCode); } }
        private byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
        private byte[] GetImage(string barCode)
        {
            try
            {
                if (string.IsNullOrEmpty(BarCode))
                    return null;


                Barcode b = new Barcode();

                int W = 280;
                int H = 75;

                b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                b.IncludeLabel = true;
                BarcodeLib.TYPE eanType = BarcodeLib.TYPE.UNSPECIFIED;
                b.EncodedType = eanType = barCode.Length == 13 ? TYPE.EAN13 : TYPE.EAN8;
                if (barCode.Length != 7 && barCode.Length != 8 && barCode.Length != 13)
                {
                    b.EncodedType = eanType = TYPE.CODE128;
                }
                b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;
                var image = b.Encode(eanType, barCode, System.Drawing.Color.Black, System.Drawing.Color.White, W, H);

                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                var photo = new MemoryStream(ms.ToArray());
                System.Drawing.Bitmap bImg = new Bitmap(photo);
                Image img = (Image)bImg;
                //System.Windows.Media.Imaging.BitmapImage bImg = new System.Windows.Media.Imaging.BitmapImage();


                return ImageToByteArray(img);



            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }



    public class AssignCategoryViewModel
    {

        public Guid ItemId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        public string asc { get; set; }
    }

    public class ItemCategoryViewModel
    {
        public Guid ItemId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public bool IsPrimary { get; set; }

        public bool Deleted { get; set; }
        public int Parant { get; set; }
        public int CategoryLevel { get; set; }

        public int SortOrder { get; set; }

        public List<ItemCategoryViewModel> Collection { get; set; }

    }

    public class ItemStockViewModel
    {
        public int Id { get; set; }
        public Guid ItemId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string BatchNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string UnitName { get; set; }
    }

    public class IngredientsViewModel : ProductIngredients
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductStockHistoryViewModel : ProductStockHistory
    {
        public string ProductName { get; set; }
        public string BarCode { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Tax { get; set; }
        public decimal Value { get; set; }
        public string CreatedDateString { get; set; }
    }

    public class ProductStockHistoryGroupViewModel : StockHistoryGroup
    {
        public string CreatedDateString { get; set; }
        public List<ProductStockHistoryViewModel> StockHistory { get; set; }
    }

    public class InventoryHistoryViewModel : InventoryHistory
    {
        public string CreatedDateString { get; set; }
    }
}