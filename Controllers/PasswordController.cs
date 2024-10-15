using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GloboClimaAPI.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GloboClimaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<LoginData> _passwordHasher;

        public PasswordController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<LoginData>(); // Inicializa o PasswordHasher
        }

        // Endpoint para atualizar a senha de um usuário específico
        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordModel model)
        {
            // Busca o usuário pelo nome de usuário
            var user = _context.Users.FirstOrDefault(u => u.UserName == model.Username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Gera o hash da nova senha
            var hashedPassword = _passwordHasher.HashPassword(user, model.NewPassword);

            // Atualiza o campo PasswordHash no banco de dados
            user.PasswordHash = hashedPassword;

            // Salva as mudanças no banco de dados
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Password for user '{model.Username}' updated successfully." });
        }
    }

    // Modelo para receber o nome de usuário e a nova senha
    public class UpdatePasswordModel
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
    }
}
