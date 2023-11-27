using System;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using POSSUM.Presenters.Payments;
using POSSUM.Handlers;
using POSSUM.Res;

namespace POSSUM.Views.PrintOrder
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintCloseTerminalWindow : Window, IPaymentTerminalUtilView
    {
        private readonly PaymentTerminalClosePresenter presenter;

        public PrintCloseTerminalWindow()
        {
            InitializeComponent();
            presenter = new PaymentTerminalClosePresenter(this);
            presenter.PrintCloseTerminal();
        }

        public void PrintReport(string description)
        {
            if (File.Exists(Defaults.CompanyInfo.Logo))
            {
                var uriSource = new Uri(Defaults.CompanyInfo.Logo);
                logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Source = new BitmapImage(uriSource)));
            }
            else
            {
                logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Visibility = Visibility.Collapsed));
                lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Text = Defaults.Outlet.Name));
                lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Visibility = Visibility.Visible));
            }
            gdMain.DataContext = Defaults.CompanyInfo;
            txtDescription.Text = description;

            SendPrint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Print_Click(object sender, RoutedEventArgs e)
        {
            SendPrint();
        }

        private void SendPrint()
        {
            try
            {
                var dlg = new PrintDialog();
              
                string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource) flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(400, gdMain.ActualHeight + 300);
                if (string.IsNullOrEmpty(printerName))
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(paginator, "Close Terminal Print");
                        presenter.UnRegisterEvent();
                        Close();
                    }
                }
                else
                {
                    PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                    PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                    var data = printers.Where(pn => pn.Name == printerName);
                    if (data.Count() > 0)
                    {
                        prntque = printers.Single(pn => pn.Name == printerName);
                    }
                    else // Fall back to XPS Document Writer
                    {
                        printerName = "Microsoft XPS Document Writer";
                        printers = new PrintServer().GetPrintQueues();
                        prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                        data = printers.Where(pn => pn.Name == printerName);
                        if (data.Count() > 0)
                        {
                            prntque = printers.Single(pn => pn.Name == printerName);
                        }
                    }

                    dlg.PrintQueue = prntque;
                    dlg.PrintDocument(paginator, "Close Terminal");
                    presenter.UnRegisterEvent();
                    Close();
                }
            }
            catch (Exception ex)
            {
                presenter.UnRegisterEvent();
                LogWriter.LogWrite(ex);
                MessageBox.Show(UI.Message_PrintingProblemTryLater, Defaults.AppProvider.AppTitle, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RenderGridSize()
        {
            gdMain.Measure(new Size(double.MaxValue, double.MaxValue));
            Size visualSize = gdMain.DesiredSize;
            gdMain.Arrange(new Rect(new Point(0, 0), visualSize));
            gdMain.UpdateLayout();
        }

        #region

        private const string SingleDottedLine = "-------------------------------------------------";

        public void SetInfoWindow(string infoText)
        {
            if (!string.IsNullOrEmpty(infoText))
            {
                StringBuilder b = new StringBuilder();

                b.AppendLine(SingleDottedLine);
                b.AppendLine("---------------- " + UI.CloseTerminal + " ---------------------");
                b.AppendLine(SingleDottedLine);
                b.AppendLine(UI.CloseTerminal + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " av " +
                             Defaults.User.UserName);
                b.AppendLine(SingleDottedLine);

                b.Append(infoText);
                PrintReport(b.ToString());
            }
        }

        public void ShowOption(int optionNr, bool visisble)
        {
            throw new NotImplementedException();
        }

        public void ShowOption(int optionNr, string textValue, bool visible)
        {
            throw new NotImplementedException();
        }

        public void ShowKeypad(bool p)
        {
            throw new NotImplementedException();
        }

        public void ShowAbort(bool p)
        {
            throw new NotImplementedException();
        }

        public void ShowOk(bool p)
        {
            throw new NotImplementedException();
        }

        public void ShowOk(string text, bool visible)
        {
            throw new NotImplementedException();
        }

        public void ShowResetMenu(string text, bool visisble)
        {
            throw new NotImplementedException();
        }

        public void Close(bool success)
        {
            throw new NotImplementedException();
        }

        public void ShowError(string title, string message)
        {
            presenter.UnRegisterEvent();
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

        #endregion
    }
}