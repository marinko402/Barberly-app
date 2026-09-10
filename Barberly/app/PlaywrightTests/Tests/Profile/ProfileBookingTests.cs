using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[TestFixture]
public class ProfileBookingsTests : BaseProfileTest
{
    [SetUp]
    public async Task SetUp()
    {
        await LoginAsync("user_50d04554", "Password123!");
    }

    [Test]
    public async Task Bookings_ShouldDisplayHeaderAndSlotsListCorrectly()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Today's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Total Slots")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2", new() { Exact = true })).ToBeVisibleAsync();

        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Duration: 30 min")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Available for booking")).ToBeVisibleAsync();
        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }).Nth(0))
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Duration: 45 min")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Petar Petrovic")).ToBeVisibleAsync();
        await Expect(Page.GetByText("petar@email.com")).ToBeVisibleAsync();
        await Expect(Page.GetByText("+381123456789")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Booked")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Bookings_ShouldDisplayEmptyState_WhenNoSlotsExist()
    {
        await Page.Clock.SetFixedTimeAsync(new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc));

        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByText("Total Slots")).ToBeVisibleAsync();
        await Expect(Page.GetByText("0", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("No timeslots generated for today.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Bookings_ShouldRenderCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Today's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Total Slots")).ToBeVisibleAsync();

        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Available for booking")).ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Petar Petrovic")).ToBeVisibleAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        var logoutButton = Page.GetByRole(AriaRole.Button)
            .Filter(new() { Has = Page.Locator("svg.lucide-log-out") });

        await logoutButton.ClickAsync();

        await Expect(Page).Not.ToHaveURLAsync($"{ApiUrl}/profile");
    }
}
