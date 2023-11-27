using POSSUM.Model;
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
    /// Interaction logic for NewProductAddWindow.xaml
    /// </summary>
    public partial class NewProductAddWindow : Window
    {
        string Barcode = "";
        public Product selectedProduct = null;
        public NewProductAddWindow(string barcode)
        {
            InitializeComponent();
            Barcode = barcode;
            lblBarcode.Text = UI.Message_UnknowBarcode;
            lblConfirm.Text = UI.Message_AddConfirm;
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow productWindow = new AddProductWindow(Barcode);
            if (productWindow.ShowDialog() == true)
                selectedProduct = productWindow.currentProduct;

            if (selectedProduct == null)
                this.Close();
            else
                this.DialogResult = true;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            SearchProductWindow searchProductWindow = new SearchProductWindow(Barcode);
            if (searchProductWindow.ShowDialog() == true)
                selectedProduct = searchProductWindow.SelectedProduct;
            if (selectedProduct == null)
                this.Close();
            else
                this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
