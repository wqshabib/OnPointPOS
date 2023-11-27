using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace POSSUM.Web.Models
{
    public class OutletUserViewModel : OutletUser
    {

        [Display(Name = "Confirm new password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Outlets { get; set; }
        public bool IsSelected { get; set; }
        public string OutletName { get; set; }

        public string asc { get; set; }

        public OutletUser GetFrom()
        {
            return new OutletUser
            {
                Id = Id,
                UserCode = UserCode,
                UserName = UserName,
                Password = Password,
				Email=Email,
				DallasKey=DallasKey,
                Active = Active,
                OutletId = OutletId,
                TrainingMode = TrainingMode                
            };
        }

		public string Status { get { return Active == true ? Resource.Active : Resource.Inactive; } }
	}
}