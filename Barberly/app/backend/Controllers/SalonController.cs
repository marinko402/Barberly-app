using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class SalonController : ControllerBase
{
    private DataContext context { get; set; }

    public SalonController(DataContext context)
    {
        this.context = context;
    }

    [HttpGet("GetAllSalons")]
    public async Task<ActionResult<List<Salon>>> GetAllSalons()
    {
        return await context.Salons.Include(s => s.barbers).ToListAsync();
    }

    [HttpGet("GetSalonById/{id}")]
    public async Task<ActionResult<Salon>> GetSalonById(Guid id)
    {
        var salon = await context
            .Salons.Include(s => s.barbers)
            .FirstOrDefaultAsync(s => s.salonId == id);

        if (salon == null)
            return NotFound();
        return salon;
    }

    [HttpPost("CreateSalon")]
    public async Task<ActionResult<Salon>> CreateSalon([FromBody] Salon salon)
    {
        context.Salons.Add(salon);
        await context.SaveChangesAsync();
        return Ok(salon);
    }

    [HttpPut("UpdateSalon/{id}")]
    public async Task<IActionResult> UpdateSalon(Guid id, [FromBody] Salon salon)
    {
        var existing = await context.Salons.FindAsync(id);
        if (existing == null)
            return NotFound();

        existing.name = salon.name;
        existing.address = salon.address;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("DeleteSalon/{id}")]
    public async Task<IActionResult> DeleteSalon(Guid id)
    {
        var salon = await context.Salons.FindAsync(id);
        if (salon == null)
            return NotFound();

        context.Salons.Remove(salon);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
