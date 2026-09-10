using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileSidebarTests : BaseProfileTest
{
    [Test]
    public async Task Sidebar_ShouldDisplayBarberTabs_OnDesktop()
    {
        await Page.SetViewportSizeAsync(1440, 900);

        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");

        await Expect(sidebar.GetByText("Profile info", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Change password", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(sidebar.GetByText("My salon", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Timeslots", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(sidebar.GetByText("Bookings", new() { Exact = true })).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test]
    public async Task Sidebar_ShouldShowLockBadge_WhenBarberHasNoSalon()
    {
        await Page.SetViewportSizeAsync(1440, 900);

        await LoginAsync("user_50d04554", "Password123!");

        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");
        await Expect(sidebar.GetByText("Lock").First).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test]
    public async Task Sidebar_ShouldNavigateToTab_OnClick()
    {
        await Page.SetViewportSizeAsync(1440, 900);

        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");
        await sidebar.GetByText("Change password", new() { Exact = true }).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/profile#security");

        await LogoutAsync();
    }

    [Test]
    public async Task Sidebar_ShouldLogout_OnDesktopClick()
    {
        await Page.SetViewportSizeAsync(1440, 900);

        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");
        await sidebar.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        await Expect(Page).Not.ToHaveURLAsync($"{ApiUrl}/profile");
    }

    [Test]
    public async Task Sidebar_ShouldRenderMobileLayout_OnMobile()
    {
        await Page.SetViewportSizeAsync(390, 844);

        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile");

        var mobileHeader = Page.Locator(".md\\:hidden");

        await Expect(mobileHeader.GetByText("@username", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(mobileHeader.GetByText("Profile info", new() { Exact = true }))
            .ToBeVisibleAsync();

        var logoutButton = Page.GetByRole(AriaRole.Button)
            .Filter(new() { Has = Page.Locator("svg.lucide-log-out") });

        await logoutButton.ClickAsync();

        await Expect(Page).Not.ToHaveURLAsync($"{ApiUrl}/profile");
    }
}
