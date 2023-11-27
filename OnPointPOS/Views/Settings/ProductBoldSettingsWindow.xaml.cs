using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Presenters.Settings;
using POSSUM.Res;
using POSSUM.Utils;
using POSSUM.Views.Sales;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ProductBoldSettingsWindow.xaml
    /// </summary>
    public partial class ProductBoldSettingsWindow : Window, ISettingView
    {
        SettingPresenter presenter;
        ConfigSetting setting = new ConfigSetting();
        public ProductBoldSettingsWindow()
        {
            InitializeComponent();
            presenter = new SettingPresenter(this);
            FillCheckBox();
            FillLanguageCombo();
            FillSaleTypeCombo();
            FillCatLineCombo();
            FillItemLineCombo();
            FillTexBoxValues();
            FillCleanCashTypeCombo();
            FillPaymentDeviceTypeCombo();
            FillCashDrawerTypeCombo();
            ReadConfigSettings();           
        }

        private void FillCheckBox()
        {            
            if (Defaults.CategoryBold)
                chkCategoryConfirm.IsChecked = true;
            if (Defaults.ProductBold)
                chkProductConfirm.IsChecked = true;
            if (Defaults.DirectBONG)
                chkBongConfirm.IsChecked = true;
            if (Defaults.ShowPrice)
                chkShowPriceConfirm.IsChecked = true;
            if (Defaults.ShowTableGrid)
                chkTableViewConfirm.IsChecked = true;
            if (Defaults.DisableCreditCard)
                chkDisableCreditCard.IsChecked = true;
            if (Defaults.DisableCashButton)
                chkDisableCashButton.IsChecked = true;
            if (Defaults.DisableSwishButton)
                chkDisableSwishButton.IsChecked = true;
            if (Defaults.ShowComments)
                chkShowComments.IsChecked = true;
            if (Defaults.TipStatus)
                chkTipStatus.IsChecked = true;
            if (Defaults.ShowBongAlert)
                chkShowBongAlertConfirm.IsChecked = true;
            if (Defaults.AskForPrintInvoice)
                chkAskForPrintInvoice.IsChecked = true;


            if (Defaults.Takeaway)
                chkTakeawayConfirm.IsChecked = true;
            if (Defaults.CustomerView)
                chkCustomerViewConfirm.IsChecked = true;
            if (Defaults.DailyBongCounter)
                chkDailyBongCounterConfirm.IsChecked = true;
            if (Defaults.BongCounter)
                chkBongCounterConfirm.IsChecked = true;
            if (Defaults.ElveCard)
                chkElveCardConfirm.IsChecked = true;
            if (Defaults.BeamPayment)
                chkBeamPaymentConfirm.IsChecked = true;
            if (Defaults.CreditNote)
                chkCreditNoteConfirm.IsChecked = true;
            if (Defaults.EnableCheckoutLog)
                chkEnableCheckoutLogConfirm.IsChecked = true;
            if (Defaults.DebugCleanCash)
                chkDebugCleanCashConfirm.IsChecked = true;
            if (Defaults.Deposit)
                chkDepositConfirm.IsChecked = true;

            
        }
        private void FillTexBoxValues()
        {
            txtSlideShowURL.Text = Defaults.SlideShowURL;
            txtClientSettingsProvider.Text = Defaults.SyncAPIUri;
            //txtControlUnitType.Text = Defaults.ControlUnitType.ToString();
            txtControlUnitConnectionStringe.Text = Defaults.ControlUnitConnectionString;
            txtTerminalId.Text = Defaults.TerminalId.ToString();
            txtCashDrawerType.Text = Defaults.CashDrawerType.ToString();
            txtCashDrawerHardwarePort.Text = Defaults.CashDrawerHardwarePort.ToString();
            txtScaleType.Text = Defaults.ScaleType.ToString();
            txtSCALEPORT.Text = Defaults.SCALEPORT;

            txtAccountNumber.Text= Defaults.AccountNumber1;
            txtPaymentReceiverName.Text=Defaults.PaymentReceiverName;
            txtInvoiceReference.Text= Defaults.InvoiceReference;

            txtPaymentDevicConnectionString.Text = Defaults.PaymentDevicConnectionString.ToString();
            txtPaymentDeviceType.Text = Defaults.PaymentDeviceType.ToString();
            txtAPIUSER.Text = Defaults.APIUSER;
            txtAPIPassword.Text = Defaults.APIPassword;
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
                var value = ConfigurationManager.AppSettings["ShowReprintButton"];
                if (value != null && value == "1")
                {
                    btnReprint.Visibility = Visibility.Visible;
                    lblReprint.Visibility = Visibility.Visible;
                }
                else
                {
                    btnReprint.Visibility = Visibility.Collapsed;
                    lblReprint.Visibility = Visibility.Collapsed;
                }

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                setting.Language = Convert.ToInt16(Defaults.Language);
                setting.SaleType = Convert.ToInt16(Defaults.SaleType);
                setting.ItemLine = Convert.ToInt16(Defaults.ItemsLines);
                setting.CategoryLine = Convert.ToInt16(Defaults.CategoryLines);
                setting.CleanCash = Convert.ToInt16(Defaults.ControlUnitType);
                setting.PaymentDeviceType = Convert.ToInt16(Defaults.PaymentDeviceType);
                setting.CashDrawer = Convert.ToInt16(Defaults.CashDrawerType);
                layout.DataContext = setting;
                cmbLanguage.SelectedValue = setting.Language;
                cmbSaleType.SelectedValue = setting.SaleType;
                cmbCatLines.SelectedValue = setting.CategoryLine;
                cmbItemLines.SelectedValue = setting.ItemLine;
                cmbCleanCash.SelectedValue = setting.CleanCash;
                cmbPaymentDeviceType.SelectedValue = setting.PaymentDeviceType; 
                cmbCashDrawerType.SelectedValue = setting.CashDrawer;

                if (Defaults.SaleType==SaleType.Restaurant)
                {
                    lblTip.Visibility = Visibility.Visible;
                    chkTipStatus.Visibility = Visibility.Visible;
                   

                    lblTableViewConfirm.Visibility = Visibility.Visible;
                    chkTableViewConfirm.Visibility = Visibility.Visible;
                    tblView.Visibility = Visibility.Visible;

                    lblBongCounterConfirm.Visibility = Visibility.Visible;
                    chkBongCounterConfirm.Visibility = Visibility.Visible;
                    bongCounter.Visibility = Visibility.Visible;

                    lblDailyBongCounterConfirm.Visibility = Visibility.Visible;
                    chkDailyBongCounterConfirm.Visibility = Visibility.Visible;
                    dailyCounter.Visibility = Visibility.Visible;

                    lblBongConfirm.Visibility = Visibility.Visible;
                    chkBongConfirm.Visibility = Visibility.Visible;
                    directBong.Visibility = Visibility.Visible;
                }
                else
                {
                    lblTip.Visibility = Visibility.Collapsed;
                    chkTipStatus.Visibility = Visibility.Collapsed;

                    lblTableViewConfirm.Visibility = Visibility.Collapsed;
                    chkTableViewConfirm.Visibility = Visibility.Collapsed;
                    tblView.Visibility = Visibility.Collapsed;

                    lblBongCounterConfirm.Visibility = Visibility.Collapsed;
                    chkBongCounterConfirm.Visibility = Visibility.Collapsed;
                    bongCounter.Visibility = Visibility.Collapsed;

                    lblDailyBongCounterConfirm.Visibility = Visibility.Collapsed;
                    chkDailyBongCounterConfirm.Visibility = Visibility.Collapsed;
                    dailyCounter.Visibility = Visibility.Collapsed;

                    lblBongConfirm.Visibility = Visibility.Collapsed;
                    chkBongConfirm.Visibility = Visibility.Collapsed;
                    directBong.Visibility = Visibility.Collapsed;
                }
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

        private void FillCleanCashTypeCombo()
        {
            var lst = Enum.GetValues(typeof(ControlUnitType))
         .Cast<ControlUnitType>()
         .Select(v => new ListDTO { Id = (int)v, Name = v.ToString() })
         .ToList();
            cmbCleanCash.ItemsSource = lst;
            if (cmbCleanCash.Items.Count > 0)
                cmbCleanCash.SelectedIndex = 0;
        }
        private void FillPaymentDeviceTypeCombo() 
        {
            var lst = Enum.GetValues(typeof(PaymentDeviceType))
         .Cast<PaymentDeviceType>()
         .Select(v => new ListDTO { Id = (int)v, Name = v.ToString() })
         .ToList();
            cmbPaymentDeviceType.ItemsSource = lst;
            if (cmbPaymentDeviceType.Items.Count > 0)
                cmbPaymentDeviceType.SelectedIndex = 0;
        }
        private void FillCashDrawerTypeCombo() 
        {
            var lst = Enum.GetValues(typeof(CashDrawerType))
         .Cast<CashDrawerType>()
         .Select(v => new ListDTO { Id = (int)v, Name = v.ToString() })
         .ToList();
            cmbCashDrawerType.ItemsSource = lst;
            if (cmbCashDrawerType.Items.Count > 0)
                cmbCashDrawerType.SelectedIndex = 0;
        }





        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);

        }

        public class ListDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
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
            public int CleanCash { get; set; }
            public int PaymentDeviceType { get; set; } 
            public int CashDrawer { get; set; }  
            
        }

        private void cmbCleanCash_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedCleanCash = cmbCleanCash.SelectedIndex;
            if ((ControlUnitType) selectedCleanCash == ControlUnitType.CLOUD_CLEAN_CASH)
            {
                btnCloudCleanCash.IsEnabled = true;
            }
            else
            {
                btnCloudCleanCash.IsEnabled = false;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                bool IsCategory = chkCategoryConfirm.IsChecked == true ? true : false;
                bool IsProduct = chkProductConfirm.IsChecked == true ? true : false;
                bool IsDirBong = chkBongConfirm.IsChecked == true ? true : false;
                bool IsShowPrice = chkShowPriceConfirm.IsChecked == true ? true : false;
                bool IsTableView = chkTableViewConfirm.IsChecked == true ? true : false;
                bool DisableCreditCard = chkDisableCreditCard.IsChecked == true ? true : false;
                bool DisableCashButton = chkDisableCashButton.IsChecked == true ? true : false;
                bool DisableSwishButton = chkDisableSwishButton.IsChecked == true ? true : false;
                bool ShowComments = chkShowComments.IsChecked == true ? true : false;
                bool TipStatus = chkTipStatus.IsChecked == true ? true : false;
                bool IsBongAlert = chkShowBongAlertConfirm.IsChecked == true ? true : false;
                bool AskForPrintInvoice = chkAskForPrintInvoice.IsChecked == true ? true : false;

                bool IsTakeaway = chkTakeawayConfirm.IsChecked == true ? true : false;
                bool IsCustomerView = chkCustomerViewConfirm.IsChecked == true ? true : false;
                bool IsDailyBongCounter = chkDailyBongCounterConfirm.IsChecked == true ? true : false;
                bool IsBongCounter = chkBongCounterConfirm.IsChecked == true ? true : false;
                bool IsElveCard = chkElveCardConfirm.IsChecked == true ? true : false;
                bool IsCreditNote = chkCreditNoteConfirm.IsChecked == true ? true : false;
                bool IsBeamPayment = chkBeamPaymentConfirm.IsChecked == true ? true : false;

                string SlideShowUrl = txtSlideShowURL.Text;
                string APIUSER = txtAPIUSER.Text;
                string APIPassword = txtAPIPassword.Text;

                string ClientSettingsProvider = txtClientSettingsProvider.Text;
                string controlUnitType = cmbCleanCash.SelectedIndex.ToString();
                string CUConnectionString = txtControlUnitConnectionStringe.Text;
                string TerminalId = txtTerminalId.Text;
                string cashDrawerType = cmbCashDrawerType.SelectedValue.ToString();
                string CashDrawerHardwarePort = txtCashDrawerHardwarePort.Text;
                string scaleType = txtScaleType.Text;
                string SCALEPORT = txtSCALEPORT.Text;

                string AccountNumber = txtAccountNumber.Text;
                string PaymentReceiverName = txtPaymentReceiverName.Text;
                string InvoiceReference = txtInvoiceReference.Text;


                bool IsEnableCheckoutLogs = chkEnableCheckoutLogConfirm.IsChecked == true ? true : false; ;
                bool IsCleanCash = chkDebugCleanCashConfirm.IsChecked == true ? true : false;
                bool IsDeposit = chkDepositConfirm.IsChecked == true ? true : false;
                //string paymentDeviceType = txtPaymentDeviceType.Text;
                string paymentDeviceType = cmbPaymentDeviceType.SelectedValue?.ToString();
                string PaymentDevicConStr = txtPaymentDevicConnectionString.Text;
                var CategoryLine = Convert.ToInt32(cmbCatLines.SelectedValue);
                var ItemLine = Convert.ToInt32(cmbItemLines.SelectedValue);
                var Language = Convert.ToInt32(cmbLanguage.SelectedValue);


                SaleType saleType;
                Enum.TryParse(cmbSaleType.SelectedValue.ToString(), true, out saleType);
                SaleType SalesType = saleType;

                setting.CategoryLine = Convert.ToInt16(cmbCatLines.SelectedValue);
                setting.ItemLine = Convert.ToInt16(cmbItemLines.SelectedValue);
                setting.Language = Convert.ToInt16(cmbLanguage.SelectedValue);
                setting.SaleType = Convert.ToInt16(cmbSaleType.SelectedValue);
                setting.CleanCash = Convert.ToInt16(cmbCleanCash.SelectedValue);
                setting.PaymentDeviceType = Convert.ToInt16(cmbPaymentDeviceType.SelectedValue);  
                setting.CashDrawer = Convert.ToInt16(cmbCashDrawerType.SelectedValue); 

                Defaults.ProductBold = IsProduct;
                Defaults.CategoryBold = IsCategory;
                Defaults.DirectBONG = IsDirBong;
                Defaults.ShowPrice = IsShowPrice;
                Defaults.ShowTableGrid = IsTableView;
                Defaults.DisableCreditCard = DisableCreditCard;
                Defaults.DisableCashButton =DisableCashButton;
                Defaults.DisableSwishButton =DisableSwishButton;
                Defaults.ShowComments =ShowComments;
                Defaults.TipStatus = TipStatus;
                Defaults.AskForPrintInvoice = AskForPrintInvoice;
                Defaults.ShowBongAlert = IsBongAlert;
                Defaults.Takeaway = IsTakeaway;
                Defaults.DailyBongCounter = IsDailyBongCounter;
                Defaults.BongCounter = IsBongCounter;
                Defaults.CustomerView = IsCustomerView;
                Defaults.SlideShowURL = SlideShowUrl;
                Defaults.ItemsLines = ItemLine;
                Defaults.CategoryLines = CategoryLine;
                Defaults.SaleType = SalesType;
                Defaults.Language = (CurrentLanguage)Enum.Parse(typeof(CurrentLanguage), Language.ToString());

                Defaults.SyncAPIUri = ClientSettingsProvider;
                Defaults.ControlUnitType = (ControlUnitType)Enum.Parse(typeof(ControlUnitType), controlUnitType.ToString());
                Defaults.ControlUnitConnectionString = CUConnectionString;
                Defaults.TerminalId = TerminalId;
                Defaults.CashDrawerType = (CashDrawerType)Enum.Parse(typeof(CashDrawerType), cashDrawerType.ToString());
                Defaults.CashDrawerHardwarePort = Convert.ToInt16(CashDrawerHardwarePort);
                Defaults.ScaleType = (ScaleType)Enum.Parse(typeof(ScaleType), scaleType.ToString());
                Defaults.SCALEPORT = SCALEPORT;

                Defaults.AccountNumber1 = AccountNumber;
                Defaults.PaymentReceiverName = PaymentReceiverName;
                Defaults.InvoiceReference = InvoiceReference;


                Defaults.PaymentDeviceType = (PaymentDeviceType)Enum.Parse(typeof(PaymentDeviceType), paymentDeviceType.ToString());
                Defaults.PaymentDevicConnectionString = PaymentDevicConStr;
                Defaults.APIUSER = APIUSER;
                Defaults.APIPassword = APIPassword;
                Defaults.ElveCard = IsElveCard;
                Defaults.CreditNote = IsCreditNote;
                Defaults.BeamPayment = IsBeamPayment;
                Defaults.EnableCheckoutLog = IsEnableCheckoutLogs;
                Defaults.DebugCleanCash = IsCleanCash;
                Defaults.Deposit = IsDeposit;

                var lst = new List<Setting> {
                new Setting { Code = SettingCode.CategoryBold, Value = IsCategory ? "1" : "0" ,Updated=DateTime.Now,},
                new Setting { Code = SettingCode.ProductBold, Value = IsProduct ? "1" : "0" ,Updated=DateTime.Now},
                new Setting { Code = SettingCode.DirectBong, Value = IsDirBong ? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.ShowPrice, Value = IsShowPrice ? "1" : "0",Updated=DateTime.Now,Type=SettingType.TerminalSettings},
                new Setting { Code = SettingCode.TableView, Value = IsTableView ? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.DisableCreditCard, Value = DisableCreditCard ? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.DisableCashButton, Value = DisableCashButton ? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.DisableSwishButton, Value = DisableSwishButton ? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.ShowComments, Value = ShowComments ? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.TipStatus, Value = TipStatus? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.AskForPrintInvoice, Value = AskForPrintInvoice? "1" : "0",Updated=DateTime.Now},

                new Setting { Code = SettingCode.ShowBongAlert, Value = IsBongAlert? "1" : "0",Updated=DateTime.Now,Type=SettingType.PrintSettings},
                new Setting { Code = SettingCode.SlideShowURL, Value = SlideShowUrl,Updated=DateTime.Now},
                new Setting { Code = SettingCode.Language, Value = Language.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.CategoryLines, Value = CategoryLine.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.SaleType, Value = SalesType.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.ItemLines, Value = ItemLine.ToString(),Updated=DateTime.Now},

                new Setting { Code = SettingCode.ClientSettingsProvider, Value = ClientSettingsProvider.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.ControlUnitType, Value = controlUnitType.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.ControlUnitConnectionString, Value = CUConnectionString.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.TerminalId, Value = CUConnectionString.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.CashDrawerType, Value = cashDrawerType.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.CashDrawerHardwarePort, Value = CashDrawerHardwarePort.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.ScaleType, Value = scaleType.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.SCALEPORT, Value = SCALEPORT.ToString(),Updated=DateTime.Now},

                new Setting { Code = SettingCode.AccountNumber, Value = AccountNumber.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.PaymentReceiver, Value = PaymentReceiverName.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.FakturaReference, Value = InvoiceReference.ToString(),Updated=DateTime.Now},

                new Setting { Code = SettingCode.PaymentDeviceType, Value = paymentDeviceType.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.PaymentDevicConnectionString, Value = PaymentDevicConStr.ToString(),Updated=DateTime.Now},
                new Setting { Code = SettingCode.TerminalId, Value = TerminalId.ToString(),Updated=DateTime.Now},

                new Setting { Code = SettingCode.Takeaway, Value = IsTakeaway? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.CustomerView, Value = IsCustomerView? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.DailyBongCounter, Value = IsDailyBongCounter? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.BongCounter, Value = IsBongCounter? "1" : "0",Updated=DateTime.Now},

                new Setting { Code = SettingCode.APIUSER, Value = APIUSER,Updated=DateTime.Now},
                new Setting { Code = SettingCode.APIPassword, Value = APIPassword,Updated=DateTime.Now},
                new Setting { Code = SettingCode.ElveCard, Value = IsElveCard? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.CreditNote, Value = IsCreditNote? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.BeamPayment, Value = IsBeamPayment? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.EnableCheckoutLog, Value = IsEnableCheckoutLogs? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.DebugCleanCash, Value = IsCleanCash? "1" : "0",Updated=DateTime.Now},
                new Setting { Code = SettingCode.Deposit, Value = IsDeposit? "1" : "0",Updated=DateTime.Now},


                };

                if (presenter.SaveSettings(lst))
                {
                    MessageBox.Show("Settings " + UI.Message_Saved_Success, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = false;
                    try
                    {
                        DataSync();
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                    MessageBox.Show(UI.Message_Saved_Fail, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Sync Data after complete settings
        private void DataSync()
        {
            try
            {
                bool res = true;
                if (res)
                {
                    //MessageBox.Show(UI.Message_DataSyncSucessfull, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    var cmb = cmbLanguage.SelectedValue.ToString();
                    var lang = cmb == "1" ? "sv-SE" : cmb == "2" ? "en-US" : cmb == "3" ? "es-ES" : cmb == "4" ? "ar-SA" : cmb == "5" ? "ur-PK" : "sv-SE";
                    App.MainWindow.SetLanguage(lang);
                    App.MainWindow.AddControlToMainCanvas(new UCSale());
                    App.MainWindow.AddUserActivityMenu();
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

        private void BtnCloudCleanCash_Click(object sender, RoutedEventArgs e)
        {
            string message = presenter.BindCloudCleanCash();
            if (message.Equals("Success"))
            {
                MessageBox.Show(UI.Cloud_Clean_Cash_Successfull, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            //App.MainWindow.Reprint();
        }
    }
}
