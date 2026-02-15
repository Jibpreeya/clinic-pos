using System.ComponentModel.DataAnnotations;

namespace ClinicPOS.Application.Appointments;

public record CreateAppointmentRequest
{
    [Required] public Guid BranchId { get; init; }
    [Required] public Guid PatientId { get; init; }
    [Required] public DateTime StartAt { get; init; }
}

public record AppointmentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public DateTime StartAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
