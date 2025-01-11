using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Models
{
    public class OrderModel
    {
        public int OrderID { get; set; }


        [Required(ErrorMessage = "Order date should not be empty.")]
        public DateTime? OrderDate { get; set; }

        [Required(ErrorMessage = "CustomerID should not be empty.")]
        public int? CustomerID { get; set; }

        public string? CustomerName { get; set; }

        [Required(ErrorMessage = "Payment mode should not be empty.")]
        public string PaymentMode { get; set; } = null;

        [Required(ErrorMessage = "Total Amount should not be empty.")]
        public decimal? TotalAmount { get; set; }

        [Required(ErrorMessage = "Shipping Address should not be empty.")]
        public string ShippingAddress {  get; set; }

        [Required(ErrorMessage = "UserID should not be empty.")]
        public int? UserID { get; set; }

        public string? UserName { get; set; }
    }

    public class OrderDropDownModel
    {
        public int OrderID { get; set; }
    }
}
