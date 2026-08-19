using System.Security.Claims;
using backend.Controllers;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AuthControllerTests
{
    private Mock<UserManager<Barber>> _userManager;
    private Mock<IUserStore<Barber>> _userStore;
    private IConfiguration _configuration;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _userStore = new Mock<IUserStore<Barber>>();

        _userManager = new Mock<UserManager<Barber>>(
            _userStore.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var configurationData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "this-is-a-very-long-secret-key-for-testing-123456789",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpiresInMinutes"] = "60",
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        _controller = new AuthController(_userManager.Object, _configuration);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };
    }

    // =========================================================
    // REGISTER
    // =========================================================

    [Test]
    public async Task Register_ShouldReturnOk_WhenDataIsValid()
    {
        var dto = new RegisterDto
        {
            UserName = "marko",
            Email = "marko@test.com",
            Password = "Password123!",
            PhoneNumber = "0601234567",
            FirstName = "Marko",
            LastName = "Markovic",
        };

        _userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync((Barber)null);

        _userManager.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((Barber)null);

        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<Barber>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<Barber>(), "Barber"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.Register(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        var dto = new RegisterDto
        {
            UserName = "marko",
            Email = "marko@test.com",
            Password = "Password123!",
        };

        var existingUser = new Barber
        {
            UserName = "marko",
            firstName = "test",
            lastName = "test",
        };

        _userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync(existingUser);

        var result = await _controller.Register(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenBirthDateIsInvalid()
    {
        var dto = new RegisterDto
        {
            UserName = "marko",
            Email = "marko@test.com",
            Password = "Password123!",
            BirthDate = "invalid-date",
        };

        _userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync((Barber)null);

        _userManager.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((Barber)null);

        var result = await _controller.Register(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [Test]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        var user = new Barber
        {
            Id = "123",
            UserName = "marko",
            Email = "marko@test.com",
            firstName = "test",
            lastName = "test",
        };

        var dto = new LoginDto { UserName = "marko", Password = "Password123!" };

        _userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync(user);

        _userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

        _userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Barber" });

        var result = await _controller.Login(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenUsernameDoesNotExist()
    {
        var dto = new LoginDto { UserName = "unknown", Password = "Password123!" };

        _userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync((Barber)null);

        var result = await _controller.Login(dto);

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        var user = new Barber
        {
            Id = "123",
            UserName = "marko",
            Email = "marko@test.com",
            firstName = "test",
            lastName = "test",
        };

        var dto = new LoginDto { UserName = "marko", Password = "WrongPassword" };

        _userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync(user);

        _userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

        var result = await _controller.Login(dto);

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    [Test]
    public void Logout_ShouldReturnOk()
    {
        var result = _controller.Logout();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public void Logout_ShouldDeleteJwtCookie()
    {
        _controller.Response.Cookies.Append("jwt", "test-token");

        var result = _controller.Logout();

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        Assert.That(_controller.Response.Headers.ContainsKey("Set-Cookie"), Is.True);
    }

    [Test]
    public void Logout_ShouldWork_WhenNoCookieExists()
    {
        var result = _controller.Logout();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    // =========================================================
    // GET CURRENT USER
    // =========================================================

    [Test]
    public async Task GetCurrentUser_ShouldReturnOk_WhenUserExists()
    {
        var user = new Barber
        {
            Id = "123",
            UserName = "marko",
            Email = "marko@test.com",
            firstName = "test",
            lastName = "test",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("id", "123") }, "TestAuth")
        );

        var result = await _controller.GetCurrentUser();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenIdClaimIsMissing()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await _controller.GetCurrentUser();

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync((Barber)null);

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("id", "123") }, "TestAuth")
        );

        var result = await _controller.GetCurrentUser();

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    // =========================================================
    // GET USER DATA
    // =========================================================

    [Test]
    public async Task GetUserData_ShouldReturnOk_WhenUserExists()
    {
        var user = new Barber
        {
            Id = "123",
            UserName = "marko",
            firstName = "test",
            lastName = "test",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        var result = await _controller.GetUserData("123");

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetUserData_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync((Barber)null);

        var result = await _controller.GetUserData("123");

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetUserData_ShouldReturnNotFound_WhenIdIsEmpty()
    {
        _userManager.Setup(x => x.FindByIdAsync("")).ReturnsAsync((Barber)null);

        var result = await _controller.GetUserData("");

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    // =========================================================
    // UPDATE USER
    // =========================================================

    [Test]
    public async Task UpdateUser_ShouldReturnOk_WhenDataIsValid()
    {
        var user = new Barber
        {
            Id = "123",
            UserName = "marko",
            Email = "old@test.com",
            firstName = "test",
            lastName = "test",
        };

        var dto = new UpdateUserDto
        {
            Id = "123",
            UserName = "markonovi",
            Email = "new@test.com",
            FirstName = "Marko",
            LastName = "Markovic",
            PhoneNumber = "0601234567",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        _userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync((Barber)null);

        _userManager.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((Barber)null);

        _userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.UpdateUser(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        Assert.Multiple(() =>
        {
            Assert.That(user.UserName, Is.EqualTo("markonovi"));
            Assert.That(user.Email, Is.EqualTo("new@test.com"));
            Assert.That(user.firstName, Is.EqualTo("Marko"));
            Assert.That(user.lastName, Is.EqualTo("Markovic"));
        });
    }

    [Test]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var dto = new UpdateUserDto
        {
            Id = "123",
            UserName = "marko",
            Email = "marko@test.com",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync((Barber)null);

        var result = await _controller.UpdateUser(dto);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        var user = new Barber
        {
            Id = "123",
            UserName = "marko",
            Email = "marko@test.com",
            firstName = "test",
            lastName = "test",
        };

        var otherUser = new Barber
        {
            Id = "456",
            UserName = "petar",
            firstName = "test",
            lastName = "test",
        };

        var dto = new UpdateUserDto
        {
            Id = "123",
            UserName = "petar",
            Email = "marko@test.com",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        _userManager.Setup(x => x.FindByNameAsync("petar")).ReturnsAsync(otherUser);

        var result = await _controller.UpdateUser(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    // =========================================================
    // CHECK PASSWORD
    // =========================================================

    [Test]
    public async Task CheckPassword_ShouldReturnOk_WhenPasswordIsCorrect()
    {
        var user = new Barber
        {
            Id = "123",
            firstName = "test",
            lastName = "test",
        };

        var dto = new CheckPasswordDto { UserId = "123", Password = "Password123!" };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        _userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

        var result = await _controller.CheckPassword(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CheckPassword_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        var user = new Barber
        {
            Id = "123",
            firstName = "test",
            lastName = "test",
        };

        var dto = new CheckPasswordDto { UserId = "123", Password = "WrongPassword" };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        _userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

        var result = await _controller.CheckPassword(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CheckPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var dto = new CheckPasswordDto { UserId = "123", Password = "Password123!" };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync((Barber)null);

        var result = await _controller.CheckPassword(dto);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    // =========================================================
    // CHANGE PASSWORD
    // =========================================================

    [Test]
    public async Task ChangePassword_ShouldReturnOk_WhenPasswordChangeSucceeds()
    {
        var user = new Barber
        {
            Id = "123",
            firstName = "test",
            lastName = "test",
        };

        var dto = new ChangePasswordDto
        {
            UserId = "123",
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        _userManager
            .Setup(x => x.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.ChangePassword(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task ChangePassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var dto = new ChangePasswordDto
        {
            UserId = "123",
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync((Barber)null);

        var result = await _controller.ChangePassword(dto);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenCurrentPasswordIsIncorrect()
    {
        var user = new Barber
        {
            Id = "123",
            firstName = "test",
            lastName = "test",
        };

        var dto = new ChangePasswordDto
        {
            UserId = "123",
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123!",
        };

        _userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);

        _userManager
            .Setup(x => x.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
            .ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "Incorrect password" })
            );

        var result = await _controller.ChangePassword(dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    // =========================================================
    // GET BY USERNAME
    // =========================================================

    [Test]
    public async Task GetByUsername_ShouldReturnOk_WhenUserExists()
    {
        var user = new Barber
        {
            Id = "123",
            UserName = "marko",
            firstName = "test",
            lastName = "test",
        };

        _userManager.Setup(x => x.FindByNameAsync("marko")).ReturnsAsync(user);

        var result = await _controller.GetByUsername("marko");

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetByUsername_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userManager.Setup(x => x.FindByNameAsync("unknown")).ReturnsAsync((Barber)null);

        var result = await _controller.GetByUsername("unknown");

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetByUsername_ShouldReturnBadRequest_WhenUsernameIsEmpty()
    {
        var result = await _controller.GetByUsername("");

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
}
