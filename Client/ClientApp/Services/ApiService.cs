using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClientApp.Models;

namespace ClientApp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    // Автоматичний вибір URL залежно від платформи (Android Emulator vs Windows Desktop)
    private static readonly string BaseUrl = DeviceInfo.Platform == DevicePlatform.Android
        ? "http://10.0.2.2:5282"
        : "http://localhost:5282";

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    // 1. АВТЕНТИФІКАЦІЯ: POST /api/auth/login
    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        try
        {
            var loginData = new { Email = email.Trim(), Password = password.Trim() };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                string token = root.GetProperty("token").GetString() ?? string.Empty;
                int userId = root.GetProperty("userID").GetInt32();
                string userName = root.GetProperty("name").GetString() ?? string.Empty;

                // Зберігаємо JWT-токен та профіль користувача у Preferences
                Preferences.Default.Set("jwt_token", token);
                Preferences.Default.Set("user_id", userId);
                Preferences.Default.Set("user_name", userName);

                return (true, null);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "Невірний email або пароль.");
            }

            return (false, $"Помилка сервера: {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Не вдалося з'єднатися з сервером ({BaseUrl}): {ex.Message}");
        }
    }

    // 2. ОТРИМАННЯ ЧЕМПІОНІВ: GET /api/items
    public async Task<List<ItemModel>> GetItemsAsync()
    {
        try
        {
            ApplyAuthorizationHeader();
            var response = await _httpClient.GetAsync("/api/items");

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<ItemModel>>();
                return items ?? new List<ItemModel>();
            }

            return new List<ItemModel>();
        }
        catch (Exception)
        {
            return new List<ItemModel>();
        }
    }

    // 3. ПУБЛІКАЦІЯ ГАЙДУ: POST /api/actions
    public async Task<(bool Success, string Message)> CreateActionAsync(ActionModel action)
    {
        try
        {
            ApplyAuthorizationHeader();

            var actionDto = new
            {
                ItemID = action.ItemID,
                UserID = action.UserID,
                OpponentChampion = action.OpponentChampion.Trim(),
                ActionDetails = action.ActionDetails.Trim(),
                Status = string.IsNullOrWhiteSpace(action.Status) ? "Draft" : action.Status
            };

            var response = await _httpClient.PostAsJsonAsync("/api/actions", actionDto);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Тактичний гайд успішно збережено в базі даних!");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            return (false, $"Помилка валідації: {errorBody}");
        }
        catch (Exception ex)
        {
            return (false, $"Помилка мережі: {ex.Message}");
        }
    }

    // Допоміжний метод: додавання збереженого токена у заголовок Authorization
    private void ApplyAuthorizationHeader()
    {
        string token = Preferences.Default.Get("jwt_token", string.Empty);
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Вихід з облікового запису
    public static void Logout()
    {
        Preferences.Default.Remove("jwt_token");
        Preferences.Default.Remove("user_id");
        Preferences.Default.Remove("user_name");
    }
}