namespace POSSUM.Data.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using POSSUM.Model;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    internal sealed class Configuration : DbMigrationsConfiguration<POSSUM.Data.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            try
            {
                /*
               var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));


               IdentityResult roleResult;

               // Check to see if Role Exists, if not create it

               if (!RoleManager.RoleExists("Admin"))
               {
                   roleResult = RoleManager.Create(new IdentityRole("Admin"));
               }
               if (!RoleManager.RoleExists("Content Manager"))
               {
                   roleResult = RoleManager.Create(new IdentityRole("Content Manager"));
               }

               var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
               if (manager.FindByName("admin") == null)
               {
                   var user = new ApplicationUser() { UserName = "admin", Active = true, Email = "admin@luqon.com", PhoneNumber = "03435478889", Created=DateTime.Now, Password="admin@123" };
                   manager.Create(user, "admin@123");
                   manager.AddToRole(user.Id, "Admin");
               }


                               context.Category.AddOrUpdate(
                                p => p.Name,
                                new Category { Id = 1, Name = "Root Category", Type = CategoryType.Root, Created = DateTime.Now, Updated = DateTime.Now }

                              );
                               context.Outlet.AddOrUpdate(
                                p => p.Id,
                                new Outlet { Id = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Name = "POS SUM", Active = true, City = "Göteborg", Created = DateTime.Now, Email = "info@possum.com", Phone = "", OrgNo = "720512-1093", PostalCode = "411 03", Address1 = "Vasagatan 25", FooterText = "Thanks and welcome again!", HeaderText = "POSSUM", UniqueCode = "SUM-1", WebUrl = "www.POSSUM.com", TaxDescription = "SE123456789", Updated = DateTime.Now }

                              );
                               context.Terminal.AddOrUpdate(
                               p => p.Id,
                               new Terminal { Id = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), UniqueIdentification = "Demo001", Created = DateTime.Now, RootCategoryId = 1, Updated = DateTime.Now }

                             );
                               context.CashDrawer.AddOrUpdate(
                              p => p.Id,
                              new CashDrawer { Id = Guid.Parse("BCC2EA00-AFD2-41C0-AFE8-315FE4569904"), TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), Location = "Main Terminal", Name = "Kassalåda 1"  }

                            );

                              context.PaymentType.AddOrUpdate(
                              p => p.Name,
                              new PaymentType { Id = 0, Name = "Free Coupon", SwedishName = "Gratis kupong", AccountingCode = 1909, Updated = DateTime.Now },
                              new PaymentType { Id = 1, Name = "Paid by Cash", SwedishName = "Betales kontant", AccountingCode = 1910, Updated = DateTime.Now },
                              new PaymentType { Id = 2, Name = "On Credit", SwedishName = "på Credit", AccountingCode = 1911, Updated = DateTime.Now },
                              new PaymentType { Id = 3, Name = "Paid By Gift", SwedishName = "Betalt av kupong", AccountingCode = 1912, Updated = DateTime.Now },
                              new PaymentType { Id = 4, Name = "Paid By Credit Card", SwedishName = "Betalt med kredittkort", AccountingCode = 1930, Updated = DateTime.Now },
                              new PaymentType { Id = 5, Name = "Paid By Debit Card", SwedishName = "Betalt av debetkort", AccountingCode = 1931, Updated = DateTime.Now },
                              new PaymentType { Id = 6, Name = "Paid By Cheque", SwedishName = "Betalt med sjekk", AccountingCode = 1932, Updated = DateTime.Now },
                              new PaymentType { Id = 7, Name = "Cash Back", SwedishName = "Tillbaka", AccountingCode = 1933, Updated = DateTime.Now },
                              new PaymentType { Id = 8, Name = "Returned", SwedishName = "Returnerad", AccountingCode = 1914, Updated = DateTime.Now },
                              new PaymentType { Id = 9, Name = "Mobile Card", SwedishName = "Mobil korterminal", AccountingCode = 1910, Updated = DateTime.Now },
                              new PaymentType { Id = 10, Name = "Swish", SwedishName = "Swish", AccountingCode = 1908, Updated = DateTime.Now },
                              new PaymentType { Id = 11, Name = "Elve Card", SwedishName = "Elevkort", AccountingCode = 1907, Updated = DateTime.Now },
                              new PaymentType { Id = 12, Name = "Credit Note", SwedishName = "Tillgodokvitto", AccountingCode = 1906, Updated = DateTime.Now },
                              new PaymentType { Id = 13, Name = "Beam", SwedishName = "beam", AccountingCode = 1905, Updated = DateTime.Now },
                              new PaymentType { Id = 14, Name = "Paid By AMEX  Card", SwedishName = "Betalt med AMEX  kort", AccountingCode = 1514 , Updated=DateTime.Now},
                              new PaymentType { Id = 15, Name = "Online Cash", SwedishName = "Online Kontant", AccountingCode = 1515, Updated = DateTime.Now }
                            );

                            context.Setting.AddOrUpdate(
                                t => t.Id,
                                new Setting { Id = Convert.ToInt16(SettingCode.SyncAPIURL), Description="LIVE DATA SYNC URL",Type=SettingType.ExternalSettings, Code = SettingCode.SyncAPIURL,Value = "http://api.stageshop.app01.luqon.com/",TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"),OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"),Created=DateTime.Now, Updated = DateTime.Now},
                                new Setting { Id = Convert.ToInt16(SettingCode.APIUSER), Description = "Sync API USER", Type = SettingType.ExternalSettings, Code = SettingCode.APIUSER, Value = "posum", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.APIPassword), Description = "Sync API PASSWORD", Type = SettingType.ExternalSettings, Code = SettingCode.APIPassword, Value = "112233", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },                    
                                new Setting { Id = Convert.ToInt16(SettingCode.ShowExternalOrder), Description = "SHOW EXTERNAL ORDER", Type = SettingType.ExternalSettings, Code = SettingCode.ShowExternalOrder, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ExternalOrderAPI), Description = "EXTERNAL ORDER API URL", Type = SettingType.ExternalSettings, Code = SettingCode.ExternalOrderAPI, Value = "www.api.com", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ExternalAPIUSER), Description = "EXTERNAL ORDER API USER", Type = SettingType.ExternalSettings, Code = SettingCode.ExternalAPIUSER, Value = "abc", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ExternalAPIPassword), Description = "EXTERNAL API PASSWORD", Type = SettingType.ExternalSettings, Code = SettingCode.ExternalAPIPassword, Value = "abc123", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                 new Setting { Id = Convert.ToInt16(SettingCode.ExternalOrderMqttService), Description = "EXTERNAL ORDER MQTT URL", Type = SettingType.ExternalSettings, Code = SettingCode.ExternalOrderMqttService, Value = "mqtt.gw.westbahr.net", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.Restaurant_group_id), Description = "RESTAURANT GROUP ID", Type = SettingType.ExternalSettings, Code = SettingCode.Restaurant_group_id, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.Restaurant_id), Description = "RESTAURANT ID", Type = SettingType.ExternalSettings, Code = SettingCode.Restaurant_id, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.EnableMqtt), Description = "ENABLE MQTT", Type = SettingType.ExternalSettings, Code = SettingCode.EnableMqtt, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.M2MqttService), Description = "M2MQTT URL", Type = SettingType.ExternalSettings, Code = SettingCode.M2MqttService, Value = "mqtt.gw.westbahr.net", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.Last_Executed), Description = "LAST EXECUTION DATE", Type = SettingType.ExternalSettings, Code = SettingCode.Last_Executed, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },

                                new Setting { Id = Convert.ToInt16(SettingCode.ControlUnitType), Description = "CONTROL UNIT TYPE", Type = SettingType.ExternalSettings, Code = SettingCode.ControlUnitType, Value = "DUMMY", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ControlUnitConnectionString), Description = "CONTROL UNIT CONECTION STRING", Type = SettingType.ExternalSettings, Code = SettingCode.ControlUnitConnectionString, Value = "COM04:9600,DTR", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CashDrawerType), Description = "CASHDRAWER TYPE", Type = SettingType.HardwareSettings, Code = SettingCode.CashDrawerType, Value = "DIRECT_HARDWARE_48C", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CashDrawerHardwarePort), Description = "CASHDRAWER PORT", Type = SettingType.HardwareSettings, Code = SettingCode.CashDrawerHardwarePort, Value = "1164", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CASH_GUARD), Description = "ENABLE CASHGUARD", Type = SettingType.HardwareSettings, Code = SettingCode.CASH_GUARD, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CASH_GuardPort), Description = "CASHGUARD PORT", Type = SettingType.HardwareSettings, Code = SettingCode.CASH_GuardPort, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.PaymentDeviceType), Description = "PAYMENT DEVICE TYPE", Type = SettingType.HardwareSettings, Code = SettingCode.PaymentDeviceType, Value = "BABS_BPTI", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.PaymentDevicConnectionString), Description = "PAYENT DEVICE CONNECTION", Type = SettingType.HardwareSettings, Code = SettingCode.PaymentDevicConnectionString, Value = "babstcp://127.0.0.1:2000", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ScaleType), Code = SettingCode.ScaleType, Description = "SCALE TYPE", Type = SettingType.HardwareSettings, Value = "DUMMY", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.SCALEPORT), Description = "SCALE PORT", Type = SettingType.HardwareSettings, Code = SettingCode.SCALEPORT, Value = "COM1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.PosIdType), Description = "POS ID FOR CU", Type = SettingType.HardwareSettings, Code = SettingCode.PosIdType, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DebugCleanCash), Description = "ENABLE CLEAN CASH LOG", Type = SettingType.HardwareSettings, Code = SettingCode.DebugCleanCash, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ReplacePosIdSpecialChars), Description = "REMOVE - FROM POSID", Type = SettingType.HardwareSettings, Code = SettingCode.ReplacePosIdSpecialChars, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },


                                new Setting { Id = Convert.ToInt16(SettingCode.Language), Description = "CASHDRAWER LANGUAGE", Type = SettingType.TerminalSettings, Code = SettingCode.Language, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.Currency), Description = "CASHDRAWER PORT", Type = SettingType.TerminalSettings, Code = SettingCode.Currency, Value = "sv-SE", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CurrencySymbol), Description = "CASHDRAWER PORT", Type = SettingType.TerminalSettings, Code = SettingCode.CurrencySymbol, Value = "kr", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.SaleType), Description = "SALE TYPE", Type = SettingType.TerminalSettings, Code = SettingCode.SaleType, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CategoryLines), Description = "SHOW CATEGORY LINE", Type = SettingType.TerminalSettings, Code = SettingCode.CategoryLines, Value = "2", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ItemLines), Description = "SHOW ITEMS LINES", Type = SettingType.TerminalSettings, Code = SettingCode.ItemLines, Value = "4", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.TableView), Description = "TABLE VIEW", Type = SettingType.TerminalSettings, Code = SettingCode.TableView, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.Takeaway), Description = "SHOW TAKEAWAY BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.Takeaway, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DirectCash), Description = "SHOW DIRECT CASH BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.DirectCash, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DirectCard), Description = "SHOW DIRECT CARD BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.DirectCard, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },                    
                                new Setting { Id = Convert.ToInt16(SettingCode.ShowSwishButton), Description = "SHOW DIRECT SWISH BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.ShowSwishButton, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ShowStudentCardButton), Description = "SHOW DIRECT STUDENT CARD BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.ShowStudentCardButton, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CustomerView), Description = "SHOW CUSTOMER VIEW", Type = SettingType.TerminalSettings, Code = SettingCode.CustomerView, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CustomerOrderInfo), Description = "ADD CUSTOMER INFO TO ORDER", Type = SettingType.TerminalSettings, Code = SettingCode.CustomerOrderInfo, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                 new Setting { Id = Convert.ToInt16(SettingCode.CreditNote), Description = "SHOW CREDIT NOTE BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.CreditNote, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.BeamPayment), Description = "SHOW BEAM PAYMENT BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.BeamPayment, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.EmployeeLog), Description = "EMPLOYEE LOG", Type = SettingType.TerminalSettings, Code = SettingCode.EmployeeLog, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DigitOnly), Description = "ENABLE ONLY DIGIT IN TEXTBOX", Type = SettingType.TerminalSettings, Code = SettingCode.DigitOnly, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.ShowPrice), Description = "SHOW PRICE ON ITEM BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.ShowPrice, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.HidePaymentButton), Description = "HIDE PAYMENT BUTTON", Type = SettingType.TerminalSettings, Code = SettingCode.HidePaymentButton, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },


                                new Setting { Id = Convert.ToInt16(SettingCode.LogoEnable), Description = "ENABLE PRINT LOGO", Type = SettingType.PrintSettings, Code = SettingCode.LogoEnable, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },                    
                                new Setting { Id = Convert.ToInt16(SettingCode.ShowBongAlert), Description = "SHOW ALERT BEFORE BONG", Type = SettingType.PrintSettings, Code = SettingCode.ShowBongAlert, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.BongNormalFont), Description = "SET BONG FONT NORMAL", Type = SettingType.PrintSettings, Code = SettingCode.BongNormalFont, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.BONG), Description = "PRINT BONG", Type = SettingType.PrintSettings, Code = SettingCode.BONG, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.BongByProduct), Description = "PRINT BONG BY PRODUCT", Type = SettingType.PrintSettings, Code = SettingCode.BongByProduct, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                 new Setting { Id = Convert.ToInt16(SettingCode.TableNeededOnBong), Description = "SHOW TABLE ON BONG", Type = SettingType.PrintSettings, Code = SettingCode.TableNeededOnBong, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DailyBongCounter), Description = "ENABLE DAILY BONG COUNTER", Type = SettingType.PrintSettings, Code = SettingCode.DailyBongCounter, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.BongCounter), Description = "ENABLE BONG COUNTER", Type = SettingType.PrintSettings, Code = SettingCode.BongCounter, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.MultiKitchen), Description = "ENABLE MULTI KICHEN", Type = SettingType.PrintSettings, Code = SettingCode.MultiKitchen, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.OrderNoOnBong), Description = "SHOW ORDER NO ON BONG", Type = SettingType.PrintSettings, Code = SettingCode.OrderNoOnBong, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },

                                 new Setting { Id = Convert.ToInt16(SettingCode.IsClient), Description = "TERMINAL MODE CLIENT OR SERVER", Type = SettingType.MiscSettings, Code = SettingCode.IsClient, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DallasKey), Description = "ENABLE DALASS KEY LOGIN", Type = SettingType.MiscSettings, Code = SettingCode.DallasKey, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },                   
                                new Setting { Id = Convert.ToInt16(SettingCode.NightStartHour), Description = "Night Mode Start Hour", Type = SettingType.MiscSettings, Code = SettingCode.NightStartHour, Value = "8", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.NightEndHour), Description = "Night Mode End hour", Type = SettingType.MiscSettings, Code = SettingCode.NightEndHour, Value = "14", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DualPriceMode), Description = "ENABLE Dual Price Mode", Type = SettingType.MiscSettings, Code = SettingCode.DualPriceMode, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },                   
                                new Setting { Id = Convert.ToInt16(SettingCode.RunningMode), Description = "SET RUNNING MODE", Type = SettingType.MiscSettings, Code = SettingCode.RunningMode, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },                    
                                new Setting { Id = Convert.ToInt16(SettingCode.DeviceLog), Description = "ENABLE PAYMENT DEVICE LOGING", Type = SettingType.MiscSettings, Code = SettingCode.DeviceLog, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.PricePolicy), Description = "ENABLE PRICE POLICy", Type = SettingType.MiscSettings, Code = SettingCode.PricePolicy, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.EnableExternalNetworking), Description = "ENABLE EXTERNALE NEWTWORKING", Type = SettingType.MiscSettings, Code = SettingCode.EnableExternalNetworking, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.CustomerId), Description = "Set Customer ID", Type = SettingType.MiscSettings, Code = SettingCode.CustomerId, Value = "00000000-0000-0000-0000-000000000000", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.SlideShowURL), Description = "CUSTOMER VIEW SLIDE URL", Type = SettingType.MiscSettings, Code = SettingCode.SlideShowURL, Value = "C://POSSUM/Index.html", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },      
                                new Setting { Id = Convert.ToInt16(SettingCode.InvoiceDueDays), Description = "INVOICE DUE DAYS", Type = SettingType.MiscSettings, Code = SettingCode.InvoiceDueDays, Value = "10", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.DiscountCode), Description = "ENABLE ASK DISCOUNT CODE", Type = SettingType.MiscSettings, Code = SettingCode.DiscountCode, Value = "", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.EnableCheckoutLog), Description = "ENABLE CHECKOUT LOG", Type = SettingType.MiscSettings, Code = SettingCode.EnableCheckoutLog, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.OrderEntryType), Description = "ENABLE ORDER ENTRY", Type = SettingType.MiscSettings, Code = SettingCode.OrderEntryType, Value = "1", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.AccountNumber), Description = "SET Account Number", Type = SettingType.MiscSettings, Code = SettingCode.AccountNumber, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.PaymentReceiver), Description = "Payment Receiver Name", Type = SettingType.MiscSettings, Code = SettingCode.PaymentReceiver, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now },
                                new Setting { Id = Convert.ToInt16(SettingCode.FakturaReference), Description = "Invoice Reference", Type = SettingType.MiscSettings, Code = SettingCode.FakturaReference, Value = "0", TerminalId = Guid.Parse("1764D338-BDFC-45A3-80AB-8EC47D3294A6"), OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA"), Created = DateTime.Now, Updated = DateTime.Now }



                                );

                           string password = CalculateSHA1("123456", Encoding.UTF8);
                           string password1 = CalculateSHA1("123456", Encoding.UTF8);

                           context.OutletUser.AddOrUpdate(
                            p => p.UserName,
                            new OutletUser { UserName = "Christer",  UserCode = "111", Email = "christer@possum.com", Password = password, Active = true, Id = "A546B2AC-C834-4E5C-8062-886EC6FCBDDE", TrainingMode = false, Updated = DateTime.Now, OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA") },
                            new OutletUser { UserName = "Shahid",  UserCode = "100", Email = "shahid@possum.com", Password = password1, Active = true, Id = "8D0DAFB5-DADC-4D1D-8677-4F0B44043A60", TrainingMode = false, Updated = DateTime.Now, OutletId = Guid.Parse("2B35687A-C363-4A40-95F1-D5A8E859C7BA") }


                          ); 

                               context.SaveChanges();
                               */
            }
            catch (Exception ex)
            {

            }
        }
        private static string CalculateSHA1(string text, Encoding enc)
        {
            try
            {
                byte[] buffer = enc.GetBytes(text);
                var cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
                return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
