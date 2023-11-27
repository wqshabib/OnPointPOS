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
    public class SaleDataProvider
    {
        public List<OrderViewModel> GetOrderHistory(string dateFrom, string dateTo, ApplicationDbContext db)
        {


            DateTime startDate = Convert.ToDateTime(dateFrom).Date;
            DateTime endDate = Convert.ToDateTime(dateTo + " 23:59:59");
            List<OrderViewModel> orders = new List<OrderViewModel>();






            string query = @"SELECT        OrderMaster.Id, Receipt.ReceiptNumber, OrderMaster.OrderNoOfDay,OrderMaster.Comments, OrderMaster.InvoiceDate,  OrderMaster.OrderTotal, OrderMaster.InvoiceGenerated
FROM            OrderMaster INNER JOIN
                         Receipt ON OrderMaster.Id = Receipt.OrderId 
						 Where OrderMaster.InvoiceGenerated=1 AND  OrderMaster.InvoiceGenerated=1 						 
						 AND InvoiceDate between '" + startDate + "' AND '" + endDate + "'";

            IDbCommand command = new SqlCommand();
            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;

                command.CommandText = query;



                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    orders.Add(new OrderViewModel
                    {
                        Id = Guid.Parse(dr["Id"].ToString()),
                        InvoiceNumber = Convert.ToString(dr["ReceiptNumber"]),
                        OrderComments = string.IsNullOrEmpty(Convert.ToString(dr["Comments"])) ? Resource.OpenCustomer : Convert.ToString(dr["Comments"]),
                        OrderNoOfDay = Convert.ToString(dr["OrderNoOfDay"]),
                        InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                        OrderTotal = Convert.ToDecimal(dr["OrderTotal"]),
                        Status = OrderStatus.Completed,
                        InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"]),
                        OrderLines = new List<OrderLine>(),

                    });
                }
                dr.Dispose();



            }

            return orders.OrderBy(o => o.InvoiceDate).ToList();
        }

        public List<OrderViewModel> GetOrderHistoryTerminal(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            DateTime startDate = Convert.ToDateTime(dateFrom).Date;
            DateTime endDate = Convert.ToDateTime(dateTo + " 23:59:59");

            List<OrderViewModel> orders = new List<OrderViewModel>();

            string query = @"SELECT OrderMaster.Id, Receipt.ReceiptNumber, OrderMaster.OrderNoOfDay, OrderMaster.Comments, OrderMaster.InvoiceDate, 
                            OrderMaster.Status, (Payment.PaidAmount * Payment.Direction) AS OrderTotal, OrderMaster.InvoiceGenerated, Payment.PaymentRef
                            FROM OrderMaster 
                            INNER JOIN Payment ON OrderMaster.Id = Payment.OrderId
                            INNER JOIN Receipt ON OrderMaster.Id = Receipt.OrderId 
                            WHERE OrderMaster.InvoiceGenerated = 1 AND OrderMaster.TerminalId = '" + terminalId + "' " +
                            "AND InvoiceDate BETWEEN '" + startDate + "' AND '" + endDate + "'";

            IDbCommand command = new SqlCommand();

            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandText = query;

                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    orders.Add(new OrderViewModel
                    {
                        Id = Guid.Parse(dr["Id"].ToString()),
                        InvoiceNumber = Convert.ToString(dr["ReceiptNumber"]),
                        OrderComments = string.IsNullOrEmpty(Convert.ToString(dr["Comments"])) ? Resource.OpenCustomer : Convert.ToString(dr["Comments"]),
                        OrderNoOfDay = Convert.ToString(dr["OrderNoOfDay"]),
                        InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                        OrderTotal = Convert.ToDecimal(dr["OrderTotal"]),
                        Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), Convert.ToString(dr["Status"])),
                        InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"]),
                        PaymentRef = Convert.ToString(dr["PaymentRef"]),
                        OrderLines = new List<OrderLine>(),

                    });
                }

                dr.Dispose();
            }

            return orders.OrderBy(o => o.InvoiceDate).ToList();
        }

        public List<OrderLineViewModel> GetOrderDetailHistoryTerminal(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            DateTime startDate = Convert.ToDateTime(dateFrom).Date;
            DateTime endDate = Convert.ToDateTime(dateTo + " 23:59:59");

            List<OrderLineViewModel> orderDetails = new List<OrderLineViewModel>();

            string query = @"SELECT Product.Description, SUM(OrderDetail.Qty) AS TotalQty
                            FROM OrderMaster 
                            INNER JOIN OrderDetail ON OrderMaster.Id = OrderDetail.OrderId
                            INNER JOIN Product ON OrderDetail.ItemId = Product.Id 
                            WHERE OrderMaster.Status = 13  and OrderDetail.Active = 1 AND OrderDetail.Direction = 1 AND 
                            OrderDetail.ItemType = 0 AND OrderMaster.TerminalId = '" + terminalId + "' " +
                            "AND OrderMaster.CreationDate BETWEEN '" + startDate + "' AND '" + endDate + "' " +
                            "GROUP BY Product.Id, Product.Description";

            IDbCommand command = new SqlCommand();

            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandText = query;

                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    orderDetails.Add(new OrderLineViewModel
                    {
                        ItemDescription = Convert.ToString(dr["Description"]),
                        Quantity = Math.Round(Convert.ToDecimal(dr["TotalQty"]), 2),
                    });
                }

                dr.Dispose();
            }

            return orderDetails.OrderBy(o => o.ItemDescription).ToList();
        }

        public List<ReportTotalViewModel> GetReportTotalTerminal(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            DateTime startDate = Convert.ToDateTime(dateFrom).Date;
            DateTime endDate = Convert.ToDateTime(dateTo + " 23:59:59");

            List<ReportTotalViewModel> orders = new List<ReportTotalViewModel>();

            string query = @"SELECT SUM(Payment.PaidAmount * Payment.Direction) AS OrderTotal
                            FROM OrderMaster 
                            INNER JOIN Payment ON OrderMaster.Id = Payment.OrderId
                            INNER JOIN Receipt ON OrderMaster.Id = Receipt.OrderId 
                            WHERE OrderMaster.InvoiceGenerated = 1 AND OrderMaster.TerminalId = '" + terminalId + "' " +
                            "AND InvoiceDate BETWEEN '" + startDate + "' AND '" + endDate + "'";

            IDbCommand command = new SqlCommand();

            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandText = query;

                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    orders.Add(new ReportTotalViewModel
                    {
                        OrderTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]), 2)
                    });
                }

                dr.Dispose();
            }

            return orders.ToList();
        }

        public List<PaymentTypeTotalViewModel> GetPaymentTypeTotalTerminal(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            DateTime startDate = Convert.ToDateTime(dateFrom).Date;
            DateTime endDate = Convert.ToDateTime(dateTo + " 23:59:59");

            List<PaymentTypeTotalViewModel> orders = new List<PaymentTypeTotalViewModel>();

            string query = @"SELECT Payment.PaymentType, Payment.PaymentRef, SUM(Payment.PaidAmount * Payment.Direction) AS OrderTotal 
                            FROM OrderMaster 
                            INNER JOIN Payment ON OrderMaster.Id = Payment.OrderId
                            INNER JOIN Receipt ON OrderMaster.Id = Receipt.OrderId 
                            WHERE OrderMaster.InvoiceGenerated = 1 AND OrderMaster.TerminalId = '" + terminalId + "' " +
                            "AND InvoiceDate BETWEEN '" + startDate + "' AND '" + endDate + "' " +
                            "GROUP BY Payment.PaymentType, Payment.PaymentRef";

            IDbCommand command = new SqlCommand();

            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandText = query;

                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    orders.Add(new PaymentTypeTotalViewModel
                    {
                        PaymentTypeId = Convert.ToInt32(dr["PaymentType"]),
                        PaymentTypeName = Convert.ToString(dr["PaymentRef"]),
                        PaymentTypeTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]), 2),
                    });
                }

                dr.Dispose();
            }

            return orders.OrderBy(o => o.PaymentTypeId).ToList();
        }

        public List<OrderViewModel> GetOrderHistoryWebShopTerminal(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            DateTime startDate = Convert.ToDateTime(dateFrom).Date;
            DateTime endDate = Convert.ToDateTime(dateTo + " 23:59:59");

            List<OrderViewModel> orders = new List<OrderViewModel>();

            string query = @"SELECT OrderMaster.Id, Receipt.ReceiptNumber, OrderMaster.OrderNoOfDay, OrderMaster.Comments, OrderMaster.InvoiceDate, 
                            OrderMaster.Status, (Payment.PaidAmount * Payment.Direction) AS OrderTotal, OrderMaster.InvoiceGenerated, Payment.PaymentRef, Payment.StripeFee
                            FROM OrderMaster 
                            INNER JOIN Payment ON OrderMaster.Id = Payment.OrderId
                            INNER JOIN Receipt ON OrderMaster.Id = Receipt.OrderId 
                            WHERE OrderMaster.InvoiceGenerated = 1 AND OrderMaster.TerminalId = '" + terminalId + "' " +
                            "AND InvoiceDate BETWEEN '" + startDate + "' AND '" + endDate + "'";

            IDbCommand command = new SqlCommand();

            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandText = query;

                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    orders.Add(new OrderViewModel
                    {
                        Id = Guid.Parse(dr["Id"].ToString()),
                        InvoiceNumber = Convert.ToString(dr["ReceiptNumber"]),
                        OrderComments = string.IsNullOrEmpty(Convert.ToString(dr["Comments"])) ? Resource.OpenCustomer : Convert.ToString(dr["Comments"]),
                        OrderNoOfDay = Convert.ToString(dr["OrderNoOfDay"]),
                        InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                        OrderTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]), 2),
                        Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), Convert.ToString(dr["Status"])),
                        InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"]),
                        PaymentRef = Convert.ToString(dr["PaymentRef"]),
                        StripeFee = Math.Round(Convert.ToDecimal(dr["StripeFee"]), 2),
                        NetTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]) - Convert.ToDecimal(dr["StripeFee"]),2),
                        OrderLines = new List<OrderLine>(),

                    });
                }

                dr.Dispose();
            }

            return orders.OrderBy(o => o.InvoiceDate).ToList();
        }

        public List<ReportTotalViewModel> GetReportTotalWebShopTerminal(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            DateTime startDate = Convert.ToDateTime(dateFrom).Date;
            DateTime endDate = Convert.ToDateTime(dateTo + " 23:59:59");

            List<ReportTotalViewModel> orders = new List<ReportTotalViewModel>();

            string query = @"SELECT SUM(Payment.PaidAmount * Payment.Direction) AS OrderTotal, SUM(Payment.StripeFee * Payment.Direction) AS FeeTotal
                            FROM OrderMaster 
                            INNER JOIN Payment ON OrderMaster.Id = Payment.OrderId
                            INNER JOIN Receipt ON OrderMaster.Id = Receipt.OrderId 
                            WHERE OrderMaster.InvoiceGenerated = 1 AND OrderMaster.TerminalId = '" + terminalId + "' " +
                            "AND InvoiceDate BETWEEN '" + startDate + "' AND '" + endDate + "'";

            IDbCommand command = new SqlCommand();

            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandText = query;

                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    orders.Add(new ReportTotalViewModel
                    {
                        OrderTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]), 2),
                        FeeTotal = Math.Round(Convert.ToDecimal(dr["FeeTotal"]), 2),
                        NetTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]) - Convert.ToDecimal(dr["FeeTotal"]), 2),
                    });
                }

                dr.Dispose();
            }

            return orders.ToList();
        }

        public List<CategorySaleDetail> GetCategorySale(DateTime dtFrom, DateTime dtTo, int categoryId)
        {
            return new List<CategorySaleDetail>();
        }
        public List<PaymentDetail> GetPaymentDetail(DateTime dtFrom, DateTime dtTo)
        {
            return new List<PaymentDetail>();
        }
        public ReportInfo GetZReport(string dateFrom, string dateTo, string terminalId, string userName, ApplicationDbContext db)
        {
            var dtFrom = Convert.ToDateTime(dateFrom).Date;
            Guid terminalGuid = Guid.Parse(terminalId);
            var dtTo = Convert.ToDateTime(dateTo + "  11:59:00 PM");
            return ReportGenerator.GetRDLCZReport(terminalId, dtFrom, dtTo, userName, db);

        }
        public SaleModel GetCategorySaleByUser(string userId, string dateFrom, string dateTo, ApplicationDbContext db)
        {
            var model = new SaleModel();

            var dtFrom = Convert.ToDateTime(dateFrom).Date;
            var dtTo = Convert.ToDateTime(dateTo + "  11:59:00 PM");

            var categorySale = new List<CategorySaleDetail>();

            var payments = new List<PaymentDetail>();
            Guid terminalId = default(Guid);
            var order = db.OrderMaster.FirstOrDefault(o => o.UserId == userId);
            var user = db.OutletUser.FirstOrDefault(o => o.Id == userId);
            if (order != null)
                terminalId = order.TerminalId;
            var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
            if (terminal != null)
            {
                model.Terminal = terminal.UniqueIdentification;
                model.Outlet = terminal.Outlet.Name;
            }
            else
            {
                model.Terminal = Resource.All + " " + Resource.Terminals;
                model.Outlet = Resource.All + " " + Resource.Outlets;
            }
            model.UserName = user.UserName;
            string paymentQuery = @"SELECT  Direction*Payment.PaidAmount as PaidAmount,OrderMaster.RoundedAmount, Payment.PaymentType as TypeId,PaymentType.SwedishName
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
				INNER JOIN PaymentType on PaymentType.Id=Payment.PaymentType
					WHERE OrderMaster.UserId='" + userId + "' AND OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND Payment.PaidAmount <>0 AND InvoiceDate BETWEEN '" + dtFrom + "' AND '" + dtTo + "'";

            IDbCommand cmd = new SqlCommand(paymentQuery);
            using (var conn = db.Database.Connection)
            {
                conn.Open();
                cmd.Connection = conn;

                List<PaymentDetail> paymentList = new List<PaymentDetail>();
                IDataReader drPayment = cmd.ExecuteReader();
                while (drPayment.Read())
                {
                    paymentList.Add(new PaymentDetail
                    {
                        PaidAmount = Convert.ToDecimal(drPayment["PaidAmount"]),
                        RoundAmount = Convert.ToDecimal(drPayment["RoundedAmount"]),
                        PaymentRef = Convert.ToString(drPayment["SwedishName"]),
                        PaymentType = Convert.ToInt16(drPayment["TypeId"])
                    });
                }
                var paymentGroups = paymentList.GroupBy(p => p.PaymentType).ToList();

                foreach (var grp in paymentGroups)
                {
                    var totalPayment = grp.Sum(s => s.PaidAmount);
                    var roundAmount = grp.Sum(s => s.RoundAmount);
                    var paymentcount = grp.Count();
                    var paymentRef = grp.First().PaymentRef;

                    payments.Add(new PaymentDetail
                    {
                        PaidAmount = totalPayment,
                        PaymentCount = paymentcount,
                        PaymentRef = paymentRef,
                        RoundAmount = roundAmount
                    });
                }
                drPayment.Dispose();
                cmd.Dispose();
                IDbCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_SaleByUser";
                command.Parameters.Add(new SqlParameter("@UserId", userId));
                command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));


                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    categorySale.Add(new CategorySaleDetail
                    {
                        CategoryName = Convert.ToString(dr["Name"]),
                        TotalSale = Convert.ToDecimal(dr["GrossTotal"]),
                        NetSale = Convert.ToDecimal(dr["NetTotal"]),
                        VAT = Convert.ToDecimal(dr["VAT"]),

                        Percentage25Total = Convert.ToDecimal(dr["Percentage25Total"]),
                        Percentage25VAT = Math.Round((Convert.ToDecimal(dr["Percentage25Total"]) / Convert.ToDecimal(1.25)) * Convert.ToDecimal(0.25), 2),
                        Percentage25NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage25Total"]) - (Convert.ToDecimal(dr["Percentage25Total"]) / Convert.ToDecimal(1.25)) * Convert.ToDecimal(0.25), 2),

                        Percentage12Total = Convert.ToDecimal(dr["Percentage12Total"]),
                        Percentage12VAT = Math.Round((Convert.ToDecimal(dr["Percentage12Total"]) / Convert.ToDecimal(1.12)) * Convert.ToDecimal(0.12), 2),
                        Percentage12NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage12Total"]) - (Convert.ToDecimal(dr["Percentage12Total"]) / Convert.ToDecimal(1.12)) * Convert.ToDecimal(0.12), 2),

                        Percentage6Total = Convert.ToDecimal(dr["Percentage6Total"]),
                        Percentage6VAT = Math.Round((Convert.ToDecimal(dr["Percentage6Total"]) / Convert.ToDecimal(1.06)) * Convert.ToDecimal(0.06), 2),
                        Percentage6NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage6Total"]) - (Convert.ToDecimal(dr["Percentage6Total"]) / Convert.ToDecimal(1.06)) * Convert.ToDecimal(0.06), 2),

                        Percentage0Total = Convert.ToDecimal(dr["Percentage0Total"]),
                        Percentage0NetTotal = Convert.ToDecimal(dr["Percentage0Total"]),
                        Percentage0VAT = 0,



                    });
                }
                dr.Dispose();

            }

            model.CategorySaleDetail = categorySale;
            model.PaymentDetails = payments;

            return model;
        }

        public SaleModel GetCategorySale(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            var model = new SaleModel();

            var dtFrom = Convert.ToDateTime(dateFrom).Date;
            Guid terminalGuid = Guid.Parse(terminalId);
            var dtTo = Convert.ToDateTime(dateTo + "  11:59:00 PM");

            var categorySale = new List<CategorySaleDetail>();

            var payments = new List<PaymentDetail>();


            var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalGuid);
            if (terminal != null)
            {
                model.Terminal = terminal.UniqueIdentification;
                model.Outlet = terminal.Outlet.Name;
            }
            else
            {
                model.Terminal = Resource.All + " " + Resource.Terminals;
                model.Outlet = Resource.All + " " + Resource.Outlets;
            }

            string paymentQuery = @"SELECT  Direction*Payment.PaidAmount as PaidAmount,OrderMaster.RoundedAmount, Payment.PaymentType as TypeId,PaymentType.SwedishName
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
				INNER JOIN PaymentType on PaymentType.Id=Payment.PaymentType
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND Payment.PaidAmount <>0 AND InvoiceDate BETWEEN '" + dtFrom + "' AND '" + dtTo + "'";
            if (terminalGuid != default(Guid))
                paymentQuery = paymentQuery + "   AND  OrderMaster.TerminalId ='" + terminalGuid + "'";
            IDbCommand cmd = new SqlCommand(paymentQuery);
            using (var conn = db.Database.Connection)
            {
                conn.Open();
                cmd.Connection = conn;

                List<PaymentDetail> paymentList = new List<PaymentDetail>();
                IDataReader drPayment = cmd.ExecuteReader();
                while (drPayment.Read())
                {
                    paymentList.Add(new PaymentDetail
                    {
                        PaidAmount = Convert.ToDecimal(drPayment["PaidAmount"]),
                        RoundAmount = Convert.ToDecimal(drPayment["RoundedAmount"]),
                        PaymentRef = Convert.ToString(drPayment["SwedishName"]),
                        PaymentType = Convert.ToInt16(drPayment["TypeId"])
                    });
                }
                var paymentGroups = paymentList.GroupBy(p => p.PaymentType).ToList();

                foreach (var grp in paymentGroups)
                {
                    var totalPayment = grp.Sum(s => s.PaidAmount);
                    var roundAmount = grp.Sum(s => s.RoundAmount);
                    var paymentcount = grp.Count();
                    var paymentRef = grp.First().PaymentRef;

                    payments.Add(new PaymentDetail
                    {
                        PaidAmount = totalPayment,
                        PaymentCount = paymentcount,
                        PaymentRef = paymentRef,
                        RoundAmount = roundAmount
                    });
                }
                drPayment.Dispose();
                cmd.Dispose();
                IDbCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_SaleByCategory";
                command.Parameters.Add(new SqlParameter("@TerminalId", terminalId));
                command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));


                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    categorySale.Add(new CategorySaleDetail
                    {
                        CategoryName = Convert.ToString(dr["Name"]),
                        TotalSale = Convert.ToDecimal(dr["GrossTotal"]),
                        NetSale = Convert.ToDecimal(dr["NetTotal"]),
                        VAT = Convert.ToDecimal(dr["VAT"]),

                        Percentage25Total = Convert.ToDecimal(dr["Percentage25Total"]),
                        Percentage25VAT = Math.Round((Convert.ToDecimal(dr["Percentage25Total"]) / Convert.ToDecimal(1.25)) * Convert.ToDecimal(0.25), 2),
                        Percentage25NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage25Total"]) - (Convert.ToDecimal(dr["Percentage25Total"]) / Convert.ToDecimal(1.25)) * Convert.ToDecimal(0.25), 2),

                        Percentage12Total = Convert.ToDecimal(dr["Percentage12Total"]),
                        Percentage12VAT = Math.Round((Convert.ToDecimal(dr["Percentage12Total"]) / Convert.ToDecimal(1.12)) * Convert.ToDecimal(0.12), 2),
                        Percentage12NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage12Total"]) - (Convert.ToDecimal(dr["Percentage12Total"]) / Convert.ToDecimal(1.12)) * Convert.ToDecimal(0.12), 2),

                        Percentage6Total = Convert.ToDecimal(dr["Percentage6Total"]),
                        Percentage6VAT = Math.Round((Convert.ToDecimal(dr["Percentage6Total"]) / Convert.ToDecimal(1.06)) * Convert.ToDecimal(0.06), 2),
                        Percentage6NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage6Total"]) - (Convert.ToDecimal(dr["Percentage6Total"]) / Convert.ToDecimal(1.06)) * Convert.ToDecimal(0.06), 2),

                        Percentage0Total = Convert.ToDecimal(dr["Percentage0Total"]),
                        Percentage0NetTotal = Convert.ToDecimal(dr["Percentage0Total"]),
                        Percentage0VAT = 0,



                    });
                }
                dr.Dispose();

            }

            model.CategorySaleDetail = categorySale;
            model.PaymentDetails = payments;

            return model;
        }
        public SaleModel GetAccountingSale(string dateFrom, string dateTo, string terminalId, ApplicationDbContext db)
        {
            var model = new SaleModel();
            DateTime date = DateTime.Now.Date;
            int today = date.Day;
            int year = date.Year;
            int month = date.Month;
            var dtFrom = Convert.ToDateTime(dateFrom).Date;
            Guid terminalGuid = Guid.Parse(terminalId);
            var dtTo = Convert.ToDateTime(dateTo + "  11:59:00 PM");

            var categorySale = new List<CategorySaleDetail>();

            var payments = new List<PaymentDetail>();
            var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalGuid);
            if (terminal != null)
            {
                model.Terminal = terminal.UniqueIdentification;
                model.Outlet = terminal.Outlet.Name;
            }
            else
            {
                model.Terminal = Resource.All + " " + Resource.Terminals;
                model.Outlet = Resource.All + " " + Resource.Outlets;
            }

            string paymentQuery = @"SELECT  Direction*Payment.PaidAmount as PaidAmount,OrderMaster.RoundedAmount, Payment.PaymentType as TypeId,PaymentType.SwedishName
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
				INNER JOIN PaymentType on PaymentType.Id=Payment.PaymentType
					WHERE  OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND Payment.PaidAmount <>0 AND InvoiceDate BETWEEN '" + dtFrom + "' AND '" + dtTo + "'";
            if (terminalGuid != default(Guid))
                paymentQuery = paymentQuery + "   AND  OrderMaster.TerminalId ='" + terminalGuid + "'";
            IDbCommand cmd = new SqlCommand(paymentQuery);
            using (var conn = db.Database.Connection)
            {
                conn.Open();
                cmd.Connection = conn;

                List<PaymentDetail> paymentList = new List<PaymentDetail>();
                IDataReader drPayment = cmd.ExecuteReader();
                while (drPayment.Read())
                {
                    paymentList.Add(new PaymentDetail
                    {
                        PaidAmount = Convert.ToDecimal(drPayment["PaidAmount"]),
                        RoundAmount = Convert.ToDecimal(drPayment["RoundedAmount"]),
                        PaymentRef = Convert.ToString(drPayment["SwedishName"]),
                        PaymentType = Convert.ToInt16(drPayment["TypeId"])
                    });
                }
                var paymentGroups = paymentList.GroupBy(p => p.PaymentType).ToList();
                foreach (var grp in paymentGroups)
                {
                    var totalPayment = grp.Sum(s => s.PaidAmount);
                    var roundAmount = grp.Sum(s => s.RoundAmount);
                    var paymentcount = grp.Count();
                    var paymentRef = grp.First().PaymentRef;

                    payments.Add(new PaymentDetail
                    {
                        PaidAmount = totalPayment,
                        PaymentCount = paymentcount,
                        PaymentRef = paymentRef,
                        RoundAmount = roundAmount
                    });
                }
                drPayment.Dispose();
                cmd.Dispose();
                IDbCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_SaleByAccounting";
                command.Parameters.Add(new SqlParameter("@TerminalId", terminalId));
                command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));



                IDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {


                    categorySale.Add(new CategorySaleDetail
                    {
                        CategoryName = Convert.ToString(dr["AcNo"]) + " " + Convert.ToString(dr["Name"]),
                        TotalSale = Convert.ToDecimal(dr["GrossTotal"]),
                        NetSale = Convert.ToDecimal(dr["NetTotal"]),
                        VAT = Convert.ToDecimal(dr["VAT"]),

                        Percentage25Total = Convert.ToDecimal(dr["Percentage25Total"]),
                        Percentage25VAT = (Math.Round(Convert.ToDecimal(dr["Percentage25Total"]), 2) / Convert.ToDecimal(1.25) * Convert.ToDecimal(0.25)),
                        Percentage25NetTotal = Convert.ToDecimal(dr["Percentage25Total"]) - (Math.Round(Convert.ToDecimal(dr["Percentage25Total"]), 2) / Convert.ToDecimal(1.25) * Convert.ToDecimal(0.25)),

                        Percentage12Total = Convert.ToDecimal(dr["Percentage12Total"]),
                        Percentage12VAT = Math.Round((Convert.ToDecimal(dr["Percentage12Total"]) / Convert.ToDecimal(1.12)) * Convert.ToDecimal(0.12), 2),
                        Percentage12NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage12Total"]) - (Convert.ToDecimal(dr["Percentage12Total"]) / Convert.ToDecimal(1.12)) * Convert.ToDecimal(0.12), 2),

                        Percentage6Total = Convert.ToDecimal(dr["Percentage6Total"]),
                        Percentage6VAT = Math.Round((Convert.ToDecimal(dr["Percentage6Total"]) / Convert.ToDecimal(1.06)) * Convert.ToDecimal(0.06), 2),
                        Percentage6NetTotal = Math.Round(Convert.ToDecimal(dr["Percentage6Total"]) - (Convert.ToDecimal(dr["Percentage6Total"]) / Convert.ToDecimal(1.06)) * Convert.ToDecimal(0.06), 2),

                        Percentage0Total = Convert.ToDecimal(dr["Percentage0Total"]),
                        Percentage0NetTotal = Convert.ToDecimal(dr["Percentage0Total"]),
                        Percentage0VAT = 0,



                    });
                }
                dr.Dispose();

            }

            model.CategorySaleDetail = categorySale;
            model.PaymentDetails = payments;

            return model;
        }

        public List<VATDetail> GetVATSale(string dateFrom, string dateTo, string terminalId, out string outlet, out string terminal, ApplicationDbContext db)
        {

            DateTime date = DateTime.Now.Date;
            int today = date.Day;
            int year = date.Year;
            int month = date.Month;
            var dtFrom = Convert.ToDateTime(dateFrom).Date;
            Guid terminalGuid = Guid.Parse(terminalId);
            var dtTo = Convert.ToDateTime(dateTo + "  11:59:00 PM");


            var vatSales = new List<VATDetail>();
            using (var conn = db.Database.Connection)
            {

                var _terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalGuid);
                if (_terminal != null)
                {
                    terminal = _terminal.UniqueIdentification;
                    outlet = _terminal.Outlet.Name;
                }
                else
                {
                    terminal = Resource.All + " " + Resource.Terminals;
                    outlet = Resource.All + " " + Resource.Outlets;
                }

                string paymentQuery = @"SELECT OrderDetail.TaxPercent,CAST(OrderMaster.InvoiceDate AS DATE) as InvoiceDate ,
Cast(Round(Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)),2)as numeric(12,2)) As Net,
Cast(Round(isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0),2) as numeric(12,2)) AS VATSum
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID	
				WHERE OrderMaster.TrainingMode=0 AND    OrderMaster.InvoiceGenerated=1 AND OrderDetail.ItemType<>1  AND  OrderDetail.Active=1 AND OrderDetail.Direction=1 AND OrderMaster.InvoiceDate BETWEEN '" + dtFrom + "' AND '" + dtTo + "'";

                if (terminalGuid != default(Guid))
                    paymentQuery = paymentQuery + "   AND  OrderMaster.TerminalId ='" + terminalGuid + "'";
                paymentQuery = paymentQuery + "GROUP BY OrderDetail.TaxPercent,CAST(OrderMaster.InvoiceDate AS DATE)   order by CAST(OrderMaster.InvoiceDate AS DATE),OrderDetail.TaxPercent";
                IDbCommand cmd = new SqlCommand(paymentQuery);
                conn.Open();
                cmd.Connection = conn;
                IDataReader drPayment = cmd.ExecuteReader();
                while (drPayment.Read())
                {
                    vatSales.Add(new VATDetail
                    {
                        InvoiceDate = Convert.ToDateTime(drPayment["InvoiceDate"]),
                        TaxPercent = Convert.ToDecimal(drPayment["TaxPercent"]),
                        Net = Convert.ToDecimal(drPayment["Net"]),
                        VatSum = Convert.ToDecimal(drPayment["VatSum"]),
                        ExcpectedVat = Math.Round(Convert.ToDecimal(drPayment["Net"]) * Convert.ToDecimal(drPayment["TaxPercent"]) / 100, 2)

                    });
                }

                drPayment.Dispose();
                cmd.Dispose();


            }

            return vatSales;
        }

        public List<ItemViewModel> GetProductPrices(ApplicationDbContext db)
        {
            List<ItemViewModel> product = new List<ItemViewModel>();
            product = db.Product.Where(c => c.PlaceHolder == false).Select(x => new ItemViewModel
            {
                Description = x.Description,
                SKU = x.SKU,
                Price = x.Price,
                BarCode = x.BarCode
            }).OrderBy(o => o.Description).ToList();


            return product;
        }

        public List<ItemViewModel> GetItemStock(ApplicationDbContext db, int catId = 0)
        {
            List<ItemViewModel> product = new List<ItemViewModel>();
            if (catId == 0 || catId == 1)
            {
                product = db.Product.Where(c => c.PlaceHolder == false).Select(x => new ItemViewModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    SKU = x.SKU,
                    Price = x.Price,
                    BarCode = x.BarCode,
                    ReorderLevelQuantity = x.ReorderLevelQuantity,
                    StockQuantity = x.StockQuantity,
                    Unit = x.Unit

                }).OrderBy(o => o.Description).ToList();

            }
            else
            {
                product = (from x in db.Product.Where(c => c.PlaceHolder == false)
                           join cp in db.ItemCategory.Where(c => c.CategoryId == catId) on x.Id equals cp.ItemId
                           select new ItemViewModel
                           {
                               Id = x.Id,
                               Description = x.Description,
                               SKU = x.SKU,
                               Price = x.Price,
                               BarCode = x.BarCode,
                               ReorderLevelQuantity = x.ReorderLevelQuantity,
                               StockQuantity = x.StockQuantity,
                               Unit = x.Unit

                           }).OrderBy(o => o.Description).ToList();
            }
            return product;
        }

        public List<ItemViewModel> GetItemStockWithPagination(ApplicationDbContext db, int catId = 0, int pageNo = 0, int pageSize = 5)
        {
            List<ItemViewModel> product = new List<ItemViewModel>();
            if (catId == 0 || catId == 1)
            {
                product = db.Product.Where(c => c.PlaceHolder == false).Select(x => new ItemViewModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    SKU = x.SKU,
                    Price = x.Price,
                    BarCode = x.BarCode,
                    ReorderLevelQuantity = x.ReorderLevelQuantity,
                    StockQuantity = x.StockQuantity,
                    Unit = x.Unit

                }).OrderBy(a => a.Description).Skip(pageNo * pageSize).Take(pageSize).ToList();

            }
            else
            {
                product = (from x in db.Product.Where(c => c.PlaceHolder == false)
                           join cp in db.ItemCategory.Where(c => c.CategoryId == catId) on x.Id equals cp.ItemId
                           select new ItemViewModel
                           {
                               Id = x.Id,
                               Description = x.Description,
                               SKU = x.SKU,
                               Price = x.Price,
                               BarCode = x.BarCode,
                               ReorderLevelQuantity = x.ReorderLevelQuantity,
                               StockQuantity = x.StockQuantity,
                               Unit = x.Unit

                           }).OrderBy(a => a.Description).Skip(pageNo * pageSize).Take(pageSize).ToList();
            }
            return product;
        }

        public List<ItemViewModel> GetItemStockById(ApplicationDbContext db, Guid id)
        {
            List<ItemViewModel> product = new List<ItemViewModel>();
           
                product = (from x in db.Product.Where(c => c.Id == id)
                         
                           select new ItemViewModel
                           {
                               Id = x.Id,
                               Description = x.Description,
                               SKU = x.SKU,
                               Price = x.Price,
                               BarCode = x.BarCode,
                               ReorderLevelQuantity = x.ReorderLevelQuantity,
                               StockQuantity = x.StockQuantity,
                               Unit = x.Unit

                           }).OrderBy(o => o.Description).ToList();
            
            return product;
        }
        public List<ItemViewModel> GetProductList(ApplicationDbContext db)
        {
            List<ItemViewModel> product = new List<ItemViewModel>();

            product = db.Product.Where(c => c.PlaceHolder == false).Select(x => new ItemViewModel
            {
                Id = x.Id,
                Description = x.Description,
                SKU = x.SKU,
                Price = x.Price,
                BarCode = x.BarCode,
                PLU = x.PLU,
                Tax = x.Tax,
                Unit = x.Unit

            }).OrderBy(o => o.Description).ToList();

            return product;

        }
        public List<ItemStockViewModel> GetExpiryItems(ApplicationDbContext db)
        {
            var data = from item in db.ItemStock.OrderBy(o => o.ExpiryDate)
                       join p in db.Product on item.ItemId equals p.Id
                       select new ItemStockViewModel
                       {
                           Id = item.Id,
                           ItemId = item.ItemId,
                           Quantity = item.Quantity,
                           BatchNo = item.BatchNo,
                           ExpiryDate = item.ExpiryDate,
                           Description = p.Description
                       };

            return data.ToList();
        }
    }

}
