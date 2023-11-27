using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class ReportDataApi
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public string DataType { get; set; }
        public string TextValue { get; set; }
        public int? ForeignId { get; set; }
        public decimal? Value { get; set; }
        public decimal? TaxPercent { get; set; }
        public DateTime? DateValue { get; set; }
        public int SortOrder { get; set; }

        public static ReportDataApi ConvertModelToApiModel(ReportData reportData)
        {
            return new ReportDataApi
            {
                Id = reportData.Id,
                ReportId = reportData.ReportId,
                DataType = reportData.DataType,
                TextValue = reportData.TextValue,
                ForeignId = reportData.ForeignId,
                Value = reportData.Value,
                TaxPercent = reportData.TaxPercent,
                DateValue = reportData.DateValue,
                SortOrder = reportData.SortOrder
            };
        }

        public static ReportData ConvertApiModelToModel(ReportDataApi reportData)
        {
            return new ReportData
            {
                Id = reportData.Id,
                ReportId = reportData.ReportId,
                DataType = reportData.DataType,
                TextValue = reportData.TextValue,
                ForeignId = reportData.ForeignId,
                Value = reportData.Value,
                TaxPercent = reportData.TaxPercent,
                DateValue = reportData.DateValue,
                SortOrder = reportData.SortOrder
            };
        }

        public static ReportData UpdateModel(ReportData dbObject, ReportDataApi reportData)
        {
            dbObject.ReportId = reportData.ReportId;
            dbObject.DataType = reportData.DataType;
            dbObject.TextValue = reportData.TextValue;
            dbObject.ForeignId = reportData.ForeignId;
            dbObject.Value = reportData.Value;
            dbObject.TaxPercent = reportData.TaxPercent;
            dbObject.DateValue = reportData.DateValue;
            dbObject.SortOrder = reportData.SortOrder;

            return dbObject;
        }
    }
}
