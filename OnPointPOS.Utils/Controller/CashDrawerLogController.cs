using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
	public class CashDrawerLogController
	{
		private string connectionString = "";

		public CashDrawerLogController(string constring)
		{
			connectionString = constring;
		}

		public void UploadCashDrawerLog(CashDrawerLog CashDrawerLog, string baseUrl = "", string apiUser = "", string apiPassword = "")
		{
			try
			{
				ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
				client.PostCashDrawerLog(CashDrawerLog);
			}
			catch (Exception ex)
			{
				Log.WriteLog(ex.ToString());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="LAST_EXECUTED_DATETIME"></param>
		/// <param name="EXECUTED_DATETIME"></param>
		/// <param name="baseUrl"></param>
		/// <param name="apiUser"></param>
		/// <param name="apiPassword"></param>
		public void UploadCashDrawerLog(DateTime LAST_EXECUTED_DATETIME, DateTime EXECUTED_DATETIME, string baseUrl = "", string apiUser = "", string apiPassword = "")
		{
			try
			{
				List<CashDrawerLog> lstCashDrawerLog = new List<CashDrawerLog>();
				using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
				{
					var CashDrawerLogRepository = uofLocal.CashDrawerLogRepository;

					lstCashDrawerLog = CashDrawerLogRepository.Where(c => c.ActivityDate >= LAST_EXECUTED_DATETIME && c.ActivityDate <= EXECUTED_DATETIME).ToList();
					foreach (var CashDrawerLog in lstCashDrawerLog)
					{
						CashDrawerLog.ActivityDate = EXECUTED_DATETIME.AddMinutes(6);
					}
				}

				foreach (var CashDrawerLog in lstCashDrawerLog)
				{
					ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
					client.PostCashDrawerLog(CashDrawerLog);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLog(ex.ToString());
			}
		}

		public void UpdateCashDrawerLogs(List<CashDrawerLog> lstCashDrawerLog)
		{
			if (lstCashDrawerLog != null && lstCashDrawerLog.Count > 0)
			{
				try
				{
					using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
					{
						var CashDrawerLogRepository = uofLocal.CashDrawerLogRepository;
						foreach (var model in lstCashDrawerLog)
						{
							var obj = CashDrawerLogRepository.FirstOrDefault(u => u.Id == model.Id);
							if (obj == null)
							{
								obj = new CashDrawerLog();
								obj.Id = model.Id;
								obj.OrderId = model.OrderId;
								obj.ActivityDate = model.ActivityDate;
								obj.ActivityType = model.ActivityType;
								obj.Amount = model.Amount;
								obj.CashDrawer = model.CashDrawer;
								obj.CashDrawerId = model.CashDrawerId;
								obj.Synced = model.Synced;
								obj.UserId = model.UserId;
							}
							else
							{
								obj.Id = model.Id;
								obj.OrderId = model.OrderId;
								obj.ActivityDate = model.ActivityDate;
								obj.ActivityType = model.ActivityType;
								obj.Amount = model.Amount;
								obj.CashDrawer = model.CashDrawer;
								obj.CashDrawerId = model.CashDrawerId;
								obj.Synced = model.Synced;
								obj.UserId = model.UserId;
							}

							CashDrawerLogRepository.AddOrUpdate(obj);
						}

						uofLocal.Commit();
					}
				}
				catch (Exception ex)
				{
					Log.WriteLog(ex.ToString());
				}
			}
		}

	}
}
