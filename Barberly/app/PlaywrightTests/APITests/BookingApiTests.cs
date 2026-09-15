using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.ApiTests;

[TestFixture]
public class BookingApiTests : PlaywrightTest
{
    private IAPIRequestContext? Request = null;

    private static string? _createdBookingId;

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
    public async Task GetAllBookingsTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var bookings = await Request.GetAsync("Booking/GetAllBookings");

        Assert.That(
            bookings.Status,
            Is.EqualTo(200),
            $"Expected 200 OK, but got {bookings.Status}: {bookings.StatusText} - {bookings.TextAsync()}"
        );

        var jsonBookings = await bookings.JsonAsync();

        var bookingsArray = jsonBookings.GetValueOrDefault().EnumerateArray().ToList();

        Assert.That(bookingsArray, Is.Not.Empty, "No bookings found in the response.");

        foreach (var booking in bookingsArray)
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    booking.TryGetProperty("bookingId", out var bookingId)
                        && !string.IsNullOrEmpty(bookingId.GetString()),
                    Is.True,
                    "Missing or empty 'bookingId'"
                );
                Assert.That(
                    booking.TryGetProperty("customerFirstName", out var fn)
                        && !string.IsNullOrEmpty(fn.GetString()),
                    Is.True,
                    "Missing or empty 'customerFirstName'"
                );
                Assert.That(
                    booking.TryGetProperty("customerLastName", out var ln)
                        && !string.IsNullOrEmpty(ln.GetString()),
                    Is.True,
                    "Missing or empty 'customerLastName'"
                );
                Assert.That(
                    booking.TryGetProperty("customerEmail", out var email)
                        && email.GetString()!.Contains("@"),
                    Is.True,
                    "Invalid or missing 'customerEmail'"
                );
                Assert.That(
                    booking.TryGetProperty("customerPhoneNumber", out _),
                    Is.True,
                    "Missing 'customerPhoneNumber' property"
                );

                Assert.That(
                    booking.TryGetProperty("timeslot", out var timeslot),
                    Is.True,
                    "Missing 'timeslot' property"
                );

                if (timeslot.ValueKind != System.Text.Json.JsonValueKind.Null)
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
                        "Missing or empty 'date' in timeslot"
                    );
                    Assert.That(
                        timeslot.TryGetProperty("startTime", out var startTime)
                            && !string.IsNullOrEmpty(startTime.GetString()),
                        Is.True,
                        "Missing or empty 'startTime' in timeslot"
                    );
                    Assert.That(
                        timeslot.TryGetProperty("duration", out var duration)
                            && duration.GetInt32() > 0,
                        Is.True,
                        "Invalid or missing 'duration' in timeslot"
                    );
                }
            });
        }
    }

    [TestCase("Dusan", "Maksimovic", "dusan@maksimovic.com", "+381123456789"), Order(2)]
    public async Task CreateBookingTest(
        string customerFirstName,
        string customerLastName,
        string customerEmail,
        string customerPhoneNumber
    )
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var timeslots = await Request.GetAsync("Timeslot/GetAllFreeTimeslots");

        if (timeslots.Status != 200)
        {
            Assert.Fail($"Code: {timeslots.Status} - {timeslots.StatusText}");
            return;
        }

        var jsonTimeslots = await timeslots.JsonAsync();

        if (!jsonTimeslots.GetValueOrDefault().EnumerateArray().Any())
        {
            Assert.Fail("No free timeslots found in the response.");
            return;
        }

        var firstTimeslot = jsonTimeslots.GetValueOrDefault().EnumerateArray().FirstOrDefault();

        var timeslotId = firstTimeslot.GetProperty("timeslotId").GetString();

        var response = await Request.PostAsync(
            "Booking/CreateBooking",
            new APIRequestContextOptions()
            {
                DataObject = new
                {
                    timeslotId,
                    customerFirstName,
                    customerLastName,
                    customerEmail,
                    customerPhoneNumber,
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
            && json.Value.TryGetProperty("bookingId", out var bookingId)
            && json.Value.TryGetProperty("timeslot", out var timeslot)
            && json.Value.TryGetProperty("customerFirstName", out var cFirstName)
            && json.Value.TryGetProperty("customerLastName", out var cLastName)
            && json.Value.TryGetProperty("customerEmail", out var cEmail)
            && json.Value.TryGetProperty("customerPhoneNumber", out var cPhoneNumber)
        )
        {
            Assert.Multiple(() =>
            {
                Assert.That(timeslot.TryGetProperty("timeslotId", out var tsId), Is.True);
                Assert.That(tsId.GetString(), Is.EqualTo(timeslotId));
                Assert.That(bookingId.GetString(), Is.Not.Null.And.Not.Empty);
                _createdBookingId = bookingId.GetString();
                Assert.That(cFirstName.GetString(), Is.EqualTo(customerFirstName));
                Assert.That(cLastName.GetString(), Is.EqualTo(customerLastName));
                Assert.That(cEmail.GetString(), Is.EqualTo(customerEmail));
                Assert.That(cPhoneNumber.GetString(), Is.EqualTo(customerPhoneNumber));
            });
        }
        else
        {
            Assert.Fail("Response object does not contain expected properties.");
        }
    }

    [Test, Order(3)]
    public async Task DeleteBookingTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");
        Assert.That(
            _createdBookingId,
            Is.Not.Null.And.Not.Empty,
            "No booking ID available for deletion."
        );

        var response = await Request.DeleteAsync($"Booking/DeleteBooking/{_createdBookingId}");

        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test, Order(4)]
    public async Task GetTotalBookingsCountTest()
    {
        Assert.That(Request, Is.Not.Null, "API Context is not initialized.");

        var countResponse = await Request!.GetAsync("Booking/GetTotalBookingsCount");
        Assert.That(countResponse.Status, Is.EqualTo(200));

        var countText = await countResponse.TextAsync();
        Assert.That(
            int.TryParse(countText, out var count),
            Is.True,
            "Response is not a valid integer."
        );
        Assert.That(count, Is.GreaterThanOrEqualTo(0), "Booking count cannot be negative.");
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
