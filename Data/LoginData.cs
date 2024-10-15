using Microsoft.AspNetCore.Identity;

namespace GloboClimaAPI.Data
{
    public class LoginData : IdentityUser
    {
        public List<FavoriteData> Favorites { get; set; }
    }

}
