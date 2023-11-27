using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class SettingApi
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public SettingType Type { get; set; }
        public SettingCode Code { get; set; }
        public string Value { get; set; }
        public Guid TerminalId { get; set; }
        public Guid OutletId { get; set; }
        public int Sort { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public static SettingApi ConvertModelToApiModel(Setting setting)
        {
            return new SettingApi
            {
                Id = setting.Id,
                Description = setting.Description,
                Type = setting.Type,
                Code = setting.Code,
                Value = setting.Value,
                TerminalId = setting.TerminalId,
                OutletId = setting.OutletId,
                Sort = setting.Sort,
                Created = setting.Created,
                Updated = setting.Updated,
            };
        }

        public static Setting ConvertApiModelToModel(SettingApi setting)
        {
            return new Setting
            {
                Id = setting.Id,
                Description = setting.Description,
                Type = setting.Type,
                Code = setting.Code,
                Value = setting.Value,
                TerminalId = setting.TerminalId,
                OutletId = setting.OutletId,
                Sort = setting.Sort,
                Created = setting.Created,
                Updated = setting.Updated,
            };
        }

        public static Setting UpdateModel(Setting dbObject, SettingApi setting)
        {
            dbObject.Description = setting.Description;
            dbObject.Type = setting.Type;
            dbObject.Code = setting.Code;
            dbObject.Value = setting.Value;
            dbObject.TerminalId = setting.TerminalId;
            dbObject.OutletId = setting.OutletId;
            dbObject.Sort = setting.Sort;
            dbObject.Created = setting.Created;
            dbObject.Updated = setting.Updated;

            return dbObject;
        }
    }
}