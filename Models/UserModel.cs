using System;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Models
{
    public class UserModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Username should not be empty.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email should not be empty.")]
        [EmailAddress]
        public string Email { get; set;}

        [Required(ErrorMessage = "Password should not be empty.")]
        public string Password { get; set;}

        [Required(ErrorMessage = "Mobile number should not be empty.")]
        [MaxLength(10)]
        [MinLength(10)]
        public string MobileNo { get; set;}

        [Required(ErrorMessage = "Address should not be empty.")]
        public string Address { get; set;}

        [Required(ErrorMessage = "IsActive should not be empty.")]
        public bool? IsActive { get; set; }
    }

    public class UserDropDownModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }
}
