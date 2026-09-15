using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.ApiTests;

[TestFixture]
public class TimeslotApiTests : PlaywrightTest
{
    private IAPIRequestContext? Request = null;

    private static string? _createdTimeslotId;
    private static string? _barberId;
    private static string? _salonId;

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
    public async Task GetAllTimeslotsTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var timeslots = await Request.GetAsync("Timeslot/GetAllTimeslots");

        Assert.That(
            timeslots.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {timeslots.Status}: {timeslots.StatusText} - {timeslots.TextAsync()}"
        );

        var jsonTimeslots = await timeslots.JsonAsync();
        var timeslotsArray = jsonTimeslots.GetValueOrDefault().EnumerateArray().ToList();

        Assert.That(timeslotsArray, Is.Not.Empty, "No timeslots found in the response.");

        foreach (var timeslot in timeslotsArray)
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    timeslot.TryGetProperty("timeslotId", out var tsId)
                        && !string.IsNullOrEmpty(tsId.GetString()),
                    Is.True,
                    "Missing or empty 'timeslotId'"
                );
                Assert.That(
                    timeslot.TryGetProperty("date", out var date)
                        && !string.IsNullOrEmpty(date.GetString()),
                    Is.True,
                    "Missing or empty 'date'"
                );
                Assert.That(
                    timeslot.TryGetProperty("startTime", out var startTime)
                        && !string.IsNullOrEmpty(startTime.GetString()),
                    Is.True,
                    "Missing or empty 'startTime'"
                );
                Assert.That(
                    timeslot.TryGetProperty("duration", out var duration)
                        && duration.GetInt32() > 0,
                    Is.True,
                    "Invalid or missing 'duration'"
                );
                Assert.That(
                    timeslot.TryGetProperty("isBooked", out var isBooked)
                        && (
                            isBooked.ValueKind == JsonValueKind.True
                            || isBooked.ValueKind == JsonValueKind.False
                        ),
                    Is.True,
                    "Missing or invalid boolean 'isBooked'"
                );

                Assert.That(
                    timeslot.TryGetProperty("salon", out var salon),
                    Is.True,
                    "Missing 'salon' property"
                );
                if (salon.ValueKind != JsonValueKind.Null)
                {
                    Assert.That(
                        salon.TryGetProperty("name", out var salonName)
                            && !string.IsNullOrEmpty(salonName.GetString()),
                        Is.True,
                        "Missing or empty 'name' in salon"
                    );
                }
            });
        }
    }

    [Test, Order(2)]
    public async Task GetAllFreeTimeslotsTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var timeslots = await Request.GetAsync("Timeslot/GetAllFreeTimeslots");

        Assert.That(
            timeslots.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {timeslots.Status}: {timeslots.StatusText} - {timeslots.TextAsync()}"
        );

        var jsonTimeslots = await timeslots.JsonAsync();

        var timeslotsArray = jsonTimeslots.GetValueOrDefault().EnumerateArray().ToList();

        Assert.That(timeslotsArray, Is.Not.Empty, "No free timeslots found in the response.");

        foreach (var timeslot in timeslotsArray)
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    timeslot.TryGetProperty("timeslotId", out var tsId)
                        && !string.IsNullOrEmpty(tsId.GetString()),
                    Is.True,
                    "Missing or empty 'timeslotId'"
                );
                Assert.That(
                    timeslot.TryGetProperty("date", out var date)
                        && !string.IsNullOrEmpty(date.GetString()),
                    Is.True,
                    "Missing or empty 'date'"
                );
                Assert.That(
                    timeslot.TryGetProperty("startTime", out var startTime)
                        && !string.IsNullOrEmpty(startTime.GetString()),
                    Is.True,
                    "Missing or empty 'startTime'"
                );
                Assert.That(
                    timeslot.TryGetProperty("duration", out var duration)
                        && duration.GetInt32() > 0,
                    Is.True,
                    "Invalid or missing 'duration'"
                );

                Assert.That(
                    timeslot.TryGetProperty("isBooked", out var isBooked) && !isBooked.GetBoolean(),
                    Is.True,
                    "Expected 'isBooked' to be false for free timeslot."
                );

                Assert.That(
                    timeslot.TryGetProperty("salon", out var salon),
                    Is.True,
                    "Missing 'salon' property"
                );
                if (salon.ValueKind != JsonValueKind.Null)
                {
                    Assert.That(
                        salon.TryGetProperty("name", out var salonName)
                            && !string.IsNullOrEmpty(salonName.GetString()),
                        Is.True,
                        "Missing or empty 'name' in salon"
                    );
                }
            });
        }
    }

    [TestCase("2026-09-05", "10:00:00", 30), Order(3)]
    public async Task CreateTimeslotTest(string date, string startTime, int duration)
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

        var validBarber = barbersArray?.FirstOrDefault(b =>
            b.TryGetProperty("salonId", out var salonIdProp)
            && salonIdProp.ValueKind != JsonValueKind.Null
            && !string.IsNullOrEmpty(salonIdProp.GetString())
        );

        if (validBarber == null || validBarber.Value.ValueKind == JsonValueKind.Undefined)
        {
            Assert.Fail("No barber assigned to a salon was found in the database.");
            return;
        }

        _barberId = validBarber.Value.GetProperty("id").GetString();
        _salonId = validBarber.Value.GetProperty("salonId").GetString();

        var response = await Request.PostAsync(
            "Timeslot/CreateTimeslot",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    date,
                    startTime,
                    duration,
                    salonId = _salonId,
                    barberId = _barberId,
                    isBooked = false,
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
            && json.Value.TryGetProperty("timeslotId", out var timeslotId)
            && json.Value.TryGetProperty("duration", out var durationProp)
            && json.Value.TryGetProperty("isBooked", out var isBookedProp)
            && json.Value.TryGetProperty("salon", out var salon)
            && json.Value.TryGetProperty("barber", out var barber)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(timeslotId.GetString(), Is.Not.Null.And.Not.Empty);
                Assert.That(durationProp.GetInt32(), Is.EqualTo(duration));
                Assert.That(isBookedProp.GetBoolean(), Is.False);

                Assert.That(salon.TryGetProperty("salonId", out var salonIdProp), Is.True);
                Assert.That(salonIdProp.GetString()?.ToLower(), Is.EqualTo(_salonId!.ToLower()));

                Assert.That(barber.TryGetProperty("id", out var barberIdProp), Is.True);
                Assert.That(barberIdProp.GetString()?.ToLower(), Is.EqualTo(_barberId!.ToLower()));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }

        if (json?.TryGetProperty("timeslotId", out var idProp) ?? false)
        {
            _createdTimeslotId = idProp.GetString();
            Assert.That(_createdTimeslotId, Is.Not.Null.And.Not.Empty);
        }
        else
        {
            Assert.Fail("Response object does not contain 'timeslotId' property.");
        }
    }

    [TestCase("2026-09-05", "10:00:00", 30), Order(4)]
    public async Task CreateOverlappingTimeslotTest(string date, string startTime, int duration)
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(_barberId, Is.Not.Null, "Barber ID from previous test is missing.");
        Assert.That(_salonId, Is.Not.Null, "Salon ID from previous test is missing.");

        var response = await Request.PostAsync(
            "Timeslot/CreateTimeslot",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    date,
                    startTime,
                    duration,
                    salonId = _salonId,
                    barberId = _barberId,
                    isBooked = false,
                },
            }
        );

        Assert.That(response.Status, Is.EqualTo(400));

        var responseText = await response.TextAsync();
        Assert.That(responseText, Does.Contain("The timeslot overlaps with an existing one."));
    }

    [Test, Order(5)]
    public async Task UpdateTimeslotTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdTimeslotId,
            Is.Not.Null,
            "Previous test failed to set _createdTimeslotId."
        );
        Assert.That(_barberId, Is.Not.Null, "Barber ID is missing.");

        var response = await Request.PutAsync(
            $"Timeslot/UpdateTimeslot/{_createdTimeslotId}",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    date = "2026-09-05",
                    startTime = "11:00:00",
                    duration = 45,
                    barberId = _barberId,
                    isBooked = false,
                },
            }
        );

        Assert.That(response.Status, Is.EqualTo(204));
    }

    [TestCase("2026-09-05"), Order(6)]
    public async Task GetBarberDailyScheduleTest(string date)
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdTimeslotId,
            Is.Not.Null,
            "Previous test failed to set _createdTimeslotId."
        );
        Assert.That(_barberId, Is.Not.Null, "Barber ID is missing.");

        var response = await Request.GetAsync(
            $"Timeslot/GetBarberDailySchedule?barberId={_barberId}&date={date}"
        );

        Assert.That(response.Status, Is.EqualTo(200));

        var jsonResponse = await response.JsonAsync();
        Assert.That(jsonResponse?.GetRawText(), Does.Contain(_createdTimeslotId));
    }

    [Test, Order(7)]
    public async Task CancelBookingTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdTimeslotId,
            Is.Not.Null,
            "Previous test failed to set _createdTimeslotId."
        );

        var response = await Request.PutAsync($"Timeslot/CancelBooking/{_createdTimeslotId}");

        Assert.That(response.Status, Is.EqualTo(400));

        var responseText = await response.TextAsync();
        Assert.That(responseText, Does.Contain("Timeslot is already available."));
    }

    [Test, Order(8)]
    public async Task DeleteTimeslotTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdTimeslotId,
            Is.Not.Null,
            "Previous test failed to set _createdTimeslotId."
        );

        var response = await Request.DeleteAsync($"Timeslot/DeleteTimeslot/{_createdTimeslotId}");

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
