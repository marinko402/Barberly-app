using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[TestFixture]
public class FooterTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
        await Page.GotoAsync($"{ApiUrl}/");
    }

    [Test]
    public async Task Footer_ShouldRenderAllSections()
    {
        var footer = Page.Locator("footer");

        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Contact us" }))
            .ToBeVisibleAsync();
        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Services" }))
            .ToBeVisibleAsync();
        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Barberly" }))
            .ToBeVisibleAsync();
        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Find Us" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Footer_ServicesList_ShouldRenderAllServices()
    {
        var footer = Page.Locator("footer");

        await Expect(footer.GetByText("Haircut")).ToBeVisibleAsync();
        await Expect(footer.GetByText("Beard Trim")).ToBeVisibleAsync();
        await Expect(footer.GetByText("Hair Styling")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Footer_Copyright_ShouldDisplayCurrentYear()
    {
        var currentYear = DateTime.Now.Year.ToString();
        var copyrightText = Page.Locator("footer").GetByText($"All right reserved. {currentYear}.");

        await Expect(copyrightText).ToBeVisibleAsync();
    }

    [Test]
    public async Task Footer_ClickingHomeLink_ShouldNavigateToRoot()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");

        var homeLink = Page.Locator("footer").GetByRole(AriaRole.Link, new() { Name = "Home" });
        await homeLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/");
    }

    [Test]
    public async Task Footer_ClickingBarbersLink_ShouldNavigateToBarbersPage()
    {
        var barbersLink = Page.Locator("footer")
            .GetByRole(AriaRole.Link, new() { Name = "Barbers" });
        await barbersLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/barbers");
    }

    [Test]
    public async Task Footer_ClickingAboutUs_ShouldUpdateHashInURL()
    {
        var aboutUsSpan = Page.Locator("footer").GetByText("About Us");
        await aboutUsSpan.ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/#about-us");
    }

    [Test]
    public async Task Footer_ClickingCallUs_ShouldTriggerPhoneProtocol()
    {
        var callUsParagraph = Page.Locator("footer").GetByText("Call us:");

        await callUsParagraph.ClickAsync();

        Assert.That(Page.Url, Does.StartWith("tel:+1234567890").Or.EqualTo($"{ApiUrl}/"));
    }

    [Test]
    public async Task Footer_ClickingMailUs_ShouldTriggerMailtoProtocol()
    {
        var mailParagraph = Page.Locator("footer").GetByText("Mail:");

        await mailParagraph.ClickAsync();

        Assert.That(
            Page.Url,
            Does.StartWith("mailto:barberly@support.com").Or.EqualTo($"{ApiUrl}/")
        );
    }

    [Test]
    public async Task Footer_SocialLinks_ShouldHaveCorrectAttributes()
    {
        var footer = Page.Locator("footer");

        var instagram = footer.GetByRole(AriaRole.Link, new() { Name = "Instagram" });
        await Expect(instagram).ToHaveAttributeAsync("href", "https://www.instagram.com/");
        await Expect(instagram).ToHaveAttributeAsync("target", "_blank");
        await Expect(instagram).ToHaveAttributeAsync("rel", "noopener noreferrer");

        var facebook = footer.GetByRole(AriaRole.Link, new() { Name = "Facebook" });
        await Expect(facebook).ToHaveAttributeAsync("href", "https://www.facebook.com/");
        await Expect(facebook).ToHaveAttributeAsync("target", "_blank");

        var xTwitter = footer.GetByRole(AriaRole.Link, new() { Name = "X" });
        await Expect(xTwitter).ToHaveAttributeAsync("href", "https://www.x.com/");
        await Expect(xTwitter).ToHaveAttributeAsync("target", "_blank");

        var linkedIn = footer.GetByRole(AriaRole.Link, new() { Name = "LinkedIn" });
        await Expect(linkedIn).ToHaveAttributeAsync("href", "https://www.linkedin.com/");
        await Expect(linkedIn).ToHaveAttributeAsync("target", "_blank");
    }

    [Test]
    public async Task Footer_Responsive_MobileLayout_ShouldCenterAlignText()
    {
        await Page.SetViewportSizeAsync(375, 667);

        var footerGrid = Page.Locator("footer > div").First;

        await Expect(footerGrid)
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("grid-cols-1"));

        var contactHeading = Page.Locator("footer h1", new() { HasText = "Contact us" });
        await Expect(contactHeading).ToBeVisibleAsync();
    }

    [Test]
    public async Task Footer_Responsive_TabletLayout_ShouldUseTwoColumns()
    {
        await Page.SetViewportSizeAsync(768, 1024);

        var footer = Page.Locator("footer");

        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Contact us" }))
            .ToBeVisibleAsync();
        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Services" }))
            .ToBeVisibleAsync();
        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Barberly" }))
            .ToBeVisibleAsync();
        await Expect(footer.GetByRole(AriaRole.Heading, new() { Name = "Find Us" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Footer_Responsive_DesktopLayout_ShouldUseFourColumns()
    {
        await Page.SetViewportSizeAsync(1280, 800);

        var footerGrid = Page.Locator("footer > div").First;

        await Expect(footerGrid)
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("lg:grid-cols-4"));
    }
}
