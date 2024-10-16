using GloboClimaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

public class LoginController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public LoginController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;

        // Obtém a URL base da API do appsettings.json
        var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/Login", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Ler o token JWT da resposta
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseModel>();

            // Armazenar o token na sessão para ser reutilizado nas próximas requisições
            HttpContext.Session.SetString("JWTToken", tokenResponse.Token);

            // O token será utilizado para requisições posteriores através da sessão
            return RedirectToAction("WeatherAndFavorites", "Weather");
        }
        else
        {
            // Login inválido, exibir mensagem de erro
            ModelState.AddModelError(string.Empty, "Login ou senha inválidos.");
            return View(model);
        }
    }
}

// Model para capturar o token JWT da resposta
public class TokenResponseModel
{
    public string Token { get; set; }
}
