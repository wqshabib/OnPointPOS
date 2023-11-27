using POSSUM.Res;
using POSSUM.Views.Sales;
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
    public partial class LanguageWindow : Window
    {
        ConfigSetting setting = new ConfigSetting();
        public LanguageWindow()
        {
            InitializeComponent();
            LoadLanguage();
           // LoadCurrency();
            cmbLanguage.SelectedValue = ((CultureInfo)Defaults.UICultureInfo).Name;
        }
        private void LoadLanguage()
        {
            List<LanguageModel> languages = new List<LanguageModel>();
            languages.Add(new LanguageModel { Id = "sv-SE", Name = "Swedish" });
            languages.Add(new LanguageModel { Id = "en-US", Name = "English" });
           languages.Add(new LanguageModel { Id = "ar-SA", Name = "Arabic" });

            cmbLanguage.ItemsSource = languages;
            cmbLanguage.SelectedIndex = 0;

        }
        private void LoadCurrency()
        {
            List<LanguageModel> languages = new List<LanguageModel>();
            languages.Add(new LanguageModel { Id = "sv-SE", Name = "SEK" });
            languages.Add(new LanguageModel { Id = "en", Name = "US$" });
            languages.Add(new LanguageModel { Id = "ar-SA", Name = "Riyal" });

            cmbCurrency.ItemsSource = languages;
            cmbCurrency.SelectedIndex = 0;

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
          

            try
            {
                var lang = cmbLanguage.SelectedValue.ToString();
                App.MainWindow.SetLanguage(lang);
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                int langId = cmbLanguage.SelectedValue.ToString() == "sv-SE" ? 1 : cmbLanguage.SelectedValue.ToString() == "en-US" ? 2 : 4;
            Defaults.Language = (CurrentLanguage)Enum.Parse(typeof(CurrentLanguage), langId.ToString());
               
                setting.Language = langId;
             //   setting.Currency = Convert.ToString(cmbCurrency.SelectedValue);
             //   Defaults.CurrencyCultureInfo = new CultureInfo(setting.Currency);
                config.AppSettings.Settings["Language"].Value = setting.Language.ToString();
               // config.AppSettings.Settings["Currency"].Value = setting.Currency.ToString();

              
                config.Save(ConfigurationSaveMode.Modified);
                MessageBox.Show("Language changes successfully", Defaults.AppProvider.Name, MessageBoxButton.OK, MessageBoxImage.Information);

                 App.MainWindow.AddControlToMainCanvas(new UCSale());
               
                App.MainWindow.AddUserActivityMenu();
                this.Close();

            }
            catch (Exception ex)
            {
                //LogWriter.LogWrite(ex);
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }
    }
    public class LanguageModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
