using ClinicPOS.Domain.Entities;
using ClinicPOS.Domain.Enums;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicPOS.Infrastructure.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Use a special seeder context that bypasses tenant filter
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
        await using var db = new SeederDbContext(options);

        await db.Database.MigrateAsync();

        if (await db.Tenants.AnyAsync()) { logger.LogInformation("Already seeded."); return; }

        // 1 Tenant
        var tenant = new Tenant { Name = "Demo Clinic Co., Ltd." };
        db.Tenants.Add(tenant);

        // 2 Branches
        var branch1 = new Branch { TenantId = tenant.Id, Name = "Main Branch", Address = "123 Main St, Bangkok" };
        var branch2 = new Branch { TenantId = tenant.Id, Name = "North Branch", Address = "456 North Rd, Chiang Mai" };
        db.Branches.AddRange(branch1, branch2);

        // Users for each role
        var adminUser = new AppUser
        {
            Email = "admin@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
            FirstName = "Admin", LastName = "User",
            Role = UserRole.Admin, TenantId = tenant.Id
        };
        var staffUser = new AppUser
        {
            Email = "staff@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff1234!"),
            FirstName = "Staff", LastName = "User",
            Role = UserRole.User, TenantId = tenant.Id
        };
        var viewerUser = new AppUser
        {
            Email = "viewer@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Viewer1234!"),
            FirstName = "Viewer", LastName = "User",
            Role = UserRole.Viewer, TenantId = tenant.Id
        };
        db.Users.AddRange(adminUser, staffUser, viewerUser);
        await db.SaveChangesAsync();

        // Associate users with branches
        db.UserBranches.AddRange(
            new UserBranch { UserId = adminUser.Id, BranchId = branch1.Id },
            new UserBranch { UserId = adminUser.Id, BranchId = branch2.Id },
            new UserBranch { UserId = staffUser.Id, BranchId = branch1.Id },
            new UserBranch { UserId = viewerUser.Id, BranchId = branch1.Id }
        );
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded: Tenant={Tenant}, 2 Branches, 3 Users", tenant.Name);
    }
}

// Bypass-filter context for seeder only
public class SeederDbContext : AppDbContext
{
    public SeederDbContext(DbContextOptions<AppDbContext> options)
        : base(options, new SeederTenantService()) { }
}

public class SeederTenantService : ICurrentTenantService
{
    public Guid TenantId => Guid.Empty;
    public Guid UserId => Guid.Empty;
    public string Role => "Admin";
}
