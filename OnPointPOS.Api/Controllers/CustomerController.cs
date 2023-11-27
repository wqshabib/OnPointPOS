using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
//using NHibernate.Proxy;
using POSSUM.Model;
using System.Reflection;
//using NHibernate;
using System.Configuration;
//using FluentNHibernate.Cfg;
//using FluentNHibernate.Cfg.Db;
using System.Runtime;
using Microsoft.CSharp;
using System.Threading.Tasks;
//using NHibernate.Mapping;
using System.Web.Http;
//using POSSUM.Data;
//using System.Web.Http.Cors;
using Microsoft.AspNet.Identity;
//using PosserAPI.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.Data;
//using PosserAPI.Providers;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
//using NHibernate.AspNet.Identity;

namespace POSSUM.Api.Controllers
{
    // [EnableCors(origins: "*", headers: "*", methods: "*")]
    // [Authorize]
    [System.Web.Http.RoutePrefix("api/Customer")]
    public class CustomerController : BaseAPIController
    {
        string connectionString = "";
        bool nonAhenticated = false;
        public CustomerController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        /// <summary>
        /// Get customer created or updated in between the given date range
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>

        [Route("GetCustomers")]
        public async Task<List<Customer>> GetCustomers([FromUri] Dates dates)
        {
            try
            {
                //LogWriter.LogException(new Exception("GetCustomers is calling"));
                LogWriter.LogWrite("GetCustomers is calling: connectionString:" + connectionString);

                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;

                List<Customer> liveCustomers = new List<Customer>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    //api / customer / GetCustomers ? dates.LastExecutedDate = 2021 - 06 - 24 09:14:25 & dates.CurrentDate = 2021 - 06 - 24 09:15:29 & dates.TerminalId = 00000000 - 0000 - 0000 - 0000 - 000000000000!
                    //2021 - 06 - 24 09:11:46.453
                    //2021 - 06 - 24 09:14:25
                    var customers = db.Customer.Where(usr => usr.Updated > LAST_EXECUTED_DATETIME && usr.Updated <= EXECUTED_DATETIME).ToList();
                    foreach (var cust in customers)
                    {
                        var _customer = new Customer
                        {
                            Id = cust.Id,
                            Active = cust.Active,
                            Address1 = cust.Address1,
                            Address2 = cust.Address2,
                            City = cust.City,
                            Created = cust.Created,
                            CustomerNo = cust.CustomerNo,
                            DirectPrint = cust.DirectPrint,
                            FloorNo = cust.FloorNo,
                            PortCode = cust.PortCode,
                            Name = cust.Name,
                            OrgNo = cust.OrgNo,
                            Phone = cust.Phone,
                            Reference = cust.Reference,
                            Updated = cust.Updated,
                            ZipCode = cust.ZipCode,
                            ExternalId = cust.ExternalId,
                            LastBalanceUpdated = cust.LastBalanceUpdated,
                            DepositAmount = cust.DepositAmount,
                            HasDeposit = cust.HasDeposit
                        };
                        //var fields = cust.Customer_CustomField.ToList();
                        //List<Customer_CustomField> _fields = new List<Customer_CustomField>();
                        //foreach (var field in fields)
                        //{
                        //    var customerField = new Customer_CustomField
                        //    {
                        //        Id = field.Id,
                        //        Caption = field.Caption,
                        //        Text = field.Text,
                        //        CustomerId = field.CustomerId,
                        //        FieldId = field.FieldId,
                        //        Updated = field.Updated,
                        //        SortOrder = field.SortOrder
                        //    };
                        //    _fields.Add(customerField);
                        //}
                        //_customer.Customer_CustomField = _fields;
                        liveCustomers.Add(_customer);
                    }


                    return liveCustomers;
                }
            }
            catch (Exception ex)
            {
                //LogWriter.LogWrite("GetCustomers is exception: " + ex.ToString());
                LogWriter.LogException(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new List<Customer>();
            }
        }



        /// <summary>
        /// GetAllCustomers return all customers
        /// </summary>
        /// <returns></returns>
        [Route("GetAllCustomers")]
        public async Task<List<Customer>> GetAllCustomers()
        {
            try
            {
                //LogWriter.LogException(new Exception("GetCustomers is calling"));
                LogWriter.LogWrite("GetCustomers is calling: connectionString:" + connectionString);

                List<Customer> liveCustomers = new List<Customer>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {

                    var customers = db.Customer.Include("Customer_CustomField").ToList();
                    foreach (var cust in customers)
                    {
                        var _customer = new Customer
                        {
                            Id = cust.Id,
                            Active = cust.Active,
                            Address1 = cust.Address1,
                            Address2 = cust.Address2,
                            City = cust.City,
                            Created = cust.Created,
                            CustomerNo = cust.CustomerNo,
                            DirectPrint = cust.DirectPrint,
                            FloorNo = cust.FloorNo,
                            PortCode = cust.PortCode,
                            Name = cust.Name,
                            OrgNo = cust.OrgNo,
                            Phone = cust.Phone,
                            Reference = cust.Reference,
                            Updated = cust.Updated,
                            ZipCode = cust.ZipCode,
                            ExternalId = cust.ExternalId,
                            LastBalanceUpdated = cust.LastBalanceUpdated,
                            DepositAmount = cust.DepositAmount,
                            HasDeposit = cust.HasDeposit
                        };
                        var fields = cust.Customer_CustomField.ToList();
                        List<Customer_CustomField> _fields = new List<Customer_CustomField>();
                        foreach (var field in fields)
                        {
                            var customerField = new Customer_CustomField
                            {
                                Id = field.Id,
                                Caption = field.Caption,
                                Text = field.Text,
                                CustomerId = field.CustomerId,
                                FieldId = field.FieldId,
                                Updated = field.Updated,
                                SortOrder = field.SortOrder
                            };
                            _fields.Add(customerField);
                        }
                        _customer.Customer_CustomField = _fields;
                        liveCustomers.Add(_customer);
                    }


                    return liveCustomers;
                }
            }
            catch (Exception ex)
            {
                //LogWriter.LogWrite("GetCustomers is exception: " + ex.ToString());
                LogWriter.LogException(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new List<Customer>();
            }
        }





        /// <summary>
        /// Get Customer disoucnt settings
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        [Route("GetCustomerDiscount")]
        public async Task<CustomerDiscountData> GetCustomerDiscount([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;

                CustomerDiscountData customerDiscountData = new CustomerDiscountData();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {

                    var customerCards = db.CustomerCard.Where(usr => usr.Updated > LAST_EXECUTED_DATETIME && usr.Updated <= EXECUTED_DATETIME).ToList();
                    var discountGroup = db.DiscountGroup.Where(usr => usr.Updated > LAST_EXECUTED_DATETIME && usr.Updated <= EXECUTED_DATETIME).ToList();
                    var customerDiscountGroup = db.CustomerDiscountGroup.Where(usr => usr.Updated > LAST_EXECUTED_DATETIME && usr.Updated <= EXECUTED_DATETIME).ToList();

                    customerDiscountData.CustomerCards = customerCards;
                    customerDiscountData.DiscountGroups = discountGroup;
                    customerDiscountData.CustomerDiscountGroups = customerDiscountGroup;

                }
                return customerDiscountData;
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new CustomerDiscountData();
            }
        }

        /// <summary>
        /// Get Customer's Company detail
        /// </summary>
        /// <returns></returns>
        [Route("GetCompany")]
        public async Task<MasterData.Company> GetCompany()
        {
            return Company;
        }
        /// <summary>
        /// Generate databse backup
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GenerateDBBackup")]
        public HttpResponseMessage GenerateDBBackup()
        {

            try
            {
                var con = new SqlConnection(connectionString);
                string backupDIR = @"E:\bak\";// @"E:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\Backup\";
                string backupLocalDIR = System.Web.HttpContext.Current.Server.MapPath("~/SQL/DBBackup/");

                if (!System.IO.Directory.Exists(backupLocalDIR))
                {
                    System.IO.Directory.CreateDirectory(backupLocalDIR);
                }
                con.Open();
                string fileName = @"" + con.Database + ".bak";
                string path = backupDIR + fileName;
                var sqlcmd = new SqlCommand("backup database " + con.Database + " to disk='" + path + "'", con);
                sqlcmd.ExecuteNonQuery();
                con.Close();

                string inputfilepath = backupLocalDIR;
                string ftphost = "ftp://10.245.10.61/";
                string ftpfilepath = path;

                string ftpfullpath = "ftp://" + ftphost + ftpfilepath;

                using (WebClient request = new WebClient())
                {
                    request.Credentials = new NetworkCredential("db-backup-admin", "BadBoll!!!");
                    byte[] fileData = request.DownloadData(ftpfullpath);

                    using (FileStream file = File.Create(inputfilepath))
                    {
                        file.Write(fileData, 0, fileData.Length);
                        file.Close();
                    }

                }

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                string downloadPath = backupLocalDIR + con.Database + ".bak";
                var stream = new System.IO.FileStream(downloadPath, System.IO.FileMode.Open);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                return response;
                //return Ok("Success:DB Backup generated successfully");
            }
            catch (Exception ex)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;
                return response;
            }
        }
        /// <summary>
        /// Post customer created from NIMPOS systems
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostCustomer")]
        //   [ValidateAntiForgeryToken]
        public IHttpActionResult PostCustomer(Customer model)
        {
            try
            {
                //LogWriter.LogWrite("PostCustomer is calling: connectionString:" + connectionString + " and model is " + model.Id);
                #if DEBUG
                connectionString = "Data Source=DESKTOP-FFLGUA4; Initial Catalog=demoretailtestuser; Integrated Security=SSPI; persist security info=True;";
                #endif

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var _customer = db.Customer.FirstOrDefault(u => u.Id == model.Id);
                    if (_customer == null)
                    {
                        _customer = new Customer();
                        _customer.Id = model.Id;
                        _customer.Name = model.Name;
                        _customer.OrgNo = model.OrgNo;
                        _customer.City = model.City;
                        _customer.ZipCode = model.ZipCode;
                        _customer.Address1 = model.Address1;
                        _customer.Address2 = model.Address2;
                        _customer.FloorNo = model.FloorNo;
                        _customer.PortCode = model.PortCode;
                        _customer.Email = model.Email;
                        _customer.Created = model.Created;
                        _customer.Updated = model.Updated;
                        _customer.Phone = model.Phone;
                        _customer.Active = model.Active;
                        _customer.Reference = model.Reference;
                        _customer.CustomerNo = model.CustomerNo;
                        _customer.DirectPrint = model.DirectPrint;
                        _customer.ExternalId = model.ExternalId;
                        _customer.DepositAmount = model.DepositAmount;
                        _customer.HasDeposit = model.HasDeposit;
                        _customer.LastBalanceUpdated = model.LastBalanceUpdated;
                        db.Customer.Add(_customer);
                        if (model.Customer_CustomField != null && model.Customer_CustomField.Count > 0)
                        {
                            foreach (var field in model.Customer_CustomField)
                            {
                                var custom_CusomerField = new Customer_CustomField();
                                custom_CusomerField.Id = field.Id;
                                custom_CusomerField.FieldId = field.FieldId;
                                custom_CusomerField.CustomerId = model.Id;
                                custom_CusomerField.Updated = model.Created;
                                custom_CusomerField.Caption = field.Caption;
                                custom_CusomerField.Text = field.Text;
                                db.Customer_CustomField.Add(custom_CusomerField);

                            }
                        }
                        else
                        {
                            var customFields = db.CustomerCustomField.ToList();
                            foreach (var customField in customFields)
                            {
                                var custom_CusomerField = new Customer_CustomField();
                                if (custom_CusomerField.Id == default(Guid))
                                    custom_CusomerField.Id = Guid.NewGuid();
                                custom_CusomerField.FieldId = customField.Id;
                                custom_CusomerField.CustomerId = model.Id;
                                custom_CusomerField.Updated = model.Created;
                                custom_CusomerField.Caption = customField.Caption;
                                custom_CusomerField.Text = "";
                                db.Customer_CustomField.Add(custom_CusomerField);
                            }
                        }
                    }
                    else
                    {
                        _customer.Name = model.Name;
                        _customer.OrgNo = model.OrgNo;
                        _customer.City = model.City;
                        _customer.ZipCode = model.ZipCode;
                        _customer.Address1 = model.Address1;
                        _customer.Address2 = model.Address2;
                        _customer.FloorNo = model.FloorNo;
                        _customer.PortCode = model.PortCode;
                        _customer.CustomerNo = model.CustomerNo;
                        _customer.DirectPrint = model.DirectPrint;
                        _customer.Email = model.Email;
                        _customer.Created = model.Created;
                        _customer.Updated = model.Updated;
                        _customer.Phone = model.Phone;
                        _customer.Active = model.Active;
                        _customer.Reference = model.Reference;
                        _customer.ExternalId = model.ExternalId;
                        _customer.DepositAmount = model.DepositAmount;
                        _customer.HasDeposit = model.HasDeposit;
                        _customer.LastBalanceUpdated = model.LastBalanceUpdated;
                        db.Entry(_customer).State = System.Data.Entity.EntityState.Modified;
                        if (model.Customer_CustomField != null && model.Customer_CustomField.Count > 0)
                        {
                            foreach (var field in model.Customer_CustomField)
                            {
                                var customField = db.Customer_CustomField.FirstOrDefault(c => c.Id == field.Id);
                                if (customField != null)
                                {
                                    customField.Text = field.Text;
                                    db.Entry(customField).State = System.Data.Entity.EntityState.Modified;
                                }

                            }
                        }
                    }

                    db.SaveChanges();
                    return StatusCode(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                LogWriter.LogWrite("PostCustomer is calling: exception:" + ex.ToString());
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }
        private static string CalculateSHA1(string text, Encoding enc)
        {
            try
            {
                byte[] buffer = enc.GetBytes(text);
                var cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
                return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            }
            catch (Exception exp)
            {

                return string.Empty;
            }
        }


        [HttpPost]
        [Route("PostVismaCustomer")]
        //   [ValidateAntiForgeryToken]
        public IHttpActionResult PostVismaCustomer(List<Customer> customers)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    foreach (var customer in customers)
                    {
                        var _customer = db.Customer.FirstOrDefault(u => u.ExternalId == customer.ExternalId);
                        if (_customer == null)
                        {
                            _customer = new Customer();
                            _customer.Id = customer.Id;
                            _customer.Name = customer.Name;
                            _customer.OrgNo = customer.OrgNo;
                            _customer.City = customer.City;
                            _customer.ZipCode = customer.ZipCode;
                            _customer.Address1 = customer.Address1;
                            _customer.Address2 = customer.Address2;
                            _customer.FloorNo = customer.FloorNo;
                            _customer.PortCode = customer.PortCode;
                            _customer.Created = customer.Created;
                            _customer.Updated = customer.Updated;
                            _customer.Phone = customer.Phone;
                            _customer.Active = customer.Active;
                            _customer.Reference = customer.Reference;
                            _customer.CustomerNo = customer.CustomerNo;
                            _customer.DirectPrint = customer.DirectPrint;
                            _customer.ExternalId = customer.ExternalId;
                            _customer.Email = customer.Email;
                            db.Customer.Add(_customer);

                        }
                        else
                        {
                            _customer.Name = customer.Name;
                            _customer.OrgNo = customer.OrgNo;
                            _customer.City = customer.City;
                            _customer.ZipCode = customer.ZipCode;
                            _customer.Address1 = customer.Address1;
                            _customer.Address2 = customer.Address2;
                            _customer.FloorNo = customer.FloorNo;
                            _customer.PortCode = customer.PortCode;
                            _customer.CustomerNo = customer.CustomerNo;
                            _customer.DirectPrint = customer.DirectPrint;
                            _customer.Updated = customer.Updated;
                            _customer.Phone = customer.Phone;
                            _customer.Active = customer.Active;
                            _customer.ExternalId = customer.ExternalId;
                            _customer.Email = customer.Email;
                            db.Entry(_customer).State = System.Data.Entity.EntityState.Modified;

                        }

                        db.SaveChanges();
                    }
                    return StatusCode(HttpStatusCode.OK);


                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }





    }

    public class Dates
    {
        public DateTime LastExecutedDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public Guid TerminalId { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }

    public class SeachProductModel : Dates
    {
        public string Term { get; set; }
    }
}



