using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.Tests;

[TestFixture]
public class NotFoundTests : PageTest
{
    private const string APIUrl = "https://localhost:5174";

    [SetUp]
    public async Task Setup()
    {
        await Page.RouteAsync(
            "**/api/Auth/Me",
            async route =>
            {
                await route.FulfillAsync(new RouteFulfillOptions { Status = 401 });
            }
        );

        await Page.GotoAsync($"{APIUrl}/non-existent-page-12345");
    }

    [Test]
    public async Task NotFound_PageRender_ShouldDisplayAllTextAndVisualElements()
    {
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "404" })).ToBeVisibleAsync();

        var alertHeading = Page.GetByRole(AriaRole.Alert);
        await Expect(alertHeading).ToBeVisibleAsync();
        await Expect(alertHeading).ToHaveTextAsync("Page Not Found");

        var description = Page.GetByText("The chair is empty, and the mirror is blank.");
        await Expect(description).ToBeVisibleAsync();

        var homeLink = Page.GetByRole(AriaRole.Link, new() { Name = "Go Home" });
        await Expect(homeLink).ToBeVisibleAsync();
        await Expect(homeLink).ToHaveAttributeAsync("href", "/");
    }

    [Test]
    public async Task NotFound_Icons_ShouldBePresentInDOM()
    {
        var goHomeLink = Page.GetByRole(AriaRole.Link, new() { Name = "Go Home" });
        await Expect(goHomeLink).ToBeVisibleAsync();

        var homeIcon = goHomeLink.Locator("svg");
        await Expect(homeIcon).ToBeAttachedAsync();
    }

    [Test]
    public async Task NotFound_ClickGoHome_ShouldNavigateToHomePage()
    {
        var homeLink = Page.GetByRole(AriaRole.Link, new() { Name = "Go Home" });
        await homeLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{APIUrl}/");
    }

    [Test]
    public async Task NotFound_MobileLayout_ShouldFitScreenWithoutHorizontalOverflow()
    {
        await Page.SetViewportSizeAsync(375, 667);

        await Expect(Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Go Home" })).ToBeVisibleAsync();

        var bodyScrollWidth = await Page.EvaluateAsync<int>("() => document.body.scrollWidth");
        var bodyClientWidth = await Page.EvaluateAsync<int>("() => document.body.clientWidth");

        Assert.That(
            bodyScrollWidth,
            Is.EqualTo(bodyClientWidth),
            "Detected horizontal scroll on mobile screen!"
        );
    }

    [Test]
    public async Task NotFound_TabletLayout_ShouldBeCenteredAndVisible()
    {
        await Page.SetViewportSizeAsync(768, 1024);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "404" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Go Home" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task NotFound_DesktopLayout_ShouldCenterCardOnScreen()
    {
        await Page.SetViewportSizeAsync(1920, 1080);

        var card = Page.GetByRole(AriaRole.Alert);
        await Expect(card).ToBeVisibleAsync();

        var homeLink = Page.GetByRole(AriaRole.Link, new() { Name = "Go Home" });
        await Expect(homeLink).ToBeVisibleAsync();
    }
}
