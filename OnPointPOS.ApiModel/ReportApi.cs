using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class ReportApi
    {
        public Guid Id { get; set; }
        public DateTime CreationDate { get; set; }
        public int ReportType { get; set; }
        public int ReportNumber { get; set; }
        public Guid TerminalId { get; set; }

        public static ReportApi ConvertModelToApiModel(Report report)
        {
            return new ReportApi
            {
                Id = report.Id,
                CreationDate = report.CreationDate,
                ReportType = report.ReportType,
                ReportNumber = report.ReportNumber,
                TerminalId = report.TerminalId,
            };
        }

        public static Report ConvertApiModelToModel(ReportApi report)
        {
            return new Report
            {
                Id = report.Id,
                CreationDate = report.CreationDate,
                ReportType = report.ReportType,
                ReportNumber = report.ReportNumber,
                TerminalId = report.TerminalId
            };
        }

        public static Report UpdateModel(Report dbObject, ReportApi report)
        {
            dbObject.CreationDate = report.CreationDate;
            dbObject.ReportType = report.ReportType;
            dbObject.ReportNumber = report.ReportNumber;
            dbObject.TerminalId = report.TerminalId;

            return dbObject;
        }
    }
}
