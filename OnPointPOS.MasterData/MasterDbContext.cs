using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.MasterData
{
    public class MasterDbContext : IdentityDbContext<MasterApplicationUser>
    {
        public MasterDbContext()
            : base("DefaultConnection")
        {
        }

        public static MasterDbContext Create()
        {
            return new MasterDbContext();
        }
        public DbSet<Company> Company { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<Contract> Contract { get; set; }
        public DbSet<AdminOutlet> Outlet { get; set; }
        public DbSet<AdminTerminal> Terminal { get; set; }
        public DbSet<OutletUser> OutletUser { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUser>().ToTable("Users", "dbo");
            modelBuilder.Entity<MasterApplicationUser>().ToTable("Users", "dbo");
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

        }
    }
}
