using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class CashDrawerLogApi
    {
        public Guid Id { get; set; }
        public Guid CashDrawerId { get; set; }
        public DateTime ActivityDate { get; set; }
        public DrawerActivityType ActivityType { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public Guid OrderId { get; set; }
        public int Synced { get; set; }

        public static CashDrawerLogApi ConvertModelToApiModel(CashDrawerLog cashDrawerLog)
        {
            return new CashDrawerLogApi
            {
                Id = cashDrawerLog.Id,
                CashDrawerId = cashDrawerLog.CashDrawerId,
                ActivityDate = cashDrawerLog.ActivityDate,
                ActivityType = cashDrawerLog.ActivityType,
                UserId = cashDrawerLog.UserId,
                Amount = cashDrawerLog.Amount,
                OrderId = cashDrawerLog.OrderId,
                Synced = cashDrawerLog.Synced,
            };
        }

        public static CashDrawerLog ConvertApiModelToModel(CashDrawerLogApi cashDrawerLog)
        {
            return new CashDrawerLog
            {
                Id = cashDrawerLog.Id,
                CashDrawerId = cashDrawerLog.CashDrawerId,
                ActivityDate = cashDrawerLog.ActivityDate,
                ActivityType = cashDrawerLog.ActivityType,
                UserId = cashDrawerLog.UserId,
                Amount = cashDrawerLog.Amount,
                OrderId = cashDrawerLog.OrderId,
                Synced = cashDrawerLog.Synced,
            };
        }

        public static CashDrawerLog UpdateModel(CashDrawerLog dbObject, CashDrawerLogApi cashDrawerLog)
        {
            dbObject.CashDrawerId = cashDrawerLog.CashDrawerId;
            dbObject.ActivityDate = cashDrawerLog.ActivityDate;
            dbObject.ActivityType = cashDrawerLog.ActivityType;
            dbObject.UserId = cashDrawerLog.UserId;
            dbObject.Amount = cashDrawerLog.Amount;
            dbObject.OrderId = cashDrawerLog.OrderId;
            dbObject.Synced = cashDrawerLog.Synced;

            return dbObject;
        }
    }
}