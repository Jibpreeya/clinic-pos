namespace ClinicPOS.Infrastructure.Persistence;

public interface ICurrentTenantService
{
    Guid TenantId { get; }
    Guid UserId { get; }
    string Role { get; }
}
