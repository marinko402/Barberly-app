using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.ApiTests;

[TestFixture]
public class SalonApiTests : PlaywrightTest
{
    private IAPIRequestContext? Request = null;

    private static string? _createdSalonId;
    private static string? _salonOwnerId;
    private static string? _salonBarberId;

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
    public async Task GetAllSalonsTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var salons = await Request.GetAsync("Salon/GetAllSalons");

        if (salons.Status != 200)
        {
            Assert.Fail($"Status Code: {salons.Status} - Failed to fetch salons.");
            return;
        }

        var jsonSalons = await salons.JsonAsync();

        if (!jsonSalons.GetValueOrDefault().EnumerateArray().Any())
        {
            Assert.Fail("No salons found in the response.");
            return;
        }

        var firstSalon = jsonSalons.GetValueOrDefault().EnumerateArray().FirstOrDefault();

        if (
            firstSalon.TryGetProperty("salonId", out var salonId)
            && firstSalon.TryGetProperty("name", out var name)
            && firstSalon.TryGetProperty("address", out var address)
            && firstSalon.TryGetProperty("city", out var city)
            && firstSalon.TryGetProperty("ownerId", out var ownerId)
            && firstSalon.TryGetProperty("owner", out var owner)
            && firstSalon.TryGetProperty("barbers", out var barbers)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(salonId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(name.GetString(), Is.EqualTo("Kvanto"));
                Assert.That(address.GetString(), Is.EqualTo("Dusanova 15"));
                Assert.That(city.GetString(), Is.EqualTo("Beograd"));
                Assert.That(ownerId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(owner.TryGetProperty("firstName", out var ownerName), Is.True);
                Assert.That(ownerName.GetString(), Is.EqualTo("first name"));
                Assert.That(barbers.GetArrayLength(), Is.GreaterThan(0));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [TestCase("Salon1", "Pobedina 25", "Nis"), Order(2)]
    public async Task CreateSalonTest(string name, string address, string city)
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var barbers = await Request!.GetAsync("Barber/GetAllBarbers");
        if (barbers.Status != 200)
        {
            Assert.Fail($"Status Code: {barbers.Status} - Failed to fetch barbers.");
            return;
        }

        var barbersJson = await barbers.JsonAsync();
        var barbersArray = barbersJson?.EnumerateArray().ToList();

        var validBarbers = barbersArray
            ?.Where(b =>
                b.TryGetProperty("salonId", out var salonIdProp)
                && salonIdProp.ValueKind == JsonValueKind.Null
            )
            .ToList();

        if (validBarbers == null || !validBarbers.Any())
        {
            Assert.Fail("There are no available barbers found in the database.");
            return;
        }

        var firstBarber = validBarbers!.FirstOrDefault();
        if (firstBarber.TryGetProperty("id", out var idProp))
        {
            _salonOwnerId = idProp.GetString();
        }

        if (string.IsNullOrEmpty(_salonOwnerId))
        {
            Assert.Fail("Salon owner ID is null or empty before sending POST request.");
            return;
        }

        var secondBarber = firstBarber;
        if (validBarbers!.Count() > 1)
        {
            secondBarber = validBarbers!.Skip(1).FirstOrDefault();
            if (secondBarber.TryGetProperty("id", out var id2))
                _salonBarberId = id2.GetString();
        }

        if (string.IsNullOrEmpty(_salonBarberId))
        {
            Assert.Fail("Salon barber ID is null or empty before sending POST request.");
            return;
        }

        var response = await Request.PostAsync(
            "Salon/CreateSalon",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    name,
                    address,
                    city,
                    owner = new
                    {
                        Id = _salonOwnerId,
                        firstName = firstBarber.GetProperty("firstName").GetString(),
                        lastName = firstBarber.GetProperty("lastName").GetString(),
                        userName = firstBarber.GetProperty("userName").GetString(),
                    },
                    barbers = new List<object>
                    {
                        new
                        {
                            Id = _salonOwnerId,
                            firstName = firstBarber.GetProperty("firstName").GetString(),
                            lastName = firstBarber.GetProperty("lastName").GetString(),
                            userName = firstBarber.GetProperty("userName").GetString(),
                        },
                        new
                        {
                            Id = _salonBarberId,
                            firstName = secondBarber.GetProperty("firstName").GetString(),
                            lastName = secondBarber.GetProperty("lastName").GetString(),
                            userName = secondBarber.GetProperty("userName").GetString(),
                        },
                    },
                },
            }
        );

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
            return;
        }

        var json = await response.JsonAsync();

        if (
            json.HasValue
            && json.Value.TryGetProperty("name", out var salonName)
            && json.Value.TryGetProperty("address", out var salonAddress)
            && json.Value.TryGetProperty("city", out var salonCity)
            && json.Value.TryGetProperty("owner", out var owner)
            && json.Value.TryGetProperty("barbers", out var salonBarbers)
        )
        {
            var responseOwnerId = owner.GetProperty("id").GetString();

            var responseBarberIds = salonBarbers
                .EnumerateArray()
                .Select(b => b.GetProperty("id").GetString())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(salonName.GetString(), Is.EqualTo(name));
                Assert.That(salonAddress.GetString(), Is.EqualTo(address));
                Assert.That(salonCity.GetString(), Is.EqualTo(city));

                Assert.That(responseOwnerId, Is.EqualTo(_salonOwnerId).IgnoreCase);

                Assert.That(responseBarberIds, Does.Contain(_salonBarberId));
                Assert.That(responseBarberIds, Does.Contain(_salonOwnerId));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }

        var allSalonsResponse = await Request.GetAsync("Salon/GetAllSalons");
        var allSalonsJson = await allSalonsResponse.JsonAsync();

        var createdSalon = allSalonsJson
            ?.EnumerateArray()
            .FirstOrDefault(s => s.GetProperty("name").GetString() == name);

        if (createdSalon != null && createdSalon.Value.ValueKind != JsonValueKind.Undefined)
        {
            _createdSalonId = createdSalon.Value.GetProperty("salonId").GetString();
            Assert.That(_createdSalonId, Is.Not.Null.And.Not.Empty);
        }
        else
        {
            Assert.Fail("Response object does not contain 'salonId' property.");
        }
    }

    [Test, Order(3)]
    public async Task GetSalonByIdTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdSalonId,
            Is.Not.Null.And.Not.Empty,
            "Salon ID is not set from the previous test."
        );

        var salon = await Request.GetAsync($"Salon/GetSalonById/{_createdSalonId}");

        if (salon.Status != 200)
        {
            Assert.Fail($"Status Code: {salon.Status} - Failed to fetch salon by ID.");
            return;
        }

        var jsonSalon = await salon.JsonAsync();

        if (!jsonSalon.HasValue)
        {
            Assert.Fail("No salon object returned in response.");
            return;
        }

        var firstSalon = jsonSalon.Value;

        if (
            firstSalon.TryGetProperty("salonId", out var salonId)
            && firstSalon.TryGetProperty("name", out var name)
            && firstSalon.TryGetProperty("address", out var address)
            && firstSalon.TryGetProperty("city", out var city)
            && firstSalon.TryGetProperty("ownerId", out var ownerId)
            && firstSalon.TryGetProperty("owner", out var owner)
            && firstSalon.TryGetProperty("barbers", out var barbers)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(salonId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(name.GetString(), Is.EqualTo("Salon1"));
                Assert.That(address.GetString(), Is.EqualTo("Pobedina 25"));
                Assert.That(city.GetString(), Is.EqualTo("Nis"));
                Assert.That(ownerId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(owner.TryGetProperty("firstName", out var ownerName), Is.True);
                Assert.That(ownerName.GetString(), Is.EqualTo("NikolaUpdated"));
                Assert.That(barbers.GetArrayLength(), Is.GreaterThan(0));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [Test, Order(4)]
    public async Task UpdateSalonTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdSalonId,
            Is.Not.Null.And.Not.Empty,
            "Salon ID is not set from the previous test."
        );

        var response = await Request.PutAsync(
            $"Salon/UpdateSalon/{_createdSalonId}",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    name = "Salon1Updated",
                    address = "Pobedina 25 Updated",
                    city = "Nis Updated",
                },
            }
        );

        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test, Order(5)]
    public async Task RemoveBarberFromSalonTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdSalonId,
            Is.Not.Null.And.Not.Empty,
            "Salon ID is not set from the previous test."
        );
        Assert.That(
            _salonBarberId,
            Is.Not.Null.And.Not.Empty,
            "Salon Barber ID is not set from the previous test."
        );
        Assert.That(
            _salonOwnerId,
            Is.Not.Null.And.Not.Empty,
            "Salon Owner ID is not set from the previous test."
        );

        var response = await Request!.PutAsync(
            "Salon/RemoveBarberFromSalon",
            new APIRequestContextOptions()
            {
                Params = new Dictionary<string, object>()
                {
                    { "barberId", _salonBarberId! },
                    { "salonId", _createdSalonId! },
                    { "ownerId", _salonOwnerId! },
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText}"
        );

        var responseText = await response.TextAsync();
        Assert.That(
            responseText.Trim('"'),
            Is.EqualTo("Barber successfully removed from the salon.")
        );
    }

    [Test, Order(6)]
    public async Task AddBarberToSalonTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdSalonId,
            Is.Not.Null.And.Not.Empty,
            "Salon ID is not set from the previous test."
        );
        Assert.That(
            _salonBarberId,
            Is.Not.Null.And.Not.Empty,
            "Salon Barber ID is not set from the previous test."
        );
        Assert.That(
            _salonOwnerId,
            Is.Not.Null.And.Not.Empty,
            "Salon Owner ID is not set from the previous test."
        );

        var response = await Request!.PutAsync(
            "Salon/AddBarberToSalon",
            new APIRequestContextOptions()
            {
                Params = new Dictionary<string, object>()
                {
                    { "barberId", _salonBarberId! },
                    { "salonId", _createdSalonId! },
                },
            }
        );

        Assert.That(
            response.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {response.Status}: {response.StatusText}"
        );

        var responseText = await response.TextAsync();
        Assert.That(responseText.Trim('"'), Is.EqualTo("Barber successfully added to the salon."));
    }

    [Test, Order(7)]
    public async Task GetSalonsCountTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var countResponse = await Request!.GetAsync("Salon/GetSalonsCount");
        Assert.That(countResponse.Status, Is.EqualTo(200));

        var countText = await countResponse.TextAsync();
        Assert.That(
            int.TryParse(countText, out var count),
            Is.True,
            "Response is not a valid integer."
        );
        Assert.That(count, Is.EqualTo(4));
    }

    [Test, Order(8)]
    public async Task GetTopSalonsTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var topSalonsResponse = await Request.GetAsync("Salon/GetTopSalons");
        Assert.That(topSalonsResponse.Status, Is.EqualTo(200));

        var json = await topSalonsResponse.JsonAsync();
        Assert.That(json.HasValue, Is.True, "Response JSON is null.");

        var salonsList = json!.Value.EnumerateArray().ToList();

        Assert.That(salonsList.Count, Is.LessThanOrEqualTo(6));

        if (salonsList.Any())
        {
            var firstSalon = salonsList.First();
            Assert.Multiple(() =>
            {
                Assert.That(firstSalon.TryGetProperty("salonId", out _), Is.True);
                Assert.That(firstSalon.TryGetProperty("name", out _), Is.True);
                Assert.That(firstSalon.TryGetProperty("staffCount", out _), Is.True);
                Assert.That(firstSalon.TryGetProperty("totalBookings", out _), Is.True);
            });

            var bookings = salonsList
                .Select(s => s.GetProperty("totalBookings").GetInt32())
                .ToList();

            var isSortedDescending = bookings.SequenceEqual(bookings.OrderByDescending(b => b));
            Assert.That(
                isSortedDescending,
                Is.True,
                "Salons are not sorted by TotalBookings in descending order."
            );
        }
    }

    [Test, Order(9)]
    public async Task DeleteSalonTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var response = await Request.DeleteAsync($"Salon/DeleteSalon/{_createdSalonId}");

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
