namespace ClinicPOS.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
