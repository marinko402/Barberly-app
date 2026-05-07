using backend.Data;
using backend.Dtos;
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

    [HttpPost("CreateBooking")]
    public async Task<ActionResult<Booking>> CreateBooking([FromBody] BookingDto bookingDto)
    {
        var timeslot = await context.Timeslots.FirstOrDefaultAsync(t =>
            t.timeslotId == bookingDto.timeslotId
        );

        if (timeslot == null)
            return NotFound("Termin ne postoji.");
        if (timeslot.isBooked)
            return BadRequest("Termin je već zauzet.");

        Booking booking = new Booking
        {
            timeslot = timeslot,
            customerFirstName = bookingDto.customerFirstName,
            customerLastName = bookingDto.customerLastName,
            customerEmail = bookingDto.customerEmail,
            customerPhoneNumber = bookingDto.customerPhoneNumber,
        };

        timeslot.isBooked = true;

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return Ok(booking);
    }

    [HttpDelete("DeleteBooking/{id}")]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        var booking = await context
            .Bookings.Include(b => b.timeslot)
            .FirstOrDefaultAsync(b => b.bookingId == id);

        if (booking == null)
            return NotFound();

        if (booking.timeslot != null)
        {
            booking.timeslot.isBooked = false;
        }

        context.Bookings.Remove(booking);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
