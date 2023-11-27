namespace POSSUM.MasterData.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<POSSUM.MasterData.MasterDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(POSSUM.MasterData.MasterDbContext context)
        {

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));


            IdentityResult roleResult;

            // Check to see if Role Exists, if not create it

            if (!RoleManager.RoleExists("Admin"))
            {
                roleResult = RoleManager.Create(new IdentityRole("Admin"));
            }
            if (!RoleManager.RoleExists("Super Admin"))
            {
                roleResult = RoleManager.Create(new IdentityRole("Super Admin"));
            }
            Guid companyId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA");
            context.Company.AddOrUpdate(
                p => p.Id,
                new Company { Id = companyId, Name = "POS SUM", Active = true, City = "Göteborg",  PostalCode = "411 03", Address = "Vasagatan 25",Country="Sweden", DBName="POSSUM",DBServer=".",DBUser="sa",DBPassword="sql2k16", AdminURL="www.possum.com" }

              );

            var manager = new UserManager<MasterApplicationUser>(new UserStore<MasterApplicationUser>(context));
            if (manager.FindByName("admin") == null)
            {
                var user = new MasterApplicationUser() { UserName = "admin", Active = true, Email = "admin@luqon.com", PhoneNumber = "03435478889",CompanyId= companyId };
                manager.Create(user, "admin@123");
                manager.AddToRole(user.Id, "Super Admin");
            }

            context.SaveChanges();

        }
    }
}
