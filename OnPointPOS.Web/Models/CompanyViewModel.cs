using POSSUM.MasterData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class CompanyViewModel:Company
    {
    }
    public class ContactViewModel : Contact
    {
    }
    public class ContractViewModel : Contract
    {
        public IEnumerable<SelectListItem> StatusList
        {
            get
            {
                var enumType = typeof(ContractStatus);
                var values = Enum.GetValues(enumType).Cast<ContractStatus>();

                var converter = TypeDescriptor.GetConverter(enumType);

                return
                    from value in values
                    select new SelectListItem
                    {
                        Text = converter.ConvertToString(value),
                        Value = value.ToString(),
                    };
            }
        }
    }
}