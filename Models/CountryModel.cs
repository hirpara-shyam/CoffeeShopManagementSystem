using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Models
{
    public class CountryModel
    {
        public int CountryID {  get; set; }

        [Required]
        [DisplayName("Country Name")]
        public string CountryName { get; set; }

        [Required]
        [DisplayName("State Code")]
        public string CountryCode { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int StateCount { get; set; }
    }

    public class CountryDropDownModel
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
    }
}
