using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class BookingController : ControllerBase
{
    private DataContext context { get; set; }

    public BookingController(DataContext context)
    {
        this.context = context;
    }

    [HttpGet("GetAllBookings")]
    public async Task<ActionResult<List<Booking>>> GetAllBookings()
    {
        return await context.Bookings.Include(b => b.timeslot).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Booking>> Create([FromBody] Booking booking)
    {
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        return Ok(booking);
    }

    [HttpDelete("DeleteBooking/{id}")]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        var booking = await context.Bookings.FindAsync(id);
        if (booking == null)
            return NotFound();

        context.Bookings.Remove(booking);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
