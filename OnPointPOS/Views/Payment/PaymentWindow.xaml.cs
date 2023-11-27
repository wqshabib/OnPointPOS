using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Presenters.Payments;
using POSSUM.PromptInfo;
using POSSUM.Res;
using POSSUM.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window, IPaymentView
    {
        public PaymentPresenter presenter;
        Guid orderId;
        public PaymentWindow(decimal totalAmount, decimal vatAmount, decimal cashbackAmount, PaymentTransactionStatus status, int tableId, Guid orderId)
        {
            try
            {
                InitializeComponent();
                this.orderId = orderId;
                LogWriter.LogWrite(" CP1 " + "orderId" + orderId + " totalAmount" + totalAmount + " vatAmount" + vatAmount);
            
                presenter = new PaymentPresenter(this, totalAmount, vatAmount, cashbackAmount, status, orderId);

                if (tableId > 0)
                {
                    txtTable.Dispatcher.BeginInvoke(new Action(() => txtTable.Text = UI.OpenOrder_TableButton + " " + tableId));
                }

                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.PaymentTerminalWindowOpen), orderId);

                var value = ConfigurationManager.AppSettings["ShowReprintButton"];
                if (value != null && value == "1")
                {
                    btnReprint.Visibility = Visibility.Visible;
                }
                else
                {
                    btnReprint.Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("CP1-PaymentWindow: " + ex);
                App.MainWindow.ShowError(UI.Report_Payment, ex.Message, true);
            }

            LogWriter.LogWrite(" CP2 ");
        }

        private void ButtonNumber_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleKeypadKeyPress((string)((sender as Button).Content));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            lbLoader.Visibility = Visibility.Visible;
            Cursor = Cursors.AppStarting;
            btnCancel.Visibility = Visibility.Collapsed;
            btnClose.Visibility = Visibility.Collapsed;
            SetStatusText("Var god vänta");
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.PaymentTerminalWindowCancel), orderId);
            presenter.HandleCancelClick();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            presenter.CancelOrder(orderId, Defaults.User.Id);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                Thread.Sleep(1000);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                CloseButton.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Collapsed;
                Cursor = Cursors.Arrow;
                presenter.SetPaymentDialogCloseForced(true);
                lbLoader.Visibility = Visibility.Collapsed;
                //presenter.HandleCancelClick();
                int waitSeconds = 5;
                var paymentWindowCloseWaitSeconds = ConfigurationManager.AppSettings["PaymentWindowCloseWaitSeconds"];
                if (!string.IsNullOrEmpty(paymentWindowCloseWaitSeconds))
                {
                    waitSeconds = Convert.ToInt32(waitSeconds);
                }

                for (int i = 0; i < waitSeconds; i++)
                {
                    Thread.Sleep(1000);
                }

                presenter.SetPaymentDialogCloseForced(true);
                this.DialogResult = false;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => Worker_RunWorkerCompleted" + ex);
            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleOkClick();
        }

        /*
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleOk();
        }
        
        private void btnCommentsCancel_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleCancel();
        }

        public void SetMultiline(bool multiline)
        {
            txtComments.AcceptsReturn = multiline;
        }

        public void SetTitle(string title)
        {
            txtTitle.Text = title;
        }

        public void SetDescription(string description)
        {
            txtDescription.Content = description;
        }

        public void SetValue(string value)
        {
            txtComments.Text = value;
        }


        public string GetValue()
        {
            return txtComments.Text;
        }

        public void CloseWithStatus(bool success)
        {            
            this.DialogResult = success;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWithStatus(false);
        }
         */
        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title);
        }

        public void ShowReconnect(bool res)
        {
            try
            {
                btnReconnect.Dispatcher.BeginInvoke(new Action(() => btnReconnect.Visibility = Visibility.Visible));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => ShowReconnect" + ex);
            }
        }



        public void ShowClose(bool res)
        {
            try
            {
                btnClose.Dispatcher.BeginInvoke(new Action(() => btnClose.Visibility = Visibility.Visible));
                btnCancel.Dispatcher.BeginInvoke(new Action(() => btnCancel.Visibility = Visibility.Collapsed));
                if (res)
                    this.Close();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => ShowClose" + ex);
            }
        }

        public void ShowWindowClose(bool res)
        {
            try
            {
                CloseButton.Dispatcher.BeginInvoke(new Action(() => CloseButton.Visibility = res ? Visibility.Visible : Visibility.Collapsed));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => ShowWindowClose" + ex);
            }
        }
        public void SetStatusText(string infoText)
        {
            try
            {
                txtTerminalInfo.Dispatcher.BeginInvoke(new Action(() => txtTerminalInfo.Text = infoText));
                LogWriter.LogWrite("SetStatusText => " + infoText);
                LogWriter.LogWrite(new Exception(" CP3 " + infoText));
                if (infoText != null && infoText.Trim() == "Medges ej Kontakta din kortutgivare")
                {
                    try
                    {
                        LogWriter.LogWrite("SetStatusText Calling Close thread => " + infoText);
                        System.Threading.Thread.Sleep(5000);
                        LogWriter.LogWrite("SetStatusText Calling Close thread after 5 seconds => " + infoText);

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                presenter.SetPaymentDialogCloseForced(true);
                                this.DialogResult = false;
                                this.Close();
                            }
                            catch (Exception ex)
                            {
                                LogWriter.LogWrite("Payment Window => SetStatusText Inner" + ex);
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogWrite("Payment Window => SetStatusText Inner 2" + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => SetStatusText" + ex);
            }
        }


        public void SetTerminalDisplay(string displayText, int? status, int? transactionType, int? transactionResult)
        {
            txtTerminalDisplay.Dispatcher.BeginInvoke(new Action(() => txtTerminalDisplay.Text = displayText));
        }

        public void SetErroTextForBambora(string text)
        {
            try
            {
                SetStatusText(text);
                LogWriter.LogWrite(new Exception(" CP3 " + text));

                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += Bg_DoWork;
                bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
                bg.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => SetErroTextForBambora" + ex);
            }
        }

        public void SetErroText(string text)
        {
            try
            {
                LogWriter.LogWrite(new Exception(" CP3 " + text));
                txtErrorMessage.Dispatcher.BeginInvoke(new Action(() => txtErrorMessage.Text = text));

                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += Bg_DoWork;
                bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
                bg.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => SetErroText" + ex);
            }
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                presenter.SetPaymentDialogCloseForced(true);
                this.DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => Bg_RunWorkerCompleted" + ex);
            }
        }

        public void SetPaymentText(string paymentText)
        {
            try
            {
                // txtInfo.Text = paymentText;
                LogWriter.LogWrite(new Exception(" CP3 " + paymentText));
                txtInfo.Dispatcher.BeginInvoke(new Action(() => txtInfo.Text = paymentText));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => SetPaymentText" + ex);
            }
        }


        public void CloseWithStatus(bool success)
        {
            try
            {
                this.DialogResult = success;
                this.Close();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(new Exception(" Payment Windows => CloseWithStatus " + ex));
            }
        }


        public void ShowKeypad(bool visible)
        {
            uiKeypad.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        public void ShowAbort(bool visible)
        {
            btnCancel.Visibility = Visibility.Visible;//:Visibility.Collapsed;// visible ? Visibility.Visible : Visibility.Hidden;
        }

        public void ShowOk(bool visible)
        {
            btnOk.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }


        public void ShowOk(string text, bool visible)
        {
            btnOk.Content = text;
            btnOk.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        public void ShowAbort(string text, bool visible)
        {
            btnCancel.Content = text;
            btnCancel.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1000);
            }

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                presenter.HandleClosing();
                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => OnClosing" + ex);
            }
        }


        public void ShowOption(string text, bool visible)
        {
            try
            {
                btnOption.Content = text;
                btnOption.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => ShowOption" + ex);
            }
        }

        public void ShowOption(bool visible)
        {
            btnOption.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                presenter.HandleOptionClick();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => btnOption_Click" + ex);
            }
        }

        private void btnReconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                presenter.HandleReconnectClick();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => btnReconnect_Click" + ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to close payment window? Payment is in progress", UI.Global_Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.PaymentTerminalWindowClosed), orderId);
                    lbLoader.Visibility = Visibility.Visible;
                    Cursor = Cursors.AppStarting;
                    btnCancel.Visibility = Visibility.Collapsed;
                    btnClose.Visibility = Visibility.Collapsed;
                    SetStatusText("Var god vänta");
                    presenter.HandleCancelClick();

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_DoWork;
                    worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                    worker.RunWorkerAsync();
                    presenter.CancelOrder(orderId, Defaults.User.Id);
                    //presenter.SetPaymentDialogCloseForced(true);
                    //this.DialogResult = false;
                    //this.Close();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => BtnClose_OnClick" + ex);
            }

        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                presenter.Reprint();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Payment Window => Reprint" + ex);
            }
        }
    }
}
