using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GloboClimaAPI.Data
{
    public class FavoritesData
    {
        [Key]  // Marca a propriedade como chave primária
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Geração automática do ID
        public int Id { get; set; }

        public string CityName { get; set; }
        public string UserId { get; set; }  // Identificação do usuário no banco de dados

        // Esta relação pode ser usada, mas não é necessária para o DTO
        [ForeignKey("UserId")]
        public LoginData User { get; set; }
    }
}
