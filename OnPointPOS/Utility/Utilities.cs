using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Data;
using System.Collections.Generic;
using POSSUM.Presenters.PromptInfo;
using System.Configuration;
using System.Threading;

namespace POSSUM
{
    public class Utilities
    {
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
                LogWriter.LogWrite(exp);
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
                LogWriter.LogWrite(exp);
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
                LogWriter.LogWrite(exp);
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

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.OperationalStatus == OperationalStatus.Up && nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) && tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }
            return macAddress;
        }

        public static decimal AmountParse(string text)
        {
            decimal result;
            if (decimal.TryParse(text, NumberStyles.Currency, Defaults.UICultureInfo, out result))
                return result;
            throw new ArgumentException("Felaktigt nummerformat");
        }

        public static string PromptInput(string title, string description, string defaultValue, bool multiline = true)
        {
            var config = new PromptInfoPresenter.PromptInfoConfig
            {
                Title = title,
                Description = description,
                PromptType =
                    multiline
                        ? PromptInfoPresenter.PromptType.TextMultiLine
                        : PromptInfoPresenter.PromptType.TextSingleLine,
                Value = defaultValue
            };
            var w = new PromptInfoWindow(config);
            if (w.ShowDialog() ?? false)
                return config.Value;
            return null;
        }
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

        public static PaymentTransactionStatus ProcessPaymentPurchase(decimal totalAmount, decimal vatAmount,
            decimal cashbackAmount, int tableId, Guid orderId)
        {
            if (Defaults.PaymentDeviceType == PaymentDeviceType.CONNECT2T && Defaults.DeviceLog)
                LogWriter.DeviceLog(orderId, totalAmount, vatAmount, cashbackAmount);

            LogWriter.LogWrite("Payment process started."+DateTime.Now);

            var status = new PaymentTransactionStatus { TransactionType = PaymentTransactionType.PURCHASE };
            var pw = new PaymentWindow(totalAmount, vatAmount, cashbackAmount, status, tableId, orderId);

            if (PosState.GetInstance().PaymentDevice == null) // paymentdevice is set to NONE
            {
                status.Result = PaymentTransactionStatus.PaymentResult.NO_DEVICE_CONFIGURED;
                return status;
            }
            // If dialog was cancelled or closed in some way not normal, set as if device cancelled
            if (!(pw.ShowDialog() ?? false))
            {
                status.Result = PaymentTransactionStatus.PaymentResult.CANCELLED;
            }

            LogWriter.LogWrite("Payment process completed." + DateTime.Now);

            return status;
        }

   
        internal static PaymentTransactionStatus ProcessPaymentRefund(decimal totalAmount, decimal vatAmount,
            decimal cashbackAmount, int tableId, Guid orderId)
        {
            if (Defaults.PaymentDeviceType == PaymentDeviceType.CONNECT2T && Defaults.DeviceLog)
                LogWriter.DeviceLog(orderId, totalAmount, vatAmount, cashbackAmount);
            var status = new PaymentTransactionStatus { TransactionType = PaymentTransactionType.REFUND };
            var pw = new PaymentWindow(totalAmount, vatAmount, cashbackAmount, status, tableId, orderId);

            if (PosState.GetInstance().PaymentDevice == null) // paymentdevice is set to NONE
            {
                status.Result = PaymentTransactionStatus.PaymentResult.NO_DEVICE_CONFIGURED;
                return status;
            }

            // If dialog was cancelled or closed in some way not normal, set as if device cancelled
            if (!(pw.ShowDialog() ?? false))
            {
                //LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.PaymentTerminalWindowCancel), orderId);
                //pw.presenter.HandleCancelClick();
                //int waitSeconds = 5;
                //var paymentWindowCloseWaitSeconds = ConfigurationManager.AppSettings["PaymentWindowCloseWaitSeconds"];
                //if (!string.IsNullOrEmpty(paymentWindowCloseWaitSeconds))
                //{
                //    waitSeconds = Convert.ToInt32(waitSeconds);
                //}

                //for (int i = 0; i < waitSeconds; i++)
                //{
                //    Thread.Sleep(1000);
                //}

                //pw.presenter.SetPaymentDialogCloseForced(true);
                status.Result = PaymentTransactionStatus.PaymentResult.CANCELLED;
            }
            return status;
        }

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

        public static SolidColorBrush GetTransparentColorBrush()
        {
            return new SolidColorBrush(Colors.Transparent);
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
                case OrderStatus.ReturnOrder:
                    return new SolidColorBrush(Colors.IndianRed);
                case OrderStatus.Completed:
                    return new SolidColorBrush(Colors.White);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

        public LocalSetting ReadLocalSettings(string userName)
        {
            try
            {
                var main = XElement.Load(@"settings.xml");

                var results =
                    main.Descendants("Terminal")
                        .Where(e => e.Attribute("UserName").Value == userName)
                        .Select(
                            e =>
                                new LocalSetting
                                {
                                    ConnectionString = e.Descendants("ConnectionString").FirstOrDefault().Value,
                                    TerminalId = e.Descendants("TerminalId").FirstOrDefault().Value,
                                    PaymentDevicConnectionString =
                                        e.Descendants("PaymentDevicConnectionString").FirstOrDefault().Value,
                                    PaymentDeviceType = e.Descendants("PaymentDeviceType").FirstOrDefault().Value,
                                    SyncAPIUri = e.Descendants("SyncAPIUri").FirstOrDefault().Value
                                });

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return null;
            }
        }


        internal static string GetPrinterByOutLetId(Guid outletId, string location)
        {
            try
            {


                var outLet = Defaults.Outlet;
                if (outLet != null)
                {
                    int outletPrinterId = outLet.BillPrinterId;
                    if (location == "Kitchen")
                        outletPrinterId = outLet.KitchenPrinterId;
                    var printer = (from p in Defaults.Printers.Where(p => p.Id == outletPrinterId)

                                   select new
                                   {
                                       p.PrinterName
                                   }).FirstOrDefault();
                    if (printer != null)
                        return printer.PrinterName;
                    return "Microsoft XPS Document Writer";
                }
                else
                {
                    var printer = (from p in Defaults.Printers
                                   select new
                                   {
                                       p.PrinterName
                                   }).FirstOrDefault();
                    if (printer != null)
                        return printer.PrinterName;
                    return "Microsoft XPS Document Writer";
                }

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return "Microsoft XPS Document Writer";
            }
        }
        internal static string GetPrinterById(int printerId)
        {
            try
            {
                var printer = (from p in Defaults.Printers.Where(p => p.Id == printerId)
                               select new
                               {
                                   p.PrinterName
                               }).FirstOrDefault();
                if (printer != null)
                    return printer.PrinterName;
                else
                    return "POS-X Thermal Printer";
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return "Microsoft XPS Document Writer";
            }
        }

        internal static string GetPrinterByTerminalId(Guid terminalId)
        {
            try
            {
                var printer = (from p in Defaults.Printers.Where(p => p.TerminalId == terminalId)
                               select new
                               {
                                   p.PrinterName
                               }).FirstOrDefault();
                //LogWriter.LogWrite("This is printer name : " + printer.PrinterName);
                if (printer != null)
                    return printer.PrinterName;
                else
                    return "THERMAL Receipt Printer";
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return "Microsoft XPS Document Writer";
            }
        }
        internal static string GetPrinterByProduct(int printerId)
        {
            try
            {
                var printer = (from p in Defaults.Printers.Where(p => p.Id == printerId)
                               select new
                               {
                                   p.PrinterName
                               }).FirstOrDefault();
                if (printer != null)
                    return printer.PrinterName;
                else
                    return null;




            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return null;
            }
        }

        internal static string GetPrinterByLocation(string location)
        {
            try
            {

                var printer = (from p in Defaults.Printers.Where(p => p.LocationName.ToLower() == location)

                               select new
                               {
                                   p.PrinterName
                               }).FirstOrDefault();
                if (printer != null)
                    return printer.PrinterName;
                return "Microsoft XPS Document Writer";


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return "Microsoft XPS Document Writer";
            }
        }

        public static void AddMqttBuffer(Guid messageId, string action, string jsonData, Guid orderId = default(Guid))
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var clients = db.MQTTClient.ToList();
                    List<MQTTBuffer> buffers = new List<MQTTBuffer>();
                    foreach (var client in clients)
                    {
                        var buffer = new MQTTBuffer
                        {
                            Id = Guid.NewGuid(),
                            MessageId = messageId,
                            ClientId = client.Id,
                            Action = action,
                            JsonData = jsonData,
                            OrderId = orderId,
                            Created = DateTime.Now,
                            Status = false

                        };
                        buffers.Add(buffer);
                    }
                    if (buffers.Count > 0)
                    {
                        db.MQTTBuffer.AddRange(buffers);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public static void UpdateMqttBuffer(Guid messageId, Guid clientId, Guid orderId = default(Guid))
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var buffer = db.MQTTBuffer.FirstOrDefault(b => b.ClientId == clientId && b.MessageId == messageId);
                    if (buffer!=null)
                    {
                        db.MQTTBuffer.Remove(buffer);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public static MQTTBuffer GetPendingMqttBuffer(Guid messageId, Guid clientId, Guid orderId = default(Guid))
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                  return db.MQTTBuffer.FirstOrDefault(b => b.ClientId == clientId && b.MessageId == messageId);
                    
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                return null;
            }
        }

        public bool SaveMqttOrder(Order order)
        {

            try
            {

                using (var uof = new UnitOfWork(new ApplicationDbContext()))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    var orderRepo = uof.OrderRepository;
                    var orderLineRepo = uof.OrderLineRepository;
                    var paymentRepo = uof.PaymentRepository;
                    var receiptRepo = uof.ReceiptRepository;
                    var prodRepo = uof.ProductRepository;
                    var catprodRepo = uof.ItemCategoryRepository;
                    var transactionRepo = uof.ItemTransactionRepository;
                    var outletRepo = uof.OutletRepository;
                    var lines = order.OrderLines;
                    order.OrderLines = null;
                    if (order.Outlet != null)
                    {
                        order.OutletId = order.Outlet.Id;
                        order.Outlet = null;
                    }
                    Guid warehouseId = default(Guid);
                    var outlet = outletRepo.FirstOrDefault(o => o.Id == order.OutletId);
                    if (outlet != null)
                        warehouseId = outlet.WarehouseID;
                    orderRepo.AddOrUpdate(order);
                    if (lines != null && lines.Count > 0)
                        foreach (var orderLine in lines)
                        {
                            orderLine.Product = null;
                            orderLineRepo.AddOrUpdate(orderLine);
                        }
                    if (order.Payments != null && order.Payments.Count > 0)
                        foreach (var payment in order.Payments)
                        {
                            paymentRepo.AddOrUpdate(payment);

                        }
                    if (order.Receipts != null && order.Receipts.Count > 0)
                        foreach (var recipt in order.Receipts)
                        {
                            receiptRepo.AddOrUpdate(recipt);

                        }
                    uof.Commit();

                }


                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                return false;

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
    }
}