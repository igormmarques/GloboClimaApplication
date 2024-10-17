using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GloboClimaAPI.Data
{
    public class FavoritesData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string CityName { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public LoginData User { get; set; }
    }
}
