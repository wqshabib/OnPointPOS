using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
   // [Serializable]
    public class CashDrawer:BaseEntity
    {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Location { get; set; }
        public virtual string UserId { get; set; }
        public virtual Guid TerminalId { get; set; }
        public virtual string ConnectionString { get; set; }
        [NotMapped]
        public virtual IList<CashDrawerLog> Logs { get; set; }
        public CashDrawer()
        {
            Logs = new List<CashDrawerLog>();
        }

        public virtual void OpenCashDrawer(string userId)
        {
            Logs.Add(
                new CashDrawerLog() 
                { 
                    Id=Guid.NewGuid(),
                    ActivityDate=DateTime.Now,
                    ActivityType= DrawerActivityType.CashBySale,
                    Amount=0,
                    CashDrawer=this,
                    UserId= userId
                }
                );
        }

        public virtual void SetOpeningBalance(string userId, decimal amount)
        {
            Logs.Add(
                new CashDrawerLog()
                {
                    Id = Guid.NewGuid(),
                    ActivityDate = DateTime.Now,
                    ActivityType = DrawerActivityType.OpeningBalance,
                    Amount = amount,
                    CashDrawer = this,
                    UserId = userId
                }
                );
        }

        public virtual void SetCashAdded(string userId, decimal amount)
        {
            Logs.Add(
                new CashDrawerLog()
                {
                    Id = Guid.NewGuid(),
                    ActivityDate = DateTime.Now,
                    ActivityType = DrawerActivityType.CashAdded,
                    Amount = amount,
                    CashDrawer = this,
                    UserId = userId
                }
                );
        }
        public virtual void SetCashDropped(string userId, decimal amount)
        {
            Logs.Add(
                new CashDrawerLog()
                {
                    Id = Guid.NewGuid(),
                    ActivityDate = DateTime.Now,
                    ActivityType = DrawerActivityType.CashDrop,
                    Amount = amount,
                    CashDrawer = this,
                    UserId = userId
                }
                );
        }


        public virtual void OpenCashDrawerClosing(string userId)
        {
            Logs.Add(
                new CashDrawerLog()
                {
                    Id = Guid.NewGuid(),
                    ActivityDate = DateTime.Now,
                    ActivityType = DrawerActivityType.Close,
                    Amount = 0,
                    CashDrawer = this,
                    UserId = userId
                }
                );
        }

        public virtual void OpenCashDrawerSale(decimal amount, string userId)
        {
            Logs.Add(
                new CashDrawerLog()
                {
                    Id = Guid.NewGuid(),
                    ActivityDate = DateTime.Now,
                    ActivityType = DrawerActivityType.CashBySale,
                    Amount = amount,
                    CashDrawer = this,
                    UserId = userId
                }
                );
        }
    }
}
