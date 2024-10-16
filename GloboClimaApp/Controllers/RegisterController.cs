using GloboClimaApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

public class RegisterController : Controller
{
    private readonly HttpClient _httpClient;

    public RegisterController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:44337");
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(UserRegistrationModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/Users/Register", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Sucesso no cadastro, redirecionar para a tela de login
            return RedirectToAction("Index", "Login");
        }
        else
        {
            // Cadastro inválido, exibir mensagem de erro
            ModelState.AddModelError(string.Empty, "Erro ao realizar o cadastro.");
            return View(model);
        }
    }
}
