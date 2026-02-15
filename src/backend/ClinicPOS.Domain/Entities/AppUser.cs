using ClinicPOS.Domain.Enums;

namespace ClinicPOS.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}

public class UserBranch
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public AppUser User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
