using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GloboClimaBlazor.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string _token;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Método para autenticar e armazenar o token JWT
        public async Task<bool> LoginAsync(string username, string password)
        {
            var credentials = new { Username = username, Password = password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://localhost:44337/api/Login", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    // Supondo que a resposta seja um JSON contendo o token
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(jsonResponse);
                    _token = jsonDoc.RootElement.GetProperty("token").GetString(); // Ajuste o caminho conforme necessário

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer login: {ex.Message}");
            }

            return false;
        }

        // Método para registrar um novo usuário
        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            var credentials = new { Username = username, Password = password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://localhost:44337/api/Users/Register", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Log ou manipular erros específicos
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erro ao registrar usuário: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Tratamento de exceção
                Console.WriteLine($"Exceção ao registrar usuário: {ex.Message}");
                return false;
            }
        }

        // Método para pegar os favoritos do usuário
        public async Task<List<string>> GetFavoritesAsync()
        {
            var response = await _httpClient.GetAsync("https://localhost:44337/api/Favorites");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<List<string>>(json);
                }
            }
            return new List<string>();
        }

        // Método para adicionar uma cidade aos favoritos
        public async Task<bool> AddFavoriteAsync(string cityName)
        {
            var favorite = new { CityName = cityName };
            var jsonContent = new StringContent(JsonSerializer.Serialize(favorite), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:44337/api/Favorites", jsonContent);
            return response.IsSuccessStatusCode;
        }
    }
}
