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
        var barber = await context
            .Barbers.Include(b => b.salon)
            .FirstOrDefaultAsync(b => b.Id == dto.barberId);

        if (barber == null)
            return BadRequest("Barber not found.");

        if (barber.salon == null)
            return BadRequest("Barber is not assigned to any salon.");

        Timeslot timeslot = new Timeslot
        {
            date = dto.date,
            startTime = dto.startTime,
            duration = dto.duration,
            isBooked = false,
            barber = barber,
            salon = barber.salon,
        };

        var newStart = timeslot.startTime;
        var newEnd = timeslot.startTime.AddMinutes(timeslot.duration);

        var overlapping = await context
            .Timeslots.Where(t =>
                t.date == timeslot.date
                && t.barber!.Id == timeslot.barber.Id
                && t.startTime < newEnd
                && newStart < t.startTime.AddMinutes(t.duration)
            )
            .AnyAsync();

        if (overlapping)
            return BadRequest("The timeslot overlaps with an existing one.");

        context.Timeslots.Add(timeslot);
        await context.SaveChangesAsync();

        return Ok(timeslot);
    }

    [HttpPut("UpdateTimeslot/{id}")]
    public async Task<IActionResult> UpdateTimeslot(Guid id, [FromBody] TimeslotDto timeslot)
    {
        if (timeslot == null)
            return BadRequest("Invalid request data.");

        var existing = await context.Timeslots.FindAsync(id);
        if (existing == null)
            return NotFound("Timeslot not found.");

        if (existing.isBooked)
            return BadRequest("Cannot update a timeslot that has already been booked.");

        var newBarber = await context.Barbers.FirstOrDefaultAsync(b => b.Id == timeslot.barberId);
        if (newBarber == null)
            return BadRequest("Barber not found.");

        var newStart = timeslot.startTime;
        var newEnd = timeslot.startTime.AddMinutes(timeslot.duration);

        var overlapping = await context
            .Timeslots.Where(t =>
                t.timeslotId != id
                && t.date == timeslot.date
                && t.barber!.Id == timeslot.barberId
                && t.startTime < newEnd
                && newStart < t.startTime.AddMinutes(t.duration)
            )
            .AnyAsync();

        if (overlapping)
            return BadRequest("The timeslot overlaps with an existing one.");

        existing.date = timeslot.date;
        existing.startTime = timeslot.startTime;
        existing.duration = timeslot.duration;
        existing.barber = newBarber;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("DeleteTimeslot/{id}")]
    public async Task<IActionResult> DeleteTimeslot(Guid id)
    {
        var ts = await context.Timeslots.FindAsync(id);
        if (ts == null)
            return NotFound("Timeslot not found.");

        if (ts.isBooked)
            return BadRequest("You cannot delete a booked timeslot.");

        context.Timeslots.Remove(ts);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("GetBarberDailySchedule")]
    public async Task<IActionResult> GetBarberDailySchedule(
        [FromQuery] Guid barberId,
        [FromQuery] DateOnly date
    )
    {
        var schedule = await context
            .Timeslots.Where(t =>
                t.barber != null && t.barber.Id == barberId.ToString() && t.date == date
            )
            .Select(t => new
            {
                t.timeslotId,
                t.startTime,
                t.duration,
                t.isBooked,
                CustomerName = context
                    .Bookings.Where(b =>
                        b.timeslot != null && b.timeslot.timeslotId == t.timeslotId
                    )
                    .Select(b => b.customerFirstName + " " + b.customerLastName)
                    .FirstOrDefault(),
                CustomerEmail = context
                    .Bookings.Where(b =>
                        b.timeslot != null && b.timeslot.timeslotId == t.timeslotId
                    )
                    .Select(b => b.customerEmail)
                    .FirstOrDefault(),
                CustomerPhoneNumber = context
                    .Bookings.Where(b =>
                        b.timeslot != null && b.timeslot.timeslotId == t.timeslotId
                    )
                    .Select(b => b.customerPhoneNumber)
                    .FirstOrDefault(),
            })
            .OrderBy(t => t.startTime)
            .ToListAsync();

        return Ok(schedule);
    }

    [HttpPut("CancelBooking/{id}")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        var timeslot = await context.Timeslots.FirstOrDefaultAsync(t => t.timeslotId == id);

        if (timeslot == null)
            return NotFound("Timeslot not found.");

        if (!timeslot.isBooked)
            return BadRequest("Timeslot is already available.");

        var booking = await context.Bookings.FirstOrDefaultAsync(b =>
            b.timeslot != null && b.timeslot.timeslotId == id
        );
        if (booking != null)
        {
            context.Bookings.Remove(booking);
        }

        timeslot.isBooked = false;

        await context.SaveChangesAsync();
        return NoContent();
    }
}
