using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[TestFixture]
public class ProfileBookingsTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";
    private const string BarberId = "barber-123";

    private readonly object currentUser = new
    {
        id = BarberId,
        userName = "dusan",
        email = "dusan@gmail.com",
        firstName = "Dusan",
        lastName = "Maksimovic",
        phoneNumber = "+381601234567",
        role = "Barber",
        salonId = "salon-1",
    };

    private readonly object sampleFreeSlot = new
    {
        timeslotId = "slot-1",
        barberId = BarberId,
        salonId = "salon-1",
        date = DateTime.Now.ToString("yyyy-MM-dd"),
        startTime = "09:00:00",
        duration = 30,
        isBooked = false,
        customerName = (string?)null,
        customerEmail = (string?)null,
        customerPhoneNumber = (string?)null,
    };

    private readonly object sampleBookedSlot = new
    {
        timeslotId = "slot-2",
        barberId = BarberId,
        salonId = "salon-1",
        date = DateTime.Now.ToString("yyyy-MM-dd"),
        startTime = "10:30:00",
        duration = 45,
        isBooked = true,
        customerName = "Marko Markovic",
        customerEmail = "marko@gmail.com",
        customerPhoneNumber = "+381641112233",
    };

    [SetUp]
    public async Task SetUp()
    {
        await SetupBaseMocks(new[] { sampleFreeSlot, sampleBookedSlot });
    }

    private async Task SetupBaseMocks(object scheduleData)
    {
        await Page.UnrouteAllAsync();

        await Context.AddCookiesAsync([
            new Cookie
            {
                Name = "jwt",
                Value = "mock-jwt-token-value",
                Url = ApiUrl,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteAttribute.Strict,
            },
        ]);

        await Page.RouteAsync(
            "**/api/Auth/Me",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(currentUser),
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Timeslot/GetBarberDailySchedule**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(scheduleData),
                    }
                );
            }
        );
    }

    [Test]
    public async Task Bookings_ShouldDisplayHeaderAndSlotsListCorrectly()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Today's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Total Slots")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2", new() { Exact = true })).ToBeVisibleAsync();

        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Duration: 30 min")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Available for booking")).ToBeVisibleAsync();
        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("10:30")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Duration: 45 min")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Marko Markovic")).ToBeVisibleAsync();
        await Expect(Page.GetByText("marko@gmail.com")).ToBeVisibleAsync();
        await Expect(Page.GetByText("+381641112233")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Booked")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Bookings_ShouldDisplayEmptyState_WhenNoSlotsExist()
    {
        await SetupBaseMocks(Array.Empty<object>());
        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByText("Total Slots")).ToBeVisibleAsync();
        await Expect(Page.GetByText("0", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("No timeslots generated for today.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Bookings_ShouldRenderCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Today's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Total Slots")).ToBeVisibleAsync();

        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Available for booking")).ToBeVisibleAsync();
        await Expect(Page.GetByText("10:30")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Marko Markovic")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Bookings_ShouldRenderCorrectly_OnTabletViewport()
    {
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Today's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("10:30")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Booked")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Bookings_ShouldRenderCorrectly_OnDesktopViewport()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Today's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Total Slots")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("10:30")).ToBeVisibleAsync();
    }
}
