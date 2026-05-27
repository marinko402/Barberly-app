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
        if (dto.owner == null || string.IsNullOrEmpty(dto.owner.Id))
        {
            return BadRequest("Owner data is required with a valid ID.");
        }

        var existingOwner = await context.Users.FindAsync(dto.owner.Id);

        if (existingOwner == null)
        {
            return NotFound("Owner not found in the database.");
        }

        Salon salon = new Salon
        {
            name = dto.name,
            address = dto.address,
            city = dto.city,
            owner = existingOwner,
        };

        if (dto.barbers != null && dto.barbers.Any())
        {
            salon.barbers = new List<Barber>();
            foreach (var barberDto in dto.barbers)
            {
                var existingBarber = await context.Users.FindAsync(barberDto.Id);
                if (existingBarber != null)
                {
                    salon.barbers.Add(existingBarber);
                }
            }
        }

        context.Salons.Add(salon);
        await context.SaveChangesAsync();

        var responseDto = new SalonDto
        {
            name = salon.name,
            address = salon.address,
            city = salon.city,
            owner = salon.owner,
            barbers = salon.barbers,
        };

        return Ok(responseDto);
    }

    [HttpPut("UpdateSalon/{id}")]
    public async Task<IActionResult> UpdateSalon(Guid id, [FromBody] SalonDto salon)
    {
        var existing = await context.Salons.FindAsync(id);
        if (existing == null)
            return NotFound();

        existing.name = salon.name;
        existing.address = salon.address;
        existing.city = salon.city;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("DeleteSalon/{id}")]
    public async Task<IActionResult> DeleteSalon(Guid id)
    {
        var salon = await context
            .Salons.Include(s => s.barbers)
            .FirstOrDefaultAsync(s => s.salonId == id);

        if (salon == null)
            return NotFound();

        foreach (var barber in salon.barbers)
        {
            barber.SalonId = null;
        }

        context.Salons.Remove(salon);

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("AddBarberToSalon")]
    public async Task<IActionResult> AddBarberToSalon(Guid barberId, Guid salonId)
    {
        var barber = await context.Barbers.FindAsync(barberId.ToString());
        if (barber == null)
            return NotFound("Barber does not exist.");

        if (barber.SalonId.HasValue && barber.SalonId.Value != salonId)
        {
            return BadRequest("This barber is already employed in another salon.");
        }

        if (barber.SalonId.HasValue && barber.SalonId.Value == salonId)
        {
            return BadRequest("The barber is already a member of your salon.");
        }

        var salon = await context.Salons.FindAsync(salonId);
        if (salon == null)
            return NotFound("Salon does not exist.");

        barber.SalonId = salonId;

        await context.SaveChangesAsync();

        return Ok("Barber successfully added to the salon.");
    }

    [HttpPut("RemoveBarberFromSalon")]
    public async Task<IActionResult> RemoveBarberFromSalon(
        Guid barberId,
        Guid salonId,
        string ownerId
    )
    {
        var salon = await context
            .Salons.Include(s => s.barbers)
            .FirstOrDefaultAsync(s => s.salonId == salonId);
        if (salon == null)
            return NotFound("Salon does not exist.");

        if (salon.OwnerId != ownerId)
            return Forbid("Only the salon owner can remove barbers.");

        var barber = salon.barbers.FirstOrDefault(b => b.Id == barberId.ToString());
        if (barber == null)
            return NotFound("The barber is not a member of this salon.");

        if (barber.Id == salon.OwnerId)
            return BadRequest("You cannot remove yourself (the owner) from the salon.");

        salon.barbers.Remove(barber);
        await context.SaveChangesAsync();

        return Ok("Barber successfully removed from the salon.");
    }

    [HttpGet("GetSalonsCount")]
    public async Task<ActionResult<int>> GetSalonsCount()
    {
        var count = await context.Salons.CountAsync();
        return Ok(count);
    }

    [HttpGet("GetTopSalons")]
    public async Task<IActionResult> GetTopSalons()
    {
        var topSalons = await context
            .Salons.Select(s => new
            {
                s.salonId,
                s.name,
                s.address,
                s.city,
                StaffCount = s.barbers != null ? s.barbers.Count : 0,
                TotalBookings = context.Bookings.Count(b =>
                    b.timeslot != null
                    && b.timeslot.salon != null
                    && b.timeslot.salon.salonId == s.salonId
                ),
            })
            .OrderByDescending(x => x.TotalBookings)
            .Take(6)
            .ToListAsync();

        return Ok(topSalons);
    }
}
