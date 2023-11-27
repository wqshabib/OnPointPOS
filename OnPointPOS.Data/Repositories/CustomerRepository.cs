using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class CustomerRepository : GenericRepository<Customer>, IDisposable
    {
        private readonly ApplicationDbContext context;

        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public CustomerRepository() : base(new ApplicationDbContext())
        {
        }
        public Customer GetCustomerById(Guid customerId)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                return db.Customer.FirstOrDefault(c => c.Id == customerId);
            }
        }
        public List<Customer> SearchCustomer(string keyword, CustomerType customerType)
        {
            using (var db = new ApplicationDbContext())
            {
                if (customerType == CustomerType.All)
                {
                    return string.IsNullOrEmpty(keyword)
                        ? db.Customer.ToList()
                        : db.Customer.Where(c => c.Name.Contains(keyword) || c.OrgNo.Contains(keyword)).ToList();
                }
                else if (customerType == CustomerType.Deposit)
                {
                    return string.IsNullOrEmpty(keyword)
                            ? db.Customer.Where(c => c.HasDeposit == true).ToList()
                            : db.Customer.Where(c => (c.Name.Contains(keyword) || c.OrgNo.Contains(keyword)) && c.HasDeposit == true).ToList();
                }
                else
                {
                    return string.IsNullOrEmpty(keyword)
                            ? db.Customer.Where(c => c.HasDeposit == false).ToList()
                            : db.Customer.Where(c => (c.Name.Contains(keyword) || c.OrgNo.Contains(keyword)) && c.HasDeposit == false).ToList();
                }
            }
        }
        public List<Customer> GetAllCustomers()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                return db.Customer.Where(c => c.Active == true && c.HasDeposit==false).ToList();
            }
        }

        public bool HasDepositHistory(Guid id)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                return db.DepositHistory.Count(c => c.CustomerId == id) > 0;
            }
        }

        public bool SaveCustomer(Customer customer)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Customer.Add(customer);
                db.SaveChanges();
                return true;
            }
        }

        public bool SaveNewCustomer(Guid customerId, Customer customer)
        {
            using (var db = new ApplicationDbContext())
            {
                var dbCustomer = db.Customer.FirstOrDefault(c => c.Id == customerId);
                if (dbCustomer == null)
                {
                    db.Customer.Add(customer);
                    db.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public bool IsOrderExistInDepositHistory(Guid orderId)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.DepositHistory.FirstOrDefault(a => a.OrderId == orderId) != null;
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            using (var db = new ApplicationDbContext())
            {
                var _customer = db.Customer.FirstOrDefault(c => c.Id == customer.Id);
                if (_customer != null)
                {
                    _customer.CustomerNo = customer.CustomerNo;
                    _customer.Email = customer.Email;
                    _customer.Name = customer.Name;
                    _customer.OrgNo = customer.OrgNo;
                    _customer.Phone = customer.Phone;
                    _customer.Reference = customer.Reference;
                    _customer.Address1 = customer.Address1;
                    _customer.Address2 = customer.Address2;
                    _customer.City = customer.City;
                    _customer.PortCode = customer.PortCode;
                    _customer.FloorNo = customer.FloorNo;
                    _customer.ZipCode = customer.ZipCode;
                    _customer.DirectPrint = customer.DirectPrint;
                    _customer.Updated = DateTime.Now;
                    _customer.LastBalanceUpdated = customer.LastBalanceUpdated;
                    _customer.DepositAmount = customer.DepositAmount;
                    _customer.HasDeposit = customer.HasDeposit;
                }
                db.SaveChanges();
                return true;
            }

        }


        public bool AddDepositHistory(Guid customerId, decimal amount, Guid createdById, Guid orderId, DepositType depositType,
            string customerReceipt, 
            string merchantReceipt,
            decimal oldBalance,
            decimal newBalance,
            Guid terminalId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var depositHistory = new DepositHistory();
                    depositHistory.TerminalId = terminalId;
                    depositHistory.CustomerId = customerId;
                    depositHistory.DepositAmount = amount;
                    depositHistory.OldBalance = oldBalance;
                    depositHistory.NewBalance = newBalance;
                    depositHistory.CreatedBy = createdById;
                    depositHistory.CreatedOn = DateTime.Now;
                    depositHistory.OrderId = orderId;
                    depositHistory.MerchantReceipt = merchantReceipt;
                    depositHistory.CustomerReceipt = customerReceipt;
                    depositHistory.DepositType = depositType;
                    db.DepositHistory.Add(depositHistory);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }



        public List<DepositHistoryViewModel> GetDepositHistory(Guid customerId)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var data = (from d in db.DepositHistory.Where(c => c.CustomerId == customerId)
                            join c in db.Customer on d.CustomerId equals c.Id
                            join o in db.OrderMaster on d.OrderId equals o.Id into joinedData
                            from joined in joinedData.DefaultIfEmpty()
                            select new DepositHistoryViewModel
                            {
                                Id = d.Id,
                                NewBalance = d.NewBalance,
                                OldBalance = d.OldBalance,
                                CreatedOn = d.CreatedOn,
                                CustomerName = c.Name,
                                OrderId = d.OrderId,
                                CustomerReceipt = d.CustomerReceipt,
                                OrderNumber = joined.OrderNoOfDay,
                                DepositType = d.DepositType,
                                DebitAmount = d.DepositType == DepositType.Debit ? d.DepositAmount.ToString() : "",
                                CreditAmount = d.DepositType != DepositType.Debit ? d.DepositAmount.ToString() : "",
                            }).OrderByDescending(a=>a.CreatedOn).ToList();
                return data;
            }
        }


        public void Dispose()
        {
            context.Dispose();
        }
    }
}
