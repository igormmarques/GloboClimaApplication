using GloboClimaAPI.Data;
using GloboClimaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GloboClimaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        // Método auxiliar para converter FavoriteModel para FavoriteData
        private FavoriteData ConvertToFavoriteData(FavoriteModel favoriteModel, string userId)
        {
            return new FavoriteData
            {
                CityName = favoriteModel.CityName,
                UserId = userId
            };
        }

        // Método auxiliar para converter FavoriteData para FavoriteModel
        private FavoriteModel ConvertToFavoriteModel(FavoriteData favoriteData)
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
                    .Where(u => u.UserName == favorite.UserId)
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

        // Obtém os favoritos do usuário
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            try
            {
                // Obtém o userId diretamente das claims do token JWT
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token claims.");
                }

                // Busca as cidades favoritas no banco de dados
                var favorites = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.CityName)
                    .ToListAsync();

                if (!favorites.Any())
                {
                    return Ok("No favorites added yet.");
                }

                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving favorites.", Details = ex.Message });
            }
        }


        // Remove uma cidade dos favoritos
        [HttpDelete("{cityName}")]
        public async Task<IActionResult> RemoveFavorite(string cityName)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Unauthorized("User ID not found in token claims.");
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
