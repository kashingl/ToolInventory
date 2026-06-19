using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.Services;

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string EmailKey = "auth_email";
    private const string DisplayNameKey = "auth_display_name";
    private const string ExpiryKey = "auth_expiry";

    private readonly HttpClient _http;
    private readonly IApiConfiguration _apiConfiguration;
    private readonly ILogger<AuthService> _logger;
    private readonly Task _loadTokenTask;

    public string? Token { get; private set; }
    public string? CurrentUserId { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public AuthService(IApiConfiguration apiConfiguration, ILogger<AuthService> logger)
    {
        _apiConfiguration = apiConfiguration;
        _logger = logger;
        _http = new HttpClient
        {
            BaseAddress = new Uri(apiConfiguration.BaseUrl)
        };
        _loadTokenTask = LoadTokenAsync();
    }

    public async Task<string?> GetValidTokenAsync()
    {
        await _loadTokenTask;
        EnsureBaseAddress();

        if (Token is null)
        {
            return null;
        }

        var expiry = await SecureStorage.Default.GetAsync(ExpiryKey);
        if (!DateTime.TryParse(expiry, out var expiresAt) || expiresAt <= DateTime.UtcNow)
        {
            await LogoutAsync();
            return null;
        }

        return Token;
    }

    public async Task<ApiResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        EnsureBaseAddress();
        var response = await _http.PostAsJsonAsync("/api/auth/login", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<AuthResponseDto>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result is null)
        {
            return ApiResult<AuthResponseDto>.Fail((int)response.StatusCode, "Invalid authentication response.");
        }

        await SaveTokenAsync(result);
        return ApiResult<AuthResponseDto>.Success(result);
    }

    public async Task<ApiResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        EnsureBaseAddress();
        var response = await _http.PostAsJsonAsync("/api/auth/register", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<AuthResponseDto>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result is null)
        {
            return ApiResult<AuthResponseDto>.Fail((int)response.StatusCode, "Invalid authentication response.");
        }

        await SaveTokenAsync(result);
        return ApiResult<AuthResponseDto>.Success(result);
    }

    public Task LogoutAsync()
    {
        Token = null;
        CurrentUserId = null;
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(EmailKey);
        SecureStorage.Default.Remove(DisplayNameKey);
        SecureStorage.Default.Remove(ExpiryKey);
        return Task.CompletedTask;
    }

    private async Task SaveTokenAsync(AuthResponseDto dto)
    {
        Token = dto.Token;
        CurrentUserId = ExtractUserIdFromJwt(dto.Token);
        await SecureStorage.Default.SetAsync(TokenKey, dto.Token);
        await SecureStorage.Default.SetAsync(EmailKey, dto.Email);
        await SecureStorage.Default.SetAsync(DisplayNameKey, dto.DisplayName);
        await SecureStorage.Default.SetAsync(ExpiryKey, dto.ExpiresAt.ToString("O"));
    }

    private async Task LoadTokenAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            var expiry = await SecureStorage.Default.GetAsync(ExpiryKey);
            if (!string.IsNullOrEmpty(token)
                && DateTime.TryParse(expiry, out var expiresAt)
                && expiresAt > DateTime.UtcNow)
            {
                Token = token;
                CurrentUserId = ExtractUserIdFromJwt(token);
            }
        }
        catch
        {
            _logger.LogWarning("Unable to load auth token from secure storage.");
            Token = null;
            CurrentUserId = null;
        }
    }

    private void EnsureBaseAddress()
    {
        var configured = new Uri(_apiConfiguration.BaseUrl);
        if (_http.BaseAddress != configured)
        {
            _http.BaseAddress = configured;
        }
    }

    private static string? ExtractUserIdFromJwt(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            var bytes = Convert.FromBase64String(payload);
            using var doc = JsonDocument.Parse(bytes);
            return doc.RootElement.TryGetProperty("sub", out var sub) ? sub.GetString() : null;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? "The request failed.";
            }
            if (doc.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? "The request failed.";
            }
        }
        catch
        {
            // Fall through to generic message.
        }

        return "The request failed.";
    }
}
