using Microsoft.AspNetCore.Identity;

namespace GloboClimaAPI.Data
{
    public class LoginData
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public List<FavoritesData> Favorites { get; set; }
    }

}
