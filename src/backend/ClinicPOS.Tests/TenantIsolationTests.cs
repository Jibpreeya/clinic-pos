using ClinicPOS.Application.Patients;
using ClinicPOS.Domain.Entities;
using ClinicPOS.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicPOS.Tests;

/// <summary>
/// Test 1: Tenant scoping — a user from Tenant A cannot read Tenant B's patients
/// </summary>
public class TenantIsolationTests
{
    private AppDbContext CreateContext(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        var tenantSvc = new FakeTenantService(tenantId);
        return new AppDbContext(options, tenantSvc);
    }

    [Fact]
    public async Task ListPatients_ShouldOnlyReturnCurrentTenantPatients()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // Seed both tenants using a bypass context
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"SharedDb_{Guid.NewGuid()}")
            .Options;

        await using (var seedCtx = new AppDbContext(options, new FakeTenantService(tenantA)))
        {
            seedCtx.Patients.Add(new Patient
            {
                FirstName = "Alice", LastName = "A", PhoneNumber = "0811111111",
                TenantId = tenantA
            });
            seedCtx.Patients.Add(new Patient
            {
                FirstName = "Bob", LastName = "B", PhoneNumber = "0822222222",
                TenantId = tenantB
            });
            await seedCtx.SaveChangesAsync();
        }

        // Query as Tenant A — should only see Alice
        await using var ctxA = new AppDbContext(options, new FakeTenantService(tenantA));
        var patients = await ctxA.Patients.ToListAsync();

        patients.Should().HaveCount(1);
        patients[0].FirstName.Should().Be("Alice");
        patients.Should().NotContain(p => p.TenantId == tenantB);
    }
}

/// <summary>
/// Test 2: Duplicate phone within same tenant is rejected
/// </summary>
public class DuplicatePhoneTests
{
    [Fact]
    public async Task CreatePatient_ShouldRejectDuplicatePhoneInSameTenant()
    {
        var tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"PhoneDb_{Guid.NewGuid()}")
            .Options;

        var tenantSvc = new FakeTenantService(tenantId);

        await using var ctx1 = new AppDbContext(options, tenantSvc);
        var svc1 = new PatientService(ctx1, tenantSvc);
        await svc1.CreateAsync(new CreatePatientRequest
        {
            FirstName = "First", LastName = "Patient", PhoneNumber = "0899999999"
        });

        await using var ctx2 = new AppDbContext(options, tenantSvc);
        var svc2 = new PatientService(ctx2, tenantSvc);

        var act = async () => await svc2.CreateAsync(new CreatePatientRequest
        {
            FirstName = "Second", LastName = "Patient", PhoneNumber = "0899999999"
        });

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreatePatient_ShouldAllowSamePhoneAcrossDifferentTenants()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"PhoneDb2_{Guid.NewGuid()}")
            .Options;

        var svcA = new FakeTenantService(tenantA);
        await using var ctxA = new AppDbContext(options, svcA);
        await new PatientService(ctxA, svcA).CreateAsync(new CreatePatientRequest
        {
            FirstName = "TenantA", LastName = "Patient", PhoneNumber = "0811111111"
        });

        var svcB = new FakeTenantService(tenantB);
        await using var ctxB = new AppDbContext(options, svcB);
        var act = async () => await new PatientService(ctxB, svcB).CreateAsync(new CreatePatientRequest
        {
            FirstName = "TenantB", LastName = "Patient", PhoneNumber = "0811111111"
        });

        // Should NOT throw — different tenant
        await act.Should().NotThrowAsync();
    }
}

public class FakeTenantService : ICurrentTenantService
{
    public FakeTenantService(Guid tenantId) => TenantId = tenantId;
    public Guid TenantId { get; }
    public Guid UserId => Guid.NewGuid();
    public string Role => "Admin";
}
