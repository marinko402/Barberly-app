using backend.Controllers;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class SalonControllerTests
{
    private DataContext _context;
    private SalonController _controller;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _controller = new SalonController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    // =========================================================
    // GET ALL SALONS
    // =========================================================

    [Test]
    public async Task GetAllSalons_ShouldReturnEmptyList_WhenThereAreNoSalons()
    {
        var result = await _controller.GetAllSalons();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAllSalons_ShouldReturnAllSalons()
    {
        var salon1 = new Salon
        {
            salonId = Guid.NewGuid(),
            name = "Salon Marko",
            address = "Niska 1",
            city = "Nis",
        };

        var salon2 = new Salon
        {
            salonId = Guid.NewGuid(),
            name = "Salon Petar",
            address = "Vozda Karadjordja 2",
            city = "Nis",
        };

        _context.Salons.AddRange(salon1, salon2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllSalons();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllSalons_ShouldReturnCorrectSalonData()
    {
        var salon = new Salon
        {
            salonId = Guid.NewGuid(),
            name = "Barber House",
            address = "Bulevar 10",
            city = "Belgrade",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllSalons();

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        Assert.That(result.Value[0].name, Is.EqualTo("Barber House"));
        Assert.That(result.Value[0].city, Is.EqualTo("Belgrade"));
    }

    // =========================================================
    // GET SALON BY ID
    // =========================================================

    [Test]
    public async Task GetSalonById_ShouldReturnSalon_WhenSalonExists()
    {
        var salon = new Salon
        {
            salonId = Guid.NewGuid(),
            name = "Test Salon",
            address = "Test Address",
            city = "Nis",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var result = await _controller.GetSalonById(salon.salonId);

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.salonId, Is.EqualTo(salon.salonId));
    }

    [Test]
    public async Task GetSalonById_ShouldReturnNotFound_WhenSalonDoesNotExist()
    {
        var result = await _controller.GetSalonById(Guid.NewGuid());

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetSalonById_ShouldReturnCorrectSalon()
    {
        var salon = new Salon
        {
            salonId = Guid.NewGuid(),
            name = "Elite Barber",
            address = "Bulevar Nemanjića 10",
            city = "Nis",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var result = await _controller.GetSalonById(salon.salonId);

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.name, Is.EqualTo("Elite Barber"));
        Assert.That(result.Value.address, Is.EqualTo("Bulevar Nemanjića 10"));
        Assert.That(result.Value.city, Is.EqualTo("Nis"));
    }

    // =========================================================
    // CREATE SALON
    // =========================================================

    [Test]
    public async Task CreateSalon_ShouldReturnBadRequest_WhenOwnerIsNull()
    {
        var dto = new SalonDto
        {
            name = "New Salon",
            address = "Niska 1",
            city = "Nis",
            owner = null,
        };

        var result = await _controller.CreateSalon(dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateSalon_ShouldReturnNotFound_WhenOwnerDoesNotExist()
    {
        var dto = new SalonDto
        {
            name = "New Salon",
            address = "Niska 1",
            city = "Nis",

            owner = new Barber
            {
                Id = "non-existing-owner",
                firstName = "noname",
                lastName = "noname",
            },
        };

        var result = await _controller.CreateSalon(dto);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateSalon_ShouldCreateSalon_WhenOwnerExists()
    {
        var owner = new Barber
        {
            Id = "owner123",
            UserName = "marko",
            firstName = "Marko",
            lastName = "Markovic",
        };

        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var dto = new SalonDto
        {
            name = "Marko Barber Shop",
            address = "Niska 25",
            city = "Nis",

            owner = new Barber
            {
                Id = "owner123",
                firstName = "noname",
                lastName = "noname",
            },
        };

        var result = await _controller.CreateSalon(dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var salonInDb = await _context.Salons.FirstOrDefaultAsync();

        Assert.That(salonInDb, Is.Not.Null);
        Assert.That(salonInDb.name, Is.EqualTo("Marko Barber Shop"));
        Assert.That(salonInDb.address, Is.EqualTo("Niska 25"));
        Assert.That(salonInDb.city, Is.EqualTo("Nis"));
    }

    // =========================================================
    // UPDATE SALON
    // =========================================================

    [Test]
    public async Task UpdateSalon_ShouldReturnNotFound_WhenSalonDoesNotExist()
    {
        var dto = new SalonDto
        {
            name = "Updated Salon",
            address = "New Address",
            city = "Nis",
        };

        var result = await _controller.UpdateSalon(Guid.NewGuid(), dto);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateSalon_ShouldReturnNoContent_WhenDataIsValid()
    {
        var id = Guid.NewGuid();

        var salon = new Salon
        {
            salonId = id,
            name = "Old Salon",
            address = "Old Address",
            city = "Nis",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var dto = new SalonDto
        {
            name = "New Salon",
            address = "New Address",
            city = "Belgrade",
        };

        var result = await _controller.UpdateSalon(id, dto);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task UpdateSalon_ShouldUpdateSalonInDatabase()
    {
        var id = Guid.NewGuid();

        var salon = new Salon
        {
            salonId = id,
            name = "Old Salon",
            address = "Old Address",
            city = "Nis",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var dto = new SalonDto
        {
            name = "Updated Salon",
            address = "Updated Address",
            city = "Belgrade",
        };

        await _controller.UpdateSalon(id, dto);

        var updatedSalon = await _context.Salons.FindAsync(id);

        Assert.That(updatedSalon, Is.Not.Null);
        Assert.That(updatedSalon.name, Is.EqualTo("Updated Salon"));
        Assert.That(updatedSalon.address, Is.EqualTo("Updated Address"));
        Assert.That(updatedSalon.city, Is.EqualTo("Belgrade"));
    }

    // =========================================================
    // DELETE SALON
    // =========================================================

    [Test]
    public async Task DeleteSalon_ShouldReturnNotFound_WhenSalonDoesNotExist()
    {
        var result = await _controller.DeleteSalon(Guid.NewGuid());

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteSalon_ShouldReturnNoContent_WhenSalonExists()
    {
        var id = Guid.NewGuid();

        var salon = new Salon
        {
            salonId = id,
            name = "Salon To Delete",
            address = "Address",
            city = "Nis",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteSalon(id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteSalon_ShouldRemoveSalonFromDatabase()
    {
        var id = Guid.NewGuid();

        var salon = new Salon
        {
            salonId = id,
            name = "Salon To Delete",
            address = "Address",
            city = "Nis",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        await _controller.DeleteSalon(id);

        var deletedSalon = await _context.Salons.FindAsync(id);

        Assert.That(deletedSalon, Is.Null);
    }

    // =========================================================
    // ADD BARBER TO SALON
    // =========================================================

    [Test]
    public async Task AddBarberToSalon_ShouldReturnNotFound_WhenBarberDoesNotExist()
    {
        var salonId = Guid.NewGuid();

        var salon = new Salon { salonId = salonId, name = "Test Salon" };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var result = await _controller.AddBarberToSalon(Guid.NewGuid(), salonId);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task AddBarberToSalon_ShouldReturnNotFound_WhenSalonDoesNotExist()
    {
        var barberId = Guid.NewGuid();

        var barber = new Barber
        {
            Id = barberId.ToString(),
            UserName = "marko",
            firstName = "noname",
            lastName = "noname",
        };

        _context.Barbers.Add(barber);
        await _context.SaveChangesAsync();

        var result = await _controller.AddBarberToSalon(barberId, Guid.NewGuid());

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task AddBarberToSalon_ShouldAddBarber_WhenBarberAndSalonExist()
    {
        var barberId = Guid.NewGuid();
        var salonId = Guid.NewGuid();

        var barber = new Barber
        {
            Id = barberId.ToString(),
            UserName = "marko",
            firstName = "noname",
            lastName = "noname",
        };

        var salon = new Salon { salonId = salonId, name = "Test Salon" };

        _context.Barbers.Add(barber);
        _context.Salons.Add(salon);

        await _context.SaveChangesAsync();

        var result = await _controller.AddBarberToSalon(barberId, salonId);

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var barberInDb = await _context.Barbers.FindAsync(barberId.ToString());

        Assert.That(barberInDb, Is.Not.Null);
        Assert.That(barberInDb.SalonId, Is.EqualTo(salonId));
    }

    // =========================================================
    // REMOVE BARBER FROM SALON
    // =========================================================

    [Test]
    public async Task RemoveBarberFromSalon_ShouldReturnNotFound_WhenSalonDoesNotExist()
    {
        var result = await _controller.RemoveBarberFromSalon(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "owner123"
        );

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task RemoveBarberFromSalon_ShouldReturnForbid_WhenUserIsNotOwner()
    {
        var salonId = Guid.NewGuid();

        var salon = new Salon
        {
            salonId = salonId,
            name = "Test Salon",
            OwnerId = "realOwner",
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var result = await _controller.RemoveBarberFromSalon(
            Guid.NewGuid(),
            salonId,
            "differentUser"
        );

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task RemoveBarberFromSalon_ShouldReturnNotFound_WhenBarberIsNotMember()
    {
        var salonId = Guid.NewGuid();

        var salon = new Salon
        {
            salonId = salonId,
            name = "Test Salon",
            OwnerId = "owner123",
            barbers = new List<Barber>(),
        };

        _context.Salons.Add(salon);
        await _context.SaveChangesAsync();

        var result = await _controller.RemoveBarberFromSalon(Guid.NewGuid(), salonId, "owner123");

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    // =========================================================
    // GET SALONS COUNT
    // =========================================================

    [Test]
    public async Task GetSalonsCount_ShouldReturnZero_WhenThereAreNoSalons()
    {
        var result = await _controller.GetSalonsCount();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetSalonsCount_ShouldReturnCorrectNumberOfSalons()
    {
        _context.Salons.AddRange(
            new Salon { salonId = Guid.NewGuid(), name = "Salon 1" },
            new Salon { salonId = Guid.NewGuid(), name = "Salon 2" },
            new Salon { salonId = Guid.NewGuid(), name = "Salon 3" }
        );

        await _context.SaveChangesAsync();

        var result = await _controller.GetSalonsCount();

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(3));
    }

    [Test]
    public async Task GetSalonsCount_ShouldIncreaseAfterAddingSalon()
    {
        var initialResult = await _controller.GetSalonsCount();

        var initialOk = initialResult.Result as OkObjectResult;

        Assert.That(initialOk!.Value, Is.EqualTo(0));

        _context.Salons.Add(new Salon { salonId = Guid.NewGuid(), name = "New Salon" });

        await _context.SaveChangesAsync();

        var finalResult = await _controller.GetSalonsCount();

        var finalOk = finalResult.Result as OkObjectResult;

        Assert.That(finalOk!.Value, Is.EqualTo(1));
    }

    // =========================================================
    // GET TOP SALONS
    // =========================================================

    [Test]
    public async Task GetTopSalons_ShouldReturnOk()
    {
        var result = await _controller.GetTopSalons();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetTopSalons_ShouldReturnEmptyList_WhenThereAreNoSalons()
    {
        var result = await _controller.GetTopSalons();

        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);

        var salons = okResult.Value as IEnumerable<object>;

        Assert.That(salons, Is.Not.Null);
    }

    [Test]
    public async Task GetTopSalons_ShouldReturnMaximumSixSalons()
    {
        for (int i = 0; i < 8; i++)
        {
            _context.Salons.Add(
                new Salon
                {
                    salonId = Guid.NewGuid(),
                    name = $"Salon {i}",
                    address = $"Address {i}",
                    city = "Nis",
                }
            );
        }

        await _context.SaveChangesAsync();

        var result = await _controller.GetTopSalons();

        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);

        var salons = okResult.Value as System.Collections.IEnumerable;

        Assert.That(salons, Is.Not.Null);

        int count = 0;

        foreach (var salon in salons)
        {
            count++;
        }

        Assert.That(count, Is.EqualTo(6));
    }
}
