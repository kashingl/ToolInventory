using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ToolInventory.API.Common;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var email = InputNormalizer.NormalizeName(dto.Email);
        var displayName = InputNormalizer.NormalizeName(dto.DisplayName);
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return this.ConflictProblem("Email already registered.", "A user account with this email already exists.");
        }

        var user = new IdentityUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select((error, index) => new { Key = $"IdentityError{index}", error.Description })
                .ToDictionary(x => x.Key, x => new[] { x.Description });

            return ValidationProblem(new ValidationProblemDetails(errors)
            {
                Title = "Registration validation failed.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(GenerateToken(user, displayName));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var email = InputNormalizer.NormalizeName(dto.Email);
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, dto.Password))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials.",
                Detail = "The provided email or password is incorrect."
            });
        }

        var displayName = user.UserName ?? user.Email ?? string.Empty;
        return Ok(GenerateToken(user, displayName));
    }

    private AuthResponseDto GenerateToken(IdentityUser user, string displayName)
    {
        var jwtKey = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("TOOLINVENTORY_JWT_KEY");
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var configuredExpiry) ? configuredExpiry : 480;
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Name, displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email!,
            DisplayName = displayName,
            ExpiresAt = expiry
        };
    }
}
