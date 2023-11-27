using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using POSSUM.Web.Models;
using System.Data.Entity;
using POSSUM.Model;
using System.Net;
using POSSUM.Data;
using Newtonsoft.Json;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace POSSUM.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class JournalController : MyBaseController
    {
        public JournalController()
        {

        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetJournals(jQueryDataTableParamModel param, Guid terminalId, DateTime dtFrom, DateTime dtTo)
        {
            List<JournalViewModel> objJournalLogs = new List<JournalViewModel>();
            int objCount = 0;

            try
            {
                using (var dbContext = GetConnection)
                {
                    using (var objConnection = dbContext.Database.Connection)
                    {
                        objConnection.Open();

                        IDbCommand objSqlCommand = new SqlCommand();
                        objSqlCommand.Connection = objConnection;
                        objSqlCommand.CommandType = CommandType.StoredProcedure;

                        if (terminalId == Guid.Empty)
                        {
                            objSqlCommand.CommandText = "SP_PrintJournal";
                            objSqlCommand.Parameters.Add(new SqlParameter("@dtFrom", dtFrom));
                            objSqlCommand.Parameters.Add(new SqlParameter("@dtTo", dtTo));
                        }
                        else
                        {
                            objSqlCommand.CommandText = "SP_PrintJournalTerminal";
                            objSqlCommand.Parameters.Add(new SqlParameter("@terminal", terminalId));
                            objSqlCommand.Parameters.Add(new SqlParameter("@dtFrom", dtFrom));
                            objSqlCommand.Parameters.Add(new SqlParameter("@dtTo", dtTo));
                        }


                        IDataReader objDataReader = objSqlCommand.ExecuteReader();

                        while (objDataReader.Read())
                        {
                            try
                            {
                                JournalViewModel objJournalViewModel = new JournalViewModel();
                                objJournalViewModel.Id = Convert.ToInt64(objDataReader["Id"]);
                                objJournalViewModel.Action = Convert.ToString(objDataReader["ActionCode"]);
                                objJournalViewModel.LogMessage = Convert.ToString(objDataReader["JournalLog"]);
                                objJournalViewModel.Created = objDataReader["Created"] != DBNull.Value ? Convert.ToDateTime(objDataReader["Created"]) : DateTime.Now;

                                objJournalLogs.Add(objJournalViewModel);
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        objDataReader.Close();
                    }
                }

                if (objJournalLogs.Count() > 0)
                {
                    objCount = objJournalLogs.Count();

                    objJournalLogs = objJournalLogs.OrderByDescending(obj => obj.Created)
                                    .Skip(param.iDisplayStart)
                                    .Take(param.iDisplayLength)
                                    .ToList();


                    foreach (var log in objJournalLogs)
                    {
                        log.CreatedDateString = log.Created.ToString("yyyy-MM-dd hh:mm:ss");

                        if (log.Action == "ReceiptPrinted")
                        {
                            string[] tokens = log.LogMessage.Split('|');

                            if (tokens.Length == 3)
                            {
                                StringBuilder objStringBuilder = new StringBuilder();

                                string text = tokens[0];
                                objStringBuilder.AppendLine(text);

                                objStringBuilder.AppendLine("---------------------------------------");

                                string items = tokens[1];

                                string[] item = items.Split('~');
                                foreach (var itm in item)
                                {
                                    objStringBuilder.AppendLine(itm);
                                }

                                objStringBuilder.AppendLine("---------------------------------------");

                                string vats = tokens[2];
                                var vatDetails = JsonConvert.DeserializeObject<List<VAT>>(vats);

                                string vatstring = Resource.Report_VAT;
                                string total = Resource.Total;

                                string s = string.Format("{0,-20} {1,5} {2,-8}", vatstring + "%", " ", vatstring + " " + total);

                                objStringBuilder.AppendLine(s);

                                foreach (var vat in vatDetails)
                                {
                                    string vatData = string.Format("{0,-20} {1,10} {2,-8}", vat.VATPercent, " ", Math.Round(vat.VATTotal, 2));
                                    objStringBuilder.AppendLine(vatData);
                                }

                                log.LogMessage = objStringBuilder.ToString();
                            }
                        }

                    }
                }

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = objCount,
                    iTotalDisplayRecords = objCount,
                    aaData = objJournalLogs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = objCount,
                    iTotalDisplayRecords = objCount,
                    aaData = objJournalLogs
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}