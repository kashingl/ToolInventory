using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.Services;

public interface IAuthService
{
    string? Token { get; }
    string? CurrentUserId { get; }
    bool IsAuthenticated { get; }
    Task<string?> GetValidTokenAsync();
    Task<ApiResult<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task LogoutAsync();
}
