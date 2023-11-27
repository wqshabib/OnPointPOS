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
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Presenter.Products;
using POSSUM.Events;

using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window, IProductView
    {
        ProductPresenter presenter;
        public Product currentProduct = null;
        public event UploadProductEventHandler UploadProduct;
        public event DownloadProductEventHandler DownloadProduct;
        public AddProductWindow(string barcode)
        {
            InitializeComponent();
            UploadProduct += AddProductWindow_UploadProduct;
            //  DownloadProduct += AddProductWindow_DownloadProduct;
            presenter = new ProductPresenter(this);
            FillComboBox();
            // LoadTreeView();
            cmbVat.SelectedValue = 12;
            cmbAccounting.SelectedValue = 2;
            currentProduct = new Product();
            currentProduct.DiscountAllowed = true;
            currentProduct.BarCode = barcode;
            layoutGrid.DataContext = currentProduct;
            if (string.IsNullOrEmpty(barcode))
                txtBarcode.Focus();
            else
                txtDescription.Focus();
        }
        public AddProductWindow(Product prodct)
        {
            InitializeComponent();
            UploadProduct += AddProductWindow_UploadProduct;
            //DownloadProduct += AddProductWindow_DownloadProduct;
            presenter = new ProductPresenter(this);
            currentProduct = prodct;
            FillComboBox();
            // LoadTreeView();
            cmbVat.SelectedValue = prodct.Tax;
            cmbAccounting.SelectedValue = prodct.AccountingId;
            var catId = presenter.GetCategoryByProduct(prodct.Id);
            cmbCategory.SelectedValue = catId;

            currentProduct.DiscountAllowed = prodct.DiscountAllowed;
            layoutGrid.DataContext = currentProduct;
            txtUnitPrice.Text = currentProduct.Price.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
            txtStockQuantity.Text = currentProduct.StockQuantity.ToString();
            txtMinStockQuantity.Text = currentProduct.MinStockLevel == null ? "" : currentProduct.MinStockLevel.ToString();
        }
        private void LoadTreeView()
        {
            //trvCategories.ItemsSource = presenter.GetCategoryHierarichy();
        }
        private void AddProductWindow_DownloadProduct(object sender)
        {
            presenter.DownloadProduct();
        }

        private void AddProductWindow_UploadProduct(object sender, Product product, int categoryId)
        {
            presenter.UploadProduct(product, categoryId);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProduct();
        }
        private void FillComboBox()
        {
            try
            {
                //Fill Category Combo
                var categories = presenter.GetCategories();
                cmbCategory.ItemsSource = categories;
                if (cmbCategory.Items.Count > 0)
                    cmbCategory.SelectedIndex = 0;

                //Fill Unit Combo
                var units = presenter.FillUnitType();
                cmbUnit.ItemsSource = units;
                if (cmbUnit.Items.Count > 0)
                    cmbUnit.SelectedIndex = 0;

                if (currentProduct != null)
                {
                    var unit = units.FirstOrDefault(a => a.Text == currentProduct.Unit.ToString());
                    if (unit!=null)
                    {
                        cmbUnit.SelectedItem = unit;
                    }
                }

                //Fill Preparation Combo
                var preparations = presenter.FillPreparationTime();
                cmbPreparationTime.ItemsSource = preparations;
                if (cmbPreparationTime.Items.Count > 0)
                    cmbPreparationTime.SelectedIndex = 0;
                //Fill VAT Combo
                var vats = presenter.GetTaxes();
                cmbVat.ItemsSource = vats;
                if (cmbVat.Items.Count > 0)
                    cmbVat.SelectedIndex = 0;

                //Fill Accounting Combo

                var accountings = presenter.GetAccountings();
                cmbAccounting.ItemsSource = accountings;
                if (cmbAccounting.Items.Count > 0)
                    cmbAccounting.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex.ToString());
            }
        }


        private void SaveProduct()
        {
            try
            {
                AddProduct();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //LogWriter.LogWrite(ex);
            }
        }

        void AddProduct()
        {
            try
            {
                currentProduct.BarCode = txtBarcode.Text;
                currentProduct.Description = txtDescription.Text;
                if (!string.IsNullOrEmpty(txtStockQuantity.Text))
                    currentProduct.StockQuantity = decimal.Parse(txtStockQuantity.Text.ToString());

                if (!string.IsNullOrEmpty(txtMinStockQuantity.Text))
                    currentProduct.MinStockLevel = decimal.Parse(txtMinStockQuantity.Text.ToString());
                // txtUnitPrice.Text= txtUnitPrice.Text.Replace(",", ".");
                decimal unitPrice = 0;


                // unitPrice = Convert.ToDecimal(txtUnitPrice.Text, Defaults.UICultureInfoWithoutCurrencySymbol);
                //var price = txtUnitPrice.Text.Replace(".", ",");
                //unitPrice = Convert.ToDecimal(price, Defaults.UICultureInfo);


                if (string.IsNullOrEmpty(currentProduct.Description))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                if (string.IsNullOrEmpty(txtUnitPrice.Text))
                {
                    MessageBox.Show(UI.Sales_EnterPrice, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                unitPrice = decimal.Parse(txtUnitPrice.Text, NumberStyles.Currency, Defaults.UICultureInfo);
                currentProduct.Price = unitPrice;
                //if (string.IsNullOrEmpty(currentProduct.BarCode))
                //{
                //    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}


                int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);
                // currentProduct.Tax = Convert.ToDecimal(cmbVat.SelectedValue);
                var accounting = cmbAccounting.SelectedItem as Accounting;
                currentProduct.Tax = accounting.TAX;
                currentProduct.AccountingId = Convert.ToInt32(cmbAccounting.SelectedValue);
                currentProduct.Unit = (ProductUnit)Enum.Parse(typeof(ProductUnit), (cmbUnit.SelectedItem as EnumValue).Text);
                currentProduct.PreparationTime = (PrepareTime)Enum.Parse(typeof(PrepareTime), (cmbPreparationTime.SelectedItem as EnumValue).Text);
                // var categories = trvCategories.ItemsSource as List<Category>;
                currentProduct.Updated = DateTime.Now.AddMinutes(-5);
                /* script to get selected categories if we use Treeview for Category selection 
                var itemCategories = new List<Category>();
                foreach (var item in trvCategories.Items)
                {

                    var model = item as CategoryModel;
                    if (model.IsSelected)
                        itemCategories.Add(new Category { Id = model.Id, Name = model.Name });
                    var childrens = model.Children.Where(ch => ch.IsSelected).Select(ch => new Category {Id=ch.Id,Name=ch.Name }).ToList();
                    if (childrens.Count > 0)
                        itemCategories.AddRange(childrens);
                }
               */
                if (currentProduct.Id == default(Guid))
                {
                    Product product = presenter.SaveProduct(currentProduct, categoryId);
                    if (product.Id != default(Guid))
                    {
                        currentProduct = product;

                        var progressDialog = new ProgressWindow();
                        var backgroundThread = new Thread(
                            new ThreadStart(() =>
                            {
                                if (UploadProduct != null)
                                {
                                    UploadProduct(this, product, categoryId);
                                }
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
                        this.DialogResult = true;
                    }
                }
                else
                {
                    Product product = presenter.UpdateProduct(currentProduct, categoryId);
                    if (UploadProduct != null)
                    {
                        UploadProduct(this, product, categoryId);
                    }
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }
        private void BtnCloseAccountInfo_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SaveProduct();
        }

        public Product GetProduct()
        {
            return currentProduct;
        }

        private void txtUnitPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            decimal val = 0;
            e.Handled = !decimal.TryParse(e.Text, out val);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var progressDialog = new ProgressWindow();
                var backgroundThread = new Thread(
                    new ThreadStart(() =>
                    {
                        if (DownloadProduct != null)
                            DownloadProduct(this);
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
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void txtUnitPrice_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

        private void txtUnitPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            //string result = "";
            //char[] validChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '.' }; // these are ok
            //foreach (char c in txtUnitPrice.Text) // check each character in the user's input
            //{
            //    if (Array.IndexOf(validChars, c) != -1)
            //        result += c; // if this is ok, then add it to the result
            //}

            //txtUnitPrice.Text = result;

            // txtUnitPrice.Text = Regex.Replace(txtUnitPrice.Text, "[^0-9]+", ".");
            //string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.,]", "");
            //((TextBox)sender).Text = s;

        }

        private void txtMinStockQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtMinStockQuantity_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    }
}
