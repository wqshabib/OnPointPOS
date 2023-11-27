using FortnoxApiSDK.Models.Authorization;
using FortnoxNET.Communication;
using FortnoxNET.Services;
using Newtonsoft.Json;
using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using Newtonsoft.Json.Linq;
using POSSUM.Data.Common;

namespace POSSUM.Integration.Services
{
    public class FortNox
    {
        private string _connectionString;
        private string ClientId = ConfigurationManager.AppSettings["ClientId"];
        private string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        private string ReturnURL = ConfigurationManager.AppSettings["ReturnURL"];
        FNSettingViewModel _fnSetting;
        public bool IsWorking { get; set; }
        OAuthToken _oAuthToken;
        public FortNox(string dbName)
        {
            var server = ConfigurationManager.AppSettings["DBServer"];
            var userName = ConfigurationManager.AppSettings["DbUserName"];
            var password = ConfigurationManager.AppSettings["DbPassword"];
            _connectionString = string.Format(@"Data Source={0};Initial Catalog={1};UID={2}; Password={3};",
                server,
                dbName,
                userName,
                password);
            _fnSetting = new FNSettingViewModel();

            var settingFile = ConfigurationManager.AppSettings["FNSetting"];
            if (System.IO.File.Exists(settingFile))
            {
                var text = System.IO.File.ReadAllText(settingFile);
                if (!string.IsNullOrEmpty(text))
                {
                    _fnSetting = JsonConvert.DeserializeObject<FNSettingViewModel>(text);
                }
            }
        }

        public void WriteLastMessage(string message)
        {
            var lastMessage = ConfigurationManager.AppSettings["FNLastMessage"];
            System.IO.File.WriteAllText(lastMessage, message);
        }


        public void RefreshToken()
        {
            try
            {
                var tokenFilePath = ConfigurationManager.AppSettings["FNTokenFile"];
                var existingToken = System.IO.File.ReadAllText(tokenFilePath);
                if (string.IsNullOrEmpty(existingToken))
                {
                    WriteLastMessage(@"Please login to FortNox again to generate required token.");
                    return;
                }

                _oAuthToken = JsonConvert.DeserializeObject<OAuthToken>(existingToken);
                if (_oAuthToken != null)
                {
                    _oAuthToken = AuthorizationService.RefreshTokenAsync(ClientId, ClientSecret, _oAuthToken.RefreshToken).GetAwaiter().GetResult();
                    System.IO.File.WriteAllText(tokenFilePath, JsonConvert.SerializeObject(_oAuthToken));
                }
            }
            catch (Exception ex)
            {
                WriteLastMessage(@"Error: Please login to FortNox again to generate required token. " + ex.Message);
            }
        }

        public void Sync()
        {
            try
            {
                IsWorking = true;

                RefreshToken();

                var lst = GetPendingOrders();
                foreach (var item in lst)
                {
                    if (item != null && item.OrderLines.Count > 0)
                    {
                        var response = PostOrderToFN(item);
                        UpdateOrder(item.Id, response);
                    }
                    else
                    {
                        UpdateOrder(item.Id, "No Data");
                    }
                }
                
                WriteLastMessage(@"Sync completed, Posted " + lst.Count + " pending orders to FortNox.");
                IsWorking = false;
            }
            catch (Exception ex)
            {
                IsWorking = false;
                WriteLastMessage(@"Error: Sync failed, Please check the details. " + ex.Message);
                Log.LogWrite(ex);
            }
        }

        private void UpdateOrder(Guid id, string response)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(_connectionString))
                {
                    var order = db.OrderMaster.FirstOrDefault(a=>a.Id == id);
                    if(order != null)
                    {
                        order.FnResponse = response;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(new Exception("Update order failed for Id = " + id, ex));
            }
        }

        public string PostOrderToFN(Order order)
        {
            try
            {
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    
                    PostVoucher postVoucher = new PostVoucher();
                    postVoucher.Voucher = new Voucher()
                    {
                        Comments = order.OrderComments,
                        Description = order.Id.ToString(),
                        Project = _fnSetting.Project,// "POSSUM",
                        TransactionDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        //url = @"url",
                        //VoucherSeries = @"1000000000",
                        VoucherRows = new List<VoucherRow>()
                    };

                    foreach (var item in order.OrderLines)
                    {
                        postVoucher.Voucher.VoucherRows.Add(new VoucherRow()
                        {
                            Account = 1010, //TODO WAQAS _fnSetting.Account, // 1010,
                            Credit = item.UnitPrice * item.Quantity,
                            Debit = item.UnitPrice * item.Quantity,
                            Description = item.Product.Description,
                            Project = _fnSetting.Project, // "POSSUM",
                            Quantity = item.Quantity,
                            TransactionInformation = item.Id.ToString()
                        });
                    }

                    if (order.OrderLines.Count == 1)
                    {
                        postVoucher.Voucher.VoucherRows.Add(new VoucherRow()
                        {
                            Account = 1010, //TODO WAQAS _fnSetting.Account, // 1010,
                            Credit = 0,
                            Debit = 0,
                            Description = "No Item",
                            Project = _fnSetting.Project, // "POSSUM",
                            Quantity = 0,
                            TransactionInformation = "No Item"
                        });
                    }

                    //postVoucher.Voucher.VoucherRows.Add(new VoucherRow()
                    //{
                    //    Account = 1010,
                    //    Credit = 0,
                    //    Debit = 0,
                    //    Description = "VoucherRows Description",
                    //    Project = "POSSUM",
                    //    Quantity = 1,
                    //    TransactionInformation = "Transaction Information"
                    //});

                    //postVoucher.Voucher.VoucherRows.Add(new VoucherRow()
                    //{
                    //    Account = 1010,
                    //    Credit = 0,
                    //    Debit = 0,
                    //    Description = "VoucherRows Description",
                    //    Project = "POSSUM",
                    //    Quantity = 1,
                    //    TransactionInformation = "Transaction Information"
                    //});

                    var client = new HttpClient();
                    var message = JsonConvert.SerializeObject(postVoucher);
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.fortnox.se/3/vouchers/"))
                    {
                        requestMessage.Content = new StringContent(message, Encoding.UTF8, "application/json");
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _oAuthToken.AccessToken);

                        requestMessage.Headers.Add("Access-Token", _oAuthToken.AccessToken);
                        requestMessage.Headers.Add("Client-Secret", ClientSecret);
                        var result = client.SendAsync(requestMessage).GetAwaiter().GetResult();
                        string apiResponse = result.Content.ReadAsStringAsync().Result;
                        return apiResponse;
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLog("Sync Product Fail:> Error Code" + e.ToString());
                    return e.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("Sync Product Fail:> Error Code" + ex.ToString());
                return ex.ToString();
            }
        }

        public List<Order> GetPendingOrders()
        {
            var liveOrders = new List<Order>();
            using (ApplicationDbContext db = new ApplicationDbContext(_connectionString))
            {
                var localOrders = db.OrderMaster.Include("OrderLines.Product").Where(o => string.IsNullOrEmpty(o.FnResponse)).ToList();
                foreach (var order in localOrders)
                {
                    var _order = new Order
                    {
                        Id = order.Id,
                        TableId = order.TableId,
                        CustomerId = order.CustomerId,
                        CreationDate = order.CreationDate,
                        OrderTotal = order.OrderTotal,
                        Status = order.Status,
                        UserId = order.UserId,
                        CheckOutUserId = order.CheckOutUserId,
                        ShiftClosed = order.ShiftClosed,
                        Comments = order.Comments,
                        TaxPercent = order.TaxPercent,
                        InvoiceDate = order.InvoiceDate,
                        InvoiceGenerated = order.InvoiceGenerated,
                        InvoiceNumber = order.InvoiceNumber,
                        OrderComments = order.OrderComments,
                        OrderNoOfDay = order.OrderNoOfDay,
                        PaymentStatus = order.PaymentStatus,
                        ShiftNo = order.ShiftNo,
                        ShiftOrderNo = order.ShiftOrderNo,
                        TipAmount = order.TipAmount,
                        TipAmountType = order.TipAmountType,
                        Updated = 0,
                        ZPrinted = order.ZPrinted,
                        Bong = order.Bong,
                        DailyBong = order.DailyBong,
                        Type = order.Type,
                        OutletId = order.OutletId,
                        TerminalId = order.TerminalId,
                        TrainingMode = order.TrainingMode,
                        RoundedAmount = order.RoundedAmount,
                        CustomerInvoiceId = order.CustomerInvoiceId,
                        OrderLines = order.OrderLines.ToList().Select(ol => new OrderLine(ol)).ToList()
                    };

                    liveOrders.Add(order);
                }
            }

            return liveOrders;
        }
    }




    public class PostVoucher
    {
        public Voucher Voucher { get; set; }
    }

    public class Voucher
    {
        //public string url { get; set; }
        public string Comments { get; set; }
        public string Description { get; set; }
        public string Project { get; set; }
        public string TransactionDate { get; set; }
        public List<VoucherRow> VoucherRows { get; set; }
        public string VoucherSeries { get; set; }
    }

    public class VoucherRow
    {
        public int Account { get; set; }
        public decimal Credit { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public string Project { get; set; }
        public string TransactionInformation { get; set; }
        public decimal Quantity { get; set; }
    }

}