using Newtonsoft.Json;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Utils.Controller;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils
{
    public class SyncController
    {
        public void ValidateData()
        {
            string localConnectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;
            localConnectionString = localConnectionString.Replace("POSSUM_S", "POSSUM_Z");
            var db = new ApplicationDbContext(localConnectionString);

            var fromDate = new DateTime(2020, 04, 22);
            var toDate = new DateTime(2020, 04, 22, 23, 59, 59);

            var lstPayments = db.Payment.Where(a => a.PaymentDate >= fromDate && a.PaymentDate <= toDate).ToList();
            var lstOrders = db.OrderMaster.Where(a => a.InvoiceDate >= fromDate && a.InvoiceDate <= toDate).ToList();
            var orderIDs = lstOrders.Select(a => a.Id).ToList();
            var lstOrderDetail = db.OrderDetail.Where(a => orderIDs.Contains(a.OrderId)).ToList();

            var lst = new List<Guid>();
            foreach (var item in lstOrders)
            {
                var lstInternalOrderDetails = lstOrderDetail.Where(a => a.OrderId == item.Id && a.ItemType != ItemType.Grouped && a.Active == 1).ToList();
                if (lstInternalOrderDetails.Count > 0)
                {
                    var sum = lstInternalOrderDetails.Sum(a => (a.UnitPrice * a.Quantity) - a.ItemDiscount);
                    var sumPayment = lstPayments.Where(a => a.OrderId == item.Id).Sum(a => a.PaidAmount - a.TipAmount);
                    if (Math.Round(sum, 1) != Math.Round(sumPayment, 1))
                    {
                        lst.Add(item.Id);
                    }
                }
                else
                {
                    lst.Add(item.Id);
                }
            }

        }

        //public bool DataSync(DateTime EXECUTED_DATETIME, Guid terminalId)
        //{
        //    return DataSync(EXECUTED_DATETIME, terminalId, ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString, "", "", "");
        //}
        //public async Task<ProductData> GetProducts()
        //{
        //    try
        //    {
        //        DateTime LAST_EXECUTED_DATETIME = DateTime.Now.Date.AddYears(-1);// dates.LastExecutedDate.AddMinutes(-5);
        //        DateTime EXECUTED_DATETIME = DateTime.Now.Date;
        //        var terminalId =Guid.Parse( "613C5E1C-460F-4F6B-AD87-EA376066BDAE");

        //        using (ApplicationDbContext db = new ApplicationDbContext(ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString))
        //        {
        //            string databaseName = db.Database.Connection.Database;
        //            var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
        //            var outletId = terminal.Outlet.Id;
        //            var liveAccountings = db.Accounting.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
        //            var liveProducts = db.Product.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
        //            var liveCategories =  db.Category.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
        //            var liveItemCampaigns =  db.ProductCampaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
        //            var liveProductPrices = db.ProductPrice.ToList();
        //            var livePricePolicy =  db.Product_PricePolicy.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();

        //            var liveItemCategories = new List<ItemCategory>();
        //            var liveItemGroups = new List<ProductGroup>();

        //            if (liveProducts.Count > 0)
        //                foreach (var prod in liveProducts)
        //                {
        //                    var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
        //                    if (itmCats.Count > 0)
        //                        liveItemCategories.AddRange(itmCats);
        //                    var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
        //                    if (itemGroups.Count > 0)
        //                        liveItemGroups.AddRange(itemGroups);
        //                    //if (databaseName == "Sannegården_1_0"||databaseName=="Jos")
        //                    //{
        //                    var outletPrice = liveProductPrices.FirstOrDefault(p => p.ItemId == prod.Id && p.OutletId == outletId && p.PriceMode == PriceMode.Day);
        //                    if (outletPrice != null)
        //                        prod.Price = outletPrice.Price;
        //                    // }

        //                }
        //            ProductData productData = new ProductData();
        //            productData.Products = liveProducts;
        //            productData.Categories = liveCategories;
        //            productData.ItemCategories = liveItemCategories;
        //            productData.Accountings = liveAccountings;
        //            productData.ProductCampaigns = liveItemCampaigns;
        //            productData.ProductGroups = liveItemGroups;
        //            if (liveProductPrices.Count > 0)
        //                liveProductPrices = liveProductPrices.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
        //            productData.ProductPrices = liveProductPrices;
        //            productData.PricePolicies = livePricePolicy;
        //            return productData;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //       // Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
        //        ProductData productData = new ProductData();
        //        return productData;
        //    }

        //}

        public bool DataSync(DateTime EXECUTED_DATETIME, Guid terminalId, string connectionstring, string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = DateTime.Now.Date;
                Guid outletId = default(Guid);
                int settingId = 0;
                var db = new ApplicationDbContext(connectionstring);

                var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);

                var setting = db.Setting.ToList().FirstOrDefault(s => s.Code == SettingCode.Last_Executed && s.TerminalId == terminalId);
                if (setting != null)
                {
                    LAST_EXECUTED_DATETIME = setting.Updated != null ? Convert.ToDateTime(setting.Updated) : DateTime.Now.Date;
                    settingId = setting.Id;
                }
                if (terminal != null)
                {
                    outletId = terminal.Outlet.Id;

                }

                ServiceClient serviceClient = new ServiceClient(baseUrl, apiUser, apiPassword);


                //Get Updated Settings
                SettingData settingData = serviceClient.GetSettings(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);

                SettingController settingController = new SettingController(settingData, db);//,settingData.ZReportSettings
                bool resSetting = settingController.UpdateSettings();
                settingController.UploadSettings(LAST_EXECUTED_DATETIME, baseUrl, apiUser, apiPassword);
                //Get update user & Role
                UserData userData = serviceClient.GetUsers(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);

                UserController userController = new UserController(userData.Users, userData.Roles, userData.TillUsers, userData.UserRoles, connectionstring);
                bool resUser = userController.UpdateUser();

                //Get updated products list                
                var productData = serviceClient.GetProducts(terminalId, LAST_EXECUTED_DATETIME, EXECUTED_DATETIME);
                ProductController productController = new ProductController(productData, db);
                bool resProduct = productController.UpdateProduct();
                productController.UploadProducts(LAST_EXECUTED_DATETIME, baseUrl, apiUser, apiPassword);

                //Get updated campaign list                
                var campaignsData = serviceClient.GetCampaignsData(terminalId, LAST_EXECUTED_DATETIME, EXECUTED_DATETIME);
                var resultCampaign = productController.UpdateCampaign(campaignsData);

                //Upload Orders
                OrderController orderController = new OrderController(connectionstring);
                var customerInvoices = serviceClient.GetCustomerInvoice(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);
                orderController.UpateCustomerInvoice(customerInvoices);
                var orders = orderController.GetUpdatedOrder(terminalId, outletId);
                foreach (var order in orders)
                {
                    var _order = order;
                    var Dadd = JsonConvert.SerializeObject(_order);
                    Log.WriteLog("ServiceClient.SyncOrder..." + _order.Id);
                    bool res = serviceClient.SyncOrder(order);

                    if (res)
                        orderController.UpdateOrderStatus(order.Id);
                }

                Log.WriteLog("Syncing InvoiceCounter Started...");
                var lstInvoiceCounters = serviceClient.GetInvoiceCounters();
                orderController.UpdateInvoiceCounter(lstInvoiceCounters);
                orderController.UploadInvoiceCounter(baseUrl, apiUser, apiPassword);

                Log.WriteLog("Total order:" + orders.Count().ToString());


                orders = null;
                List<TerminalStatusLog> preparedReports = orderController.GetReportData(terminalId);
                foreach (var preparedReport in preparedReports)
                {

                    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                    bool res = client.SyncZReport(preparedReport);
                    if (res)
                        orderController.UpdateReportStatus(preparedReport.Id);
                }
                preparedReports = null;
                List<CustomerInvoice> preparedInvoices = orderController.GetCustomerInvoices();
                foreach (var invoice in preparedInvoices)
                {

                    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                    bool res = client.SyncCustomerInvoice(invoice);
                    if (res)
                        orderController.UpdateInvoiceStatus(invoice.Id);
                }
                preparedInvoices = null;
                ExceptionController exceptionController = new ExceptionController(connectionstring);
                List<PaymentDeviceLog> deviceLogs = exceptionController.GetDeviceLog();
                foreach (var deviceLog in deviceLogs)
                {

                    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                    bool res = client.SyncDeviceLog(deviceLog);
                    if (res)
                        exceptionController.UpdateDeviceLogStatus(deviceLog.Id);
                }
                var employeeLogs = exceptionController.GetEmployeeLog();
                foreach (var employeeLog in employeeLogs)
                {

                    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                    bool res = client.SyncEmployeeLog(employeeLog);
                    if (res)
                        exceptionController.UpdateEmployeeLogStatus(employeeLog.LogId);
                }

                try
                {
                    //Syncing Customers
                    Log.WriteLog("Syncing Customers Started...");
                    var customers = serviceClient.GetCustomers(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME);
                    CustomerController customerController = new CustomerController(connectionstring);
                    customerController.UploadCustomers(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, baseUrl, apiUser, apiPassword);
                    customerController.UpdateCustomers(customers);
                }
                catch (Exception ex)
                {
                    Log.LogException(ex);
                }

                try
                {
                    //Syncing Cash Drawer Logs
                    Log.WriteLog("Syncing Cash Drawer Logs Started...");
                    var lstCashDrawerLog = serviceClient.GetCashDrawerLog(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);
                    CashDrawerLogController cashDrawerLogController = new CashDrawerLogController(connectionstring);
                    cashDrawerLogController.UploadCashDrawerLog(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, baseUrl, apiUser, apiPassword);
                    cashDrawerLogController.UpdateCashDrawerLogs(lstCashDrawerLog);
                }
                catch (Exception ex)
                {
                    Log.LogException(ex);
                }

                try
                {
                    //Syncing Deposit History
                    Log.WriteLog("Syncing Deposit History Started...");
                    var lstDepositHistory = serviceClient.GetDepositHistory(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);
                    DepositHistoryController depositHistoryController = new DepositHistoryController(connectionstring);
                    depositHistoryController.UploadDepositHistory(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, baseUrl, apiUser, apiPassword);
                    depositHistoryController.UpdateDepositHistories(lstDepositHistory);
                }
                catch (Exception ex)
                {
                    Log.LogException(ex);
                }


                if (resProduct && resUser && resSetting)//
                {
                    // var helper = new NHibernateHelper(ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString).SessionFactory;

                    using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionstring)))
                    {
                        var localSettingRepo = uofLocal.SettingRepository;
                        Setting _setting = new Setting();
                        if (settingId != 0)
                        {
                            _setting = localSettingRepo.FirstOrDefault(t => t.Id == settingId);
                            _setting.Value = EXECUTED_DATETIME.ToString();
                            _setting.Updated = EXECUTED_DATETIME;
                        }
                        else
                        {
                            _setting.Code = SettingCode.Last_Executed;
                            _setting.OutletId = outletId;
                            _setting.TerminalId = terminalId;
                            _setting.Created = DateTime.Now;
                            _setting.Type = SettingType.TerminalSettings;
                            _setting.Sort = 0;
                            _setting.Description = "Last Executed";
                            _setting.Value = EXECUTED_DATETIME.ToString();
                            _setting.Updated = EXECUTED_DATETIME;
                            var maxId = localSettingRepo.GetAll().Max(m => m.Id);
                            _setting.Id = maxId + 1;
                            localSettingRepo.Add(_setting);
                        }
                        localSettingRepo.AddOrUpdate(_setting);
                        uofLocal.Commit();
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }

        }



        public void UpdateStockAndWeight(string connectionstring)
        {
            try
            {
                var db = new ApplicationDbContext(connectionstring);
                var lstOrderDetails = db.OrderDetail.Where(o => !o.IsInventoryAdjusted).ToList();
                if (lstOrderDetails.Count > 0)
                {
                    foreach (var orderDetail in lstOrderDetails)
                    {
                        try
                        {
                            var product = db.Product.FirstOrDefault(p => p.Id == orderDetail.ItemId);
                            var order = db.OrderMaster.FirstOrDefault(or => or.Id == orderDetail.OrderId &&
                            (or.Status == OrderStatus.ReturnOrder || or.Status == OrderStatus.Completed));
                            if (order != null)
                            {
                                if (product.Unit == ProductUnit.Piece)
                                    product.StockQuantity = order.Status == OrderStatus.Completed ?
                                        product.StockQuantity - orderDetail.Quantity : product.StockQuantity + orderDetail.Quantity;

                                else
                                    product.Weight = order.Status == OrderStatus.Completed ?
                                        product.Weight - orderDetail.Quantity : product.Weight + orderDetail.Quantity;

                                db.Entry(product).State = EntityState.Modified;

                                orderDetail.IsInventoryAdjusted = true;
                                db.Entry(orderDetail).State = EntityState.Modified;

                                db.SaveChanges();

                            }

                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(ex.ToString());
                        }

                    }
                }
                else
                {
                    Task.Delay(new TimeSpan(0, 5, 0)).ContinueWith(o => { UpdateStockAndWeight(connectionstring); });
                }


            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
            }
        }




        public void DownloadFile(DateTime EXECUTED_DATETIME, Guid terminalId, string connectionstring, string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = DateTime.Now.Date;

                var db = new ApplicationDbContext(connectionstring);

                var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                var setting = db.Setting.ToList().FirstOrDefault(s => s.Code == SettingCode.Last_Executed);// condition s.TerminalId == terminalId can be add  if using multuple terminal same local db
                if (setting != null)
                {
                    LAST_EXECUTED_DATETIME = setting.Updated != null ? Convert.ToDateTime(setting.Updated) : DateTime.Now.Date;
                }

                Log.WriteLog("Checking if file exist on server.");

                ServiceClient serviceClient = new ServiceClient(baseUrl, apiUser, apiPassword);
                var filetoDownload = serviceClient.GetLatestFileToDownload(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);
                if (!string.IsNullOrEmpty(filetoDownload))
                {
                    filetoDownload = "/storage/" + filetoDownload;
                    Log.WriteLog("File found with URL = " + baseUrl + filetoDownload);
                    string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(baseUrl + filetoDownload, rootpath + filetoDownload);

                    UpdateStockAccordingToFTPFile(rootpath + filetoDownload);
                }
                else
                {
                    Log.WriteLog("No file found for processing.");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
            }
        }


        public void UpdateStockAccordingToFTPFile(string fileName)
        {
            Log.WriteLog("Processing file and updating stock from it. " + fileName);
            StockRepository stockRepository = new StockRepository();
            stockRepository.UpdateStock(fileName);
        }
    }

    public class SyncPOSController
    {
        public bool DataSync(DateTime EXECUTED_DATETIME, Guid terminalId)
        {
            return DataSync(EXECUTED_DATETIME, terminalId, ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString, "", "", "");
        }

        public bool DataSync(DateTime EXECUTED_DATETIME, Guid terminalId, string connectionstring, string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = DateTime.Now.Date;
                Guid outletId = default(Guid);
                int settingId = 0;

                var db = new ApplicationDbContext(connectionstring);



                var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);

                var setting = db.Setting.ToList().FirstOrDefault(s => s.Code == SettingCode.Last_Executed);// condition s.TerminalId == terminalId can be add  if using multuple terminal same local db
                if (setting != null)
                {
                    LAST_EXECUTED_DATETIME = setting.Updated != null ? Convert.ToDateTime(setting.Updated) : DateTime.Now.Date;
                    settingId = setting.Id;
                }
                if (terminal != null)
                {
                    outletId = terminal.Outlet.Id;

                }

                ServiceClient serviceClient = new ServiceClient(baseUrl, apiUser, apiPassword);


                ////Get Updated Settings
                ////SettingData settingData = serviceClient.GetSettings(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);

                ////SettingController settingController = new SettingController(settingData);//,settingData.ZReportSettings
                //bool resSetting = true;// settingController.UpdateSettings();

                ////Get update user & Role
                //UserData userData = serviceClient.GetUsers(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);

                //UserController userController = new UserController(userData.Users, userData.Roles, userData.TillUsers, userData.UserRoles, connectionstring);
                //bool resUser = userController.UpdateUser();

                //Get updated products list                
                var productData = serviceClient.GetProducts(terminalId, LAST_EXECUTED_DATETIME, EXECUTED_DATETIME);

                ProductController productController = new ProductController(productData, db);
                bool resProduct = productController.UpdateProduct();


                //Get updated customer list                
                var customers = serviceClient.GetCustomers(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME);
                CustomerController customerController = new CustomerController(connectionstring);
                customerController.UploadCustomers(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, baseUrl, apiUser, apiPassword);

                customerController.UpdateCustomers(customers);


                //Upload Orders
                //OrderController orderController = new OrderController(connectionstring);
                //var customerInvoices = serviceClient.GetCustomerInvoice(LAST_EXECUTED_DATETIME, EXECUTED_DATETIME, terminalId);
                //orderController.UpateCustomerInvoice(customerInvoices);
                //var orders = orderController.GetUpdatedOrder(terminalId, outletId);
                //foreach (var order in orders)
                //{
                //    var _order = order;

                //    bool res = serviceClient.SyncOrder(order);
                //    if (res)
                //        orderController.UpdateOrderStatus(order.Id);
                //}
                //orders = null;
                //List<TerminalStatusLog> preparedReports = orderController.GetReportData(terminalId);
                //foreach (var preparedReport in preparedReports)
                //{

                //    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                //    bool res = client.SyncZReport(preparedReport);
                //    if (res)
                //        orderController.UpdateReportStatus(preparedReport.Id);
                //}
                //preparedReports = null;
                //List<CustomerInvoice> preparedInvoices = orderController.GetCustomerInvoices();
                //foreach (var invoice in preparedInvoices)
                //{

                //    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                //    bool res = client.SyncCustomerInvoice(invoice);
                //    if (res)
                //        orderController.UpdateInvoiceStatus(invoice.Id);
                //}
                //preparedInvoices = null;
                //ExceptionController exceptionController = new ExceptionController(connectionstring);
                //List<PaymentDeviceLog> deviceLogs = exceptionController.GetDeviceLog();
                //foreach (var deviceLog in deviceLogs)
                //{

                //    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                //    bool res = client.SyncDeviceLog(deviceLog);
                //    if (res)
                //        exceptionController.UpdateDeviceLogStatus(deviceLog.Id);
                //}
                //var employeeLogs = exceptionController.GetEmployeeLog();
                //foreach (var employeeLog in employeeLogs)
                //{

                //    ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                //    bool res = client.SyncEmployeeLog(employeeLog);
                //    if (res)
                //        exceptionController.UpdateEmployeeLogStatus(employeeLog.LogId);
                //}

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }

        }

        public bool SyncOnlineOrdersSync(string orderId, int status, string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                ServiceClient serviceClient = new ServiceClient(baseUrl, apiUser, apiPassword);
                return serviceClient.UpdateOnlineOrderStatus(orderId, status);
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }

        }
    }
}
