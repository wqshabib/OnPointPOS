using POSSUM.Presenters.Login;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POSSUM.Views.Login
{
    /// <summary>
    /// Interaction logic for UCLogin.xaml
    /// </summary>
    public partial class UCLogin : UserControl, ILoginView
    {

        LoginPresenter presenter;
        private bool isNewUser = false;
        public UCLogin()
        {

            InitializeComponent();
            presenter = new LoginPresenter(this);
            App.MainWindow.btnQuitApp.Visibility = Visibility.Visible;
            App.MainWindow.btnUserInfo.Visibility = Visibility.Collapsed;

#if (DEBUG)
            txtUserName.Text = "111";
            txtPassword.Password = "123456";
#endif
        }

        public string GetUsername()
        {
            return txtUserName.Text;
        }

        public void SetFocusUsername()
        {
            txtUserName.Focus();
        }

        public string GetPassword()
        {
            return txtPassword.Password;
        }

        public void SetPassword(string password)
        {
            txtPassword.Password = password;
        }

        public void SetFocusPassword()
        {
            txtPassword.Focus();
        }

        public void ShowOpening()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                openingViewBorder.Visibility = Visibility.Visible;
                loginViewBorder.Visibility = Visibility.Hidden;
            }));
        }

        public void ShowLogin()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                openingViewBorder.Visibility = Visibility.Hidden;
                loginViewBorder.Visibility = Visibility.Visible;
            }));
        }

        public bool GetConfirm()
        {
            return chkConfirm.IsChecked ?? false;
        }

        public decimal GetOpeningAmount()
        {
            decimal amount;
            if (decimal.TryParse(txtAmount.Text, out amount))
            {
                return decimal.Parse(txtAmount.Text);
            }
            return 0;
        }

        public void ShowIsClosed(bool isClosed)
        {
            txtStatusBar.Text = isClosed ? UI.Login_Closed : string.Empty;
        }

        public bool IsNewUser()
        {
            return isNewUser;
        }

        public void ShowError(string caption, string message)
        {
            App.MainWindow.ShowError(caption, message);
        }

        private void BTNLogin_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleLoginClick();
        }

        bool IsUserName = true;
        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsUserName)
            {
                txtUserName.Text += (sender as Button).Content;
                txtUserName.Focus();
                IsUserName = true;
            }
            else
            {
                txtPassword.Password += (sender as Button).Content;
                txtPassword.Focus();
                IsUserName = false;
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            txtUserName.Text = "";
            txtPassword.Password = "";
            txtUserName.Focus();
            IsUserName = true;
        }

        private void txtUserName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //decimal val = 0;
            //e.Handled = !decimal.TryParse(e.Text, out val);
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            //  e.Handled = false;
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            IsUserName = true;
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            IsUserName = false;
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                presenter.HandleLoginClick();
        }

        private void BackSapace_Click(object sender, RoutedEventArgs e)
        {
            if (IsUserName)
            {
                if (!string.IsNullOrEmpty(txtUserName.Text))
                {
                    if (txtUserName.Text.Length > 1)
                        txtUserName.Text = txtUserName.Text.Remove(txtUserName.Text.Length - 1, 1);
                    else
                        txtUserName.Text = "";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(txtPassword.Password))
                {
                    if (txtPassword.Password.Length > 1)
                        txtPassword.Password = txtPassword.Password.Remove(txtPassword.Password.Length - 1, 1);
                    else
                        txtPassword.Password = "";
                }
            }
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserName.Focus();
        }

        private void OpeningButtonNumber_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null) txtAmount.Text += button.Content;
        }

        private void OpeningBTNNotClearAll_Click(object sender, RoutedEventArgs e)
        {
            txtAmount.Text = "";
        }

        private void OpeningbtnSave_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleOpeningSave();
        }

    }
}
