using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class ItemCampaignViewModel: ProductCampaign
    {
        public IEnumerable<SelectListItem> Campaigns { get; set; } 

    }

    public class CategoryCampaignViewModel : CategoryCampaign
    {
        public IEnumerable<SelectListItem> Campaigns { get; set; }
    }
}