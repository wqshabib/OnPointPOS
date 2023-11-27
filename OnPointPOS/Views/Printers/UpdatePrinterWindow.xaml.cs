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
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Presenter.Products;
using POSSUM.Events;
using POSSUM.Presenters.Printers;
using System.Threading;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for UpdatePrinterWindow.xaml
    /// </summary>
    public partial class UpdatePrinterWindow : Window, IPrinterView
    {
        PrinterPresenter presenter;
        public Printer currentPrinter = null;

        public UpdatePrinterWindow(Printer printer)
        {
            InitializeComponent();
            presenter = new PrinterPresenter(this);

            currentPrinter = printer;
            layoutGrid.DataContext = currentPrinter;

        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SavePrinter();
        }
        private void SavePrinter()
        {
            try
            {
                currentPrinter.LocationName = txtLocationName.Text;
                currentPrinter.PrinterName = txtPrinterName.Text;
                if (string.IsNullOrEmpty(currentPrinter.LocationName))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(currentPrinter.PrinterName))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                currentPrinter.Updated = DateTime.Now;
                bool res = presenter.UpdatePrinter(currentPrinter);
                if (res)
                {
                    this.DialogResult = true;
                }
                else
                {
                    ShowError(UI.Message_Error, UI.Message_Saved_Fail);

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

            }


        }
        private void BtnCloseAccountInfo_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SavePrinter();
        }





        private void txtUnitPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            decimal val = 0;
            e.Handled = !decimal.TryParse(e.Text, out val);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


        }

        public void SetPrinterResult(List<Printer> printers)
        {

        }

        public string GetKeyword()
        {
            return " ";
        }
    }
}
