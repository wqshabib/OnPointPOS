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
    public class ReportRepository
    {
        public List<Report> LoadZReports(DateTime dtFrom, DateTime dtTo)
        {
            using (var db = new ApplicationDbContext())
            {

                return db.Report.Where(r => r.ReportType == 1).OrderByDescending(o => o.ReportNumber).ToList();
            }

        }
        public Guid GenerateReport(Guid terminalId, int reportType)
        {

            using (var db = new ApplicationDbContext())
            {
                using (var conn = db.Database.Connection)
                {
                    IDbCommand command = new SqlCommand();

                    conn.Open();
                    command.Connection = conn;
                    command.CommandTimeout = 180;
                    command.CommandType = CommandType.StoredProcedure;
                    
                   command.CommandText = "GenerateReportByTerminal"; //"SP_GenerateReportByTerminal";//
                    command.Parameters.Add(new SqlParameter("@TerminalId", terminalId));
                    command.Parameters.Add(new SqlParameter("@ReportType", reportType));
                    SqlParameter outputParameter = new SqlParameter("@ReportId", SqlDbType.UniqueIdentifier)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParameter);


                    command.ExecuteNonQuery();

                    var reportId = ((SqlParameter)command.Parameters["@ReportId"]).Value.ToString();



                    return Guid.Parse(reportId);
                }
            }

        }
        public List<ReportData> GetReport(Guid reportId)
        {
            using (var db = new ApplicationDbContext())
            {
                return   db.ReportData.Where(rd => rd.ReportId == reportId).OrderBy(rd => rd.SortOrder).ToList();
            }
        }

        public List<ReportData> GetUserReport(Guid terminalId,DateTime dtFrom, DateTime dtTo, string userId)
        {

            using (var db = new ApplicationDbContext())
            {
                using (var conn = db.Database.Connection)
                {
                    IDbCommand command = new SqlCommand();

                    conn.Open();
                    command.Connection = conn;
                    command.CommandTimeout = 180;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "SP_UserReportByDateRange";
                    command.Parameters.Add(new SqlParameter("@TerminalId", terminalId));
                    command.Parameters.Add(new SqlParameter("@OpenDate", dtFrom));
                    command.Parameters.Add(new SqlParameter("@CloseDate", dtTo));
                    command.Parameters.Add(new SqlParameter("@UserId", userId));
                    
                    
                    IDataReader dr= command.ExecuteReader();
                    List<ReportData> lstReportData = new List<ReportData>();

                    while (dr.Read())
                    {
                        lstReportData.Add(new ReportData
                        {

                            DataType = Convert.ToString(dr["DataType"]),
                            TextValue = Convert.ToString(dr["TextValue"]),
                            ForeignId = dr["ForeignId"] != DBNull.Value ? Convert.ToInt16(dr["ForeignId"]):0,
                            Value = dr["Value"] != DBNull.Value ? Convert.ToDecimal(dr["Value"]):0,
                            TaxPercent = dr["TaxPercent"] != DBNull.Value ? Convert.ToDecimal(dr["TaxPercent"]):0,
                            DateValue = dr["DateValue"] != DBNull.Value ? Convert.ToDateTime(dr["DateValue"]):DateTime.Now,
                            SortOrder = dr["SortOrder"] != DBNull.Value ? Convert.ToInt16(dr["SortOrder"]):0,

                        });
                    }
                    dr.Close();

                    return lstReportData;

                }
            }

        }


        public List<Journal> PrintJournalReport(DateTime dtFrom, DateTime dtTo)
        {
            
                var journalLogs = new List<Journal>();

            using (var db = new ApplicationDbContext())
            {
                using (var conn = db.Database.Connection)
                {
                    IDbCommand command = new SqlCommand();
                    conn.Open();
                    command.Connection = conn;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "SP_PrintJournal";
                    command.Parameters.Add(new SqlParameter("@dtFrom", dtFrom));
                    command.Parameters.Add(new SqlParameter("@dtTo", dtTo));


                    IDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        journalLogs.Add(new Journal
                        {
                            Id = Convert.ToInt64(dr["Id"]),
                            Action = Convert.ToString(dr["ActionCode"]),
                            LogMessage = Convert.ToString(dr["JournalLog"]),
                            Created = dr["Created"] != DBNull.Value ? Convert.ToDateTime(dr["Created"]) : DateTime.Now
                        });
                    }
                    dr.Close();
                }
            }
            return journalLogs;
        }

    }
}
