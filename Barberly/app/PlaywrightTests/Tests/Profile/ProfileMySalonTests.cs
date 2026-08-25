using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[TestFixture]
public class ProfileMySalonTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    private readonly object currentUser = new
    {
        id = "owner-123",
        userName = "dusan",
        email = "dusan@gmail.com",
        firstName = "Dusan",
        lastName = "Maksimovic",
        phoneNumber = "+381601234567",
        role = "Barber",
    };

    private readonly object sampleSalon = new
    {
        salonId = "salon-1",
        name = "Gentleman Barber",
        address = "Knez Mihailova 10",
        city = "Belgrade",
        owner = new
        {
            id = "owner-123",
            firstName = "Dusan",
            lastName = "Maksimovic",
            userName = "dusan",
        },
        barbers = new[]
        {
            new
            {
                id = "owner-123",
                firstName = "Dusan",
                lastName = "Maksimovic",
                userName = "dusan",
            },
            new
            {
                id = "barber-456",
                firstName = "Marko",
                lastName = "Markovic",
                userName = "marko.m",
            },
        },
    };

    [SetUp]
    public async Task SetUp()
    {
        await SetupBaseMocks(new[] { sampleSalon });
    }

    private async Task SetupBaseMocks(object salonData)
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
            "**/Auth/Me**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(
                            new
                            {
                                id = "owner-123",
                                userName = "dusan",
                                email = "dusan@gmail.com",
                                firstName = "Dusan",
                                lastName = "Maksimovic",
                                phoneNumber = "+381601234567",
                                birthDate = "2000-01-01",
                                salonId = "salon-1",
                                role = "Barber",
                            }
                        ),
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Barber/GetBarberById/**",
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
            "**/Salon/Get**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(salonData),
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Salon/GetSalonsCount",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(1),
                    }
                );
            }
        );
    }

    [Test]
    public async Task MySalon_ShouldDisplayRegistrationForm_WhenUserHasNoSalon()
    {
        await SetupBaseMocks(Array.Empty<object>());
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Expect(Page.GetByText("Register New Salon")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Launch Salon" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldShowValidationErrors_WhenRegisteringWithEmptyFields()
    {
        await SetupBaseMocks(Array.Empty<object>());
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Launch Salon" }).ClickAsync();

        await Expect(Page.GetByText("Salon name must be at least 2 characters")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Address is required")).ToBeVisibleAsync();
        await Expect(Page.GetByText("City is required")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldRegisterNewSalonSuccessfully()
    {
        await SetupBaseMocks(Array.Empty<object>());

        await Page.RouteAsync(
            "**/Salon/CreateSalon",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(sampleSalon),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByPlaceholder("e.g., The Gentleman's Club").FillAsync("Gentleman Barber");
        await Page.GetByPlaceholder("e.g., Knez Mihailova 21").FillAsync("Knez Mihailova 10");
        await Page.GetByPlaceholder("e.g., Belgrade").FillAsync("Belgrade");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Launch Salon" }).ClickAsync();

        await Expect(Page.GetByText("Salon registered successfully!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldDisplayExistingSalonDetailsAndTeam()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Expect(Page.GetByPlaceholder("e.g., The Gentleman's Club"))
            .ToHaveValueAsync("Gentleman Barber");
        await Expect(Page.GetByPlaceholder("e.g., Knez Mihailova 21"))
            .ToHaveValueAsync("Knez Mihailova 10");
        await Expect(Page.GetByPlaceholder("e.g., Belgrade")).ToHaveValueAsync("Belgrade");

        await Expect(Page.GetByText("Active Team (2)")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Marko Markovic")).ToBeVisibleAsync();
        await Expect(Page.GetByText("@marko.m")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldUpdateSalonDetailsSuccessfully()
    {
        await Page.RouteAsync(
            "**/Salon/UpdateSalon/**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions { Status = 204, ContentType = "application/json" }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Salon" }).ClickAsync();

        await Page.GetByPlaceholder("e.g., The Gentleman's Club").FillAsync("Updated Salon Name");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Salon updated successfully!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldAddBarberToTeamSuccessfully()
    {
        await Page.RouteAsync(
            "**/api/Auth/GetByUsername/**",
            async route =>
            {
                var mockBarber = new
                {
                    id = "barber-789",
                    firstName = "Nikola",
                    lastName = "Nikolic",
                    userName = "nikola.n",
                    email = "nikola@gmail.com",
                    role = "Barber",
                };

                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(mockBarber),
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Salon/AddBarberToSalon**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize("Barber successfully added to the salon."),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByPlaceholder("e.g., john.barber").FillAsync("nikola.n");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Barber" }).ClickAsync();

        await Expect(Page.GetByText("Barber successfully added to salon!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldRemoveBarberFromTeamViaModal()
    {
        await Page.RouteAsync(
            "**/Salon/RemoveBarberFromSalon**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(
                            "Barber successfully removed from the salon."
                        ),
                    }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByTitle("Remove from salon").ClickAsync();

        await Expect(Page.GetByText("Remove Team Member")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Remove", Exact = true }).ClickAsync();

        await Expect(Page.GetByText("Barber successfully removed from salon.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldDeleteSalonSuccessfully()
    {
        await Page.RouteAsync(
            "**/Salon/DeleteSalon/**",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions { Status = 204, ContentType = "application/json" }
                );
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Salon" }).ClickAsync();

        await Expect(Page.GetByText("Salon successfully deleted!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldRenderResponsiveLayout_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);

        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Expect(Page.GetByPlaceholder("e.g., The Gentleman's Club")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Active Team (2)")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Add Team Member" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit Salon" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task MySalon_ShouldDisplayModalCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);

        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByTitle("Remove from salon").First.ClickAsync();

        var modalHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Remove Team Member" });
        await Expect(modalHeading).ToBeVisibleAsync();

        var removeBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Remove", Exact = true });
        await Expect(removeBtn).ToBeVisibleAsync();

        await removeBtn.ClickAsync();
    }
}
