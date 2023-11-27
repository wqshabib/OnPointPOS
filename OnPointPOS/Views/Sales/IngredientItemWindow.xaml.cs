using POSSUM.Model;
using POSSUM.Presenter.Products;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for IngredientItemWindow.xaml
    /// </summary>
    /// 
    public partial class IngredientItemWindow : Window
    {
        List<OrderLine> CurrentorderDetails = new List<OrderLine>();
        List<OrderLine> IngredientsCurrentorderDetails = new List<OrderLine>();
        OrderLine selectedItem;
        public IngredientItemWindow(OrderLine selectedItem)
        {
            InitializeComponent();
            if (selectedItem.IngredientItems != null)
                IngredientsCurrentorderDetails = selectedItem.IngredientItems;
            else
                IngredientsCurrentorderDetails = new List<OrderLine>();
            this.selectedItem = selectedItem;
            LoadIngredientCategories();
            LoadItems();
        }

        #region Load Category
        int subCatPrePage = 0;
        int subCatNextPage = 6;
        int prevCatId = -1;
        int x = 5;
        int y = 6;
        int z = 7;
        List<Category> lstCategories = new List<Category>();
        private void LoadIngredientCategories()
        {
            ProductPresenter Presenter = new ProductPresenter();
            var lstCats = Presenter.GetIngredientCategories(); //Defaults.Categories;
            if (lstCats.Count == 0)
            {
                return;
            }
            lstCategories = lstCats;
            var totalCats = lstCategories.Count;

            if (totalCats > 0)
            {


                if (lstCategories.Count > z)
                {
                    subCatNextPage = y;
                    var cats = lstCategories.Skip(subCatPrePage).Take(subCatNextPage).ToList();
                    cats.Insert(y, new Category { Id = 0, Name = ">>", ColorCode = "#FFDCDEDE" });
                    AddCategoriesToGrid(cats);

                }
                else if (lstCategories.Count == z)
                    AddCategoriesToGrid(lstCategories);
                else
                    AddCategoriesToGrid(lstCategories.Skip(subCatPrePage).Take(subCatNextPage).ToList());

            }


        }
        private void AddCategoriesToGrid(List<Category> categories)
        {
            uniformGridCategories.Children.Clear();
            Style style = Application.Current.FindResource("ListButtonStyle") as Style;


            if (categories.Count < 7)
                uniformGridCategories.Columns = categories.Count;
            else
                uniformGridCategories.Columns = 7;
            foreach (var category in categories)
            {
                var btn1 = new Button();
                btn1.Style = style;
                btn1.Background = Utilities.GetColorBrush(category.ColorCode);
                btn1.Content = GetCategoryButtonContent(category);
                btn1.Width = 95;
                btn1.Height = 45;
                btn1.DataContext = category;
                uniformGridCategories.Children.Add(btn1);
                btn1.Click += ButtonCategory_Click;
                // btn1.TouchUp += ButtonCategory_Click;
            }
        }
        private StackPanel GetCategoryButtonContent(Category category)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;
            txtBlock.Text = category.Name;
            txtBlock.Width = 70;
            txtBlock.TextAlignment = TextAlignment.Center;
            txtBlock.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(txtBlock);

            return stackPanel;
        }
        private void ButtonCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                var category = (Category)(sender as Button).DataContext;
                if (category.Name == "") return;

                ItemsPageSettings();
                if (category.Name == "<<")
                {

                    if (subCatPrePage <= 0)
                    {
                        subCatPrePage = 0;
                        subCatNextPage = y;
                        LoadIngredientCategories();
                    }
                    else
                    {
                        subCatNextPage = x;
                        subCatPrePage = subCatPrePage - x;
                        if (subCatPrePage - x <= 0)
                        {
                            subCatPrePage = 0;
                            subCatNextPage = y;
                        }
                        var cats = lstCategories.Skip(subCatPrePage).Take(subCatNextPage).ToList();
                        if (subCatPrePage > x)
                        {
                            cats.Insert(0, new Category { Id = 0, Name = "<<", ColorCode = "#FFDCDEDE" });
                        }
                        if (subCatPrePage < 0)
                            subCatPrePage = 0;

                        cats.Insert(y, new Category { Id = 0, Name = ">>", ColorCode = "#FFDCDEDE" });
                        AddCategoriesToGrid(cats);
                        // LBCategory.ItemsSource = cats;
                    }
                    // LoadCategoryByBroadId(category.Id, true);
                }
                else if (category.Name == ">>")
                {
                    subCatNextPage = x;
                    subCatPrePage = subCatPrePage + x;
                    if (subCatPrePage + x >= lstCategories.Count)
                        subCatPrePage = lstCategories.Count == 0 ? 0 : lstCategories.Count - x;
                    var cats = lstCategories.Skip(subCatPrePage).Take(subCatNextPage).ToList();
                    cats.Insert(0, new Category { Id = 0, Name = "<<", ColorCode = "#FFDCDEDE" });
                    if (subCatNextPage + subCatPrePage < lstCategories.Count)
                        cats.Insert(y, new Category { Id = 0, Name = ">>", ColorCode = "#FFDCDEDE" });
                    else
                        cats.Insert(y, new Category { Id = 0, Name = "", ColorCode = "#FFDCDEDE" });

                    AddCategoriesToGrid(cats);
                    //LBCategory.ItemsSource = cats;

                }
                else
                {

                    subCatPrePage = 0;
                    subCatNextPage = y;

                    ProductPresenter presenter = new ProductPresenter();
                    var items = presenter.GetProductsByCategory(category.Id);
                    SetProductList(items);

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }
        #endregion


        int itemPrePage = 0;
        int itemNextPage = 19;
        int itemX = 18;
        int itemY = 19;
        int itemZ = 20;
        int itemLines = 4;
        private void ItemsPageSettings()
        {
            itemLines = Defaults.ItemsLines;
            if (itemLines == 5)
            {
                itemPrePage = 0;
                itemNextPage = 24;
                itemX = 23;
                itemY = 24;
                itemZ = 25;
            }
            else if (itemLines == 6)
            {
                itemPrePage = 0;
                itemNextPage = 29;
                itemX = 28;
                itemY = 29;
                itemZ = 30;
            }
            else if (itemLines == 4)
            {
                itemPrePage = 0;
                itemNextPage = 19;
                itemX = 18;
                itemY = 19;
                itemZ = 20;
            }
            else if (itemLines == 2)
            {
                itemPrePage = 0;
                itemNextPage = 9;
                itemX = 8;
                itemY = 9;
                itemZ = 10;
            }
            else
            {
                itemPrePage = 0;
                itemNextPage = 14;
                itemX = 13;
                itemY = 14;
                itemZ = 15;
            }
        }
        private void LoadItems()
        {
            ProductPresenter presenter = new ProductPresenter();
            var items = presenter.GetIngredeintProducts();
            SetProductList(items);

        }
        List<Product> lstItems = new List<Product>();
        private void SetProductList(List<Product> itmList)
        {
            lstItems = itmList;
            var items = new List<Product>();
            items = lstItems;
            var totaItem = items.Count;
            if (totaItem < itemZ)
            {
                var addmoreItems = itemZ - totaItem;
                //for (var i = 0; i < addmoreItems; i++)
                //{
                //    //    products.Add(new Product { Id = 0, Description = "", ColorCode = "#FFDCDEDE" });
                //}

            }
            if (totaItem > itemZ)
            {
                itemNextPage = itemY;
                var _items = items.Skip(itemPrePage).Take(itemNextPage).ToList();
                if (_items.Count >= itemY)
                    _items.Insert(itemY, new Product { Id = default(Guid), Description = ">>", ColorCode = "#FFDCDEDE" });
                // LBProductList.ItemsSource = _tems;
                AddItemsToGrid(_items);
            }
            else
            {
                // items.AddRange(products);
                //    LBProductList.ItemsSource = items.Skip(itemPrePage).Take(itemNextPage).ToList();
                AddItemsToGrid(items.Skip(itemPrePage).Take(itemNextPage).ToList());
            }


        }
        private void AddItemsToGrid(List<Product> products)
        {
            uniformGrid1.Children.Clear();
            Style style = Application.Current.FindResource("ListButtonStyle") as Style;

            lvCart.ItemsSource = IngredientsCurrentorderDetails;
            if (products.Count < 5)
                uniformGrid1.Columns = products.Count;
            else
                uniformGrid1.Columns = 5;
            foreach (var product in products)
            {
                var btn1 = new Button();
                btn1.Style = style;
                btn1.Background = Utilities.GetColorBrush(product.ColorCode);
                btn1.Content = GetButtonContent(product);
                btn1.Width = 133;
                btn1.Height = 60;
                btn1.DataContext = product;
                uniformGrid1.Children.Add(btn1);
                btn1.Click += ButtonItem_Click;
                //  btn1.TouchUp += ButtonItem_Click;
            }
        }

        private StackPanel GetButtonContent(Product product)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;
            if (product.PlaceHolder == false)//When Place holder true then item content will be invisible
                txtBlock.Text = product.Description;
            txtBlock.Width = 125;
            txtBlock.TextAlignment = TextAlignment.Center;
            txtBlock.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(txtBlock);

            return stackPanel;
        }
        private void ButtonItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var product = (Product)((Button)sender).DataContext;
                if (product.PlaceHolder == true) return;
                if (product.Description == "") return;
                if (product.Description == "<<")
                {

                    if (itemPrePage <= 0)
                    {
                        itemPrePage = 0;
                        itemNextPage = itemY;
                        SetProductList(lstItems);
                    }
                    else
                    {
                        itemNextPage = itemX;
                        itemPrePage = itemPrePage - itemX;
                        if (itemPrePage <= 0)
                        {
                            itemPrePage = 0;
                            itemNextPage = itemY;
                        }
                        var items = lstItems.Skip(itemPrePage).Take(itemNextPage).ToList();
                        if (itemPrePage > itemX)
                        {
                            items.Insert(0, new Product { Id = default(Guid), Description = "<<", ColorCode = "#FFDCDEDE" });
                        }
                        if (itemPrePage < 0)
                            itemPrePage = 0;
                        if (itemPrePage > itemY)
                        {
                            items.Insert(itemY, new Product { Id = default(Guid), Description = ">>", ColorCode = "#FFDCDEDE" });
                            //   LBProductList.ItemsSource = items;
                            AddItemsToGrid(items);

                        }
                        else
                        {
                            itemPrePage = 0;
                            itemNextPage = itemLines * 5;
                            SetProductList(lstItems);
                        }
                    }
                    // LoadCategoryByBroadId(category.Id, true);
                }
                else if (product.Description == ">>")
                {
                    itemNextPage = itemX;
                    itemPrePage = itemPrePage + itemX;

                    if (itemPrePage >= lstItems.Count)
                        itemPrePage = lstItems.Count == 0 ? 0 : lstItems.Count - itemX;
                    var items = lstItems.Skip(itemPrePage).Take(itemNextPage).ToList();
                    items.Insert(0, new Product { Id = default(Guid), Description = "<<", ColorCode = "#FFDCDEDE" });
                    if (itemNextPage + itemPrePage < lstItems.Count)
                        items.Insert(items.Count, new Product { Id = default(Guid), Description = ">>", ColorCode = "#FFDCDEDE" });
                    else
                        if (items.Count > itemY)
                        items.Insert(itemY, new Product { Id = default(Guid), Description = "", ColorCode = "#FFDCDEDE" });
                    //  LBProductList.ItemsSource = items;
                    AddItemsToGrid(items);
                    // SetProductListBox(items,false);

                }
                else
                {
                    //AddToCart(product, 1);
                    AddToCart2(product, 1); 
                    //lvCart.ItemsSource = CurrentorderDetails;
                    
                    lvCart.ItemsSource = IngredientsCurrentorderDetails;
                    lvCart.Items.Refresh();
                }

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }
        private void AddToCart(Product product, int qty)
        {
            try
            {
                int itemIdex = 0;
                if (CurrentorderDetails.Count > 0)
                    itemIdex = CurrentorderDetails.Max(od => od.ItemIdex);

                var data = CurrentorderDetails.Where(p => p.ItemId == product.Id && p.UnitPrice == product.Price);

                //TODO:FIX
                if (data.Count() > 0 && product.AskPrice == false && product.AskWeight == false)
                {
                    var orderDetail = CurrentorderDetails.Single(p => p.ItemId == product.Id);
                    decimal quantity = orderDetail.Quantity;
                    quantity = quantity + qty;

                    var grossTotal = (quantity * orderDetail.UnitPrice);
                    if (orderDetail.DiscountPercentage > 0)
                    {
                        decimal itemdiscount = (grossTotal / 100) * orderDetail.DiscountPercentage;
                        itemdiscount = quantity * itemdiscount;
                        orderDetail.ItemDiscount = itemdiscount;
                        orderDetail.DiscountDescription = UI.Sales_Discount;

                    }
                    orderDetail.Quantity = quantity;

                }
                else
                {
                    var orderItem = new OrderLine
                    {

                        ItemIdex = itemIdex + 1,
                        TaxPercent = product.Tax,
                        Quantity = qty,
                        UnitPrice = product.Price,
                        DiscountedUnitPrice = (product.Price),
                        PurchasePrice = product.PurchasePrice,
                        Active = 1,
                        Direction = 1,
                        PrinterId = product.PrinterId,
                        Percentage = product.Tax,
                        ItemId = product.Id,
                        Product = product,
                        ItemType = Model.ItemType.Ingredient,
                        GroupId = selectedItem.ItemId,
                        IngredientMode = "+"
                    };


                    CurrentorderDetails.Add(orderItem);
                    if (IngredientsCurrentorderDetails != null)
                        IngredientsCurrentorderDetails.Add(orderItem);
                    else
                        IngredientsCurrentorderDetails = new List<OrderLine>();

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void AddToCart2(Product product, int qty) 
        {
            try
            {
                int itemIdex = 0;
                if (IngredientsCurrentorderDetails.Count > 0)
                    itemIdex = IngredientsCurrentorderDetails.Max(od => od.ItemIdex);

                var data = IngredientsCurrentorderDetails.Where(p => p.ItemId == product.Id && p.UnitPrice == product.Price);

                //TODO:FIX
                if (data.Count() > 0 && product.AskPrice == false && product.AskWeight == false)
                {
                    var orderDetail = IngredientsCurrentorderDetails.Single(p => p.ItemId == product.Id);
                    decimal quantity = orderDetail.Quantity;
                    quantity = quantity + qty;

                    var grossTotal = (quantity * orderDetail.UnitPrice);
                    if (orderDetail.DiscountPercentage > 0)
                    {
                        decimal itemdiscount = (grossTotal / 100) * orderDetail.DiscountPercentage;
                        itemdiscount = quantity * itemdiscount;
                        orderDetail.ItemDiscount = itemdiscount;
                        orderDetail.DiscountDescription = UI.Sales_Discount;

                    }
                    orderDetail.Quantity = quantity;

                }
                else
                {
                    var orderItem = new OrderLine
                    {

                        ItemIdex = itemIdex + 1,
                        TaxPercent = product.Tax,
                        Quantity = qty,
                        UnitPrice = product.Price,
                        DiscountedUnitPrice = (product.Price),
                        PurchasePrice = product.PurchasePrice,
                        Active = 1,
                        Direction = 1,
                        PrinterId = product.PrinterId,
                        Percentage = product.Tax,
                        ItemId = product.Id,
                        Product = product,
                        ItemType = Model.ItemType.Ingredient,
                        GroupId = selectedItem.ItemId,
                        IngredientMode = "+"
                    };


                    CurrentorderDetails.Add(orderItem);
                    if (IngredientsCurrentorderDetails != null)
                        IngredientsCurrentorderDetails.Add(orderItem);
                    else
                        IngredientsCurrentorderDetails = new List<OrderLine>();

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        private void add_Click(object sender, RoutedEventArgs e)
        {
            var seletedItem = (OrderLine)(sender as Button).DataContext;
            if (seletedItem != null)
            {
                var item = IngredientsCurrentorderDetails.FirstOrDefault(cd => cd.ItemId == seletedItem.ItemId);
                if (item != null)
                {
                    item.Quantity = item.Quantity + 1;
                }
                lvCart.Items.Refresh();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var _seletedItem = (OrderLine)(sender as Button).DataContext;
            if (_seletedItem != null)
            {
                var itm2= IngredientsCurrentorderDetails.FirstOrDefault(cd => cd.ItemId == _seletedItem.ItemId);
                var item = CurrentorderDetails.FirstOrDefault(cd => cd.ItemId == _seletedItem.ItemId);
                //if (item != null)
                //{
                //    item.Quantity = item.Quantity - 1;
                //    if (item.Quantity == 0)
                //    {
                //        CurrentorderDetails.Remove(item);
                //    }

                //}
                if (itm2 != null)
                {
                    itm2.Quantity = itm2.Quantity - 1;
                    if (itm2.Quantity == 0)
                    {
                        IngredientsCurrentorderDetails.Remove(itm2);
                    }

                }
            }
            lvCart.Items.Refresh();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var _seletedItem = (OrderLine)(sender as Button).DataContext;

            if (_seletedItem != null)
            {
                var item = CurrentorderDetails.FirstOrDefault(cd => cd.ItemId == _seletedItem.ItemId);
                if (item != null)
                {
                    CurrentorderDetails.Remove(item);
                }
            }
            lvCart.Items.Refresh();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedItem.IngredientItems == null)
                {
                    selectedItem.IngredientItems = CurrentorderDetails.OrderBy(o => o.IngredientMode).ToList();
                    IngredientsCurrentorderDetails = CurrentorderDetails.OrderBy(o => o.IngredientMode).ToList();
                }
                else
                {
                    //selectedItem.IngredientItems.AddRange(CurrentorderDetails.OrderBy(o => o.IngredientMode).ToList());
                    //IngredientsCurrentorderDetails.AddRange(CurrentorderDetails.OrderBy(o => o.IngredientMode).ToList());

                }
                this.DialogResult = true;
                //IngredientsCurrentorderDetails = CurrentorderDetails;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void Tuggle_Click(object sender, RoutedEventArgs e)
        {

            var _seletedItem = (OrderLine)(sender as Button).DataContext;

            if (_seletedItem != null)
            {
                if (_seletedItem.IngredientMode == "+")
                {
                    _seletedItem.IngredientMode = "-";

                }
                else
                {
                    _seletedItem.IngredientMode = "+";

                }

                lvCart.Items.Refresh();
            }
        }
    }
}
