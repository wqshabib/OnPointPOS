using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.ApiModel
{
    public class PaymentTypeApi
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SwedishName { get; set; }
        public int AccountingCode { get; set; }
        public DateTime Updated { get; set; }

        public static PaymentTypeApi ConvertModelToApiModel(PaymentType paymentType)
        {
            return new PaymentTypeApi
            {
                Id = paymentType.Id,
                Name = paymentType.Name,
                SwedishName = paymentType.SwedishName,
                AccountingCode = paymentType.AccountingCode,
                Updated = paymentType.Updated
            };
        }
    }
}