using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
	public class CustomerController
	{
		private string connectionString = "";
		public CustomerController(string constring)
		{
			connectionString = constring;
		}
		public void UploadCustomer(Customer customer, string baseUrl = "", string apiUser = "", string apiPassword = "")
		{
			try
			{

				ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
				client.PostCustomer(customer);


			}
			catch (Exception ex)
			{

				Log.WriteLog(ex.ToString());
			}
		}
		public void UploadCustomers(DateTime LAST_EXECUTED_DATETIME, DateTime EXECUTED_DATETIME, string baseUrl = "", string apiUser = "", string apiPassword = "")
		{
			try
			{
				List<Customer> customers = new List<Customer>();
				using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
				{
					var localCustomerRepo = uofLocal.CustomerRepository;
					var customerFieldRepo = uofLocal.Customer_CustomFieldRepository;
					customers = localCustomerRepo.Where(c => c.Updated >= LAST_EXECUTED_DATETIME && c.Updated <= EXECUTED_DATETIME).ToList();
					foreach (var customer in customers)
					{
						customer.Updated = EXECUTED_DATETIME.AddMinutes(6);
						customer.Customer_CustomField = customerFieldRepo.Where(c => c.CustomerId == customer.Id).ToList();
					}
				}
				foreach (var customer in customers)
				{
					ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
					client.PostCustomer(customer);
				}

			}
			catch (Exception ex)
			{

				Log.WriteLog(ex.ToString());
			}
		}

		public void UpdateCustomers(List<Customer> customers)
		{
			if (customers != null && customers.Count > 0)
			{
				try
				{
					using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
					{
						var localCustomerRepo = uofLocal.CustomerRepository;
						var customerFieldRepo = uofLocal.Customer_CustomFieldRepository;
						foreach (var model in customers)
						{
							var _customer = localCustomerRepo.FirstOrDefault(u => u.Id == model.Id);
							if (_customer == null)
							{
								_customer = new Customer();
								_customer.Id = model.Id;
								_customer.Name = model.Name;
								_customer.OrgNo = model.OrgNo;
								_customer.City = model.City;
								_customer.ZipCode = model.ZipCode;
								_customer.Address1 = model.Address1;
								_customer.Address2 = model.Address2;
								_customer.FloorNo = model.FloorNo;
								_customer.PortCode = model.PortCode;
								_customer.Created = model.Created;
								_customer.Updated = model.Updated;
								_customer.Phone = model.Phone;
								_customer.Active = model.Active;
								_customer.Reference = model.Reference;
								_customer.CustomerNo = model.CustomerNo;
								_customer.DirectPrint = model.DirectPrint;
								_customer.ExternalId = model.ExternalId;
                                _customer.Email = model.Email;
								_customer.DepositAmount = model.DepositAmount;
								_customer.HasDeposit = model.HasDeposit;
								_customer.LastBalanceUpdated = model.LastBalanceUpdated;
							}
							else
							{
								_customer.Name = model.Name;
								_customer.OrgNo = model.OrgNo;
								_customer.City = model.City;
								_customer.ZipCode = model.ZipCode;
								_customer.Address1 = model.Address1;
								_customer.Address2 = model.Address2;
								_customer.FloorNo = model.FloorNo;
								_customer.PortCode = model.PortCode;
								_customer.CustomerNo = model.CustomerNo;
								_customer.DirectPrint = model.DirectPrint;
								_customer.Created = model.Created;
								_customer.Updated = model.Updated;
								_customer.Phone = model.Phone;
								_customer.Active = model.Active;
								_customer.Reference = model.Reference;
								_customer.ExternalId = model.ExternalId;
                                _customer.Email = model.Email;
								_customer.DepositAmount = model.DepositAmount;
								_customer.HasDeposit = model.HasDeposit;
								_customer.LastBalanceUpdated = model.LastBalanceUpdated;
							}
                            
							localCustomerRepo.AddOrUpdate(_customer);
							var existingList = customerFieldRepo.Where(c => c.CustomerId == model.Id);
							if (existingList != null && existingList.Count() > 0)
								foreach (var existing in existingList)
									customerFieldRepo.Delete(existing);


						}
						uofLocal.Commit();


					}

					using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
					{
						var localCustomerRepo = uofLocal.CustomerRepository;
						var customerFieldRepo = uofLocal.Customer_CustomFieldRepository;
						foreach (var model in customers)
						{
							if (model.Customer_CustomField != null && model.Customer_CustomField.Count > 0)
							{

								foreach (var customField in model.Customer_CustomField)
								{
									if (customField.Id == default(Guid))
										customField.Id = Guid.NewGuid();
									customField.CustomerId = model.Id;
									customField.Updated = DateTime.Now;
									customerFieldRepo.AddOrUpdate(customField);
								}
							}

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
