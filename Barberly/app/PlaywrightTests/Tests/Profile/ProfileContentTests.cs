using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileContentTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
        await SetupAuthenticatedUserAsync(role: "Barber", salonId: "salon-1");
    }

    private async Task SetupAuthenticatedUserAsync(
        string role = "Barber",
        string? salonId = "salon-1"
    )
    {
        await Context.AddCookiesAsync(
            new[]
            {
                new Cookie
                {
                    Name = "jwt",
                    Value = "mock-jwt-token-value",
                    Url = ApiUrl,
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteAttribute.Strict,
                },
            }
        );

        await Page.RouteAsync(
            "**/api/Auth/Me",
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
                                id = "123",
                                userName = "dusan",
                                email = "dusan@gmail.com",
                                firstName = "Dusan",
                                lastName = "Maksimovic",
                                phoneNumber = "+381601234567",
                                birthDate = "2000-01-01",
                                salonId,
                                role,
                            }
                        ),
                    }
                );
            }
        );
    }

    [Test]
    public async Task ProfileContent_ShouldRenderHeaderWithUserGreetingAndRole()
    {
        await Page.GotoAsync($"{ApiUrl}/profile");

        await Expect(Page.GetByText("Welcome back, Dusan")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Barber", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Here's what's happening with your shop today."))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldShowInvalidSectionError_WhenSectionDoesNotExist()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#invalid-section");

        await Expect(Page.GetByText("Invalid section").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldShowLockedState_WhenBarberHasNoSalon_OnTimeslots()
    {
        await SetupAuthenticatedUserAsync(role: "Barber", salonId: null);

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Feature Temporarily Locked")).ToBeVisibleAsync();
        await Expect(
                Page.GetByText(
                    "You cannot manage your timeslots until you are actively registered under or owning a salon workspace."
                )
            )
            .ToBeVisibleAsync();

        var setupButton = Page.GetByRole(AriaRole.Link, new() { Name = "Set up My Salon" });
        await Expect(setupButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldShowLockedState_WhenBarberHasNoSalon_OnBookings()
    {
        await SetupAuthenticatedUserAsync(role: "Barber", salonId: null);

        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByText("Feature Temporarily Locked")).ToBeVisibleAsync();
        await Expect(
                Page.GetByText(
                    "You cannot manage your bookings until you are actively registered under or owning a salon workspace."
                )
            )
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldNavigateToSalonTab_WhenClickingSetUpSalonButton()
    {
        await SetupAuthenticatedUserAsync(role: "Barber", salonId: null);

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Set up My Salon" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/profile#salon");
    }
}
