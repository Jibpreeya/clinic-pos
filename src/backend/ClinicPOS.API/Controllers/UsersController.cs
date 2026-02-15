using ClinicPOS.Application.Users;
using ClinicPOS.Domain.Entities;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UsersController(AppDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        var user = new AppUser
        {
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FirstName = req.FirstName,
            LastName = req.LastName,
            Role = req.Role,
            TenantId = _tenant.TenantId,
        };
        _db.Users.Add(user);

        foreach (var branchId in req.BranchIds)
            _db.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = branchId });

        await _db.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            TenantId = user.TenantId,
            BranchIds = req.BranchIds,
        });
    }
}
