using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[TestFixture]
public class NavbarTests : PageTest
{
    private const string apiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
        await MockUnauthenticatedUser();
        await Page.GotoAsync($"{apiUrl}/");
    }

    private async Task MockUnauthenticatedUser()
    {
        await Page.RouteAsync(
            "**/Me",
            async route =>
                await route.FulfillAsync(
                    new()
                    {
                        Status = 401,
                        ContentType = "application/json",
                        Body = "{\"message\": \"Unauthenticated\"}",
                    }
                )
        );
    }

    private async Task MockAuthenticatedUser()
    {
        await Page.RouteAsync(
            "**/Me",
            async route =>
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = "{\"id\": 1, \"name\": \"Dusan\", \"role\": \"Client\"}",
                    }
                )
        );
    }

    [Test]
    public async Task Navbar_UnauthenticatedUser_ShouldShowLoginButton()
    {
        var loginLink = Page.Locator("nav").GetByRole(AriaRole.Link, new() { Name = "Login" });
        await Expect(loginLink).ToBeVisibleAsync();

        await loginLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{apiUrl}/login");
    }

    [Test]
    public async Task Navbar_AuthenticatedUser_ShouldNavigateToProfileWhenClickingAvatar()
    {
        await MockAuthenticatedUser();
        await Page.GotoAsync($"{apiUrl}/");

        var profileLink = Page.Locator("nav a[href='/profile']");
        await Expect(profileLink).ToBeVisibleAsync();

        await profileLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{apiUrl}/profile");
    }

    [Test]
    public async Task Navbar_Desktop_ShouldRenderNavLinksAndNavigate()
    {
        var nav = Page.Locator("nav");

        await Expect(nav.GetByText("Home")).ToBeVisibleAsync();
        await Expect(nav.GetByText("About Us")).ToBeVisibleAsync();
        await Expect(nav.GetByText("Barbers")).ToBeVisibleAsync();

        await nav.GetByRole(AriaRole.Link, new() { Name = "Barbers" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{apiUrl}/barbers");
    }

    [Test]
    public async Task Navbar_ClickingAboutUs_ShouldUpdateHashInURL()
    {
        var aboutUsSpan = Page.Locator("nav").GetByText("About Us").First;
        await aboutUsSpan.ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{apiUrl}/#about-us");
    }

    [Test]
    public async Task Navbar_Responsive_ShouldHideDesktopLinksOnMobile()
    {
        await Page.SetViewportSizeAsync(375, 667);

        var desktopLinksContainer = Page.Locator("nav .max-lg\\:hidden").First;
        await Expect(desktopLinksContainer).ToBeHiddenAsync();

        var hamburgerIcon = Page.Locator("nav .lg\\:hidden");
        await Expect(hamburgerIcon).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navbar_MobileMenu_ShouldOpenAndCloseViaCloseButton()
    {
        await Page.SetViewportSizeAsync(375, 667);

        var hamburgerIcon = Page.Locator("nav .lg\\:hidden");
        await hamburgerIcon.ClickAsync();

        var drawer = Page.Locator("div.fixed.right-0.top-0");
        await Expect(drawer).ToBeVisibleAsync();
        await Expect(drawer.GetByText("About Us")).ToBeVisibleAsync();

        var closeIcon = drawer.Locator("svg").First;
        await closeIcon.ClickAsync();

        await Expect(drawer).ToBeHiddenAsync();
    }

    [Test]
    public async Task Navbar_MobileMenu_ClickingLinkShouldNavigateAndCloseMenu()
    {
        await Page.SetViewportSizeAsync(375, 667);

        var hamburgerIcon = Page.Locator("nav .lg\\:hidden");
        await hamburgerIcon.ClickAsync();

        var drawer = Page.Locator("div.fixed.right-0.top-0");
        await drawer.GetByRole(AriaRole.Link, new() { Name = "Barbers" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{apiUrl}/barbers");
        await Expect(drawer).ToBeHiddenAsync();
    }

    [Test]
    public async Task Navbar_Logo_ShouldNavigateToHome()
    {
        await Page.GotoAsync($"{apiUrl}/barbers");

        var logoLink = Page.Locator("nav a[href='/']").First;
        await logoLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{apiUrl}/");
    }

    [Test]
    public async Task Navbar_MobileMenu_ClickingOverlayShouldCloseMenu()
    {
        await Page.SetViewportSizeAsync(375, 667);

        await Page.Locator("nav .lg\\:hidden").ClickAsync();

        var drawer = Page.Locator("div.fixed.right-0.top-0");
        await Expect(drawer).ToBeVisibleAsync();

        var backdrop = Page.Locator("div.fixed.inset-0.bg-black\\/40");

        await backdrop.ClickAsync(
            new()
            {
                Position = new Position { X = 10, Y = 10 },
            }
        );

        await Expect(drawer).ToBeHiddenAsync();
    }

    [Test]
    public async Task Navbar_ThemeToggle_ShouldToggleDarkMode()
    {
        var themeCheckbox = Page.Locator("nav input[type='checkbox']").First;

        bool initialChecked = await themeCheckbox.IsCheckedAsync();

        await themeCheckbox.ClickAsync(new() { Force = true });

        Assert.That(await themeCheckbox.IsCheckedAsync(), Is.Not.EqualTo(initialChecked));
    }
}
