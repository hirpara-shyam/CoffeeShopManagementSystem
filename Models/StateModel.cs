using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementSystem.Models
{
    public class StateModel
    {
        public int? StateID { get; set; }

        [Required]
        [DisplayName("State Name")]
        public string StateName { get; set; }

        [Required]
        [DisplayName("Country Name")]
        public int CountryID { get; set; }

        public string? CountryName { get; set; }

        [Required]
        [DisplayName("State Code")]
        public string StateCode { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public int CityCount { get; set; }
    }

    public class StateDropDownModel
    {
        public int StateID { get; set; }
        public string StateName { get; set; }
    }
}
