using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace POSSUM.Web.Models
{
    public class ReportInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string URL { get; set; }
        public string Logo { get; set; }
        public string OrgNo { get; set; }
        public string Footer { get; set; }
        public string Header { get; set; }
        public string TaxDescription { get; set; }
        public List<ReportRow> ReportRows { get; set; }
        public string PostalCode { get; internal set; }
    }
    public class ReportGenerator
    {


        internal static ReportInfo GetReport(Guid reportId, int reportType, string userId, ApplicationDbContext db)
        {
            ReportInfo info = new ReportInfo();
            List<ReportRow> rows = new List<ReportRow>();



            info.Header = Resource.Report_Printed + " " + DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss") + "  " + Resource.Report_PrintedOf + " " + userId;




            var report = db.Report.FirstOrDefault(i => i.Id == reportId);
            if (report != null && report.Terminal != null && report.Terminal.Outlet != null)
            {
                info.Name = report.Terminal.Outlet.Name;
                info.Address = report.Terminal.Outlet.Address;
                info.OrgNo = report.Terminal.Outlet.OrgNo;
                info.Phone = report.Terminal.Outlet.Phone;

            }
            var reportData = db.ReportData.Where(rd => rd.ReportId == reportId).OrderBy(rd => rd.SortOrder).ToList();
            SaleType saleType = SaleType.Retail;

            var setting = db.Setting.FirstOrDefault(a => a.Code == SettingCode.SaleType);
            if (setting != null)
            {
                Enum.TryParse(setting.Value, true, out saleType);
            }

            //Deposit_FIX_WAQAS
            var depositInCash = reportData.FirstOrDefault(a => a.DataType == "Deposit In (Cash)");
            var depositInCard = reportData.FirstOrDefault(a => a.DataType == "Deposit In (Card)");

            if (depositInCash != null)
            {
                var record = reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 1);
                if (record != null)
                {
                    reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 1).Value = record.Value + depositInCash.Value;
                }
                else
                {
                    var sortRecord = reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale");
                    reportData.Add(new ReportData()
                    {
                        DataType = "PaymentTypeSale",
                        ForeignId = 1,
                        Value = depositInCash.Value,
                        TaxPercent = 0,
                        TextValue = "",
                        ReportId = depositInCash.ReportId,
                        SortOrder = sortRecord.SortOrder
                    });
                }
            }

            if (depositInCard != null)
            {
                var record = reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 4);
                if (record != null)
                {
                    reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 4).Value = record.Value + depositInCard.Value;
                }
            }
            //Deposit_FIX_WAQAS

            foreach (var dr in reportData)
            {
                if (dr.DataType == "TipTypeSale" && saleType == SaleType.Retail)
                {
                    continue;
                }

                rows.Add(ReportRow.FromDb(dr));
            }


            info.ReportRows = rows;
            //string query = @"select * from reportdata where reportId = '" + reportId.ToString() + "' order by SortOrder asc";
            //var tbl = DBAccess.GetData(query);
            //foreach(DataRow row in tbl.Rows)
            //{
            //    b.AppendLine(ReportRow.FromDb(row).ToReportString());
            //}

            return info;
        }

        internal static String GetPDFReport(string terminalId, DateTime dtFrom, DateTime dtTo, string userName, ApplicationDbContext db)
        {
            StringBuilder b = new StringBuilder();
            b.Append("<html style='font-size:25px; font-family:Helvetica, sans-serif;'>");
            b.Append("<body style='display:block; margin:auto;'>");
            string SingleDottedLine = "---------------------------------------------";
            string newline = "<br>";
            b.AppendLine(SingleDottedLine);
            b.Append(newline);
            b.AppendLine("-------------- Z  RAPPORT -------------------");
            b.Append(newline);
            //if (Defaults.User.TrainingMode == true)
            //    b.AppendLine("------------ " + Resource.Report_Trainingmode + " -------------");
            b.AppendLine(SingleDottedLine);
            b.Append(newline);
            b.AppendLine("Utskriven " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " av " + userName);
            b.Append(newline);
            b.AppendLine(SingleDottedLine);

            b.Append(newline);
            ReportInfo info = new ReportInfo();
            List<ReportRow> data = new List<ReportRow>();
            List<ReportData> reportData = new List<ReportData>();



            var outlet = db.Outlet.FirstOrDefault();
            if (outlet != null)
            {
                info.Name = outlet.Name;
                info.Address = outlet.Address;
                info.OrgNo = outlet.OrgNo;
                info.Phone = outlet.Phone;
            }

            List<ZReportSetting> reportSettings = db.ZReportSetting.ToList();
            bool ShowSale = reportSettings.First(rp => rp.Id == 1).Visiblity;
            bool ShowVat = reportSettings.First(rp => rp.Id == 2).Visiblity;
            bool ShowCashDrawer = reportSettings.First(rp => rp.Id == 3).Visiblity;
            bool ShowCategorySale = reportSettings.First(rp => rp.Id == 4).Visiblity;
            bool ShowAccountSale = reportSettings.First(rp => rp.Id == 5).Visiblity;
            bool ShowPayment = reportSettings.First(rp => rp.Id == 6).Visiblity;
            bool ShowRoundig = reportSettings.First(rp => rp.Id == 7).Visiblity;
            bool ShowItemCorrection = reportSettings.First(rp => rp.Id == 8).Visiblity;
            bool ShowSaleTotal = reportSettings.First(rp => rp.Id == 9).Visiblity;
            bool ShowTrainingSale = reportSettings.First(rp => rp.Id == 10).Visiblity;
            IDbCommand command = new SqlCommand();
            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_PrintDetailReportByDateRange";
                command.Parameters.Add(new SqlParameter("@TerminalId", terminalId));
                command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));

                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    reportData.Add(new ReportData
                    {
                        ReportId = Guid.Parse(dr["ReportId"].ToString()),
                        DataType = Convert.ToString(dr["DataType"]),
                        DateValue = dr["DateValue"] != DBNull.Value ? Convert.ToDateTime(dr["DateValue"]) : DateTime.Now,
                        Value = dr["Value"] != DBNull.Value ? Convert.ToDecimal(dr["Value"]) : 0,
                        SortOrder = dr["SortOrder"] != DBNull.Value ? Convert.ToInt32(dr["SortOrder"]) : 0,
                        ForeignId = dr["ForeignId"] != DBNull.Value ? Convert.ToInt32(dr["ForeignId"]) : 0,
                        TaxPercent = dr["TaxPercent"] != DBNull.Value ? Convert.ToDecimal(dr["TaxPercent"]) : 0,
                        TextValue = Convert.ToString(dr["TextValue"])
                    });
                }
                dr.Close();

            }
            var reportSortedData = reportData.OrderBy(rd => rd.SortOrder).ToList();

            foreach (var dr in reportSortedData)
            {
                if ((dr.DataType == "ItemCorrection" || dr.DataType == "RemovedItem") && ShowItemCorrection == false)
                    continue;
                if ((dr.DataType == "CashDrawerOpen" || dr.DataType == "CashAdded" || dr.DataType == "CashDropped" || dr.DataType == "CashIn" || dr.DataType == "CashOut" || dr.DataType == "CashSum") && ShowCashDrawer == false)
                    continue;
                if (dr.DataType == "TrainingModeSale" && ShowTrainingSale == false)
                    continue;
                if ((dr.DataType == "ACVATPercent" || dr.DataType == "ACVatSum" || dr.DataType == "ACTable" || dr.DataType == "ACTakeaway" || dr.DataType == "ACStandared" || dr.DataType == "ACNetTotal" || dr.DataType == "ACTotal" || dr.DataType == "Accounting") && ShowAccountSale == false)
                    continue;

                if ((dr.DataType == "CategorySaleCount" || dr.DataType == "CategorySaleCountHeading" || dr.DataType == "CategorySale" || dr.DataType == "CategorySaleHeading") && ShowCategorySale == false)
                    continue;
                if (dr.DataType == "ACStandared")
                {

                }
                if (dr.DataType == "ACTable")
                {

                }
                if (dr.DataType == "TotalSale" || dr.DataType == "GrandTotalSale" || dr.DataType == "CashDrawerOpenCount" || dr.DataType == "OrderCount" || dr.DataType == "TrainingModeSale" || dr.DataType == "CategorySaleCountHeading")
                    b.AppendLine(SingleDottedLine);
                b.Append(newline);
                if (dr.DataType == "CashSum" || dr.DataType == "CashAdded" || dr.DataType == "OpenDate" || dr.DataType == "SaleTotal" || dr.DataType == "HoldCount" || dr.DataType == "ReturnVATSum" || dr.DataType == "Accounting")
                    b.AppendLine("");
                b.Append(newline);
                if (dr.DataType == "PaymentReturnCount")
                {
                    b.AppendLine(SingleDottedLine);
                    continue;
                }
                //Display data
                var row = ReportRow.FromDb(dr);
                data.Add(row);
                string str = ReportRow.FromDb(dr).ToReportString();
                b.AppendLine(str);
                b.Append(newline);
                //End display data
                if (dr.DataType == "Rounding" || dr.DataType == "CashSum" || dr.DataType == "ProductsCount" || dr.DataType == "TrainingModeSale" || dr.DataType == "Accounting")
                    b.AppendLine(SingleDottedLine);


            }



            //string query = @"select * from reportdata where reportId = '" + reportId.ToString() + "' order by SortOrder asc";
            //var tbl = DBAccess.GetData(query);
            //foreach(DataRow row in tbl.Rows)
            //{
            //    b.AppendLine(ReportRow.FromDb(row).ToReportString());
            //}
            b.AppendLine("");
            b.AppendLine(SingleDottedLine);
            b.AppendLine("-------------- SLUT ----------------");
            b.AppendLine(SingleDottedLine);
            b.Append("</body>");
            b.Append("</html>");
            return b.ToString();
        }

        private const string SingleDottedLine = "---------------------------------------------------------------------------";

        internal static ReportInfo GetRDLCZReport(string terminalId, DateTime dtFrom, DateTime dtTo, string userName, ApplicationDbContext db)
        {
            ReportInfo info = new ReportInfo();
            StringBuilder b = new StringBuilder();
            string newLine = Environment.NewLine;
            List<ReportRow> rows = new List<ReportRow>();
            b.AppendLine(SingleDottedLine);
            b.AppendLine(string.Format("{0,-42}", "---------------------------- Z  RAPPORT ----------------------------"));
            b.AppendLine(SingleDottedLine);

            b.AppendLine(string.Format("{0,-42}", "Utskriven " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " av " + userName));
            b.AppendLine(string.Format("{0,-42}", SingleDottedLine));
            info.Header = b.ToString();
            List<ReportData> reportData = new List<ReportData>();


            Guid guid = Guid.Parse(terminalId);

            var terminal = db.Terminal.FirstOrDefault(t => t.Id == guid);
            if (terminal != null)
            {
                var outlet = terminal.Outlet;
                //var outlet = outletRepo.FirstOrDefault();
                if (outlet != null)
                {
                    info.Name = outlet.Name;
                    info.Address1 = outlet.Address1;
                    info.City = outlet.City;
                    info.PostalCode = outlet.PostalCode;
                    info.Address = outlet.Address;
                    info.OrgNo = outlet.OrgNo;
                    info.Phone = outlet.Phone;
                    info.Email = outlet.Email;
                }
            }
            IDbCommand command = new SqlCommand();
            using (var conn = db.Database.Connection)
            {
                conn.Open();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_PrintDetailReportByDateRange";
                command.Parameters.Add(new SqlParameter("@TerminalId", terminalId));
                command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));

                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    reportData.Add(new ReportData
                    {
                        ReportId = Guid.Parse(dr["ReportId"].ToString()),
                        DataType = Convert.ToString(dr["DataType"]),
                        DateValue = dr["DateValue"] != DBNull.Value ? Convert.ToDateTime(dr["DateValue"]) : DateTime.Now,
                        Value = dr["Value"] != DBNull.Value ? Convert.ToDecimal(dr["Value"]) : 0,
                        SortOrder = dr["SortOrder"] != DBNull.Value ? Convert.ToInt32(dr["SortOrder"]) : 0,
                        ForeignId = dr["ForeignId"] != DBNull.Value ? Convert.ToInt32(dr["ForeignId"]) : 0,
                        TaxPercent = dr["TaxPercent"] != DBNull.Value ? Convert.ToDecimal(dr["TaxPercent"]) : 0,
                        TextValue = Convert.ToString(dr["TextValue"])
                    });
                }
                dr.Close();

            }
            var reportSortedData = reportData.OrderBy(rd => rd.SortOrder).ToList();
            List<ReportRow> data = new List<ReportRow>();
            string extraline = "";
            foreach (var dr in reportSortedData)
            {
                if (dr.DataType == "ACStandared")
                {

                }
                if (dr.DataType == "ACTable")
                {

                }
                if (dr.DataType == "TotalSale" || dr.DataType == "GrandTotalSale" || dr.DataType == "CashDrawerOpenCount" || dr.DataType == "OrderCount" || dr.DataType == "TrainingModeSale" || dr.DataType == "CategorySaleCountHeading")
                {
                    b.AppendLine(string.Format("{0,-42}", SingleDottedLine));
                    extraline = string.Format("{0,-42}", SingleDottedLine);
                }

                if (dr.DataType == "CashSum" || dr.DataType == "CashAdded" || dr.DataType == "OpenDate" || dr.DataType == "SaleTotal" || dr.DataType == "HoldCount" || dr.DataType == "ReturnVATSum" || dr.DataType == "Accounting")
                {
                    b.AppendLine(newLine);
                    extraline = string.Format("{0,-42}", newLine);
                }
                if (dr.DataType == "PaymentReturnCount")
                {
                    b.AppendLine(string.Format("{0,-42}", SingleDottedLine));
                    extraline = string.Format("{0,-42}", SingleDottedLine);
                    continue;
                }
                if (!string.IsNullOrEmpty(extraline))
                {
                    rows.Add(new ReportRow { FormatedText = extraline });
                    extraline = "";
                }
                //Display data
                var row = ReportRow.FromDb(dr);


                data.Add(row);
                string str = ReportRow.FromDb(dr).ToReportString();
                b.AppendLine(newLine);
                b.AppendLine(str);

                rows.Add(new ReportRow { DataType = row.DataType, DataTypeText = row.DataTypeText, Extra = row.Extra, Value = row.Value, SortOrder = row.SortOrder, FormatedText = row.FormatedText });

                //End display data
                if (dr.DataType == "Rounding" || dr.DataType == "CashSum" || dr.DataType == "ProductsCount" || dr.DataType == "TrainingModeSale" || dr.DataType == "Accounting")
                    b.AppendLine(string.Format("{0,-42}", SingleDottedLine));


            }

            b = new StringBuilder();
            b.AppendLine(newLine);
            b.AppendLine(string.Format("{0,-42}", SingleDottedLine));
            b.AppendLine(string.Format("{0,-42}", "-------------------------------- SLUT ----------------------------------"));
            b.AppendLine(string.Format("{0,-42}", SingleDottedLine));
            info.Footer = b.ToString();
            info.ReportRows = rows;

            return info;
        }


        internal static ReportInfo PrintDetailReportByDateRange(string terminalId, DateTime dtFrom, DateTime dtTo, string userName, ApplicationDbContext db)
        {
            ReportInfo info = new ReportInfo();
            List<ReportRow> rows = new List<ReportRow>();



            info.Header = Resource.Report_Printed + " " + DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss") + "  " + Resource.Report_PrintedOf + " " + userName;


            List<ReportData> reportData = new List<ReportData>();


            // var outletRepo = new Repository<Outlet, Guid>(uof.Session);

            Guid guid = Guid.Parse(terminalId);
            var terminal = db.Terminal.FirstOrDefault(t => t.Id == guid);
            using (var conn = db.Database.Connection)
            {
                conn.Open();
                if (terminal != null)
                {
                    var outlet = terminal.Outlet;
                    //var outlet = outletRepo.FirstOrDefault();
                    if (outlet != null)
                    {
                        info.Name = outlet.Name;
                        info.Address = outlet.Address;
                        info.OrgNo = outlet.OrgNo;
                        info.Phone = outlet.Phone;
                    }
                }
                IDbCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_PrintDetailReportByDateRange";
                command.Parameters.Add(new SqlParameter("@TerminalId", terminalId));
                command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));

                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    try
                    {
                        reportData.Add(new ReportData
                        {
                            ReportId = Guid.Parse(dr["ReportId"].ToString()),
                            DataType = Convert.ToString(dr["DataType"]),
                            DateValue = dr["DateValue"] != DBNull.Value ? Convert.ToDateTime(dr["DateValue"]) : DateTime.Now,
                            Value = dr["Value"] != DBNull.Value ? Convert.ToDecimal(dr["Value"]) : 0,
                            SortOrder = dr["SortOrder"] != DBNull.Value ? Convert.ToInt32(dr["SortOrder"]) : 0,
                            ForeignId = dr["ForeignId"] != DBNull.Value ? Convert.ToInt32(dr["ForeignId"]) : 0,
                            TaxPercent = dr["TaxPercent"] != DBNull.Value ? Convert.ToDecimal(dr["TaxPercent"]) : 0,
                            TextValue = Convert.ToString(dr["TextValue"])
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }
                dr.Close();

            }

            var depositInCash = reportData.FirstOrDefault(a => a.DataType == "Deposit In (Cash)");
            var depositInCard = reportData.FirstOrDefault(a => a.DataType == "Deposit In (Card)");

            if (depositInCash != null)
            {
                var record = reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 1);
                if (record != null)
                {
                    reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 1).Value = record.Value + depositInCash.Value;
                }
            }

            if (depositInCard != null)
            {
                var record = reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 4);
                if (record != null)
                {
                    reportData.FirstOrDefault(a => a.DataType == "PaymentTypeSale" && a.ForeignId == 4).Value = record.Value + depositInCard.Value;
                }
            }

            var reportSortedData = reportData.OrderBy(rd => rd.SortOrder).ToList();

            foreach (var dr in reportSortedData)
            {
                rows.Add(ReportRow.FromDb(dr));
            }

            info.ReportRows = rows;

            return info;
        }

        internal static ReportInfo PrintDetailReportByDateRangeForPOSMini(int reportSource, DateTime dtFrom, DateTime dtTo, string userName, ApplicationDbContext db)
        {
            ReportInfo info = new ReportInfo();
            List<ReportRow> rows = new List<ReportRow>();
            info.Header = Resource.Report_Printed + " " + DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss") + "  " + Resource.Report_PrintedOf + " " + userName;
            List<ReportData> reportData = new List<ReportData>();

            using (var conn = db.Database.Connection)
            {
                conn.Open();

                IDbCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_PrintDetailReportByDateRangePOSMini";
                command.Parameters.Add(new SqlParameter("@ReportSource", reportSource));
                command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));

                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    try
                    {
                        reportData.Add(new ReportData
                        {
                            ReportId = Guid.Parse(dr["ReportId"].ToString()),
                            DataType = Convert.ToString(dr["DataType"]),
                            DateValue = dr["DateValue"] != DBNull.Value ? Convert.ToDateTime(dr["DateValue"]) : DateTime.Now,
                            Value = dr["Value"] != DBNull.Value ? Convert.ToDecimal(dr["Value"]) : 0,
                            SortOrder = dr["SortOrder"] != DBNull.Value ? Convert.ToInt32(dr["SortOrder"]) : 0,
                            ForeignId = dr["ForeignId"] != DBNull.Value ? Convert.ToInt32(dr["ForeignId"]) : 0,
                            TaxPercent = dr["TaxPercent"] != DBNull.Value ? Convert.ToDecimal(dr["TaxPercent"]) : 0,
                            TextValue = Convert.ToString(dr["TextValue"])
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }
                dr.Close();

            }

            var reportSortedData = reportData.OrderBy(rd => rd.SortOrder).ToList();

            foreach (var dr in reportSortedData)
            {
                rows.Add(ReportRow.FromDb(dr));
            }

            info.ReportRows = rows;

            return info;
        }


        internal static ReportInfo PrintSaleByCategory(DateTime dtFrom, DateTime dtTo, int categoryId, string userName, ApplicationDbContext db)
        {
            ReportInfo info = new ReportInfo();
            List<ReportRow> rows = new List<ReportRow>();



            info.Header = Resource.Report_Printed + " " + DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss") + "  " + Resource.Report_PrintedOf + " " + userName;


            List<ReportData> reportData = new List<ReportData>();

            var category = db.Category.FirstOrDefault(c => c.Id == categoryId);

            var outlet = db.Outlet.FirstOrDefault();
            if (outlet != null)
            {
                info.Name = outlet.Name;
                info.Address = outlet.Address;
                info.OrgNo = outlet.OrgNo;
                info.Phone = outlet.Phone;
            }
            info.Description = category.Name + Resource.Report_Sale + " " + Resource.Report_From + " " + dtFrom.Year + "-" + dtFrom.Month + "-" + dtFrom.Day + " " + Resource.Report_To + " " + dtTo.Year + "-" + dtTo.Month + "-" + dtTo.Day;
            string query = @"SELECT       sum(OrderDetail.Qty) as Qty, sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) as Amount, Item.Id, Item.TextDescription,Item.Unit
            FROM            OrderMaster INNER JOIN
                                     OrderDetail ON OrderMaster.OrderId = OrderDetail.OrderId INNER JOIN
                                     Item ON OrderDetail.ItemId = Item.Id
                                      INNER JOIN ItemCategory ON OrderDetail.ItemId = ItemCategory.ItemId
                                     Where OrderMaster.CreationDate between '" + dtFrom + "' AND '" + dtTo + "' AND ItemCategory.CategoryId=" + categoryId
                    + " group by Item.Id, Item.TextDescription,Item.Unit";
            using (var conn = db.Database.Connection)
            {
                IDbCommand command = new SqlCommand();
                conn.Open();
                command.Connection = conn;
                command.CommandType = CommandType.Text;
                command.CommandText = query;


                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    reportData.Add(new ReportData
                    {
                        TextValue = Convert.ToString(dr["TextDescription"]),
                        ForeignId = dr["Unit"] != DBNull.Value ? Convert.ToInt32(dr["Unit"]) : 0,
                        Value = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0,
                        TaxPercent = dr["Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Qty"]) : 0,

                    });
                }
                dr.Close();

            }
            var reportSortedData = reportData.OrderBy(rd => rd.TextValue).ToList();

            foreach (var dr in reportSortedData)
            {
                rows.Add(new ReportRow
                {
                    DataTypeText = dr.TextValue,
                    Extra = (dr.ForeignId == 1 ? dr.TaxPercent + " " + "g" : dr.ForeignId == 2 ? dr.TaxPercent + " " + "hg" : dr.ForeignId == 3 ? dr.TaxPercent + " " + "kg" : Math.Round(Convert.ToDecimal(dr.TaxPercent), 0).ToString()),
                    Value = Math.Round(Convert.ToDecimal(dr.Value), 2).ToString()

                });
            }
            info.ReportRows = rows;

            return info;
        }


    }
    public class ReportRow
    {
        public enum ReportDataType
        {
            [LocalizedDescModelAttribute(@"Report_UniqueIdentification", typeof(Resource))]
            UniqueIdentification,
            [LocalizedDescModelAttribute(@"Report_CloseDate", typeof(Resource))]
            CloseDate,
            [LocalizedDescModelAttribute(@"Report_OpenDate", typeof(Resource))]
            OpenDate,
            [LocalizedDescModelAttribute(@"Report_CashDrawerOpen", typeof(Resource))]
            CashDrawerOpen,
            [LocalizedDescModelAttribute(@"Report_ReceiptCount", typeof(Resource))]
            ReceiptCount,
            [LocalizedDescModelAttribute(@"Report_CashDrawerOpenCount", typeof(Resource))]
            CashDrawerOpenCount,
            [LocalizedDescModelAttribute(@"Report_TotalSale", typeof(Resource))]
            TotalSale,
            [LocalizedDescModelAttribute(@"Report_Rounding", typeof(Resource))]
            Rounding,
            [LocalizedDescModelAttribute(@"Report_SaleTotal", typeof(Resource))]
            SaleTotal,
            [LocalizedDescModelAttribute(@"Report_TotalReturn", typeof(Resource))]
            TotalReturn,
            [LocalizedDescModelAttribute(@"Report_TotalNet", typeof(Resource))]
            TotalNet,
            [LocalizedDescModelAttribute(@"Report_VATSum", typeof(Resource))]
            VATSum,
            [LocalizedDescModelAttribute(@"Report_VATPercent", typeof(Resource))]
            VATPercent,
            [LocalizedDescModelAttribute(@"Report_ReturnVATSum", typeof(Resource))]
            ReturnVATSum,
            [LocalizedDescModelAttribute(@"Report_ReturnVATPercent", typeof(Resource))]
            ReturnVATPercent,
            [LocalizedDescModelAttribute(@"Report_GrandTotalSale", typeof(Resource))]
            GrandTotalSale,
            [LocalizedDescModelAttribute(@"Report_GrandTotalReturn", typeof(Resource))]
            GrandTotalReturn,
            [LocalizedDescModelAttribute(@"Report_GrandTotalNet", typeof(Resource))]
            GrandTotalNet,
            [LocalizedDescModelAttribute(@"Report_OrderCount", typeof(Resource))]
            OrderCount,
            [LocalizedDescModelAttribute(@"Report_ReturnCount", typeof(Resource))]
            ReturnCount,
            [LocalizedDescModelAttribute(@"Report_HoldCount", typeof(Resource))]
            HoldCount,
            [LocalizedDescModelAttribute(@"Report_HoldTotalSale", typeof(Resource))]
            HoldTotalSale,
            [LocalizedDescModelAttribute(@"Report_PaymentTypeSale", typeof(Resource))]
            PaymentTypeSale,
            [LocalizedDescModelAttribute(@"Report_ReportNumber", typeof(Resource))]
            ReportNumber,
            [LocalizedDescModelAttribute(@"Report_ProductsCount", typeof(Resource))]
            ProductsCount,
            [LocalizedDescModelAttribute(@"Report_ServicesCount", typeof(Resource))]
            ServicesCount,
            [LocalizedDescModelAttribute(@"Report_DiscountCount", typeof(Resource))]
            DiscountCount,
            [LocalizedDescModelAttribute(@"Report_TrainingModeCount", typeof(Resource))]
            TrainingModeCount,
            [LocalizedDescModelAttribute(@"Report_TrainingModeSale", typeof(Resource))]
            TrainingModeSale,
            [LocalizedDescModelAttribute(@"Report_CategorySaleHeading", typeof(Resource))]
            CategorySaleHeading,
            [LocalizedDescModelAttribute(@"Report_CategorySale", typeof(Resource))]
            CategorySale,
            [LocalizedDescModelAttribute(@"Report_CategorySaleCountHeading", typeof(Resource))]
            CategorySaleCountHeading,
            [LocalizedDescModelAttribute(@"Report_CategorySaleCount", typeof(Resource))]
            CategorySaleCount,
            [LocalizedDescModelAttribute(@"Report_PaymentTypeNet", typeof(Resource))]
            PaymentTypeNet,
            [LocalizedDescModelAttribute(@"Report_PaymentTypeReturn", typeof(Resource))]
            PaymentTypeReturn,
            [LocalizedDescModelAttribute(@"Report_ReceiptCopyCount", typeof(Resource))]
            ReceiptCopyCount,
            [LocalizedDescModelAttribute(@"Report_ReceiptCopyAmount", typeof(Resource))]
            ReceiptCopyAmount,
            [LocalizedDescModelAttribute(@"Report_CashAdded", typeof(Resource))]
            CashAdded,
            [LocalizedDescModelAttribute(@"Report_CashDropped", typeof(Resource))]
            CashDropped,
            [LocalizedDescModelAttribute(@"Report_CashIn", typeof(Resource))]
            CashIn,
            [LocalizedDescModelAttribute(@"Report_CashOut", typeof(Resource))]
            CashOut,
            [LocalizedDescModelAttribute(@"Report_CashSum", typeof(Resource))]
            CashSum,
            [LocalizedDescModelAttribute(@"Report_OtherNegativeRegistrations", typeof(Resource))]
            OtherNegativeRegistrations,
            Accounting,
            ACTotal,
            ACNetTotal,
            ACStandared,
            ACTakeaway,
            ACTable,
            ACVatSum,
            ACVATPercent,
            None

        }

        internal string ToReportString()
        {


            if (string.IsNullOrEmpty(Extra))
            {
                int textLength = DataTypeText.Length;
                int valuelength = 0;
                if (Value != null)
                {
                    valuelength = Value.Length;
                }

                int whiteSapceLength = 42 - textLength - valuelength;
                //return string.Format("{0," + textLength + "}{1," + whiteSapceLength + "}{2," + valuelength + "}", DataTypeText, Extra ?? "", Value ?? "");
                return string.Format("{0,-32} {1,10}", DataTypeText, Value ?? " ");
            }
            else
            {
                if (DataTypeText.Length > 18)
                    DataTypeText = DataTypeText.Substring(0, 18) + ".";
                return string.Format("{0,-20} {1,-12} {2,10}", DataTypeText, Extra ?? "", Value ?? "");
            }
        }

        internal static ReportRow FromDb(ReportData row)
        {
            ReportRow rRow = new ReportRow();
            rRow.DataTypeText = row.DataType;// Convert.ToString(row["DataType"]);
            rRow.SortOrder = row.SortOrder;
            try
            {
                if (row.TextValue == "DepositType")
                {
                    rRow.DataType = ReportDataType.None;
                }
                else
                {
                    rRow.DataType = (ReportDataType)Enum.Parse(typeof(ReportDataType), Convert.ToString(row.DataType));
                }
            }
            catch (Exception ex)
            {
                // LogWriter.LogWrite(ex);
                rRow.DataType = ReportDataType.None;
            }
            switch (rRow.DataType)
            {
                case ReportDataType.CloseDate:
                    if (row.DateValue == null)
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = Convert.ToDateTime(row.DateValue).ToString("yyyy-MM-dd HH:mm");
                    }
                    rRow.Value = "";
                    rRow.DataTypeText = Resource.Report_CloseDate;
                    break;
                case ReportDataType.OpenDate:
                    if (row.DateValue == null)
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = Convert.ToDateTime(row.DateValue).ToString("yyyy-MM-dd HH:mm");
                    }
                    rRow.Value = "";
                    rRow.DataTypeText = Resource.Report_OpenDate;
                    break;
                case ReportDataType.CashDrawerOpenCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CashDrawerOpenCount;
                    break;
                case ReportDataType.HoldCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_HoldCount;
                    break;
                case ReportDataType.TrainingModeCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_TrainingModeCount;
                    break;
                case ReportDataType.OrderCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_OrderCount;
                    break;
                case ReportDataType.ReturnCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_ReturnCount;
                    break;
                case ReportDataType.ReceiptCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_ReceiptCount;
                    break;
                case ReportDataType.ProductsCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_ProductsCount;
                    break;
                case ReportDataType.ServicesCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_ServicesCount;
                    break;
                case ReportDataType.DiscountCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_DiscountCount;
                    break;
                case ReportDataType.ReceiptCopyCount:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_ReceiptCopyCount;
                    break;
                case ReportDataType.CashDrawerOpen:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CashDrawerOpen;
                    break;
                case ReportDataType.HoldTotalSale:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_HoldTotalSale;
                    break;
                case ReportDataType.TrainingModeSale:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = "TrainingModeSale";
                    break;
                case ReportDataType.TotalNet:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_TotalNet;
                    break;
                case ReportDataType.TotalReturn:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_TotalReturn;
                    break;
                case ReportDataType.TotalSale:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_TotalSale;
                    break;
                case ReportDataType.GrandTotalNet:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_GrandTotalNet;
                    break;
                case ReportDataType.GrandTotalReturn:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_GrandTotalReturn;
                    break;
                case ReportDataType.GrandTotalSale:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_GrandTotalSale;
                    break;
                case ReportDataType.SaleTotal:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_SaleTotal;
                    break;
                case ReportDataType.ReportNumber:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_ReportNumber;
                    break;
                case ReportDataType.VATPercent:
                    rRow.Extra = row.TaxPercent.ToString();
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                    }
                    if (string.IsNullOrEmpty(row.TextValue)) //to print Percentage
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = row.TaxPercent.ToString();
                    }
                    rRow.DataTypeText = Resource.Report_VATPercent;
                    break;
                case ReportDataType.VATSum:
                    if (row.TaxPercent == 0)
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = row.TaxPercent.ToString();
                    }
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToString(row.Value);// Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_VATSum;
                    break;
                case ReportDataType.ReturnVATSum:
                    if (row.TaxPercent == 0)
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = row.TaxPercent.ToString();
                    }
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_ReturnVATSum;
                    break;

                case ReportDataType.ReturnVATPercent:
                    rRow.Extra = row.TaxPercent.ToString();
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                    }
                    if (string.IsNullOrEmpty(row.TextValue)) //to print Percentage
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = row.TaxPercent.ToString();
                    }
                    rRow.DataTypeText = Resource.Report_VATPercent;
                    break;
                case ReportDataType.CategorySaleHeading:
                    // rRow.Extra = row.TaxPercent.ToString();
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = "";// Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CategorySaleHeading;
                    break;
                case ReportDataType.Rounding:
                    rRow.Extra = row.TaxPercent.ToString();
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                    }
                    if (string.IsNullOrEmpty(row.TextValue)) //to print Percentage
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = row.TaxPercent.ToString();
                    }
                    rRow.DataTypeText = Resource.Report_Rounding;
                    break;
                case ReportDataType.CategorySale:
                    // rRow.Extra = row.TaxPercent.ToString();
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    if (string.IsNullOrEmpty(row.TextValue)) //to print category name
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        //   rRow.Extra = row.TextValue;
                    }
                    rRow.DataTypeText = "   " + row.TextValue;// Resource.Report_CategorySale;
                    break;
                case ReportDataType.CategorySaleCountHeading:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CategorySaleCountHeading;
                    break;
                case ReportDataType.CategorySaleCount:
                    // rRow.Extra = row.TaxPercent.ToString();
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToInt32(row.Value).ToString();
                    }
                    //if (string.IsNullOrEmpty(row.TextValue)) //to print category name
                    //{
                    //    rRow.Extra = "";
                    //}
                    //else
                    //{
                    //    rRow.Extra = row.TextValue;
                    //}
                    rRow.DataTypeText = "   " + row.TextValue;// Resource.Report_CategorySaleCount;
                    break;
                case ReportDataType.PaymentTypeSale:
                    if (row.ForeignId == 0)
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = GetPaymentType(Convert.ToInt32(row.ForeignId));
                    }
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_PaymentTypeSale;
                    break;
                case ReportDataType.PaymentTypeReturn:
                    if (row.ForeignId == 0)
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = GetPaymentType(Convert.ToInt32(row.ForeignId));
                    }
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_PaymentTypeReturn;
                    break;
                case ReportDataType.PaymentTypeNet:
                    if (row.ForeignId == 0)
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = GetPaymentType(Convert.ToInt32(row.ForeignId));
                    }
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_Payment;
                    break;
                case ReportDataType.UniqueIdentification:
                    if (string.IsNullOrEmpty(row.TextValue))
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = row.TextValue;
                    }
                    rRow.DataTypeText = Resource.Report_UniqueIdentification;
                    break;
                case ReportDataType.CashAdded:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CashAdded;
                    break;
                case ReportDataType.CashDropped:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CashDropped;
                    break;
                case ReportDataType.CashIn:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CashIn;
                    break;
                case ReportDataType.CashOut:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CashOut;
                    break;

                case ReportDataType.CashSum:
                    if (row.Value == 0)
                    {
                        rRow.Value = "0";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_CashSum;
                    break;

                case ReportDataType.OtherNegativeRegistrations:
                    if (row.TextValue == "")
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = row.TextValue;
                    }
                    if (row.Value == 0)
                    {
                        rRow.Value = "0,00";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString("N", new CultureInfo("sv-SE")).Trim();
                    }
                    rRow.DataTypeText = Resource.Report_OtherNegativeRegistrations;
                    break;
                //Accounting Tags
                case ReportDataType.Accounting:
                    rRow.Extra = "";

                    rRow.Value = "";

                    rRow.DataTypeText = Resource.Accounting;// "Accounting";
                    break;
                case ReportDataType.ACTotal:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = (row.TextValue.Length > 24 ? row.TextValue.Substring(0, 24) : row.TextValue) + " tot";
                    break;
                case ReportDataType.ACNetTotal:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = (row.TextValue.Length > 24 ? row.TextValue.Substring(0, 24) : row.TextValue) + " net";
                    break;
                case ReportDataType.ACStandared:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = "Standared";
                    break;
                case ReportDataType.ACTable:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = "Table";
                    break;
                case ReportDataType.ACTakeaway:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = "Takeaway";
                    break;
                case ReportDataType.ACVatSum:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    rRow.DataTypeText = Resource.Report_VAT;
                    break;
                case ReportDataType.ACVATPercent:
                    rRow.Extra = "";
                    if (row.Value == 0)
                    {
                        rRow.Value = "";
                    }
                    else
                    {
                        rRow.Value = Convert.ToDecimal(row.Value).ToString();
                    }
                    if (string.IsNullOrEmpty(row.TextValue)) //to print Percentage
                    {
                        rRow.Extra = "";
                    }
                    else
                    {
                        rRow.Extra = row.TaxPercent.ToString();
                    }
                    rRow.DataTypeText = Resource.Report_VATPercent;
                    break;
                default:
                    {
                        rRow.Extra = "";
                        if (row.Value == 0)
                        {
                            if (row.DataType == "Loaded Deposit"
                                || row.DataType == "PaymentReturnCount"
                                || row.DataType == "CategorySaleHeading"
                                || row.DataType == "CategorySaleCountHeading"
                                || row.DataType == "Accounting"
                                || row.DataType == "Subtotal deponering"
                                )
                            {
                                rRow.Value = "";
                            }
                        }
                        else
                        {
                            rRow.Value = Convert.ToDecimal(row.Value).ToString("C", new CultureInfo("sv-SE")).Trim();
                        }

                        rRow.DataTypeText = row.DataType;
                    }
                    break;
            }

            return rRow;
        }


        private static string GetPaymentType(int p)
        {
            switch (p)
            {
                case 0: return Resource.Payment_Method_FreeCoupon;// "Free Coupon";
                case 1: return Resource.Payment_Method_Cash;// "Kontant";
                case 2: return Resource.Payment_Method_Account;//"Faktura";
                case 3: return "Presentkort";//"Presentkort";
                case 4: return Resource.Payment_Method_CreditCard;// "Kort";
                case 5: return "Bankkort";
                case 6: return "Check";
                case 7: return Resource.Payment_Method_CashBack;//"Utbetalning"; 
                case 8: return Resource.Payment_Method_Return;//"Retur";
                case 9: return Resource.Payment_Method_Mobile;//"Retur";
                case 10: return "Swish";
                case 11: return "Elvekort";
                case 16: return "Deposit";
            }
            return Resource.Other;// "Annan";
        }

        public ReportDataType DataType { get; set; }

        public string Value { get; set; }

        public string Extra { get; set; }

        public string DataTypeText { get; set; }

        public int SortOrder { get; set; }
        public static object EnumToNameConverter { get; private set; }
        public string FormatedText { get; set; }
    }


}