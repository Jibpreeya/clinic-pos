using ClinicPOS.Application.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly PatientService _service;

    public PatientsController(PatientService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin,User,Viewer")]
    public async Task<IActionResult> List([FromQuery] ListPatientsQuery query)
    {
        var result = await _service.ListAsync(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,User")]  // Viewer cannot create patients
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest req)
    {
        try
        {
            var patient = await _service.CreateAsync(req);
            return CreatedAtAction(nameof(List), new { }, patient);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
