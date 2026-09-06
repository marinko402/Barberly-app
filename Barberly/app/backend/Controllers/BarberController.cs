using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class BarberController : ControllerBase
{
    private DataContext context { get; set; }
    private readonly UserManager<Barber> _userManager;

    public BarberController(DataContext context, UserManager<Barber> userManager)
    {
        this.context = context;
        _userManager = userManager;
    }

    [HttpGet("GetAllBarbers")]
    public async Task<ActionResult<List<Barber>>> GetAllBarbers()
    {
        return await context.Barbers.ToListAsync();
    }

    [HttpGet("GetBarberById/{id}")]
    public async Task<ActionResult<Barber>> GetBarberById(Guid id)
    {
        var barber = await context.Barbers.FindAsync(id);
        if (barber == null)
            return NotFound();
        return barber;
    }

    [HttpPost("CreateBarber")]
    public async Task<ActionResult<Barber>> CreateBarber([FromBody] BarberDto barberDto)
    {
        if (barberDto == null)
        {
            return BadRequest("Barber data is required");
        }

        if (
            string.IsNullOrWhiteSpace(barberDto.firstName)
            || string.IsNullOrWhiteSpace(barberDto.lastName)
            || string.IsNullOrWhiteSpace(barberDto.email)
        )
        {
            return BadRequest("All fields are required");
        }

        var barber = new Barber
        {
            firstName = barberDto.firstName,
            lastName = barberDto.lastName,
            Email = barberDto.email,
        };

        context.Barbers.Add(barber);
        await context.SaveChangesAsync();

        return Ok(barber);
    }

    [HttpPut("UpdateBarber/{id}")]
    public async Task<IActionResult> UpdateBarber(string id, [FromBody] BarberDto barber)
    {
        var existing = await context.Barbers.FindAsync(id);
        if (existing == null)
            return NotFound();

        if (barber == null)
        {
            return BadRequest("Barber data is required");
        }

        if (
            string.IsNullOrWhiteSpace(barber.firstName)
            || string.IsNullOrWhiteSpace(barber.lastName)
            || string.IsNullOrWhiteSpace(barber.email)
        )
        {
            return BadRequest("All fields are required");
        }

        existing.firstName = barber.firstName;
        existing.lastName = barber.lastName;
        existing.Email = barber.email;
        existing.PhoneNumber = barber.phoneNumber;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("DeleteBarber/{id}")]
    public async Task<IActionResult> DeleteBarber(string id)
    {
        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (loggedInUserId == null)
            return Unauthorized();

        if (loggedInUserId != id)
            return Forbid();

        var barber = await _userManager.FindByIdAsync(id);

        if (barber == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(barber);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}
