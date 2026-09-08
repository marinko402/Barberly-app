using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.ApiTests;

[TestFixture]
public class AuthApiTests : PlaywrightTest
{
    private IAPIRequestContext? Request = null;

    private static string? _userId;
    private static string? _userToken;

    [SetUp]
    public async Task Setup()
    {
        var headers = new Dictionary<string, string>
        {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" },
        };

        Request = await Playwright.APIRequest.NewContextAsync(
            new()
            {
                BaseURL = "https://localhost:7035/",
                ExtraHTTPHeaders = headers,
                IgnoreHTTPSErrors = true,
            }
        );
    }

    [
        TestCase(
            "testUsername",
            "test@email.com",
            "#Test123",
            "ime",
            "prezime",
            "+381123456789",
            "2000-01-01"
        ),
        Order(1)
    ]
    public async Task RegisterTest(
        string userName,
        string email,
        string password,
        string firstName,
        string lastName,
        string phoneNumber,
        string birthDate
    )
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var response = await Request.PostAsync(
            "api/Auth/Register",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    userName,
                    email,
                    password,
                    firstName,
                    lastName,
                    phoneNumber,
                    birthDate,
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (json.HasValue && json.Value.TryGetProperty("message", out var message))
        {
            Assert.That(message.GetString(), Is.EqualTo("Barber registered successfully"));
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [TestCase("testUsername", "#Test123"), Order(2)]
    public async Task LoginTest(string userName, string password)
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var loginResponse = await Request.PostAsync(
            "api/Auth/Login",
            new APIRequestContextOptions() { DataObject = new { userName, password } }
        );

        Assert.That(
            loginResponse.Status,
            Is.EqualTo(200),
            $"Expected 200 OK for login, but got {loginResponse.Status}: {loginResponse.StatusText} - {await loginResponse.TextAsync()}"
        );

        var headers = loginResponse.Headers;
        if (headers.TryGetValue("set-cookie", out var setCookieHeader))
        {
            var jwtPart = setCookieHeader
                .Split(';')
                .FirstOrDefault(p => p.Trim().StartsWith("jwt="));

            if (jwtPart != null)
            {
                _userToken = jwtPart.Split('=')[1].Trim();
            }
        }

        var json = await loginResponse.JsonAsync();

        if (
            json.HasValue
            && json.Value.TryGetProperty("roles", out var roles)
            && json.Value.TryGetProperty("userId", out var id)
            && json.Value.TryGetProperty("username", out var username)
        )
        {
            var rolesList = roles.EnumerateArray().Select(r => r.GetString()).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(id.GetString(), Is.Not.Null.And.Not.Empty);
                _userId = id.GetString();
                Assert.That(username.GetString(), Is.EqualTo(userName));
                Assert.That(rolesList, Is.Not.Empty, "Roles array is empty.");
                Assert.That(rolesList, Contains.Item("Barber"), "User does not have Barber role.");
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }

        Assert.Multiple(() =>
        {
            Assert.That(_userId, Is.Not.Null.And.Not.Empty, "Barber ID missing.");
            Assert.That(_userToken, Is.Not.Null.And.Not.Empty, "JWT cookie missing.");
        });
    }

    [Test, Order(3)]
    public async Task GetCurrentUserTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _userToken,
            Is.Not.Null.And.Not.Empty,
            "No JWT token available from previous test."
        );

        var response = await Request.GetAsync(
            "api/Auth/Me",
            new APIRequestContextOptions()
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {_userToken}" },
                    { "Cookie", $"jwt={_userToken}" },
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (!json.HasValue)
        {
            Assert.Fail("No user object returned in response.");
            return;
        }

        var b = json.Value;

        if (
            b.TryGetProperty("id", out var barberId)
            && b.TryGetProperty("firstName", out var fn)
            && b.TryGetProperty("lastName", out var ln)
            && b.TryGetProperty("email", out var e)
            && b.TryGetProperty("phoneNumber", out var pn)
            && b.TryGetProperty("birthDate", out var bd)
            && b.TryGetProperty("userName", out var un)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(barberId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(fn.GetString(), Is.EqualTo("ime"));
                Assert.That(ln.GetString(), Is.EqualTo("prezime"));
                Assert.That(e.GetString(), Is.EqualTo("test@email.com"));
                Assert.That(pn.GetString(), Is.EqualTo("+381123456789"));
                Assert.That(bd.GetString(), Is.EqualTo("2000-01-01"));
                Assert.That(un.GetString(), Is.EqualTo("testUsername"));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [Test, Order(4)]
    public async Task GetUserDataTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(_userId, Is.Not.Null.And.Not.Empty, "No user ID available from previous test.");

        var response = await Request.GetAsync($"api/Auth/GetUserData/{_userId}");

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (!json.HasValue)
        {
            Assert.Fail("No user object returned in response.");
            return;
        }

        var b = json.Value;

        if (
            b.TryGetProperty("id", out var id)
            && b.TryGetProperty("firstName", out var fn)
            && b.TryGetProperty("lastName", out var ln)
            && b.TryGetProperty("email", out var e)
            && b.TryGetProperty("phoneNumber", out var pn)
            && b.TryGetProperty("birthDate", out var bd)
            && b.TryGetProperty("userName", out var un)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(id.GetString(), Is.EqualTo(_userId));
                Assert.That(fn.GetString(), Is.EqualTo("ime"));
                Assert.That(ln.GetString(), Is.EqualTo("prezime"));
                Assert.That(e.GetString(), Is.EqualTo("test@email.com"));
                Assert.That(pn.GetString(), Is.EqualTo("+381123456789"));
                Assert.That(bd.GetString(), Is.EqualTo("2000-01-01"));
                Assert.That(un.GetString(), Is.EqualTo("testUsername"));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [
        TestCase(
            "testUsernameUpdated",
            "testUpdated@email.com",
            "imeUpdated",
            "prezimeUpdated",
            "+381123456798",
            "2002-08-01"
        ),
        Order(5)
    ]
    public async Task UpdateUserDataTest(
        string userName,
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        string birthDate
    )
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(_userId, Is.Not.Null.And.Not.Empty, "No user ID available from previous test.");

        var response = await Request.PutAsync(
            "api/Auth/UpdateUser",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    id = _userId,
                    userName,
                    email,
                    firstName,
                    lastName,
                    phoneNumber,
                    birthDate,
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (!json.HasValue)
        {
            Assert.Fail("No user object returned in response.");
            return;
        }

        var b = json.Value;

        if (
            b.TryGetProperty("id", out var id)
            && b.TryGetProperty("firstName", out var fn)
            && b.TryGetProperty("lastName", out var ln)
            && b.TryGetProperty("email", out var e)
            && b.TryGetProperty("phoneNumber", out var pn)
            && b.TryGetProperty("birthDate", out var bd)
            && b.TryGetProperty("userName", out var un)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(id.GetString(), Is.EqualTo(_userId));
                Assert.That(fn.GetString(), Is.EqualTo(firstName));
                Assert.That(ln.GetString(), Is.EqualTo(lastName));
                Assert.That(e.GetString(), Is.EqualTo(email));
                Assert.That(pn.GetString(), Is.EqualTo(phoneNumber));
                Assert.That(bd.GetString(), Is.EqualTo(birthDate));
                Assert.That(un.GetString(), Is.EqualTo(userName));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [TestCase("#Test123"), Order(6)]
    public async Task CheckPasswordTest(string password)
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(_userId, Is.Not.Null.And.Not.Empty, "No user ID available from previous test.");

        var response = await Request.PostAsync(
            "api/Auth/CheckPassword",
            new APIRequestContextOptions() { DataObject = new { userId = _userId, password } }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (json.HasValue && json.Value.TryGetProperty("message", out var message))
        {
            Assert.That(message.GetString(), Is.EqualTo("Password is correct"));
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [TestCase("#Test123", "#Test123Updated"), Order(7)]
    public async Task ChangePasswordTest(string currentPassword, string newPassword)
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(_userId, Is.Not.Null.And.Not.Empty, "No user ID available from previous test.");

        var response = await Request.PostAsync(
            "api/Auth/ChangePassword",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    userId = _userId,
                    currentPassword,
                    newPassword,
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (json.HasValue && json.Value.TryGetProperty("message", out var message))
        {
            Assert.That(message.GetString(), Is.EqualTo("Password changed successfully"));
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [TestCase("testUsernameUpdated"), Order(8)]
    public async Task GetByUsernameTest(string username)
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(_userId, Is.Not.Null.And.Not.Empty, "No user ID available from previous test.");

        var response = await Request.GetAsync($"api/Auth/GetByUsername/{username}");

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (!json.HasValue)
        {
            Assert.Fail("No user object returned in response.");
            return;
        }

        var b = json.Value;

        if (
            b.TryGetProperty("id", out var id)
            && b.TryGetProperty("firstName", out var fn)
            && b.TryGetProperty("lastName", out var ln)
            && b.TryGetProperty("email", out var e)
            && b.TryGetProperty("phoneNumber", out var pn)
            && b.TryGetProperty("birthDate", out var bd)
            && b.TryGetProperty("userName", out var un)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(id.GetString(), Is.EqualTo(_userId));
                Assert.That(fn.GetString(), Is.EqualTo("imeUpdated"));
                Assert.That(ln.GetString(), Is.EqualTo("prezimeUpdated"));
                Assert.That(e.GetString(), Is.EqualTo("testUpdated@email.com"));
                Assert.That(pn.GetString(), Is.EqualTo("+381123456798"));
                Assert.That(bd.GetString(), Is.EqualTo("2002-08-01"));
                Assert.That(un.GetString(), Is.EqualTo(username));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [Test, Order(9)]
    public async Task DeleteUserTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(_userId, Is.Not.Null.And.Not.Empty, "No user ID available from previous test.");
        Assert.That(
            _userToken,
            Is.Not.Null.And.Not.Empty,
            "No JWT token available from previous test."
        );

        var response = await Request!.DeleteAsync(
            $"Barber/DeleteBarber/{_userId}",
            new APIRequestContextOptions()
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {_userToken}" },
                    { "Cookie", $"jwt={_userToken}" },
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(204),
            $"Expected 204 No Content, got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );
    }

    [Test, Order(10)]
    public async Task LogoutTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var response = await Request.PostAsync("api/Auth/Logout");

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText} - {await response.TextAsync()}"
        );

        var json = await response.JsonAsync();

        if (json.HasValue && json.Value.TryGetProperty("message", out var message))
        {
            Assert.That(message.GetString(), Is.EqualTo("Logged out successfully"));
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [TearDown]
    public async Task End()
    {
        if (Request != null)
        {
            await Request.DisposeAsync();
            Request = null;
        }
    }
}
