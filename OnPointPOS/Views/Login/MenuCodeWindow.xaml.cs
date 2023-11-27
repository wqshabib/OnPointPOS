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
    public partial class MenuCodeWindow : Window
    { 
        
        public MenuCodeWindow()
        {
            InitializeComponent();
            txtMenuPinCode.Focus();
            txtMenuPinCode.HorizontalAlignment = HorizontalAlignment.Center;
            txtMenuPinCode.VerticalAlignment = VerticalAlignment.Center;
        }
       
        private void btnMenuPinClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuPinYesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtMenuPinCode.Password))
            {
                if (txtMenuPinCode.Password == Defaults.MenuPinCode)
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

        private void ButtonMenuPin_Click(object sender, RoutedEventArgs e)
        {
            txtMenuPinCode.Password = txtMenuPinCode.Password + (sender as Button).Content;
        }

        private void ButtonMenuPinClear_Click(object sender, RoutedEventArgs e)
        {
            txtMenuPinCode.Password = "";
        }

       
    }
}
