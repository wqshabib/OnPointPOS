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
    public partial class DiscountCodeWindow : Window
    { 
        
        public DiscountCodeWindow()
        {
            InitializeComponent();
        }
       
        private void btnDiscountClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DiscountYesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDiscount.Text))
            {
                if (txtDiscount.Text == Defaults.DiscountCode)
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter a correct code.", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    // App.MainWindow.ShowError(UI.Global_InvalidCode, UI.Global_InvalidCode);
                }
               
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

       
    }
}
