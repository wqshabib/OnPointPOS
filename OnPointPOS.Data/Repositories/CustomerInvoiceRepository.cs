using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class CustomerInvoiceRepository
    {
        public Guid GenerateCustomerInvoiceSingleOrder(Guid OrderId, Guid customerId)
        {
            using (var db = new ApplicationDbContext())
            {

                var order = db.OrderMaster.FirstOrDefault(o => o.Id == OrderId);
                long invoiceNo = 0;

                var invoiceCounter = db.InvoiceCounter.FirstOrDefault(inc => inc.Id == 2);

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
                    db.InvoiceCounter.Add(invoiceCounter);
                }
                invoiceNo = invoiceNo + 1;
                Guid invoiceId = Guid.NewGuid();
                var invoice = new CustomerInvoice
                {
                    Id = invoiceId,
                    InvoiceNumber = invoiceNo,
                    CreationDate = DateTime.Now,
                    InvoiceTotal = order.OrderTotal,
                    Remarks = order.OrderComments,
                    CustomerId = customerId
                };
                db.CustomerInvoice.Add(invoice);

                order.CustomerInvoiceId = invoiceId;
                db.SaveChanges();
                return invoiceId;
            }
        }

        public Guid GenerateCustomerInvoice(List<Order> orders, Guid customerId, string remanrks)
        {
            using (var db = new ApplicationDbContext())
            {

                decimal invoiceTotal = orders.Sum(o => o.OrderTotal);
                long invoiceNo = 0;
                var invoiceCounter = db.InvoiceCounter.FirstOrDefault(inc => inc.Id == 2);

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
                    db.InvoiceCounter.Add(invoiceCounter);
                }
                invoiceNo = invoiceNo + 1;
                Guid invoiceId = Guid.NewGuid();
                var invoice = new CustomerInvoice
                {
                    Id = invoiceId,
                    InvoiceNumber = invoiceNo,
                    CreationDate = DateTime.Now,
                    InvoiceTotal = invoiceTotal,
                    Remarks = remanrks,
                    CustomerId = customerId
                };
                db.CustomerInvoice.Add(invoice);
                foreach (var order in orders)
                {
                    var _order = db.OrderMaster.FirstOrDefault(o => o.Id == order.Id);
                    _order.CustomerInvoiceId = invoiceId;
                    db.Entry(_order).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                return invoiceId;
            }
        }


        public CustomerInvoiceModel GetCustomerInvoice(Guid InvoiceId)
        {

            CustomerInvoiceModel model = new CustomerInvoiceModel();
            using (var db = new ApplicationDbContext())
            {
                db.Configuration.LazyLoadingEnabled = false;
                var invoice = db.CustomerInvoice.FirstOrDefault(inv => inv.Id == InvoiceId);
                if (invoice != null)
                {
                    model.Invoice = invoice;
                    var customer = db.Customer.FirstOrDefault(cust => cust.Id == invoice.CustomerId);
                    if (customer != null)
                        model.Customer = customer;
                    //Detail


                    var orderLines = (from ordLine in db.OrderDetail.Include("Product").Where(o => o.Active == 1 && o.IsCoupon != 1)
                                      join ord in db.OrderMaster on ordLine.OrderId equals ord.Id
                                      where ord.CustomerInvoiceId == InvoiceId
                                      select new { ordLine, ordLine.Product }).ToList().Select(d => new OrderLine(d.ordLine)).ToList();



                    var groups = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice }).ToList();

                    List<OrderLine> lines = new List<OrderLine>();
                    foreach (var group in groups)
                    {
                        var ordLine = group.First();
                        var grossTotal = group.Sum(s => s.GrossAmount());
                        var netTotal = group.Sum(s => s.NetAmount());
                        var qty = group.Sum(s => (s.Direction * s.Quantity));

                        lines.Add(new OrderLine(ordLine));

                    }




                    model.OrderDetails = lines.OrderBy(o => o.ItemName).ToList();

                }

            }
            return model;

        }
        public List<OrderLine> GetOrderDetailForCustomerInvoiceById(Guid customerInvoiceId)
        {


            var orderLines = new List<OrderLine>();
            using (var db = new ApplicationDbContext())
            {
                db.Configuration.LazyLoadingEnabled = false;
                orderLines = (from ordLine in db.OrderDetail.Include("Product").Where(o => o.Active == 1 && o.IsCoupon != 1)
                              join ord in db.OrderMaster on ordLine.OrderId equals ord.Id
                              where ord.CustomerInvoiceId == customerInvoiceId
                              select new { ordLine, ordLine.Product }).ToList().Select(d => new OrderLine(d.ordLine)).ToList();
                var groups = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice }).ToList();
                List<OrderLine> lines = new List<OrderLine>();
                foreach (var group in groups)
                {
                    var ordLine = group.First();
                    var grossTotal = group.Sum(s => s.GrossAmount());
                    var netTotal = group.Sum(s => s.NetAmount());
                    var qty = group.Sum(s => (s.Direction * s.Quantity));

                    lines.Add(ordLine);

                }

                return orderLines.OrderBy(o => o.ItemName).ToList();

            }

        }


        public List<Order> GetCustomerInvoices(string queryText, DateTime startDate, DateTime endDate)
        {


            string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
            string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";


            using (var db = new ApplicationDbContext())
            {
                string query = @"SELECT  CustomerInvoice.Id,  Customer.Id as CustomerId, Customer.Name as CustomerName, CustomerInvoice.InvoiceNumber, CustomerInvoice.Remarks, CustomerInvoice.CreationDate,CustomerInvoice.InvoiceTotal
                        FROM  Customer INNER JOIN
                                         CustomerInvoice ON Customer.Id = CustomerInvoice.CustomerId 
                         Where CustomerInvoice.CreationDate between '" + dt1 + "' AND '" + dt2 + "'";
                if (!string.IsNullOrEmpty(queryText))
                {
                    query = query + " AND CustomerInvoice.InvoiceNumber LIKE '%" + queryText + "%'";
                }
                List<Order> orders = new List<Order>();
                using (var conn = db.Database.Connection)
                {
                    IDbCommand command = new SqlCommand();
                    conn.Open();
                    command.Connection = conn;

                    command.CommandText = query;



                    IDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new Order
                        {
                            Id = Guid.Parse(dr["Id"].ToString()),
                            InvoiceNumber = Convert.ToString(dr["InvoiceNumber"]),
                            CustomerId = Guid.Parse(dr["CustomerId"].ToString()),
                            CustomerName = Convert.ToString(dr["CustomerName"]),
                            InvoiceDate = Convert.ToDateTime(dr["CreationDate"]),
                            OrderTotal = Convert.ToDecimal(dr["InvoiceTotal"])
                        });
                    }
                    dr.Dispose();
                }
                return orders;
            }

        }



        public List<Order> GetCustomerPendingOrders(Guid customerId, DateTime startDate,
            DateTime endDate)
        {
            List<Order> orders = new List<Order>();

            string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
            string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";
            using (var db = new ApplicationDbContext())
            {
                string query = @"SELECT OrderMaster.Id,OrderMaster.OrderNoOfDay, OrderMaster.InvoiceNumber, OrderMaster.CustomerId, (Customer.Name+' '+isnull(Customer.OrgNo,' ')+' '+isnull(Customer.Address1,' ')) as CustomerName, OrderMaster.InvoiceDate, OrderMaster.OrderTotal, OrderMaster.InvoiceGenerated
                                FROM OrderMaster INNER JOIN
                                     Receipt ON OrderMaster.Id = Receipt.OrderId INNER JOIN
                                     Customer ON OrderMaster.CustomerId = Customer.Id  
                                    Where OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status=13 AND OrderMaster.CustomerId='" + customerId +
                                       "' AND OrderMaster.CustomerInvoiceId is null AND InvoiceDate between '" + dt1 +
                                       "' AND '" + dt2 + "' Order By OrderMaster.CreationDate";
                using (var conn = db.Database.Connection)
                {
                    IDbCommand command = new SqlCommand();
                    command.Connection = conn;
                    conn.Open();
                    command.CommandText = query;


                    IDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new Order
                        {
                            Id = Guid.Parse(dr["Id"].ToString()),
                            InvoiceNumber = Convert.ToString(dr["InvoiceNumber"]),
                            OrderNoOfDay = Convert.ToString(dr["OrderNoOfDay"]),
                            CustomerId = Guid.Parse(dr["Id"].ToString()),
                            CustomerName = Convert.ToString(dr["CustomerName"]),
                            InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                            OrderTotal = Convert.ToDecimal(dr["OrderTotal"]),
                            InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"])
                        });
                    }
                    dr.Dispose();
                }
            }

            return orders;
        }

        public List<Order> GetCustomerCompletedOrder(string queryText, DateTime startDate, DateTime endDate)
        {


            string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
            string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";


            using (var db = new ApplicationDbContext())
            {
                string query = @"SELECT        OrderMaster.OrderId, OrderMaster.InvoiceNumber, OrderMaster.CustomerId, (Customer.Name+' '+isnull(Customer.OrgNo,' ')+' '+isnull(Customer.Address1,' ')) as CustomerName, OrderMaster.InvoiceDate, OrderMaster.OrderTotal, OrderMaster.InvoiceGenerated
FROM            OrderMaster INNER JOIN
                         Receipt ON OrderMaster.OrderId = Receipt.OrderId INNER JOIN
                         Customer ON OrderMaster.CustomerId = Customer.Id  
 Where OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status=13 AND InvoiceDate between '" + dt1 + "' AND '" +
                               dt2 + "'";
                if (!string.IsNullOrEmpty(queryText))
                {
                    query = query + " AND OrderMaster.InvoiceNumber LIKE '%" + queryText + "%'";
                }
                List<Order> orders = new List<Order>();
                using (var conn = db.Database.Connection)
                {
                    IDbCommand command = new SqlCommand();
                    conn.Open();
                    command.Connection = conn;

                    command.CommandText = query;



                    IDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new Order
                        {
                            Id = Guid.Parse(dr["OrderId"].ToString()),
                            InvoiceNumber = Convert.ToString(dr["InvoiceNumber"]),
                            CustomerId = Guid.Parse(dr["OrderId"].ToString()),
                            CustomerName = Convert.ToString(dr["CustomerName"]),
                            InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                            OrderTotal = Convert.ToDecimal(dr["OrderTotal"]),
                            InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"])
                        });
                    }
                    dr.Dispose();
                }
                return orders;
            }

        }
    }
}
