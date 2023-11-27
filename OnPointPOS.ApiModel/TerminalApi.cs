using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class TerminalApi
    {
        public Guid Id { get; set; }
        public Guid OutletId { get; set; }
        public int TerminalNo { get; set; }
        public Guid TerminalType { get; set; }
        public string UniqueIdentification { get; set; }
        public string HardwareAddress { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int RootCategoryId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string CCUData { get; set; }
        public bool AutoLogin { get; set; }
        public string ApplicationNameAndVersion { get; set; }
        public CCUDataResponse CCUDataResponse { get; set; }
        public CashDrawerApi CashDrawer { get; set; }

        public static TerminalApi ConvertModelToApiModel(Terminal terminal)
        {
            return new TerminalApi
            {
                Id = terminal.Id,
                OutletId = terminal.OutletId,
                TerminalNo = terminal.TerminalNo,
                TerminalType = terminal.TerminalType,
                UniqueIdentification = terminal.UniqueIdentification,
                HardwareAddress = terminal.HardwareAddress,
                Description = terminal.Description,
                Status = (int) terminal.Status,
                RootCategoryId = terminal.RootCategoryId,
                IsDeleted = terminal.IsDeleted,
                Created = terminal.Created,
                Updated = terminal.Updated,
                CCUData = terminal.CCUData,
                AutoLogin = terminal.AutoLogin
            };
        }
    }
}
