using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using POSSUM.Web.Pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.Text;

namespace POSSUM.Web.Controllers
{
    public class CustomerController : MyBaseController
    {

        private int TOTAL_ROWS = 0;

        public CustomerController()
        {


        }
        public ActionResult Index()
        {
            var db = GetConnection;

            var customers = db.Customer.Include("Customer_CustomField").ToList();
            return View(customers);
        }
        public ActionResult Create(Guid? id)
        {
            Customer model = null;
            var db = GetConnection;
            if (id != null)
            {
                model = db.Customer.Include("Customer_CustomField").FirstOrDefault(s => s.Id == id);
                ViewBag.HasDepositHistory = db.DepositHistory.Count(c => c.CustomerId == id) > 0;
            }

            if (model == null)
            {
                ViewBag.HasDepositHistory = false;
                model = new Customer() { Id = default(Guid), Active = true };
                var fielsList = db.CustomerCustomField.ToList();
                var list = fielsList.Select(cf => new Customer_CustomField
                {
                    FieldId = cf.Id,
                    Caption = cf.Caption
                }).ToList();
                model.Customer_CustomField = list;
            }
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Create(Customer viewModel)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(db))
                {
                    var customerRepo = uof.CustomerRepository;
                    var customerFieldRepo = uof.Customer_CustomFieldRepository;
                    var customer = new Customer();
                    bool isEdit = false;
                    if (viewModel.Id != default(Guid))
                    {
                        customer = customerRepo.Single(s => s.Id == viewModel.Id);
                        isEdit = true;
                        customer.Updated = DateTime.Now;

                    }
                    else
                    {
                        viewModel.Id = Guid.NewGuid();

                    }
                    customer.Id = viewModel.Id;
                    customer.Name = viewModel.Name;
                    customer.OrgNo = viewModel.OrgNo;
                    customer.Phone = viewModel.Phone;
                    customer.City = viewModel.City;
                    customer.Address1 = viewModel.Address1;
                    customer.Address2 = viewModel.Address2;
                    customer.FloorNo = viewModel.FloorNo;
                    customer.PortCode = viewModel.PortCode;
                    customer.Active = viewModel.Active;
                    customer.ZipCode = viewModel.ZipCode;
                    customer.Reference = viewModel.Reference;
                    customer.DirectPrint = viewModel.DirectPrint;
                    customer.CustomerNo = viewModel.CustomerNo;
                    customer.Email = viewModel.Email;
                    //customer.HasDeposit = viewModel.HasDeposit;
                    //customer.DepositAmount = viewModel.DepositAmount;
                    customer.Created = DateTime.Now;
                    customer.Updated = DateTime.Now;
                    customerRepo.AddOrUpdate(customer);
                    if (viewModel.Customer_CustomField != null && viewModel.Customer_CustomField.Count > 0)
                    {
                        var lst = viewModel.Customer_CustomField.Select(s => new { s.FieldId }).Distinct().ToList();

                        List<Customer_CustomField> uniqueList = new List<Customer_CustomField>();
                        foreach (var id in lst)
                        {
                            Guid guid = Guid.Parse(id.FieldId.ToString());
                            var firstVal = viewModel.Customer_CustomField.FirstOrDefault(c => c.FieldId == guid);
                            uniqueList.Add(firstVal);
                        }
                        foreach (var customField in uniqueList)
                        {
                            if (customField.Id == default(Guid))
                                customField.Id = Guid.NewGuid();
                            customField.CustomerId = customer.Id;
                            customField.Updated = DateTime.Now;
                            customerFieldRepo.AddOrUpdate(customField);
                        }
                    }

                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == customer.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = customer.Id.ToString(),
                                TableName = TableName.Customer
                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }

                    uof.Commit();

                    if (isEdit)
                        msg = "Success" + ":" + Resource.Customer + " " + Resource.Updated + " " + Resource.successfully;
                    else
                        msg = "Success" + ":" + Resource.Customer + " " + Resource.saved + " " + Resource.successfully;
                }
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var customerRepo = uof.CustomerRepository;
                    var customer = customerRepo.Single(o => o.Id == id);
                    customer.Active = false;
                    customer.Updated = DateTime.Now;
                    customerRepo.AddOrUpdate(customer);

                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == customer.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = customer.Id.ToString(),
                                TableName = TableName.Customer
                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }

                    uof.Commit();


                }

                msg = Resource.Success + ":" + Resource.Customer + " " + Resource.Deleted + " " + Resource.successfully;

            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Restore(Guid id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var customerRepo = uof.CustomerRepository;
                    var customer = customerRepo.Single(o => o.Id == id);
                    customer.Active = true;
                    customer.Updated = DateTime.Now;
                    customerRepo.AddOrUpdate(customer);
                    uof.Commit();
                }
                //  msg = "Success:Setting deleted successfully";

                msg = Resource.Success + ":" + Resource.Customer + " " + Resource.Restore + " " + Resource.successfully;

            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _customFields()
        {
            var db = GetConnection;
            var customFields = db.CustomerCustomField.OrderBy(o => o.SortOrder).ToList();

            return PartialView(customFields);
        }
        [HttpPost]
        public ActionResult SaveCustomField(List<CustomerCustomField> fields)
        {
            string msg = "";
            try
            {

                var db = GetConnection;
                foreach (var field in fields)
                {
                    var customeField = db.CustomerCustomField.FirstOrDefault(c => c.Id == field.Id);
                    if (customeField == null)
                    {
                        field.Id = Guid.NewGuid();
                        db.CustomerCustomField.Add(field);
                        var customers = db.Customer.ToList();
                        foreach (var customer in customers)
                        {
                            var customer_field = new Customer_CustomField
                            {
                                Id = Guid.NewGuid(),
                                Caption = field.Caption,
                                Text = "",
                                CustomerId = customer.Id,
                                FieldId = field.Id,
                                SortOrder = field.SortOrder,
                                Updated = DateTime.Now
                            };
                            db.Customer_CustomField.Add(customer_field);
                            customer.Updated = DateTime.Now;
                            db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    else
                    {
                        customeField.Caption = field.Caption;
                        customeField.SortOrder = field.SortOrder;

                        db.Entry(customeField).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                db.SaveChanges();


                msg = "Success:" + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteCustomField(Guid id)
        {
            string msg = "";
            try
            {

                var db = GetConnection;
                if (id != default(Guid))
                {
                    var customeField = db.CustomerCustomField.FirstOrDefault(c => c.Id == id);
                    if (customeField != null)
                    {
                        var customerfields = db.Customer_CustomField.Where(c => c.FieldId == id);
                        if (customerfields != null && customerfields.Count() > 0)
                        {
                            db.Customer_CustomField.RemoveRange(customerfields);
                        }
                        db.CustomerCustomField.Remove(customeField);
                    }
                    var customers = db.Customer.ToList();
                    foreach (var customer in customers)
                    {

                        customer.Updated = DateTime.Now;
                        db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                db.SaveChanges();


                msg = "Success:" + Resource.Deleted + " " + Resource.successfully;
            }
            catch (Exception ex)
            {

                msg = "Error:" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        #region Generate Invoice 
        public ActionResult Invoices()
        {
            return View();
        }

        public ActionResult EmailInvoice(string invId)
        {
            string msg = "";
            try
            {
                var ms = GenerateOcrPdf(Guid.Parse(invId));
                var invoiceFileName = DateTime.Now.ToString("yyyyddMHHmmss") + ".pdf";
                string tempInvoicePath = ConfigurationManager.AppSettings["TempInvoicePath"];
                if (string.IsNullOrEmpty(tempInvoicePath))
                    tempInvoicePath = @"E:\tmp\";
                var path = tempInvoicePath + invoiceFileName;
                FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
                ms.WriteTo(file);
                file.Close();
                ms.Close();
                CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
                var customer = handler.GetCustomerDetailFromInvoice(Guid.Parse(invId), GetConnection);
                var invoice = handler.GetCustomerInvoiceObj(Guid.Parse(invId), GetConnection);
                var outlet = handler.GetOutlet(GetConnection);
                if (customer == null)
                {
                    throw new Exception("Invalid invoice, no customer found.");
                }

                if (string.IsNullOrEmpty(customer.Email))
                {
                    throw new Exception("This customer doesn't have any email address.");
                }

                var invoiceEmailSenderName = ConfigurationManager.AppSettings["InvoiceEmailSenderName"];
                if (string.IsNullOrEmpty(invoiceEmailSenderName))
                    invoiceEmailSenderName = "POSSUM Invoice";

                if (outlet != null)
                {
                    invoiceEmailSenderName = outlet.Name = " Invoice";
                }

                var invoiceEmailSenderEmail = ConfigurationManager.AppSettings["InvoiceEmailSenderEmail"];
                if (string.IsNullOrEmpty(invoiceEmailSenderEmail))
                    invoiceEmailSenderEmail = "handlarnreso007@gmail.com";

                var invoiceEmailSenderPassword = ConfigurationManager.AppSettings["InvoiceEmailSenderPassword"];
                if (string.IsNullOrEmpty(invoiceEmailSenderPassword))
                    invoiceEmailSenderPassword = "Reso@123";

                var invoiceEmailSubject = ConfigurationManager.AppSettings["InvoiceEmailSubject"];
                if (string.IsNullOrEmpty(invoiceEmailSubject))
                    invoiceEmailSubject = "POSSUM INVOICE - ";

                var invoiceEmailBody = ConfigurationManager.AppSettings["InvoiceEmailBody"];
                if (string.IsNullOrEmpty(invoiceEmailBody))
                    invoiceEmailBody = "POSSUM invoice attached.";

                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential(invoiceEmailSenderEmail, invoiceEmailSenderPassword);
                SmtpServer.EnableSsl = true;

                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(invoiceEmailSenderEmail, invoiceEmailSenderName);
                mail.To.Add(customer.Email);

                mail.Subject = customer.Name + " : Invoice Number = " + invoice.InvoiceNumber;
                mail.Body = @"Dear Customer," + Environment.NewLine + "Please see the attachment for invoice." + Environment.NewLine + "Thanks.";

                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(path);
                mail.Attachments.Add(attachment);

                SmtpServer.Send(mail);

                msg = "Success" + ":Invoice sent successfully";
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>

        public PartialViewResult _partialInvoices(string invNo, Guid customerId, string dtFrom, string dtTo)
        {
            CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
            DateTime startDate = Convert.ToDateTime(dtFrom).Date;
            DateTime endDate = Convert.ToDateTime(dtTo);
            var model = handler.CustomerInvoiceSearch(invNo, customerId, startDate, endDate, GetConnection);
            return PartialView(model);
        }
        public ActionResult GenerateInvoice()
        {
            return View();
        }
        public PartialViewResult _partialPendingInvoices(Guid customerId, string dtFrom, string dtTo)
        {
            CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
            DateTime startDate = Convert.ToDateTime(dtFrom).Date;
            DateTime endDate = Convert.ToDateTime(dtTo);
            var model = handler.CustomerPendingOrderSearch(customerId, startDate, endDate, GetConnection);
            return PartialView(model);
        }
        public ActionResult GenerateCustomerInvoice(string ids, Guid customerId, string remanrks)
        {
            string msg = "";
            try
            {
                ids = ids.TrimEnd(',');
                List<string> orderGuid = ids.Split(',').ToList();//.Select(string).ToList();
                CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
                var id = handler.GenerateCustomerInvoice(orderGuid, customerId, remanrks, GetConnection);
                msg = "Success:Invoice generated successfully";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetInvoicedCustomers()
        {

            using (var db = GetConnection)
            {
                List<CustomerViewModel> customers = new List<CustomerViewModel>();
                customers.Add(new CustomerViewModel { Id = default(Guid), Name = Resource.Select });

                var data = (from cust in db.Customer
                            join ord in db.OrderMaster on cust.Id equals ord.CustomerId
                            where cust.HasDeposit == false
                            select new CustomerViewModel
                            {
                                Id = cust.Id,
                                Name = cust.Name
                            }).Distinct().ToList();
                if (data.Count > 0)
                    customers.AddRange(data);
                return Json(customers, JsonRequestBehavior.AllowGet);

            }


        }
        public JsonResult GetPendingInvoicedCustomers()
        {

            using (var db = GetConnection)
            {
                List<CustomerViewModel> customers = new List<CustomerViewModel>();
                customers.Add(new CustomerViewModel { Id = default(Guid), Name = Resource.Select });
                var data = (from cust in db.Customer
                            join ord in db.OrderMaster on cust.Id equals ord.CustomerId
                            where ord.CustomerInvoiceId == null
                            select new CustomerViewModel
                            {
                                Id = cust.Id,
                                Name = cust.Name
                            }).Distinct().ToList();
                if (data.Count > 0)
                    customers.AddRange(data);
                return Json(customers, JsonRequestBehavior.AllowGet);

            }


        }
        public FileResult DownloadInovice(Guid invoiceId)
        {
            try
            {

                var ms = GenerateOcrPdf(invoiceId);
                byte[] file = ms.ToArray();

                ms.Write(file, 0, file.Length);
                ms.Position = 0;
                string fileName = Resource.Invoice + ".pdf";
                HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + fileName + "");


                // Return the output stream
                return File(ms, "application/pdf");

                //return File(ms, "application/pdf", fileName + ".pdf");
                // return new FileStreamResult(ms, "application/pdf");
                //file.Close();
                //ms.Close();
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public PartialViewResult GetReadyForGenerateInvoice(string ids, Guid customerId)
        {
            ids = ids.TrimEnd(',');
            List<string> orderGuid = ids.Split(',').ToList();
            CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
            var model = handler.GetCustomerOrdersForInvoice(orderGuid, customerId, GetConnection);
            return PartialView("_readypartialInvoices", model);
        }

        public PartialViewResult GetOrderDetailForGenerateInvoice(string ids, Guid customerId)
        {
            var orderGuid = new Guid(ids);
            CustomerInvoiceHandler handler = new CustomerInvoiceHandler();
            var model = handler.GetCustomerOrderDetailsForInvoice(orderGuid, customerId, GetConnection);
            return PartialView("_orderDetail", model);
        }


        public ActionResult OrderReceipt(Guid id)
        {

            var model = new PrintViewModel();
            using (var db = GetConnection)
            {

                var orderMaster = db.OrderMaster.Include("OrderLines.Product").FirstOrDefault(o => o.Id == id);
                if (orderMaster != null)
                {
                    var customer = db.Customer.FirstOrDefault(o => o.Id == orderMaster.CustomerId);
                    if (customer != null)
                    {
                        model.Customer= customer.Name;
                    }

                    var user = db.Users.FirstOrDefault(usr => usr.Id == orderMaster.CheckOutUserId);
                    if (user != null)
                    {
                        model.Cashier = user.UserName;
                    }

                    model.OrderMaster = new OrderViewModel
                    {
                        Id = orderMaster.Id,
                        OrderNoOfDay = orderMaster.OrderNoOfDay,
                    };

                    var details = orderMaster.OrderLines.ToList();


                    var orderLines = (from ordLine in details.Where(o => o.OrderId == id && o.Active == 1 && o.IsCoupon != 1)
                                          // join prod in productRepo on ordLine.Product.Id equals prod.Id
                                      select new OrderLineViewModel
                                      {
                                          Id = ordLine.Id,
                                          OrderId = ordLine.OrderId,
                                          Product = ordLine.Product,
                                          ItemId = ordLine.Product.Id,
                                          ItemName = ordLine.Product.Description,
                                          Quantity = ordLine.Quantity,
                                          UnitPrice = ordLine.UnitPrice,
                                          UnitsInPackage = ordLine.UnitsInPackage,
                                          DiscountedUnitPrice = ordLine.DiscountedUnitPrice,
                                          DiscountPercentage = ordLine.DiscountPercentage,
                                          Direction = ordLine.Direction,
                                          Active = ordLine.Active,
                                          PurchasePrice = ordLine.PurchasePrice,
                                          IsCoupon = ordLine.IsCoupon,
                                          ItemDiscount = ordLine.ItemDiscount,
                                          ItemComments = ordLine.ItemComments,
                                          ItemStatus = ordLine.ItemStatus,
                                          TaxPercent = ordLine.TaxPercent,
                                          DiscountType = ordLine.DiscountType,
                                          DiscountDescription = ordLine.DiscountDescription,
                                          GrossTotal = ordLine.GrossAmount(),
                                          VAT = ordLine.VatAmount()
                                      }).ToList();

                    var receipt = db.Receipt.FirstOrDefault(r => r.OrderId == id);
                    var lstPayments = db.Payment.Where(p => p.OrderId == id).ToList();
                    decimal returncash = lstPayments.Where(pmt => pmt.TypeId == 1 || pmt.TypeId == 7 || pmt.TypeId == 4).Sum(tot => tot.CashChange);
                    decimal collectedcash = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.CashCollected);
                    decimal collectedcard = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.CashCollected);
                    model.CollectCard = collectedcard;
                    model.CollectedCash = collectedcash;
                    model.CashBack = returncash;

                    if (receipt != null)
                    {
                        model.ReceiptNo = receipt.ReceiptNumber.ToString();
                        model.ControlUnitCode = receipt.ControlUnitCode;
                        model.ControlUnitName = receipt.ControlUnitName;
                        receipt.ReceiptNumber.ToString();
                        if (receipt.IsSignature)
                        {

                            model.MarchantPaymentReceipt = receipt.MerchantPaymentReceipt;
                            model.CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;

                        }
                        else if (receipt.CustomerPaymentReceipt != null)
                        {

                            model.CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;

                        }
                    }
                    model.ReceiptNo = receipt.ReceiptNumber.ToString();
                    model.ReceiptDate = receipt.PrintDate;
                    // VAT calculation

                    var vatGroups = orderLines.Where(i => i.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Individuals).GroupBy(od => od.TaxPercent);
                    var vatDetails = new List<VATModel>();
                    var vatAmounts = new List<VAT>();
                    foreach (var grp in vatGroups)
                    {
                        decimal vat = grp.First().TaxPercent;
                        decimal total = grp.Sum(tot => tot.GrossAmountDiscounted());
                        decimal net = grp.Sum(tot => tot.NetAmount());
                        decimal vatAmount = grp.Sum(tot => tot.VatAmount());
                        var vatModel = new VATModel(vat, vatAmount)
                        {
                            NetAmount = net,
                            Total = total
                        };
                        vatDetails.Add(vatModel);
                        vatAmounts.Add(vatModel.GetVatAmounts());
                    }

                    var vatDetailsGroup = vatDetails.GroupBy(o => o.VATPercent).ToList();
                    var vat_Details = new List<VATModel>();
                    foreach (var vatgrp in vatDetailsGroup)
                    {
                        decimal vat = vatgrp.First().VATPercent;
                        decimal total = vatgrp.Sum(tot => tot.Total);
                        decimal net = vatgrp.Sum(tot => tot.NetAmount);
                        decimal vatAmount = vatgrp.Sum(tot => tot.VATTotal);
                        var vatModel = new VATModel(vat, vatAmount)
                        {
                            NetAmount = net,
                            Total = total
                        };

                        vat_Details.Add(vatModel);

                    }

                    model.VatDetails = vat_Details.OrderBy(o => o.VATPercent).ToList();
                    model.VATAmounts = vatAmounts;

                    model.Items = orderLines;
                    model.Payments = lstPayments;
                    model.GrandTotal = orderLines.Sum(s => s.GrossAmountDiscounted());
                    model.OrgNo = orderMaster.OrderNoOfDay;


                }
            }
            return PartialView(model);
        }




        private MemoryStream GenerateOcrPdf(Guid invoiceId)
        {
            var db = GetConnection;
            var handler = new CustomerInvoiceHandler();
            var customerInvoice = handler.GetCustomerInvoice(invoiceId, db);
            var outletId = customerInvoice.OrderDetails.First().OutletId;
            var _outlet = db.Outlet.FirstOrDefault(c => c.Id == outletId);
            var outlet = new Outlet
            {
                Id = _outlet.Id,
                Name = _outlet.Name,
                Address1 = string.IsNullOrEmpty(_outlet.Address1) ? " " : _outlet.Address1,
                Email = string.IsNullOrEmpty(_outlet.Email) ? " " : _outlet.Email,
                Phone = string.IsNullOrEmpty(_outlet.Phone) ? " " : _outlet.Phone,
                WebUrl = string.IsNullOrEmpty(_outlet.WebUrl) ? " " : _outlet.WebUrl,
                OrgNo = string.IsNullOrEmpty(_outlet.OrgNo) ? " " : _outlet.OrgNo,
                TaxDescription = string.IsNullOrEmpty(_outlet.TaxDescription) ? " " : _outlet.TaxDescription,
                HeaderText = string.IsNullOrEmpty(_outlet.HeaderText) ? " " : _outlet.HeaderText,
                FooterText = string.IsNullOrEmpty(_outlet.FooterText) ? " " : _outlet.FooterText,
                City = _outlet.City,
                PostalCode = _outlet.PostalCode
            };
            string UniqueIdentification = db.Terminal.FirstOrDefault(t => t.OutletId == outletId).UniqueIdentification;
            var settings = db.Setting.ToList();
            string paymentReceiver = settings.FirstOrDefault(t => t.Code == SettingCode.PaymentReceiver).Value;
            string bankAccount = settings.FirstOrDefault(t => t.Code == SettingCode.AccountNumber).Value;
            string fakturaRef = settings.FirstOrDefault(t => t.Code == SettingCode.FakturaReference).Value;
            var invoice = customerInvoice.Invoice;
            var orderDetails = customerInvoice.OrderDetails;
            var customer = customerInvoice.Customer;
            //Dictionary<string, string> orderLines = new Dictionary<string, string>();
            //foreach (var orderItem in orderDetails)
            //{
            //    orderLines.Add(orderItem.ItemName, Text.FormatNumber(orderItem.UnitPrice, 2));
            //}
            string filePath = System.Web.HttpContext.Current.Server.MapPath("~/Pdf");
            // string filePath = Server.MapPath(@"../../Pdf");
            decimal grossTotal =
                Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
            // decimal itemsDiscount = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ItemDiscount), 2);
            var totalVat = orderDetails.Sum(ol => ol.VatAmount());

            decimal netTotal = grossTotal - totalVat;

            decimal decTotal = grossTotal;
            decimal decVatPercent = totalVat; // orderDetails.FirstOrDefault().VAT; // Get first VAT available

            string strReminderText = string.Empty;
            //if (receipt.ReminderFee > 0 && invoice.Reminder)
            //{
            //    strReminderText = string.Concat("Vid sen betalning tar vi ut en påminnelseavgift på ", Text.FormatNumber(invoice.ReminderFee), " kr.");
            //}

            //decimal decReminderFee = 0;
            //if (invoiceType == InvoiceType.Reminder)
            //{
            //    decReminderFee = invoice.ReminderFee;
            //}
            decimal decReminderFee = 0;
            string strOcr = invoice.InvoiceNumber.ToString();
            DateTime dtExpireDate = DateTime.Now.AddDays(21);
            // invoice.LastRetryDateTime.Date.AddDays(invoice.Interval);

            return Ocr.Create(invoice.InvoiceNumber, outlet.Name, invoice.CreationDate, dtExpireDate,
                customer.Name, strOcr, invoice.InvoiceNumber, invoice.Remarks
                // orderMaster.InvoiceNumber //  string.Concat(invoice.tOrder.tCustomer.tCompany.tCorporate.FirstName, " ", invoice.tOrder.tCustomer.tCompany.tCorporate.LastName)
                , " ", orderDetails, UniqueIdentification + "-" + invoice.InvoiceNumber,
                "Customer Invoice", decTotal, decVatPercent, netTotal, strReminderText,
                string.Concat(customer.Name, " ", customer.OrgNo), customer.Address1,
                string.Concat(customer.ZipCode, " ", customer.City), dtExpireDate, decReminderFee, fakturaRef, filePath, outlet, bankAccount, paymentReceiver);

            // decTotalAmount * companyAgreement.DibsPartnerPercent / 100
        }

        #endregion


        public ActionResult AjaxHandler(int status, jQueryDataTableParamModel param)
        {
            var db = GetConnection;
            var allcustomers = db.Customer;
            IEnumerable<Customer> filteredCustomers = allcustomers;

            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<Customer, string> orderingFunction = (c => sortColumnIndex == 0 ? c.Name :
                                                                sortColumnIndex == 1 ? c.OrgNo :
                                                                sortColumnIndex == 2 ? c.Phone :
                                                                sortColumnIndex == 3 ? c.City : c.CustomerNo.ToString());

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            if (sortDirection == "asc")
                filteredCustomers = filteredCustomers.OrderBy(orderingFunction);
            else
                filteredCustomers = filteredCustomers.OrderByDescending(orderingFunction);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredCustomers = filteredCustomers.Where(c =>              
                 !string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(param.sSearch.ToLower())
                || !string.IsNullOrEmpty(c.OrgNo) && c.OrgNo.ToLower().Contains(param.sSearch.ToLower())
                || !string.IsNullOrEmpty(c.Phone) && c.Phone.ToLower().Contains(param.sSearch.ToLower())
                || !string.IsNullOrEmpty(c.CustomerNo) && c.CustomerNo.ToLower().Contains(param.sSearch.ToLower())
                );
            }

            var displayedCustomers = filteredCustomers.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedCustomers
                         select new[] { c.Name, c.OrgNo, c.Phone, c.City, c.CustomerNo, c.Active == true ? Resource.Active : Resource.Inactive, "<a class='btn btn-primary btn-gradient btn-sm fa fa-edit' style='margin - right:2px; float:right;'  data-toggle='modal' data-target='#addEditModal'   onclick='Edit(" + "\"" + c.Id + "\"" + ")'>" + "  " + Resource.Edit + "</a>", c.Active == true ? "<a class='btn btn-danger btn-gradient btn-sm fa fa-trash-o' style='margin - right:2px; float:right;'  onclick='Delete(" + "\"" + c.Id + "\"" + ")'>" + "  " + Resource.Delete + "</a>" : "<a class='btn btn-primary btn-gradient btn-sm fa fa-trash-o' style='margin - right:2px; float:right;'  onclick='Restore(" + "\"" + c.Id + "\"" + ")'>" + "  " + Resource.Restore + "</a>" };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allcustomers.Count(),
                iTotalDisplayRecords = filteredCustomers.Count(),
                aaData = result
            },
                JsonRequestBehavior.AllowGet);
        }
    }

    public class jQueryDataTableParamModel
    {
        /// <summary>
        /// Request sequence number sent by DataTable,
        /// same value must be returned in response
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// Text used for filtering
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// Number of records that should be shown in table
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// First record that should be shown(used for paging)
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Number of columns in table
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public int iSortingCols { get; set; }

        /// <summary>
        /// Comma separated list of column names
        /// </summary>
        public string sColumns { get; set; }
    }
}