using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.ApiModel
{
    public class TerminalStatusLogApi
    {
        public Guid Id { get; set; }
        public Guid TerminalId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string UserId { get; set; }
        public Guid ReportId { get; set; }
        public Terminal.TerminalStatus Status { get; set; }
        public int Synced { get; set; }

        public static TerminalStatusLogApi ConvertModelToApiModel(TerminalStatusLog terminalStatusLog)
        {
            return new TerminalStatusLogApi
            {
                Id = terminalStatusLog.Id,
                TerminalId = terminalStatusLog.TerminalId,
                ActivityDate = terminalStatusLog.ActivityDate,
                UserId = terminalStatusLog.UserId,
                ReportId = terminalStatusLog.ReportId,
                Status = terminalStatusLog.Status,
                Synced = terminalStatusLog.Synced
            };
        }

        public static TerminalStatusLog ConvertApiModelToModel(TerminalStatusLogApi terminalStatusLog)
        {
            return new TerminalStatusLog
            {
                Id = terminalStatusLog.Id,
                TerminalId = terminalStatusLog.TerminalId,
                ActivityDate = terminalStatusLog.ActivityDate,
                UserId = terminalStatusLog.UserId,
                ReportId = terminalStatusLog.ReportId,
                Status = terminalStatusLog.Status,
                Synced = terminalStatusLog.Synced
            };
        }

        public static TerminalStatusLog UpdateModel(TerminalStatusLog dbObject, TerminalStatusLogApi terminalStatusLog)
        {
            dbObject.TerminalId = terminalStatusLog.TerminalId;
            dbObject.ActivityDate = terminalStatusLog.ActivityDate;
            dbObject.UserId = terminalStatusLog.UserId;
            dbObject.ReportId = terminalStatusLog.ReportId;
            dbObject.Status = terminalStatusLog.Status;
            dbObject.Synced = terminalStatusLog.Synced;

            return dbObject;
        }
    }
}
