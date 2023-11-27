using POSSUM.Presenters.PromptInfo;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// Interaction logic for PromptInfoAmount.xaml
    /// </summary>
    public partial class PromptInfoAmount : Window, IPromptInfoAmountView
    {
        PromptInfoAmountPresenter presenter;

        public PromptInfoAmount()
        {
            InitializeComponent();
            presenter = new PromptInfoAmountPresenter(this);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpeningButtonNumber_Click(object sender, RoutedEventArgs e)
        {
            txtAmount.Text = txtAmount.Text + (sender as Button).Content;
        }

        private void OpeningbtnSave_Click(object sender, RoutedEventArgs e)
        {

           
        }



        private void OpeningBTNNotClearAll_Click(object sender, RoutedEventArgs e)
        {

        }

        public decimal GetCashAmount()
        {
            decimal cashAmount = 0;
            decimal.TryParse(txtAmount.Text, out cashAmount);
            return cashAmount;
        }

        public void ShowError(string message, string title)
        {
            
        }

        public void ClosePrompt()
        {
           this.Close();
        }

        public void SuccessMessage(string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void btnOptionWithDraw_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleDropCashSave();
        }

        private void btnOptionDeposit_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleAddCashSave();
        }
        private void ChangeCashDrawerClick(object sender, RoutedEventArgs e)
        {
            SwitchCashdrawer();
        }
        private void SwitchCashdrawer()
        {
            try
            {

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                //Defaults.CASH_GUARD = true;
                //config.AppSettings.Settings["CASH_GUARD"].Value = "1";
                Defaults.CASH_GUARD = false;
                config.AppSettings.Settings["CASH_GUARD"].Value = "0";
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                MessageBox.Show("Cash drawer changed successfully", Defaults.AppProvider.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                App.MainWindow.AddUserActivityMenu();
                this.Close();

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
