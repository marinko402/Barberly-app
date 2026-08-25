using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[TestFixture]
public class ProfileTimeslotTests : PageTest
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
    };

    private readonly object sampleBookedSlot = new
    {
        timeslotId = "slot-2",
        barberId = BarberId,
        salonId = "salon-1",
        date = DateTime.Now.ToString("yyyy-MM-dd"),
        startTime = "10:00:00",
        duration = 45,
        isBooked = true,
        customerName = "Petar Petrovic",
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
    public async Task Timeslot_ShouldDisplayFormAndScheduleListCorrectly()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("Active Schedule")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2 Slots")).ToBeVisibleAsync();

        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("30 mins")).ToBeVisibleAsync();
        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }))
            .ToBeAttachedAsync();

        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("45 mins")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Booked")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Petar Petrovic")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldDisplayEmptyState_WhenNoSlotsExist()
    {
        await SetupBaseMocks(Array.Empty<object>());
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("0 Slots")).ToBeVisibleAsync();
        await Expect(
                Page.GetByText(
                    "No timeslots generated for this date yet. Use the quick builder above!"
                )
            )
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldCreateNewTimeslotSuccessfully()
    {
        await Page.RouteAsync(
            "**/Timeslot/CreateTimeslot",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(sampleFreeSlot),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }).ClickAsync();

        await Expect(Page.GetByText("Timeslot successfully created!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldDisplayCustomError_WhenCreateTimeslotFails()
    {
        await Page.RouteAsync(
            "**/Timeslot/CreateTimeslot",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 400,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(
                            new
                            {
                                errors = new Dictionary<string, string[]>
                                {
                                    {
                                        "Time",
                                        new[] { "Timeslot overlaps with an existing slot!" }
                                    },
                                },
                            }
                        ),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }).ClickAsync();

        await Expect(Page.GetByText("Timeslot overlaps with an existing slot!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldEditAndSaveSlotSuccessfully()
    {
        await Page.RouteAsync(
            "**/Timeslot/UpdateTimeslot/**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(sampleFreeSlot),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Edit timeslot").ClickAsync();

        await Expect(Page.GetByText("Modify Selected Slot")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Cancel Edit")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Timeslot successfully updated!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldCancelEditMode()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Edit timeslot").ClickAsync();
        await Expect(Page.GetByText("Modify Selected Slot")).ToBeVisibleAsync();

        await Page.GetByText("Cancel Edit").ClickAsync();
        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldDeleteTimeslotSuccessfully()
    {
        await Page.RouteAsync(
            "**/Timeslot/DeleteTimeslot/**",
            async route =>
            {
                await route.FulfillAsync(new RouteFulfillOptions { Status = 200 });
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Delete timeslot").ClickAsync();

        await Expect(Page.GetByText("Timeslot deleted.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldCancelBookingSuccessfully()
    {
        await Page.RouteAsync(
            "**/Timeslot/CancelBooking/**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(sampleFreeSlot),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Cancel booking & free up slot").ClickAsync();

        await Expect(Page.GetByText("Booking cancelled. Timeslot is now available!"))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldRenderCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Combobox)).ToBeVisibleAsync();

        await Expect(Page.GetByText("Active Schedule")).ToBeVisibleAsync();
        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();

        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }))
            .ToBeHiddenAsync();
    }

    [Test]
    public async Task Timeslot_ShouldRenderCorrectly_OnTabletViewport()
    {
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }))
            .ToBeAttachedAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Timeslot_ShouldRenderCorrectly_OnDesktopViewport()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Active Schedule")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2 Slots")).ToBeVisibleAsync();
        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }))
            .ToBeAttachedAsync();
    }
}
