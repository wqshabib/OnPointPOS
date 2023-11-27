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
    [System.Web.Http.RoutePrefix("api/CashDrawerLog")]
    public class CashDrawerLogController : BaseAPIController
    {
        string connectionString = "";
        bool nonAhenticated = false;
        public CashDrawerLogController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        /// <summary>
        /// GetCashDrawerLog
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        [Route("GetCashDrawerLog")]
        public async Task<List<CashDrawerLog>> GetCashDrawerLog([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;

                List<CashDrawerLog> liveCashDrawerLog = new List<CashDrawerLog>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var skip = dates.PageNo * dates.PageSize;
                    var take = dates.PageSize;
                    var lstCashDrawerLog = db.CashDrawerLog.Where(usr => usr.ActivityDate > LAST_EXECUTED_DATETIME
                    && usr.ActivityDate <= EXECUTED_DATETIME).OrderByDescending(a => a.ActivityDate).Skip(skip).Take(take).ToList();
                    foreach (var model in lstCashDrawerLog)
                    {
                        var newCashDrawerLog = new CashDrawerLog
                        {
                            Id = model.Id,
                            OrderId = model.OrderId,
                            ActivityDate = model.ActivityDate,
                            ActivityType = model.ActivityType,
                            Amount = model.Amount,
                            CashDrawer = model.CashDrawer,
                            CashDrawerId = model.CashDrawerId,
                            Synced = model.Synced,
                            UserId = model.UserId
                        };

                        liveCashDrawerLog.Add(newCashDrawerLog);
                    }


                    return liveCashDrawerLog;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("GetCashDrawerLog Exception: " + ex);
                LogWriter.LogException(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new List<CashDrawerLog>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostCashDrawerLog")]
        public IHttpActionResult PostCashDrawerLog(CashDrawerLog model)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var existingCashDrawerLog = db.CashDrawerLog.FirstOrDefault(u => u.Id == model.Id);
                    if (existingCashDrawerLog == null)
                    {
                        var newCashDrawerLog = new CashDrawerLog
                        {
                            Id = model.Id,
                            OrderId = model.OrderId,
                            ActivityDate = model.ActivityDate,
                            ActivityType = model.ActivityType,
                            Amount = model.Amount,
                            CashDrawer = model.CashDrawer,
                            CashDrawerId = model.CashDrawerId,
                            Synced = model.Synced,
                            UserId = model.UserId
                        };

                        db.CashDrawerLog.Add(newCashDrawerLog);
                    }
                    else
                    {
                        existingCashDrawerLog.OrderId = model.OrderId;
                        existingCashDrawerLog.ActivityDate = model.ActivityDate;
                        existingCashDrawerLog.ActivityType = model.ActivityType;
                        existingCashDrawerLog.Amount = model.Amount;
                        existingCashDrawerLog.CashDrawer = model.CashDrawer;
                        existingCashDrawerLog.CashDrawerId = model.CashDrawerId;
                        existingCashDrawerLog.Synced = model.Synced;
                        existingCashDrawerLog.UserId = model.UserId;
                        db.Entry(existingCashDrawerLog).State = System.Data.Entity.EntityState.Modified;
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