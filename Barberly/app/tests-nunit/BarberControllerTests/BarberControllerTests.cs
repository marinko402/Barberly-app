using System.Security.Claims;
using backend.Controllers;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace BarberControllerTests;

public class BarberControllerTests
{
    private DataContext _context;
    private BarberController _controller;
    private Mock<UserManager<Barber>> _userManagerMock;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);

        var userStoreMock = new Mock<IUserStore<Barber>>();

        _userManagerMock = new Mock<UserManager<Barber>>(
            userStoreMock.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        _controller = new BarberController(_context, _userManagerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    ////////////                 CREATE                      ////////////////////////////
    ///
    ///
    [Test]
    public async Task CreateBarber_ShouldReturnOk_WhenDataIsValid()
    {
        var dto = new BarberDto
        {
            firstName = "Marko",
            lastName = "Markovic",
            email = "marko@test.com",
            userName = "marko123",
            password = "Password123!",
            phoneNumber = "0601234567",
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<Barber>(), dto.password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<Barber>(), "Barber"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.CreateBarber(dto);

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);

        var barber = okResult!.Value as Barber;

        Assert.That(barber, Is.Not.Null);
        Assert.That(barber!.firstName, Is.EqualTo("Marko"));
        Assert.That(barber.lastName, Is.EqualTo("Markovic"));
        Assert.That(barber.Email, Is.EqualTo("marko@test.com"));

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<Barber>(), dto.password), Times.Once);

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<Barber>(), "Barber"), Times.Once);
    }

    [Test]
    public async Task CreateBarber_ShouldReturnBadRequest_WhenRequiredFieldIsMissing()
    {
        var dto = new BarberDto
        {
            firstName = "",
            lastName = "Markovic",
            email = "marko@test.com",
            userName = "marko123",
            password = "Password123!",
            phoneNumber = "0601234567",
        };

        var result = await _controller.CreateBarber(dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<Barber>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Test]
    public async Task CreateBarber_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        var result = await _controller.CreateBarber(null);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateBarber_ShouldReturnBadRequest_WhenUserManagerFails()
    {
        var dto = new BarberDto
        {
            firstName = "Marko",
            lastName = "Markovic",
            email = "marko@test.com",
            userName = "marko123",
            password = "Password123!",
            phoneNumber = "0601234567",
        };

        var errors = new[]
        {
            new IdentityError
            {
                Code = "DuplicateUserName",
                Description = "Username already exists.",
            },
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<Barber>(), dto.password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var result = await _controller.CreateBarber(dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<Barber>(), "Barber"), Times.Never);
    }

    [Test]
    public async Task CreateBarber_ShouldGenerateId_WhenDataIsValid()
    {
        var dto = new BarberDto
        {
            firstName = "Marko",
            lastName = "Markovic",
            email = "marko@test.com",
            userName = "marko123",
            password = "Password123!",
            phoneNumber = "0601234567",
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<Barber>(), dto.password))
            .Callback<Barber, string>(
                (barber, password) =>
                {
                    barber.Id = Guid.NewGuid().ToString();
                }
            )
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<Barber>(), "Barber"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.CreateBarber(dto);

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);

        var barber = okResult!.Value as Barber;

        Assert.That(barber, Is.Not.Null);
        Assert.That(barber!.Id, Is.Not.Null);
        Assert.That(barber.Id, Is.Not.Empty);
    }

    [Test]
    public async Task CreateBarber_ShouldCreateMultipleBarbers_WhenDataIsValid()
    {
        var dto1 = new BarberDto
        {
            firstName = "Marko",
            lastName = "Markovic",
            email = "marko@test.com",
            userName = "marko123",
            password = "Password123!",
            phoneNumber = "0601234567",
        };

        var dto2 = new BarberDto
        {
            firstName = "Petar",
            lastName = "Petrovic",
            email = "petar@test.com",
            userName = "petar123",
            password = "Password123!",
            phoneNumber = "0611234567",
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<Barber>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<Barber>(), "Barber"))
            .ReturnsAsync(IdentityResult.Success);

        var result1 = await _controller.CreateBarber(dto1);
        var result2 = await _controller.CreateBarber(dto2);

        Assert.That(result1.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(result2.Result, Is.TypeOf<OkObjectResult>());

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<Barber>(), It.IsAny<string>()),
            Times.Exactly(2)
        );

        _userManagerMock.Verify(
            x => x.AddToRoleAsync(It.IsAny<Barber>(), "Barber"),
            Times.Exactly(2)
        );
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

    ////////////                 DELETE                      ////////////////////////////
    ///
    ///
    [Test]
    public async Task DeleteBarber_ShouldReturnNoContent_WhenUserDeletesOwnAccount()
    {
        var barber = new Barber
        {
            Id = "barber123",
            UserName = "marko123",
            Email = "marko@test.com",
            firstName = "Marko",
            lastName = "Markovic",
        };

        SetLoggedInUser(barber.Id);

        _userManagerMock.Setup(x => x.FindByIdAsync(barber.Id)).ReturnsAsync(barber);

        _userManagerMock.Setup(x => x.DeleteAsync(barber)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.DeleteBarber(barber.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());

        _userManagerMock.Verify(x => x.FindByIdAsync(barber.Id), Times.Once);

        _userManagerMock.Verify(x => x.DeleteAsync(barber), Times.Once);
    }

    [Test]
    public async Task DeleteBarber_ShouldReturnForbid_WhenUserDeletesAnotherAccount()
    {
        var loggedInUserId = "barber123";
        var otherUserId = "barber456";

        SetLoggedInUser(loggedInUserId);

        var result = await _controller.DeleteBarber(otherUserId);

        Assert.That(result, Is.TypeOf<ForbidResult>());

        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);

        _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<Barber>()), Times.Never);
    }

    [Test]
    public async Task DeleteBarber_ShouldReturnNotFound_WhenBarberDoesNotExist()
    {
        var barberId = "barber123";

        SetLoggedInUser(barberId);

        _userManagerMock.Setup(x => x.FindByIdAsync(barberId)).ReturnsAsync((Barber?)null);

        var result = await _controller.DeleteBarber(barberId);

        Assert.That(result, Is.TypeOf<NotFoundResult>());

        _userManagerMock.Verify(x => x.FindByIdAsync(barberId), Times.Once);

        _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<Barber>()), Times.Never);
    }

    //                    POMOCNE FUNKCIJE

    private void SetLoggedInUser(string userId)
    {
        var claims = new List<Claim> { new Claim("id", userId) };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };
    }
}
