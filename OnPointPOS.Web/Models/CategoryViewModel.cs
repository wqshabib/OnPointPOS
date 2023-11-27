using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Models
{
    public class CategoryViewModel:Category
    {
        public bool IsSelected { get; set; }
        public string asc { get; set; }
      
        public List<CategoryViewModel> Children { get; set; }

        public SeededCategories SeedCategories { get; set; }

      

        public string Edit { get; set; }
        public string Delete { get; set; }

        public IEnumerable<SelectListItem> Icons { get; set; }

        public string ActiveStatus
        {
            get
            {
                return Active == true ? Resource.Active : Resource.Inactive;
            }
        }
    }

    public class SeededCategories
    {
        public int? Seed { get; set; }
        public IList<ItemCategoryViewModel> Categories { get; set; }
    }

}