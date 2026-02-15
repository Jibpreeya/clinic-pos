using System.ComponentModel.DataAnnotations;

namespace ClinicPOS.Application.Auth;

public record LoginRequest
{
    [Required][EmailAddress] public string Email { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
}
