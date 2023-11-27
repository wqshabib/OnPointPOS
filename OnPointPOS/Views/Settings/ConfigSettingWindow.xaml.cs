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
    /// Interaction logic for ConfigSettingWindow.xaml
    /// </summary>
    public partial class ConfigSettingWindow : Window
    {
        ConfigSetting setting = new ConfigSetting();
        public ConfigSettingWindow()
        {
            InitializeComponent();
            FillLanguageCombo();
            FillSaleTypeCombo();
            FillCatLineCombo();
            FillItemLineCombo();
            ReadConfigSettings();

        }
        private void FillCatLineCombo()
        {
            List<ListDTO> lst = new List<ListDTO>();
            lst.Add(new ListDTO { Id = 1, Name = "1 Row" });
            lst.Add(new ListDTO { Id = 2, Name = "2 Rows" });
            lst.Add(new ListDTO { Id = 3, Name = "3 Rows" });
            cmbCatLines.ItemsSource = lst;
            if (cmbCatLines.Items.Count > 0)
                cmbCatLines.SelectedIndex = 0;
        }
        private void FillItemLineCombo()
        {
            List<ListDTO> lst = new List<ListDTO>();
            lst.Add(new ListDTO { Id = 2, Name = "2 Rows" });
            lst.Add(new ListDTO { Id = 4, Name = "4 Rows" });
            lst.Add(new ListDTO { Id = 5, Name = "5 Rows" });
            lst.Add(new ListDTO { Id = 6, Name = "6 Rows" });
            cmbItemLines.ItemsSource = lst;
            if (cmbItemLines.Items.Count > 0)
                cmbItemLines.SelectedIndex = 0;
        }
        private void ReadConfigSettings()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                setting.Language = Convert.ToInt16(config.AppSettings.Settings["Language"].Value);
                setting.SaleType = Convert.ToInt16(config.AppSettings.Settings["SaleType"].Value);
                setting.ItemLine = Convert.ToInt16(config.AppSettings.Settings["ItemLines"].Value);
                setting.CategoryLine = Convert.ToInt16(config.AppSettings.Settings["CategoryLines"].Value);
                setting.BONG = config.AppSettings.Settings["BONG"].Value == "1" ? true : false;
                setting.DirectCard = config.AppSettings.Settings["DirectCard"].Value == "1" ? true : false;
                setting.DirectCash = config.AppSettings.Settings["DirectCash"].Value == "1" ? true : false;
                setting.LogoEnable = config.AppSettings.Settings["LogoEnable"].Value == "1" ? true : false;
                layout.DataContext = setting;
                cmbLanguage.SelectedValue = setting.Language;
                cmbSaleType.SelectedValue = setting.SaleType;
                cmbCatLines.SelectedValue = setting.CategoryLine;
                cmbItemLines.SelectedValue = setting.ItemLine;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FillLanguageCombo()
        {
            var lst = Enum.GetValues(typeof(CurrentLanguage))
         .Cast<CurrentLanguage>()
         .Select(v => new ListDTO { Id = (int)v, Name = v.ToString() })
         .ToList();
            cmbLanguage.ItemsSource = lst;
            if (cmbLanguage.Items.Count > 0)
                cmbLanguage.SelectedIndex = 0;
        }
        private void FillSaleTypeCombo()
        {
            var lst = Enum.GetValues(typeof(SaleType))
         .Cast<SaleType>()
         .Select(v => new ListDTO { Id = (int)v, Name = v.ToString() })
         .ToList();
            cmbSaleType.ItemsSource = lst;
            if (cmbSaleType.Items.Count > 0)
                cmbSaleType.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                setting.CategoryLine = Convert.ToInt16(cmbCatLines.SelectedValue);
                setting.ItemLine = Convert.ToInt16(cmbItemLines.SelectedValue);
                setting.Language = Convert.ToInt16(cmbLanguage.SelectedValue);
                setting.SaleType = Convert.ToInt16(cmbSaleType.SelectedValue);
                config.AppSettings.Settings["Language"].Value = setting.Language.ToString();
                config.AppSettings.Settings["SaleType"].Value = setting.SaleType.ToString();
                config.AppSettings.Settings["CategoryLines"].Value = setting.CategoryLine.ToString();
                config.AppSettings.Settings["ItemLines"].Value = setting.ItemLine.ToString();
                config.AppSettings.Settings["BONG"].Value = setting.BONG ? "1" : "0";
                config.AppSettings.Settings["DirectCash"].Value = setting.DirectCash ? "1" : "0";
                config.AppSettings.Settings["DirectCard"].Value = setting.DirectCard ? "1" : "0";
                config.AppSettings.Settings["LogoEnable"].Value = setting.LogoEnable ? "1" : "0";
                config.Save(ConfigurationSaveMode.Modified);
                MessageBox.Show("Saved successfully", Defaults.AppProvider.Name, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    public class ConfigSetting
    {
        public int Language { get; set; }
        public int SaleType { get; set; }
        public int CategoryLine { get; set; }
        public int ItemLine { get; set; }
        public bool BONG { get; set; }
        public bool Takeaway { get; set; }
        public bool DirectCash { get; set; }
        public bool DirectCard { get; set; }
        public bool LogoEnable { get; set; }
        public string Currency { get; set; }
    }
    public class ListDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
