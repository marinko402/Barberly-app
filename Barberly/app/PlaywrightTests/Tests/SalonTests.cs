using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[TestFixture]
public class SalonTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";
    private const string TargetSalonId = "salon-123";

    private readonly object mockSalon = new
    {
        salonId = TargetSalonId,
        name = "Imperial Cut",
        address = "Nemanjica 14",
        city = "Nis",
        barbers = new[]
        {
            new
            {
                id = "barber-1",
                firstName = "Marko",
                lastName = "Markovic",
                userName = "marko123",
            },
            new
            {
                id = "barber-2",
                firstName = "Nikola",
                lastName = "Nikolic",
                userName = "nikola99",
            },
        },
    };

    private readonly object[] mockSalons =
    [
        new
        {
            salonId = TargetSalonId,
            name = "Imperial Cut",
            address = "Nemanjica 14",
            city = "Nis",
            barbers = new[]
            {
                new
                {
                    id = "barber-1",
                    firstName = "Marko",
                    lastName = "Markovic",
                    userName = "marko123",
                },
                new
                {
                    id = "barber-2",
                    firstName = "Nikola",
                    lastName = "Nikolic",
                    userName = "nikola99",
                },
            },
        },
    ];

    private readonly object[] mockSlots =
    [
        new
        {
            timeslotId = "slot-1",
            startTime = "10:00:00",
            duration = 30,
            isBooked = false,
        },
        new
        {
            timeslotId = "slot-2",
            startTime = "11:00:00",
            duration = 45,
            isBooked = false,
        },
        new
        {
            timeslotId = "slot-3",
            startTime = "12:00:00",
            duration = 30,
            isBooked = true,
        },
    ];

    [SetUp]
    public async Task SetUp()
    {
        await Page.UnrouteAllAsync();

        await Page.RouteAsync(
            "**/Salon/GetAllSalons**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(mockSalons),
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Salon/GetSalonById/**",
            async route =>
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(mockSalon),
                    }
                )
        );

        await Page.RouteAsync(
            "**/Timeslot/GetBarberDailySchedule**",
            async route =>
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(mockSlots),
                    }
                )
        );
    }

    private async Task NavigateToSalonPageAsync()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");
        var card = Page.Locator("div").Filter(new() { HasText = "Imperial Cut" }).First;
        await card.GetByRole(AriaRole.Button, new() { Name = "View Salon" }).ClickAsync();
    }

    [Test]
    public async Task Salon_ShouldDisplayLoadingState_WhenSalonDataIsFetching()
    {
        await Page.RouteAsync(
            "**/Salon/GetSalonById/**",
            async route =>
            {
                await Task.Delay(1000);
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(mockSalon),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/barbers");
        var card = Page.Locator("div").Filter(new() { HasText = "Imperial Cut" }).First;
        await card.GetByRole(AriaRole.Button, new() { Name = "View Salon" }).ClickAsync();

        await Expect(Page.GetByText("Loading salon experience...")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldRenderSalonInfoAndBarbersList()
    {
        await NavigateToSalonPageAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Imperial Cut" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Nemanjica 14")).ToBeVisibleAsync();

        await Expect(Page.GetByText("Marko Markovic")).ToBeVisibleAsync();
        await Expect(Page.GetByText("@marko123")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Nikola Nikolic")).ToBeVisibleAsync();
        await Expect(Page.GetByText("@nikola99")).ToBeVisibleAsync();

        await Expect(Page.GetByText("Please select a barber from the left side"))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldDisplayBarberSlots_WhenBarberIsSelected()
    {
        await NavigateToSalonPageAsync();

        await Page.GetByText("Marko Markovic").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Marko's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("11:00")).ToBeVisibleAsync();

        await Expect(Page.GetByText("12:00")).ToBeHiddenAsync();
    }

    [Test]
    public async Task Salon_ShouldDisplayEmptyState_WhenNoSlotsAvailable()
    {
        await Page.RouteAsync(
            "**/Timeslot/GetBarberDailySchedule**",
            async route =>
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = "[]",
                    }
                )
        );

        await NavigateToSalonPageAsync();
        await Page.GetByText("Marko Markovic").ClickAsync();

        await Expect(Page.GetByText("All slots are taken or none are created for this day."))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldAllowSelectingSlot_AndFillingBookingForm()
    {
        await NavigateToSalonPageAsync();
        await Page.GetByText("Marko Markovic").ClickAsync();

        await Page.GetByText("10:00").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Complete Booking" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00 (30 mins)")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "First Name" }).FillAsync("Petar");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Last Name" }).FillAsync("Petrovic");

        await Page.GetByPlaceholder("email@example.com").FillAsync("petar@gmail.com");
        await Page.GetByPlaceholder("+381 6X XXX XXXX").FillAsync("+381641234567");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Back to Slots" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Marko's Schedule" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldSubmitBookingSuccessfully()
    {
        bool bookingApiCalled = false;

        await Page.RouteAsync(
            "**/Booking/CreateBooking**",
            async route =>
            {
                bookingApiCalled = true;
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(new { message = "Booking created" }),
                    }
                );
            }
        );

        await NavigateToSalonPageAsync();
        await Page.GetByText("Marko Markovic").ClickAsync();
        await Page.GetByText("10:00").ClickAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "First Name" }).FillAsync("Petar");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Last Name" }).FillAsync("Petrovic");
        await Page.GetByPlaceholder("email@example.com").FillAsync("petar@gmail.com");
        await Page.GetByPlaceholder("+381 6X XXX XXXX").FillAsync("+381641234567");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Appointment" }).ClickAsync();

        await Expect(Page.GetByText("Appointment successfully booked!")).ToBeVisibleAsync();
        Assert.That(bookingApiCalled, Is.True, "API za zakazivanje nije pozvan.");
    }

    [Test]
    public async Task Salon_ShouldDisplayErrorToast_WhenBookingFails()
    {
        await Page.RouteAsync(
            "**/Booking/CreateBooking**",
            async route =>
                await route.FulfillAsync(
                    new()
                    {
                        Status = 400,
                        ContentType = "application/json",
                        Body = "\"Timeslot is already booked.\"",
                    }
                )
        );

        await NavigateToSalonPageAsync();
        await Page.GetByText("Marko Markovic").ClickAsync();
        await Page.GetByText("10:00").ClickAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "First Name" }).FillAsync("Petar");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Last Name" }).FillAsync("Petrovic");
        await Page.GetByPlaceholder("email@example.com").FillAsync("petar@gmail.com");
        await Page.GetByPlaceholder("+381 6X XXX XXXX").FillAsync("+381641234567");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Appointment" }).ClickAsync();

        await Expect(Page.GetByText("Timeslot is already booked.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldRenderCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(390, 844);

        await NavigateToSalonPageAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Imperial Cut" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Marko Markovic")).ToBeVisibleAsync();

        await Page.GetByText("Marko Markovic").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Marko's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();

        await Page.GetByText("10:00").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Appointment" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldFetchNewSlots_WhenDateIsChanged()
    {
        await NavigateToSalonPageAsync();

        await Page.GetByText("Marko Markovic").ClickAsync();

        var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Timeslot/GetBarberDailySchedule")
            && request.Url.Contains($"date={tomorrow}")
        );

        await Page.Locator("input[type='date']").FillAsync(tomorrow);

        var request = await requestTask;

        Assert.That(request.Url, Does.Contain($"date={tomorrow}"));
    }

    [Test]
    public async Task Salon_ShouldResetSelectedSlot_WhenDifferentBarberIsSelected()
    {
        await NavigateToSalonPageAsync();

        await Page.GetByText("Marko Markovic").ClickAsync();
        await Page.GetByText("10:00").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Complete Booking" }))
            .ToBeVisibleAsync();

        await Page.GetByText("Nikola Nikolic").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Nikola's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldPreventSubmit_WhenRequiredFieldsAreEmpty()
    {
        await NavigateToSalonPageAsync();
        await Page.GetByText("Marko Markovic").ClickAsync();
        await Page.GetByText("10:00").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Appointment" }).ClickAsync();

        var firstNameInput = Page.GetByRole(AriaRole.Textbox, new() { Name = "First Name" });
        var isInvalid = await firstNameInput.EvaluateAsync<bool>("el => !el.checkValidity()");

        Assert.That(isInvalid, Is.True, "Forma je poslata iako su obavezna polja prazna.");
    }

    [Test]
    public async Task Salon_ShouldDisplayEmptyState_WhenSalonHasNoBarbers()
    {
        var emptySalonMock = new
        {
            salonId = TargetSalonId,
            name = "Imperial Cut",
            address = "Nemanjica 14",
            city = "Nis",
            barbers = Array.Empty<object>(),
        };

        await Page.RouteAsync(
            "**/Salon/GetSalonById/**",
            async route =>
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(emptySalonMock),
                    }
                )
        );

        await NavigateToSalonPageAsync();

        await Expect(Page.GetByText("No barbers found for this salon.")).ToBeVisibleAsync();
    }
}
