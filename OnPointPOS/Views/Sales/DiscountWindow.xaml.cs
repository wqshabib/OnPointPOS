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
    /// Interaction logic for DiscountWindow.xaml
    /// </summary>
    public partial class DiscountWindow : Window
    {
        public decimal Amount { get; set; }
        public decimal OriginalPrice { get; set; }
        public bool Percentage { get; set; }
        bool completeOrderDiscount = false;
        //public DiscountWindow()
        //{
        //    InitializeComponent();
        //    rdbItemPrice.IsEnabled = false;
        //    completeOrderDiscount = true;
        //}
        public DiscountWindow(decimal originalPrice)
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
            try
            {
                if (!string.IsNullOrEmpty(txtDiscount.Text))
                {
                    if (rdbItemPrice.IsChecked == true)
                    {
                        Percentage = false;
                        Amount = OriginalPrice - Convert.ToDecimal(txtDiscount.Text);

                    }
                    else
                    {
                        Percentage = rdbDiscontInPercentage.IsChecked == true ? true : false;
                        Amount = Convert.ToDecimal(txtDiscount.Text);
                    }
                    //if (Amount > OriginalPrice && completeOrderDiscount == false)
                    //{
                    //    App.MainWindow.ShowError(UI.Main_InvalidAmount, "Rabattbeloppet är större än produktpriset");
                    //    return;
                    //}

                    if(Percentage==true && Amount>100)
                    {
                        MessageBox.Show("Percentage cannot be greater then 100.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (Percentage == false && Amount > OriginalPrice)
                    {
                        MessageBox.Show("Amount cannot be greater then " + Math.Round(OriginalPrice,2) +" (Discount allowed on items)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
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
            txtPrice.Visibility = Visibility.Visible;

        }

        private void rdbItemPrice_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPrice.Visibility = Visibility.Collapsed;
        }
    }
}
