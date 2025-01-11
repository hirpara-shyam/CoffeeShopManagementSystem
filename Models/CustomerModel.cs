using System;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Models
{
    public class CustomerModel
    {
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Customer Name should not be empty.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Address should not be empty.")]
        public string HomeAddress { get; set; }

        [Required(ErrorMessage = "Email should not be empty.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number should not be empty.")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "GST number should not be empty.")]
        public string GSTNo { get; set; }

        [Required(ErrorMessage = "City name should not be empty.")]
        public string CityName { get; set; }

        [Required(ErrorMessage = "Pincode should not be empty.")]
        public string PinCode { get; set; }

        [Required(ErrorMessage = "Net Amount should not be empty.")]
        public decimal? NetAmount { get; set; }

        [Required(ErrorMessage = "UserID should not be empty.")]
        public int? UserID { get; set; }

        public string? UserName { get; set; }
    }

    public class CustomerDropDownModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
    }
}
