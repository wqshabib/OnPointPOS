using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
    public class DepositHistoryController
    {
		private string connectionString = "";
		
		public DepositHistoryController(string constring)
		{
			connectionString = constring;
		}
		
		public void UploadDepositHistory(DepositHistory depositHistory, string baseUrl = "", string apiUser = "", string apiPassword = "")
		{
			try
			{
				ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
				client.PostDepositHistory(depositHistory);
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
		public void UploadDepositHistory(DateTime LAST_EXECUTED_DATETIME, DateTime EXECUTED_DATETIME, string baseUrl = "", string apiUser = "", string apiPassword = "")
		{
			try
			{
				List<DepositHistory> lstDepositHistory = new List<DepositHistory>();
				using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
				{
					var depositHistoryRepository = uofLocal.DepositHistoryRepository;

					lstDepositHistory = depositHistoryRepository.Where(c => c.CreatedOn >= LAST_EXECUTED_DATETIME && c.CreatedOn <= EXECUTED_DATETIME).ToList();
					foreach (var depositHistory in lstDepositHistory)
					{
						depositHistory.CreatedOn = EXECUTED_DATETIME.AddMinutes(6);
					}
				}

				foreach (var depositHistory in lstDepositHistory)
				{
					ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
					client.PostDepositHistory(depositHistory);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLog(ex.ToString());
			}
		}

		public void UpdateDepositHistories(List<DepositHistory> lstDepositHistory)
		{
			if (lstDepositHistory != null && lstDepositHistory.Count > 0)
			{
				try
				{
					using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
					{
						var depositHistoryRepository = uofLocal.DepositHistoryRepository;
						foreach (var model in lstDepositHistory)
						{
							var obj = depositHistoryRepository.FirstOrDefault(u => u.Id == model.Id);
							if (obj == null)
							{
								obj = new DepositHistory();
								obj.Id = model.Id;
								obj.OrderId = model.OrderId;
								obj.OldBalance = model.OldBalance;
								obj.NewBalance = model.NewBalance;
								obj.MerchantReceipt = model.MerchantReceipt;
								obj.CreatedBy = model.CreatedBy;
								obj.CreatedOn = model.CreatedOn;
								obj.CustomerId = model.CustomerId;
								obj.CustomerReceipt = model.CustomerReceipt;
								obj.DepositAmount = model.DepositAmount;
								obj.DepositType = model.DepositType;
								obj.TerminalId = model.TerminalId;
							}
							else
							{
								obj.OrderId = model.OrderId;
								obj.OldBalance = model.OldBalance;
								obj.NewBalance = model.NewBalance;
								obj.MerchantReceipt = model.MerchantReceipt;
								obj.CreatedBy = model.CreatedBy;
								obj.CreatedOn = model.CreatedOn;
								obj.CustomerId = model.CustomerId;
								obj.CustomerReceipt = model.CustomerReceipt;
								obj.DepositAmount = model.DepositAmount;
								obj.DepositType = model.DepositType;
								obj.TerminalId = model.TerminalId;
							}

							depositHistoryRepository.AddOrUpdate(obj);
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
