using POSSUM.Presenters.Payments;
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
    /// Interaction logic for PaymentTerminalUtil.xaml
    /// </summary>
    public partial class PaymentTerminalUtil : Window, IPaymentTerminalUtilView
    {
        PaymentTerminalUtilPresenter presenter = null;

        public PaymentTerminalUtil()
        {
            InitializeComponent();
            presenter = new PaymentTerminalUtilPresenter(this);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            presenter.HandleClosing();
            base.OnClosing(e);
        }

        private void ButtonNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                presenter.HandleKeypadKeyPress((string)((sender as Button).Content));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("ButtonNumber_Click  == > Payemnt termnal Configration window ==>> " + ex.Message);
                MessageBox.Show("Some thing wrong with payment terminal. = > " + ex.Message, "Payemnt Terminal",MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleCloseClick();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleOkClick();
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button b = sender as Button;
                presenter.HandleOptionClick((int)Char.GetNumericValue(b.Name.Last()));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("btnOption_Click  == > Payemnt termnal Configration window ==>> " + ex.Message);
                MessageBox.Show("Some thing wrong with payment terminal. = > " + ex.Message, "Payemnt Terminal", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        public void ShowOption(int optionNr, bool visible)
        {
            switch(optionNr)
            {
                case 1:
                    btnOption1.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 2:
                    btnOption2.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 3:
                    btnOption3.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 4:
                    btnOption4.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 5:
                    btnOption5.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 6:
                    btnOption6.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        public void ShowOption(int optionNr, string textValue, bool visible)
        {
            switch (optionNr)
            {
                case 1:
                    btnOption1.Content = textValue;
                    btnOption1.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 2:
                    btnOption2.Content = textValue;
                    btnOption2.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 3:
                    btnOption3.Content = textValue;
                    btnOption3.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 4:
                    btnOption4.Content = textValue;
                    btnOption4.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 5:
                    btnOption5.Content = textValue;
                    btnOption5.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 6:
                    btnOption6.Content = textValue;
                    btnOption6.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        public void ShowKeypad(bool visible)
        {
            uiKeypad.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public void ShowAbort(bool visible)
        {
            btnCancel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowOk(bool visible)
        {
            btnOk.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }


        public void ShowOk(string text, bool visible)
        {
            btnOk.Content = text;
            btnOk.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowResetMenu(string text, bool visible)
        {
            btnResetMenu.Content = text;
            btnResetMenu.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }


        public void SetInfoWindow(string infoText)
        {
            txtInfo.Text = infoText;
        }


        public void Close(bool success)
        {
            this.DialogResult = success;
            Close();
        }

        private void btnResetMenu_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleResetMenuClick();
        }
    }
}
