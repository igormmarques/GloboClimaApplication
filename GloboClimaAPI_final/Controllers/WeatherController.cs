using GloboClimaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Threading.Tasks;

[Authorize]
public class WeatherController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private void AddJwtToClient(HttpClient client)
    {
        // Recupera o token da sessão em vez de cookies
        var token = HttpContext.Session.GetString("JWTToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    [HttpGet]
    public async Task<IActionResult> WeatherAndFavorites()
    {
        var model = new WeatherViewModel
        {
            FavoriteCities = await GetFavoriteCitiesAsync(),
            WeatherInfo = null
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> GetWeatherInfo(WeatherViewModel model)
    {
        if (!string.IsNullOrEmpty(model.CityName))
        {
            var weatherInfo = await FetchWeatherFromApi(model.CityName);
            if (weatherInfo != null)
            {
                model.WeatherInfo = weatherInfo;
            }
            else
            {
                ModelState.AddModelError("", "Cidade não encontrada ou houve um erro na busca.");
            }
        }

        model.FavoriteCities = await GetFavoriteCitiesAsync();
        return View("WeatherAndFavorites", model);
    }

    private async Task<WeatherInfo> FetchWeatherFromApi(string cityName)
    {
        var client = _httpClientFactory.CreateClient();
        AddJwtToClient(client);  // Adiciona o token no cabeçalho

        var apiKey = _configuration["WeatherApiSettings:APIKey"];
        var baseUrl = _configuration["WeatherApiSettings:BaseUrl"];

        var response = await client.GetAsync($"{baseUrl}weather?q={cityName}&appid={apiKey}&units=metric");

        if (response.IsSuccessStatusCode)
        {
            var weatherData = await response.Content.ReadFromJsonAsync<WeatherInfo>();
            return weatherData;
        }

        return null;
    }

    private async Task<List<string>> GetFavoriteCitiesAsync()
    {
        var client = _httpClientFactory.CreateClient();
        AddJwtToClient(client);  // Adiciona o token no cabeçalho

        var baseUrl = _configuration["FavoritesApiSettings:BaseUrl"];
        var response = await client.GetAsync($"{baseUrl}Favorites");

        if (response.IsSuccessStatusCode)
        {
            var favoriteCities = await response.Content.ReadFromJsonAsync<List<string>>();
            return favoriteCities ?? new List<string>();
        }

        return new List<string>();
    }
}
