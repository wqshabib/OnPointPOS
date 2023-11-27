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
    /// Interaction logic for PriceChangeWindow.xaml
    /// </summary>
    public partial class PriceChangeWindow : Window
    {
        public PriceChangeWindow()
        {
            InitializeComponent();

        }
        public decimal Amount { get; set; }
        public decimal OriginalPrice { get; set; }

        public PriceChangeWindow(decimal originalPrice)
        {
            InitializeComponent();
            OriginalPrice = originalPrice;
            txtPrice.Text = UI.Global_ActualPrice + ": " + OriginalPrice;
        }
        private void btnDiscountClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DiscountYesButton_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtDiscount.Text))
            {
                Amount = Convert.ToDecimal(txtDiscount.Text);

                this.DialogResult = true;
            }
        }

        private void ButtonDiscount_Click(object sender, RoutedEventArgs e)
        {
            txtDiscount.Text = txtDiscount.Text + (sender as Button).Content;
        }

        private void ButtonDiscountClear_Click(object sender, RoutedEventArgs e)
        {
            txtDiscount.Text = "";
        }

        private void rdbItemPrice_Checked(object sender, RoutedEventArgs e)
        {
            // txtPrice.Visibility = Visibility.Visible;

        }

        private void rdbItemPrice_Unchecked(object sender, RoutedEventArgs e)
        {
           // txtPrice.Visibility = Visibility.Collapsed;
        }

        private void rdbActualPrice_Checked(object sender, RoutedEventArgs e)
        {
             txtDiscount.Text = OriginalPrice.ToString();
        }
    }
}
