using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class CustomerInvoiceHandler
    {

        public CustomerInvoiceModel GetCustomerInvoice(Guid InvoiceId, ApplicationDbContext db)
        {
            try
            {

                CustomerInvoiceModel model = new CustomerInvoiceModel();


                var invoice = db.CustomerInvoice.FirstOrDefault(inv => inv.Id == InvoiceId);
                if (invoice != null)
                {
                    model.Invoice = invoice;
                    var customer = db.Customer.FirstOrDefault(cust => cust.Id == invoice.CustomerId);
                    if (customer != null)
                        model.Customer = customer;
                    //Detail


                    var orderLines = (from ordLine in db.OrderDetail.Where(o => o.Active == 1 && o.IsCoupon != 1)
                                      join ord in db.OrderMaster on ordLine.OrderId equals ord.Id
                                      where ord.CustomerInvoiceId == InvoiceId
                                      select new OrderLineViewModel
                                      {
                                          Id = ordLine.Id,
                                          OrderId = ordLine.OrderId,
                                          OutletId = ord.OutletId,
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
                                          DiscountDescription = ordLine.DiscountDescription


                                      }).ToList();



                    var groups = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice }).ToList();
                    List<OrderLineViewModel> lines = new List<OrderLineViewModel>();
                    foreach (var group in groups)
                    {
                        var ordLine = group.First();
                        var grossTotal = group.Sum(s => s.GrossAmountDiscounted());
                        var netTotal = group.Sum(s => s.NetAmount());
                        var qty = group.Sum(s => (s.Direction * s.Quantity));
                        lines.Add(new OrderLineViewModel
                        {
                            Id = ordLine.Id,
                            OutletId = ordLine.OutletId,
                            Product = ordLine.Product,
                            ItemName = ordLine.ItemName,
                            Quantity = qty,
                            UnitPrice = ordLine.UnitPrice,
                            UnitsInPackage = ordLine.UnitsInPackage,
                            DiscountedUnitPrice = ordLine.DiscountedUnitPrice,
                            DiscountPercentage = ordLine.DiscountPercentage,
                            Direction = 1,
                            Active = ordLine.Active,
                            PurchasePrice = ordLine.PurchasePrice,
                            IsCoupon = ordLine.IsCoupon,
                            ItemDiscount = ordLine.ItemDiscount,
                            ItemComments = ordLine.ItemComments,
                            ItemStatus = ordLine.ItemStatus,
                            TaxPercent = ordLine.TaxPercent,
                            GrossTotal = grossTotal,// ordLine.GrossAmount(),

                        });

                    }




                    model.OrderDetails = lines.OrderBy(o => o.ItemName).ToList();

                }


                return model;
            }
            catch (Exception ex)
            {

                return new CustomerInvoiceModel();
            }
        }


        public List<OrderViewModel> CustomerPendingOrderSearch(Guid customerId, DateTime startDate, DateTime endDate, ApplicationDbContext GetConnection)
        {
            List<OrderViewModel> orders = new List<OrderViewModel>();

            /********
             * Gather info from view
             ********/


            try
            {
                string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
                string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";

                using (var db = GetConnection) 
                {
                    string queryOld = @"SELECT        OrderMaster.Id,OrderMaster.OrderNoOfDay, OrderMaster.InvoiceNumber, OrderMaster.CustomerId, (Customer.Name+' '+isnull(Customer.OrgNo,' ')+' '+isnull(Customer.Address1,' ')) as CustomerName, OrderMaster.InvoiceDate, OrderMaster.OrderTotal, OrderMaster.InvoiceGenerated
FROM            OrderMaster INNER JOIN
                         Receipt ON OrderMaster.Id = Receipt.OrderId
                         INNER JOIN  Customer ON OrderMaster.CustomerId = Customer.Id 
                         INNER JOIN  Payment ON Payment.OrderId =  OrderMaster.Id  
 Where OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status=13 AND  Payment.PaymentType=2 AND  OrderMaster.CustomerId='" + customerId + "' AND OrderMaster.CustomerInvoiceId is null AND InvoiceDate between '" + dt1 + "' AND '" + dt2 + "' Order By OrderMaster.CreationDate";

                    string query = @"SELECT OrderMaster.Id,OrderMaster.OrderNoOfDay, OrderMaster.InvoiceNumber, OrderMaster.CustomerId, (Customer.Name+' '+isnull(Customer.OrgNo,' ')+' '+isnull(Customer.Address1,' ')) as CustomerName, OrderMaster.InvoiceDate, OrderMaster.OrderTotal, OrderMaster.InvoiceGenerated
FROM            OrderMaster INNER JOIN
                         Receipt ON OrderMaster.Id = Receipt.OrderId
                         INNER JOIN  Customer ON OrderMaster.CustomerId = Customer.Id 
                         INNER JOIN  Payment ON Payment.OrderId =  OrderMaster.Id  
 Where OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status=13 AND  Payment.PaymentType=2 AND OrderMaster.CustomerInvoiceId is null AND InvoiceDate between '" + dt1 + "' AND '" + dt2 + "' ";

                    if (customerId != default(Guid))
                        query = query + " AND OrderMaster.CustomerId = '" + customerId + "'";
                    query = query + " Order By OrderMaster.CreationDate";

                    orders = db.Database.SqlQuery<OrderViewModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {

            }

            return orders;
        }
        public List<OrderViewModel> CustomerIsInIncoiceOrderSearch(Guid customerId, DateTime startDate, DateTime endDate, ApplicationDbContext GetConnection)
        {
            List<OrderViewModel> orders = new List<OrderViewModel>();

            /********
             * Gather info from view
             ********/

            try
            {
                string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
                string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";

                using (var db = GetConnection)
                {
                    string query = @"SELECT        OrderMaster.Id,OrderMaster.OrderNoOfDay, OrderMaster.InvoiceNumber, OrderMaster.CustomerId, (Customer.Name+' '+isnull(Customer.OrgNo,' ')+' '+isnull(Customer.Address1,' ')) as CustomerName, OrderMaster.InvoiceDate, OrderMaster.OrderTotal as SumAmt, OrderMaster.InvoiceGenerated
FROM            OrderMaster INNER JOIN
                         Receipt ON OrderMaster.Id = Receipt.OrderId
                         INNER JOIN  Customer ON OrderMaster.CustomerId = Customer.Id 
                         INNER JOIN  Payment ON Payment.OrderId =  OrderMaster.Id  
 Where OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status=13 AND  Payment.PaymentType=2 AND  OrderMaster.IsInInvoice=1  AND OrderMaster.CustomerInvoiceId is null AND InvoiceDate between '" + dt1 + "' AND '" + dt2 + "' Order By OrderMaster.CreationDate";
                    orders = db.Database.SqlQuery<OrderViewModel>(query).ToList();


                }
            }
            catch (Exception ex)
            {

            }
            return orders;
        }

        public List<OrderViewModel> CustomerInvoiceSearch(string queryText, Guid customerId, DateTime startDate, DateTime endDate, ApplicationDbContext db)
        {

            string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
            string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";


            string query = @"SELECT CustomerInvoice.Id,  Customer.Id as CustomerId,  (Customer.Name+' '+isnull(Customer.OrgNo,' ')+' '+isnull(Customer.Address1,' ')) as CustomerName, Cast( CustomerInvoice.InvoiceNumber as varchar(20)) as InvoiceNumber, CustomerInvoice.Remarks, CustomerInvoice.CreationDate,CustomerInvoice.InvoiceTotal as OrderTotal
                    FROM   Customer INNER JOIN
                                             CustomerInvoice ON Customer.Id = CustomerInvoice.CustomerId 
                     Where CustomerInvoice.CreationDate between '" + dt1 + "' AND '" + dt2 + "' ";
            if (!string.IsNullOrEmpty(queryText))
            {
                query = query + " AND CustomerInvoice.InvoiceNumber LIKE '%" + queryText + "%'";
            }
            if (customerId != default(Guid))
                query = query + " AND CustomerInvoice.CustomerId = '" + customerId + "'";
            List<OrderViewModel> orders = db.Database.SqlQuery<OrderViewModel>(query).ToList();

            return orders;

        }
        internal Guid GenerateCustomerInvoice(List<string> orderGuid, Guid customerId, string remanrks, ApplicationDbContext db)
        {
            using (var uof = new UnitOfWork(db))
            {
                var invoiceRepo = uof.CustomerInvoiceRepository;
                var orderRepo = uof.OrderRepository;
                var orders = orderRepo.Where(o => orderGuid.Contains(o.Id.ToString())).ToList();
                var groupByCustomer = orders.GroupBy(o => o.CustomerId).ToList();

                foreach (var group in groupByCustomer)
                {
                    var customrId = group.Key;
                    var ordersFiltered = orders.Where(a => a.CustomerId == customrId).ToList();

                    var invoiceCounterRepo = uof.InvoiceCounterRepository;
                    decimal invoiceTotal = ordersFiltered.Sum(o => o.OrderTotal);
                    long invoiceNo = 0;
                    var invoiceCounter = invoiceCounterRepo.FirstOrDefault(inc => inc.Id == 2);

                    if (invoiceCounter != null)
                    {
                        invoiceNo = Convert.ToInt64(invoiceCounter.LastNo);
                        invoiceCounter.LastNo = (invoiceNo + 1).ToString();

                    }
                    else
                    {
                        invoiceCounter = new InvoiceCounter
                        {
                            Id = 2,
                            LastNo = "1",
                            InvoiceType = InvoiceType.Customer
                        };

                    }
                    invoiceCounterRepo.AddOrUpdate(invoiceCounter);
                    invoiceNo = invoiceNo + 1;
                    Guid invoiceId = Guid.NewGuid();
                    var invoice = new CustomerInvoice
                    {
                        Id = invoiceId,
                        InvoiceNumber = invoiceNo,
                        CreationDate = DateTime.Now,
                        InvoiceTotal = invoiceTotal,
                        Remarks = remanrks,
                        CustomerId = customrId
                    };
                    invoiceRepo.Add(invoice);
                    foreach (var order in ordersFiltered)
                    {
                        // var _order = orderRepo.FirstOrDefault(o => o.Id == order.Id);
                        order.CustomerInvoiceId = invoiceId;
                        orderRepo.AddOrUpdate(order);
                    }
                    uof.Commit();
                }

                return Guid.Empty;
            }
        }

        internal void GenerateCustomerIsInInvoice(List<string> orderGuid, Guid customerId, string remanrks, ApplicationDbContext db)
        {
            using (var uof = new UnitOfWork(db))
            {
                var invoiceRepo = uof.CustomerInvoiceRepository;
                var orderRepo = uof.OrderRepository;
                var orders = orderRepo.Where(o => orderGuid.Contains(o.Id.ToString())).ToList();

                foreach (var order in orders)
                {
                    order.IsInInvoice = false;
                    orderRepo.AddOrUpdate(order);
                }
                uof.Commit();
            }
        }


        internal Customer GetCustomerDetailFromInvoice(Guid invoiceId, ApplicationDbContext db)
        {
            var invoice = db.CustomerInvoice.FirstOrDefault(inv => inv.Id == invoiceId);
            var customer = db.Customer.FirstOrDefault(cust => cust.Id == invoice.CustomerId);
            return customer;
        }


        public CustomerInvoice GetCustomerInvoiceObj(Guid invoiceId, ApplicationDbContext db)
        {
            var invoice = db.CustomerInvoice.FirstOrDefault(inv => inv.Id == invoiceId);
            return invoice;
        }

        public Outlet GetOutlet(ApplicationDbContext db)
        {
            var outlet = db.Outlet.FirstOrDefault();
            return outlet;
        }



        public ReadyInvoiceModel GetCustomerOrdersForInvoice(List<string> orderGuid, Guid customerId, ApplicationDbContext db)
        {

            using (var uof = new UnitOfWork(db))
            {
                var customerRepo = uof.CustomerRepository;
                var orderRepo = uof.OrderRepository;
                var lstOrders = orderRepo.Where(o => orderGuid.Contains(o.Id.ToString())).ToList();

                var tmpList = (from data in lstOrders
                               group data by data.CustomerId into groupedData
                               select new
                               {
                                   Key = groupedData.Key,
                                   Value = groupedData.ToList()
                               }).ToList();

                if (tmpList.Count > 1)
                {
                    //We have orders from multiple customers
                    return new ReadyInvoiceModel() {
                        IsMultiCustomerOrders = true
                    };
                }

                ReadyInvoiceModel model = new ReadyInvoiceModel();
                var customer = customerRepo.GetById(customerId);
                if (customer == null && lstOrders.Count > 0)
                {
                    customer = customerRepo.GetById(lstOrders[0].CustomerId);
                }

                foreach (var order in lstOrders)
                {
                    var orderModel = new OrderViewModel
                    {
                        Id = order.Id,
                        OrderTotal = order.OrderTotal,
                        CustomerName = customer.Name,
                        OrderNoOfDay = order.OrderNoOfDay,
                        Remarks = order.OrderComments,
                        CreationDate = order.CreationDate,
                    };
                    model.OrderViewModel.Add(orderModel);
                }
                model.InvoiceTotal = lstOrders.Sum(o => o.OrderTotal);
                model.Timestamp = DateTime.Now;
                model.CustomerName = customer.Name;
                return model;
            }


        }




        public OrderViewModel GetCustomerOrderDetailsForInvoice(Guid orderGuid, Guid customerId, ApplicationDbContext db)
        {

            using (var uof = new UnitOfWork(db))
            {
                var customerRepo = uof.CustomerRepository;
                var orderRepo = uof.OrderRepository;
                var lstOrders = orderRepo.Where(o => o.Id == orderGuid).ToList();

                OrderViewModel model = new OrderViewModel();
                var customer = customerRepo.GetById(customerId);
                foreach (var order in lstOrders)
                {
                    var orderModel = new OrderViewModel
                    {
                        Id = order.Id,
                        OrderTotal = order.OrderTotal,
                        CustomerName = customer.Name,
                        OrderNoOfDay = order.OrderNoOfDay,
                        Remarks = order.OrderComments,
                        OrderLines = order.OrderLines,
                    };
                    model = orderModel;
                }
                return model;
            }


        }




    }
}