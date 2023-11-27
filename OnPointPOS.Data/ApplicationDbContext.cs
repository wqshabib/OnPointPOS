using Microsoft.AspNet.Identity.EntityFramework;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class ApplicationUser : IdentityUser
    {
        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //    // Add custom user claims here
        //    return userIdentity;
        //}
        public string Password { get; set; }
        public bool TrainingMode { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string DallasKey { get; set; }
    }



    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("LocalConnection")
        {
            //#if DEBUG
            //            //perform initialization
            //          //  Database.SetInitializer<ApplicationDbContext>(null);
            //#else
            Database.SetInitializer<ApplicationDbContext>(null);
            //#endif
        }

        public ApplicationDbContext(string connectionString) : base(connectionString)
        {
            this.Database.Connection.ConnectionString = connectionString;
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        //, throwIfV1Schema: false
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Booking> Booking { get; set; }
        public DbSet<BookingArea> BookingArea { get; set; }
        public DbSet<BookingCategory> BookingCategory { get; set; }
        public DbSet<BookingSpot> BookingSpot { get; set; }
        public DbSet<Terminal> Terminal { get; set; }
        public DbSet<Outlet> Outlet { get; set; }
        public DbSet<CashDrawer> CashDrawer { get; set; }
        public DbSet<CashDrawerLog> CashDrawerLog { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<TerminalStatusLog> TerminalStatusLog { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<ReportData> ReportData { get; set; }
        public DbSet<ItemCategory> ItemCategory { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductGroup> ProductGroup { get; set; }
        public DbSet<ItemStock> ItemStock { get; set; }

        public DbSet<Accounting> Accounting { get; set; }
        public DbSet<Order> OrderMaster { get; set; }
        public DbSet<OrderLine> OrderDetail { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Receipt> Receipt { get; set; }
        public DbSet<Setting> Setting { get; set; }
        public DbSet<ZReportSetting> ZReportSetting { get; set; }
        public DbSet<OutletUser> OutletUser { get; set; }
        public DbSet<UserLog> UserLog { get; set; }
        public DbSet<Printer> Printer { get; set; }
        public DbSet<Campaign> Campaign { get; set; }
        public DbSet<ProductCampaign> ProductCampaign { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeLog> EmployeeLog { get; set; }
        public DbSet<ExceptionLog> ExceptionLog { get; set; }
        public DbSet<FoodTable> FoodTable { get; set; }
        public DbSet<Floor> Floor { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<CustomerBonus> CustomerBonus { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<CustomerInvoice> CustomerInvoice { get; set; }
        public DbSet<VoucherTransaction> VoucherTransaction { get; set; }
        public DbSet<PaymentType> PaymentType { get; set; }
        public DbSet<IconStore> IconStore { get; set; }
        public DbSet<Journal> Journal { get; set; }
        public DbSet<JournalAction> JournalAction { get; set; }
        public DbSet<BongCounter> BongCounter { get; set; }
        public DbSet<InvoiceCounter> InvoiceCounter { get; set; }
        public DbSet<Tax> Tax { get; set; }
        public DbSet<InventoryHistory> InventoryHistory { get; set; }
        public DbSet<InventoryTask> InventoryTask { get; set; }
        public DbSet<ProductPrice> ProductPrice { get; set; }
        public DbSet<Product_Text> Product_Text { get; set; }
        public DbSet<Product_PricePolicy> Product_PricePolicy { get; set; }
        public DbSet<PricePolicy> PricePolicy { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<PaymentDeviceLog> PaymentDeviceLog { get; set; }
        public DbSet<CustomerCustomField> CustomerCustomField { get; set; }
        public DbSet<Customer_CustomField> Customer_CustomField { get; set; }
        public DbSet<CustomerDiscountGroup> CustomerDiscountGroup { get; set; }
        public DbSet<CustomerCard> CustomerCard { get; set; }
        public DbSet<DiscountGroup> DiscountGroup { get; set; }
        public DbSet<ItemTransaction> ItemTransaction { get; set; }
        public DbSet<ItemInventory> ItemInventory { get; set; }
        public DbSet<Inbox> Inbox { get; set; }
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<WarehouseLocation> WarehouseLocation { get; set; }
        public DbSet<MQTTClient> MQTTClient { get; set; }
        public DbSet<MQTTBuffer> MQTTBuffer { get; set; }
        public DbSet<TablesSyncLog> TablesSyncLog { get; set; }
        //public DbSet<PantProduct> PantProduct { get; set; }
        public DbSet<DepositHistory> DepositHistory { get; set; }
        public DbSet<ProductIngredients> ProductIngredients { get; set; }
        public DbSet<CategoryCampaign> CategoryCampaign { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<StockHistoryGroup> StockHistoryGroup { get; set; }
        public DbSet<ProductStockHistory> ProductStockHistory { get; set; }
        public DbSet<UserOrder> UserOrder { get; set; }
        public DbSet<SwishPayment> SwishPayment { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUser>().ToTable("Users", "dbo");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users", "dbo");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UsersInRoles", "dbo");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins", "dbo");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims", "dbo");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "dbo");

            modelBuilder.Entity<IdentityUser>().Ignore(c => c.AccessFailedCount)
                                         .Ignore(c => c.LockoutEnabled)
                                         .Ignore(c => c.LockoutEndDateUtc)
                                         .Ignore(c => c.PhoneNumberConfirmed)
                                         .Ignore(c => c.EmailConfirmed)
                                         .Ignore(c => c.TwoFactorEnabled);
            modelBuilder.Entity<IdentityUser>().Property(x => x.PhoneNumber).HasMaxLength(20);
            modelBuilder.Entity<IdentityUser>().Property(x => x.Email).HasMaxLength(256);
            modelBuilder.Entity<IdentityUser>().Property(x => x.PasswordHash).HasMaxLength(70);
            modelBuilder.Entity<IdentityUser>().Property(x => x.SecurityStamp).HasMaxLength(40);

            modelBuilder.Entity<Order>().Property(x => x.OrderTotal).HasPrecision(18, 10);
            modelBuilder.Entity<Order>().Property(x => x.RoundedAmount).HasPrecision(8, 3);
            modelBuilder.Entity<OrderLine>().Property(x => x.UnitPrice).HasPrecision(8, 2);
            modelBuilder.Entity<OrderLine>().Property(x => x.PurchasePrice).HasPrecision(8, 2);
            modelBuilder.Entity<OrderLine>().Property(x => x.DiscountedUnitPrice).HasPrecision(8, 2);
            modelBuilder.Entity<OrderLine>().Property(x => x.DiscountPercentage).HasPrecision(8, 2);
            modelBuilder.Entity<OrderLine>().Property(x => x.ItemDiscount).HasPrecision(18, 10);
            modelBuilder.Entity<Payment>().Property(x => x.PaidAmount).HasPrecision(18, 10);
            modelBuilder.Entity<Payment>().Property(x => x.CashCollected).HasPrecision(8, 2);
            modelBuilder.Entity<Payment>().Property(x => x.CashChange).HasPrecision(8, 2);
            modelBuilder.Entity<Payment>().Property(x => x.ReturnAmount).HasPrecision(8, 2);
            modelBuilder.Entity<Payment>().Property(x => x.TipAmount).HasPrecision(8, 2);
            modelBuilder.Entity<Receipt>().Property(x => x.GrossAmount).HasPrecision(8, 2);
            modelBuilder.Entity<OrderLine>().Property(x => x.Quantity).HasPrecision(18, 10);
            modelBuilder.Entity<Product>().Property(x => x.Weight).HasPrecision(18, 4);


            modelBuilder.Entity<Category>().Property(p => p.Id)
   .HasDatabaseGeneratedOption(System.ComponentModel
   .DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<ProductCampaign>().Property(p => p.Id)
  .HasDatabaseGeneratedOption(System.ComponentModel
  .DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<ProductGroup>().Property(p => p.Id)
.HasDatabaseGeneratedOption(System.ComponentModel
.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<InvoiceCounter>().Property(p => p.Id)
.HasDatabaseGeneratedOption(System.ComponentModel
.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<Campaign>().Property(p => p.Id)
.HasDatabaseGeneratedOption(System.ComponentModel
.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<Accounting>().Property(p => p.Id)
.HasDatabaseGeneratedOption(System.ComponentModel
.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<Setting>().Property(p => p.Id)
.HasDatabaseGeneratedOption(System.ComponentModel
.DataAnnotations.Schema.DatabaseGeneratedOption.None);

            modelBuilder.Entity<PaymentType>().Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel
                    .DataAnnotations.Schema.DatabaseGeneratedOption.None);

            modelBuilder.Entity<Client>().Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel
                    .DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<Tax>().Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel
                    .DataAnnotations.Schema.DatabaseGeneratedOption.None);


            modelBuilder.Entity<Floor>().Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel
                    .DataAnnotations.Schema.DatabaseGeneratedOption.None);

            modelBuilder.Entity<FoodTable>().Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel
                    .DataAnnotations.Schema.DatabaseGeneratedOption.None);

            modelBuilder.Entity<JournalAction>().Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel
                    .DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<CategoryCampaign>().Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel
                    .DataAnnotations.Schema.DatabaseGeneratedOption.None);

        }
    }
}
