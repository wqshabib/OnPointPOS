using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using POSSUM.Model;
using System.Reflection;
using System.Configuration;
using System.Runtime;
using Microsoft.CSharp;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.Data;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;

namespace POSSUM.Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/DepositHistory")]
    public class DepositHistoryController : BaseAPIController
    {
        string connectionString = "";
        bool nonAhenticated = false;
        public DepositHistoryController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        /// <summary>
        /// GetDepositHistory
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        [Route("GetDepositHistory")]
        public async Task<List<DepositHistory>> GetDepositHistory([FromUri] Dates dates)
        {
            try
            {
                //LogWriter.LogException(new Exception("GetDepositHistory is calling"));
                //LogWriter.LogWrite("GetDepositHistory is calling: connectionString:" + connectionString);

                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;

                List<DepositHistory> liveDepositHistory = new List<DepositHistory>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var skip = dates.PageNo * dates.PageSize;
                    var take = dates.PageSize;
                    var lstDepositHistory = db.DepositHistory.Where(usr => usr.CreatedOn > LAST_EXECUTED_DATETIME 
                    && usr.CreatedOn <= EXECUTED_DATETIME).OrderByDescending(a=>a.CreatedOn).Skip(skip).Take(take).ToList();
                    foreach (var objDepositHistory in lstDepositHistory)
                    {
                        var newDepositHistory = new DepositHistory
                        {
                            Id = objDepositHistory.Id,
                            CreatedBy = objDepositHistory.CreatedBy,
                            CreatedOn = objDepositHistory.CreatedOn,
                            CustomerId = objDepositHistory.CustomerId,
                            CustomerReceipt = objDepositHistory.CustomerReceipt,
                            DepositAmount = objDepositHistory.DepositAmount,
                            DepositType = objDepositHistory.DepositType,
                            MerchantReceipt = objDepositHistory.MerchantReceipt,
                            NewBalance = objDepositHistory.NewBalance,
                            OldBalance = objDepositHistory.OldBalance,
                            OrderId = objDepositHistory.OrderId,
                            TerminalId = objDepositHistory.TerminalId
                        };
                        
                        liveDepositHistory.Add(newDepositHistory);
                    }


                    return liveDepositHistory;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("GetDepositHistory Exception: " + ex);
                LogWriter.LogException(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new List<DepositHistory>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostDepositHistory")]
        public IHttpActionResult PostDepositHistory(DepositHistory model)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var existingDepositHistory = db.DepositHistory.FirstOrDefault(u => u.Id == model.Id);
                    if (existingDepositHistory == null)
                    {
                        var newDepositHistory = new DepositHistory
                        {
                            Id = model.Id,
                            CreatedBy = model.CreatedBy,
                            CreatedOn = model.CreatedOn,
                            CustomerId = model.CustomerId,
                            CustomerReceipt = model.CustomerReceipt,
                            DepositAmount = model.DepositAmount,
                            DepositType = model.DepositType,
                            MerchantReceipt = model.MerchantReceipt,
                            NewBalance = model.NewBalance,
                            OldBalance = model.OldBalance,
                            OrderId = model.OrderId,
                            TerminalId = model.TerminalId
                        };

                        db.DepositHistory.Add(newDepositHistory);
                    }
                    else
                    {
                        existingDepositHistory.CreatedBy = model.CreatedBy;
                        existingDepositHistory.CreatedOn = model.CreatedOn;
                        existingDepositHistory.CustomerId = model.CustomerId;
                        existingDepositHistory.CustomerReceipt = model.CustomerReceipt;
                        existingDepositHistory.DepositAmount = model.DepositAmount;
                        existingDepositHistory.DepositType = model.DepositType;
                        existingDepositHistory.TerminalId = model.TerminalId;
                        existingDepositHistory.MerchantReceipt = model.MerchantReceipt;
                        existingDepositHistory.NewBalance = model.NewBalance;
                        existingDepositHistory.OldBalance = model.OldBalance;
                        existingDepositHistory.OrderId = model.OrderId;
                        db.Entry(existingDepositHistory).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.SaveChanges();
                    return StatusCode(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }
    }
}