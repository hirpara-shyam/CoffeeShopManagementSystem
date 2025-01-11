using System;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Models
{
    public class OrderDetailModel
    {
        public int OrderDetailID { get; set; }

        [Required(ErrorMessage = "Order id should not be empty.")]
        public int? OrderID { get; set; }

        [Required(ErrorMessage = "Product id should not be empty.")]
        public int? ProductID { get; set; }

        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Quantity should not be empty.")]
        [Range(1,10)]
        public int? Quantity { get; set; }

        [Required(ErrorMessage = "Amount should not be empty.")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Total Amount should not be empty.")]
        public decimal? TotalAmount { get; set; }

        [Required(ErrorMessage = "User id should not be empty.")]
        public int? UserID { get; set; }

        public string? UserName { get; set; }
    }
}
