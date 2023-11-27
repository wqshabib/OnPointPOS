using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class CashDrawerApi
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string UserId { get; set; }
        public Guid TerminalId { get; set; }
        public string ConnectionString { get; set; }

        public static CashDrawerApi ConvertModelToApiModel(CashDrawer cashDrawer)
        {
            return new CashDrawerApi
            {
                Id = cashDrawer.Id,
                Name = cashDrawer.Name,
                Location = cashDrawer.Location,
                UserId = cashDrawer.UserId,
                TerminalId = cashDrawer.TerminalId,
                ConnectionString = cashDrawer.ConnectionString
            };
        }
    }
}
