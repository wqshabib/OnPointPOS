using POSSUM.Model;
using POSSUM.Presenter.Products;
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

namespace POSSUM.Views.Products
{
    /// <summary>
    /// Interaction logic for ProductStockWindow.xaml
    /// </summary>
    /// 

    public partial class ProductStockWindow : Window
    {
        List<StockModel> stockes = new List<StockModel>();
        public ProductStockWindow()
        {
            InitializeComponent();
            ProductPresenter presenter = new ProductPresenter();
            ProductDataGrid.ItemsSource = stockes = presenter.GetItemStock();
        }

       
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ProductPresenter presenter = new ProductPresenter();
            ProductDataGrid.ItemsSource = stockes = presenter.GetItemStock();
        }

        private void txtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchBox.Text))
                ProductDataGrid.ItemsSource = stockes.Where(p => p.ItemName.ToLower().Contains(txtSearchBox.Text.ToLower())).ToList();
            else
                ProductDataGrid.ItemsSource = stockes;
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
