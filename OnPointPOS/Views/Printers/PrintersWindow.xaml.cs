using POSSUM.Presenters.Printers;
using POSSUM.Model;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POSSUM
{

    public partial class PrintersWindow : Window, IPrinterView
    {
        //public string SelectedTableName = "Select Table";
        public bool isTakeaway = false;
        PrinterPresenter presenter;
        public Printer SelectedPrinter { get; set; }

        public List<Printer> printers = new List<Printer>();
        public PrintersWindow()
        {
            try
            {

                InitializeComponent();
                //SelectedTableName = UI.PlaceOrder_SelectTableButton;
                presenter = new PrinterPresenter(this);
                presenter.LoadPrinterClick();

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        public void SetPrinterResult(List<Printer> printers)
        {
            this.printers = printers;
            CustomerDataGrid.ItemsSource = this.printers;
        }

        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            presenter.LoadPrinterClick();
        }

        public string GetKeyword()
        {
            return txtSearchBox.Text;
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedPrinter = CustomerDataGrid.SelectedItem as Printer;

        }

        private void BtnCloseAccountInfo_OnClick(object sender, RoutedEventArgs e)
        {

        }
        private void popupError_Loaded(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
        }
        private void txtSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.IsOpen = true;
        }

        private void txtSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PopupKeyborad.IsOpen = false;
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            UpdatePrinterWindow newCustomer = new UpdatePrinterWindow(new Printer());
            if (newCustomer.ShowDialog() == true)
            {
                SelectedPrinter = newCustomer.currentPrinter;
                presenter.LoadPrinterClick();
            }
        }

        private void txtSearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupKeyborad.IsOpen = false;
                presenter.LoadPrinterClick();
            }
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var SelectedPrinter = (Printer)(sender as Button).DataContext;
                if (SelectedPrinter != null)
                {
                    UpdatePrinterWindow newCustomer = new UpdatePrinterWindow(SelectedPrinter);
                    if (newCustomer.ShowDialog() == true)
                    {
                        SelectedPrinter = newCustomer.currentPrinter;
                        presenter.LoadPrinterClick();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
    }
}
