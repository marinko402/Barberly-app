using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileSidebarTests : PageTest
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
            [
                new Cookie
                {
                    Name = "jwt",
                    Value = "mock-jwt-token-value",
                    Url = ApiUrl,
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteAttribute.Strict,
                },
            ]
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
    public async Task Sidebar_ShouldDisplayBarberTabs_OnDesktop()
    {
        await Page.SetViewportSizeAsync(1440, 900);
        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");

        await Expect(sidebar.GetByText("Profile info", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Change password", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(sidebar.GetByText("My salon", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Timeslots", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Bookings", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task Sidebar_ShouldShowLockBadge_WhenBarberHasNoSalon()
    {
        await Page.SetViewportSizeAsync(1440, 900);
        await SetupAuthenticatedUserAsync(role: "Barber", salonId: null);

        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");
        await Expect(sidebar.GetByText("Lock").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Sidebar_ShouldNavigateToTab_OnClick()
    {
        await Page.SetViewportSizeAsync(1440, 900);
        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");
        await sidebar.GetByText("Change password", new() { Exact = true }).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/profile#security");
    }

    [Test]
    public async Task Sidebar_ShouldLogout_OnDesktopClick()
    {
        await Page.SetViewportSizeAsync(1440, 900);
        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");
        await sidebar.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        await Expect(Page).Not.ToHaveURLAsync($"{ApiUrl}/profile");
    }

    [Test]
    public async Task Sidebar_ShouldRenderMobileLayout_OnMobile()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await Page.GotoAsync($"{ApiUrl}/profile");

        var mobileHeader = Page.Locator(".md\\:hidden");

        await Expect(mobileHeader.GetByText("@dusan", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(mobileHeader.GetByText("Profile info", new() { Exact = true }))
            .ToBeVisibleAsync();
    }
}
