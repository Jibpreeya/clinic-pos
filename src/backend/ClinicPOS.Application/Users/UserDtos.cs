using System.ComponentModel.DataAnnotations;
using ClinicPOS.Domain.Enums;

namespace ClinicPOS.Application.Users;

public record CreateUserRequest
{
    [Required][EmailAddress] public string Email { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
    [Required] public string FirstName { get; init; } = string.Empty;
    [Required] public string LastName { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.User;
    public List<Guid> BranchIds { get; init; } = new();
}

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public List<Guid> BranchIds { get; init; } = new();
}
