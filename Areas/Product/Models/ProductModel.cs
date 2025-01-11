using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Areas.Product.Models
{
    public class ProductModel
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product Name should not be Empty.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Price should be given.")]
        public decimal? ProductPrice { get; set; }

        [Required(ErrorMessage = "Product Code should not be Empty.")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Product Description should not be Empty.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "UserID should not be Empty.")]
        public int? UserID { get; set; }

        public string? UserName { get; set; }
    }

    public class ProductDropDownModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
    }
}
