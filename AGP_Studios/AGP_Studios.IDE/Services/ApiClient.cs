using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using AGP_Studios.IDE.Models;

namespace AGP_Studios.IDE.Services;

/// <summary>
/// API client for AGP AI Server communication
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _authToken;
    
    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Set default headers required by the API
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AGP-Studios-IDE/1.0");
    }
    
    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
    
    private string GetBaseUrl()
    {
        return ConfigurationManager.Instance.GetFullServerUrl();
    }
    
    /// <summary>
    /// Authenticate user with phpBB3 credentials
    /// </summary>
    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };
            
            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(
                $"{GetBaseUrl()}/api/auth/login",
                content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseJson);
                return loginResponse ?? new LoginResponse { Success = false, Message = "Invalid response" };
            }
            else
            {
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = $"Login failed: {response.StatusCode}" 
                };
            }
        }
        catch (Exception ex)
        {
            return new LoginResponse 
            { 
                Success = false, 
                Message = $"Error: {ex.Message}" 
            };
        }
    }
    
    /// <summary>
    /// Get current user information including admin status
    /// </summary>
    public async Task<UserInfoResponse?> GetUserInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{GetBaseUrl()}/api/user/me");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserInfoResponse>(json);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting user info: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get list of published games
    /// </summary>
    public async Task<List<Game>> GetGamesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{GetBaseUrl()}/api/games");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var games = JsonConvert.DeserializeObject<List<Game>>(json);
                return games ?? new List<Game>();
            }
            
            return new List<Game>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting games: {ex.Message}");
            return new List<Game>();
        }
    }
    
    /// <summary>
    /// Publish code/package (admin only)
    /// </summary>
    public async Task<bool> PublishCodeAsync(string name, string content)
    {
        try
        {
            var publishData = new
            {
                Name = name,
                Content = content,
                PublishedDate = DateTime.UtcNow
            };
            
            var json = JsonConvert.SerializeObject(publishData);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(
                $"{GetBaseUrl()}/api/admin/publish",
                httpContent);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error publishing code: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Download game file
    /// </summary>
    public async Task<byte[]?> DownloadGameAsync(string downloadUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(downloadUrl);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error downloading game: {ex.Message}");
            return null;
        }
    }
}
