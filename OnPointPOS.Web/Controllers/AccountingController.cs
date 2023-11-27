using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace POSSUM.Web.Controllers
{
    // [Authorize(Roles = "Admin")]
    [Authorize]
    public class AccountingController : MyBaseController
    {


        public AccountingController()
        {

        }
        public ActionResult Index()
        {
            try
            {
                using (var db = GetConnection)
                {
                    var acountings = db.Accounting.Where(o => !o.IsDeleted );
                    var accountinglist = acountings.OrderBy(or => or.SortOrder).ToList();
                    return View(accountinglist); 
                }

            }




            catch (Exception ex)
            {

                throw;
            }
        }
        public ActionResult ExportFile()
        {
            return View();
        }
        private async Task<List<OrderViewModel>> GetDataAsync(string _terminalId, string _dtFrom, string _dtTo)
        {
            using (ApplicationDbContext db = GetConnection)
            {
                Guid terminalId = Guid.Parse(_terminalId);
                DateTime dateFrom = Convert.ToDateTime(_dtFrom).Date;
                DateTime dateTo = Convert.ToDateTime(_dtTo + "  11:59:00 PM");
                return await (from om in db.OrderMaster.Include("OrderLines").Where(c => c.TrainingMode == false && (c.CreationDate >= dateFrom && c.CreationDate <= dateTo) && c.TerminalId == terminalId && c.InvoiceGenerated == 1)

                               select new OrderViewModel
                               {
                                   Id = om.Id,
                                   RoundedAmount = om.RoundedAmount,
                                   InvoiceDate = om.InvoiceDate,
                                   CreationDate = om.CreationDate,

                                   OrderLines = om.OrderLines.Where(od => od.ItemType != ItemType.Grouped && od.Active == 1).ToList()
                               }).ToListAsync();

               
            }
        }
        public FileResult DownloadFile(string _terminalId, string _dtFrom, string _dtTo)
        {
            try
            {
                StringBuilder b = new StringBuilder();
                string terminalcode = "";
                string exportDate = "";
                Terminal terminal = new Terminal();
                Outlet outlet = new Outlet();
                List<AccountingData> data = new List<AccountingData>();
                using (ApplicationDbContext db = GetConnection)
                {
                    Guid terminalId = Guid.Parse(_terminalId);
                    DateTime dateFrom = Convert.ToDateTime(_dtFrom).Date;
                    DateTime dateTo = Convert.ToDateTime(_dtTo + "  11:59:00 PM");
                    
                    terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                    outlet.Id = terminal.Outlet.Id;
                    outlet.Name = terminal.Outlet.Name;
                    outlet.OrgNo = terminal.Outlet.OrgNo;
                    terminalcode = terminal.UniqueIdentification;
                    exportDate = dateFrom.Year.ToString() + dateFrom.Month.ToString() + dateFrom.Day.ToString() + "_" + dateTo.Year.ToString() + dateTo.Month.ToString() + dateTo.Day.ToString();

                    string query = "exec SP_ExportAccounting '" + terminalId + "', '" + dateFrom + "','" + dateTo + "'";
                    data = db.Database.SqlQuery<AccountingData>(query).ToList();

                    //using (var conn = db.Database.Connection)
                    //{
                    //    conn.Open();
                    //    IDbCommand command = new SqlCommand(query);
                    //    command.Connection = conn;
                    //    // command.Connection = uof.Session.Connection;
                    //    //command.CommandType = CommandType.Text;
                    //    //command.CommandText = query;
                    //    // uof.Session.Transaction.Enlist(command);

                    //    IDataReader dr = command.ExecuteReader();

                    //    while (dr.Read())
                    //    {
                    //        data.Add(new AccountingData
                    //        {

                    //            SortOrder = Convert.ToInt32(dr["SortOrder"]),
                    //            DataType = Convert.ToString(dr["DataType"]),
                    //            DataTypeText = Convert.ToString(dr["DataTypeText"]),
                    //            DataValue = Convert.ToString(dr["DataValue"]),
                    //            SaleDay = Convert.ToString(dr["SaleDay"]),
                    //            Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0,

                    //        });
                    //    }
                    //    dr.Dispose();
                    //}

                }
                string month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
                string day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
                var generatedOn = DateTime.Now.Year.ToString() + month + day;

                string newline = Environment.NewLine;
                b.Append("#FLAGGA 0");
                b.Append(newline);
                b.Append("#FORMAT PC8");
                b.Append(newline);
                b.Append("#SIETYP 4");
                b.Append(newline);
                b.Append("#PROGRAM \"POSSUM SE\" 1.0");
                b.Append(newline);
                b.Append("#GEN " + generatedOn);
                b.Append(newline);
                b.Append("#FNAMN \"" + outlet.Name + "\"");
                b.Append(newline);
                if (!string.IsNullOrEmpty(outlet.OrgNo))
                {
                    b.Append("#ORGNR " + outlet.OrgNo);
                    b.Append(newline);
                }

                b.Append("#VALUTA SEK");
                b.Append(newline);
                
                List<string> dataTypes = new List<string>();
                 var groups = data.GroupBy(o => new { o.SaleDay });
                foreach (var grp in groups)
                {
                    foreach (var dr in grp.OrderBy(o => o.SortOrder))
                    {
                        if (!dataTypes.Contains(dr.DataType) && dr.DataType!= "TipAmount")
                        {
                            string row = "#KONTO " + dr.DataType + " \"" + dr.DataTypeText + "\"";
                            b.Append(row);
                            b.Append(newline);
                            dataTypes.Add(dr.DataType);
                        }
                    }
                }

                foreach (var grp in groups)
                {
                    var d = grp.First();
                    b.Append("#VER " + "\"" + "\"" + " " + "\"" + d.DataType + "\"" + " " + d.SaleDay.Replace("-", "") + "" + " " + "\"" + terminalcode + "" + " Sales\"" + "");
                    b.Append(newline);
                    b.Append("{");
                    b.Append(newline);
                    var payments = grp.Where(p => p.SortOrder < 2).Sum(p => p.Amount);
                    var rounded = grp.Where(p => p.SortOrder == 2).Sum(p => p.Amount);
                    var netsale = grp.Where(p => p.SortOrder > 2 && p.SortOrder < 50).Sum(p => p.Amount);
                    var returnPayment = grp.Where(p => p.SortOrder == 50).Sum(p => p.Amount);
                    var returnSale = grp.Where(p => p.SortOrder > 50).Sum(p => p.Amount);
                    //var retPaymentDiff = returnPayment + returnSale;
                    // rounded = rounded + retPaymentDiff;
                    var res = payments - returnSale - returnPayment + rounded + netsale;
                    if (res != 0)
                        rounded = rounded - res;

                    /*bool canAddInMinus = false;
                    if (grp.FirstOrDefault(a=>a.DataTypeText == "Paid by Cash" && Convert.ToDouble(a.DataValue) >= 0) == null)
                    {
                        if (grp.FirstOrDefault(a => a.DataTypeText == "Paid by Cash" && Convert.ToDouble(a.DataValue) < 0) != null)
                        {
                            canAddInMinus = true;
                        }
                        else
                        {
                            double tipAmount = 0;
                            var tipAmountItem = grp.FirstOrDefault(a => a.DataType == "TipAmount");
                            if (tipAmountItem != null && !string.IsNullOrEmpty(tipAmountItem.DataValue))
                            {
                                tipAmount = Convert.ToDouble(tipAmountItem.DataValue);
                            }

                            if (tipAmount != 0)
                            {
                                string row = "#TRANS " + "1910" + " {} -" + tipAmount + " " + d.SaleDay.Replace("-", "") + " \"" + "Paid by Cash" + "\"";
                                b.Append(row);
                                b.Append(newline);
                            }
                        }
                    }*/

                    foreach (var dr in grp.OrderBy(o => o.SortOrder))
                    {
                        /*if (dr.DataType == "TipAmount")
                        {
                            continue;
                        }*/

                        //20/12/2019
                        // We have to use values returned from SP and it should only show
                        // Rounded values from database
                        if (dr.SortOrder == 2)
                        {
                            string row = "#TRANS " + dr.DataType + " {} " + rounded.ToString().Replace(',', '.') + " " + dr.SaleDay.Replace("-", "") + " \"" + dr.DataTypeText + "\"";
                            b.Append(row);
                        }
                        else
                        {
                            //if (dr.DataTypeText == "Paid by Cash")
                            //{
                            //    double tipAmount = 0;
                            //    var tipAmountItem = grp.FirstOrDefault(a => a.DataType == "TipAmount");
                            //    if (tipAmountItem != null && !string.IsNullOrEmpty(tipAmountItem.DataValue))
                            //    {
                            //        tipAmount = Convert.ToDouble(tipAmountItem.DataValue);
                            //    }

                            //    var value = Convert.ToDouble(dr.DataValue);
                            //    if (tipAmount != 0 && (value > 0 || canAddInMinus))
                            //        value = value - tipAmount;

                            //    string row = "#TRANS " + dr.DataType + " {} " + value + " " + dr.SaleDay.Replace("-", "") + " \"" + dr.DataTypeText + "\"";
                            //    b.Append(row);
                            //}
                            //else
                            {
                                if (dr.DataType == "2820")
                                {
                                    string row = "#TRANS " + dr.DataType + " {} -" + dr.DataValue + " " + dr.SaleDay.Replace("-", "") + " \"" + dr.DataTypeText + "\"";
                                    b.Append(row);
                                }
                                else
                                {
                                    string row = "#TRANS " + dr.DataType + " {} " + dr.DataValue + " " + dr.SaleDay.Replace("-", "") + " \"" + dr.DataTypeText + "\"";
                                    b.Append(row);
                                }
                            }
                        }
                        b.Append(newline);
                    }

                    b.Append("}");
                    b.Append(newline);

                    //var d = grp.First();
                    //b.Append("#VER " + "\"" + "\"" + " " + "\"" + "\"" + " " + d.SaleDay.Replace("-", "") + "" + " " + "\"" + terminalcode + "" + " Sales\"" + "");
                    //b.Append(newline);
                    //b.Append("{");
                    //b.Append(newline);
                    //var payments = grp.Where(p => p.SortOrder < 2).Sum(p => p.Amount);
                    //var rounded = grp.Where(p => p.SortOrder == 2).Sum(p => p.Amount);
                    //var netsale = grp.Where(p => p.SortOrder > 2 && p.SortOrder < 49).Sum(p => p.Amount);
                    //decimal returnCash = 0;
                    //try
                    //{
                    //    returnCash = grp.Where(p => p.SortOrder == 49).Sum(p => p.Amount);
                    //}
                    //catch
                    //{
                    //}
                    //var cashExists = grp.Any(p => p.DataType == "1910");

                    //var returnPayment = grp.Where(p => p.SortOrder == 50).Sum(p => p.Amount);
                    //var returnSale = grp.Where(p => p.SortOrder > 50).Sum(p => p.Amount);
                    ////var retPaymentDiff = returnPayment + returnSale;
                    //// rounded = rounded + retPaymentDiff;

                    //var res = payments - returnSale - returnPayment + rounded + netsale;
                    //if (res != 0)
                    //    rounded = rounded - res;
                    //foreach (var dr in grp.OrderBy(o => o.SortOrder))
                    //{
                    //    if (dr.DataType == "-1")
                    //    {
                    //        continue;
                    //    }
                    //    if (dr.SortOrder == 2)
                    //    {
                    //        string row = "#TRANS " + dr.DataType + " {} " + rounded.ToString().Replace(',', '.');
                    //        b.Append(row);
                    //    }
                    //    else
                    //    {
                    //        if (dr.DataType == "1930")
                    //        {
                    //            var val = dr.Amount + returnCash;
                    //            string row = "#TRANS " + dr.DataType + " {} " + val.ToString().Replace(',', '.');
                    //            b.Append(row);
                    //            if (cashExists == false)
                    //            {
                    //                b.Append(newline);
                    //                var cash = (-1) * returnCash;
                    //                string rw = "#TRANS " + "1910" + " {} " + cash.ToString().Replace(',', '.');
                    //                b.Append(rw);

                    //            }
                    //        }
                    //        else if (dr.DataType == "1910")
                    //        {

                    //            var val = dr.Amount - returnCash;
                    //            string row = "#TRANS " + dr.DataType + " {} " + val.ToString().Replace(',', '.');
                    //            b.Append(row);
                    //        }
                    //        else
                    //        {
                    //            string row = "#TRANS " + dr.DataType + " {} " + dr.DataValue;
                    //            b.Append(row);
                    //        }
                    //    }
                    //    b.Append(newline);
                    //}

                    //b.Append("}");
                    //b.Append(newline);
                }


                Encoding iso = Encoding.GetEncoding("ISO-8859-1");//For western language uni code
                var byteArray = iso.GetBytes(b.ToString()); // Encoding.ASCII.GetBytes(b.ToString());
                var stream = new MemoryStream(byteArray);
                string fileName = terminalcode + "_" + exportDate;
                return File(stream, "text/plain", fileName + ".si");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public JsonResult FillTerminal()
        {
            using (var db = GetConnection)
            {
                var terminals = db.Terminal.Where(c => c.IsDeleted == false).Select(t => new TerminalData
                {
                    Id = t.Id,
                    Name = t.UniqueIdentification
                }).ToList();

                return Json(terminals.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(int id)
        {
            Accounting model = new Accounting();
            if (id > 0)
            {
                using (var db = GetConnection)
                {

                    model = db.Accounting.FirstOrDefault(t => t.Id == id);
                    if (model == null)
                        model = new Accounting();
                }
            }

            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Create(Accounting viewModel)
        {
            string msg = "";
            try
            {
                var db = GetConnection;
                using (var uof = new UnitOfWork(db))
                {
                    var acountingRepo = uof.AccountingRepository;
                    var prodRepo = uof.ProductRepository;
                    Accounting accounting = new Accounting();

                    if (viewModel.Id != 0)
                    {
                        accounting = acountingRepo.Single(t => t.Id == viewModel.Id);

                    }
                    else
                    {
                        int lastId = 0;
                        try
                        {
                            lastId = (int)acountingRepo.Max(i => i.Id);
                        }
                        catch
                        {
                        }

                        accounting.Id = lastId + 1;
                    }
                    accounting.AcNo = viewModel.AcNo;
                    accounting.Name = viewModel.Name;
                    accounting.TAX = viewModel.TAX;
                    accounting.SortOrder = viewModel.SortOrder;
                    accounting.Updated = DateTime.Now;


                    acountingRepo.AddOrUpdate(accounting);

                    //Update Tax value in Link Products of current accounting
                    var items = prodRepo.AsQueryable().Where(p => p.AccountingId == accounting.Id).ToList();
                    if (items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            item.Tax = accounting.TAX;
                            item.Updated = DateTime.Now;
                            prodRepo.AddOrUpdate(item);
                        }
                    }


                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == accounting.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = accounting.Id.ToString(),
                                TableName = TableName.Accounting
                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }
                    uof.Commit();
                }
                msg = "Success" + ":" + Resource.Accounting + " " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteAccounting(int id)
        {
            string msg = "";
            try
            {
                var db = GetConnection;

                using (var uof = new UnitOfWork(GetConnection))
                {
                    var accoountingRepo = uof.AccountingRepository;
                    var accounting = accoountingRepo.Single(o => o.Id == id);
                    accounting.IsDeleted = true;
                    accoountingRepo.AddOrUpdate(accounting);

                    var terminalRepo = uof.TerminalRepository;
                    var terminals = terminalRepo.GetAll().ToList();
                    foreach (var terminal in terminals)
                    {
                        if (!db.TablesSyncLog.Any(s => s.TerminalId == terminal.Id && s.TableKey == accounting.Id.ToString()))
                        {
                            var syncLog = new TablesSyncLog
                            {
                                OutletId = terminal.OutletId,
                                TerminalId = terminal.Id,
                                TableKey = accounting.Id.ToString(),
                                TableName = TableName.Customer
                            };
                            db.TablesSyncLog.Add(syncLog);
                            db.SaveChanges();
                        }
                    }

                    uof.Commit();
                }
                msg = Resource.Success + ":" + Resource.Accounting + " " + Resource.Deleted + " " + Resource.successfully;
            }
            catch (Exception ex)
            {

                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }

    public class AccountingData
    {
        public int SortOrder { get; set; }
        public string SaleDay { get; set; }
        public string DataType { get; set; }
        public string DataTypeText { get; set; }
        public string DataValue { get; set; }

        public decimal Amount { get; set; }


    }
}