using backend.Controllers;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace tests_nunit;

[TestFixture]
public class TimeslotControllerTests
{
    private DataContext _context = null!;
    private TimeslotController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _controller = new TimeslotController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    // ---------------------------------------------------------
    // Helper metode
    // ---------------------------------------------------------

    private Barber CreateBarber(string? id = null)
    {
        return new Barber
        {
            Id = id ?? Guid.NewGuid().ToString(),
            firstName = "Marko",
            lastName = "Markovic",
            Email = "marko@test.com",
            UserName = "marko",
        };
    }

    private Salon CreateSalon()
    {
        return new Salon
        {
            salonId = Guid.NewGuid(),
            name = "Test Salon",
            address = "Test Address",
            city = "Nis",
        };
    }

    private Timeslot CreateTimeslot(
        Barber barber,
        Salon salon,
        DateOnly? date = null,
        TimeOnly? startTime = null,
        int duration = 60,
        bool isBooked = false
    )
    {
        return new Timeslot
        {
            timeslotId = Guid.NewGuid(),
            date = date ?? new DateOnly(2026, 9, 10),
            startTime = startTime ?? new TimeOnly(10, 0),
            duration = duration,
            isBooked = isBooked,
            barber = barber,
            salon = salon,
        };
    }

    // =========================================================
    // GetAllTimeslots
    // =========================================================

    [Test]
    public async Task GetAllTimeslots_ShouldReturnAllTimeslots()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var ts1 = CreateTimeslot(barber, salon, startTime: new TimeOnly(10, 0));

        var ts2 = CreateTimeslot(barber, salon, startTime: new TimeOnly(11, 0));

        _context.Timeslots.AddRange(ts1, ts2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllTimeslots();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllTimeslots_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        var result = await _controller.GetAllTimeslots();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task GetAllTimeslots_ShouldReturnBookedAndFreeTimeslots()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var free = CreateTimeslot(barber, salon, startTime: new TimeOnly(10, 0), isBooked: false);

        var booked = CreateTimeslot(barber, salon, startTime: new TimeOnly(11, 0), isBooked: true);

        _context.Timeslots.AddRange(free, booked);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllTimeslots();

        Assert.That(result.Value!.Count, Is.EqualTo(2));
        Assert.That(result.Value!.Any(x => x.isBooked), Is.True);
        Assert.That(result.Value!.Any(x => !x.isBooked), Is.True);
    }

    // =========================================================
    // GetAllFreeTimeslots
    // =========================================================

    [Test]
    public async Task GetAllFreeTimeslots_ShouldReturnOnlyFreeTimeslots()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var free = CreateTimeslot(barber, salon, startTime: new TimeOnly(10, 0), isBooked: false);

        var booked = CreateTimeslot(barber, salon, startTime: new TimeOnly(11, 0), isBooked: true);

        _context.Timeslots.AddRange(free, booked);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllFreeTimeslots();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Count, Is.EqualTo(1));
        Assert.That(result.Value![0].isBooked, Is.False);
    }

    [Test]
    public async Task GetAllFreeTimeslots_WhenAllAreBooked_ShouldReturnEmptyList()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var ts1 = CreateTimeslot(barber, salon, startTime: new TimeOnly(10, 0), isBooked: true);

        var ts2 = CreateTimeslot(barber, salon, startTime: new TimeOnly(11, 0), isBooked: true);

        _context.Timeslots.AddRange(ts1, ts2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllFreeTimeslots();

        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task GetAllFreeTimeslots_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        var result = await _controller.GetAllFreeTimeslots();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Empty);
    }

    // =========================================================
    // CreateTimeslot
    // =========================================================

    [Test]
    public async Task CreateTimeslot_WhenBarberDoesNotExist_ShouldReturnBadRequest()
    {
        var dto = new TimeslotDto
        {
            date = new DateOnly(2026, 9, 10),
            startTime = new TimeOnly(10, 0),
            duration = 60,
            barberId = "sdgsdgdsg",
        };

        var result = await _controller.CreateTimeslot(dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result.Result as BadRequestObjectResult;

        Assert.That(badRequest!.Value, Is.EqualTo("Barber not found."));
    }

    [Test]
    public async Task CreateTimeslot_WhenBarberHasNoSalon_ShouldReturnBadRequest()
    {
        var barber = CreateBarber();

        _context.Barbers.Add(barber);
        await _context.SaveChangesAsync();

        var dto = new TimeslotDto
        {
            date = new DateOnly(2026, 9, 10),
            startTime = new TimeOnly(10, 0),
            duration = 60,
            barberId = barber.Id,
        };

        var result = await _controller.CreateTimeslot(dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result.Result as BadRequestObjectResult;

        Assert.That(badRequest!.Value, Is.EqualTo("Barber is not assigned to any salon."));
    }

    [Test]
    public async Task CreateTimeslot_WithValidData_ShouldCreateFreeTimeslot()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        barber.salon = salon;

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);

        await _context.SaveChangesAsync();

        var dto = new TimeslotDto
        {
            date = new DateOnly(2026, 9, 10),
            startTime = new TimeOnly(10, 0),
            duration = 60,
            barberId = barber.Id,

            // Čak i ako klijent pošalje true,
            // kontroler mora da napravi slot kao free.
            isBooked = true,
        };

        var result = await _controller.CreateTimeslot(dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;
        var created = okResult!.Value as Timeslot;

        Assert.That(created, Is.Not.Null);
        Assert.That(created!.isBooked, Is.False);
        Assert.That(created.date, Is.EqualTo(dto.date));
        Assert.That(created.startTime, Is.EqualTo(dto.startTime));
        Assert.That(created.duration, Is.EqualTo(dto.duration));

        var saved = await _context.Timeslots.FirstOrDefaultAsync();

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.isBooked, Is.False);
    }

    [Test]
    public async Task CreateTimeslot_WhenTimeslotOverlaps_ShouldReturnBadRequest()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        barber.salon = salon;

        var existing = CreateTimeslot(barber, salon, startTime: new TimeOnly(10, 0), duration: 60);

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(existing);

        await _context.SaveChangesAsync();

        var dto = new TimeslotDto
        {
            date = existing.date,
            startTime = new TimeOnly(10, 30),
            duration = 60,
            barberId = barber.Id,
        };

        var result = await _controller.CreateTimeslot(dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result.Result as BadRequestObjectResult;

        Assert.That(badRequest!.Value, Is.EqualTo("The timeslot overlaps with an existing one."));
    }

    // =========================================================
    // UpdateTimeslot
    // =========================================================

    [Test]
    public async Task UpdateTimeslot_WhenTimeslotDoesNotExist_ShouldReturnNotFound()
    {
        var dto = new TimeslotDto
        {
            date = new DateOnly(2026, 9, 10),
            startTime = new TimeOnly(12, 0),
            duration = 60,
            barberId = Guid.NewGuid().ToString(),
        };

        var result = await _controller.UpdateTimeslot(Guid.NewGuid(), dto);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        var notFound = result as NotFoundObjectResult;

        Assert.That(notFound!.Value, Is.EqualTo("Timeslot not found."));
    }

    [Test]
    public async Task UpdateTimeslot_WhenTimeslotIsBooked_ShouldReturnBadRequest()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var timeslot = CreateTimeslot(barber, salon, isBooked: true);

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);

        await _context.SaveChangesAsync();

        var dto = new TimeslotDto
        {
            date = timeslot.date,
            startTime = new TimeOnly(12, 0),
            duration = 60,
            barberId = barber.Id,
        };

        var result = await _controller.UpdateTimeslot(timeslot.timeslotId, dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result as BadRequestObjectResult;

        Assert.That(
            badRequest!.Value,
            Is.EqualTo("Cannot update a timeslot that has already been booked.")
        );
    }

    [Test]
    public async Task UpdateTimeslot_WithValidData_ShouldUpdateTimeslot()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var timeslot = CreateTimeslot(barber, salon, startTime: new TimeOnly(10, 0), duration: 60);

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);

        await _context.SaveChangesAsync();

        var dto = new TimeslotDto
        {
            date = new DateOnly(2026, 9, 11),
            startTime = new TimeOnly(14, 0),
            duration = 90,
            barberId = barber.Id,
        };

        var result = await _controller.UpdateTimeslot(timeslot.timeslotId, dto);

        Assert.That(result, Is.TypeOf<NoContentResult>());

        var updated = await _context.Timeslots.FirstAsync(t => t.timeslotId == timeslot.timeslotId);

        Assert.That(updated.date, Is.EqualTo(dto.date));
        Assert.That(updated.startTime, Is.EqualTo(dto.startTime));
        Assert.That(updated.duration, Is.EqualTo(dto.duration));
        Assert.That(updated.barber!.Id, Is.EqualTo(barber.Id));
    }

    [Test]
    public async Task UpdateTimeslot_WhenBarberDoesNotExist_ShouldReturnBadRequest()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var timeslot = CreateTimeslot(barber, salon);

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);

        await _context.SaveChangesAsync();

        var dto = new TimeslotDto
        {
            date = timeslot.date,
            startTime = new TimeOnly(13, 0),
            duration = 60,
            barberId = Guid.NewGuid().ToString(),
        };

        var result = await _controller.UpdateTimeslot(timeslot.timeslotId, dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result as BadRequestObjectResult;

        Assert.That(badRequest!.Value, Is.EqualTo("Barber not found."));
    }

    // =========================================================
    // DeleteTimeslot
    // =========================================================

    [Test]
    public async Task DeleteTimeslot_WhenTimeslotDoesNotExist_ShouldReturnNotFound()
    {
        var result = await _controller.DeleteTimeslot(Guid.NewGuid());

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        var notFound = result as NotFoundObjectResult;

        Assert.That(notFound!.Value, Is.EqualTo("Timeslot not found."));
    }

    [Test]
    public async Task DeleteTimeslot_WhenTimeslotIsBooked_ShouldReturnBadRequest()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var timeslot = CreateTimeslot(barber, salon, isBooked: true);

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);

        await _context.SaveChangesAsync();

        var result = await _controller.DeleteTimeslot(timeslot.timeslotId);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result as BadRequestObjectResult;

        Assert.That(badRequest!.Value, Is.EqualTo("You cannot delete a booked timeslot."));
    }

    [Test]
    public async Task DeleteTimeslot_WhenTimeslotIsFree_ShouldDeleteTimeslot()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var timeslot = CreateTimeslot(barber, salon, isBooked: false);

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);

        await _context.SaveChangesAsync();

        var id = timeslot.timeslotId;

        var result = await _controller.DeleteTimeslot(id);

        Assert.That(result, Is.TypeOf<NoContentResult>());

        var deleted = await _context.Timeslots.FirstOrDefaultAsync(t => t.timeslotId == id);

        Assert.That(deleted, Is.Null);
    }

    // =========================================================
    // GetBarberDailySchedule
    // =========================================================

    [Test]
    public async Task GetBarberDailySchedule_ShouldReturnOnlyTimeslotsForBarberAndDate()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();
        var otherBarber = CreateBarber();

        var date = new DateOnly(2026, 9, 10);

        var ts1 = CreateTimeslot(barber, salon, date, new TimeOnly(10, 0));

        var ts2 = CreateTimeslot(barber, salon, date, new TimeOnly(12, 0));

        // Drugi barber
        var ts3 = CreateTimeslot(otherBarber, salon, date, new TimeOnly(11, 0));

        // Isti barber, drugi datum
        var ts4 = CreateTimeslot(barber, salon, new DateOnly(2026, 9, 11), new TimeOnly(13, 0));

        _context.Salons.Add(salon);
        _context.Barbers.AddRange(barber, otherBarber);

        _context.Timeslots.AddRange(ts1, ts2, ts3, ts4);

        await _context.SaveChangesAsync();

        var result = await _controller.GetBarberDailySchedule(barber.Id, date);

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var okResult = result as OkObjectResult;

        var schedule = okResult!.Value as IEnumerable<object>;

        Assert.That(schedule, Is.Not.Null);
        Assert.That(schedule!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetBarberDailySchedule_ShouldReturnTimeslotsOrderedByStartTime()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var date = new DateOnly(2026, 9, 10);

        var late = CreateTimeslot(barber, salon, date, new TimeOnly(15, 0));

        var early = CreateTimeslot(barber, salon, date, new TimeOnly(9, 0));

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.AddRange(late, early);

        await _context.SaveChangesAsync();

        var result = await _controller.GetBarberDailySchedule(barber.Id, date);

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var okResult = result as OkObjectResult;

        var schedule = okResult!.Value as IEnumerable<dynamic>;

        Assert.That(schedule, Is.Not.Null);
    }

    [Test]
    public async Task GetBarberDailySchedule_WhenBookingExists_ShouldReturnCustomerData()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var date = new DateOnly(2026, 9, 10);

        var timeslot = CreateTimeslot(barber, salon, date, new TimeOnly(10, 0), 60, true);

        var booking = new Booking
        {
            bookingId = Guid.NewGuid(),
            timeslot = timeslot,
            customerFirstName = "Petar",
            customerLastName = "Petrovic",
            customerEmail = "petar@test.com",
            customerPhoneNumber = "0641234567",
        };

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var result = await _controller.GetBarberDailySchedule(barber.Id, date);

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var okResult = result as OkObjectResult;

        Assert.That(okResult!.Value, Is.Not.Null);

        var schedule = okResult.Value as IEnumerable<object>;

        Assert.That(schedule, Is.Not.Null);
        Assert.That(schedule!.Count(), Is.EqualTo(1));

        var item = schedule.First();

        var customerName = item.GetType().GetProperty("CustomerName")!.GetValue(item);

        var customerEmail = item.GetType().GetProperty("CustomerEmail")!.GetValue(item);

        var customerPhone = item.GetType().GetProperty("CustomerPhoneNumber")!.GetValue(item);

        Assert.That(customerName, Is.EqualTo("Petar Petrovic"));
        Assert.That(customerEmail, Is.EqualTo("petar@test.com"));
        Assert.That(customerPhone, Is.EqualTo("0641234567"));
    }

    // =========================================================
    // CancelBooking
    // =========================================================

    [Test]
    public async Task CancelBooking_WhenTimeslotDoesNotExist_ShouldReturnNotFound()
    {
        var result = await _controller.CancelBooking(Guid.NewGuid());

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        var notFound = result as NotFoundObjectResult;

        Assert.That(notFound!.Value, Is.EqualTo("Timeslot not found."));
    }

    [Test]
    public async Task CancelBooking_WhenTimeslotIsAlreadyFree_ShouldReturnBadRequest()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var timeslot = CreateTimeslot(barber, salon, isBooked: false);

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);

        await _context.SaveChangesAsync();

        var result = await _controller.CancelBooking(timeslot.timeslotId);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result as BadRequestObjectResult;

        Assert.That(badRequest!.Value, Is.EqualTo("Timeslot is already available."));
    }

    [Test]
    public async Task CancelBooking_WhenBookingExists_ShouldCancelBookingAndFreeTimeslot()
    {
        var salon = CreateSalon();
        var barber = CreateBarber();

        var timeslot = CreateTimeslot(barber, salon, isBooked: true);

        var booking = new Booking
        {
            bookingId = Guid.NewGuid(),
            timeslot = timeslot,
            customerFirstName = "Petar",
            customerLastName = "Petrovic",
            customerEmail = "petar@test.com",
            customerPhoneNumber = "0641234567",
        };

        _context.Salons.Add(salon);
        _context.Barbers.Add(barber);
        _context.Timeslots.Add(timeslot);
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var bookingId = booking.bookingId;
        var timeslotId = timeslot.timeslotId;

        var result = await _controller.CancelBooking(timeslotId);

        Assert.That(result, Is.TypeOf<NoContentResult>());

        var updatedTimeslot = await _context.Timeslots.FirstAsync(t => t.timeslotId == timeslotId);

        var deletedBooking = await _context.Bookings.FirstOrDefaultAsync(b =>
            b.bookingId == bookingId
        );

        Assert.That(updatedTimeslot.isBooked, Is.False);
        Assert.That(deletedBooking, Is.Null);
    }
}
