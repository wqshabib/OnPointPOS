using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notifications.Wpf;
using POSSUM.Data;
using POSSUM.Events;
using POSSUM.Handlers;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Pdf;
using POSSUM.Presenters.Sales;
using POSSUM.Res;
using POSSUM.Utility;
using POSSUM.Utils;
using POSSUM.ViewModels;
using POSSUM.Views;
using POSSUM.Views.CheckOut;
using POSSUM.Views.Login;
using POSSUM.Views.Sales;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
//using NotificationManager = POSSUM.Handlers.NotificationManager;
using NotificationManager = Notifications.Wpf.NotificationManager;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables/Properties
        public int ShiftNo { get; set; }
        public SerialPort serialPort = new SerialPort();
        #endregion
        public CustomerViewCart customerViewCart;
        public event AddItemEventHandler AddItemUpdated;
        public ObservableCollection<OrderLine> ItemsList;
        public event OrderCompletedEventHandler OrderCompleted;
        public event NewOrderEventHandler NewOrder;
        public event InsertedAmountEventHandler InsertedAmount;
        public event CGWarningEventHandler CGWarning;
        public event DallasEventHandler DallasDataReceived;
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);
        private readonly NotificationManager _notificationManager = new NotificationManager();
        private readonly Random _random = new Random();

        //MqttClient client;
        public System.Timers.Timer timer;
        private static System.Timers.Timer aTimer;

        //   public LUEOrderServiceClient lueApiClient;
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                // call a method repeatedly in timer
                //aTimer = new System.Timers.Timer(10000);
                //aTimer.Elapsed += new ElapsedEventHandler(CheckMQTTStatus);
                //// Set the Interval to 2 seconds (2000 milliseconds).
                //aTimer.Interval = 5000;
                //aTimer.Enabled = true;

                Log.WriteLog("MainWindow:");

                int logvisibility = 0;
                int.TryParse(ConfigurationManager.AppSettings["EmployeeLog"], out logvisibility);
                btnCheckIn.Visibility = logvisibility == 1 ? Visibility.Visible : Visibility.Collapsed;

                TerminalMode terminalMode;
                Enum.TryParse(ConfigurationManager.AppSettings["TerminalMode"], true, out terminalMode);
                Defaults.TerminalMode = terminalMode;
                if (Defaults.TerminalMode == TerminalMode.SingleOutlet)
                {
                    Defaults.Init();
                    if (Defaults.Terminal == null)
                    {
                        throw new Exception("Invalid Terminal");
                    }
                }
                App.MainWindow = this;
                //ItemsList = new ObservableCollection<OrderLine>();
                customerViewCart = new CustomerViewCart(this);
                ShiftNo = 0;



                if (Defaults.IsDallasKey)
                {
                    OpenSerialPort();
                }
                //using (var cu = PosState.GetInstance().ControlUnitAction)
                //{ on system reboot we are testing status
                try
                {
                    LogWriter.LogWrite("cp 1");

                    var cu = PosState.GetInstance().ControlUnitAction;
                    var status = cu.ControlUnit.CheckStatus();

                    LogWriter.LogWrite("cp 2" + status);

                    if (status != ControlUnitStatus.OK)
                    {
                        LogWriter.LogWrite("cp 3" + " Retry");

                        PosState.GetInstance().ReTryControlUnit();
                        cu = PosState.GetInstance().ControlUnitAction;
                        status = cu.ControlUnit.CheckStatus();
                    }
                    if (status != ControlUnitStatus.OK)
                    {
                        this.ShowError(UI.CannotConnectToController, "");
                        CUConnectionWindow ucConnectionWindow = new CUConnectionWindow();
                        if (ucConnectionWindow.ShowDialog() == true)
                        {
                            OnlineImage();
                            var uc = new UCLogin();

                            AddControlToMainCanvas(uc);
                        }
                        else
                            Environment.Exit(0);
                    }
                    else
                    {
                        OnlineImage();
                        var uc = new UCLogin();

                        AddControlToMainCanvas(uc);
                    }
                    // cu.ControlUnit.Close();
                }
                catch (ControlUnitException e)
                {
                    LogWriter.LogWrite("cp 4" + " Exception" + e.ToString());

                    PosState.GetInstance().ReTryControlUnit();
                    var cu = PosState.GetInstance().ControlUnitAction;
                    var status = cu.ControlUnit.CheckStatus();

                    LogWriter.LogWrite("cp 5" + " Retry" + status);

                    if (status != ControlUnitStatus.OK)
                    {

                        // this.ShowError(UI.Global_Error, e.Message);
                        CUConnectionWindow ucConnectionWindow = new CUConnectionWindow();
                        if (ucConnectionWindow.ShowDialog() == true)
                        {
                            OnlineImage();
                            var uc = new UCLogin();

                            AddControlToMainCanvas(uc);
                        }
                        else
                            Environment.Exit(0);
                    }
                    else
                    {
                        //this.ShowError(UI.Global_Error, e.Message);
                        MessageBox.Show("Restart control unit", UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    }
                }
                //}

                //if (Defaults.EnableMqtt)
                //    MqttSubscriber();
                // GetLUEOrderDetail("371");
                InsertedAmount += MainWindow_InsertedAmount;
                CGWarning += MainWindow_CGWarning;


            }
            catch (Exception ex)
            {
                // LogWriter.LogWrite(ex);
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);

            }
            DispatcherTimer clockTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Normal, delegate
            {
                this.txtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            }, this.Dispatcher);

            DispatcherTimer connectionTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 30, 0), DispatcherPriority.Normal, delegate
            {

                OnlineImage();
            }, this.Dispatcher);
            DispatcherTimer messageTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 60, 0), DispatcherPriority.Normal, delegate
            {
                //ping("9E8CD2DB-1582-4722-87C5-A2F775FB1299");
                //pingLUE("open");
                // AdminMessageAlert();
            }, this.Dispatcher);

            DispatcherTimer closeTerminalTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 60, 0), DispatcherPriority.Normal, delegate
            {
                var currentTime = DateTime.Now;
                TimeSpan start = new TimeSpan(23, 59, 0);
                if (currentTime.TimeOfDay.Hours == start.Hours && currentTime.TimeOfDay.Minutes == start.Minutes)
                {
                    if (!string.IsNullOrEmpty(Defaults.MenuPinCode) )
                    {
                        if(Defaults.Terminal.IsOpen)
                        { 
                         CloseTerminal();
                        }
                    }
                }
            }, this.Dispatcher);
        }

        private void CloseTerminal()
        {
            if (_uc != null)
            {
                _uc.CloseTerminal();
            }
        }

        public bool CloseCleanCashFailedOrder(string userId)
        {
            InvoiceHandler handler = new InvoiceHandler();
            var result = InvoiceHandler.FailedOrdersCCStatus.OrderNotFound;
            for (int i = 0; i < 3; i++)
            {
                result = handler.ProcessFailedOrders(userId, null);
            }

            if (result == InvoiceHandler.FailedOrdersCCStatus.OrderNotFound)
            {
                return true;
            }

            if (result == InvoiceHandler.FailedOrdersCCStatus.OrderFoundButFailed)
            {
                //MessageBox.Show("There are some failed orders available in order history. Please process these and then close terminal.");
                return false;
            }

            if (result == InvoiceHandler.FailedOrdersCCStatus.OrderFoundAndSuccess)
            {
                var count = handler.LoadFailedOrdersCount();
                if (count > 0)
                {
                    //MessageBox.Show("There are some failed orders available in order history. Please process these and then close terminal.");
                    return false;
                }

                return true;
            }

            return true;
        }



        //public void showNotification()
        //{
        //    NotificationView notiWindow = new NotificationView();
        //    notificationCon.Visibility = Visibility.Visible;
        //    notificationCon.Children.Add(notiWindow);

        //}

        //public void HideNotification()
        //{
        //    notificationCon.Visibility = Visibility.Hidden;
        //    notificationCon.Children.Clear();

        //}



        private void MainWindow_CGWarning(object sender, string warning)
        {
            if (Defaults.CGWarnings.Count > 0)
                btnCGError.Dispatcher.BeginInvoke(new Action(() => btnCGError.Visibility = Visibility.Visible));
            else
                btnCGError.Dispatcher.BeginInvoke(new Action(() => btnCGError.Visibility = Visibility.Collapsed));

        }
        public void SetCGErrVisibilty()
        {
            btnCGError.Visibility = Visibility.Collapsed;
        }
        private void MainWindow_InsertedAmount(object sender, int amout)
        {

        }

        public long JavaDateTimetoTimeStamp(DateTime dt2)
        {

            long dt2long = dt2.Ticks;
            DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0);
            long epochlong = epochTime.Ticks;
            long timeStamp = (dt2long - epochlong) / 10000;
            return timeStamp;
        }
        private void AdminMessageAlert()
        {

            //SettingPresenter settingPresenter = new SettingPresenter();
            //List<Inbox> messages = settingPresenter.GetVendorMessage();
            //if (messages.Count > 0)
            //    btnVendorMessage.Visibility = Visibility.Visible;
            //else
            //    btnVendorMessage.Visibility = Visibility.Collapsed;
        }

        private void BtnMessage_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Show Message");
        }

        private string RandomWPFColor()
        {
            var random = new Random();
            var color = String.Format("#{0:X8}", random.Next(0x1000000));
            return color;
        }

        private void OpenSerialPort()
        {
            try
            {


                serialPort.PortName = "COM7";
                serialPort.BaudRate = 9600;

                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void TestLogin(string key)
        {
            Defaults.DallasKey = key;
            if (DallasDataReceived != null)
                DallasDataReceived(this, Defaults.DallasKey);
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = serialPort.ReadExisting();
            //  MessageBox.Show("Main Window" + data);
            if (data.Length > 10)
            {
                Defaults.DallasKey = data.Substring(6, data.Length - 7);
                if (DallasDataReceived != null)
                    DallasDataReceived(this, Defaults.DallasKey);
            }
            else
            {
                try
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CloseExternalOrder();
                        AdminArea.Visibility = Visibility.Collapsed;
                        AcitivityManuGrid.Children.Clear();
                        //  btnUserActivity.Visibility = Visibility.Collapsed;
                        LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.LoggedOut));
                        //var uc = new UCLoginArea();
                        //AddControlToMainCanvas(uc);
                        if (PosState.GetInstance().PaymentDevice != null)
                        {
                            //PosState.GetInstance().PaymentDevice.Disconnect();
                        }
                    }));


                }
                catch (Exception ex)
                {

                    LogWriter.LogWrite(ex);
                }
            }

        }
        //    public ExternalOrderServiceClient externalClient;
        public void ShowExternalOrder()
        {
            //  LUEMqttSubscriber();

            //timer = new System.Timers.Timer();
            //timer.Enabled = true;
            //timer.Interval = 10000;
            //timer.Elapsed += Timer_Elapsed;
        }
        public void CloseExternalOrder()
        {
            try
            {


                //pingLUE("closed");
                //lueApiClient = null;
                //if (clientLUE != null)
                //    clientLUE.Disconnect();
                //clientLUE = null;
            }
            catch (Exception ex)
            {
                //LogWriter.LogWrite(ex);
            }
            if (timer != null)
            {
                timer.Enabled = false;
                timer.Dispose();
            }

        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            GetExternalOrder();
        }



        System.Media.SoundPlayer player;
        bool IsStoped = false;
        private void PlayAlarm()
        {
            try
            {
                if (File.Exists(@"alarm.wav"))
                {
                    if (IsStoped)
                        return;
                    if (player == null)
                        player = new System.Media.SoundPlayer(@"alarm.wav");//c:\temp\
                    player.PlayLooping();
                }
            }
            catch
            {


            }
        }
        private void StopAlarm()
        {
            try
            {
                player.Stop();

            }
            catch
            {
                player = null;
            }
        }




        public void GetExternalOrder()
        {
            try
            {
                //timer.Enabled = false;
                //externalClient = new ExternalOrderServiceClient(Defaults.ExternalAPIUSER, Defaults.ExternalAPIPassword);

                //var orders = externalClient.GetPendingOrdersAsync();
                //timer.Enabled = true;

                //if (orders != null && orders.Count > 0)
                //{

                //    this.Dispatcher.Invoke((Action)(() =>
                //    {
                //      //  btnPendingOrders.Content = UI.Global_External + " " + UI.Global_Pending + " Order(s) (" + orders.Count + ")";
                //        PendingDataGrid.ItemsSource = orders;
                //        PendingDataGrid.Items.Refresh();
                //        //if (externalOrdersWindow == null)
                //        //{
                //        //    externalOrdersWindow = new ExternalOrdersWindow();
                //        //    externalOrdersWindow.Show();
                //        //}
                //        //externalOrdersWindow.UpdateOrders(orders);
                //        PlayAlarm();
                //    }));
                //}
                //else
                //{
                //    this.Dispatcher.Invoke((Action)(() =>
                //    {
                //        btnPendingOrders.Content = "";
                //        PendingDataGrid.ItemsSource = null;
                //        PendingDataGrid.Items.Refresh();
                //    }));
                //    //if (externalOrdersWindow != null)
                //    //    externalOrdersWindow.UpdateOrders(orders);
                //    timer.Enabled = true;
                //    StopAlarm();
                //}

            }
            catch (Exception ex)
            {
                timer.Enabled = false;
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                timer.Enabled = true;
            }
        }



        public void SetLanguage(string lang)
        {
            Defaults.UICultureInfo = new CultureInfo(lang);
            Res.UI.Culture = (CultureInfo)Defaults.UICultureInfo;
            Properties.Resources.Culture = Res.UI.Culture;

            InitializeComponent();
            btnCheckIn.Content = UI.Main_PersonLog;

        }
        public void ShowError(string title, string content, bool okCancel = false)
         {
            List<string> lstSuccessMessages = new List<string>();
            var messages = ConfigurationManager.AppSettings["SuccessMessages"];
            if (!string.IsNullOrEmpty(messages))
            {
                lstSuccessMessages = messages.Split('|').ToList();
            }

            if (lstSuccessMessages.Contains(content))
            {
                PopupContent.Dispatcher.BeginInvoke(new Action(() => PopupContent.Background = new SolidColorBrush(Colors.Green)));
            }
            else
            {
                PopupContent.Dispatcher.BeginInvoke(new Action(() => PopupContent.Background = new SolidColorBrush(Colors.Red)));
            }
            popupTitle.Dispatcher.BeginInvoke(new Action(() => popupTitle.Text = title));
            popupContent.Dispatcher.BeginInvoke(new Action(() => popupContent.Text = content));

            btnPopupOK.Dispatcher.BeginInvoke(new Action(() => btnPopupOK.Visibility = okCancel ? Visibility.Visible : Visibility.Collapsed));
            btnPopupCancel.Dispatcher.BeginInvoke(new Action(() => btnPopupCancel.Visibility = okCancel ? Visibility.Visible : Visibility.Collapsed));
            btnPopupClose.Dispatcher.BeginInvoke(new Action(() => btnPopupClose.Visibility = !okCancel ? Visibility.Visible : Visibility.Collapsed));

            btnPopupClose.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!PopupError.IsOpen)
                {
                    PopupError.IsOpen = true;
                    this.IsEnabled = !PopupError.IsOpen;
                }
            }));

        }

        private void popupError_Loaded(object sender, RoutedEventArgs e)
        {
            PopupError.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            PopupError.IsOpen = false;
            this.IsEnabled = !PopupError.IsOpen;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            PopupError.IsOpen = false;
            this.IsEnabled = !PopupError.IsOpen;
        }

        private void PopupClose_Click(object sender, RoutedEventArgs e)
        {
            PopupError.IsOpen = false;
            this.IsEnabled = !PopupError.IsOpen;
        }

        public void OnlineImage()
        {

            var status = Defaults.RunningMode;// DBAccess.CheckConnectivity();
            var _imgpath = new ImagePathClass();
            var imgPath = "";
            bool connectionStatus = CheckForInternetConnection();
            if (connectionStatus)//Defaults.RunningMode == RunningMode.Online
            {
                imgPath = "/POSSUM;component/images/online.png";
                //OrderCompleted += MainWindow_OrderCompleted;
                txtAppMode.Text = "ONLINE";
            }
            else
            {
                imgPath = "/POSSUM;component/images/offline.png";
                txtAppMode.Text = "OFFLINE";
            }
            _imgpath.ImgPath = imgPath;
            imgCanvas.DataContext = _imgpath;
        }
        public bool CheckForInternetConnection()
        {
            try
            {
                Ping ping = new Ping();
                //PingReply pingresult = ping.Send("195.24.224.112");
                //zahid: 29/10/2022 : 8.8.8.8 google ping
                PingReply pingresult = ping.Send("8.8.8.8");
                if (pingresult.Status.ToString() == "Success")
                {
                    return true;
                }
                else
                    return false;
                //using (var client = new WebClient())
                //{
                // //   string url = "http://www.google.com";
                //    string url = "http://195.24.224.112/";
                //    using (var stream = client.OpenRead(url))
                //    {
                //        return true;
                //    }
                //}
            }
            catch
            {
                return false;
            }
        }
        private void MainWindow_OrderCompleted(object sender, Guid orderId)
        {
            try
            {

                //OrderController orderController = new OrderController();
                //orderController.UpdateOrder(orderId);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = System.Windows.SystemParameters.WorkArea.Width;//SystemParameters.MaximizedPrimaryScreenWidth - 16.0 - 2 * SystemParameters.BorderWidth;
            this.Height = System.Windows.SystemParameters.WorkArea.Height; // SystemParameters.MaximizedPrimaryScreenHeight - 16.0 - 2 * SystemParameters.BorderWidth;            
            this.Left = 0.0;
            this.Top = 0.0;
            if (this.Width == 1366)
            {

                Defaults.ScreenResulution = ScreenResulution.SR_1366X768;

            }
            else if (this.Width == 1024)
            {
                this.Width = 1024;
                Defaults.ScreenResulution = ScreenResulution.SR_1000X768;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timer != null)
                timer.Dispose();

            Environment.Exit(0);
        }

        UCUserActivity _uc;

        public void AddUserActivityMenu()
        {
            _uc = new UCUserActivity();
            _uc.HorizontalAlignment = HorizontalAlignment.Stretch;
            _uc.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            AcitivityManuGrid.Children.Clear();
            AcitivityManuGrid.Children.Add(_uc);
        }

        Guid OrderId;
        public void UpdateOrderCompleted(Guid orderId)
        {
            OrderId = orderId;
            if (OrderCompleted != null)
                OrderCompleted(this, orderId);

            PostPosMiniOrderOrderToMqtt(orderId.ToString());
        }
        UCSale _ucSale;
        public void AddControlToMainCanvas(UserControl uc)
        {
            if (_switchUserWindow != null)
                _switchUserWindow.Close();

            //new SyncController().ValidateData();
            if (uc is UCSale)
                _ucSale = uc as UCSale;

            uc.HorizontalAlignment = HorizontalAlignment.Stretch;
            uc.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            mainCanvas.Children.Clear();
            mainCanvas.Children.Add(uc);
            if (Defaults.CustomerView)
                if (uc is UCSale)
                {
                    //((UCSale)uc).ValidateEAN13();
                    if (customerViewCart != null && customerViewCart.Visibility == Visibility.Visible)
                    {

                        // customerViewCart.Show();
                    }
                    else
                    {
                        if (customerViewCart == null)
                            customerViewCart = new CustomerViewCart(this);
                        if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                        {
                            System.Windows.Forms.Screen s2 = System.Windows.Forms.Screen.AllScreens[1];
                            System.Drawing.Rectangle r2 = s2.WorkingArea;
                            customerViewCart.Top = r2.Top;
                            customerViewCart.Left = r2.Left;
                            customerViewCart.Height = r2.Height;
                            customerViewCart.Width = r2.Width;
                            customerViewCart.Show();
                        }
                        else
                        {
#if DEBUG
                            //System.Windows.Forms.Screen s1 = System.Windows.Forms.Screen.AllScreens[0];
                            //System.Drawing.Rectangle r1 = s1.WorkingArea;
                            //customerViewCart.Top = r1.Top;
                            //customerViewCart.Left = r1.Left;
                            //customerViewCart.Height = r1.Height;
                            //customerViewCart.Width = r1.Width;
                            //customerViewCart.Show();
#endif
                        }
                    }
                }
        }

        public void Reprint()
        {
            //if (PosState.GetInstance().PaymentDevice != null)	
            //{	
            //    MessageBox.Show("Sending Reprint");	

            //    PosState.GetInstance().PaymentDevice.RegisterEventHandler(new PaymentEventHandler(this, view));	
            //    // Connect to device if its not already connected	
            //    PosState.GetInstance().PaymentDevice.Connect();	
            //    PosState.GetInstance().PaymentDevice.RePrintLastCancelledPayment();	
            //}
        }

        internal void SetInsertedAmount(int value)
        {
            if (InsertedAmount != null)
                InsertedAmount(this, value);
        }
        internal void SetCGWarning(string value)
        {
            if (CGWarning != null)
                CGWarning(this, value);
        }
        internal void UpdateNewOrder(string msg)
        {

            if (NewOrder != null)
                NewOrder(this, msg);

            Defaults.PerformanceLog.Add("Ready for New Order           -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            int enableDebug = 0;
            int.TryParse(ConfigurationManager.AppSettings["DebugMode"], out enableDebug);
            if (enableDebug == 1)
            {
                LogWriter.PerformanceLog("--------------System Performance Log Started------------------");
                foreach (string line in Defaults.PerformanceLog)
                {

                    LogWriter.PerformanceLog(line);

                }
                LogWriter.PerformanceLog("--------------  End System Performance Log  ------------------");
            }
            // Defaults.PerformanceLog = new List<string>();


        }

        private void Checkin_Click(object sender, RoutedEventArgs e)
        {
            EmployeeWindow employeeWindow = new EmployeeWindow();
            employeeWindow.ShowDialog();
        }


        private void PendingOrder_Click(object sender, RoutedEventArgs e)
        {
            //var masterOrder = (LUEOrder)(sender as Button).DataContext;
            //if (masterOrder != null)
            //{
            //    var detailWindow = new LUEOrderDetailWindow(masterOrder);
            //    if (detailWindow.ShowDialog() == true)
            //    {
            //        if (LUEOrders != null && LUEOrders.Count > 0)
            //        {
            //            LUEOrders.Remove(masterOrder);
            //            PendingDataGrid.ItemsSource = null;
            //            PendingDataGrid.ItemsSource = LUEOrders;
            //            if (LUEOrders == null || LUEOrders.Count == 0)
            //            {
            //                btnPendingOrders.Content = "";
            //            }
            //        }
            //    }


            //    //StopAlarm();
            //    //IsStoped = true;

            //    //ExternalOrderDetailWindow detailWindow = new ExternalOrderDetailWindow(masterOrder.OrderGuid);
            //    //if (detailWindow.ShowDialog() == true)
            //    //    GetExternalOrder();
            //    //IsStoped = false;

            //}
        }
        private void VendorMessage_Click(object sender, RoutedEventArgs e)
        {
            //NIMPOSMessageWindow messageWindow = new NIMPOSMessageWindow();
            //if (messageWindow.ShowDialog() == true)
            //    btnVendorMessage.Visibility = Visibility.Collapsed;
        }

        public void ItemsforPublishMessage(List<OrderLine> line)
        {

        }

        public void UpdateItems(List<OrderLine> items)
        {
            //  ItemsList = items as ObservableCollection<OrderLineViewModel>();
            if (customerViewCart != null)
            {
                if (customerViewCart.Visibility == Visibility.Collapsed && System.Windows.Forms.Screen.AllScreens.Length > 1 && Defaults.CustomerView)
                {
                    customerViewCart.Visibility = Visibility.Visible;
                }
                customerViewCart.RerfreshWindow(items);

            }
            else if (customerViewCart == null && Defaults.CustomerView)
            {
                customerViewCart = new CustomerViewCart(this);
                if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                {
                    System.Windows.Forms.Screen s2 = System.Windows.Forms.Screen.AllScreens[1];
                    System.Drawing.Rectangle r2 = s2.WorkingArea;
                    customerViewCart.Top = r2.Top;
                    customerViewCart.Left = r2.Left;
                    customerViewCart.Height = r2.Height;
                    customerViewCart.Width = r2.Width;
                    customerViewCart.Show();
                }
            }
        }






        private void CGError_Click(object sender, RoutedEventArgs e)
        {
            //CashGuardWindow cgWindow = new CashGuardWindow();
            //cgWindow.ShowDialog();
        }

        #region 1 Working for Altanor Client  MQTTPossumAlive  Not Pos mini popup window in terminal to accept and reject orders

        #region MQTTPossumAlive Not Pos mini Check if possum client alive or not

        /// <summary>
        ///subscribe MQTT and publish order
        /// </summary>
        /// 

        private INotificationManager _manager;

        //send message to mobile possum is alive
        IMqttClient mqttclientAlive;
        public void InitializeMQTTPOSSUMForMobileClientAlive()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobileClientmqttclientAlive:");

                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttclientAlive = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttclientAlive.Connected += MqttclientAlive_Connected; ;
                mqttclientAlive.ApplicationMessageReceived += MqttclientAlive_ApplicationMessageReceived; ;
                mqttclientAlive.Disconnected += MqttclientAlive_Disconnected; ;
                mqttclientAlive.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void MqttclientAlive_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var message = e.ApplicationMessage.Payload;
            string result = System.Text.Encoding.UTF8.GetString(message);
            LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);

            var order = JsonConvert.DeserializeObject<PossumAlive>(result);
            var msg = order.Message.Contains("Mobile Verification");
            if (msg)
            {
                var obj = new PossumAlive
                {
                    Message = "Possum is Alive",
                    IsAlive = true,
                    Id = Guid.NewGuid(),
                    Terminal = Defaults.TerminalId,
                    OutletId = Defaults.Outlet.Id.ToString()
                };
                string orderid = JsonConvert.SerializeObject(obj);
                byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                var messages = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + "PossumClientAlive").WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag(false).Build();
                mqttclientAlive.PublishAsync(messages);
            }

        }

        private void MqttclientAlive_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            var lstTopics = new List<TopicFilter>();
            lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + "PossumClientAlive", MqttQualityOfServiceLevel.AtLeastOnce));
            mqttclientAlive.SubscribeAsync(lstTopics);
        }

        private void MqttclientAlive_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttclientAlive.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMForMobileClientAlive());
        }

        public void DisconnectMQTTClientNotMini()
        {
            try
            {

                IsTeminalClose = true;
                mqttclientAlive.DisconnectAsync();
                mqttclient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        #endregion

        #region MQTT Not PosMini Accept orders from scan app and show popup window interminal

        /// <summary>
        ///subscribe MQTT and publish order
        /// </summary>
        /// 
        IMqttClient mqttclient;
        private bool IsTeminalClose = false;

        public void InitializeMQTTPOSSUMClient()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMClient:");
                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var factory = new MqttFactory();
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                mqttclient = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(true).Build();
                mqttclient.Connected += Client_Connected;
                mqttclient.ApplicationMessageReceived += Client_ApplicationMessageReceived;
                mqttclient.Disconnected += Client_Disconnected;
                mqttclient.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                //Log.WriteLog("InitializeMQTTPOSSUMClient exception..:" + ex.ToString());
                LogWriter.LogWrite(ex);
            }
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            try
            {
                var lstTopics = new List<TopicFilter>();
                lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + "ACCEPTED_ORDER", MqttQualityOfServiceLevel.ExactlyOnce));
                mqttclient.SubscribeAsync(lstTopics);
                //LogWriter.LogWrite("MQTTClient of Possum Connected");

            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        private void Client_ApplicationMessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                Log.WriteLog("Client_ApplicationMessageReceived:");

                if (_dicOrders == null)
                {
                    _dicOrders = new Dictionary<string, NotificationViewModel>();
                }

                var message = e.ApplicationMessage.Payload;
                string result = System.Text.Encoding.UTF8.GetString(message);
                ////LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);
                _manager = new NotificationManager();

                var order = JsonConvert.DeserializeObject<POSSUM.ViewModels.Order>(result);
                //if order is cancelled
                if (order.OrderStatus == 14)
                {
                    if (_dicOrders.Count > 0)
                    {
                        var model = _dicOrders[order.OrderId];
                        model.ShowCloseButtons();
                    }
                    //_manager.Show(content, expirationTime: TimeSpan.FromMinutes(1000));
                }
                else
                {
                    //else
                    var content = new NotificationViewModel(_manager, this)
                    {
                        Title = "New Order Notification",
                        Message = "Newly order is Created from App !",
                        OrderId = order.OrderId,
                        Id = order.Id,
                        OrderTotal = order.Amount,
                        CartItems = MQTTHandler.GetCartItems(order),
                        PhoneNo = order.PhoneNo,
                        Address = order.Address,
                        Email = order.Email,
                        Tax = order.Tax,
                        CustomerName = order.CustomerName,
                        DeliveryDate = order.DeliveryDate,
                        IsPosMini = false,
                        AcceptVisibility = Visibility.Visible,
                        RejectVisibility = Visibility.Visible,
                        CloseVisibility = Visibility.Collapsed
                    };
                    try
                    {
                        _dicOrders.Add(order.OrderId, content);
                    }
                    catch (Exception)
                    {

                    }
                    if (!Convert.ToBoolean(ConfigurationManager.AppSettings["ShowNotification"]))
                    {
                        Log.WriteLog("!Convert.ToBoolean(ConfigurationManager.AppSettings[ShowNotification]:");

                        UpdateOrderStatus(order.OrderId, order.Id, true);
                    }
                    else
                    {
                        _manager.Show(content, expirationTime: TimeSpan.FromMinutes(1000));
                    }
                }

            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);
            }

        }

        private Dictionary<string, NotificationViewModel> _dicOrders;

        private void Client_Disconnected(object sender, EventArgs e)
        {
            if (!mqttclient.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMClient());
            //LogWriter.LogWrite("Client_disconnected");

        }

        public bool UpdateOrderStatus(string orderid, string id, bool IsTrue)
        {
            try
            {
                _dicOrders.Remove(orderid);
                Log.WriteLog("UpdateOrderStatus call....  " + orderid);
                // var ids = JsonConvert.DeserializeObject(orderid);
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var apiURL = ConfigurationManager.AppSettings["FoodOrderApiURL"];
                    client.BaseAddress = new Uri(apiURL);
                    var url = "UpdateOrderStatus/" + orderid + "/" + id + "/" + IsTrue;
                    var response = client.GetAsync(url).Result;
                    if (IsTrue)
                        InitializeMQTTPOSSUMForMobileClient(orderid, true);
                    else
                        InitializeMQTTPOSSUMForMobileClient(orderid, false);
                    if (response.IsSuccessStatusCode)
                    {
                        Log.WriteLog("success response from UpdateOrderStatus api");
                        return true;
                    }
                    else
                    {
                        Log.WriteLog("success response from UpdateOrderStatus api");
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Log.LogWrite(ex);
                return false;

            }
        }

        IMqttClient mqttclient2;
        private string MobileOrderId = "";
        private bool orderStatus = false;

        public void InitializeMQTTPOSSUMForMobileClient(string orderId, bool status)
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobileClient:");

                MobileOrderId = orderId;
                orderStatus = status;
                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttclient2 = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttclient2.Connected += Mqttclient2_Connected;
                mqttclient2.ApplicationMessageReceived += Mqttclient2_ApplicationMessageReceived;
                mqttclient2.Disconnected += Mqttclient2_Disconnected;
                mqttclient2.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void Mqttclient2_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            try
            {
                var lstTopics = new List<TopicFilter>();
                lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + "ACCEPTED_ORDER/" + MobileOrderId, MqttQualityOfServiceLevel.AtLeastOnce));
                mqttclient2.SubscribeAsync(lstTopics);
                //LogWriter.LogWrite("MQTTClient of Possum Mobile Clietnt Connected");
                POSSUM.ViewModels.Order orderobject = new POSSUM.ViewModels.Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = MobileOrderId,
                    Amount = 0,
                    Status = orderStatus,

                };

                if (!string.IsNullOrEmpty(MobileOrderId))
                {
                    string orderid = JsonConvert.SerializeObject(orderobject);
                    byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                    var message = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + "ACCEPTED_ORDER/" + MobileOrderId).WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag().Build();
                    mqttclient2.PublishAsync(message);
                }


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

            }
        }

        private void Mqttclient2_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var message = e.ApplicationMessage.Payload;
            string result = System.Text.Encoding.UTF8.GetString(message);
            LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);

        }

        private void Mqttclient2_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            LogWriter.LogWrite("Mobile Client_disconnected");

        }

        #endregion

        #endregion

        #region 2 Depreciated Not working MQTT_POS-MINI_Orders Directly checkout the orders and save orders to history  depreciated now use apis instead

        #region MQTT_Pos_Mini_Client_Alive depreciated now use apis instead

        IMqttClient mqttPosMiniclientAlive;

        public void InitializeMQTTPOSSUMForMobilePosMiniClientAlive()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobileClientmqttclientAlive:");

                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttPosMiniclientAlive = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttPosMiniclientAlive.Connected += MqttPosMiniclientAlive_Connected; ;
                mqttPosMiniclientAlive.ApplicationMessageReceived += MqttPosMiniclientAlive_ApplicationMessageReceived;
                mqttPosMiniclientAlive.Disconnected += MqttPosMiniclientAlive_Disconnected;
                mqttPosMiniclientAlive.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void MqttPosMiniclientAlive_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttPosMiniclientAlive.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMForMobilePosMiniClientAlive());
        }

        private void MqttPosMiniclientAlive_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var message = e.ApplicationMessage.Payload;
            string result = System.Text.Encoding.UTF8.GetString(message);
            LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);

            var order = JsonConvert.DeserializeObject<PossumAlive>(result);
            var msg = order.Message.Contains("Mobile Verification");
            if (msg)
            {
                var obj = new PossumAlive
                {
                    Message = "Possum is Alive",
                    IsAlive = true,
                    Id = Guid.NewGuid(),
                    Terminal = Defaults.TerminalId,
                    OutletId = Defaults.Outlet.Id.ToString()
                };
                string orderid = JsonConvert.SerializeObject(obj);
                byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                var messages = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + Defaults.TerminalId + "_PosMiniClientAlive").WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag(false).Build();
                mqttPosMiniclientAlive.PublishAsync(messages);
            }

        }

        private void MqttPosMiniclientAlive_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            var lstTopics = new List<TopicFilter>();
            lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + Defaults.TerminalId + "_PosMiniClientAlive", MqttQualityOfServiceLevel.AtLeastOnce));
            mqttPosMiniclientAlive.SubscribeAsync(lstTopics);

        }

        #endregion

        #region MQTT_POS-MINI_Orders Directly checkout the orders and save orders to history

        /// <summary>
        ///MINI_Orders MQTT and publish order
        /// </summary>
        /// 

        //Get message from mobile that is order and create order in possum
        IMqttClient mqttclientPosMini;

        public void InitializeMQTTPOSSUMForMobilePosMini()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobilePosMini:");

                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttclientPosMini = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttclientPosMini.Connected += MqttclientPosMiniOrder_Connected; ; ;
                mqttclientPosMini.ApplicationMessageReceived += MqttclientPosMiniOrder_ApplicationMessageReceived; ; ;
                mqttclientPosMini.Disconnected += MqttclientPosMiniOrder_Disconnected; ; ;
                mqttclientPosMini.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void MqttclientPosMiniOrder_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            var lstTopics = new List<TopicFilter>();
            lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + Defaults.TerminalId + "_PosClientMini", MqttQualityOfServiceLevel.ExactlyOnce));
            mqttclientPosMini.SubscribeAsync(lstTopics);
        }

        private void MqttclientPosMiniOrder_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttclientPosMini.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMForMobilePosMini());
        }

        public void DisconnectMQTTPosMini()
        {
            try
            {

                IsTeminalClose = true;
                mqttclientPosMini.DisconnectAsync();
            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        private void MqttclientPosMiniOrder_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var orderRepo = new OrderRepository(PosState.GetInstance().Context);

                var message = e.ApplicationMessage.Payload;
                string result = System.Text.Encoding.UTF8.GetString(message);
                LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);
                var jsonResult = JsonConvert.DeserializeObject(result).ToString();
                var order = JsonConvert.DeserializeObject<MobileOrderViewModel>(jsonResult);

                var existingOrder = orderRepo.GetOrderMaster(order.Order.Id);

                if (existingOrder == null)
                {
                    PosMiniOrder window = new PosMiniOrder();
                    var res = window.HandleCheckOutClick(order.Order, order.AccountAmount, order.TipAmount, UI.CheckOutOrder_Method_Account);
                    if (res)
                    {
                        var obj = new PosMiniOrderStatus
                        {
                            OrderStatus = true,
                            Id = Guid.NewGuid(),
                            OrderId = order.Order.Id,
                            Status = order.Order.Status,
                            InvoiceNumber = order.Order.InvoiceNumber,
                            OrderNumber = order.Order.OrderNumber,
                            ReceiptNumber = order.Order.ReceiptNumber
                        };
                        string orderid = JsonConvert.SerializeObject(obj);
                        byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                        var messages = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + Defaults.TerminalId + "_PosClientMini/" + order.Order.Id).WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag().Build();
                        mqttclientPosMini.PublishAsync(messages);

                        this.Dispatcher.BeginInvoke(new Action(() => _ucSale.Presenter.UpdateHistoryGrid()));


                    }

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Client_ApplicationMessageReceived exception " + ex.ToString());

            }

        }

        #endregion

        #endregion

        #region 3 New MQTT PosMini For Order Show ordres in Cart New 

        /// <summary>
        ///This method is use to get orders from pos mini and show popup on terminal screen and after accept show order in cart on terminal
        /// </summary>
        /// 
        IMqttClient mqttclientPosMiniOrderCheckout;
        private Dictionary<string, NotificationViewModel> _posMiniDicOrders;

        public void InitializeMQTTPOSSUMClientMiniOrderInCart()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMClientMiniOrderInCart:");
                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var factory = new MqttFactory();
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                mqttclientPosMiniOrderCheckout = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(true).Build();
                mqttclientPosMiniOrderCheckout.Connected += MqttclientPosMiniOrderCheckout_Connected; ;
                mqttclientPosMiniOrderCheckout.ApplicationMessageReceived += MqttclientPosMiniOrderCheckout_ApplicationMessageReceived; ;
                mqttclientPosMiniOrderCheckout.Disconnected += MqttclientPosMiniOrderCheckout_Disconnected; ;
                mqttclientPosMiniOrderCheckout.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                //Log.WriteLog("InitializeMQTTPOSSUMClient exception..:" + ex.ToString());
                LogWriter.LogWrite(ex);
            }
        }

        private void MqttclientPosMiniOrderCheckout_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            try
            {
                var topic = Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ACCEPTED_ORDER";
                var PendingOrderTopic = Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/PENDING_ORDER_STATUS";
                var lstTopics = new List<TopicFilter>();
                lstTopics.Add(new TopicFilter(topic, MqttQualityOfServiceLevel.ExactlyOnce));
                lstTopics.Add(new TopicFilter(PendingOrderTopic, MqttQualityOfServiceLevel.ExactlyOnce));
                mqttclientPosMiniOrderCheckout.SubscribeAsync(lstTopics);
                Log.WriteLog("Posmini client connected with topic: " + topic);

            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        private void MqttclientPosMiniOrderCheckout_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttclientPosMiniOrderCheckout.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMClientMiniOrderInCart());
            //LogWriter.LogWrite("Client_disconnected");
        }

        private void MqttclientPosMiniOrderCheckout_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine("MqttclientPosMiniOrderCheckout_ApplicationMessageReceived");
                Log.WriteLog("MqttclientPosMiniOrderCheckout_ApplicationMessageReceived:");

                if (e == null || e.ApplicationMessage == null || string.IsNullOrEmpty(e.ApplicationMessage.Topic))
                    return;

                var message = e.ApplicationMessage.Payload;
                string result = System.Text.Encoding.UTF8.GetString(message);

                if (e.ApplicationMessage.Topic.Contains("/POSMINI/ACCEPTED_ORDER"))
                {
                    if (_posMiniDicOrders == null)
                    {
                        _posMiniDicOrders = new Dictionary<string, NotificationViewModel>();
                    }

                    //var message = e.ApplicationMessage.Payload;
                    //string result = System.Text.Encoding.UTF8.GetString(message);
                    //LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);

                    _manager = new NotificationManager();

                    var order = JsonConvert.DeserializeObject<POSSUM.Model.Order>(result);
                    //if order is cancelled
                    if (order.Status == OrderStatus.OrderCancelled)
                    {
                        if (_posMiniDicOrders.Count > 0)
                        {
                            var model = _posMiniDicOrders[order.Id.ToString()];
                            model.ShowCloseButtons();
                        }
                        //_manager.Show(content, expirationTime: TimeSpan.FromMinutes(1000));
                    }
                    else
                    {
                        //else
                        var content = new NotificationViewModel(_manager, this)
                        {
                            Title = "New PosMini Order Notification",
                            Message = "Newly PosMini order is Created from App !",
                            Order = order,
                            OrderId = order.Id.ToString(),
                            Id = order.Id.ToString(),
                            DeliveryDate = order.DeliveryDate,
                            CustomerName = (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Name)) ? order.Customer.Name : "",
                            PhoneNo = (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Phone)) ? order.Customer.Phone : "",
                            Email = (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Email)) ? order.Customer.Email : "",
                            Address = (order.Customer != null && !string.IsNullOrEmpty(GetCustomerAddress(order.Customer))) ? GetCustomerAddress(order.Customer) : "",
                            OrderTotal = order.OrderTotal,
                            Tax = Math.Round(order.TaxPercent, 2),
                            IsPosMini = true,
                            DeliveryDateVisibility = order.DeliveryDate != null ? Visibility.Visible : Visibility.Collapsed,
                            CustomerNameVisibility = (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Name)) ? Visibility.Visible : Visibility.Collapsed,
                            CustomerPhoneVisibility = (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Phone)) ? Visibility.Visible : Visibility.Collapsed,
                            CustomerEmailVisibility = (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Email)) ? Visibility.Visible : Visibility.Collapsed,
                            CustomerAddressVisibility = (order.Customer != null && !string.IsNullOrEmpty(GetCustomerAddress(order.Customer))) ? Visibility.Visible : Visibility.Collapsed,
                            AcceptVisibility = Visibility.Visible,
                            RejectVisibility = Visibility.Visible,
                            CloseVisibility = Visibility.Collapsed
                        };
                        try
                        {
                            order.Status = OrderStatus.Pending;
                            order.DailyBongCounter = Defaults.DailyBongCounter;
                            MQTTHandler.SavePauseOrder(order);

                            _posMiniDicOrders.Add(order.Id.ToString(), content);
                        }
                        catch (Exception)
                        {

                        }
                        if (!Convert.ToBoolean(ConfigurationManager.AppSettings["ShowNotification"]))
                        {
                            Log.WriteLog("!Convert.ToBoolean(ConfigurationManager.AppSettings[ShowNotification]:");

                            UpdatePosMiniOrderStatus(order.Id.ToString(), order.Id.ToString(), true);
                        }
                        else
                        {
                            _manager.Show(content, expirationTime: TimeSpan.FromMinutes(1000));
                        }
                    }
                }
                else if (e.ApplicationMessage.Topic.Contains("/POSMINI/PENDING_ORDER_STATUS"))
                {
                    var orderIds = JsonConvert.DeserializeObject<string[]>(result);

                    foreach (var orderId in orderIds)
                    {
                        PostPosMiniOrderOrderToMqtt(orderId);
                    }
                }

            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);
            }
        }

        public bool UpdatePosMiniOrderStatus(string orderid, string id, bool IsTrue)
        {
            try
            {
                _posMiniDicOrders.Remove(orderid);
                Log.WriteLog("UpdateOrderStatus call....  " + orderid);
                if (IsTrue)
                    MQTTHandler.InitializeMQTTPOSSUMForMobileClientPosMiniOrderStatus(orderid, true);

                else
                    MQTTHandler.InitializeMQTTPOSSUMForMobileClientPosMiniOrderStatus(orderid, false);
                return true;
            }

            catch (Exception ex)
            {
                Log.LogWrite(ex);
                return false;

            }
        }

        public void PostPosMiniOrderOrderToMqtt(string orderId)
        {
            try
            {
                var isMqttForPosMini = string.IsNullOrEmpty(ConfigurationManager.AppSettings["ISMQTTFORPOSMINI"]) ? 0 : Convert.ToInt32(ConfigurationManager.AppSettings["ISMQTTFORPOSMINI"]);
                if (isMqttForPosMini == 0)
                    return;

                if (!mqttclientPosMiniOrderCheckout.IsConnected && IsTeminalClose)
                    InitializeMQTTPOSSUMClientMiniOrderInCart();

                var orderToReturn = GetPendingOrders(orderId);
                if (orderToReturn != null)
                {
                    string orderJson = JsonConvert.SerializeObject(orderToReturn);
                    byte[] orderBytes = Encoding.UTF8.GetBytes(orderJson);
                    string orderTopic = Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ACCEPTED_ORDER/" + orderToReturn.Id;
                    var messages = new MqttApplicationMessageBuilder().WithTopic(orderTopic).WithPayload(orderBytes).WithExactlyOnceQoS().WithRetainFlag().WithRetainFlag(false).Build();
                    mqttclientPosMiniOrderCheckout.PublishAsync(messages);
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex);
            }
        }

        public Model.Order GetPendingOrders(string orderId)
        {
            try
            {
                var orderToReturn = new OrderRepository(PosState.GetInstance().Context).GetOrderMasterDetailById(Guid.Parse(orderId));
                if (orderToReturn != null)
                    return orderToReturn;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex);
                return null;
            }
        }

        public void DisconnectMQTTClientPosMini()
        {
            try
            {

                IsTeminalClose = true;
                mqttclientPosMiniOrderCheckout.DisconnectAsync();
            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        #endregion

        #region 4 New MQTT Pos Online App Orders For nothing will happen just accept and reject order here

        /// <summary>
        ///This method is use to get orders from pos mini and show popup on terminal screen and after accept show order in cart on terminal
        /// </summary>
        /// 
        IMqttClient mqttclientPosOnlineOrderCheckout;
        private Dictionary<string, NotificationViewModel> _posOnlineDicOrders;

        public void InitializeMQTTPOSSUMClientOnlineOrder()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMClientOnlineOrder:");
                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var factory = new MqttFactory();
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                mqttclientPosOnlineOrderCheckout = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(true).Build();
                mqttclientPosOnlineOrderCheckout.Connected += MqttclientPosOnlineOrderCheckout_Connected;
                mqttclientPosOnlineOrderCheckout.ApplicationMessageReceived += MqttclientPosOnlineOrderCheckout_ApplicationMessageReceived;
                mqttclientPosOnlineOrderCheckout.Disconnected += MqttclientPosOnlineOrderCheckout_Disconnected;
                mqttclientPosOnlineOrderCheckout.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                //Log.WriteLog("InitializeMQTTPOSSUMClient exception..:" + ex.ToString());
                LogWriter.LogWrite(ex);
            }
        }

        private void MqttclientPosOnlineOrderCheckout_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            try
            {
                var topic = Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/ONLINEORDER/ACCEPTED_ORDER";
                var lstTopics = new List<TopicFilter>();
                lstTopics.Add(new TopicFilter(topic, MqttQualityOfServiceLevel.ExactlyOnce));
                mqttclientPosOnlineOrderCheckout.SubscribeAsync(lstTopics);
                Log.WriteLog("Pos online order connected with topic:" + topic);

            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        private void MqttclientPosOnlineOrderCheckout_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttclientPosOnlineOrderCheckout.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMClientOnlineOrder());
            //LogWriter.LogWrite("Client_disconnected");
        }

        private void MqttclientPosOnlineOrderCheckout_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine("MqttclientPosMiniOrderCheckout_ApplicationMessageReceived");
                Log.WriteLog("MqttclientPosOnlineOrderCheckout_ApplicationMessageReceived:");

                if (_posOnlineDicOrders == null)
                {
                    _posOnlineDicOrders = new Dictionary<string, NotificationViewModel>();
                }

                var message = e.ApplicationMessage.Payload;
                string result = System.Text.Encoding.UTF8.GetString(message);
                ////LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);
                _manager = new NotificationManager();

                var order = JsonConvert.DeserializeObject<POSSUM.Model.Order>(result);
                //if order is cancelled
                if (order.Status == OrderStatus.OrderCancelled)
                {
                    if (_posOnlineDicOrders.Count > 0)
                    {
                        var model = _posOnlineDicOrders[order.Id.ToString()];
                        model.ShowCloseButtons();
                    }
                    //_manager.Show(content, expirationTime: TimeSpan.FromMinutes(1000));
                }
                else
                {
                    //else
                    var content = new NotificationViewModel(_manager, this)
                    {
                        Title = "New Online Order Notification Order# " + order.OrderIntID,
                        Message = "Newly Online order is Created from App !",
                        Order = order,
                        OrderId = order.Id.ToString(),
                        Id = order.Id.ToString(),
                        OrderTotal = Math.Round(order.OrderTotal, 2),
                        PhoneNo = order.Customer.Phone,
                        Address = order.Customer.Address1 + " " + order.Customer.Address2,
                        Email = order.Customer.Email,
                        Tax = Math.Round(order.VatAmount, 2),
                        IsOnlineOrder = true,
                        CustomerName = "",
                        DeliveryDate = order.DeliveryDate,
                        AcceptVisibility = Visibility.Visible,
                        RejectVisibility = Visibility.Visible,
                        CloseVisibility = Visibility.Collapsed
                    };
                    try
                    {
                        _posOnlineDicOrders.Add(order.Id.ToString(), content);
                    }
                    catch (Exception)
                    {

                    }
                    if (!Convert.ToBoolean(ConfigurationManager.AppSettings["ShowNotification"]))
                    {
                        Log.WriteLog("!Convert.ToBoolean(ConfigurationManager.AppSettings[ShowNotification]:");
                        UpdateOnlineOrderStatus(order.Id.ToString(), order.Id.ToString(), true);
                    }
                    else
                    {
                        _manager.Show(content, expirationTime: TimeSpan.FromMinutes(1000));
                    }
                }

            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);
            }
        }

        //update online order from terminal to server and also send status back to mobile 
        public bool UpdateOnlineOrderStatus(string orderid, string id, bool IsTrue)
        {
            try
            {
                _posOnlineDicOrders.Remove(orderid);
                Log.WriteLog("UpdateOnlineOrderStatus call....  " + orderid);
                var status = IsTrue ? (int)OrderStatus.Completed : (int)OrderStatus.Rejected;
                SyncPOSController syncController = new SyncPOSController();
                var res = syncController.SyncOnlineOrdersSync(orderid, status, Defaults.SyncAPIUri, Defaults.APIUSER, Defaults.APIPassword);
                if (IsTrue)
                    MQTTHandler.InitializeMQTTPOSSUMForMobileClientOnlineOrderStatus(orderid, true);
                else
                    MQTTHandler.InitializeMQTTPOSSUMForMobileClientOnlineOrderStatus(orderid, false);
                if (res)
                {
                    Log.WriteLog("success response from UpdateOrderStatus api");
                    return true;
                }
                else
                {
                    Log.WriteLog("success response from UpdateOrderStatus api");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Log.LogWrite(ex);
                return false;

            }
        }

        public void DisconnectMQTTClientPosOnline()
        {
            try
            {
                IsTeminalClose = true;
                mqttclientPosOnlineOrderCheckout.DisconnectAsync();
                mqttclientPosMiniDirCheckoutOrder.DisconnectAsync();
            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        #endregion

        #region 5 MQTT_POS-MINI_Orders Directly checkout the orders and save orders to history

        /// <summary>
        ///MINI_Orders MQTT and publish order
        /// </summary>
        /// 

        //Get message from mobile that is order and create order in possum
        IMqttClient mqttclientPosMiniDirCheckoutOrder;

        public void InitializeMQTTPOSSUMForMobilePosMiniDirCheckoutOrder()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobilePosMini:");

                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttclientPosMiniDirCheckoutOrder = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttclientPosMiniDirCheckoutOrder.Connected += MqttclientPosMiniDirCheckoutOrder_Connected; ;
                mqttclientPosMiniDirCheckoutOrder.ApplicationMessageReceived += MqttclientPosMiniDirCheckoutOrder_ApplicationMessageReceived; ;
                mqttclientPosMiniDirCheckoutOrder.Disconnected += MqttclientPosMiniDirCheckoutOrder_Disconnected; ;
                mqttclientPosMiniDirCheckoutOrder.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void MqttclientPosMiniDirCheckoutOrder_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {

            try
            {
                var orderRepo = new OrderRepository(PosState.GetInstance().Context);

                var message = e.ApplicationMessage.Payload;
                string result = System.Text.Encoding.UTF8.GetString(message);
                LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);
                var jsonResult = JsonConvert.DeserializeObject(result).ToString();

                if (jsonResult.Contains("Mobile Verification"))
                {
                    var obj = new PossumAlive
                    {
                        Message = "Possum is Alive",
                        IsAlive = true,
                        Id = Guid.NewGuid(),
                        Terminal = Defaults.TerminalId,
                        OutletId = Defaults.Outlet.Id.ToString()
                    };
                    string orderid = JsonConvert.SerializeObject(obj);
                    byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                    var messages = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ONLINEORDER/DIRCHECK/ISONLINE").WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag(false).Build();
                    mqttclientPosMiniDirCheckoutOrder.PublishAsync(messages);
                }
                else if (!jsonResult.Contains("Possum is Alive"))
                {
                    var order = JsonConvert.DeserializeObject<MobileOrderViewModel>(jsonResult);

                    var existingOrder = orderRepo.GetOrderMaster(order.Order.Id);

                    if (existingOrder == null)
                    {
                        PosMiniOrder window = new PosMiniOrder();
                        var res = window.HandleCheckOutClick(order.Order, order.AccountAmount, order.TipAmount, order.PaymentRefType, false);
                        if (res)
                        {
                            //var receipt = new ReceiptHandler().GetByOrderId(order.Order.Id);
                            var orderToReturn = new OrderRepository(PosState.GetInstance().Context).GetOrderMasterDetailById(order.Order.Id);
                            if (orderToReturn != null)
                            {
                                string orderid = JsonConvert.SerializeObject(orderToReturn);
                                byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                                string orderTopic = Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ACCEPTED_ORDER/" + order.Order.Id;
                                var messages = new MqttApplicationMessageBuilder().WithTopic(orderTopic).WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag().WithRetainFlag(false).Build();
                                mqttclientPosMiniDirCheckoutOrder.PublishAsync(messages);

                            }

                            this.Dispatcher.BeginInvoke(new Action(() => _ucSale.Presenter.UpdateHistoryGrid()));
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("Client_ApplicationMessageReceived exception " + ex.ToString());

            }
        }

        private void MqttclientPosMiniDirCheckoutOrder_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttclientPosMiniDirCheckoutOrder.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMForMobilePosMiniDirCheckoutOrder());
        }

        private void MqttclientPosMiniDirCheckoutOrder_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            var lstTopics = new List<TopicFilter>();
            lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ONLINEORDER/DIRCHECK/ACCEPTED_ORDER", MqttQualityOfServiceLevel.ExactlyOnce));
            lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ONLINEORDER/DIRCHECK/ISONLINE", MqttQualityOfServiceLevel.ExactlyOnce));
            mqttclientPosMiniDirCheckoutOrder.SubscribeAsync(lstTopics);
        }


        public void DisconnectMQTTPosMiniDirCheckoutOrder()
        {
            try
            {

                IsTeminalClose = true;
                mqttclientPosMini.DisconnectAsync();
            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }

        #endregion

        public string GetCustomerAddress(Customer customer)
        {
            string customerAddress = "";

            if (!string.IsNullOrEmpty(customer.Address1))
                customerAddress = customer.Address1;
            if (!string.IsNullOrEmpty(customer.Address2))
                customerAddress = !string.IsNullOrEmpty(customerAddress) ? (customerAddress + ", " + customer.Address2) : customer.Address2;
            if (!string.IsNullOrEmpty(customer.City))
                customerAddress = !string.IsNullOrEmpty(customerAddress) ? (customerAddress + ", " + customer.City) : customer.City;

            customerAddress = !string.IsNullOrEmpty(customerAddress) ? (customerAddress + ", Floor: " + customer.FloorNo) : ("Floor: " + customer.FloorNo);

            if (!string.IsNullOrEmpty(customer.ZipCode))
                customerAddress = !string.IsNullOrEmpty(customerAddress) ? (customerAddress + ", ZipCode: " + customer.ZipCode) : ("ZipCode: " + customer.FloorNo);

            return customerAddress;
        }

        private void btnQuitApp_Click(object sender, RoutedEventArgs e)
        {
           this.Close();
        }

        SwitchUserWindow _switchUserWindow;

        private void btnUserInfo_Click(object sender, RoutedEventArgs e)
        {
            _switchUserWindow = new SwitchUserWindow();
            _switchUserWindow.ShowDialog();
            //_uc.LogOut();
        }

        public void LogOut()
        {
            _uc.LogOut();
        }
    }
}
