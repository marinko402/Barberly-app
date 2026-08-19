using System.Threading.Tasks;
using backend.Controllers;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class BarberControllerTests
{
    private DataContext _context;
    private BarberController _controller;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _controller = new BarberController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task CreateBarber_ShouldReturnOk_AndSaveBarber()
    {
        var barberDto = new BarberDto
        {
            firstName = "Marko",
            lastName = "Markovic",
            email = "marko@test.com",
        };

        var result = await _controller.CreateBarber(barberDto);

        Assert.That(result, Is.Not.Null);

        var okResult = result.Result as Microsoft.AspNetCore.Mvc.OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var barber = okResult.Value as Barber;
        Assert.That(barber, Is.Not.Null);

        Assert.That(barber.firstName, Is.EqualTo("Marko"));
        Assert.That(barber.lastName, Is.EqualTo("Markovic"));
        Assert.That(barber.Email, Is.EqualTo("marko@test.com"));

        var barberInDb = await _context.Barbers.FirstOrDefaultAsync();
        Assert.That(barberInDb, Is.Not.Null);
        Assert.That(barberInDb.firstName, Is.EqualTo("Marko"));
    }

    [Test]
    public async Task CreateBarber_ShouldNotSaveBarber_WhenFirstNameIsEmpty()
    {
        var dto = new BarberDto
        {
            firstName = "",
            lastName = "Petrovic",
            email = "petar@test.com",
        };

        await _controller.CreateBarber(dto);

        var barber = await _context.Barbers.FirstOrDefaultAsync();

        Assert.That(barber, Is.Null);
    }

    [Test]
    public async Task CreateBarber_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        var result = await _controller.CreateBarber(null);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateBarber_ShouldGenerateId_WhenDataIsValid()
    {
        var dto = new BarberDto
        {
            firstName = "Marko",
            lastName = "Markovic",
            email = "marko@test.com",
        };

        var result = await _controller.CreateBarber(dto);

        var okResult = result.Result as OkObjectResult;
        var barber = okResult.Value as Barber;

        Assert.That(barber.Id, Is.Not.Null);
        Assert.That(barber.Id, Is.Not.Empty);
    }

    [Test]
    public async Task CreateBarber_ShouldSaveMultipleBarbers()
    {
        var barber1 = new BarberDto
        {
            firstName = "Marko",
            lastName = "Markovic",
            email = "marko@test.com",
        };

        var barber2 = new BarberDto
        {
            firstName = "Petar",
            lastName = "Petrovic",
            email = "petar@test.com",
        };

        await _controller.CreateBarber(barber1);
        await _controller.CreateBarber(barber2);

        var count = await _context.Barbers.CountAsync();

        Assert.That(count, Is.EqualTo(2));
    }

    ////////////                 UPDATE                      ////////////////////////////
    ///
    ///
    [Test]
    public async Task UpdateBarber_ShouldUpdateBarber_WhenDataIsValid()
    {
        var barber = new Barber
        {
            Id = Guid.NewGuid().ToString(),
            firstName = "Marko",
            lastName = "Markovic",
            Email = "old@test.com",
        };

        _context.Barbers.Add(barber);
        await _context.SaveChangesAsync();

        var dto = new BarberDto
        {
            firstName = "Petar",
            lastName = "Petrovic",
            email = "new@test.com",
            phoneNumber = "123456789",
        };

        var result = await _controller.UpdateBarber(barber.Id, dto);

        Assert.That(result, Is.TypeOf<NoContentResult>());

        var updated = await _context.Barbers.FindAsync(barber.Id);

        Assert.That(updated.firstName, Is.EqualTo("Petar"));
        Assert.That(updated.lastName, Is.EqualTo("Petrovic"));
        Assert.That(updated.Email, Is.EqualTo("new@test.com"));
    }

    [Test]
    public async Task UpdateBarber_ShouldReturnNotFound_WhenBarberDoesNotExist()
    {
        var id = Guid.NewGuid().ToString();

        var dto = new BarberDto
        {
            firstName = "Petar",
            lastName = "Petrovic",
            email = "petar@test.com",
        };

        var result = await _controller.UpdateBarber(id, dto);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateBarber_ShouldReturnBadRequest_WhenFieldsAreEmpty()
    {
        var barber = new Barber
        {
            Id = Guid.NewGuid().ToString(),
            firstName = "Marko",
            lastName = "Markovic",
            Email = "marko@test.com",
        };

        _context.Barbers.Add(barber);
        await _context.SaveChangesAsync();

        var dto = new BarberDto
        {
            firstName = "",
            lastName = "Petrovic",
            email = "petar@test.com",
        };

        var result = await _controller.UpdateBarber(barber.Id, dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateBarber_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        var barber = new Barber
        {
            Id = Guid.NewGuid().ToString(),
            firstName = "Marko",
            lastName = "Markovic",
            Email = "marko@test.com",
        };

        _context.Barbers.Add(barber);
        await _context.SaveChangesAsync();

        var result = await _controller.UpdateBarber(barber.Id, null);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
