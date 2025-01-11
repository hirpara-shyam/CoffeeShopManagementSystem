using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Models
{
    public class BillsModel
    {
        public int BillID { get; set; }

        [Required(ErrorMessage = "Bill number should not be empty.")]
        public string BillNumber { get; set; }

        [Required(ErrorMessage = "Bill date should not be empty.")]
        public DateTime? BillDate { get; set; }

        [Required(ErrorMessage = "Order id should not be empty.")]
        public int? OrderID { get; set; }

        [Required(ErrorMessage = "Total Amount should not be empty.")]
        public decimal? TotalAmount { get; set; }

        [Required(ErrorMessage = "Discount should not be empty.")]
        [Range(0,25)]
        public decimal? Discount { get; set; }

        [Required(ErrorMessage = "Net amount should not be empty.")]
        public decimal? NetAmount { get; set; }

        [Required(ErrorMessage = "User id should not be empty.")]
        public int? UserID { get; set; }

        public string? UserName { get; set; }
    }
}
