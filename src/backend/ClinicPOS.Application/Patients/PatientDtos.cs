using System.ComponentModel.DataAnnotations;

namespace ClinicPOS.Application.Patients;

public record CreatePatientRequest
{
    [Required] public string FirstName { get; init; } = string.Empty;
    [Required] public string LastName { get; init; } = string.Empty;
    [Required] [Phone] public string PhoneNumber { get; init; } = string.Empty;
    public Guid? PrimaryBranchId { get; init; }
}

public record PatientDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public Guid? PrimaryBranchId { get; init; }
    public string? PrimaryBranchName { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ListPatientsQuery
{
    public Guid? BranchId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
