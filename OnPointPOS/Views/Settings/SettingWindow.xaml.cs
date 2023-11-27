using POSSUM.Model;
using POSSUM.Presenters.Settings;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Windows;

namespace POSSUM
{
    public partial class SettingWindow : Window, ISettingView
    {
        SettingPresenter presenter;
        public SettingWindow()
        {
            InitializeComponent();
            presenter = new SettingPresenter(this);
            GetSetting();
        }
        private void GetSetting()
        {
            txtSymbole.Text = Defaults.SettingsList[SettingCode.CurrencySymbol];
            if (Defaults.TipStatus)
                rdbTipYes.IsChecked = true;
            else
                rdbTipNo.IsChecked = false;

        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSymbole.Text))
            {
                MessageBox.Show(UI.Message_RequiredField, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtSymbole.Focus();
                return;
            }
            Defaults.TipStatus = rdbTipYes.IsChecked == true;
            Defaults.SettingsList[SettingCode.CurrencySymbol] = txtSymbole.Text;
            var lst = new List<Setting> {
                new Setting { Code = SettingCode.Last_Executed, Value =DateTime.Now.ToString() }, 
                new Setting { Code = SettingCode.CurrencySymbol, Value = Defaults.SettingsList[SettingCode.CurrencySymbol] } };

            if (presenter.SaveSettings(lst))
                MessageBox.Show("Settings "+ UI.Message_Saved_Success, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(UI.Message_Saved_Fail, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);
        }
    }
}
