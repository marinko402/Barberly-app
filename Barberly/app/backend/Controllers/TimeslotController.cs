using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class TimeslotController : ControllerBase
{
    private DataContext context { get; set; }

    public TimeslotController(DataContext context)
    {
        this.context = context;
    }

    [HttpGet("GetAllTimeslots")]
    public async Task<ActionResult<List<Timeslot>>> GetAllTimeslots()
    {
        return await context.Timeslots.Include(t => t.barber).Include(t => t.salon).ToListAsync();
    }

    [HttpGet("GetAllFreeTimeslots")]
    public async Task<ActionResult<List<Timeslot>>> GetAllFreeTimeslots()
    {
        return await context
            .Timeslots.Include(t => t.barber)
            .Include(t => t.salon)
            .Where(t => t.isBooked == false)
            .ToListAsync();
    }

    [HttpPost("CreateTimeslot")]
    public async Task<ActionResult<Timeslot>> CreateTimeslot([FromBody] TimeslotDto dto)
    {
        Timeslot timeslot = new Timeslot
        {
            date = dto.date,
            startTime = dto.startTime,
            duration = dto.duration,
            isBooked = false,
        };
        var barber = await context
            .Barbers.Where(b => b.barberId == dto.barberId)
            .FirstOrDefaultAsync();

        var salon = await context.Salons.Where(s => s.salonId == dto.salonId).FirstOrDefaultAsync();

        timeslot.barber = barber;
        timeslot.salon = salon;

        var newStart = timeslot.startTime;
        var newEnd = timeslot.startTime.AddMinutes(timeslot.duration);

        if (timeslot.barber == null)
            return BadRequest("Barber not found");

        var overlapping = await context
            .Timeslots.Where(t =>
                t.date == timeslot.date
                && t.barber!.barberId == timeslot.barber.barberId
                && t.startTime < newEnd
                && newStart < t.startTime.AddMinutes(t.duration)
            )
            .AnyAsync();

        if (overlapping)
            return BadRequest("Termin se preklapa sa postojećim.");

        context.Timeslots.Add(timeslot);
        await context.SaveChangesAsync();

        return Ok(timeslot);
    }

    [HttpPut("UpdateTimeslot/{id}")]
    public async Task<IActionResult> UpdateTimeslot(Guid id, [FromBody] TimeslotDto timeslot)
    {
        var existing = await context.Timeslots.FindAsync(id);
        if (existing == null)
            return NotFound();

        var barber = await context
            .Barbers.Where(b => b.barberId == timeslot.barberId)
            .FirstOrDefaultAsync();

        if (barber == null)
            return BadRequest("Barber not found");

        var newStart = timeslot.startTime;
        var newEnd = timeslot.startTime.AddMinutes(timeslot.duration);

        var overlapping = await context
            .Timeslots.Where(t =>
                t.timeslotId != id
                && t.date == timeslot.date
                && t.barber!.barberId == timeslot.barberId
                && t.startTime < newEnd
                && newStart < t.startTime.AddMinutes(t.duration)
            )
            .AnyAsync();

        if (overlapping)
            return BadRequest("Termin se preklapa sa postojećim.");

        existing.date = timeslot.date;
        existing.startTime = timeslot.startTime;
        existing.duration = timeslot.duration;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("DeleteTimeslot/{id}")]
    public async Task<IActionResult> DeleteTimeslot(Guid id)
    {
        var ts = await context.Timeslots.FindAsync(id);
        if (ts == null)
            return NotFound();

        if (ts.isBooked)
            return BadRequest("Ne možete obrisati rezervisan termin.");

        context.Timeslots.Remove(ts);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
