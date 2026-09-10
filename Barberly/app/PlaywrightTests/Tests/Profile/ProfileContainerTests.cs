using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileContainerTests : BaseProfileTest
{
    [Test]
    public async Task ProfileContainer_ShouldRenderDesktopSidebarAndContent()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile");

        var desktopSidebar = Page.Locator("aside");
        await Expect(desktopSidebar).ToBeVisibleAsync();
        await Expect(desktopSidebar.GetByText("Profile info")).ToBeVisibleAsync();
        await Expect(desktopSidebar.GetByText("@username")).ToBeVisibleAsync();

        await Expect(Page.GetByText("Welcome back, Name")).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test]
    public async Task ProfileContainer_ShouldRenderMobileLayoutInDomOnly()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile");

        var mobileNav = Page.Locator("nav.overflow-x-auto");
        await Expect(mobileNav).ToBeAttachedAsync();
        await Expect(mobileNav).ToBeHiddenAsync();
    }
}
