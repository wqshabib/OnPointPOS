using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace POSSUM.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }
    public class UserViewModel : User
    {
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string[] SelectedIds { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Roles { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Companies { get; set; }

        public Guid CompanyId { get; set; }

        public string RoleId { get; set; }
    }
    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {        
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
    public class RoleViewModel
    {
        public string UserId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        public string asc { get; set; }
    }

    public class ResetPasswordRequestViewModel
    {
        [Required]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }


    public class RegisterMasterUserViewModel
    {
        [Required]
        [Display(Name = "User Name")]
        //[System.Web.Mvc.Remote("doesUserNameExist", "Account", HttpMethod = "POST", ErrorMessage = "User name already exists. Please enter a different user name.")]
        [System.Web.Mvc.Remote("IsUserExists", "Account", ErrorMessage = "User Name already in use")]
        public string UserName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        
        // [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        // [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Email { get; set; }


      

        public string UserId { get; set; }

        public string Id { get; set; }

        public string asc { get; set; }

      

        public string PhoneNumber { get; set; }
        public string RoleId { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Roles { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Companies { get; set; }

        public bool Active { get; set; }

        public Guid CompanyId { get; set; }

    }
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "User Code")]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "{0} must be a Number.")]
        //[System.Web.Mvc.Remote("doesUserNameExist", "Account", HttpMethod = "POST", ErrorMessage = "User name already exists. Please enter a different user name.")]
        [System.Web.Mvc.Remote("IsUserExists", "Account", ErrorMessage = "User Name already in use")]
        public string UserName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "{0} must be a Number.")]
        // [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        // [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Email { get; set; }

       
        public int UserCode { get; set; }

        public string UserId { get; set; }

        public string Id { get; set; }
        
        public string asc { get; set; }

        public bool TrainingMode { get; set; }

        public string PhoneNumber { get; set; }
        public string RoleId { get;  set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Roles { get; set; }

        public bool Active { get; set; }
        public string Company { get; set; }
     
             
    }
    public class ProfileViewModel
    {

        public ProfileViewModel()
        {
            Activated = true;
        }
        public int Id { get; set; }
        public string FaceBookId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Nick Name")]
        public string NickName { get; set; }
        public string Email { get; set; }

        public bool Activated { get; set; }

        [DataType(DataType.ImageUrl)]
        public string ProfilePic { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        [Display(Name = "Date Of Birth")]
        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }
        [Display(Name = "Mobile No")]
        public string MobileNo { get; set; }

        public string UserId { get; set; }




    }

}
