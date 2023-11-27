using Newtonsoft.Json;
using POSSUM.Data;
using POSSUM.Handlers;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Presenter.Products;
using POSSUM.Presenters.Sales;
using POSSUM.Res;
using POSSUM.Utils;
using POSSUM.Views.Customers;
using POSSUM.Views.FoodTables;
using POSSUM.Views.OpenOrder;
using POSSUM.Views.PrintOrder;
using POSSUM.Views.SplitOrder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POSSUM.Views.Sales
{
    /// <summary>
    /// Interaction logic for UCSale.xaml
    /// </summary>
    public partial class UCSale : UserControl, ISaleOrderView
    {
        #region Public Properties /Variables
        public SaleOrderPresenter Presenter { get; set; }
        ProductPresenter productPresenter;
        #endregion
        #region Private Properties /Variables       
        private EntryModeType EntryMode { get; set; }
        //MOVE TO Default.Init
        List<Product> lstProducts = new List<Product>();
        List<int> parentCategoryIds = new List<int>();
        // How we can remove  or re-adjust
        private bool IsFromHold = false;
        // How we can remove or re-adjust
        private int addCartIndex = -1;
        public int selectedCategoryId = 0;
        public string selectedCategory;
        private decimal currentQty = 1;
        private bool AskByUser = false;
        Category currentCategory;
        Category currentCategory2;
        Product selectedProduct;
        OrderLine selectedItem;
        //  List<Product> lstItems = new List<Product>();
        #endregion
        UCItemCart ucCart;

        public UCSale()
        {
            InitializeComponent();
            CategoryPageSettings();
            ItemsPageSettings();
            Presenter = new SaleOrderPresenter(this);
            productPresenter = new ProductPresenter();
            AddItemCart();
            LoadCategoryByBroadId();
            Presenter.MasterOrder = new Order();
            Presenter.CurrentorderDetails = new List<OrderLine>();
            NewEntry();
            SaleView();

            CGSetting();
            UpdateButtonsVisibility();

            DataObject.AddPastingHandler(txtNumber, OnPaste);



        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            try
            {
                LogWriter.LogWrite("OnPaste : Barcode add Product start => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                if (ConfigurationManager.AppSettings["HandlePaste"] == "1")
                {
                    this.Dispatcher.BeginInvoke(new Action(async () => {
                        var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
                        if (!isText) return;

                        var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
                        txtNumber.Text = text;
                        e.Handled = false;
                        await Task.Delay(100);
                        if (text.Length > 20)
                        {
                            decimal discousnt = Presenter.GetCardDiscount();
                            if (discousnt > 0)
                            {
                                txtNumber.Text = "";
                                txtNumber.Focus();
                                return;
                            }
                        }
                        lblPriceMessage.Text = "";
                        if (EntryMode == EntryModeType.CodeEntry)
                            BarCodeEntry(text);
                        else if (selectedProduct != null)
                            btnEnter_Click(sender, null);

                        txtNumber.Text = "";
                    }));
                }

            }
            catch (Exception ex)
            {

                LogWriter.LogWrite("OnPaste : Exception => " + ex.ToString());
            }
            finally
            {
                LogWriter.LogWrite("OnPaste : Barcode add Product end => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            }

        }

        private void UpdateButtonsVisibility_Old()
        {
            if (Defaults.DisableCreditCard)
            {
                DirectCreditCardButton.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(DirectCashButton, 2);
            }
            else
            {
                DirectCreditCardButton.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(DirectCashButton, 1);
            }

            if (Defaults.DisableCashButton)
            {
                DirectCashButton.Visibility = Visibility.Collapsed;
                Grid.SetColumn(DirectCreditCardButton, 0);
                Grid.SetColumnSpan(DirectCreditCardButton, 2);
            }
            else
            {
                DirectCashButton.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(DirectCreditCardButton, 1);
            }
        }

        private void UpdateButtonsVisibility()
        {
            DirectCreditCardButton.Visibility = Defaults.DisableCreditCard ? Visibility.Collapsed : Visibility.Visible;
            DirectCashButton.Visibility = Defaults.DisableCashButton ? Visibility.Collapsed : Visibility.Visible;
            DirectSwishButton.Visibility = Defaults.DisableSwishButton ? Visibility.Collapsed : Visibility.Visible;

            if (!Defaults.DisableCreditCard && !Defaults.DisableCashButton && !Defaults.DisableSwishButton) {
                DirectSwishButton.Visibility = Visibility.Collapsed;
            }

            if (!Defaults.DisableCreditCard && Defaults.DisableCashButton && Defaults.DisableSwishButton)
            {
                Grid.SetColumn(DirectCreditCardButton, 0);
                Grid.SetColumnSpan(DirectCreditCardButton, 2);
            }
            
            if (Defaults.DisableCreditCard && !Defaults.DisableCashButton && Defaults.DisableSwishButton)
            {
                Grid.SetColumn(DirectCashButton, 0);
                Grid.SetColumnSpan(DirectCashButton, 2);
            }

            if (Defaults.DisableCreditCard && Defaults.DisableCashButton && !Defaults.DisableSwishButton)
            {
                Grid.SetColumn(DirectSwishButton, 0);
                Grid.SetColumnSpan(DirectSwishButton, 2);
            }
        }

        public UCSale(Guid orderId)
        {
            InitializeComponent();
            CategoryPageSettings();
            ItemsPageSettings();
            Presenter = new SaleOrderPresenter(this, orderId);
            productPresenter = new ProductPresenter();
            AddItemCart();
            LoadCategoryByBroadId();
            DisplayOrder(orderId);
            SaleView();
            CGSetting();
            UpdateButtonsVisibility();
        }
        private void SaleView()
        {
            if (Defaults.SaleType == SaleType.Restaurant)
            {
                btnTakeAway.Visibility = Visibility.Visible;
                //btnSelectTable.Visibility = Visibility.Visible;
                BongButton.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;
                btnNew.Width = 255;
                btnSearchItem.Width = 126;
                if ( ConfigurationManager.AppSettings["RestaurantGridShow"] == "0")
                { 
                    orderHistoryGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                btnTakeAway.Visibility = Visibility.Collapsed;
                //btnSelectTable.Visibility = Visibility.Collapsed;
                BongButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Visible;
                btnNew.Width = 255;
                btnSearchItem.Width = 255;

            }
        }
        private Grid TableButtonContent(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = UI.Sales_OpenOrderButton;
            string imageUrl = @"/POSSUM;component/images/Table.png";

            Grid grid = new Grid();
            grid.Width = 125;
            var cldef = new ColumnDefinition();
            cldef.Width = new GridLength(25);
            grid.ColumnDefinitions.Add(cldef);
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            Image img = new Image();
            img.Source = new BitmapImage(new Uri(imageUrl, UriKind.Relative));
            img.Height = 24;
            img.Width = 24;
            img.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(img);

            TextBlock txtBlock = new TextBlock();
            txtBlock.Text = name;
            txtBlock.Margin = new Thickness(10, 0, 0, 0);
            txtBlock.VerticalAlignment = VerticalAlignment.Center;
            txtBlock.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetColumn(txtBlock, 1);
            grid.Children.Add(txtBlock);

            return grid;// (ControlTemplate)XamlReader.Parse(template);

        }
        private void CGSetting()
        {
            if (Defaults.CASH_GUARD)
            {
                DirectCashButton.IsEnabled = false;
                App.MainWindow.InsertedAmount += MainWindow_InsertedAmount;
            }
        }
        private void MainWindow_InsertedAmount(object sender, int amout)
        {
            if ((amout >= Presenter.MasterOrder.OrderTotal && amout > 0) || Presenter.MasterOrder.Status == OrderStatus.ReturnOrder)
                DirectCashButton.Dispatcher.BeginInvoke(new Action(() => DirectCashButton.IsEnabled = true));
            else
                DirectCashButton.Dispatcher.BeginInvoke(new Action(() => DirectCashButton.IsEnabled = false));

        }


        private void AddItemCart()
        {
            ucCart = new UCItemCart(Presenter);
            ucCart.HorizontalAlignment = HorizontalAlignment.Stretch;
            ucCart.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ucCart.Height = CartContainer.Height;
            CartContainer.Children.Add(ucCart);
        }

        #region Load Category
        int subCatPrePage = 0;
        int subCatNextPage = 5;

        int x = 3;
        int y = 4;
        int z = 5;
        int k = 6; // Added new variable for 4 rows


        private void CategoryPageSettings()
        {
            subCatPrePage = 0;
            if (Defaults.CategoryLines == 2)
            {
                subCatNextPage = 9;
                x = 8;
                y = 9;
                z = 10;
                k = 11;
            }
            else if (Defaults.CategoryLines == 3)
            {
                subCatPrePage = 0;
                subCatNextPage = 13;

                x = 12;
                y = 13;
                z = 14;
                k = 15;
            }
            else if (Defaults.CategoryLines == 4) /*added new for 4 rows*/
            {
                subCatPrePage = 0;
                subCatNextPage = 17;

                x = 15;
                y = 16;
                z = 17;
                k = 18;

            }

            else
            {
                subCatPrePage = 0;
                subCatNextPage = 6;

                x = 5;
                y = 6;
                z = 7;
                k = 8;
            }
        }

        List<Category> lstCategories = new List<Category>();

        private void LoadCategoryByBroadId()
        {
            var lstCats = Presenter.LoadCategories(1);// Defaults.Categories;
            lstCategories = lstCats;
            if (lstCats.Count == 0)
            {
                Presenter.LoadItemByCategory(Defaults.RootCategoryId);
                return;
            }
            lstCategories = lstCats;
            var totalCats = lstCategories.Count;


            if (totalCats > 0)
            {

                if (totalCats < z)
                {
                    var addmoreCats = z - totalCats;
                    for (var i = 0; i < addmoreCats; i++)
                    {
                        lstCategories.Add(new Category
                        {
                            Id = 0,
                            Name = "",
                            ColorCode = "#FFDCDEDE"
                        });
                    }

                    AddCategoriesToGrid(lstCategories.Skip(subCatPrePage).Take(z).ToList());

                    return;
                }
                else if (totalCats < k)
                {
                    var addmoreCats = k - totalCats;
                    for (var i = 0; i < addmoreCats; i++)
                    {
                        lstCategories.Add(new Category
                        {
                            Id = 0,
                            Name = "",
                            ColorCode = "#FFDCDEDE"
                        });
                    }

                    AddCategoriesToGrid(lstCategories.Skip(subCatPrePage).Take(k).ToList());

                    return;
                }
                if (lstCategories.Count > k)
                {
                    subCatNextPage = y;
                    var cats = lstCategories.Skip(subCatPrePage).Take(subCatNextPage).ToList();
                    cats.Insert(y, new Category { Id = 0, Name = ">>", ColorCode = "#FFDCDEDE" });
                    AddCategoriesToGrid(cats);
                    //  LBCategory.ItemsSource = cats;
                }
                else if (lstCategories.Count == k)
                    AddCategoriesToGrid(lstCategories);
                else
                    AddCategoriesToGrid(lstCategories.Skip(subCatPrePage).Take(subCatNextPage).ToList());
                // LBCategory.ItemsSource = lstCategories.Skip(subCatPrePage).Take(subCatNextPage).ToList();
            }

        }
        private void AddCategoriesToGrid(List<Category> categories)
        {
            uniformGridCategories.Children.Clear();
            Style style = Application.Current.FindResource("ListButtonStyle") as Style;
            Style dummyStyle = Application.Current.FindResource("DummyButton") as Style;
            var categoryBold = Defaults.SettingsList[SettingCode.CategoryBold];


            if (categories.Count < 5)
                uniformGridCategories.Columns = categories.Count;
            else if (categories.Count == 7)
                uniformGridCategories.Columns = 5;
            else
                uniformGridCategories.Columns = 5;

            foreach (var category in categories)
            {
                var btn1 = new Button();

                btn1.Content = GetCategoryButtonContent(category);
                btn1.Background = Utilities.GetColorBrush("#FFF2F2F2");
                btn1.Style = style;

                btn1.Width = 130;
                btn1.Height = 55;
                btn1.Margin = new Thickness(4, 4, 4, 4);
                btn1.DataContext = category;
                if (Defaults.CategoryBold)
                    btn1.FontWeight = FontWeights.UltraBold;
                uniformGridCategories.Children.Add(btn1);
                btn1.Click += ButtonCategory_Click;
            }
        }

        private Grid GetCategoryButtonContent(Category category)
        {
            Grid grid = new Grid();
            grid.Height = 55;
            grid.Width = 130;
            Border border = new Border();
            border.Background = Utilities.GetColorBrush(category.ColorCode);
            border.VerticalAlignment = VerticalAlignment.Top;
            border.HorizontalAlignment = HorizontalAlignment.Stretch;
            border.Height = 8;
            border.CornerRadius = new CornerRadius(5, 5, 0, 0);
            grid.Children.Add(border);
            StackPanel stkPanel = new StackPanel();
            stkPanel.Orientation = Orientation.Vertical;
            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;
            txtBlock.Text = category.Name;
            txtBlock.Width = 70;

            txtBlock.TextAlignment = TextAlignment.Center;
            txtBlock.VerticalAlignment = VerticalAlignment.Center;
            stkPanel.Children.Add(txtBlock);


            stkPanel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(stkPanel, 1);
            grid.Children.Add(stkPanel);

            return grid;
        }

        private void ButtonCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                var category = (Category)(sender as Button).DataContext;
                if (category.Name == "") return;
                currentSubCategory = -1;
                SubCategoryClick = false;
                ItemsPageSettings();
                if (category.Name == "<<")
                {

                    if (subCatPrePage <= 0)
                    {
                        subCatPrePage = 0;
                        subCatNextPage = y;
                        LoadCategoryByBroadId();
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
                    currentCategory = category;
                    subCatPrePage = 0;
                    subCatNextPage = y;
                    selectedCategory = category.Name;
                    selectedCategoryId = category.Id;
                    LoadSubCategory(category.Id);


                    // LoadCategoryByBroadId(category.Id, false);

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }


        private void LoadSubCategory(int categoryId)
        {

            var parentCategory = Defaults.Categories.First(c => c.Id == categoryId);
            lstSubCategory = parentCategory.SubCategories;
            if (lstSubCategory.Count == 0)
            {
                Presenter.LoadItemByCategory(categoryId);

                return;
            }
            else
            {
                if (false)
                {
                    lstProducts = lstSubCategory.Select(cat => new Product
                    {
                        Id = default(Guid),
                        CategoryId = cat.Id,
                        Description = cat.Name,
                        Type = 1,
                        IconId = cat.IconId ?? 0,
                        ImagePath = "/POSSUM;component/images/category.png",
                        ImageVisibility = true,
                        ColorCode = cat.ColorCode
                    }).ToList();

                    var items = new List<Product>();
                    items = lstProducts;
                    itemZ = 5;
                    itemY = 4;
                    itemX = 3;
                    int totaItem = items.Count;
                    if (totaItem > itemZ)
                    {
                        itemNextPage = itemY;
                        var _items = items.Skip(itemPrePage).Take(itemNextPage).ToList();
                        if (_items.Count >= itemY)
                            _items.Insert(itemY, new Product { Id = default(Guid), Description = ">>", ColorCode = "#FFDCDEDE" });
                        // LBProductList.ItemsSource = _tems;
                        AddItemsToGrid(_items);
                    }
                    else if (totaItem == itemZ)
                        AddItemsToGrid(items);
                    else
                    {
                        // items.AddRange(products);
                        //    LBProductList.ItemsSource = items.Skip(itemPrePage).Take(itemNextPage).ToList();
                        AddItemsToGrid(items.Skip(itemPrePage).Take(itemNextPage).ToList());
                    }
                }
                else
                {
                    ItemsPageSettings();
                    Presenter.LoadItemByCategory(categoryId);
                }
            }
        }
        private bool IsSubCategory = false;
        private void LoadSubCategory2(int categoryId)
        {

            var parentCategory = lstSubCategory.First(c => c.Id == categoryId);
            var lstCats = Presenter.LoadCategories2(categoryId);// Defaults.Categories;
            lstSubCategory = lstCats;

            if (lstCats.Count == 0)
            {
                IsSubCategory = false;
                Presenter.LoadItemByCategory(categoryId);

                return;
            }
            else
            {
                if (false)
                {
                    lstProducts = lstSubCategory.Select(cat => new Product
                    {
                        Id = default(Guid),
                        CategoryId = cat.Id,
                        Description = cat.Name,
                        Type = 1,
                        IconId = cat.IconId ?? 0,
                        ImagePath = "/POSSUM;component/images/category.png",
                        ImageVisibility = true,
                        ColorCode = cat.ColorCode
                    }).ToList();

                    var items = new List<Product>();
                    items = lstProducts;
                    itemZ = 5;
                    itemY = 4;
                    itemX = 3;
                    int totaItem = items.Count;
                    if (totaItem > itemZ)
                    {
                        itemNextPage = itemY;
                        var _items = items.Skip(itemPrePage).Take(itemNextPage).ToList();
                        if (_items.Count >= itemY)
                            _items.Insert(itemY, new Product { Id = default(Guid), Description = ">>", ColorCode = "#FFDCDEDE" });
                        // LBProductList.ItemsSource = _tems;
                        AddItemsToGrid(_items);
                    }
                    else if (totaItem == itemZ)
                        AddItemsToGrid(items);
                    else
                    {
                        // items.AddRange(products);
                        //    LBProductList.ItemsSource = items.Skip(itemPrePage).Take(itemNextPage).ToList();
                        AddItemsToGrid(items.Skip(itemPrePage).Take(itemNextPage).ToList());
                    }
                }
                else
                {
                    IsSubCategory = true;
                    ItemsPageSettings();
                    Presenter.LoadItemByCategory(categoryId);
                }
            }
        }
        #endregion

        #region Load Items

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

        public void SetProductListBox(List<Product> itemsList, bool isButton = false)
        {

            lstProducts = itemsList;
            var products = new List<Product>();
            products = itemsList;
            var items = new List<Product>();

            if (SubCategoryClick == false || lstSubCategory.Count <= 5)
                foreach (var cat in lstSubCategory)
                {
                    if (string.IsNullOrEmpty(selectedCategory))
                        items.Add(new Product { Id = default(Guid), CategoryId = cat.Id, Description = cat.Name, Type = 1, IconId = cat.IconId ?? 0, ImagePath = "/POSSUM;component/images/category.png", ImageVisibility = true, ColorCode = cat.ColorCode });
                    else
                    {

                        if (selectedCategory.Contains("Seamless"))
                            items.Add(new Product { Id = default(Guid), CategoryId = cat.Id, Description = cat.Name, Type = 1, IconId = cat.IconId ?? 0, ImagePath = "/POSSUM;component/images/seamless.png", ImageVisibility = true, ColorCode = cat.ColorCode });
                        else
                            items.Add(new Product { Id = default(Guid), CategoryId = cat.Id, Description = cat.Name, Type = 1, IconId = cat.IconId ?? 0, ImagePath = "/POSSUM;component/images/category.png", ImageVisibility = true, ColorCode = cat.ColorCode });
                    }
                }
            if (SubCategoryClick && lstSubCategory.Count > 5)
                items.Add(new Product { Id = default(Guid), Description = "<<" });
            items.AddRange(products);
            var totaItem = items.Count;
            if (totaItem < itemZ)
            {
                var addmoreItems = itemZ - totaItem;
                for (var i = 0; i < addmoreItems; i++)
                {
                    products.Add(new Product { Id = default(Guid), Description = "", ColorCode = "#FFDCDEDE" });
                }

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
                if(items.Count==20)
                    AddItemsToGrid(items);
                else
                AddItemsToGrid(items.Skip(itemPrePage).Take(itemNextPage).ToList());
            }
            // LBProductList.Items.Refresh();
        }

        private void AddItemsToGrid(List<Product> products)
        {
            uniformGrid1.Children.Clear();
            Style style = Application.Current.FindResource("ListButtonStyle") as Style;
            //  Style style = Application.Current.FindResource("FlatImageButton") as Style;
            var productBold = Defaults.SettingsList[SettingCode.ProductBold];
            if (productBold == "1")

                if (products.Count < 5)
                    uniformGrid1.Columns = products.Count;
                else
                    uniformGrid1.Columns = 5;
            foreach (var product in products)
            {
                var btn1 = new Button();
                btn1.Style = style;
                btn1.Background = Utilities.GetColorBrush(product.ColorCode);
                btn1.Content = GetButtonContent(product);//GetImageButtonContent(product);//
                btn1.Width = 132;
                btn1.Height = 60;
                btn1.Margin = new Thickness(1, 1, 1, 1);
                if (Defaults.ProductBold)
                    btn1.FontWeight = FontWeights.UltraBold;
                btn1.DataContext = product;
                uniformGrid1.Children.Add(btn1);
                btn1.Click += ButtonItem_Click;
            }
        }

        private StackPanel GetButtonContent(Product product)
        {
            if (product.Type == 1)
                return GetImageButtonContent(product);
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;
            txtBlock.Text = product.Description;
            txtBlock.Width = 125;
            txtBlock.TextAlignment = TextAlignment.Center;
            txtBlock.VerticalAlignment = VerticalAlignment.Center;

            stackPanel.Children.Add(txtBlock);
            if (Defaults.ShowPrice && product.AskPrice == false)
            {
                TextBlock txtPriceBlock = new TextBlock();

                txtPriceBlock.Text = product.Price.ToString("C", Defaults.UICultureInfo);

                txtPriceBlock.TextAlignment = TextAlignment.Center;
                txtPriceBlock.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.Children.Add(txtPriceBlock);
            }
            return stackPanel;
        }

        private StackPanel GetImageButtonContent(Product item)
        {
            Image img = new Image();

            img.Source = new BitmapImage(new Uri(item.ImagePath, UriKind.Relative));
            img.Height = 22;
            img.Width = 22;
            img.HorizontalAlignment = HorizontalAlignment.Center;
            StackPanel stkPanel = new StackPanel();
            stkPanel.Orientation = Orientation.Vertical;
            stkPanel.Margin = new Thickness(2);
            stkPanel.Children.Add(img);

            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;
            txtBlock.Text = item.Description;
            txtBlock.Width = 125;

            txtBlock.TextAlignment = TextAlignment.Center;
            txtBlock.VerticalAlignment = VerticalAlignment.Center;
            stkPanel.Children.Add(txtBlock);


            stkPanel.VerticalAlignment = VerticalAlignment.Center;

            return stkPanel;


        }

        #endregion
        private decimal GetWeightFromScale()
        {
            decimal weight = -1;
            try
            {
                if (Defaults.ScaleType == ScaleType.DUMMY || Defaults.ScaleType == ScaleType.NCIPROTOCOL || Defaults.ScaleType == ScaleType.NCIPROTOCOL48 || Defaults.ScaleType == ScaleType.NCRPROTOCOL)
                {
                    IScale scale = PosState.GetInstance().Scale;
                    scale.Connect();
                    weight = scale.GetWeight();
                    scale.Disconnect();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                weight = -1;
            }
            return weight;
        }
        private int currentSubCategory = -1;
        private bool SubCategoryClick = false;
        List<Category> lstSubCategory = new List<Category>();
        private void ButtonItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                LogWriter.LogWrite("ButtonItem_Click : Menu item add Product start => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                AskByUser = false;
                var product = (Product)((Button)sender).DataContext;
                if (product.PlaceHolder == true) return;
                if (product.Description == "") return;
                if (product.Description == "<<")
                {
                    if (currentSubCategory != -1)
                    {
                        currentSubCategory = -1;
                        SubCategoryClick = false;
                        Presenter.LoadItemByCategory(currentCategory.Id);
                        return;
                    }
                    if (itemPrePage <= 0)
                    {
                        itemPrePage = 0;
                        itemNextPage = itemY;
                        SetProductListBox(lstProducts);
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
                        var items = lstProducts.Skip(itemPrePage).Take(itemNextPage).ToList();
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
                            SetProductListBox(lstProducts);
                        }
                    }
                    // LoadCategoryByBroadId(category.Id, true);
                }
                else if (product.Description == ">>")
                {
                    itemNextPage = itemX;
                    itemPrePage = itemPrePage + itemX + 1;
                    if (lstSubCategory.Count > 1)
                        itemPrePage = itemPrePage - lstSubCategory.Count;
                    if (itemPrePage >= lstProducts.Count)
                        itemPrePage = lstProducts.Count == 0 ? 0 : lstProducts.Count - itemX;
                    var items = lstProducts.Skip(itemPrePage).Take(itemNextPage).ToList();
                    items.Insert(0, new Product { Id = default(Guid), Description = "<<", ColorCode = "#FFDCDEDE" });
                    if (itemNextPage + itemPrePage < lstProducts.Count)
                        items.Insert(items.Count, new Product { Id = default(Guid), Description = ">>", ColorCode = "#FFDCDEDE" });
                    else
                        if (items.Count > itemY)
                        items.Insert(itemY, new Product { Id = default(Guid), Description = "", ColorCode = "#FFDCDEDE" });
                    //  LBProductList.ItemsSource = items;
                    AddItemsToGrid(items);
                    // SetProductListBox(items,false);

                }
                //if (product.Description == "<<")
                //{
                //    itemNextPage = 20;
                //    itemPrePage = 0;
                //    Presenter.LoadItemByCategory(selectedCategoryId);
                //}
                //else if (product.Description == ">>")
                //{
                //    itemPrePage = itemPrePage + itemNextPage;
                //    var items = lstItems.Skip(itemPrePage).Take(itemNextPage).ToList();
                //    items.Insert(0, new Product { Id = 0, Description = "<<", ColorCode = "#FFDCDEDE" });
                //    SetProductListBox(items, true);
                //}
                else if (product.Type == 1)
                {
                    if (lstCategories.Count > 5)
                    {
                        currentSubCategory = product.CategoryId;
                        //SubCategoryClick = true;

                    }

                    else
                        SubCategoryClick = false;
                    //  isSeamless = product.Seamless; 

                    //subCatPrePage = 0;
                    //subCatNextPage = y;
                    selectedCategory = product.Description;
                    selectedCategoryId = product.CategoryId;
                    LoadSubCategory2(currentSubCategory);
                    //Presenter.LoadItemByCategory(product.CategoryId);
                    LoadCategoryByBroadId();
                    return;
                }
                else
                {

                    if (product.Id == default(Guid)) return;
                    EntryMode = EntryModeType.ItemEntry;
                    if (product.ItemType == ItemType.Grouped)
                    {
                        var items = product.Products;// Presenter.GetProductByGroup(product.Id);
                        if (product.ReceiptMethod == ReceiptMethod.Show_Product_As_Individuals)
                        {
                            foreach (var itm in items)
                            {
                                Presenter.AddItemToCart(itm, 1);
                            }
                        }
                        else
                        {
                            var grpPrice = items.Sum(p => p.Price);
                            product.Price = grpPrice;
                            Presenter.AddGroupItemToCart(product, 1, items);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(lblPriceMessage.Text))
                        {
                            currentQty = 1;
                            txtNumber.Text = "";
                        }
                        if (!string.IsNullOrEmpty(txtNumber.Text))
                        {
                            if (txtNumber.Text.Length > 10)
                            {
                                MessageBox.Show(UI.Message_InvalidQty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                txtNumber.Focus();
                                return;
                            }
                            if (txtNumber.Text.ToLower().Contains("x"))
                                txtNumber.Text = txtNumber.Text.Remove(txtNumber.Text.Length - 1);
                            decimal.TryParse(txtNumber.Text, out currentQty);
                            if (currentQty == 0)
                            {
                                currentQty = 1;
                            }
                        }
                        selectedProduct = product;
                        var orderDetail = Presenter.CurrentorderDetails.FirstOrDefault(p => p.ItemId == product.Id && p.UnitPrice == product.Price && p.ItemStatus == 0 && p.BIDSNO == BIDSNO && p.IngredientItems == null && string.IsNullOrEmpty(p.ItemComments) && p.RewardApplied == false);
                        if (orderDetail != null && selectedProduct.AskPrice == false && selectedProduct.AskWeight == false && selectedProduct.AskVolume == false)
                        {
                            orderDetail.Product = product;
                            Presenter.HandelAddProductClick(orderDetail, currentQty, BIDSNO);
                            SetCartItems(Presenter.CurrentorderDetails, true);

                            return;
                        }

                        if (product.AskWeight)
                        {
                            decimal weight = -1;
                            weight = GetWeightFromScale();

                            if (weight == -1)
                            {
                                AskByUser = true;
                                lblPriceMessage.Text = UI.EnterWeight;// "Ange weight :";
                                EntryMode = EntryModeType.PluEntry;
                                txtNumber.Text = "";
                                txtNumber.Focus();
                            }
                            else
                            {
                                if (selectedProduct.Unit == ProductUnit.g)
                                {
                                    //Grams
                                    currentQty = Convert.ToDecimal(weight);
                                }
                                else if (selectedProduct.Unit == ProductUnit.kg)
                                {
                                    //Kilograms
                                    var grams = Convert.ToDecimal(weight);
                                    currentQty = grams / 1000;

                                }
                                else if (selectedProduct.Unit == ProductUnit.hg)
                                {
                                    //Hectograms
                                    var grams = Convert.ToDecimal(weight);
                                    currentQty = grams / 100;
                                }

                                if (selectedProduct.AskPrice)
                                {
                                    selectedProduct.BackupAskWeight = selectedProduct.AskWeight;
                                    selectedProduct.AskWeight = false;

                                    EntryMode = EntryModeType.PluEntry;
                                    txtNumber.Text = "";
                                    txtNumber.Focus();
                                    lblPriceMessage.Text = UI.Sales_EnterPrice + ":";

                                    return;
                                }

                                Presenter.AddItemToCart(product, currentQty, BIDSNO, false, true);
                                currentQty = 1;
                                AskByUser = false;
                            }

                        }

                        else if (product.AskPrice)
                        {
                            if (selectedProduct.BackupAskWeight)
                            {
                                selectedProduct.AskWeight = true;
                            }

                            AskByUser = true;
                            lblPriceMessage.Text = UI.Sales_EnterPrice;// "Ange pris :";
                            EntryMode = EntryModeType.PluEntry;
                            txtNumber.Text = "";
                            txtNumber.Focus();
                        }
                        else if (product.AskVolume)
                        {
                            decimal volume = -1;
                            // volume = GetWeightFromScale();
                            int askVolumeQty = 0;
                            int.TryParse(txtNumber.Text, out askVolumeQty);
                            AskVolumeQty = askVolumeQty == 0 ? 1 : askVolumeQty;
                            if (volume == -1)
                            {
                                AskByUser = true;
                                lblPriceMessage.Text = "Ange Volym :";
                                EntryMode = EntryModeType.PluEntry;
                                txtNumber.Text = "";
                                txtNumber.Focus();
                            }

                        }
                        else
                        {
                            LogWriter.LogWrite("ButtonItem_Click : CP1 => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                            Presenter.AddItemToCart(product, currentQty, BIDSNO, false, true);
                            LogWriter.LogWrite("ButtonItem_Click : CP2  => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                            currentQty = 1;
                            AskByUser = false;
                        }
                    }
                    LogWriter.LogWrite("ButtonItem_Click : CP3 => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                    SetCartItems(Presenter.CurrentorderDetails, true);
                    LogWriter.LogWrite("ButtonItem_Click : CP4 => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
            finally
            {
                LogWriter.LogWrite("ButtonItem_Click : Menu item add Product end => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            }
        }
        private void DisplayOrder(Guid orderId)
        {
            try
            {
                Presenter.MasterOrder = Presenter.GetOrderMasterDetailById(orderId);
                if (Presenter.MasterOrder != null)
                {
                    if (!string.IsNullOrEmpty(Presenter.MasterOrder.OrderComments))
                    {
                        brOrderComments.Visibility = Visibility.Visible;
                        txtOrderComment.Text = Presenter.MasterOrder.OrderComments;
                    }
                    else
                    {
                        brOrderComments.Visibility = Visibility.Collapsed;
                        txtOrderComment.Visibility = Visibility.Collapsed;
                        txtOrderComment.Text = "";
                    }
                    Presenter.CurrentorderDetails = Presenter.GetOrderDetailById(orderId);

                    SetCartItems(Presenter.CurrentorderDetails);
                    if (Presenter.CurrentorderDetails.Count == 0)
                        NewEntry();
                }
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
        }

        #region Add To Item Cart

        public void RefreshNewItem()
        {
            txtNumber.Text = "";
            txtNumber.Focus();
            selectedProduct = null;
            EntryMode = EntryModeType.CodeEntry;
            if (Defaults.CustomerView)
                App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
            // ucCart.RefreshItems();
        }

        public OrderType GetOrderType()
        {
            return Presenter.MasterOrder.Type;
        }
        public void SetEntryMode(EntryModeType mode)
        {
            currentQty = 1;
            EntryMode = mode;
        }
        private void UpdateOrderEntry()
        {
            Presenter.MasterOrder.Updated = 1;
            Presenter.OrderEntry(Presenter.MasterOrder);
        }

        /// <summary>
        /// 2011111V1111C
        /// 2111111V1111C
        /// 2211111V1111C
        /// 2311111V1111C
        /// 2411111V1111C
        /// 2511111V1111C
        /// 28RRRRR11111C
        /// </summary>
        /// <param name="value"></param>
        public bool ValidateEAN13(string value = "2354090502440")
        {
            if ((value.StartsWith("20")
                    || value.StartsWith("21")
                    || value.StartsWith("22")
                    || value.StartsWith("23")
                    || value.StartsWith("24")
                    || value.StartsWith("25")
                    || value.StartsWith("28"))
                    && value.Length == 13)
            {
                var barCodeStart = value.Substring(0, 7);
                var barCodeStartWith8 = value.Substring(0, 8);
                var code = value.Substring(1, 1);
                var productCode = value.Substring(2, 5);
                var priceString = "";
                decimal price = 0;
                decimal weight = 0;
                bool isPrice = false;
                bool isWeight = false;
                if (code == "8")
                {
                    isPrice = true;
                    priceString = value.Substring(7, 5);
                    price = Convert.ToDecimal(priceString) / Convert.ToDecimal(100.0);
                }
                else
                {
                    priceString = value.Substring(8, 4);
                    if (code == "0")
                    {
                        isPrice = true;
                        price = Convert.ToDecimal(priceString) / Convert.ToDecimal(100.0);
                    }
                    else if (code == "1")
                    {
                        isPrice = true;
                        price = Convert.ToDecimal(priceString) / Convert.ToDecimal(10.0);
                    }
                    else if (code == "2")
                    {
                        isPrice = true;
                        price = Convert.ToDecimal(priceString);
                    }
                    else if (code == "3")
                    {
                        isWeight = true;
                        weight = Convert.ToDecimal(priceString) / Convert.ToDecimal(1000.0);

                    }
                    else if (code == "4")
                    {
                        isWeight = true;
                        weight = Convert.ToDecimal(priceString) / Convert.ToDecimal(100.0);
                    }
                    else if (code == "5")
                    {
                        isWeight = true;
                        weight = Convert.ToDecimal(priceString) / Convert.ToDecimal(10.0);
                    }
                }

                if (isWeight)
                {
                    var product = productPresenter.GetProductByBarCodeEAN13StartCode(barCodeStartWith8);
                    if (product == null)
                    {
                        MessageBox.Show("No product found with this bar code.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return true;
                    }
                    char[] charsToTrim1 = { '0' };
                    if (weight <= 0)
                    {
                        weight = 1;
                    }
                    currentQty = weight;
                    Presenter.AddItemToCart(product, currentQty, 0, true);
                    AskByUser = false;
                    currentQty = 1;
                    selectedProduct = product;
                }
                else if (isPrice)
                {
                    var product = productPresenter.GetProductByBarCodeEAN13StartCode(barCodeStart);
                    if (product == null)
                    {
                        MessageBox.Show("No product found with this bar code.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return true;
                    }
                    char[] charsToTrim1 = { '0' };
                    currentQty = 1;
                    product.Price = price;
                    Presenter.AddItemToCart(product, currentQty);
                    AskByUser = false;
                    currentQty = 1;
                    selectedProduct = product;
                }

                return true;
            }

            return false;
        }

        public void BarCodeEntry(string value)
        {
            var barcode = "";
            //string value = txtNumber.Text;
            if (ValidateEAN13(value))
            {

            }
            else if (value.StartsWith("7388") && value.Length == 13) // NewsPaper
            {
                var barCodeStart = value.Substring(0, 8);
                var price = value.Substring(8, 3);
                var product = productPresenter.GetProductByBarCodeEAN13StartCode(barCodeStart);
                if (product == null)
                {
                    MessageBox.Show("No product found with this bar code.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (product != null)
                {
                    if (price != "000")
                    {
                        product.Price = Convert.ToDecimal(price);
                    }
                    currentQty = 1;
                    Presenter.AddItemToCart(product, currentQty);
                    AskByUser = false;
                    currentQty = 1;
                    selectedProduct = product;
                    txtNumber.Text = "";
                    txtNumber.Focus();
                }
            }
            else if (value.StartsWith("9899") && value.Length == 13)
            {
                var tax25 = value.Substring(4, 4);
                var tax12 = value.Substring(8, 4);
                if (tax25 != "0000")
                {
                    var productPANT = productPresenter.GetProductByPANT("PANT25");
                    if (productPANT == null)
                    {
                        MessageBox.Show("No product found with this bar code.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (productPANT != null)
                    {
                        char[] charsToTrim1 = { '0' };
                        var intTax25 = Convert.ToInt32(tax25.TrimStart(charsToTrim1));
                        productPANT.Price = (-1) * Convert.ToDecimal(intTax25 / 10.0);
                        productPANT.Tax = 25;
                        Presenter.AddItemToCart(productPANT, currentQty);
                        AskByUser = false;
                        currentQty = 1;
                        selectedProduct = productPANT;
                    }
                }
                if (tax12 != "0000")
                {
                    var productPANT = productPresenter.GetProductByPANT("PANT12");
                    if (productPANT == null)
                    {
                        MessageBox.Show("No product found with this bar code.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (productPANT != null)
                    {
                        char[] charsToTrim1 = { '0' };
                        var intTax25 = Convert.ToInt32(tax12.TrimStart(charsToTrim1));
                        productPANT.Price = (-1) * Convert.ToDecimal(intTax25 / 10.0);
                        productPANT.Tax = 12;
                        Presenter.AddItemToCart(productPANT, currentQty);
                        AskByUser = false;
                        currentQty = 1;
                        selectedProduct = productPANT;
                    }
                }
                txtNumber.Text = "";
                txtNumber.Focus();
            }
            else
            {
                if (value.Contains("x"))
                {
                    string[] values = value.Split('x');

                    currentQty = Convert.ToInt32(values[0]);

                    barcode = values[1];
                }
                else
                    barcode = value;
                if (string.IsNullOrEmpty(barcode))
                {
                    MessageBox.Show(UI.Message_EANMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumber.Text = "";
                    txtNumber.Focus();
                    return;
                }
                var product = Presenter.GetProductByBarCode(barcode);

                if (product.Id == default(Guid))
                {
                    //if (MessageBox.Show(UI.Message_UnknowBarcode + "\n" + UI.Message_AddConfirm, UI.Global_Warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    //{
                    //    AddProductWindow productWindow = new AddProductWindow(barcode);
                    //    productWindow.ShowDialog();
                    //}
                    NewProductAddWindow productWindow = new NewProductAddWindow(barcode);
                    if (productWindow.ShowDialog() == true)
                    {
                        product = productWindow.selectedProduct;
                    }
                    txtNumber.Text = "";

                    txtNumber.Focus();
                    if (product == null || product.Id == default(Guid))
                        return;
                    if (product.Price == 0)
                    {
                        App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.Main_InvalidAmount);
                        return;
                    }
                }

                if (Presenter.MasterOrder.Status == OrderStatus.Completed)
                    return;
                selectedProduct = product;
                if (VariableWeight(barcode))
                {
                    Presenter.AddItemToCart(product, currentQty, BIDSNO);
                    currentQty = 1;
                    AskByUser = false;
                }
                else if (product.AskWeight)
                {
                    decimal weight = -1;
                    weight = GetWeightFromScale();

                    if (weight == -1)
                    {
                        AskByUser = true;
                        lblPriceMessage.Text = UI.EnterWeight + " :";
                        EntryMode = EntryModeType.ItemEntry;
                        txtNumber.Text = "";
                        txtNumber.Focus();
                    }
                    else
                    {
                        if (selectedProduct.Unit == ProductUnit.g)
                        {
                            //Grams
                            currentQty = Convert.ToDecimal(weight);
                        }
                        else if (selectedProduct.Unit == ProductUnit.kg)
                        {
                            //Kilograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 1000;

                        }
                        else if (selectedProduct.Unit == ProductUnit.hg)
                        {
                            //Hectograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 100;
                        }
                        Presenter.AddItemToCart(product, currentQty, BIDSNO, false, true);
                        currentQty = 1;
                        AskByUser = false;
                    }

                }
                else if (product.AskPrice)
                {
                    AskByUser = true;
                    lblPriceMessage.Text = UI.EnterPrice + " :";
                    EntryMode = EntryModeType.ItemEntry;
                    txtNumber.Text = "";
                    txtNumber.Focus();
                }
                else
                {
                    Presenter.AddItemToCart(selectedProduct, currentQty, BIDSNO, false, true);
                    AskByUser = false;
                    currentQty = 1;
                }
                txtNumber.Text = "";
                txtNumber.Focus();
                if (AskByUser == false)
                    selectedProduct = null;
            }

        }

        private bool VariableWeight(string _barcode)
        {
            try
            {
                if (selectedProduct.BarCode.Length != 8)
                    return false;

                Regex EAN13 = new Regex(@"\b(?:\d{13})\b");

                if (string.IsNullOrEmpty(_barcode) || EAN13.IsMatch(_barcode) == false)
                    return false;


                string ean = _barcode.Substring(0, 8);

                string weightSpec = _barcode.Substring(8, 5);
                string weight = weightSpec.Substring(0, 4);
                string checkDigit = weightSpec.Substring(4, 1);
                //  decimal Weightdecimal = decimalPosition == "0" ? 1 : decimalPosition == "1" ? 10 : decimalPosition == "2" ? 100 : decimalPosition == "3" ? 100 : decimalPosition == "4" ? 10000 : 100000;
                decimal netWeght = Convert.ToDecimal(weight);// / Weightdecimal;
                currentQty = netWeght;
                if (selectedProduct.Unit == ProductUnit.g)// UnitName == "Kg" ? ProductUnit.kg : ProductUnit.kg;
                    currentQty = netWeght;
                else if (selectedProduct.Unit == ProductUnit.kg)
                    currentQty = netWeght / 1000;
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                return false;
            }
        }
        private int AskVolumeQty = 1;
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogWriter.LogWrite("btnEnter_Click : Barcode add Product start => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                if (EntryMode == EntryModeType.CodeEntry)
                {
                    string code = txtNumber.Text;
                    if (!string.IsNullOrEmpty(code))//&& code.Contains("x")
                    {
                        BarCodeEntry(code);
                        return;
                    }
                }
                if (selectedProduct == null)
                {
                    MessageBox.Show(UI.Message_SelectItemToAddInCart, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtNumber.Focus();
                    return;
                }

                if (!string.IsNullOrEmpty(txtNumber.Text))
                {
                    RegionInfo local = RegionInfo.CurrentRegion;
                    var value = local.Name == "SE" ? txtNumber.Text : txtNumber.Text.Replace(',', '.');

                    if (selectedProduct.AskWeight)
                    {
                        if (selectedProduct.Unit == ProductUnit.g)
                        {
                            //Grams
                            currentQty = Convert.ToDecimal(value);
                        }
                        else if (selectedProduct.Unit == ProductUnit.kg)
                        {
                            //Kilograms
                            var grams = Convert.ToDecimal(value);
                            currentQty = grams / 1000;

                        }
                        else if (selectedProduct.Unit == ProductUnit.hg)
                        {
                            //Hectograms
                            var grams = Convert.ToDecimal(value);
                            currentQty = grams / 100;
                        }

                        if (selectedProduct.AskPrice)
                        {
                            selectedProduct.BackupAskWeight = selectedProduct.AskWeight;
                            selectedProduct.AskWeight = false;

                            EntryMode = EntryModeType.PluEntry;
                            txtNumber.Text = "";
                            txtNumber.Focus();
                            lblPriceMessage.Text = UI.Sales_EnterPrice + ":";

                            return;
                        }
                    }
                    else if (selectedProduct.AskPrice)
                    {
                        if (selectedProduct.BackupAskWeight)
                        {
                            selectedProduct.AskWeight = true;
                        }

                        decimal unitPriceWithVat = Convert.ToDecimal(value);
                        selectedProduct.Price = unitPriceWithVat;
                        lblPriceMessage.Text = "";
                    }

                    if (selectedProduct.AskVolume)
                    {
                        currentQty = Convert.ToDecimal(value);
                    }
                    txtNumber.Text = "";
                }
                txtNumber.Focus();
                if (selectedProduct.AskVolume)
                {

                    List<Product> itemsList = new List<Product>();
                    for (int i = 0; i < AskVolumeQty; i++)
                    {
                        itemsList.Add(selectedProduct);
                    }
                    decimal qty = currentQty;
                    foreach (var itemList in itemsList)
                        Presenter.AddItemToCart(itemList, qty, 0);
                    selectedProduct = null;
                }
                else
                    Presenter.AddItemToCart(selectedProduct, currentQty, 0);
                currentQty = 1;
                AskByUser = false;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LogWriter.LogWrite("btnEnter_Click : Barcode add Product End => " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            }
            txtNumber.Text = "";
            txtNumber.Focus();


        }

        #endregion
        #region Check Out Area
        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                Defaults.PerformanceLog.Add("Clicked On Check out Button   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                //if (Defaults.IsOpenOrder == false && Presenter.MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder)
                //{
                //    if (Defaults.BONG)
                //    {
                //        Presenter.SendPrintToKitchenWithoutReset(Presenter.MasterOrder);
                //    }
                //}

                IsFromHold = false;
                //if (IsMerge && mergeOrder != null)
                //    SetMergeOrder();
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    sb.AppendLine("cp1= if (Defaults.OrderEntryType == OrderEntryType.RecordAll)");
                    Presenter.MasterOrder.Status = Presenter.MasterOrder.OrderStatusFromType;
                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.CheckOutUserId = Defaults.User.Id;
                    var masterorder = Presenter.GetOrderMasterDetailById(Presenter.MasterOrder.Id);
                    Presenter.MasterOrder.Bong = masterorder.Bong;
                    Presenter.MasterOrder.DailyBong = masterorder.DailyBong;
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.OrderEntry(Presenter.MasterOrder);

                    //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                    //    Presenter.SendPrintToKitchen(Presenter.MasterOrder, false);
                }
                else
                    Presenter.HandleHoldClick();
                sb.AppendLine("cp2= HandleHoldClick)");

                Defaults.PerformanceLog.Add("Checkout starting....         -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Presenter.HandleCheckOutClick();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);

                LogWriter.LogWrite(new Exception(sb.ToString(), ex));
            }
        }
        #endregion



        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (AskByUser == true && !string.IsNullOrEmpty(txtNumber.Text))
            {
                if (txtNumber.Text.Length >= 10)
                    return;
            }
            lblPriceMessage.Text = "";
            txtNumber.Text = txtNumber.Text + (sender as Button).Content;
            txtNumber.Focus();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsFromHold = false;
                lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
                if (Presenter.MasterOrder != null && Presenter.MasterOrder.Status == OrderStatus.New)
                    CancelOrder();

                NewRecord();
                NewEntry();
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeNew), Presenter.MasterOrder.Id);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }
        private void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
        {
            if (AskByUser == true && !string.IsNullOrEmpty(txtNumber.Text))
            {
                if (txtNumber.Text.Length >= 10)
                    e.Handled = true;
            }
            else
            {
                if (Defaults.DigitOnly)
                {
                    decimal val = 0;
                    e.Handled = !decimal.TryParse(e.Text, out val);
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = "";
            txtNumber.Focus();
        }

        int BIDSNO = 0;
        private void btnBIDS_Click(object sender, RoutedEventArgs e)
        {
            if (Presenter.CurrentorderDetails.Count > 1)
            {
                ucCart.SetBEDS();
                BIDSNO = BIDSNO + 1;
            }
        }
        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                if (Defaults.ShowBongAlert)
                {
                    if (MessageBox.Show(UI.Global_Bong_Confirm, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                IsFromHold = true;
                Presenter.MasterOrder.OrderLines = Presenter.CurrentorderDetails;

                if (isMerge && mergeOrder != null)
                {
                    var masterorder = Presenter.GetOrderMasterDetailById(mergeOrder.Id);
                    Presenter.MasterOrder.Bong = masterorder.Bong;
                    Presenter.MasterOrder.DailyBong = masterorder.DailyBong;
                    Presenter.SendPrintToKitchenWithoutReset(Presenter.MasterOrder);
                    SetMergeOrders();

                }

                if ((selectedTable == null || selectedTable.Id == 0) && Defaults.TableNeededOnBong && Presenter.MasterOrder.Type != OrderType.TakeAway)
                {
                    var tableWindow = new TableWindow();
                    //  this.IsEnabled = false;
                    if (tableWindow.ShowDialog() == true)
                    {
                        selectedTable = tableWindow.SelectedTable;
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                    }
                }
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = OrderStatus.AssignedKitchenBar;
                    if (selectedTable != null)
                    {
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                        if (string.IsNullOrEmpty(Presenter.MasterOrder.Comments))
                            Presenter.MasterOrder.Comments = selectedTable.Name;
                    }
                    Presenter.MasterOrder.Updated = 1;

                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.OrderEntry(Presenter.MasterOrder);
                    bool mrg = isMerge;
                    Order mrgorder = mergeOrder;
                    var masterID = Presenter.MasterOrder.Id;
                    if (Defaults.BONG && Presenter.MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder)
                    {
                        int dailyBongCounter = 0;
                        InvoiceHandler invoiceHndler = new InvoiceHandler();
                        int bongNo = invoiceHndler.GetLastBongNo(out dailyBongCounter, false);
                        Presenter.MasterOrder.Bong = bongNo.ToString();
                        Presenter.MasterOrder.DailyBong = dailyBongCounter.ToString();
                        var orderid = Presenter.MasterOrder.Id;
                        Presenter.SendPrintToKitchen(Presenter.MasterOrder);
                        invoiceHndler.UpdateBongNo(orderid, dailyBongCounter);
                        //if (mrg && mrgorder != null)
                        //{
                        //    mergeOrder = mrgorder;
                        //    SetMergeOrders();
                        //}
                    }
                    else
                        NewRecord();
                }
                else
                {
                    Presenter.HandleHoldClick();

                }


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (Defaults.ShowBongAlert)
                {
                    if (MessageBox.Show(UI.Sale_ConfirmPause, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                if (string.IsNullOrEmpty(Presenter.MasterOrder.OrderComments))
                {
                    Presenter.MasterOrder.Comments = Utilities.PromptInput(UI.Sales_OrderCommentButton, UI.Sales_EnterComment, Presenter.MasterOrder.OrderComments);
                }
                IsFromHold = true;
                if (isMerge && mergeOrder != null)
                    SetMergeOrder();

                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = OrderStatus.AssignedKitchenBar;

                    Presenter.MasterOrder.Updated = 1;

                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.OrderEntry(Presenter.MasterOrder);
                    NewRecord();
                }
                else
                {
                    Presenter.HandleHoldClick();

                }


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }
        private void btnCashDraw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PosState.OpenDrawer();
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
                txtNumber.Text = "";
                txtNumber.Focus();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                App.MainWindow.ShowError(UI.Global_Error, ex.Message);
            }
        }

        private void btnComma_Click(object sender, RoutedEventArgs e)
        {
            if (!txtNumber.Text.Contains(","))
                txtNumber.Text = txtNumber.Text + ",";
        }

        private void btnMultiply_Click(object sender, RoutedEventArgs e)
        {
            txtNumber.Text = txtNumber.Text + "x";
            txtNumber.Focus();
            txtNumber.Select(txtNumber.Text.Length, 0);
        }

        private void txtNumber_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                { // ENTER pressed
                  // LogWriter.LogWrite("Card Reader Event Called" + " " + txtNumber.Text);
                    if (txtNumber.Text.Length > 20)
                    {
                        decimal discousnt = Presenter.GetCardDiscount();
                        if (discousnt > 0)
                        {
                            txtNumber.Text = "";
                            txtNumber.Focus();
                            return;
                        }
                    }
                    lblPriceMessage.Text = "";
                    if (EntryMode == EntryModeType.CodeEntry)
                        BarCodeEntry(txtNumber.Text);
                    else if (selectedProduct != null)
                        btnEnter_Click(sender, null);

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }
        private void btnSavePLU_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtNumber.Text))
                return;
            try
            {
                var pluCode = "";


                string value = txtNumber.Text;
                if (value.Contains("x"))
                {
                    string[] values = value.Split('x');
                    currentQty = Convert.ToInt16(values[0]);
                    pluCode = values[1];
                }
                else
                    pluCode = value;

                if (string.IsNullOrEmpty(pluCode))
                {
                    MessageBox.Show(UI.Message_PLUMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumber.Text = "";
                    txtNumber.Focus();
                    return;
                }
                var product = Presenter.GetProductByPlu(pluCode);
                if (product.Id == default(Guid))
                {
                    MessageBox.Show(UI.Message_InvalidPLU, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumber.Text = "";
                    txtNumber.Focus();
                    return;
                }

                if (Presenter.MasterOrder.Status == OrderStatus.Completed)
                    return;

                selectedProduct = product;

                if (product.AskWeight)
                {
                    decimal weight = -1;
                    weight = GetWeightFromScale();

                    if (weight == -1)
                    {
                        EntryMode = EntryModeType.ItemEntry;
                        txtNumber.Text = "";
                        txtNumber.Focus();
                        lblPriceMessage.Text = UI.EnterWeight + ":";
                    }
                    else
                    {
                        if (selectedProduct.Unit == ProductUnit.g)
                        {
                            //Grams
                            currentQty = Convert.ToDecimal(weight);
                        }
                        else if (selectedProduct.Unit == ProductUnit.kg)
                        {
                            //Kilograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 1000;

                        }
                        else if (selectedProduct.Unit == ProductUnit.hg)
                        {
                            //Hectograms
                            var grams = Convert.ToDecimal(weight);
                            currentQty = grams / 100;
                        }
                        Presenter.AddItemToCart(product, currentQty, BIDSNO);
                        currentQty = 1;
                        AskByUser = false;
                    }
                }
                else if (product.AskPrice)
                {
                    EntryMode = EntryModeType.ItemEntry;
                    txtNumber.Text = "";
                    txtNumber.Focus();
                    lblPriceMessage.Text = UI.Sales_EnterPrice + ":";
                }
                else
                {
                    Presenter.AddItemToCart(selectedProduct, currentQty, BIDSNO);
                    AskByUser = false;
                    currentQty = 1;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                //LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (ucCart.SelectedItem == null)
            {
                MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!string.IsNullOrEmpty(Defaults.ReturnCode))
            {
                ReturnCodeWindow returnCodeWindow = new ReturnCodeWindow();
                if (returnCodeWindow.ShowDialog() == false)
                    return;
            }

            if (MessageBox.Show(UI.Global_ReturnConfirm, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Presenter.MasterOrder.Type = OrderType.Return;
                    lblOrderType.Visibility = Visibility.Visible;
                    lblOrderType.Text = UI.Sales_ReturnOrder;

                    Presenter.CurrentorderDetails
                           .Select(s =>
                           {
                               s.Direction = -1;
                               //s.ItemDiscount = -1;
                               s.OrderType = OrderType.Return;
                               if (s.IngredientItems != null && s.IngredientItems.Count() > 0)
                               {
                                   s.IngredientItems.Select(c => { c.Direction = -1; return c; }).ToList();
                               }
                               return s;
                           }).ToList();


                    foreach (var item in Presenter.CurrentorderDetails)
                    {
                        //item.Direction = -1;
                        //item.ItemDiscount = (-1) * item.ItemDiscount;
                        //item.OrderType = OrderType.Return;
                        /*Ingredients items*/
                        //if (item.IngredientItems != null && item.IngredientItems.Count() > 0 )
                        //{
                        //    foreach (var i in item.IngredientItems)
                        //    {
                        //        // var ingItem = db.OrderDetail.First(o => o.Id == i.Id);
                        //        i.Direction = -1;
                        //    }
                        //}

                        Presenter.EditItem(item);

                    }
                    //}
                    if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                    {
                        UpdateOrderEntry();
                    }
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeReturn), Presenter.MasterOrder.Id);

                    SetCartItems(Presenter.CurrentorderDetails);
                    txtNumber.Text = "";
                    txtNumber.Focus();
                    DirectCashButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    //LogWriter.LogWrite(ex);
                }
            }
        }

        public void ShowError(string errorTitle, string errorMessage)
        {
            App.MainWindow.ShowError(errorTitle, errorMessage);
        }

        public void SetHistoryViewResult(List<Order> orders)
        {
            var _orders = orders.OrderByDescending(o => Convert.ToInt32(o.ReceiptNumber)).ToList();
            orderHistoryGrid.ItemsSource = _orders;
        }

        private void OnRowLoading(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item != null)
            {
                var order = e.Row.Item as Order;
                if (order != null)
                {
                    if (order.Status == OrderStatus.ReturnOrder)
                    {
                        e.Row.Foreground = new SolidColorBrush(Colors.IndianRed);
                    }
                    else
                    {
                        e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        public void SetOrderTotalOfDay(string orderTotalOfDay)
        {

        }

        private void btnOrderComments_Click(object sender, RoutedEventArgs e)
        {            
            Presenter.MasterOrder.OrderComments = Utilities.PromptInput(UI.Sales_OrderCommentButton, UI.Sales_EnterComment, Presenter.MasterOrder.OrderComments);
            if (!string.IsNullOrEmpty(Presenter.MasterOrder.OrderComments))
            {
                brOrderComments.Visibility = Visibility.Visible;
                txtOrderComment.Visibility = Visibility.Visible;
                txtOrderComment.Text = Presenter.MasterOrder.OrderComments;
                LogWriter.JournalLog((int)JournalActionCode.OrderComment, Presenter.MasterOrder.Id);
            }
            else
            {
                brOrderComments.Visibility = Visibility.Collapsed;
                txtOrderComment.Text = "";
                txtOrderComment.Visibility = Visibility.Collapsed;
            }
            txtNumber.Text = "";
            txtNumber.Focus();
        }
        #region IPlaceOrderView
        public int GetCartIndex()
        {
            return addCartIndex;
        }

        public bool FormHoldStatus()
        {
            return IsFromHold;
        }

        public void ShowSurvey()
        {
            if (ConfigurationManager.AppSettings["ShowSurvey"] == "1")
            {
                //SurveyHTMLWindow window = new SurveyHTMLWindow();
                //window.ShowDialog();
                ProcessStartInfo start = new ProcessStartInfo();
                // Enter in the command line arguments, everything you would enter after the executable name itself
                start.Arguments = "";
                // Enter the executable to run, including the complete path
                start.FileName = ConfigurationManager.AppSettings["SurveyEXEPath"];
                // Do you want to show a console window?
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;
                int exitCode;


                // Run the external process & wait for it to finish
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();

                    // Retrieve the app's exit code
                    exitCode = proc.ExitCode;
                }

                //System.Diagnostics.Process.Start(ConfigurationManager.AppSettings["SurveyEXEPath"]);
            }
        }

        public void NewRecord()
        {
            Defaults.IsOpenOrder = false;
            AskVolumeQty = 1;
            brOrderComments.Visibility = Visibility.Collapsed;
            txtOrderComment.Text = "";
            Presenter.MasterOrder = new Order();
            Presenter.CurrentorderDetails = new List<OrderLine>();
            //Presenter.db = PosState.GetInstance().Context;
            currentQty = 1;
            AskByUser = false;
            isMerge = false;
            mergeOrder = null;
            ucCart.NewRecord();
            type = OrderType.Standard;
            selectedProduct = null;
            selectedTable = new FoodTable();
            lblOrderType.Visibility = Visibility.Collapsed;
            lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
            lblOrderTypeCustomer.Visibility = Visibility.Collapsed;
            lblOrderType.Text = "New Order";
            Console.WriteLine("btnSelectTable.Content" + TableButtonContent(UI.Sales_SelectTableButton));
            ButtonOpenOrder.Content = UI.Sales_OpenOrderButton;
            // btnSelectTable.Content = UI.Sales_SelectTableButton;
            App.MainWindow.UpdateNewOrder("New Order");
            txtNumber.Text = "";
            txtNumber.Focus();
            lblPriceMessage.Text = "";

        }

        public void NewEntry()
        {
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                if (Presenter.MasterOrder == null)
                    Presenter.MasterOrder = new Order
                    {
                        ShiftNo = App.MainWindow.ShiftNo
                    };
                else
                    Presenter.MasterOrder.ShiftNo = App.MainWindow.ShiftNo;
                Presenter.MasterOrder = Presenter.OrderEntry(Presenter.MasterOrder);

                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.NewOrderEntry), Presenter.MasterOrder.Id);
            }
            try
            {
                // ClientConnection();
            }
            catch (Exception e)
            {
                Log.LogWrite(e);
            }

        }
        #endregion


        private void btnTakeAway_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Presenter.MasterOrder.Type == OrderType.Return || Presenter.MasterOrder.Type == OrderType.TakeAwayReturn)
                {
                    if (lblOrderTypeSecondary.Visibility == Visibility.Visible)
                    {
                        Presenter.MasterOrder.Type = OrderType.Return;
                        lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Presenter.MasterOrder.Type = OrderType.TakeAwayReturn;
                        lblOrderTypeSecondary.Visibility = Visibility.Visible;
                        LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeReturnTakeAway), Presenter.MasterOrder.Id);
                    }
                    return;
                }
                else
                    Presenter.MasterOrder.Type = OrderType.TakeAway;
                lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
                //  Presenter.MasterOrder.Type = (Presenter.MasterOrder.Type == OrderType.TakeAway) ? OrderType.Standard : OrderType.TakeAway;
                if (Presenter.MasterOrder.Type == OrderType.TakeAway)
                {
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTypeTakeAway), Presenter.MasterOrder.Id);
                    lblOrderType.Visibility = Visibility.Visible;
                    lblOrderType.Text = UI.Sales_TakeAwayButton;
                }
                else
                {
                    lblOrderType.Visibility = Visibility.Collapsed;
                    lblOrderTypeSecondary.Visibility = Visibility.Collapsed;
                    lblOrderType.Text = "";
                }
                if (Presenter.MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    UpdateOrderEntry();
                }

                txtNumber.Text = "";
                txtNumber.Focus();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }
        FoodTable selectedTable;
        OrderType selectedType;
        private void btnSelectTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tableWindow = new TableWindow();
                HandleTableWindow(tableWindow.ShowDialog() == true, tableWindow.IsNewOrder, tableWindow.SelectedTable);

                txtNumber.Text = "";
                txtNumber.Focus();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        private void HandleTableWindow(
            bool pResult,
            bool pIsNewOrder,
            FoodTable pSelectedTable)
        {
            if (pResult)
            {
                if (Presenter.MasterOrder.Type == OrderType.TakeAway)
                {
                    type = OrderType.TableTakeAwayOrder;
                    selectedType = OrderType.TableTakeAwayOrder;
                }
                else
                {
                    type = OrderType.TableOrder;
                    selectedType = OrderType.TableOrder;

                }
                isNewOrder = pIsNewOrder;
                selectedTable = pSelectedTable;
                ButtonOpenOrder.Content = selectedTable.Name;
                // btnSelectTable.Content = selectedTable.Name;
                Console.WriteLine("btnSelectTable.Content" + TableButtonContent(selectedTable.Name));
                if (isNewOrder)
                {

                    Presenter.MasterOrder.TableId = selectedTable.Id;
                    Presenter.MasterOrder.Comments = selectedTable.Name;
                    Presenter.MasterOrder.TableName = selectedTable.Name;
                    Presenter.MasterOrder.Type = type;
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    isMerge = false;
                }
                else
                {
                    List<Order> orders = Presenter.GetOpenOrdersOnTable(selectedTable.Id);
                    isMerge = true;
                    mergeOrders = orders;
                    mergeOrder = orders.FirstOrDefault();
                    Presenter.MasterOrder.TableId = selectedTable.Id;
                    Presenter.MasterOrder.Comments = selectedTable.Name;
                    Presenter.MasterOrder.TableName = selectedTable.Name;
                    Presenter.MasterOrder.Type = type;
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);


                }
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OrderTableSelected), Presenter.MasterOrder.Id, null, Presenter.MasterOrder.TableId, "");
            }
            else
            {
                Presenter.MasterOrder.TableId = 0;
                Console.WriteLine("btnSelectTable.Content" + UI.Sales_SelectTableButton);
                ButtonOpenOrder.Content = UI.Sales_OpenOrderButton;

            }
        }

        private void CustomerInfo_Click(object sender, RoutedEventArgs e)
        {
            if (Defaults.CustomerOrderInfo == true)
            {
                var customerwindow = new CustomerWindow(false, UI.Sales_CustomerButton, CustomerType.All);
                if (customerwindow.ShowDialog() == true)
                {
                    Presenter.MasterOrder.CustomerId = customerwindow.SelectedCustomer.Id;
                    Presenter.MasterOrder.Comments = customerwindow.SelectedCustomer.Name;
                    lblOrderTypeCustomer.Text = customerwindow.SelectedCustomer.Name;
                    lblOrderTypeCustomer.Visibility = Visibility.Visible;
                }
            }
            else
            {
                var title = UI.Sales_CustomerButton;
                Presenter.MasterOrder.Comments = Utilities.PromptInput(title, UI.Sales_EnterComment, Presenter.MasterOrder.OrderComments);
            }
            txtNumber.Text = "";
            txtNumber.Focus();
        }

        private void ButtonOpenOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Defaults.SaleType == SaleType.Restaurant)
                {
                    if (Presenter.CurrentorderDetails != null && Presenter.CurrentorderDetails.Count > 0)
                    {
                        Presenter.MasterOrder.OrderLines = Presenter.CurrentorderDetails;
                        if (selectedTable != null && Presenter.MasterOrder.TableId == 0)
                            Presenter.MasterOrder.TableId = selectedTable.Id;



                        var openOrderWindow = new OpenOrderWindow(Presenter.MasterOrder);
                        openOrderWindow.ShowDialog();

                        HandleTableWindow(openOrderWindow.DialogResult == true, openOrderWindow.IsNewOrder, openOrderWindow.SelectedTable);

                        if (openOrderWindow.NewOrderMerged)
                        {
                            if (Presenter.MasterOrder != null && Presenter.MasterOrder.IsForAdult)
                            {
                                Presenter.ShowSurvey();
                            }
                            NewRecord();
                        }
                    }
                    else
                    {
                        var openOrderWindow = new OpenOrderWindow();
                        openOrderWindow.ShowDialog();
                        HandleTableWindow(openOrderWindow.DialogResult == true, openOrderWindow.IsNewOrder, openOrderWindow.SelectedTable);
                    }
                }
                else
                {
                    var openOrderWindow = new OpenRetailOrderWindow();
                    openOrderWindow.ShowDialog();
                }
                txtNumber.Text = "";
                txtNumber.Focus();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                //LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message);
            }

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelOrder();
        }
        private void CancelOrder()
        {
            try
            {
                if (Presenter.MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = OrderStatus.OrderCancelled;
                    UpdateOrderEntry();

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        public void SetTextBoxFocus()
        {
            txtNumber.Text = "";
            txtNumber.Focus();

        }

        public Product GetSelectedItem()
        {
            return selectedProduct;
        }

        public void SetSelectedItem(Product item)
        {
            selectedProduct = item;
        }

        public string GetTextBoxValue()
        {
            return txtNumber.Text;
        }

        public void SetAskPriceValue(string enterPrice)
        {
            lblPriceMessage.Text = enterPrice;
        }

        private void btnDirectCash_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Defaults.PerformanceLog.Add("Clicked On direct cash payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                IsFromHold = false;
                //if (IsMerge && mergeOrder != null)
                //    SetMergeOrder();
                Presenter.MasterOrder.OrderLines = Presenter.CurrentorderDetails;
                //(Presenter.MasterOrder.OrderType== OrderType.TakeAway)
                if (Defaults.IsOpenOrder == false && Presenter.MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder)
                {
                    if (Defaults.BONG && Defaults.DirectBONG)
                    {
                        Presenter.SendPrintToKitchenWithoutReset(Presenter.MasterOrder);
                    }
                }

                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = Presenter.MasterOrder.OrderStatusFromType;
                    if (selectedTable != null)
                    {
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                        Presenter.MasterOrder.Comments = selectedTable.Name;

                    }
                    if (Presenter.MasterOrder.OrderStatusFromType == OrderStatus.ReturnOrder)// "ReturnOrder")
                    {

                        foreach (var det in Presenter.CurrentorderDetails)
                        {
                            if (det.Quantity < 0)
                                det.Quantity = -1 * det.Quantity; // conver in plus
                        }
                    }

                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.CheckOutUserId = Defaults.User.Id;
                    //Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.GetOrderMasterDetailById(Presenter.MasterOrder.Id));
                    //Presenter.MasterOrder = Presenter.OrderEntry(Presenter.GetOrderMasterDetailById(Presenter.MasterOrder.Id));
                    if (string.IsNullOrEmpty(Presenter.MasterOrder.Bong) || Presenter.MasterOrder.Bong == "0")
                    {
                        int dailyBongCounter = 0;
                        InvoiceHandler invoiceHndler = new InvoiceHandler();
                        int bongNo = invoiceHndler.GetLastBongNo(out dailyBongCounter, true);
                        Presenter.MasterOrder.Bong = bongNo.ToString();
                        Presenter.MasterOrder.DailyBong = dailyBongCounter.ToString();
                        //var masterorder = Presenter.GetOrderMasterDetailById(Presenter.MasterOrder.Id);
                        //Presenter.MasterOrder.Bong = masterorder.Bong;
                    }
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.MasterOrder = Presenter.OrderEntry(Presenter.MasterOrder);


                    //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                    //    Presenter.SendPrintToKitchen(Presenter.MasterOrder, false);
                }
                else
                    Presenter.HandleHoldClick();
                Defaults.PerformanceLog.Add("Direct Cash Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Presenter.HandleDirectPaymentClick(false);


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        private void btnCreditCard_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Presenter.MasterOrder.OrderLines = Presenter.CurrentorderDetails;

                //if (Defaults.IsOpenOrder == false && Presenter.MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder)
                //{
                //    if (Defaults.BONG)
                //    {
                //        Presenter.SendPrintToKitchenWithoutReset(Presenter.MasterOrder);
                //    }
                //}

                //if (Defaults.IsOpenOrder == false)
                //{
                //    if (Defaults.BONG)
                //    {
                //        Presenter.SendPrintToKitchenWithoutReset(Presenter.MasterOrder);
                //    }
                //}

                //if (Utilities.CheckDeviceBamboraConnection() == false && Defaults.PaymentDeviceType == PaymentDeviceType.CONNECT2T)// 
                //{
                //    DeviceConnectionWindow deviceConnection = new DeviceConnectionWindow();
                //    if (deviceConnection.ShowDialog() == false)
                //        return;

                //}
                Defaults.PerformanceLog.Add("Clicked On direct card payment -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                IsFromHold = false;
                //if (IsMerge && mergeOrder != null)
                //    SetMergeOrder();
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    if (selectedTable != null)
                    {
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                        Presenter.MasterOrder.Comments = selectedTable.Name;
                    }
                    Presenter.MasterOrder.Status = Presenter.MasterOrder.OrderStatusFromType;
                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.CheckOutUserId = Defaults.User.Id;
                    var masterorder = Presenter.GetOrderMasterDetailById(Presenter.MasterOrder.Id);
                    Presenter.MasterOrder.Bong = masterorder.Bong;
                    Presenter.MasterOrder.DailyBong = masterorder.DailyBong;
                    int dailyBongCounter = 0;
                    InvoiceHandler invoiceHndler = new InvoiceHandler();
                    int bongNo = invoiceHndler.GetLastBongNo(out dailyBongCounter, false);
                    //Presenter.MasterOrder.Bong = bongNo.ToString();

                    //invoiceHndler.UpdateBongNo(Presenter.MasterOrder.Id, dailyBongCounter);

                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.MasterOrder = Presenter.OrderEntry(Presenter.MasterOrder);

                    //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                    //    Presenter.SendPrintToKitchen(Presenter.MasterOrder, false);
                }
                else
                    Presenter.HandleHoldClick();
                Defaults.PerformanceLog.Add("Direct Card Payment Started    -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Presenter.HandleDirectPaymentClick(true);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        private void BtnSwish_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Defaults.PerformanceLog.Add("Clicked On direct Swish payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                IsFromHold = false;
                if (isMerge && mergeOrder != null)
                    SetMergeOrder();
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = Presenter.MasterOrder.OrderStatusFromType;
                    if (selectedTable != null)
                    {
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                        Presenter.MasterOrder.Comments = selectedTable.Name;
                    }
                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.CheckOutUserId = Defaults.User.Id;
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.MasterOrder = Presenter.OrderEntry(Presenter.MasterOrder);
                    //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                    //    Presenter.SendPrintToKitchen(Presenter.MasterOrder, false);
                }
                else
                    Presenter.HandleHoldClick();
                Defaults.PerformanceLog.Add("Direct Swish Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Presenter.HandelDirectSwishPaymentClick(true);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        private void BtnStudentCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Defaults.PerformanceLog.Add("Clicked On direct studentkort payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                IsFromHold = false;
                if (isMerge && mergeOrder != null)
                    SetMergeOrder();
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = Presenter.MasterOrder.OrderStatusFromType;
                    if (selectedTable != null)
                    {
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                        Presenter.MasterOrder.Comments = selectedTable.Name;
                    }
                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.CheckOutUserId = Defaults.User.Id;
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.MasterOrder = Presenter.OrderEntry(Presenter.MasterOrder);

                    //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                    //    Presenter.SendPrintToKitchen(Presenter.MasterOrder, false);
                }
                else
                    Presenter.HandleHoldClick();
                Defaults.PerformanceLog.Add("Direct Studentkort Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Presenter.HandelDirectSwishPaymentClick(false);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        private void txtNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedProduct == null)
                EntryMode = EntryModeType.CodeEntry;
            else
            {
                if (!(txtNumber.Text.Length == 13 && txtNumber.Text.StartsWith("7388")))
                {
                    if (!(txtNumber.Text.Length == 13 && txtNumber.Text.StartsWith("9899")))
                    {
                        if (!txtNumber.Text.StartsWith("2")
                            && txtNumber.Text.StartsWith("20")
                            && !txtNumber.Text.StartsWith("21")
                            && !txtNumber.Text.StartsWith("22")
                            && !txtNumber.Text.StartsWith("23")
                            && !txtNumber.Text.StartsWith("24")
                            && !txtNumber.Text.StartsWith("25")
                            && !txtNumber.Text.StartsWith("28"))
                        {
                            if (txtNumber.Text.Length >= 10)
                            {
                                MessageBox.Show("Pris för högt", Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                txtNumber.Text = "";
                                return;
                            }
                            EntryMode = EntryModeType.ItemEntry;
                        }
                    }
                }
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtNumber.Focus();
        }

        private void popupTable_Loaded(object sender, RoutedEventArgs e)
        {
            PopupTable.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;

        }
        Order mergeOrder;
        List<Order> mergeOrders;
        bool isMerge = false;
        private void TableOrderMergTo_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button).DataContext as Order;
            if (order != null)
            {
                mergeOrder = order;
                PopupTable.IsOpen = false;
                isMerge = true;

                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = OrderStatus.AssignedKitchenBar;
                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.Type = OrderType.TableOrder;
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);

                    Presenter.OrderEntry(Presenter.MasterOrder);

                }
            }

        }

        private Order MergeOrders(List<Order> selectedOrders, FoodTable selectedTable, OrderType type)
        {
            Guid orderId = default(Guid);
            Presenter.DiscardOrder(Presenter.MasterOrder.Id);
            var orderLines = new List<OrderLine>();
            try
            {

                List<Guid> itemForLogs = new List<Guid>();
                using (var db = PosState.GetInstance().Context)
                {

                    decimal orderTotal = selectedOrders.Sum(c => c.OrderTotal);
                    string bong = selectedOrders.Select(c => c.Bong).FirstOrDefault();
                    string dailybong = selectedOrders.Select(c => c.DailyBong).FirstOrDefault();

                    foreach (var currentOrder in selectedOrders) // adding all selected orders in orderLines list
                    {
                        var lines = db.OrderDetail.Where(ol => ol.OrderId == currentOrder.Id && ol.Active == 1).ToList();
                        foreach (var line in lines)
                        {
                            line.ItemId = line.ItemId;
                            orderLines.Add(line);
                        }

                    }

                    if (orderLines.Count > 0)
                    {
                        var newLines = new List<OrderLine>();

                        var groups = orderLines.Where(c => c.GroupKey == null).GroupBy(g => new { g.ItemId, g.UnitPrice, g.GroupId }).ToList();
                        var groupItemDiscount = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice, g.GroupId }).ToList();

                        foreach (var grp in groups) /*Summation of Items*/
                        {
                            var item = grp.First();
                            item.Quantity = grp.Sum(s => s.Quantity);
                            newLines.Add(item);

                            var groupIngredients = orderLines.Where(c => c.GroupId == item.ItemId && c.GroupKey != null).GroupBy(g => new { g.ItemId, g.UnitPrice, g.GroupId }).ToList();

                            foreach (var grp2 in groupIngredients) /*Summation of Ingredients*/
                            {
                                var igredient = grp2.First();
                                igredient.GroupKey = item.Id;
                                igredient.Quantity = grp2.Sum(s => s.Quantity);
                                newLines.Add(igredient);
                            }
                        }



                        foreach (var grp in groupItemDiscount) /*Summation of ItemDiscount*/
                        {
                            var line = grp.First();
                            line.ItemDiscount = grp.Sum(s => s.ItemDiscount);
                            newLines.Add(line);
                        }



                        //var orderTotal = newLines.Select(s => new
                        //{
                        //    GrossTotal = s.UnitPrice * (s.Direction * s.Quantity)-s.ItemDiscount}).Sum(ol => ol.GrossTotal);

                        int lastNo = 0;
                        //  var total = newLines.Where(ol => ol.ItemType != ItemType.Grouped).Sum(s => s.GrossAmountDiscounted());
                        var ord = db.OrderMaster.OrderByDescending(o => o.CreationDate).Where(o => o.OrderNoOfDay != "" && o.CreationDate >= DateTime.Now).SingleOrDefault();
                        if (ord != null)
                        {
                            string[] orNo = ord.OrderNoOfDay.Split('-');
                            if (orNo.Length > 1)
                                int.TryParse(orNo[1], out lastNo);
                        }

                        var endDate = DateTime.Now.Day;
                        string OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + endDate + "-" + (lastNo + 1);

                        var order = new Order
                        {
                            Id = Guid.NewGuid(),
                            TableId = selectedTable.Id,
                            CreationDate = DateTime.Now,
                            OrderTotal = orderTotal, //total,
                            UserId = Defaults.User.Id,
                            Status = OrderStatus.AssignedKitchenBar,
                            ShiftNo = App.MainWindow.ShiftNo,
                            Updated = 1,
                            PaymentStatus = 2,
                            Comments = selectedTable.Name,
                            Type = type,
                            TerminalId = Defaults.Terminal.Id,
                            OutletId = Defaults.Terminal.Outlet.Id,
                            OrderNoOfDay = OrderNoOfDay,
                            Bong = bong,
                            DailyBong = dailybong
                        };
                        db.OrderMaster.Add(order);
                        var journal = new Journal
                        {
                            OrderId = order.Id,
                            ActionId = Convert.ToInt32(JournalActionCode.NewOrderEntry),
                            Created = DateTime.Now,
                            TerminalId = order.TerminalId
                        };
                        if (!string.IsNullOrEmpty(Defaults.User.Id))
                            journal.UserId = Defaults.User.Id;
                        db.Journal.Add(journal);
                        foreach (var line in newLines)
                        {
                            itemForLogs.Add(line.ItemId);
                            line.OrderId = order.Id;
                            var _journal = new Journal
                            {
                                OrderId = order.Id,
                                ItemId = line.ItemId,
                                ActionId = Convert.ToInt32(JournalActionCode.ItemAdded),
                                Created = DateTime.Now,
                                TerminalId = order.TerminalId
                            };
                            if (!string.IsNullOrEmpty(Defaults.User.Id))
                                _journal.UserId = Defaults.User.Id;
                            db.Journal.Add(_journal);

                        }
                        string[] ary = new string[selectedOrders.Count];

                        int i = 0;
                        Guid ordId;
                        foreach (var currentOrder in selectedOrders)
                        {
                            ary[i] = currentOrder.Id.ToString();
                            ordId = currentOrder.Id;
                            ////  orderRepo.Remove(currentOrder.GetOrderFrom());
                            i++;
                        }

                        foreach (string id in ary)
                        {
                            var id2 = Guid.Parse(id);
                            var _ord = db.OrderMaster.FirstOrDefault(o => o.Id == id2);
                            _ord.Status = OrderStatus.OrderCancelled;
                            var _journal = new Journal
                            {
                                Created = DateTime.Now,
                                OrderId = _ord.Id,
                                ActionId = Convert.ToInt16(JournalActionCode.OrderCancelled),
                                UserId = Defaults.User.Id,
                                TerminalId = _ord.TerminalId
                            };

                            db.Journal.Add(_journal);
                            //var journs=  journalRepo.Where(j=>j.OrderId== _ord.Id).ToList();
                            //  if (journs != null && journs.Count > 0)
                            //  {
                            //      foreach (var journ in journs)
                            //      {
                            //         // journ.OrderId = order.Id;
                            //      }
                            //  }

                        }
                        db.SaveChanges();
                        orderId = order.Id;

                    }
                }
                return new Order();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return null;
            }
        }
        private void SetMergeOrder()
        {
            Presenter.DiscardOrder(Presenter.MasterOrder.Id);
            type = OrderType.TableOrder;
            Presenter.MasterOrder = new Order
            {
                Id = mergeOrder.Id,
                CreationDate = mergeOrder.CreationDate,
                TableId = mergeOrder.TableId,
                Status = mergeOrder.Status,
                PaymentStatus = mergeOrder.PaymentStatus,
                OrderLines = mergeOrder.OrderLines,
                OrderNoOfDay = mergeOrder.OrderNoOfDay,
                ShiftNo = mergeOrder.ShiftNo,
                ShiftOrderNo = mergeOrder.ShiftOrderNo,
                OrderTotal = mergeOrder.OrderTotal,
                TaxPercent = mergeOrder.TaxPercent,
                UserId = mergeOrder.UserId,
                Updated = mergeOrder.Updated,
                Outlet = mergeOrder.Outlet,
                Bong = mergeOrder.Bong,
                DailyBong = mergeOrder.DailyBong,
                TerminalId = mergeOrder.TerminalId,
                TrainingMode = mergeOrder.TrainingMode,
                Type = mergeOrder.Type

            };

        }
        private void SetMergeOrders()
        {
            Presenter.DiscardOrder(Presenter.MasterOrder.Id);
            type = OrderType.TableOrder;
            Presenter.MasterOrder = new Order
            {
                Id = mergeOrder.Id,
                CreationDate = mergeOrder.CreationDate,
                TableId = mergeOrder.TableId,
                Status = mergeOrder.Status,
                PaymentStatus = mergeOrder.PaymentStatus,
                OrderLines = mergeOrder.OrderLines,
                OrderNoOfDay = mergeOrder.OrderNoOfDay,
                ShiftNo = mergeOrder.ShiftNo,
                ShiftOrderNo = mergeOrder.ShiftOrderNo,
                OrderTotal = mergeOrder.OrderTotal,
                TaxPercent = mergeOrder.TaxPercent,
                UserId = mergeOrder.UserId,
                Updated = mergeOrder.Updated,
                Outlet = mergeOrder.Outlet,
                Bong = mergeOrder.Bong,
                DailyBong = mergeOrder.DailyBong,
                TerminalId = mergeOrder.TerminalId,
                TrainingMode = mergeOrder.TrainingMode,
                Type = mergeOrder.Type

            };
            Presenter.OrderEntry(Presenter.MasterOrder);

        }
        bool isNewOrder = false;
        OrderType type = OrderType.Standard;
        private void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
            isNewOrder = true;
            isMerge = false;
            mergeOrder = null;
            type = OrderType.TableOrder;
            Presenter.MasterOrder.Type = type;
            Presenter.MasterOrder.SelectedTable = selectedTable;
            Presenter.MasterOrder.TableId = selectedTable.Id;
            Presenter.MasterOrder.Comments = selectedTable.Name;
            if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
            {
                Presenter.MasterOrder.Status = OrderStatus.AssignedKitchenBar;
                Presenter.MasterOrder.Updated = 1;
                Presenter.OrderEntry(Presenter.MasterOrder);
                if (Presenter.CurrentorderDetails.Count > 0)
                {
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                }

            }
        }

        private void CancelPopupButton_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
            Console.WriteLine("btnSelectTable.Content" + UI.Sales_SelectTableButton);
            ButtonOpenOrder.Content = UI.Sales_OpenOrderButton;

        }

        private void PopupTable_Closed(object sender, EventArgs e)
        {
            DataEntryGrid.IsEnabled = true;
        }

        private void Discount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ucCart.SelectedItem == null)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (ucCart.SelectedItem != null && ucCart.SelectedItem.Product.DiscountAllowed)
                {
                    if (!string.IsNullOrEmpty(Defaults.DiscountCode))
                    {
                        DiscountCodeWindow discountCodeWindow = new DiscountCodeWindow();
                        if (discountCodeWindow.ShowDialog() == false)
                            return;
                    }
                    DiscountWindow discountWindow = new DiscountWindow(ucCart.SelectedItem.Product.Price);
                    if (discountWindow.ShowDialog() == true)
                    {
                        if (ucCart.SelectedItem.ItemType == ItemType.Grouped)
                        {
                            var groupedItems = ucCart.SelectedItem.ItemDetails;
                            if (groupedItems != null)
                            {
                                foreach (var groupItem in groupedItems)
                                {
                                    decimal innetQty = groupItem.Quantity;
                                    decimal innetGrossTotal = groupItem.UnitPrice;
                                    decimal inneritemdiscount = 0;
                                    if (discountWindow.Percentage)
                                    {
                                        groupItem.DiscountPercentage = discountWindow.Amount;
                                        inneritemdiscount = (innetGrossTotal / 100) * groupItem.DiscountPercentage;
                                        innetGrossTotal = innetQty * innetGrossTotal;
                                    }
                                    else
                                    {
                                        // ucCart.SelectedItem.DiscountPercentage = (discountWindow.Amount * 100) / grossTotal;
                                        inneritemdiscount = discountWindow.Amount / groupedItems.Count;
                                    }
                                    if (Presenter.MasterOrder.Type == OrderType.Return)
                                        inneritemdiscount = (-1) * inneritemdiscount;
                                    groupItem.ItemDiscount = inneritemdiscount;
                                    groupItem.DiscountedUnitPrice = innetGrossTotal - inneritemdiscount;


                                }
                                ucCart.SelectedItem.ItemDetails = groupedItems;
                            }
                        }
                        decimal qunatity = ucCart.SelectedItem.Quantity;
                        decimal grossTotal = ucCart.SelectedItem.UnitPrice;
                        decimal itemdiscount = 0;

                        if (discountWindow.Percentage)
                        {
                            ucCart.SelectedItem.DiscountPercentage = discountWindow.Amount;
                            itemdiscount = (grossTotal / 100) * ucCart.SelectedItem.DiscountPercentage;
                            itemdiscount = qunatity * itemdiscount;
                        }
                        else
                        {
                            // ucCart.SelectedItem.DiscountPercentage = (discountWindow.Amount * 100) / grossTotal;f
                            itemdiscount = discountWindow.Amount;
                        }
                        // grossTotal = grossTotal - itemdiscount;
                        if (Presenter.MasterOrder.Type == OrderType.Return)
                            itemdiscount = (-1) * itemdiscount;
                        ucCart.SelectedItem.DiscountedUnitPrice = Presenter.MasterOrder.Type == OrderType.Return ? grossTotal + itemdiscount : grossTotal - itemdiscount;
                        grossTotal = grossTotal * qunatity;
                        ucCart.SelectedItem.ItemDiscount = itemdiscount;
                        ucCart.SelectedItem.DiscountDescription = UI.Sales_Discount;

                        if (Presenter.MasterOrder.Id != default(Guid) && Defaults.OrderEntryType == OrderEntryType.RecordAll)
                        {
                            Presenter.EditItem(ucCart.SelectedItem);
                        }


                        ucCart.SetCartItems2(Presenter.CurrentorderDetails);
                        //  ucCart.RefreshItems();
                        App.MainWindow.UpdateItems(Presenter.CurrentorderDetails);
                    }
                }
                txtNumber.Text = "";
                txtNumber.Focus();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        private void SplitOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Presenter.MasterOrder != null && Presenter.CurrentorderDetails.Count > 0)
            {
                if (Defaults.BONG && Presenter.MasterOrder.Status == OrderStatus.New)
                    Presenter.SendPrintToKitchen(Presenter.MasterOrder);
                var splitOrderWindow = new SplitOrderWindow(Presenter.MasterOrder, Presenter.CurrentorderDetails);

                if (splitOrderWindow.ShowDialog() == true)
                {
                    Presenter.CurrentorderDetails = new List<OrderLine>();
                    Presenter.MasterOrder = new Order();
                    SetCartItems(Presenter.CurrentorderDetails);
                    NewEntry();
                }
                else
                    DisplayOrder(Presenter.MasterOrder.Id);
            }
        }

        private void btnMobileCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Defaults.PerformanceLog.Add("Clicked On direct mobilekort payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                IsFromHold = false;
                if (isMerge && mergeOrder != null)
                    SetMergeOrder();
                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = Presenter.MasterOrder.OrderStatusFromType;
                    if (selectedTable != null)
                    {
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                        Presenter.MasterOrder.Comments = selectedTable.Name;
                    }
                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.CheckOutUserId = Defaults.User.Id;
                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.MasterOrder = Presenter.OrderEntry(Presenter.MasterOrder);

                    //if (Presenter.MasterOrder.Status != OrderStatus.ReturnOrder && Defaults.BONG)
                    //    Presenter.SendPrintToKitchen(Presenter.MasterOrder, false);
                }
                else
                    Presenter.HandleHoldClick();
                Defaults.PerformanceLog.Add("Direct Studentkort Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Presenter.HandelDirectMobileCardClick();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        public void SetCartItems(List<OrderLine> order, bool IsAddToCart = false)
        {

            ucCart.SetCartItems2(order, IsAddToCart);


        }

        public OrderLine GetSelectedOrderLine()
        {
            return selectedItem;
        }

        public void SetSelectedOrderLine(OrderLine orderLine)
        {
            selectedItem = orderLine;
        }

        public string GetAskPriceValue()
        {
            throw new NotImplementedException();
        }

        public List<Product> GetProductsList()
        {
            return lstProducts;
        }

        public EntryModeType GetEntryMode()
        {
            return EntryMode;
        }

        public void SetAskByUser(bool ask)
        {
            AskByUser = ask;
        }

        public bool GetAskByUser()
        {
            return AskByUser;
        }

        public void SetAskVolumeQty(int qty)
        {
            AskVolumeQty = qty;
        }

        public int GetAskVolumeQty()
        {
            return AskVolumeQty;
        }

        public void SetCurrentQty(decimal qty)
        {
            currentQty = qty;
        }

        public decimal GetCurrentQty()
        {
            return currentQty;
        }



        public void SetIsMerge(bool merge)
        {
            isMerge = merge;
        }

        public bool IsNewOrder()
        {
            return isNewOrder;
        }

        public int GetBIDSNO()
        {
            return BIDSNO;
        }

        public Order GetMergeOrder()
        {
            return mergeOrder;
        }

        public void SetMergeOrder(Order order)
        {
            mergeOrder = order;
        }

        public FoodTable GetSelectedTable()
        {
            return selectedTable;
        }

        public void SetSelectedTable(FoodTable table)
        {
            selectedTable = table;
        }

        public void SetTableButtonContent(string text)
        {
            ButtonOpenOrder.Content = text;
            // btnSelectTable.Content=text;
            TableButtonContent(text);
        }

        public void SetOrderCommentsVisibility(bool visibility, string comments)
        {
            txtOrderComment.Visibility = brOrderComments.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;
            txtOrderComment.Text = comments;
        }

        public void SetOrderTypeVisibility(bool visibility, string text)
        {
            lblOrderType.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;
            lblOrderType.Text = text;

        }

        public void SetOrderCustomerTypeVisibility(bool visibility, string text)
        {
            lblOrderTypeCustomer.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;
            lblOrderTypeCustomer.Text = text;
        }

        public void SetOrderTypeSecondaryVisibility(bool visibility)
        {
            lblOrderTypeSecondary.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;

        }

        public bool GetOrderTypeSecondaryVisibility()
        {
            return lblOrderTypeSecondary.Visibility == Visibility.Visible;
        }

        public void OpenMegerOrderDialog(List<Order> orders)
        {

        }

        private void btnBackSapace_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNumber.Text))
            {
                if (txtNumber.Text.Length > 1)
                    txtNumber.Text = txtNumber.Text.Remove(txtNumber.Text.Length - 1, 1);
                else
                    txtNumber.Text = "";
            }
        }





        private void btnSearchItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchProductWindow productWindow = new SearchProductWindow();
                if (productWindow.ShowDialog() == true)
                {

                    EntryMode = EntryModeType.ItemEntry;
                    var product = productWindow.SelectedProduct;
                    if (product.Id == default(Guid)) return;
                    if (!string.IsNullOrEmpty(lblPriceMessage.Text))
                    {
                        currentQty = 1;
                        txtNumber.Text = "";
                    }
                    if (!string.IsNullOrEmpty(txtNumber.Text))
                    {
                        if (txtNumber.Text.Length > 10)
                        {
                            MessageBox.Show(UI.Message_InvalidQty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            txtNumber.Focus();
                            return;
                        }
                        decimal.TryParse(txtNumber.Text, out currentQty);
                        if (currentQty == 0)
                        {
                            currentQty = 1;
                        }
                    }
                    selectedProduct = product;
                    var orderDetail = Presenter.CurrentorderDetails.FirstOrDefault(p => p.ItemId == product.Id && p.ItemStatus == 0 && p.BIDSNO == BIDSNO);
                    if (orderDetail != null && selectedProduct.AskPrice == false && selectedProduct.AskWeight == false)
                    {
                        Presenter.HandelAddProductClick(orderDetail, currentQty, BIDSNO);
                        return;
                    }
                    if (product.AskWeight)
                    {
                        //Weight From Scale
                        decimal weight = -1;
                        weight = GetWeightFromScale();
                        if (weight == -1)
                        {
                            AskByUser = true;
                            lblPriceMessage.Text = UI.EnterWeight;// "Ange weight :";
                            EntryMode = EntryModeType.PluEntry;
                            txtNumber.Text = "";
                            txtNumber.Focus();
                        }
                        else
                        {


                            if (selectedProduct.Unit == ProductUnit.g)
                            {
                                //Grams
                                currentQty = Convert.ToDecimal(weight);
                            }
                            else if (selectedProduct.Unit == ProductUnit.kg)
                            {
                                //Kilograms
                                var grams = Convert.ToDecimal(weight);
                                currentQty = grams / 1000;

                            }
                            else if (selectedProduct.Unit == ProductUnit.hg)
                            {
                                //Hectograms
                                var grams = Convert.ToDecimal(weight);
                                currentQty = grams / 100;
                            }
                            Presenter.AddItemToCart(product, currentQty, BIDSNO);
                            currentQty = 1;
                            AskByUser = false;
                        }
                    }
                    else if (product.AskPrice)
                    {
                        AskByUser = true;
                        lblPriceMessage.Text = UI.Sales_EnterPrice;// "Ange pris :";
                        EntryMode = EntryModeType.PluEntry;
                        txtNumber.Text = "";
                        txtNumber.Focus();
                    }
                    else
                    {
                        Presenter.AddItemToCart(product, currentQty, BIDSNO, false, true);
                        currentQty = 1;
                        AskByUser = false;
                    }

                }
            }
            catch (Exception ex)
            {
                ShowError(Defaults.AppProvider.AppTitle, ex.Message);
            }
        }

        public bool IsMerge()
        {
            return isMerge;
        }

        private void orderGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Order order = orderHistoryGrid.SelectedItem as Order;
            if (order != null)
            {
                orderHistoryGrid.UnselectAll();
                PrintInvoiceWindow invoice = new PrintInvoiceWindow(order.Id, true);
                invoice.ShowDialog();
            }
        }

        private void BtnPANT_Click(object sender, RoutedEventArgs e)
        {

            var product = productPresenter.GetProductByPANT("PANT");
            if (product != null)
            {
                product.Price = -20;
                var productaa = productPresenter.UpdateProduct(product, 15);
                Presenter.AddItemToCart(product, 1, 0);
            }
            else
            {
                //var tpnt = txtPANT.Text;
                //MessageBox.Show(tpnt, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);

            }


        }

        private void DirectSwishButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Presenter.CurrentorderDetails.Count == 0)
                {
                    MessageBox.Show(UI.PlanceOrder_CartEmpty, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                Defaults.PerformanceLog.Add("Clicked On direct swish payment-> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                IsFromHold = false;
                Presenter.MasterOrder.OrderLines = Presenter.CurrentorderDetails;
                if (Defaults.IsOpenOrder == false && Presenter.MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder)
                {
                    if (Defaults.BONG && Defaults.DirectBONG)
                    {
                        Presenter.SendPrintToKitchenWithoutReset(Presenter.MasterOrder);
                    }
                }

                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    Presenter.MasterOrder.Status = Presenter.MasterOrder.OrderStatusFromType;
                    if (selectedTable != null)
                    {
                        Presenter.MasterOrder.SelectedTable = selectedTable;
                        Presenter.MasterOrder.TableId = selectedTable.Id;
                        Presenter.MasterOrder.Comments = selectedTable.Name;
                    }
                    if (Presenter.MasterOrder.OrderStatusFromType == OrderStatus.ReturnOrder)// "ReturnOrder")
                    {
                        foreach (var det in Presenter.CurrentorderDetails)
                        {
                            if (det.Quantity < 0)
                                det.Quantity = -1 * det.Quantity; // conver in plus
                        }
                    }

                    Presenter.MasterOrder.Updated = 1;
                    Presenter.MasterOrder.CheckOutUserId = Defaults.User.Id;
                    if (string.IsNullOrEmpty(Presenter.MasterOrder.Bong) || Presenter.MasterOrder.Bong == "0")
                    {
                        int dailyBongCounter = 0;
                        InvoiceHandler invoiceHndler = new InvoiceHandler();
                        int bongNo = invoiceHndler.GetLastBongNo(out dailyBongCounter, true);
                        Presenter.MasterOrder.Bong = bongNo.ToString();
                        Presenter.MasterOrder.DailyBong = dailyBongCounter.ToString();
                    }

                    Presenter.UpdateOrderDetail(Presenter.CurrentorderDetails, Presenter.MasterOrder);
                    Presenter.MasterOrder = Presenter.OrderEntry(Presenter.MasterOrder);
                }
                else
                    Presenter.HandleHoldClick();
                
                Defaults.PerformanceLog.Add("Direct Swish Payment Started   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Presenter.HandleDirectPaymentClick(false, true);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
