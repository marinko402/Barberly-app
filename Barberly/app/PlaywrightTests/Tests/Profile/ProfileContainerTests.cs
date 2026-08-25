using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileContainerTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetupBarberAsync()
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

        await MockAuthMeResponse(
            new
            {
                id = "123",
                userName = "dusan",
                email = "dusan@gmail.com",
                firstName = "Dusan",
                lastName = "Maksimovic",
                phoneNumber = "+381601234567",
                birthDate = "2000-01-01",
                salonId = "salon-1",
                role = "Barber",
            }
        );
    }

    private async Task MockAuthMeResponse(object userData)
    {
        await Page.RouteAsync(
            "**/api/Auth/Me",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(userData),
                    }
                );
            }
        );
    }

    [Test]
    public async Task ProfileContainer_ShouldRenderDesktopSidebarAndContent()
    {
        await Page.GotoAsync($"{ApiUrl}/profile");

        var desktopSidebar = Page.Locator("aside");
        await Expect(desktopSidebar).ToBeVisibleAsync();
        await Expect(desktopSidebar.GetByText("Profile info")).ToBeVisibleAsync();
        await Expect(desktopSidebar.GetByText("@dusan")).ToBeVisibleAsync();

        await Expect(Page.GetByText("Welcome back, Dusan")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileContainer_ShouldRenderMobileLayoutInDomOnly()
    {
        await Page.GotoAsync($"{ApiUrl}/profile");

        var mobileNav = Page.Locator("nav.overflow-x-auto");
        await Expect(mobileNav).ToBeAttachedAsync();
        await Expect(mobileNav).ToBeHiddenAsync();
    }
}
