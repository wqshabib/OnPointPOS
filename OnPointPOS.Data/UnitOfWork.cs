using System;
using POSSUM.Model;

namespace POSSUM.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Outlet> OutletRepository { get; }
        IGenericRepository<Terminal> TerminalRepository { get; }
        IGenericRepository<Setting> SettingRepository { get; }
        IGenericRepository<TerminalStatusLog> TerminalStatusLogRepository { get; }
        IGenericRepository<CashDrawer> CashDrawerRepository { get; }
        IGenericRepository<CashDrawerLog> CashDrawerLogRepository { get; }
        IGenericRepository<CashdrawerActivity> CashdrawerActivityRepository { get; }
        IGenericRepository<Printer> PrinterRepository { get; }
        IGenericRepository<User> UserRepository { get; }
        IGenericRepository<UserLog> UserLogRepository { get; }
        IGenericRepository<InvoiceCounter> InvoiceCounterRepository { get; }
        IGenericRepository<BongCounter> BongCounterRepository { get; }
        IGenericRepository<Journal> JournalRepository { get; }
        IGenericRepository<JournalAction> JournalActionRepository { get; }
        IGenericRepository<Category> CategoryRepository { get; }
        IGenericRepository<ItemCategory> ItemCategoryRepository { get; }
        IGenericRepository<ProductPrice> ProductPriceRepository { get; }

        IGenericRepository<ProductCampaign> ItemCampaignRepository { get; }
        IGenericRepository<ProductGroup> ItemGroupRepository { get; }
        IGenericRepository<Campaign> CampaignRepository { get; }
        IGenericRepository<CategoryCampaign> CategoryCampaignRepository { get; } 

        IGenericRepository<OrderLine> OrderLineRepository { get; }
        IGenericRepository<Receipt> ReceiptRepository { get; }
        IGenericRepository<Report> ReportRepository { get; }
        IGenericRepository<ReportData> ReportDataRepository { get; }
        IGenericRepository<Payment> PaymentRepository { get; }
        IGenericRepository<PaymentType> PaymentTypeRepository { get; }
        IGenericRepository<VoucherTransaction> VoucherTransactionRepository { get; }
        IGenericRepository<Accounting> AccountingRepository { get; }
        IGenericRepository<Tax> TaxRepository { get; }

        IGenericRepository<Customer> CustomerRepository { get; }
        IGenericRepository<CustomerInvoice> CustomerInvoiceRepository { get; }

        IGenericRepository<Client> ClientRepository { get; }
        IGenericRepository<Employee> EmployeeRepository { get; }
        IGenericRepository<EmployeeLog> EmployeeLogRepository { get; }

        IGenericRepository<Floor> FloorRepository { get; }
        IGenericRepository<FoodTable> FoodTableRepository { get; }
        IGenericRepository<IconStore> IconStoreRepository { get; }
        IGenericRepository<InventoryTask> InventoryTaskRepository { get; }
        IGenericRepository<OutletUser> TillUserRepository { get; }
		IGenericRepository<ItemTransaction> ItemTransactionRepository { get; }
		IGenericRepository<Inbox> InboxRepository { get; }
		IGenericRepository<CustomerBonus> CustomerBonusRepository { get; }
        IGenericRepository<Product_PricePolicy> ProductPricePolicyRepository { get; }
        ProductRepository ProductRepository { get; set; }
        OrderRepository OrderRepository { get; set; }
       
        void Commit();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        //Repositories
        private readonly GenericRepository<Outlet> outletRepo;

        private readonly GenericRepository<Terminal> terminalRepo;
        private readonly GenericRepository<TerminalStatusLog> terminalStatusLogRepo;
        private readonly GenericRepository<CashDrawerLog> cashDrawerLogRepo;
        private readonly GenericRepository<CashdrawerActivity> cashdrawerActivityRepo;
        private readonly GenericRepository<CashDrawer> cashDrawerRepo;
        private readonly GenericRepository<Printer> printerRepo;
        private readonly GenericRepository<InvoiceCounter> invoiceCounterRepo;
        private readonly GenericRepository<BongCounter> bongCounterRepo;
        private readonly GenericRepository<Accounting> accountingRepo;
        private readonly GenericRepository<User> userRepo;
        private readonly GenericRepository<UserLog> userLogRepo;
        private readonly GenericRepository<ItemCategory> itemCategoryRepo;
        private readonly GenericRepository<ProductCampaign> itemCampaignRepo;
        private readonly GenericRepository<CategoryCampaign> CategoryCampaignRepo;  
        private readonly GenericRepository<Campaign> campaignRepo;
        private readonly GenericRepository<PricePolicy> pricePolicyRepo;
        private readonly GenericRepository<ProductGroup> itemGroupRepo;
		private readonly GenericRepository<ItemTransaction> itemTransactionRepo;

		private readonly GenericRepository<Category> categoryRepo;
        private readonly GenericRepository<Customer> customerRepo;
        private readonly GenericRepository<DepositHistory> depositHistoryRepo;
        private readonly GenericRepository<Customer_CustomField> customer_CustomFieldRepo;
        private readonly GenericRepository<CustomerInvoice> customerInvoiceRepo;
        private readonly GenericRepository<EmployeeLog> employeeLogRepo;
        private readonly GenericRepository<Employee> employeeRepo;
        private readonly GenericRepository<OrderLine> orderLineRepo;
        private readonly GenericRepository<Payment> paymentRepo;
        private readonly GenericRepository<PaymentType> paymentTypeRepo;
        private readonly GenericRepository<ProductPrice> productPriceRepo;
        private readonly GenericRepository<Product_PricePolicy> productPricePolicyRepo;
        
        private readonly GenericRepository<Product_Text> productTextRepo;
        private readonly GenericRepository<ZReportSetting> ZReportSettingRepo;
		private readonly GenericRepository<Inbox> inboxRepo; 

		private readonly GenericRepository<Receipt> receiptRepo;
        private readonly GenericRepository<Report> reportRepo;
        private readonly GenericRepository<ReportData> reportDataRepo;
        private readonly GenericRepository<VoucherTransaction> voucherTransactionRepo;

        private readonly GenericRepository<Setting> settingRepo;
        private readonly GenericRepository<Journal> journalRepo;
        private readonly GenericRepository<JournalAction> journalActionRepo;
        private readonly GenericRepository<Tax> taxRepo;
        private readonly GenericRepository<Floor> floorRepo;
        private readonly GenericRepository<FoodTable> foodTableRepo;
        private readonly GenericRepository<Client> clientRepo;
        private readonly GenericRepository<InventoryTask> inventoryTaskRepo;
        private readonly GenericRepository<IconStore> iconStoreRepo;
        private readonly GenericRepository<OutletUser> tillUserRepo;
        private readonly GenericRepository<CustomerBonus> customerBonusRepo;

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;

            outletRepo = new GenericRepository<Outlet>(context);
            terminalRepo = new GenericRepository<Terminal>(context);
            terminalStatusLogRepo = new GenericRepository<TerminalStatusLog>(context);
            cashDrawerRepo = new GenericRepository<CashDrawer>(context);
            cashDrawerLogRepo = new GenericRepository<CashDrawerLog>(context);
            cashdrawerActivityRepo = new GenericRepository<CashdrawerActivity>(context);
            printerRepo = new GenericRepository<Printer>(context);
            tillUserRepo = new GenericRepository<OutletUser>(context);

            categoryRepo = new GenericRepository<Category>(context);
            itemCategoryRepo = new GenericRepository<ItemCategory>(context);
            ProductRepository = new ProductRepository(context);
            campaignRepo = new GenericRepository<Campaign>(context);
            pricePolicyRepo = new GenericRepository<PricePolicy>(context);
            itemCampaignRepo = new GenericRepository<ProductCampaign>(context);
            CategoryCampaignRepo = new GenericRepository<CategoryCampaign>(context);
            itemGroupRepo = new GenericRepository<ProductGroup>(context);
            productPriceRepo = new GenericRepository<ProductPrice>(context);
            productPricePolicyRepo = new GenericRepository<Product_PricePolicy>(context);
            productTextRepo = new GenericRepository<Product_Text>(context);
            ZReportSettingRepo=new GenericRepository<ZReportSetting>(context);
            OrderRepository = new OrderRepository(context);
            orderLineRepo = new GenericRepository<OrderLine>(context);
            paymentRepo = new GenericRepository<Payment>(context);
            receiptRepo = new GenericRepository<Receipt>(context);
            paymentTypeRepo = new GenericRepository<PaymentType>(context);
            reportRepo = new GenericRepository<Report>(context);
            reportDataRepo = new GenericRepository<ReportData>(context);
            voucherTransactionRepo = new GenericRepository<VoucherTransaction>(context);

            settingRepo = new GenericRepository<Setting>(context);
            invoiceCounterRepo = new GenericRepository<InvoiceCounter>(context);
            bongCounterRepo = new GenericRepository<BongCounter>(context);
            customerInvoiceRepo = new GenericRepository<CustomerInvoice>(context);
            customerRepo = new GenericRepository<Customer>(context);
            depositHistoryRepo = new GenericRepository<DepositHistory>(context);
            clientRepo = new GenericRepository<Client>(context);
            employeeRepo = new GenericRepository<Employee>(context);
            employeeLogRepo = new GenericRepository<EmployeeLog>(context);

            iconStoreRepo = new GenericRepository<IconStore>(context);
            floorRepo = new GenericRepository<Floor>(context);
            foodTableRepo = new GenericRepository<FoodTable>(context);

            userRepo = new GenericRepository<User>(context);
            userLogRepo = new GenericRepository<UserLog>(context);
            inventoryTaskRepo = new GenericRepository<InventoryTask>(context);
            journalRepo = new GenericRepository<Journal>(context);
            journalActionRepo = new GenericRepository<JournalAction>(context);
            iconStoreRepo = new GenericRepository<IconStore>(context);
            taxRepo = new GenericRepository<Tax>(context);
            accountingRepo = new GenericRepository<Accounting>(context);
            customer_CustomFieldRepo = new GenericRepository<Customer_CustomField>(context);
			itemTransactionRepo = new GenericRepository<ItemTransaction>(context);

			inboxRepo = new GenericRepository<Inbox>(context);
            customerBonusRepo = new GenericRepository<CustomerBonus>(context);


        }

        #region IUnitOfWork Implementation

        public IGenericRepository<Terminal> TerminalRepository => terminalRepo;

        public IGenericRepository<Outlet> OutletRepository => outletRepo;

        public IGenericRepository<OrderLine> OrderLineRepository => orderLineRepo;
        public IGenericRepository<Customer> CustomerRepository => customerRepo;
        public IGenericRepository<DepositHistory> DepositHistoryRepository => depositHistoryRepo;
        public IGenericRepository<Customer_CustomField> Customer_CustomFieldRepository => customer_CustomFieldRepo;
        public IGenericRepository<Employee> EmployeeRepository => employeeRepo;
        public ProductRepository ProductRepository { get; }
        public IGenericRepository<Category> CategoryRepository => categoryRepo;
        public IGenericRepository<Setting> SettingRepository => settingRepo;
        public IGenericRepository<TerminalStatusLog> TerminalStatusLogRepository => terminalStatusLogRepo;
        public IGenericRepository<CashDrawer> CashDrawerRepository => cashDrawerRepo;
        public IGenericRepository<CashDrawerLog> CashDrawerLogRepository => cashDrawerLogRepo;
        public IGenericRepository<CashdrawerActivity> CashdrawerActivityRepository => cashdrawerActivityRepo;
        public IGenericRepository<Printer> PrinterRepository => printerRepo;
        public IGenericRepository<User> UserRepository => userRepo;
        public IGenericRepository<UserLog> UserLogRepository => userLogRepo;
        public IGenericRepository<InvoiceCounter> InvoiceCounterRepository => invoiceCounterRepo;
        public IGenericRepository<BongCounter> BongCounterRepository => bongCounterRepo;
        public IGenericRepository<Journal> JournalRepository => journalRepo;
        public IGenericRepository<JournalAction> JournalActionRepository => journalActionRepo;
        public IGenericRepository<ItemCategory> ItemCategoryRepository => itemCategoryRepo;
        public IGenericRepository<ProductCampaign> ItemCampaignRepository => itemCampaignRepo;
        public IGenericRepository<CategoryCampaign> CategoryCampaignRepository => CategoryCampaignRepo;    
        public IGenericRepository<ProductGroup> ItemGroupRepository => itemGroupRepo;
        public IGenericRepository<Campaign> CampaignRepository => campaignRepo;
        public IGenericRepository<PricePolicy> PricePolicyRepository => pricePolicyRepo;
        public IGenericRepository<Receipt> ReceiptRepository => receiptRepo;
        public IGenericRepository<Report> ReportRepository => reportRepo;
        public IGenericRepository<ReportData> ReportDataRepository => reportDataRepo;
        public IGenericRepository<Payment> PaymentRepository => paymentRepo;
        public IGenericRepository<PaymentType> PaymentTypeRepository => paymentTypeRepo;
        public IGenericRepository<VoucherTransaction> VoucherTransactionRepository => voucherTransactionRepo;
        public IGenericRepository<Accounting> AccountingRepository => accountingRepo;
        public IGenericRepository<Tax> TaxRepository => taxRepo;
        public IGenericRepository<CustomerInvoice> CustomerInvoiceRepository => customerInvoiceRepo;
        public IGenericRepository<Client> ClientRepository => clientRepo;
        public IGenericRepository<EmployeeLog> EmployeeLogRepository => employeeLogRepo;
        public IGenericRepository<Floor> FloorRepository => floorRepo;
        public IGenericRepository<FoodTable> FoodTableRepository => foodTableRepo;
        public IGenericRepository<IconStore> IconStoreRepository => iconStoreRepo;
        public IGenericRepository<InventoryTask> InventoryTaskRepository => inventoryTaskRepo;
        public IGenericRepository<OutletUser> TillUserRepository => tillUserRepo;
        public IGenericRepository<ProductPrice> ProductPriceRepository => productPriceRepo;
        public IGenericRepository<Product_PricePolicy> ProductPricePolicyRepository => productPricePolicyRepo;
        public IGenericRepository<Product_Text> ProductTextRepository => productTextRepo;
        public IGenericRepository<ZReportSetting> ZReportSettingRepository => ZReportSettingRepo;
        public IGenericRepository<Inbox> InboxRepository => inboxRepo;
        public IGenericRepository<CustomerBonus> CustomerBonusRepository => customerBonusRepo;
        public OrderRepository OrderRepository { get; }

		public IGenericRepository<ItemTransaction> ItemTransactionRepository => itemTransactionRepo;

		ProductRepository IUnitOfWork.ProductRepository
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        OrderRepository IUnitOfWork.OrderRepository
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Commit()
        {
            this.context.SaveChanges();
        }

        public void Dispose()
        {
            this.context.Dispose();
        }

        #endregion IUnitOfWork Implementation
    }
}