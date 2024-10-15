using GloboClimaAPI.Models;
using GloboClimaAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GloboClimaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<LoginData> _userManager;

        public UsersController(UserManager<LoginData> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new LoginData
                    {
                        UserName = model.Username,
                        Email = model.Email
                    };

                    // Criptografar a senha e criar o usuário
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        return Ok("User registered successfully.");
                    }

                    // Caso tenha erros de validação (como senha fraca)
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                // Tratar exceção e retornar erro apropriado
                return StatusCode(500, new { Message = "An error occurred during registration.", Details = ex.Message });
            }
        }
    }
}
