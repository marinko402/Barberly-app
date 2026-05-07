using backend.Data;
using backend.Dtos;
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
    public async Task<ActionResult<Salon>> CreateSalon([FromBody] SalonDto dto)
    {
        Salon salon = new Salon { name = dto.name, address = dto.address };
        context.Salons.Add(salon);
        await context.SaveChangesAsync();
        return Ok(salon);
    }

    [HttpPut("UpdateSalon/{id}")]
    public async Task<IActionResult> UpdateSalon(Guid id, [FromBody] SalonDto salon)
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

    [HttpPut("AddBarberToSalon")]
    public async Task<IActionResult> AddBarberToSalon(Guid barberId, Guid salonId)
    {
        var barber = await context.Barbers.FindAsync(barberId);
        if (barber == null)
            return NotFound("Barber ne postoji.");

        var salon = await context.Salons.FindAsync(salonId);
        if (salon == null)
            return NotFound("Salon ne postoji.");

        salon.barbers.Add(barber);

        await context.SaveChangesAsync();

        return Ok("Barber uspešno dodat u salon.");
    }
}
