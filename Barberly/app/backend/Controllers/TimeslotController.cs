using backend.Data;
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

    [HttpPost("CreateTimeslot")]
    public async Task<ActionResult<Timeslot>> CreateTimeslot([FromBody] Timeslot timeslot)
    {
        context.Timeslots.Add(timeslot);
        await context.SaveChangesAsync();
        return Ok(timeslot);
    }

    [HttpPut("UpdateTimeslot/{id}")]
    public async Task<IActionResult> UpdateTimeslot(Guid id, [FromBody] Timeslot timeslot)
    {
        var existing = await context.Timeslots.FindAsync(id);
        if (existing == null)
            return NotFound();

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

        context.Timeslots.Remove(ts);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
