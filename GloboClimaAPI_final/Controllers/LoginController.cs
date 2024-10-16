using GloboClimaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using GloboClimaAPI.Data;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace GloboClimaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly PasswordHasher<LoginData> _passwordHasher;

        public LoginController(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
            _passwordHasher = new PasswordHasher<LoginData>(); // Inicializa o PasswordHasher
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginModel credentials)
        {
            try
            {
                // Verificar se as credenciais estão corretas
                if (credentials == null || string.IsNullOrEmpty(credentials.Username) || string.IsNullOrEmpty(credentials.Password))
                {
                    return BadRequest("Username and password are required.");
                }

                // Validando o usuário no banco de dados
                var user = _context.Users.FirstOrDefault(u => u.UserName == credentials.Username);

                if (user == null)
                {
                    return Unauthorized("Invalid login attempt.");
                }

                // Verificando se a senha corresponde à hash no banco de dados
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, credentials.Password);

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                }

                return Unauthorized("Invalid login attempt.");
            }
            catch (Exception ex)
            {
                // Logar a exceção (você pode integrar com um serviço de log)
                return StatusCode(500, new { Message = "An error occurred during login.", Details = ex.Message });
            }
        }

        private JwtSecurityToken GenerateJwtToken(LoginData user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SigningKey"]));
            var credentialsSigning = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName)
    };

            // Ajuste aqui o tempo de expiração do token para mais tempo, por exemplo 60 minutos
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),  // Aumente o tempo se necessário
                signingCredentials: credentialsSigning);

            return token;
        }
    }
}
