using System.ComponentModel.DataAnnotations;

namespace GloboClimaAPI.Models
{
    public class FavoriteModel
    {
        [Required(ErrorMessage = "City name is required.")]
        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
        public string CityName { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; }
    }
}
