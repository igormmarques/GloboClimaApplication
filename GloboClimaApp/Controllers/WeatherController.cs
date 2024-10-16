using GloboClimaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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

    // Método para adicionar o JWT ao cliente Http
    private bool AddJwtToClient(HttpClient client)
    {
        var token = HttpContext.Session.GetString("JWTToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }
        return false;
    }


    [HttpGet]
    public async Task<IActionResult> WeatherAndFavorites()
    {
        var client = _httpClientFactory.CreateClient();
        if (!AddJwtToClient(client))
        {
            // Redireciona para login se o token não estiver presente
            return RedirectToAction("Index", "Login");
        }

        var model = new WeatherViewModel
        {
            FavoriteCities = await GetFavoriteCitiesAsync(client),
            WeatherInfo = null
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> GetWeatherInfo(WeatherViewModel model)
    {
        var client = _httpClientFactory.CreateClient();
        if (!AddJwtToClient(client))
        {
            return RedirectToAction("Index", "Login");
        }

        if (!string.IsNullOrEmpty(model.CityName))
        {
            var weatherInfo = await FetchWeatherFromApi(client, model.CityName);
            if (weatherInfo != null)
            {
                model.WeatherInfo = weatherInfo;
            }
            else
            {
                ModelState.AddModelError("", "Cidade não encontrada ou houve um erro na busca.");
            }
        }

        model.FavoriteCities = await GetFavoriteCitiesAsync(client);
        return View("WeatherAndFavorites", model);
    }

    [HttpPost]
    public async Task<IActionResult> AddToFavorites(string cityName, string userId)
    {
        var client = _httpClientFactory.CreateClient();
        if (!AddJwtToClient(client))
        {
            return RedirectToAction("Index", "Login");
        }

        if (!string.IsNullOrEmpty(cityName))
        {
            await AddOrRemoveCityFromFavoritesAsync(client, cityName, userId);
        }

        return RedirectToAction("WeatherAndFavorites");
    }

    // Método para buscar informações do clima de uma cidade
    private async Task<WeatherInfo> FetchWeatherFromApi(HttpClient client, string cityName)
    {
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

    // Método para obter cidades favoritas
    private async Task<List<string>> GetFavoriteCitiesAsync(HttpClient client)
    {
        var baseUrl = _configuration["FavoritesApiSettings:BaseUrl"];

        // Problema a resolver.
        if (!AddJwtToClient(client))
        {
            Console.WriteLine("Token JWT não encontrado.");
            return new List<string>();
        }

        // Fazer a requisição para a API de favoritos
        var response = await client.GetAsync($"{baseUrl}favorites");

        if (response.IsSuccessStatusCode)
        {
            var favoriteCities = await response.Content.ReadFromJsonAsync<List<string>>();
            return favoriteCities ?? new List<string>();
        }

        return new List<string>();
    }


    private async Task AddOrRemoveCityFromFavoritesAsync(HttpClient client, string cityName, string userId)
    {
        var favorite = new { CityName = cityName, UserId = userId };
        var baseUrl = _configuration["FavoritesApiSettings:BaseUrl"];
        var endpointUrl = $"{baseUrl.TrimEnd('/')}/favorites";

        var response = await client.PostAsJsonAsync(endpointUrl, favorite);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Cidade {cityName} adicionada aos favoritos.");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var deleteUrl = $"{baseUrl.TrimEnd('/')}/favorites/{cityName}";
            var deleteResponse = await client.DeleteAsync(deleteUrl);

            if (deleteResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Cidade {cityName} removida dos favoritos.");
            }
            else
            {
                Console.WriteLine($"Erro ao tentar remover a cidade {cityName} dos favoritos.");
            }
        }
        else
        {
            Console.WriteLine($"Erro: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Login");
    }


}
