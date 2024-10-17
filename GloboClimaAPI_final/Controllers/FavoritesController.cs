using GloboClimaAPI.Data;
using GloboClimaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GloboClimaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public FavoritesController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Método auxiliar para converter FavoriteModel para FavoriteData
        private FavoritesData ConvertToFavoriteData(FavoriteModel favoriteModel, string userId)
        {
            return new FavoritesData
            {
                CityName = favoriteModel.CityName,
                UserId = userId
            };
        }

        // Método auxiliar para converter FavoriteData para FavoriteModel
        private FavoriteModel ConvertToFavoriteModel(FavoritesData favoriteData)
        {
            return new FavoriteModel
            {
                CityName = favoriteData.CityName,
                UserId = favoriteData.UserId
            };
        }

        // Adiciona uma cidade aos favoritos
        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteModel favorite)
        {
            try
            {
                if (favorite == null || string.IsNullOrEmpty(favorite.CityName))
                {
                    return BadRequest("CityName is required.");
                }

                var userId = _context.Users
                    .Where(u => u.Id == favorite.UserId)
                    .Select(u => u.Id)
                    .FirstOrDefault();

                // Verifica se a cidade já está favoritada pelo usuário
                var existingFavorite = await _context.Favorites
                    .Where(f => f.CityName == favorite.CityName && f.UserId == userId)
                    .FirstOrDefaultAsync();

                if (existingFavorite != null)
                {
                    return Conflict($"City {favorite.CityName} is already in the favorites list.");
                }

                // Converte FavoriteModel para FavoriteData
                var newFavorite = ConvertToFavoriteData(favorite, userId);

                _context.Favorites.Add(newFavorite);
                await _context.SaveChangesAsync();

                // Retorna um FavoriteModel para o cliente
                var result = ConvertToFavoriteModel(newFavorite);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding the favorite.", Details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites([FromQuery] string token)
        {
            try
            {
                // Validar o token recebido como parâmetro
                var principal = ValidateToken(token);
                if (principal == null)
                {
                    return Unauthorized("Token inválido ou expirado.");
                }

                var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Unauthorized("User ID not found in token claims.");
                }

                var favorites = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Select(f => new { f.CityName })
                    .ToListAsync();

                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving favorites.", Details = ex.Message });
            }
        }


        // Método para validar o token JWT
        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Obter as configurações do token a partir do appsettings.json
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Reduzir tolerância de tempo para zero
            };

            try
            {
                // Validar o token e retornar o ClaimsPrincipal
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine("Token expirado.");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                Console.WriteLine("Assinatura do token inválida.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao validar o token: {ex.Message}");
                return null;
            }
        }

        // Remove uma cidade dos favoritos
        [HttpDelete("{cityName}")]
        public async Task<IActionResult> RemoveFavorite(string cityName, [FromQuery] string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found.");
                }

                var favorite = await _context.Favorites
                    .Where(f => f.CityName == cityName && f.UserId == userId)
                    .FirstOrDefaultAsync();

                if (favorite == null)
                {
                    return NotFound($"City {cityName} is not in the favorites list.");
                }

                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();

                return Ok($"City {cityName} removed from favorites.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while removing the favorite.", Details = ex.Message });
            }
        }
    }
}
