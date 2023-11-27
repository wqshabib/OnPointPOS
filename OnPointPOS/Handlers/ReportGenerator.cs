using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Res;
using Newtonsoft.Json;

namespace POSSUM.Handlers
{
    public class ReportGenerator
    {
        private const string SingleDottedLine = "------------------------------------";

        internal static List<EmployeeLog> PrintEmployeeLog(DateTime dtFrom, DateTime dtTo)
        {
            try
            {
                return new EmployeeRepository(PosState.GetInstance().Context).PrintEmployeeLog(dtFrom, dtTo);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new List<EmployeeLog>();
            }
        }

        internal static List<Journal> PrintJournalReport(DateTime dtFrom, DateTime dtTo)
        {
            try
            {

                var journalLogs = new ReportRepository().PrintJournalReport(dtFrom,dtTo);               
                foreach (var log in journalLogs)
                {
                    if (log.Action == "ReceiptPrinted")
                    {
                        string[] tokens = log.LogMessage.Split('|');
                        if (tokens.Length == 3)
                        {
                            string text = tokens[0];
                            string items = tokens[1];
                            string vats = tokens[2];
                            var vatDetails = JsonConvert.DeserializeObject<List<VAT>>(vats);
                            StringBuilder b = new StringBuilder();
                            b.AppendLine(text);
                            b.AppendLine("---------------------------------------");
                            string[] item = items.Split('~');
                            foreach (var itm in item)
                            {
                                b.AppendLine(itm);
                            }
                            b.AppendLine("---------------------------------------");
                            string vatstring = UI.Global_VAT;
                            string total = UI.Global_Total;
                            string s = string.Format("{0,-20} {1,5} {2,-8}", vatstring + "%", " ",
                                vatstring + " " + total);
                            //string heading = String.Format("{0,-20}{ 1,10}{ 2,-8}",vatstring," h", vatstring);
                            b.AppendLine(s);
                            // b.AppendLine(vatstring + "%           " + vatstring + " " + total);
                            foreach (var vat in vatDetails)
                            {
                                string vatData = string.Format("{0,-20} {1,10} {2,-8}", vat.VATPercent, " ",
                                    Math.Round(vat.VATTotal, 2));
                                // string data = string.Format("{ 0,-20}{ 1,10}{ 2,-8}", vat.VATPercent," ", Math.Round(vat.VATTotal, 2));
                                // b.AppendLine(vat.VATPercent + "                    " + Math.Round(vat.VATTotal, 2));
                                b.AppendLine(vatData);
                            }
                            log.LogMessage = b.ToString();
                        }
                    }
                    
                }
                return journalLogs;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new List<Journal>();
            }
        }

        internal static List<Report> LoadZReports(DateTime dtFrom, DateTime dtTo)
    {
            try
            {
                var reports = new List<Report>();
                //::TODO need to create map for store procedure "GenerateReportByTerminal" in nhibernate
                using (var db = new ApplicationDbContext())
                {

                    return db.Report.Where(r => r.CreationDate >= dtFrom && r.CreationDate <= dtTo && r.ReportType == 1 && r.TerminalId==Defaults.Terminal.Id).OrderByDescending(o => o.ReportNumber).ToList();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new List<Report>();
            }
        }

        public static Guid GenerateReport(Guid terminalId, int reportType)
        {
            try
            {
                return new ReportRepository().GenerateReport(terminalId, reportType);
                
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                throw;
            }
        }

        internal static string GetReport(Guid reportId, int reportType, string userId)
        {
            var b = new StringBuilder();

            if (reportType == 1)
            {
                b.AppendLine(SingleDottedLine);
                b.AppendLine("---------- Z  RAPPORT --------------");
                if (Defaults.User.TrainingMode)
                    b.AppendLine("------------ " + UI.Report_Trainingmode + " -------------");
                b.AppendLine(SingleDottedLine);
                b.AppendLine("Utskriven " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " av " + userId);
                b.AppendLine(SingleDottedLine);
            }
            else
            {
                b.AppendLine(SingleDottedLine);
                b.AppendLine("----------- X  RAPPORT -------------");
                if (Defaults.User.TrainingMode)
                    b.AppendLine("------------ " + UI.Report_Trainingmode + " -------------");
                b.AppendLine(SingleDottedLine);
                b.AppendLine("Utskriven " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " av " + userId);
                b.AppendLine(SingleDottedLine);
            }
            b.AppendLine("");
            var data = new List<ReportRow>();
            var reportData = new ReportRepository().GetReport(reportId);
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
            var mob = reportData.Where(c => c.DataType.Contains("mobile")).SingleOrDefault();
            if (reportData!=null)
            {
                foreach (var dr in reportData)
                {
                    try
                    {
                        if (dr.DataType == "TipTypeSale" && Defaults.SaleType == SaleType.Retail)
                        {
                            continue;
                        }

                        if (dr.DataType == "ACStandared")
                        {
                        }
                        if (dr.DataType == "ACTable")
                        {
                        }
                        if (dr.DataType == "TotalSale" || dr.DataType == "GrandTotalSale" ||
                            dr.DataType == "CashDrawerOpenCount" || dr.DataType == "OrderCount" ||
                            dr.DataType == "TrainingModeSale" || dr.DataType == "CategorySaleCountHeading"
                             )
                        {
                            b.AppendLine(SingleDottedLine);
                        }

                        if (dr.DataType == "TipTypeSale" && Defaults.SaleType == SaleType.Restaurant)
                        {
                            b.AppendLine(SingleDottedLine);
                        }

                        if (dr.DataType == "CashSum" || dr.DataType == "CashAdded" || dr.DataType == "OpenDate" ||
                            dr.DataType == "SaleTotal" || dr.DataType == "HoldCount" || dr.DataType == "ReturnVATSum" ||
                            dr.DataType == "Accounting")
                            b.AppendLine("");
                        if (dr.DataType == "PaymentReturnCount" || dr.DataType == "LoadedDepositEnd")
                        {
                            b.AppendLine(SingleDottedLine);
                            continue;
                        }
                        //Display data
                        var row = ReportRow.FromDb(dr);
                        data.Add(row);
                        string str = ReportRow.FromDb(dr).ToReportString();
                        b.AppendLine(str);

                        //End display data
                        if (dr.DataType == "Rounding" || dr.DataType == "LOTTER" || dr.DataType == "CashSum" || dr.DataType == "ProductsCount" ||
                            dr.DataType == "TrainingModeSale" || dr.DataType == "Accounting"
                            || dr.DataType == "Deposit Sum")
                            b.AppendLine(SingleDottedLine);

                        if (dr.DataType == "TipTypeSale" && Defaults.SaleType == SaleType.Restaurant)
                        {
                            b.AppendLine(SingleDottedLine);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
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

            return b.ToString();
        }

        internal List<ReportData> PrintDetailReportByDateRange(string terminalId, DateTime dtFrom, DateTime dtTo)
        {
            List<ReportData> reportData = new List<ReportData>();
            using (var db = new ApplicationDbContext())
            {
                //Guid guid = Guid.Parse(terminalId);
                //var terminal = db.Terminal.FirstOrDefault(t => t.Id == guid);
                using (var conn = db.Database.Connection)
                {
                    //conn.Open();
                    //if (terminal != null)
                    //{
                    //    var outlet = terminal.Outlet;
                    //    //var outlet = outletRepo.FirstOrDefault();
                    //    if (outlet != null)
                    //    {
                    //        //info.Name = outlet.Name;
                    //        //info.Address = outlet.Address;
                    //        //info.OrgNo = outlet.OrgNo;
                    //        //info.Phone = outlet.Phone;
                    //    }
                    //}

                    IDbCommand command = new SqlCommand();

                    conn.Open();
                    command.Connection = conn;
                    command.CommandTimeout = 180;
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
            }

            return reportData;
        }

        internal string GetReportByDateRange(string terminalId, DateTime dtFrom, DateTime dtTo, string userName)
        {
            var b = new StringBuilder();

            b.AppendLine(SingleDottedLine);
                b.AppendLine("---------- Z RAPPORT --------------");
                if (Defaults.User.TrainingMode)
                    b.AppendLine("------------ " + UI.Report_Trainingmode + " -------------");
                b.AppendLine(SingleDottedLine);
                b.AppendLine("Utskriven " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " av " + userName);
                b.AppendLine(SingleDottedLine);

            b.AppendLine("");
            var data = new List<ReportRow>();
            var reportData = PrintDetailReportByDateRange(terminalId, dtFrom, dtTo);
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
            if (reportData != null)
            {
                foreach (var dr in reportData)
                {
                    try
                    {
                        if (dr.DataType == "TipTypeSale" && Defaults.SaleType == SaleType.Retail)
                        {
                            continue;
                        }

                        if (dr.DataType == "ACStandared")
                        {
                        }
                        if (dr.DataType == "ACTable")
                        {
                        }
                        if (dr.DataType == "TotalSale" || dr.DataType == "GrandTotalSale" ||
                            dr.DataType == "CashDrawerOpenCount" || dr.DataType == "OrderCount" ||
                            dr.DataType == "TrainingModeSale" || dr.DataType == "CategorySaleCountHeading"
                             )
                        {
                            b.AppendLine(SingleDottedLine);
                        }

                        if (dr.DataType == "TipTypeSale" && Defaults.SaleType == SaleType.Restaurant)
                        {
                            b.AppendLine(SingleDottedLine);
                        }

                        if (dr.DataType == "CashSum" || dr.DataType == "CashAdded" || dr.DataType == "OpenDate" ||
                            dr.DataType == "SaleTotal" || dr.DataType == "HoldCount" || dr.DataType == "ReturnVATSum" ||
                            dr.DataType == "Accounting")
                            b.AppendLine("");
                        if (dr.DataType == "PaymentReturnCount" || dr.DataType == "LoadedDepositEnd")
                        {
                            b.AppendLine(SingleDottedLine);
                            continue;
                        }
                        //Display data
                        var row = ReportRow.FromDb(dr);
                        data.Add(row);
                        string str = ReportRow.FromDb(dr).ToReportString();
                        b.AppendLine(str);

                        //End display data
                        if (dr.DataType == "Rounding" || dr.DataType == "LOTTER" || dr.DataType == "CashSum" || dr.DataType == "ProductsCount" ||
                            dr.DataType == "TrainingModeSale" || dr.DataType == "Accounting"
                            || dr.DataType == "Deposit Sum")
                            b.AppendLine(SingleDottedLine);

                        if (dr.DataType == "TipTypeSale" && Defaults.SaleType == SaleType.Restaurant)
                        {
                            b.AppendLine(SingleDottedLine);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            b.AppendLine("");
            b.AppendLine(SingleDottedLine);
            b.AppendLine("-------------- SLUT ----------------");
            b.AppendLine(SingleDottedLine);

            return b.ToString();
        }

        internal static string GetUserReport(Guid terminalId,DateTime dtFrom,DateTime dtTo, string userId)
        {
            var b = new StringBuilder();

          
                b.AppendLine(SingleDottedLine);
                b.AppendLine(string.Format( "------{0} av {1} --------", UI.Global_Sale,Defaults.User.UserName));
                if (Defaults.User.TrainingMode)
                    b.AppendLine("------------ " + UI.Report_Trainingmode + " -------------");
                b.AppendLine(SingleDottedLine);
                b.AppendLine("Utskriven " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " av " + userId);
                b.AppendLine(SingleDottedLine);
            
            b.AppendLine("");
            var data = new List<ReportRow>();
            var reportData = new ReportRepository().GetUserReport(terminalId,dtFrom,dtTo,userId);
            if (reportData != null)
            {


                foreach (var dr in reportData)
                {
                    if (dr.DataType == "ACStandared")
                    {
                    }
                    if (dr.DataType == "ACTable")
                    {
                    }
                    if (dr.DataType == "TotalSale" || dr.DataType == "GrandTotalSale" ||
                        dr.DataType == "CashDrawerOpenCount" || dr.DataType == "OrderCount" ||
                        dr.DataType == "TrainingModeSale" || dr.DataType == "CategorySaleCountHeading")
                        b.AppendLine(SingleDottedLine);

                    if (dr.DataType == "CashSum" || dr.DataType == "CashAdded" || dr.DataType == "OpenDate" ||
                        dr.DataType == "SaleTotal" || dr.DataType == "HoldCount" || dr.DataType == "ReturnVATSum" ||
                        dr.DataType == "Accounting")
                        b.AppendLine("");
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

                    //End display data
                    if (dr.DataType == "Rounding" || dr.DataType == "CashSum" || dr.DataType == "ProductsCount" ||
                        dr.DataType == "TrainingModeSale" || dr.DataType == "Accounting")
                        b.AppendLine(SingleDottedLine);
                }
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

            return b.ToString();
        }

        private class ReportRow
        {
            private ReportDataType DataType { get; set; }

            private string Value { get; set; }

            private string Extra { get; set; }

            private string DataTypeText { get; set; }
            private int SortOrder { get; set; }

            internal string ToReportString()
            {
                if (DataTypeText.Length > 28)
                    DataTypeText = DataTypeText.Substring(0, 26) + ".";
                if (string.IsNullOrEmpty(Extra))
                {
                    if (string.IsNullOrEmpty(Value))
                        Value = "";
                    int textLength = DataTypeText.Length;
                    int valuelength = Value.Length;

                    int whiteSapceLength = 36 - textLength - valuelength;
                    return string.Format("{0," + textLength + "}{1," + whiteSapceLength + "}{2," + valuelength + "}",
                        DataTypeText, Extra ?? "", Value ?? "");
                    // return string.Format("{0,-19}{1,-10}{2,7}", EnumToNameConverter.Convert(DataType), Extra ?? "", Value ?? "");
                }
                return string.Format("{0,-19}{1,-7}{2,10}", DataTypeText, Extra ?? "", Value ?? "");
            }

            internal static ReportRow FromDb(ReportData row)
            {
                var rRow = new ReportRow { DataTypeText = row.DataType, SortOrder = row.SortOrder };
                // Convert.ToString(row["DataType"]);
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
                        rRow.Extra = row.DateValue == null
                            ? ""
                            : Convert.ToDateTime(row.DateValue).ToString("yyyy-MM-dd HH:mm");
                        rRow.Value = "";
                        rRow.DataTypeText = UI.Report_CloseDate;
                        break;
                    case ReportDataType.OpenDate:
                        rRow.Extra = row.DateValue == null
                            ? ""
                            : Convert.ToDateTime(row.DateValue).ToString("yyyy-MM-dd HH:mm");
                        rRow.Value = "";
                        rRow.DataTypeText = UI.Report_OpenDate;
                        break;
                    case ReportDataType.CashDrawerOpenCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CashDrawerOpenCount;
                        break;
                    case ReportDataType.HoldCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_HoldCount;
                        break;
                    case ReportDataType.TrainingModeCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_TrainingModeCount;
                        break;
                    case ReportDataType.OrderCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_OrderCount;
                        break;
                    case ReportDataType.ReturnCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_ReturnCount;
                        break;
                    case ReportDataType.ReceiptCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_ReceiptCount;
                        break;
                    case ReportDataType.ProductsCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_ProductsCount;
                        break;
                    case ReportDataType.ServicesCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_ServicesCount;
                        break;
                    case ReportDataType.Discount:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Global_DiscountAmount;
                        break;
                    case ReportDataType.DiscountCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_DiscountCount;
                        break;
                    case ReportDataType.ReceiptCopyCount:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_ReceiptCopyCount;
                        break;
                    case ReportDataType.CashDrawerOpen:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CashDrawerOpen;
                        break;
                    case ReportDataType.HoldTotalSale:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_HoldTotalSale;
                        break;
                    case ReportDataType.TrainingModeSale:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_TrainingModeSale;
                        break;
                    case ReportDataType.TotalNet:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_TotalNet;
                        break;
                    case ReportDataType.TotalReturn:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_TotalReturn;
                        break;
                    case ReportDataType.TotalSale:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_TotalSale;
                        break;
                    case ReportDataType.GrandTotalNet:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_GrandTotalNet;
                        break;
                    case ReportDataType.GrandTotalReturn:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_GrandTotalReturn;
                        break;
                    case ReportDataType.GrandTotalSale:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_GrandTotalSale;
                        break;
                    case ReportDataType.SaleTotal:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_SaleTotal;
                        break;
                    case ReportDataType.ReportNumber:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_ReportNumber;
                        break;
                    case ReportDataType.VATPercent:
                        rRow.Extra = row.TaxPercent.ToString();
                        rRow.Value = row.Value == 0 ? "0" : Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                        rRow.Extra = string.IsNullOrEmpty(row.TextValue) ? "" : row.TaxPercent.ToString();
                        rRow.DataTypeText = UI.Report_VATPercent;
                        break;
                    case ReportDataType.VATSum:
                        rRow.Extra = row.TaxPercent == 0 ? "" : row.TaxPercent.ToString();
                        rRow.Value = row.Value == 0 ? "0" : Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                        rRow.DataTypeText = UI.Report_VATSum;
                        break;
                    case ReportDataType.ReturnVATSum:
                        rRow.Extra = row.TaxPercent == 0 ? "" : row.TaxPercent.ToString();
                        rRow.Value = row.Value == 0 ? "0" : Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                        rRow.DataTypeText = UI.Report_ReturnVATSum;
                        break;

                    case ReportDataType.ReturnVATPercent:
                        rRow.Extra = row.TaxPercent.ToString();
                        rRow.Value = row.Value == 0 ? "0" : Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                        rRow.Extra = string.IsNullOrEmpty(row.TextValue) ? "" : row.TaxPercent.ToString();
                        rRow.DataTypeText = UI.Report_VATPercent;
                        break;
                    case ReportDataType.CategorySaleHeading:
                        // rRow.Extra = row.TaxPercent.ToString();
                        rRow.Value = "";
                        rRow.DataTypeText = UI.Report_CategorySaleHeading;
                        break;
                    case ReportDataType.Rounding:
                        rRow.Extra = row.TaxPercent.ToString();
                        rRow.Value = row.Value == 0 ? "0" : Math.Round(Convert.ToDecimal(row.Value), 2).ToString();
                        rRow.Extra = string.IsNullOrEmpty(row.TextValue) ? "" : row.TaxPercent.ToString();
                        rRow.DataTypeText = UI.Report_Rounding;
                        break;
                    case ReportDataType.CategorySale:
                        // rRow.Extra = row.TaxPercent.ToString();
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        if (string.IsNullOrEmpty(row.TextValue)) //to print category name
                        {
                            rRow.Extra = "";
                        }
                        rRow.DataTypeText = "   " + row.TextValue; // UI.Report_CategorySale;
                        break;
                    case ReportDataType.CategorySaleCountHeading:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToInt32(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CategorySaleCountHeading;
                        break;
                    case ReportDataType.CategorySaleCount:
                        // rRow.Extra = row.TaxPercent.ToString();
                        rRow.Value = row.Value == 0 ? "" : Convert.ToInt32(row.Value).ToString();
                        //if (string.IsNullOrEmpty(row.TextValue)) //to print category name
                        //{
                        //    rRow.Extra = "";
                        //}
                        //else
                        //{
                        //    rRow.Extra = row.TextValue;
                        //}
                        rRow.DataTypeText = "   " + row.TextValue; // UI.Report_CategorySaleCount;
                        break;
                    case ReportDataType.PaymentTypeSale:
                        rRow.Extra = row.ForeignId == 0 ? "" : GetPaymentType(Convert.ToInt32(row.ForeignId));
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_PaymentTypeSale;
                        break;
                    case ReportDataType.TipTypeSale:
                        rRow.Extra = row.ForeignId == 0 ? "" : GetPaymentType(Convert.ToInt32(row.ForeignId));
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_TipTypeSale;
                        break;
                    case ReportDataType.PaymentTypeReturn:
                        rRow.Extra = row.ForeignId == 0 ? "" : GetPaymentType(Convert.ToInt32(row.ForeignId));
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_PaymentTypeReturn;
                        break;
                    case ReportDataType.PaymentTypeNet:
                        rRow.Extra = row.ForeignId == 0 ? "" : GetPaymentType(Convert.ToInt32(row.ForeignId));
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_Payment;
                        break;
                    case ReportDataType.UniqueIdentification:
                        rRow.Value = string.IsNullOrEmpty(row.TextValue) ? "" : row.TextValue;
                        rRow.DataTypeText = UI.Report_UniqueIdentification;
                        break;
                    case ReportDataType.CashAdded:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CashAdded;
                        break;
                    case ReportDataType.CashDropped:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CashDropped;
                        break;
                    case ReportDataType.CashIn:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CashIn;
                        break;
                    case ReportDataType.CashOut:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CashOut;
                        break;

                    case ReportDataType.CashSum:
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Report_CashSum;
                        break;

                    case ReportDataType.OtherNegativeRegistrations:
                        rRow.Extra = row.TextValue == "" ? "" : row.TextValue;
                        if (row.Value == 0)
                        {
                            rRow.Value = "0,00";
                        }
                        else
                        {
                            rRow.Value =
                                Convert.ToDecimal(row.Value)
                                    .ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)
                                    .Trim();
                        }
                        rRow.DataTypeText = UI.Report_OtherNegativeRegistrations;
                        break;
                    //Accounting Tags
                    case ReportDataType.Accounting:
                        rRow.Extra = "";

                        rRow.Value = "";

                        rRow.DataTypeText = UI.Report_Accounting; // "Accounting";
                        break;
                    case ReportDataType.ACTotal:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = (row.TextValue.Length > 24 ? row.TextValue.Substring(0, 24) : row.TextValue) +
                                            " tot";
                        break;
                    case ReportDataType.ACNetTotal:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = (row.TextValue.Length > 24 ? row.TextValue.Substring(0, 24) : row.TextValue) +
                                            " net";
                        break;
                    case ReportDataType.ACStandared:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Global_Standard;// "Standard";
                        break;
                    case ReportDataType.ACTable:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.OpenOrder_TableButton;
                        break;
                    case ReportDataType.ACTakeaway:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Sales_TakeAwayButton;
                        break;
                    case ReportDataType.ACVatSum:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.DataTypeText = UI.Global_VAT;
                        break;
                    case ReportDataType.ACVATPercent:
                        rRow.Extra = "";
                        rRow.Value = row.Value == 0 ? "0" : Convert.ToDecimal(row.Value).ToString();
                        rRow.Extra = string.IsNullOrEmpty(row.TextValue) ? "" : row.TaxPercent.ToString();
                        rRow.DataTypeText = UI.Report_VATPercent;
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
                                rRow.Value =
                                    Convert.ToDecimal(row.Value)
                                        .ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)
                                        .Trim();
                            }
                            rRow.DataTypeText = EnumToNameConverter.Convert(row.DataType);
                        }
                        break;
                }

                return rRow;
            }

            private static string GetPaymentType(int p)
            {
                //var type = Defaults.PaymentTypes.FirstOrDefault(pt => pt.Id == p);
                switch (p)
                {
                    case 0:
                        return UI.CheckOutOrder_Method_FreeCoupon; // "Free Coupon";
                    case 1:
                        return UI.CheckOutOrder_Method_Cash; // "Kontant";
                    case 2:
                        return UI.CheckOutOrder_Method_Account; //"Faktura";
                    case 3:
                        return "Presentkort";
                    case 4:
                        return UI.CheckOutOrder_Method_CreditCard; // "Kort";
                    case 5:
                        return "Bankkort";
                    case 6:
                        return "Check";
                    case 7:
                        return UI.CheckOutOrder_Label_CashBack; //"Utbetalning";
                    case 8:
                        return UI.Global_Return; //"Retur";
                    case 9:
                        return UI.CheckOutOrder_Method_Mobile; //"Retur";
                    case 10:
                        return "Swish";
                    case 11:
                        return "Elevkort";
                    case 12: return UI.CheckOutOrder_Method_CreditNote;
                    case 13: return UI.CheckOutOrder_Method_Beam;
                    case 14: return "AMEX";
                    case 15: return UI.CheckOutOrder_Method_OnlineCash;
                    case 16: return UI.CheckOutOrder_Method_Deposit;
                }
                return "Annan";
            }

            private enum ReportDataType
            {
                [LocalizedDescModel(@"Report_UniqueIdentification", typeof(UI))]
                UniqueIdentification,
                [LocalizedDescModel(@"Report_CloseDate", typeof(UI))]
                CloseDate,
                [LocalizedDescModel(@"Report_OpenDate", typeof(UI))]
                OpenDate,
                [LocalizedDescModel(@"Report_CashDrawerOpen", typeof(UI))]
                CashDrawerOpen,
                [LocalizedDescModel(@"Report_ReceiptCount", typeof(UI))]
                ReceiptCount,
                [LocalizedDescModel(@"Report_CashDrawerOpenCount", typeof(UI))]
                CashDrawerOpenCount,
                [LocalizedDescModel(@"Report_TotalSale", typeof(UI))]
                TotalSale,
                [LocalizedDescModel(@"Report_Rounding", typeof(UI))]
                Rounding,
                [LocalizedDescModel(@"Report_TotalReturn", typeof(UI))]
                TotalReturn,
                [LocalizedDescModel(@"Report_SaleTotal", typeof(UI))]
                SaleTotal,
                [LocalizedDescModel(@"Report_TotalNet", typeof(UI))]
                TotalNet,
                [LocalizedDescModel(@"Report_VATSum", typeof(UI))]
                VATSum,
                [LocalizedDescModel(@"Report_VATPercent", typeof(UI))]
                VATPercent,
                [LocalizedDescModel(@"Report_ReturnVATSum", typeof(UI))]
                ReturnVATSum,
                [LocalizedDescModel(@"Report_ReturnVATPercent", typeof(UI))]
                ReturnVATPercent,
                [LocalizedDescModel(@"Report_GrandTotalSale", typeof(UI))]
                GrandTotalSale,
                [LocalizedDescModel(@"Report_GrandTotalReturn", typeof(UI))]
                GrandTotalReturn,
                [LocalizedDescModel(@"Report_GrandTotalNet", typeof(UI))]
                GrandTotalNet,
                [LocalizedDescModel(@"Report_OrderCount", typeof(UI))]
                OrderCount,
                [LocalizedDescModel(@"Report_ReturnCount", typeof(UI))]
                ReturnCount,
                [LocalizedDescModel(@"Report_HoldCount", typeof(UI))]
                HoldCount,
                [LocalizedDescModel(@"Report_HoldTotalSale", typeof(UI))]
                HoldTotalSale,
                [LocalizedDescModel(@"Report_PaymentTypeSale", typeof(UI))]
                PaymentTypeSale,
                [LocalizedDescModel(@"Report_TipTypeSale", typeof(UI))]
                TipTypeSale,
                [LocalizedDescModel(@"Report_ReportNumber", typeof(UI))]
                ReportNumber,
                [LocalizedDescModel(@"Report_ProductsCount", typeof(UI))]
                ProductsCount,
                [LocalizedDescModel(@"Report_ServicesCount", typeof(UI))]
                ServicesCount,
                [LocalizedDescModel(@"Global_DiscountAmount", typeof(UI))]
                Discount,
                [LocalizedDescModel(@"Report_DiscountCount", typeof(UI))]
                DiscountCount,
                [LocalizedDescModel(@"Report_TrainingModeCount", typeof(UI))]
                TrainingModeCount,
                [LocalizedDescModel(@"Report_TrainingModeSale", typeof(UI))]
                TrainingModeSale,
                [LocalizedDescModel(@"Report_CategorySaleHeading", typeof(UI))]
                CategorySaleHeading,
                [LocalizedDescModel(@"Report_CategorySale", typeof(UI))]
                CategorySale,
                [LocalizedDescModel(@"Report_CategorySaleCountHeading", typeof(UI))]
                CategorySaleCountHeading,
                [LocalizedDescModel(@"Report_CategorySaleCount", typeof(UI))]
                CategorySaleCount,
                [LocalizedDescModel(@"Report_PaymentTypeNet", typeof(UI))]
                PaymentTypeNet,
                [LocalizedDescModel(@"Report_PaymentTypeReturn", typeof(UI))]
                PaymentTypeReturn,
                [LocalizedDescModel(@"Report_ReceiptCopyCount", typeof(UI))]
                ReceiptCopyCount,
                [LocalizedDescModel(@"Report_ReceiptCopyAmount", typeof(UI))]
                ReceiptCopyAmount,
                [LocalizedDescModel(@"Report_CashAdded", typeof(UI))]
                CashAdded,
                [LocalizedDescModel(@"Report_CashDropped", typeof(UI))]
                CashDropped,
                [LocalizedDescModel(@"Report_CashIn", typeof(UI))]
                CashIn,
                [LocalizedDescModel(@"Report_CashOut", typeof(UI))]
                CashOut,
                [LocalizedDescModel(@"Report_CashSum", typeof(UI))]
                CashSum,
                [LocalizedDescModel(@"Report_OtherNegativeRegistrations", typeof(UI))]
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
        }
    }
}