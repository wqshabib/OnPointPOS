using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
namespace POSSUM.Web.Controllers
{
    public class TransactionReportController : MyBaseController
    {
        // GET: Sales
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OrderList(string dtFrom, string dtTo, Guid  terminal)
        {

            DateTime startDate = Convert.ToDateTime(dtFrom).Date;
            DateTime endDate = Convert.ToDateTime(dtTo + " 23:59:59");
            List<OrderViewModel> orders = new List<OrderViewModel>();
            try
            {

                using (var db = GetConnection)
                {
                    string query = @"SELECT OrderMaster.Id, Receipt.ReceiptNumber, OrderMaster.Comments, OrderMaster.OrderNoOfDay, 
                        OrderMaster.InvoiceDate, OrderMaster.OrderTotal, OrderMaster.Status, OrderMaster.InvoiceGenerated
                        FROM OrderMaster INNER JOIN
                        Receipt ON OrderMaster.Id = Receipt.OrderId 
				        Where OrderMaster.InvoiceGenerated = 1 AND  OrderMaster.InvoiceGenerated = 1 						 
				        AND OrderMaster.TerminalId = '" + terminal + "' AND InvoiceDate between '" + startDate + "' AND '" + endDate + "'";

                    using (var conn = db.Database.Connection)
                    {
                        conn.Open();
                        IDbCommand command = new SqlCommand();
                        command.Connection = conn;

                        command.CommandText = query;

                        IDataReader dr = command.ExecuteReader();

                        while (dr.Read())
                        {
                            orders.Add(new OrderViewModel
                            {
                                Id = Guid.Parse(dr["Id"].ToString()),
                                InvoiceNumber = Convert.ToString(dr["ReceiptNumber"]),
                                OrderComments = Convert.ToString(dr["Comments"]),
                                OrderNoOfDay = Convert.ToString(dr["OrderNoOfDay"]),
                                InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                                OrderTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]), 2),
                                Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), Convert.ToString(dr["Status"])),
                                InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"])
                            });
                        }

                        dr.Dispose();
                    }

                    foreach (var item in orders)
                    {
                        if (!string.IsNullOrEmpty(item.OrderNoOfDay) && item.OrderNoOfDay.StartsWith("SA"))
                        {
                            item.InvoiceNumber = "SA-" + item.InvoiceNumber;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return PartialView(orders);
        }

        public ActionResult OrderReceipt(Guid id)
        {

            var model = new PrintViewModel();
            using (var db = GetConnection)
            {

                var orderMaster = db.OrderMaster.Include("OrderLines.Product").FirstOrDefault(o => o.Id == id);
                if (orderMaster != null)
                {
                    if (orderMaster.Outlet == null)
                        orderMaster.Outlet = db.Outlet.FirstOrDefault(o => o.Id == orderMaster.OutletId);

                    if (orderMaster.Outlet != null)
                    {
                        model.OutletName = orderMaster.Outlet.Name;
                        model.OrgNo = orderMaster.Outlet.OrgNo;
                        model.PhoneNo = orderMaster.Outlet.Phone;
                        model.Header = orderMaster.Outlet.HeaderText;
                        model.Footer = orderMaster.Outlet.FooterText;
                        model.Email = orderMaster.Outlet.Email;
                        model.Address = orderMaster.Outlet.Address1;
                        model.City = orderMaster.Outlet.City;

                        model.TaxDesc = orderMaster.Outlet.TaxDescription;
                    }

                    model.OrderComments = orderMaster.OrderComments;

                    var outletUser = db.OutletUser.FirstOrDefault(usr => usr.Id == orderMaster.CheckOutUserId); //db.Users.FirstOrDefault(usr => usr.Id == orderMaster.CheckOutUserId);
                    if (outletUser != null)
                        model.Cashier = outletUser.UserName;
                    else
                    {
                        var user = db.Users.FirstOrDefault(usr => usr.Id == orderMaster.CheckOutUserId);
                        if (user != null)
                            model.Cashier = outletUser.UserName;
                    }


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
                    if (!string.IsNullOrEmpty(orderMaster.OrderNoOfDay) && orderMaster.OrderNoOfDay.StartsWith("SA"))
                    {
                        model.ReceiptNo = "SA-" + receipt.ReceiptNumber.ToString();
                    }
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
                }
            }
            return PartialView(model);
        }


        public ActionResult _orderDetailReport(string dtFrom, string dtTo, Guid terminal)
        {
            /********
             * Gather info from view
             ********/
            DateTime startDate = Convert.ToDateTime(dtFrom).Date;
            DateTime endDate = Convert.ToDateTime(dtTo + " 23:59:59");
            var models = new List<PrintViewModel>();
            
            using (var db = GetConnection)
            {
                var orders = db.OrderMaster.Include("OrderLines.Product").Where(o => (o.InvoiceDate >= startDate && o.InvoiceDate <= endDate && o.TerminalId == terminal) && o.InvoiceGenerated == 1).OrderByDescending(o => o.InvoiceDate).ToList();
                
                foreach (var orderMaster in orders)
                {
                    var model = new PrintViewModel();
                    
                    if (orderMaster != null)
                    {
                        if (orderMaster.Outlet == null)
                            orderMaster.Outlet = db.Outlet.FirstOrDefault(o => o.Id == orderMaster.OutletId);

                        if (orderMaster.Outlet != null)
                        {
                            model.OutletName = orderMaster.Outlet.Name;
                            model.OrgNo = orderMaster.Outlet.OrgNo;
                            model.PhoneNo = orderMaster.Outlet.Phone;
                            model.Header = orderMaster.Outlet.HeaderText;
                            model.Footer = orderMaster.Outlet.FooterText;
                            model.Email = orderMaster.Outlet.Email;
                            model.Address = orderMaster.Outlet.Address1;
                            model.City = orderMaster.Outlet.City;

                            model.TaxDesc = orderMaster.Outlet.TaxDescription;
                        }

                        var outletUser = db.OutletUser.FirstOrDefault(usr => usr.Id == orderMaster.CheckOutUserId); //db.Users.FirstOrDefault(usr => usr.Id == orderMaster.CheckOutUserId);
                        if (outletUser != null)
                            model.Cashier = outletUser.UserName;
                        else
                        {
                            var user = db.Users.FirstOrDefault(usr => usr.Id == orderMaster.CheckOutUserId);
                            if (user != null)
                                model.Cashier = user.UserName;
                        }

                        // order detail

                        var orderLines = (from ordLine in orderMaster.OrderLines.Where(o => o.OrderId == orderMaster.Id && o.Active == 1 && o.IsCoupon != 1)
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
                                          })
                                          .ToList();

                        var receipt = db.Receipt.FirstOrDefault(r => r.OrderId == orderMaster.Id);
                        var lstPayments = db.Payment.Where(p => p.OrderId == orderMaster.Id).ToList();
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
                            model.ReceiptNo = receipt.ReceiptNumber.ToString();
                            model.ReceiptDate = receipt.PrintDate;
                        }

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

                        model.RoundedAmount = orderMaster.RoundedAmount;

                        model.Items = orderLines;
                        model.Payments = lstPayments;
                        model.GrandTotal = orderLines.Sum(s => s.GrossAmountDiscounted());
                        models.Add(model);
                    }
                }
            }
            return PartialView(models);
        }
    }
}