using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class OutletUserApi
    {
        public string Id { get; set; }
        public Guid OutletId { get; set; }
        public string Email { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool TrainingMode { get; set; }
        public bool Active { get; set; }
        public string DallasKey { get; set; }
        public DateTime? Updated { get; set; }

        public static OutletUserApi ConvertModelToApiModel(OutletUser outletUser)
        {
            return new OutletUserApi
            {
                Id = outletUser.Id,
                OutletId = outletUser.OutletId,
                Email = outletUser.Email,
                UserCode = outletUser.UserCode,
                UserName = outletUser.UserName,
                Password = outletUser.Password,
                TrainingMode = outletUser.TrainingMode,
                Active = outletUser.Active,
                DallasKey = outletUser.DallasKey,
                Updated = outletUser.Updated
            };
        }
    }
}
