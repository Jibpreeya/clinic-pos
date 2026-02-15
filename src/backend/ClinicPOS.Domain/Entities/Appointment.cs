namespace ClinicPOS.Domain.Entities;

public class Appointment : TenantEntity
{
    public Guid BranchId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime StartAt { get; set; }
    public Branch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
