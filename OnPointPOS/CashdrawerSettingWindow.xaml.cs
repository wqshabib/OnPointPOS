using POSSUM.Integration;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
    /// Interaction logic for LanguageWindow.xaml
    /// </summary>
    public partial class CashdrawerSettingWindow : Window
    {
        ConfigSetting setting = new ConfigSetting();
        public CashdrawerSettingWindow()
        {
            InitializeComponent();
            if (Defaults.CASH_GUARD)
                rdbCashGuard.IsChecked = Defaults.CASH_GUARD;
            else
                rdbCashDrawer.IsChecked = true;
          //  LoadTypes();
        }
        private void LoadTypes()
        {
            

            cmbType.ItemsSource = FillTypes();
            cmbType.SelectedIndex = 0;
            cmbType.SelectedValue = (int)Defaults.CashDrawerType;
            txtPortNo.Text = Defaults.CashDrawerHardwarePort.ToString();

        }
        private List<CashDrawerTypeModel> FillTypes()
        {
            IEnumerable<CashDrawerType> unitTypes = Enum.GetValues(typeof(CashDrawerType))
                                                       .Cast<CashDrawerType>();

            var types = (from enumValue in unitTypes
                        select new CashDrawerTypeModel
                        {
                            Name = enumValue.ToString(),
                            Id = (int)enumValue

                        }).ToList();
            return types;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
          
            
            try
            {
                //short portNo = 0;
                //short.TryParse(txtPortNo.Text, out portNo);
                //if (portNo == 0)
                //{
                //    MessageBox.Show("Enter a valid port no", Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}
                //var typeId = cmbType.SelectedValue.ToString();
             
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


                //Defaults.CashDrawerType = (CashDrawerType)Enum.Parse(typeof(CashDrawerType), typeId.ToString());

                //    Defaults.CashDrawerHardwarePort = portNo;
                //   setting.Currency = Convert.ToString(cmbCurrency.SelectedValue);
                //   Defaults.CurrencyCultureInfo = new CultureInfo(setting.Currency);
                // config.AppSettings.Settings["CashDrawerType"].Value = Defaults.CashDrawerType.ToString();
                //  config.AppSettings.Settings["CashDrawerHardwarePort"].Value =txtPortNo.Text;

                /*
                Defaults.CASH_GUARD = rdbCashGuard.IsChecked == true ? true : false;
                config.AppSettings.Settings["CASH_GUARD"].Value = Defaults.CASH_GUARD?"1":"0";
                */
                Defaults.CASH_GUARD = false;
                config.AppSettings.Settings["CASH_GUARD"].Value ="0";
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                MessageBox.Show("Cash drawer changes successfully", Defaults.AppProvider.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                if (Defaults.CASH_GUARD==false)
                {
                    PosState.OpenDrawer();
                }

                this.Close();

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
          

        }
    }
    public class CashDrawerTypeModel
    {
        public int Id { get; set; }
        public string   Name { get; set; }
    }
}
