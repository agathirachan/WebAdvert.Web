using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Models.Accounts
{
    public class SignupModel
    {
        [Required]
        [EmailAddress]
        [Display(Name ="Email")]
        public string Email { get; set; }
       
        [Required]
        [DataType(DataType.Password)]
        [StringLength(50, ErrorMessage ="Password must be at least 6 characters long.",MinimumLength =6)]
        [Display(Name = "Password")]
        public string Password { get; set; }
       
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="Password and its confirmation do not match")]
        public string ConfirmPassword { get; set; }
    }
}
