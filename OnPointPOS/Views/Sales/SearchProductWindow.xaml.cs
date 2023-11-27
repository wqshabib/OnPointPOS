using POSSUM.Presenter.Products;
using POSSUM.Model;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;

namespace POSSUM
{

    public partial class SearchProductWindow : Window, IProductView
    {
        //public string SelectedTableName = "Select Table";
        public bool isTakeaway = false;
        ProductPresenter presenter;
        string BarCode = "";
        List<Product> products;
        public Product SelectedProduct { get; set; }

        public SearchProductWindow()
        {
            InitializeComponent();
            //SelectedTableName = UI.Sales_SelectTableButton;
            presenter = new ProductPresenter(this);
            products = new List<Product>();
            loadProduct();

        }

        public SearchProductWindow(string barCode)
        {
            InitializeComponent();
            BarCode = barCode;
            presenter = new ProductPresenter(this);
            products = new List<Product>();
            loadProduct();

        }


        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public void SetFloors(List<Floor> floors)
        {

        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ProductDataGrid.ItemsSource = products = presenter.GetProductsBykeyword(txtSearchBox.Text);
        }
        void loadProduct()
        {
            try
            {
                ProductDataGrid.ItemsSource = products = presenter.GetProductsBykeyword("").ToList();//(txtSearchBox.Text);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message);
                //LogWriter.LogWrite(ex);

            }

        }
        public string GetKeyword()
        {
            return txtSearchBox.Text;
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedProduct = ProductDataGrid.SelectedItem as Product;
            if (SelectedProduct != null)
                OKButton.IsEnabled = true;
            else
                OKButton.IsEnabled = false;
        }

        private void BtnCloseAccountInfo_OnClick(object sender, RoutedEventArgs e)
        {

        }
        private void popupError_Loaded(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
        }
        private void txtSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.IsOpen = true;
        }

        private void txtSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.IsOpen = false;
        }



        private void txtSearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupKeyborad.IsOpen = false;
                ProductDataGrid.ItemsSource = products = presenter.GetProductsBykeyword(txtSearchBox.Text);
            }
        }


        public Product GetProduct()
        {
            return SelectedProduct;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow("");
            addProductWindow.ShowDialog();
            loadProduct();
        }

        private async void txtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                
                //txtSearchBox.Delay = 1000;
                TextBox tb = (TextBox)sender;
                int startLength = tb.Text.Length;

                await Task.Delay(300);

                if (startLength == tb.Text.Length)

                    if (!string.IsNullOrEmpty(txtSearchBox.Text))
                    {
                        if (products == null && products.Count == 0)
                            return;
                        string keyword = txtSearchBox.Text.ToLower();
                        ProductDataGrid.ItemsSource = products.Where(itm => itm.Description.ToLower().Contains(keyword) || itm.PLU == keyword || (itm.BarCode != null && itm.BarCode.Contains(keyword))).ToList();
                    }
                    else
                    {
                        ProductDataGrid.ItemsSource = products;
                    }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            SelectedProduct = (sender as Button).DataContext as Product;
            if (SelectedProduct != null)
            {
               // SelectedProduct.BarCode = BarCode;
                AddProductWindow addProductWindow = new AddProductWindow(SelectedProduct);
                if (addProductWindow.ShowDialog() == true)
                {
                    loadProduct();
                }
            }
        }
    }
}
