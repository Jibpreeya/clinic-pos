namespace ClinicPOS.Domain.Entities;

public class Patient : TenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? PrimaryBranchId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Branch? PrimaryBranch { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
