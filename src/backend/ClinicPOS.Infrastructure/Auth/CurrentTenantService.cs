using System.Security.Claims;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;

namespace ClinicPOS.Infrastructure.Auth;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _http;
    public CurrentTenantService(IHttpContextAccessor http) => _http = http;

    public Guid TenantId
    {
        get
        {
            var v = _http.HttpContext?.User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(v, out var id) ? id : Guid.Empty;
        }
    }
    public Guid UserId
    {
        get
        {
            var v = _http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(v, out var id) ? id : Guid.Empty;
        }
    }
    public string Role => _http.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
