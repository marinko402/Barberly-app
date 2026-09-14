using backend.Controllers;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace tests_nunit;

[TestFixture]
public class BookingControllerTests
{
    private DataContext _context = null!;
    private BookingController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _controller = new BookingController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    // Helper

    private Timeslot CreateTimeslot(
        bool isBooked = false,
        DateOnly? date = null,
        TimeOnly? startTime = null,
        int duration = 60
    )
    {
        return new Timeslot
        {
            timeslotId = Guid.NewGuid(),
            date = date ?? new DateOnly(2026, 9, 10),
            startTime = startTime ?? new TimeOnly(10, 0),
            duration = duration,
            isBooked = isBooked,
        };
    }

    private Booking CreateBooking(
        Timeslot timeslot,
        string firstName = "Petar",
        string lastName = "Petrovic",
        string email = "petar@test.com",
        string? phone = "0641234567"
    )
    {
        return new Booking
        {
            bookingId = Guid.NewGuid(),
            timeslot = timeslot,
            customerFirstName = firstName,
            customerLastName = lastName,
            customerEmail = email,
            customerPhoneNumber = phone,
        };
    }

    // GET ALL

    [Test]
    public async Task GetAllBookings_ShouldReturnAllBookings()
    {
        var timeslot1 = CreateTimeslot();
        var timeslot2 = CreateTimeslot(startTime: new TimeOnly(11, 0));

        var booking1 = CreateBooking(timeslot1);
        var booking2 = CreateBooking(
            timeslot2,
            "Marko",
            "Markovic",
            "marko@test.com",
            "0651234567"
        );

        _context.Timeslots.AddRange(timeslot1, timeslot2);
        _context.Bookings.AddRange(booking1, booking2);

        await _context.SaveChangesAsync();

        var result = await _controller.GetAllBookings();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllBookings_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        var result = await _controller.GetAllBookings();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task GetAllBookings_ShouldReturnBookingWithTimeslot()
    {
        var timeslot = CreateTimeslot();

        var booking = CreateBooking(timeslot);

        _context.Timeslots.Add(timeslot);
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var result = await _controller.GetAllBookings();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Count, Is.EqualTo(1));

        var returnedBooking = result.Value.First();

        Assert.That(returnedBooking.timeslot, Is.Not.Null);
        Assert.That(returnedBooking.timeslot!.timeslotId, Is.EqualTo(timeslot.timeslotId));
    }

    // CREATE

    [Test]
    public async Task CreateBooking_WhenTimeslotDoesNotExist_ShouldReturnNotFound()
    {
        var dto = new BookingDto
        {
            timeslotId = Guid.NewGuid(),
            customerFirstName = "Petar",
            customerLastName = "Petrovic",
            customerEmail = "petar@test.com",
            customerPhoneNumber = "0641234567",
        };

        var result = await _controller.CreateBooking(dto);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());

        var notFound = result.Result as NotFoundObjectResult;

        Assert.That(notFound!.Value, Is.EqualTo("Termin ne postoji."));
    }

    [Test]
    public async Task CreateBooking_WhenTimeslotIsAlreadyBooked_ShouldReturnBadRequest()
    {
        var timeslot = CreateTimeslot(isBooked: true);

        _context.Timeslots.Add(timeslot);
        await _context.SaveChangesAsync();

        var dto = new BookingDto
        {
            timeslotId = timeslot.timeslotId,
            customerFirstName = "Petar",
            customerLastName = "Petrovic",
            customerEmail = "petar@test.com",
            customerPhoneNumber = "0641234567",
        };

        var result = await _controller.CreateBooking(dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result.Result as BadRequestObjectResult;

        Assert.That(badRequest!.Value, Is.EqualTo("Termin je već zauzet."));
    }

    [Test]
    public async Task CreateBooking_WithValidData_ShouldCreateBookingAndBookTimeslot()
    {
        var timeslot = CreateTimeslot(isBooked: false);

        _context.Timeslots.Add(timeslot);
        await _context.SaveChangesAsync();

        var dto = new BookingDto
        {
            timeslotId = timeslot.timeslotId,
            customerFirstName = "Petar",
            customerLastName = "Petrovic",
            customerEmail = "petar@test.com",
            customerPhoneNumber = "0641234567",
        };

        var result = await _controller.CreateBooking(dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;

        var createdBooking = okResult!.Value as Booking;

        Assert.That(createdBooking, Is.Not.Null);
        Assert.That(createdBooking!.customerFirstName, Is.EqualTo("Petar"));
        Assert.That(createdBooking.customerLastName, Is.EqualTo("Petrovic"));
        Assert.That(createdBooking.customerEmail, Is.EqualTo("petar@test.com"));

        var savedTimeslot = await _context.Timeslots.FirstAsync(t =>
            t.timeslotId == timeslot.timeslotId
        );

        var savedBooking = await _context.Bookings.FirstOrDefaultAsync(b =>
            b.bookingId == createdBooking.bookingId
        );

        Assert.That(savedBooking, Is.Not.Null);
        Assert.That(savedTimeslot.isBooked, Is.True);
    }

    // DELETE

    [Test]
    public async Task DeleteBooking_WhenBookingDoesNotExist_ShouldReturnNotFound()
    {
        var result = await _controller.DeleteBooking(Guid.NewGuid());

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteBooking_ShouldDeleteBooking()
    {
        var timeslot = CreateTimeslot(isBooked: true);

        var booking = CreateBooking(timeslot);

        _context.Timeslots.Add(timeslot);
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var bookingId = booking.bookingId;

        var result = await _controller.DeleteBooking(bookingId);

        Assert.That(result, Is.TypeOf<NoContentResult>());

        var deletedBooking = await _context.Bookings.FirstOrDefaultAsync(b =>
            b.bookingId == bookingId
        );

        Assert.That(deletedBooking, Is.Null);
    }

    [Test]
    public async Task DeleteBooking_ShouldFreeTimeslot()
    {
        var timeslot = CreateTimeslot(isBooked: true);

        var booking = CreateBooking(timeslot);

        _context.Timeslots.Add(timeslot);
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var timeslotId = timeslot.timeslotId;

        var result = await _controller.DeleteBooking(booking.bookingId);

        Assert.That(result, Is.TypeOf<NoContentResult>());

        var updatedTimeslot = await _context.Timeslots.FirstAsync(t => t.timeslotId == timeslotId);

        Assert.That(updatedTimeslot.isBooked, Is.False);
    }

    // GET TOTAL BOOKINGS COUNT

    [Test]
    public async Task GetTotalBookingsCount_ShouldReturnCorrectCount()
    {
        var timeslot1 = CreateTimeslot();
        var timeslot2 = CreateTimeslot(startTime: new TimeOnly(11, 0));

        var booking1 = CreateBooking(timeslot1);
        var booking2 = CreateBooking(timeslot2, "Marko", "Markovic", "marko@test.com");

        _context.Timeslots.AddRange(timeslot1, timeslot2);
        _context.Bookings.AddRange(booking1, booking2);

        await _context.SaveChangesAsync();

        var result = await _controller.GetTotalBookingsCount();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult!.Value, Is.EqualTo(2));
    }

    [Test]
    public async Task GetTotalBookingsCount_WhenDatabaseIsEmpty_ShouldReturnZero()
    {
        var result = await _controller.GetTotalBookingsCount();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult!.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetTotalBookingsCount_ShouldIncreaseAfterAddingBooking()
    {
        var timeslot = CreateTimeslot();

        var booking = CreateBooking(timeslot);

        _context.Timeslots.Add(timeslot);
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var result = await _controller.GetTotalBookingsCount();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult!.Value, Is.EqualTo(1));
    }
}
