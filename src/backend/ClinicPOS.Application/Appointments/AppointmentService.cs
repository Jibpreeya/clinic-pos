using ClinicPOS.Domain.Entities;
using ClinicPOS.Infrastructure.Messaging;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Application.Appointments;

public class AppointmentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IMessagePublisher _publisher;

    public AppointmentService(AppDbContext db, ICurrentTenantService tenant, IMessagePublisher publisher)
    {
        _db = db;
        _tenant = tenant;
        _publisher = publisher;
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest req)
    {
        // Verify patient belongs to this tenant (global filter does this automatically)
        var patient = await _db.Patients.FindAsync(req.PatientId)
            ?? throw new NotFoundException("Patient not found in this tenant.");

        var branch = await _db.Branches.FindAsync(req.BranchId)
            ?? throw new NotFoundException("Branch not found in this tenant.");

        var appointment = new Appointment
        {
            TenantId = _tenant.TenantId,
            BranchId = req.BranchId,
            PatientId = req.PatientId,
            StartAt = req.StartAt,
        };

        _db.Appointments.Add(appointment);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Appointments_NoDuplicate") == true)
        {
            throw new ConflictException("Appointment already exists for this patient, branch, and time.");
        }

        // Publish event to RabbitMQ
        await _publisher.PublishAsync("clinic.events", "appointment.created", new
        {
            EventType = "AppointmentCreated",
            AppointmentId = appointment.Id,
            TenantId = appointment.TenantId,
            BranchId = appointment.BranchId,
            PatientId = appointment.PatientId,
            StartAt = appointment.StartAt,
            OccurredAt = DateTime.UtcNow,
        });

        return new AppointmentDto
        {
            Id = appointment.Id,
            TenantId = appointment.TenantId,
            BranchId = appointment.BranchId,
            BranchName = branch.Name,
            PatientId = appointment.PatientId,
            PatientName = $"{patient.FirstName} {patient.LastName}",
            StartAt = appointment.StartAt,
            CreatedAt = appointment.CreatedAt,
        };
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
