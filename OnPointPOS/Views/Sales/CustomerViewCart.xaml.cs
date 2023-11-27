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
using POSSUM.Events;
using POSSUM.Model;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

using System.Collections.ObjectModel;
using System.Configuration;

namespace POSSUM.Views.Sales
{
    /// <summary>
    /// Interaction logic for CustomerViewCart.xaml
    /// </summary>
    public partial class CustomerViewCart : Window
    {
        MainWindow mainWindow;
        public CustomerViewCart(MainWindow mw)
        {
            InitializeComponent();
            mainWindow = mw;
            this.Title = Defaults.AppProvider.AppTitle;
            mainWindow.AddItemUpdated += MainWindow_AddItemUpdated;
            mainWindow.NewOrder += MainWindow_NewOrder;
            myBrowser.Navigated += MyBrowser_Navigated;
            myBrowser2.Navigated += MyBrowser2_Navigated;

        }

        private void MyBrowser2_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SetSilent(myBrowser2, true);
        }

        public ObservableCollection<OrderLine> ObservableItemsList
        {
            get
            {
                return mainWindow.ItemsList;
            }
        }
        private void MyBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SetSilent(myBrowser, true);
        }

        private void MainWindow_NewOrder(object sender, string msg)
        {
            //   Console.WriteLine("Updateting customer view->" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            //   lvCart.ItemsSource = null;
            CalculatOrderTotal(new List<OrderLine>());
            ActiveOrderGrid.Visibility = Visibility.Collapsed;
            SlideShowGrid.Visibility = Visibility.Visible;
            QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(Defaults.SlideShowURL))
                myBrowser.Navigate(Defaults.SlideShowURL);
            //  Console.WriteLine("Customer view updated->" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

            //try
            //{
            //    GC.Collect();
            //    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            //}
            //catch
            //{

            //}
        }
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }
        public static void SetSilent2(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("08311487-1F93-4943-8E00-F9433759B174");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        private void MainWindow_AddItemUpdated(object sender, List<OrderLine> args)
        {
            try
            {
                ActiveOrderGrid.Visibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(Defaults.SlideShowURL))
                {
                    SlideShowGridURL2.Visibility = Visibility.Visible;
                    QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;
                    myBrowser2.Navigate(Defaults.SlideShowURL);
                    myBrowser2.Visibility = Visibility.Visible;
                    double Width = this.Width;
                    myBrowser2.Width = Width / 2;
                }
                else
                {
                    SlideShowGridURL2.Visibility = Visibility.Collapsed;
                    QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;
                    myBrowser2.Visibility = Visibility.Collapsed;
                }
                SlideShowGrid.Visibility = Visibility.Collapsed;
                QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;
                //lvCart.ItemsSource = null;
                //lvCart.ItemsSource = args;
                UpdateItemList(args);
                CalculatOrderTotal(args);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }



        }
        public void RerfreshWindow(IList<OrderLine> lst)
        {

            ActiveOrderGrid.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(Defaults.SlideShowURL))
            {
                SlideShowGridURL2.Visibility = Visibility.Visible;
                QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;
                myBrowser2.Navigate(Defaults.SlideShowURL);
                myBrowser2.Visibility = Visibility.Visible;
                double Width = this.Width;
                myBrowser2.Width = Width / 2;
            }
            else
            {
                SlideShowGridURL2.Visibility = Visibility.Collapsed;
                QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;
                myBrowser2.Visibility = Visibility.Collapsed;
            }
            SlideShowGrid.Visibility = Visibility.Collapsed;
            QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;
            UpdateItemList(lst);
            //lvCart.Items.Refresh();// = lst;
            CalculatOrderTotal(lst);
        }
        private void UpdateItemList(IList<OrderLine> lst)
        {
            lvCart.ItemsSource = null;
            lvCart.ItemsSource = lst;
            try
            {
                if (lvCart.Items.Count > 0)
                {
                    lvCart.SelectedIndex = lvCart.Items.Count - 1;
                    lvCart.ScrollIntoView(lvCart.SelectedItem);
                }
            lvCart.Items.Refresh();
                
            }
            catch
            {
            }
        }
        public void CalculatOrderTotal(IList<OrderLine> lst)
        {
            try
            {
                decimal OrderTotal = 0;
                decimal tax = 0;
                foreach (var item in lst)
                {
                    if (item.IsValid)
                    {
                        if (item.Product.ItemType == ItemType.Individual)
                            tax += item.VatAmount();
                        OrderTotal += item.GrossAmountDiscounted();
                        if (item.IngredientItems != null)
                            foreach (var ingredient in item.IngredientItems)
                            {
                                tax += ingredient.VatAmount();
                                OrderTotal += ingredient.GrossAmountDiscounted();
                            }
                    }
                }
                foreach (var detail in lst.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
                {
                    // var vatInnerGroups = detail.OrderItemDetails.GroupBy(od => od.TAX);
                    if (detail.ItemDetails != null)
                    {
                        foreach (var itm in detail.ItemDetails)
                        {

                            decimal vatAmount = itm.VatAmount();
                            tax += vatAmount;
                        }
                    }
                }
                decimal nettotal = OrderTotal - tax;
                lblGrossTotal.Text = nettotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                lblVatTotal.Text = tax.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                lblTotal.Text = OrderTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                var discountTotal = lst.Where(a => a.Active == 1).Sum(s => s.ItemDiscount);
                if (discountTotal > 0)
                {
                    lblDiscountTotal.Visibility = Visibility.Visible;
                    txtDiscountTotal.Visibility = Visibility.Visible;
                    txtDiscountTotal.Text = discountTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol);
                }
                else
                {
                    lblDiscountTotal.Visibility = Visibility.Collapsed;
                    txtDiscountTotal.Visibility = Visibility.Collapsed;
                    txtDiscountTotal.Text = "0";
                }
                lblGrossTotal.Dispatcher.BeginInvoke(new Action(() => lblGrossTotal.Text = nettotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
                lblVatTotal.Dispatcher.BeginInvoke(new Action(() => lblVatTotal.Text = tax.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
                lblTotal.Dispatcher.BeginInvoke(new Action(() => lblTotal.Text = OrderTotal.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow_NewOrder(this, "New Order");
        }

        public void LoadSwishQRCode()
        {
            var imgpath = ConfigurationManager.AppSettings["QRCodeSlideShowURL"];
            if (string.IsNullOrEmpty(imgpath))
                return;
            SlideShowGrid.Visibility = Visibility.Collapsed;
            ActiveOrderGrid.Visibility = Visibility.Collapsed;
            QRCodeSlideShowGrid.Visibility = Visibility.Visible;
            BitmapImage image = new BitmapImage(new Uri(imgpath,
                                             UriKind.Absolute));
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.Freeze();

            QRCodeSlideShowGrid_Image.Source = image;

        }

        public void UnloadSwishQRCode()
        {
            SlideShowGrid.Visibility = Visibility.Collapsed;
            ActiveOrderGrid.Visibility = Visibility.Visible;
            QRCodeSlideShowGrid.Visibility = Visibility.Collapsed;

        }
    }
}
