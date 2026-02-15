using ClinicPOS.Application.Appointments;
using ClinicPOS.Application.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,User")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _service;

    public AppointmentsController(AppointmentService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest req)
    {
        try
        {
            var result = await _service.CreateAsync(req);
            return CreatedAtAction(nameof(Create), new { }, result);
        }
        catch (ConflictException ex) { return Conflict(new { error = ex.Message }); }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}
