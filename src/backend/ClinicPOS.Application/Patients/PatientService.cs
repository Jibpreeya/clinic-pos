using ClinicPOS.Domain.Entities;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Application.Patients;

public class PatientService
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public PatientService(AppDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<PatientDto> CreateAsync(CreatePatientRequest req)
    {
        // Check duplicate phone within tenant (DB unique index is the safety net)
        var exists = await _db.Patients
            .AnyAsync(p => p.PhoneNumber == req.PhoneNumber);
        if (exists)
            throw new ConflictException($"Phone number '{req.PhoneNumber}' already exists in this tenant.");

        var patient = new Patient
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            PhoneNumber = req.PhoneNumber,
            TenantId = _tenant.TenantId,
            PrimaryBranchId = req.PrimaryBranchId,
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        return ToDto(patient, null);
    }

    public async Task<PagedResult<PatientDto>> ListAsync(ListPatientsQuery query)
    {
        // TenantId filter applied automatically via EF Core global query filter
        var q = _db.Patients
            .Include(p => p.PrimaryBranch)
            .AsQueryable();

        if (query.BranchId.HasValue)
            q = q.Where(p => p.PrimaryBranchId == query.BranchId);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<PatientDto>
        {
            Items = items.Select(p => ToDto(p, p.PrimaryBranch?.Name)),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    private static PatientDto ToDto(Patient p, string? branchName) => new()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        PhoneNumber = p.PhoneNumber,
        TenantId = p.TenantId,
        PrimaryBranchId = p.PrimaryBranchId,
        PrimaryBranchName = branchName,
        CreatedAt = p.CreatedAt,
    };
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
