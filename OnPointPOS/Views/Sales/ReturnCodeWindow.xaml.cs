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

namespace POSSUM.Views.Sales
{
    /// <summary>
    /// Interaction logic for ReturnCodeWindow.xaml
    /// </summary>
    public partial class ReturnCodeWindow : Window
    {
        public ReturnCodeWindow()
        {
            InitializeComponent();
        }

        private void btnReturnCodeClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnReturnCodeYesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtReturnCode.Password))
            {
                if (txtReturnCode.Password == Defaults.ReturnCode)
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

        private void BtnReturnCode_Click(object sender, RoutedEventArgs e)
        {
            txtReturnCode.Password = txtReturnCode.Password + (sender as Button).Content;
        }

        private void BtnReturnCodeClear_Click(object sender, RoutedEventArgs e)
        {
            txtReturnCode.Password = "";
        }

        
    }
}
