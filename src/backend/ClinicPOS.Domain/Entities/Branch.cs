namespace ClinicPOS.Domain.Entities;

public class Branch : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
