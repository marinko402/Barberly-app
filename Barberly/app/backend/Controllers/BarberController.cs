using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class BarberController : ControllerBase
{
    private DataContext context { get; set; }

    public BarberController(DataContext context)
    {
        this.context = context;
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
    public async Task<ActionResult<Barber>> CreateBarber([FromBody] Barber barber)
    {
        context.Barbers.Add(barber);
        await context.SaveChangesAsync();
        return Ok(barber);
    }

    [HttpPut("UpdateBarber/{id}")]
    public async Task<IActionResult> UpdateBarber(Guid id, [FromBody] Barber barber)
    {
        var existing = await context.Barbers.FindAsync(id);
        if (existing == null)
            return NotFound();

        existing.firstName = barber.firstName;
        existing.lastName = barber.lastName;
        existing.email = barber.email;
        existing.phoneNumber = barber.phoneNumber;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("DeleteBarber/{id}")]
    public async Task<IActionResult> DeleteBarber(Guid id)
    {
        var barber = await context.Barbers.FindAsync(id);
        if (barber == null)
            return NotFound();

        context.Barbers.Remove(barber);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
