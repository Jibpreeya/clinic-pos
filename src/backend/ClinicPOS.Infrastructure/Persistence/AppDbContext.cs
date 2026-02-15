using ClinicPOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly Guid _tenantId;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantId = tenantService.TenantId;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filters — every query automatically adds WHERE tenant_id = @tenantId
        modelBuilder.Entity<Patient>().HasQueryFilter(p => p.TenantId == _tenantId);
        modelBuilder.Entity<Branch>().HasQueryFilter(b => b.TenantId == _tenantId);
        modelBuilder.Entity<Appointment>().HasQueryFilter(a => a.TenantId == _tenantId);

        // PhoneNumber unique per Tenant (not globally)
        modelBuilder.Entity<Patient>()
            .HasIndex(p => new { p.TenantId, p.PhoneNumber })
            .IsUnique()
            .HasDatabaseName("IX_Patients_TenantId_PhoneNumber");

        // No duplicate appointment: same patient + branch + time within tenant
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.TenantId, a.PatientId, a.BranchId, a.StartAt })
            .IsUnique()
            .HasDatabaseName("IX_Appointments_NoDuplicate");

        modelBuilder.Entity<Patient>()
            .HasOne(p => p.Tenant).WithMany().HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.PrimaryBranch).WithMany(b => b.Patients).HasForeignKey(p => p.PrimaryBranchId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Branch>()
            .HasOne(b => b.Tenant).WithMany(t => t.Branches).HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Branch).WithMany().HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Tenant).WithMany(t => t.Users).HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<UserBranch>().HasKey(ub => new { ub.UserId, ub.BranchId });
    }
}
