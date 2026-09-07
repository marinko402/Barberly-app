using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.ApiTests;

[TestFixture]
public class BarberApiTests : PlaywrightTest
{
    private IAPIRequestContext? Request = null;

    private static string? _createdBarberId;
    private static string? _createdBarberToken;

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

    [Test, Order(1)]
    public async Task GetAllBarbersTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var barbers = await Request.GetAsync("Barber/GetAllBarbers");

        if (barbers.Status != 200)
        {
            Assert.Fail($"Status Code: {barbers.Status} - Failed to fetch barbers.");
            return;
        }

        var jsonBarbers = await barbers.JsonAsync();

        if (!jsonBarbers.GetValueOrDefault().EnumerateArray().Any())
        {
            Assert.Fail("No barbers found in the response.");
            return;
        }

        var firstBarber = jsonBarbers.GetValueOrDefault().EnumerateArray().FirstOrDefault();

        if (
            firstBarber.TryGetProperty("id", out var id)
            && firstBarber.TryGetProperty("firstName", out var firstName)
            && firstBarber.TryGetProperty("lastName", out var lastName)
            && firstBarber.TryGetProperty("birthDate", out var birthDate)
            && firstBarber.TryGetProperty("salonId", out var salonId)
            && firstBarber.TryGetProperty("userName", out var userName)
            && firstBarber.TryGetProperty("email", out var email)
            && firstBarber.TryGetProperty("phoneNumber", out var phoneNumber)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(id.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(firstName.GetString(), Is.EqualTo("first name"));
                Assert.That(lastName.GetString(), Is.EqualTo("last name"));
                Assert.That(birthDate.GetString(), Is.EqualTo("2002-02-02"));
                Assert.That(salonId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(userName.GetString(), Is.EqualTo("username2"));
                Assert.That(email.GetString(), Is.EqualTo("username2@email.com"));
                Assert.That(phoneNumber.GetString(), Is.EqualTo("+381654435400"));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [
        TestCase(
            "barber ime",
            "barber prezime",
            "barber@email.com",
            "+381123456789",
            "barber",
            "#Barber123"
        ),
        Order(2)
    ]
    public async Task CreateBarberTest(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string userName,
        string password
    )
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var response = await Request.PostAsync(
            "Barber/CreateBarber",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    firstName,
                    lastName,
                    email,
                    phoneNumber,
                    userName,
                    password,
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText}"
        );

        var json = await response.JsonAsync();

        if (
            json.HasValue
            && json.Value.TryGetProperty("id", out var barberId)
            && json.Value.TryGetProperty("firstName", out var fn)
            && json.Value.TryGetProperty("lastName", out var ln)
            && json.Value.TryGetProperty("email", out var e)
            && json.Value.TryGetProperty("phoneNumber", out var pn)
            && json.Value.TryGetProperty("userName", out var un)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(barberId.GetString(), Is.Not.Null.And.Not.Empty);
                _createdBarberId = barberId.GetString();
                Assert.That(fn.GetString(), Is.EqualTo(firstName));
                Assert.That(ln.GetString(), Is.EqualTo(lastName));
                Assert.That(e.GetString(), Is.EqualTo(email));
                Assert.That(pn.GetString(), Is.EqualTo(phoneNumber));
                Assert.That(un.GetString(), Is.EqualTo(userName));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }

        var loginResponse = await Request.PostAsync(
            "api/Auth/Login",
            new APIRequestContextOptions() { DataObject = new { userName, password } }
        );

        Assert.That(
            loginResponse.Status,
            Is.EqualTo(200),
            $"Expected 200 OK for login, but got {loginResponse.Status}: {loginResponse.StatusText}"
        );

        var headers = loginResponse.Headers;
        if (headers.TryGetValue("set-cookie", out var setCookieHeader))
        {
            var jwtPart = setCookieHeader
                .Split(';')
                .FirstOrDefault(p => p.Trim().StartsWith("jwt="));

            if (jwtPart != null)
            {
                _createdBarberToken = jwtPart.Split('=')[1].Trim();
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(_createdBarberId, Is.Not.Null.And.Not.Empty, "Barber ID missing.");
            Assert.That(_createdBarberToken, Is.Not.Null.And.Not.Empty, "JWT cookie missing.");
        });
    }

    [Test, Order(3)]
    public async Task GetBarberByIdTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdBarberId,
            Is.Not.Null.And.Not.Empty,
            "No barber ID available from previous test."
        );

        var barber = await Request.GetAsync($"Barber/GetBarberById/{_createdBarberId}");

        if (barber.Status != 200)
        {
            Assert.Fail($"Status Code: {barber.Status} - Failed to fetch barber by ID.");
            return;
        }

        var jsonBarber = await barber.JsonAsync();

        if (!jsonBarber.HasValue)
        {
            Assert.Fail("No barber object returned in response.");
            return;
        }

        var b = jsonBarber.Value;

        if (
            b.TryGetProperty("id", out var barberId)
            && b.TryGetProperty("firstName", out var fn)
            && b.TryGetProperty("lastName", out var ln)
            && b.TryGetProperty("email", out var e)
            && b.TryGetProperty("phoneNumber", out var pn)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(barberId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(fn.GetString(), Is.EqualTo("barber ime"));
                Assert.That(ln.GetString(), Is.EqualTo("barber prezime"));
                Assert.That(e.GetString(), Is.EqualTo("barber@email.com"));
                Assert.That(pn.GetString(), Is.EqualTo("+381123456789"));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [Test, Order(4)]
    public async Task UpdateBarberTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdBarberId,
            Is.Not.Null.And.Not.Empty,
            "No barber ID available from previous test."
        );

        var response = await Request.PutAsync(
            $"Barber/UpdateBarber/{_createdBarberId}",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    firstName = "Barber Ime",
                    lastName = "Barber Prezime",
                    email = "barber@email.com",
                    phoneNumber = "+381123456798",
                },
            }
        );

        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test, Order(5)]
    public async Task DeleteBarberTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdBarberId,
            Is.Not.Null.And.Not.Empty,
            "No barber ID available from previous test."
        );
        Assert.That(
            _createdBarberToken,
            Is.Not.Null.And.Not.Empty,
            "No JWT token available from previous test."
        );

        Console.WriteLine($"Deleting barber with token: {_createdBarberToken}");

        var response = await Request!.DeleteAsync(
            $"Barber/DeleteBarber/{_createdBarberId}",
            new APIRequestContextOptions()
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {_createdBarberToken}" },
                },
            }
        );
        Assert.That(response.Status, Is.EqualTo(204));
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
