using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Globalization;
using System.Windows.Media;
using POSSUM.Integration;
using POSSUM.Model;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace POSSUM
{ 
    public static class UtilitiesIntegrations
    {
        /// <summary>
        /// Extension method for casting IList or IEnumerable to Observalable collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            var col = new ObservableCollection<T>();
            foreach (var cur in enumerable)
            {
                col.Add(cur);
            }
            return col;
        }
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        public static string GetMySQLDateTimeFormat(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception exp)
            {
                IntegrationLogWriter.LogWrite("GetMySQLDateTimeFormat >> " + exp);
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        public static string GetMySQLDateFormat(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("yyyy-MM-dd");
            }
            catch (Exception exp)
            {
                IntegrationLogWriter.LogWrite("GetMySQLDateTimeFormat >> " + exp);
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }
        public static string GetStandardDateFormat(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("dd-MM-yyyy");
            }
            catch (Exception exp)
            {
                IntegrationLogWriter.LogWrite("Exception >> " + exp);
                return DateTime.Now.ToString("dd-MM-yyyy");
            }
        }
        /// <summary>
        /// Get the local machines physical address
        /// </summary>
        public static string GetPhysicalAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }
            return macAddress;
        }
        public static decimal AmountParse(String text)
        {
            decimal result;
            if (Decimal.TryParse(text, NumberStyles.Currency, DefaultsIntegration.UICultureInfo, out result))
                return result;
            else
                throw new ArgumentException("Felaktigt nummerformat");
        }

        //public static String PromptInput(String title, String description, String defaultValue, bool multiline = true)
        //{
        //    var config = new PromptInfo.PromptInfoPresenter.PromptInfoConfig()
        //    {
        //        Title = title,
        //        Description = description,
        //        PromptType = multiline ? PromptInfo.PromptInfoPresenter.PromptType.TEXT_MULTI_LINE : PromptInfo.PromptInfoPresenter.PromptType.TEXT_SINGLE_LINE,
        //        Value = defaultValue
        //    };
        //    var w = new PromptInfoWindow(config);
        //    if (w.ShowDialog() ?? false)
        //        return config.Value;
        //    return null;
        //}
        public static BitmapImage LoadImageFromByte(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
        public static bool CheckDeviceBamboraConnection()
        {
            //try
            //{


            //return  PosState.GetInstance().PaymentDevice.IsConnected();
            //}
            //catch (Exception ex)
            //{
            //    LogWriter.LogWrite(ex);
            return false;
            //}
        }
        public static bool PerfromResetOnPyamentDevice()
        {
            //try
            //{


            //PosState.GetInstance().PaymentDevice.Connect();
            // return PosState.GetInstance().PaymentDevice.IsConnected();
            //}
            //catch (Exception ex)
            //{
            //    LogWriter.LogWrite(ex);
            return false;
            //}
            ////
        }
        //public static PaymentTransactionStatus ProcessPaymentPurchase(decimal totalAmount, decimal vatAmount, decimal cashbackAmount, int tableId, Guid orderId)
        //{

        //    //if (Defaults.PaymentDeviceType == PaymentDeviceType.CONNECT2T && Defaults.DeviceLog)
        //    //    LogWriter.DeviceLog(orderId, totalAmount, vatAmount, cashbackAmount);
        //    var status = new PaymentTransactionStatus { TransactionType = PaymentTransactionType.PURCHASE };
        //    try
        //    {


        //        var pw = new PaymentWindow(totalAmount, vatAmount, cashbackAmount, status, tableId,orderId);

        //        if (PosState.GetInstance().PaymentDevice == null) // paymentdevice is set to NONE
        //        {
        //            status.Result = PaymentTransactionStatus.PaymentResult.NO_DEVICE_CONFIGURED;
        //            return status;
        //        }

        //        // If dialog was cancelled or closed in some way not normal, set as if device cancelled
        //        if (!(pw.ShowDialog() ?? false))
        //        {
        //            status.Result = PaymentTransactionStatus.PaymentResult.CANCELLED;
        //        }
        //        else
        //        {
        //            /* if (status.MerchantReceipt != null)
        //            {
        //                var b = new StringBuilder();
        //                status.MerchantReceipt.ToList().ForEach(r => b.AppendLine(r));
        //                var prwi = new printReportWindow(b.ToString(), ReportType.PaymentPurchase);
        //                prwi.ShowDialog();
        //            }

        //            if (status.CustomerReceipt != null)
        //            {
        //                StringBuilder b = new StringBuilder();
        //                status.CustomerReceipt.ToList().ForEach(r => b.AppendLine(r));
        //                printReportWindow prwi = new printReportWindow(b.ToString());
        //                prwi.ShowDialog();
        //            }*/
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogWriter.LogException(ex);
        //    }
        //    return status;
        //}

        //internal static PaymentTransactionStatus ProcessPaymentRefund(decimal totalAmount, decimal vatAmount, decimal cashbackAmount, int tableId, Guid orderId)
        //{
        //    //if (Defaults.PaymentDeviceType == PaymentDeviceType.CONNECT2T && Defaults.DeviceLog)
        //    //    LogWriter.DeviceLog(orderId, totalAmount, vatAmount, cashbackAmount);
        //    var status = new PaymentTransactionStatus { TransactionType = PaymentTransactionType.REFUND };
            
        //    var pw = new PaymentWindow(totalAmount, vatAmount, cashbackAmount, status, tableId,orderId);

        //    if (PosState.GetInstance().PaymentDevice == null) // paymentdevice is set to NONE
        //    {
        //        status.Result = PaymentTransactionStatus.PaymentResult.NO_DEVICE_CONFIGURED;
        //        return status;
        //    }

        //    // If dialog was cancelled or closed in some way not normal, set as if device cancelled
        //    if (!(pw.ShowDialog() ?? false))
        //    {
        //        status.Result = PaymentTransactionStatus.PaymentResult.CANCELLED;
        //    }
        //    else
        //    {
        //        /* if (status.MerchantReceipt != null)
        //        {
        //            var b = new StringBuilder();
        //            status.MerchantReceipt.ToList().ForEach(r => b.AppendLine(r));
        //            var prwi = new printReportWindow(b.ToString(), ReportType.PaymentRefund);
        //            prwi.ShowDialog();
        //        }
               
        //        if (status.CustomerReceipt != null)
        //        {
        //            StringBuilder b = new StringBuilder();
        //            status.CustomerReceipt.ToList().ForEach(r => b.AppendLine(r));
        //            printReportWindow prwi = new printReportWindow(b.ToString());
        //            prwi.ShowDialog();
        //        }*/
        //    }
        //    return status;
        //}
        //FOR PRODUCT/CATEGORIES
        public static SolidColorBrush GetColorBrush(string colorcode)
        {
            if (!string.IsNullOrEmpty(colorcode) && colorcode.StartsWith("#") && colorcode.Length >= 7)
            {
                var brush = (Brush)new BrushConverter().ConvertFromString(colorcode);
                return new SolidColorBrush(((SolidColorBrush)brush).Color);
            }
            else
            {
                var brush = (Brush)new BrushConverter().ConvertFromString("#FFDCDEDE");
                return new SolidColorBrush(((SolidColorBrush)brush).Color);
            }
        }
        //TODO: TRY TO COMBINE WITH ABOVE FUNCTION
        public static SolidColorBrush GetItemStatusColorBrush(int id)
        {
            switch (id)
            {
                case 0:
                    var brush = (Brush)new BrushConverter().ConvertFromString("#FF035274");
                    return new SolidColorBrush(((SolidColorBrush)brush).Color);
                case 3:
                    return new SolidColorBrush(Colors.Red);
                case 4:
                    return new SolidColorBrush(Colors.Orange);
                case 5:
                    return new SolidColorBrush(Colors.Green);
                default:
                    return new SolidColorBrush(Colors.IndianRed);
            }
        }

        public static SolidColorBrush GetColorBrushFromOrderStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    return new SolidColorBrush(Colors.White);
                case OrderStatus.AssignedKitchenBar:
                    return new SolidColorBrush(Colors.Yellow);
                case OrderStatus.OrderAvailabilityComplete:
                    return new SolidColorBrush(Colors.LightGreen);
                case OrderStatus.Served:
                    return new SolidColorBrush(Colors.LightBlue);
                case OrderStatus.Completed:
                    return new SolidColorBrush(Colors.IndianRed);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }


        public static LocalSetting ReadLocalSettings(string userName)
        {
            try
            {
                XElement main = XElement.Load(@"settings.xml");

                var results = main.Descendants("Terminal")
                        .Where(e => e.Attribute("UserName").Value == userName)
                    .Select(e => new LocalSetting
                    {

                        ConnectionString = e.Descendants("ConnectionString").FirstOrDefault().Value,
                        TerminalId = e.Descendants("TerminalId").FirstOrDefault().Value,
                        PaymentDevicConnectionString = e.Descendants("PaymentDevicConnectionString").FirstOrDefault().Value,
                        PaymentDeviceType = e.Descendants("PaymentDeviceType").FirstOrDefault().Value,
                        SyncAPIUri = e.Descendants("SyncAPIUri").FirstOrDefault().Value,
                        ApiUserID = e.Descendants("ApiUserID").FirstOrDefault().Value,
                        ApiPassword = e.Descendants("ApiPassword").FirstOrDefault().Value
                    });

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.LogWrite(ex);
                return null;
            }
        }
    }
    public class LocalSetting
    {
        public string ConnectionString { get; set; }
        public string TerminalId { get; set; }
        public string SyncAPIUri { get; set; }
        public string PaymentDeviceType { get; set; }
        public string PaymentDevicConnectionString { get; set; }
        public string ApiUserID { get; set; }
        public string ApiPassword { get; set; }
    }
}
