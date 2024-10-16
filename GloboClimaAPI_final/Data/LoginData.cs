using Microsoft.AspNetCore.Identity;

namespace GloboClimaAPI.Data
{
    public class LoginData : IdentityUser
    {
        public List<FavoritesData> Favorites { get; set; }
    }

}
