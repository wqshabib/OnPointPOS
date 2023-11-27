using POSSUM.Data;
using POSSUM.Handlers;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Presenters.Login;
using POSSUM.Res;
using POSSUM.Utils;
using POSSUM.View;
using POSSUM.View.OrderHistory;
using POSSUM.Views.Login;
using POSSUM.Views.PrintOrder;
using POSSUM.Views.Products;
using POSSUM.Views.Sales;
//using POSSUM.Utility;
//using POSSUM.Utils;
//using POSSUM.View;
//using POSSUM.Views.Products;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for UCUserActivity.xaml
    /// </summary>
    public partial class UCUserActivity : UserControl
    {
        public UCUserActivity()
        {
            InitializeComponent();
            lblAppProvider.Text = Defaults.AppProvider.Name;

            var link = ConfigurationManager.AppSettings["SeamlessWebbPortal"];
            if (string.IsNullOrEmpty(link))
            {
                this.mnSeamlessWebbPortal.Visibility = Visibility.Collapsed;
                this.sepSeamlessWebbPortal.Visibility = Visibility.Collapsed;
            }

        }

        private void menuViewOrderHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new OrderHistoryWindow();
            historyWindow.Show();
        }

        private void menu_paymentTerminalUtil_Click(object sender, RoutedEventArgs e)
        {
            if (PosState.GetInstance().PaymentDevice != null)
            {
                var ptu = new PaymentTerminalUtil();
                ptu.ShowDialog();
            }
        }

        private void menu_MenuCashDrawerChangek(object sender, RoutedEventArgs e)
        {
            var ptu = new PromptInfoAmount();
            ptu.ShowDialog();

        }



        private void showReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {



                var reportId = ReportGenerator.GenerateReport(Defaults.Terminal.Id, 0);
                var reportContent = ReportGenerator.GetReport(reportId, 0, Defaults.User.UserCode);

                //Printing printing = new Printing();
                //printing.setPrinterName(Defaults.PrinterName);
                //printing.print(reportContent);

                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReportXViewed));
                var printWindow = new PrintReportWindow(reportContent, ReportType.XReport, reportId);
                printWindow.ShowDialog();
                // printWindow.GenerateReport(reportContent, ReportType.XReport);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void btnJournal_Click(object sender, RoutedEventArgs e)
        {
            PrintJournalWindow journalWindow = new PrintJournalWindow();
            journalWindow.ShowDialog();
        }



        private void menuzReport_Click(object sender, RoutedEventArgs e)
        {
            RePrintZReportWindow printZReport = new RePrintZReportWindow();
            printZReport.ShowDialog();
        }

        private void mnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow("");
            addProductWindow.ShowDialog();
        }



        private void mnPrinter_Click(object sender, RoutedEventArgs e)
        {
            PrintersWindow printerWindow = new PrintersWindow();
            printerWindow.ShowDialog();
        }

        private void mnViewCustomerOrder_Click(object sender, RoutedEventArgs e)
        {
            CustomerOrderInvoicesWindow invoicesWindow = new CustomerOrderInvoicesWindow();
            invoicesWindow.ShowDialog();
        }

        private void mnSettingsa_Click(object sender, RoutedEventArgs e)
        {
            //CashdrawerSettingWindow cdWindow = new CashdrawerSettingWindow();
            //cdWindow.ShowDialog();
            //App.MainWindow.AddUserActivityMenu();
            //ConfigSettingWindow configSetting = new ConfigSettingWindow();
            //configSetting.ShowDialog();
        }
        private void menuSyncData_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(UI.Message_DoYouWantToSyncData, Defaults.AppProvider.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {

                    bool res = false;
                    var progressDialog = new ProgressWindow();

                    var backgroundThread = new Thread(
                        new ThreadStart(() =>
                        {
                            SyncPOSController syncController = new SyncPOSController();
                            res = syncController.DataSync(DateTime.Now, Defaults.Terminal.Id, Defaults.LocalConnectionString, Defaults.SyncAPIUri, Defaults.APIUSER, Defaults.APIPassword);
                            progressDialog.Closed += (arg, ev) =>
                            {
                                progressDialog = null;
                            };
                            progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                progressDialog.Close();
                            }));

                        }));
                    backgroundThread.Start();
                    progressDialog.ShowDialog();
                    backgroundThread.Join();
                    if (res)
                    {
                        MessageBox.Show(UI.Message_DataSyncSucessfull, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                        var uc = new UCSale();
                        App.MainWindow.AddControlToMainCanvas(uc);
                    }
                    else
                        MessageBox.Show(UI.Message_DataSyncFailed, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);

                    App.MainWindow.ShowError(UI.Global_Error, ex.Message);
                }
            }
        }


        public void CloseTerminal() {

            if (Defaults.IsClient)
            {
                return;
            }
            try
            {
                App.MainWindow.CloseExternalOrder();
                LogWriter.LogWrite("mnClose_Click  == > " + "2");
                try
                {
                    try
                    {
                        if (Defaults.ISMQTTInitialize == "1")
                        {
                            App.MainWindow.DisconnectMQTTClientNotMini();
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex);
                    }

                    LogWriter.LogWrite("mnClose_Click  == > " + "3");
                    using (var db = PosState.GetInstance().Context)
                    {
                        var _bong = db.BongCounter.FirstOrDefault(b => b.Id == 2);
                        if (_bong != null)
                        {
                            _bong.Counter = 1;
                        }
                        Defaults.CGWarnings = new List<CGWarning>();
                        var terminal = db.Terminal.Find(Defaults.Terminal.Id);
                        terminal.Close(Defaults.User.Id);
                        if (Defaults.TerminalMode == TerminalMode.SingleOutlet)
                        {
                            if (Defaults.CASH_GUARD == false)
                            {

                                var cd = db.CashDrawer.FirstOrDefault(c => c.TerminalId == terminal.Id);
                                cd.OpenCashDrawerClosing(Defaults.User.Id);
                            }
                        }
                        if (terminal.StatusLog != null && terminal.StatusLog.Count > 0)
                        {
                            db.TerminalStatusLog.AddRange(terminal.StatusLog);
                        }


                        db.SaveChanges();
                        //UPDATE TERMINAL SESSION OBJECT
                        Defaults.Terminal = terminal;
                    }
                    LogWriter.LogWrite("mnClose_Click  == > " + "4");
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.TerminalClosed));
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                }

                try
                {
                    /******
               * Generate Z-report
               ******/
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReportZViewed));
                    //   App.MainWindow.SetLanguage("sv-SE");
                    var reportId = ReportGenerator.GenerateReport(Defaults.Terminal.Id, 1);
                    var reportContent = ReportGenerator.GetReport(reportId, 1, Defaults.User.UserCode);

                    //var printWindow = new printReportWindow(reportContent, ReportType.ZReport);
                    //printWindow.ShowDialog();
                    Printing printing = new Printing();
                    printing.setPrinterName(Defaults.PrinterName);
                    printing.print(reportContent);

                    //  App.MainWindow.SetLanguage(CultureString);
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                }
                LogWriter.LogWrite("mnClose_Click  == > " + "5");
                /******
                  * Close Terminal
                  ******/
                try
                {

                    PosState state = PosState.GetInstance();
                    var paymentDevice = state.PaymentDevice;
                    var devicestatus = paymentDevice.GetDeviceStatus();
                    if (devicestatus != PaymentDeviceStatus.CLOSED)
                    {
                        PrintCloseTerminalWindow printCloseTerminal = new PrintCloseTerminalWindow();
                        //   paymentDevice.CloseBatch();
                    }
                    //System.Threading.Thread.Sleep(5000);
                    //paymentDevice.Disconnect();

                    if (PosState.GetInstance().ControlUnitAction.ControlUnit != null)
                    {
                        PosState.GetInstance().ControlUnitAction.ControlUnit.Close();
                    }
                    LogWriter.LogWrite("mnClose_Click  == > " + "6");

                }
                catch (Exception ex)
                {
                    //  LogWriter.LogWrite(ex);
                    LogWriter.LogWrite("mnClose_Click  == > " + "6 No point ex");
                    App.MainWindow.ShowError("Dagsavslut för terminal", "Kassan stängd");//Fel vid 
                }

                //Switch to Login Screen
                App.MainWindow.AdminArea.Visibility = Visibility.Collapsed;
                App.MainWindow.AcitivityManuGrid.Children.Clear();
                //  btnUserActivity.Visibility = Visibility.Collapsed;
                var uc = new UCLogin();
                App.MainWindow.AddControlToMainCanvas(uc);
                LogWriter.LogWrite("mnClose_Click  == > " + "7");

                //if (PosState.GetInstance().PaymentDevice != null)
                //{
                //    //PosState.GetInstance().PaymentDevice.Disconnect();
                //}


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("mnClose_Click  == > " + "As a whole method:  >> " + ex.Message);
                App.MainWindow.ShowError("Stängning", "Kassan ej stängd");//Fel vid 
            }
        }

        private void mnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseTerminal();
        }
        private string CultureString
        {
            get
            {
                switch (Defaults.Language)
                {
                    case CurrentLanguage.Swedish:
                        return "sv-SE";
                    case CurrentLanguage.English:
                        return "en-US";
                    //case CurrentLanguage.Spanish:
                    //    return "es-ES";
                    case CurrentLanguage.Arabic:
                        return "ar-SA";

                    default:
                        return "sv-SE";
                }

            }
        }
        private void mnPowerLogOff_Click(object sender, RoutedEventArgs e)
        {
            LogOut();
        }

        public void LogOut()
        {
            try
            {
                Defaults.CGWarnings = new List<CGWarning>();
                App.MainWindow.CloseExternalOrder();
                App.MainWindow.AdminArea.Visibility = Visibility.Collapsed;
                App.MainWindow.AcitivityManuGrid.Children.Clear();
                //  btnUserActivity.Visibility = Visibility.Collapsed;
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.LoggedOut));
                var uc = new UCLogin();
                App.MainWindow.AddControlToMainCanvas(uc);

                //if (PosState.GetInstance().PaymentDevice != null)
                //{
                //    PosState.GetInstance().PaymentDevice.Disconnect();
                //}
                //if (PosState.GetInstance().ControlUnitAction.ControlUnit != null)
                //{
                //    PosState.GetInstance().ControlUnitAction.ControlUnit.Close();
                //}

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void Lanague_Click(object sender, RoutedEventArgs e)
        {
            LanguageWindow langWindow = new LanguageWindow();
            langWindow.ShowDialog();
        }

        private void POSAdmin_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://admin.possumsystem.com/");
            //UCPOSAdmin uc = new UCPOSAdmin();
            //App.MainWindow.AddControlToMainCanvas(uc);
        }

        private void menuViewProductSale_Click(object sender, RoutedEventArgs e)
        {
            SaleByItemsWindow saleByItemWindow = new SaleByItemsWindow();
            saleByItemWindow.Show();
        }

        private void menuViewProductStock_Click(object sender, RoutedEventArgs e)
        {
            ProductStockWindow stockWindow = new ProductStockWindow();
            stockWindow.Show();
        }

        private void UCUserActivity_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Defaults.CASH_GUARD)
            {
                txtCashDrawer.Text = "CASH GUARD";
            }


           

        }

        private void menu_Item_Click(object sender, RoutedEventArgs e)
        {
            //MenuItemWindow itemWindow = new MenuItemWindow();
            //itemWindow.Show();
        }

        private void AboutPOS_Click(object sender, RoutedEventArgs e)
        {
            var aboutwindow = new AboutWindow();
            aboutwindow.ShowDialog();
        }

        private void mnCalculator_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process p = System.Diagnostics.Process.Start("calc.exe");
            p.WaitForInputIdle();
            // NativeMethods.SetParent(p.MainWindowHandle, this.Handle);
        }

        private void showUserReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportContent = ReportGenerator.GetUserReport(Defaults.Terminal.Id, DateTime.Now.Date, DateTime.Now, Defaults.User.Id);

                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReportXViewed));
                var printWindow = new PrintReportWindow(reportContent, ReportType.XReport, default(Guid));
                printWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void MnAddSettings_Click(object sender, RoutedEventArgs e)
        {
            ProductBoldSettingsWindow productWindow = new ProductBoldSettingsWindow();
            productWindow.ShowDialog();
        }

        private void mnSeamlessWebbPortal_Click(object sender, RoutedEventArgs e)
        {
            var link = ConfigurationManager.AppSettings["SeamlessWebbPortal"];
            if (!string.IsNullOrEmpty(link))
                System.Diagnostics.Process.Start(link)
;
            //SeamlessWebbPortal
        }

        private void btnMenuPin_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Defaults.MenuPinCode))
            {
                MenuCodeWindow menuCodeWindow = new MenuCodeWindow();
                if (menuCodeWindow.ShowDialog() == false)
                    return;

                btnUserActivity.Visibility = Visibility.Visible;
                btnMenuPin.Visibility = Visibility.Collapsed;
                mainContextMenu.IsOpen = true;
                mainContextMenu.PlacementTarget = btnUserActivity;
                mainContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            }
            else
            {
                btnUserActivity.Visibility = Visibility.Visible;
                btnMenuPin.Visibility = Visibility.Collapsed;
                mainContextMenu.IsOpen = true;
                mainContextMenu.PlacementTarget = btnUserActivity;
                mainContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            }
        }

        private void mainContextMenu_ContextMenuClosing_1(object sender, ContextMenuEventArgs e)
        {
            //btnmenupin.Visibility = Visibility.Visible;
            //btnUserActivity.Visibility = Visibility.Collapsed;
        }

        private void mainContextMenu_ContextMenuOpening_1(object sender, ContextMenuEventArgs e)
        {
        //    btnmenupin.Visibility = Visibility.Visible;
        //    btnUserActivity.Visibility = Visibility.Collapsed;
        }

        private void mainContextMenu_Opened_1(object sender, RoutedEventArgs e)
        {
            //btnmenupin.Visibility = Visibility.Visible;
            //btnUserActivity.Visibility = Visibility.Collapsed;

        }

        private void mainContextMenu_Closed_1(object sender, RoutedEventArgs e)
        {
            btnMenuPin.Visibility = Visibility.Visible;
            btnUserActivity.Visibility = Visibility.Collapsed;
        }
    }
}
